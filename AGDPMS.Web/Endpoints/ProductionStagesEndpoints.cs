using AGDPMS.Shared.Models;
using AGDPMS.Shared.Models.DTOs;
using AGDPMS.Shared.Services;
using AGDPMS.Web.Data;
using AGDPMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace AGDPMS.Web.Endpoints;

public static class ProductionStagesEndpoints
{
    public static IEndpointRouteBuilder MapProductionStages(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stages")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager,QA,Director" });

        group.MapPut("/{stageId:int}/assign-qa", async (int stageId, AssignStageQaDto dto, StageService svc) =>
        {
            await svc.AssignQaAsync(stageId, dto.QaUserId);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        // PM can mark a stage completed
        group.MapPost("/{stageId:int}/pm-complete", async (int stageId, StageService svc) =>
        {
            await svc.CompleteByPmAsync(stageId);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        // Progress and planning
        group.MapPut("/{stageId:int}/plan", async (int stageId, UpdateStagePlanDto dto, StageService svc) =>
        {
            try
            {
                await svc.UpdatePlanAsync(stageId, dto.PlannedStartDate, dto.PlannedFinishDate, dto.PlannedTimeHours);
                return Results.Ok();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        group.MapPut("/{stageId:int}/dates", async (int stageId, UpdateStageDatesDto dto, StageService svc) =>
        {
            await svc.UpdateActualDatesAsync(stageId, dto.ActualStartDate, dto.ActualFinishDate, dto.ActualTimeHours);
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        group.MapPut("/{stageId:int}/units", async (int stageId, UpdateStageUnitsDto dto, ProductionItemStageDataAccess stageAccess) =>
        {
            if (dto.PlannedUnits.HasValue)
            {
                await stageAccess.UpdatePlannedUnitsAsync(stageId, dto.PlannedUnits.Value);
            }
            if (dto.ActualUnits.HasValue)
            {
                await stageAccess.UpdateActualUnitsAsync(stageId, dto.ActualUnits.Value);
            }
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });


        var itemGroup = app.MapGroup("/api/items").RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });
        itemGroup.MapPost("/{itemId:int}/complete", async (int itemId, StageService svc) =>
        {
            await svc.ForceCompleteItemAsync(itemId);
            return Results.Ok();
        });

        // helper to fetch order id from item id (for UI convenience)
        app.MapGet("/api/items/{itemId:int}/order-id", async (int itemId, ProductionItemDataAccess items) =>
        {
            var it = await items.GetByIdAsync(itemId);
            return Results.Ok(it?.ProductionOrderId ?? 0);
        });

        // Update item code (draft phase only)
        itemGroup.MapPut("/{itemId:int}/code", async (int itemId, UpdateItemCodeDto dto, ProductionItemDataAccess itemAccess, ProductionOrderDataAccess orderAccess) =>
        {
            var item = await itemAccess.GetByIdAsync(itemId);
            if (item is null) return Results.NotFound();
            var order = await orderAccess.GetByIdAsync(item.ProductionOrderId);
            if (order is null) return Results.NotFound();
            if (order.Status != ProductionOrderStatus.Draft)
                return Results.BadRequest("Code can only be updated in draft phase");
            
            // Validate code: 4 chars max, alphanumeric
            if (string.IsNullOrWhiteSpace(dto.Code) || dto.Code.Length > 4)
                return Results.BadRequest("Code must be 1-4 characters");
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Code, @"^[a-zA-Z0-9]+$"))
                return Results.BadRequest("Code must contain only alphanumeric characters");
            
            // Check uniqueness within order
            var existingItems = await itemAccess.ListByOrderAsync(item.ProductionOrderId);
            if (existingItems.Any(i => i.Id != itemId && i.Code.Equals(dto.Code, StringComparison.OrdinalIgnoreCase)))
                return Results.BadRequest("Code must be unique within the order");
            
            await itemAccess.UpdateCodeAsync(itemId, dto.Code);
            return Results.Ok();
        });

        itemGroup.MapPut("/{itemId:int}/stored", async (int itemId, ProductionItemDataAccess itemAccess) =>
        {
            var item = await itemAccess.GetByIdAsync(itemId);
            if (item is null) return Results.NotFound();
            if (!item.IsCompleted)
                return Results.BadRequest("Item must be completed before storing");
            
            await itemAccess.SetStoredAsync(itemId, true);
            return Results.Ok();
        });

        // Get item details with stages
        app.MapGet("/api/items/{itemId:int}", async (
            int itemId,
            ProductionItemDataAccess itemAccess,
            ProductionItemStageDataAccess stageAccess,
            StageTypeDataAccess stageTypeAccess,
            UserDataAccess userAccess,
            CavityDataAccess cavityAccess,
            ProductionOrderDataAccess orderAccess) =>
        {
            var item = await itemAccess.GetByIdAsync(itemId);
            if (item is null) return Results.NotFound();
            var order = await orderAccess.GetByIdAsync(item.ProductionOrderId);
            if (order is null) return Results.NotFound();
            var types = (await stageTypeAccess.GetAllAsync()).ToDictionary(t => t.Id);
            // Create order mapping based on code order in data.sql
            var stageTypeOrder = new Dictionary<string, int>
            {
                { "CUT_AL", 1 },
                { "MILL_LOCK", 2 },
                { "DOOR_CORNER_CUT", 3 },
                { "ASSEMBLE_FRAME", 4 },
                { "GLASS_INSTALL", 5 },
                { "PRESS_GASKET", 6 },
                { "INSTALL_ACCESSORIES", 7 },
                { "CUT_FLUSH", 8 },
                { "FINISH_SILICON", 9 }
            };
            var allUsers = (await userAccess.GetAllAsync()).ToDictionary(u => u.Id);
            var cavity = await cavityAccess.GetByIdWithBOMsAsync(item.CavityId);
            var stages = (await stageAccess.ListByItemAsync(itemId)).ToList();
            // Filter: Hide stages with planned_time_hours = 0 or NULL unless order is Draft
            var filteredStages = stages.Where(s =>
                (s.PlannedTimeHours > 0 || s.PlannedTimeHours.HasValue) ||
                order.Status == ProductionOrderStatus.Draft
            ).ToList();

            // Order by stage type code order to match the order in data.sql
            var stageDtos = filteredStages
                .OrderBy(s =>
                {
                    if (types.ContainsKey(s.StageTypeId))
                    {
                        var code = types[s.StageTypeId].Code;
                        return stageTypeOrder.ContainsKey(code) ? stageTypeOrder[code] : 999;
                    }
                    return 999;
                })
                .ThenBy(s => s.StageTypeId)
                .Select(s => new
                {
                    s.Id,
                    s.StageTypeId,
                    StageCode = types.ContainsKey(s.StageTypeId) ? types[s.StageTypeId].Code : string.Empty,
                    StageName = types.ContainsKey(s.StageTypeId) ? types[s.StageTypeId].Name : $"Stage Type {s.StageTypeId}",
                    s.PlannedStartDate,
                    s.PlannedFinishDate,
                    s.ActualStartDate,
                    s.ActualFinishDate,
                    s.PlannedTimeHours,
                    s.ActualTimeHours,
                    s.PlannedUnits,
                    s.ActualUnits,
                    s.AssignedQaUserId,
                    AssignedQaUserName = s.AssignedQaUserId.HasValue && allUsers.ContainsKey(s.AssignedQaUserId.Value)
                        ? allUsers[s.AssignedQaUserId.Value].FullName
                        : null,
                    s.IsCompleted,
                    s.CompletedAt
                }).ToList();
            var totalPlannedHours = stageDtos.Sum(s => (decimal?)(s.PlannedTimeHours ?? 0m)) ?? 0m;
            var totalActualHours = stageDtos.Sum(s => (decimal?)(s.ActualTimeHours ?? 0m)) ?? 0m;
            var now = DateTime.UtcNow;
            var isLate = false;
            var isOverdue = false;
            var daysLate = 0;

            if (item.PlannedFinishDate.HasValue)
            {
                var planFinish = item.PlannedFinishDate.Value;
                if (item.IsCompleted && item.ActualFinishDate.HasValue && item.ActualFinishDate.Value > planFinish)
                {
                    isLate = true;
                    daysLate = (int)Math.Ceiling((item.ActualFinishDate.Value - planFinish).TotalDays);
                }
                else if (!item.IsCompleted && now > planFinish)
                {
                    isLate = true;
                    isOverdue = true;
                    daysLate = (int)Math.Ceiling((now - planFinish).TotalDays);
                }
            }

            // Calculate completed stages - only check IsCompleted flag
            var completedStagesCount = stageDtos.Count(s => s.IsCompleted);
            return Results.Ok(new
            {
                item = new
                {
                    item.Id,
                    item.CavityId,
                    CavityCode = cavity?.Code,
                    CavityName = cavity != null ? $"{cavity.Code} - {cavity.Description ?? cavity.Location ?? ""}" : null,
                    item.Code,
                    item.LineNo,
                    item.QRCode,
                    item.IsCompleted,
                    item.IsStored,
                    item.CompletedAt,
                    item.PlannedStartDate,
                    item.PlannedFinishDate,
                    item.ActualStartDate,
                    item.ActualFinishDate,
                    PlannedTimeHours = totalPlannedHours,
                    ActualTimeHours = totalActualHours,
                    IsLate = isLate,
                    IsOverdue = isOverdue,
                    DaysLate = daysLate,
                    CompletedStages = completedStagesCount,
                    TotalStages = stageDtos.Count
                },
                materials = cavity?.BOMs?.Select(b => new
                {
                    MaterialId = b.Material?.Id,
                    MaterialName = b.Material?.Name,
                    MaterialType = b.Material?.Type?.Name,
                    Quantity = b.Quantity,
                    Length = b.Length,
                    Width = b.Width,
                    Unit = b.Unit,
                    Color = b.Color
                }).ToList() ?? new(),
                order = new
                {
                    order.Id,
                    order.Code,
                    order.Status,
                    order.ProjectId
                },
                stages = stageDtos
            });
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager,QA,Director" });

        itemGroup.MapPost("/{itemId:int}/assign-qa-bulk", async (int itemId, AssignItemQaDto dto, StageService svc) =>
        {
            await svc.BulkAssignQaToItemAsync(itemId, dto.QaUserId);
            return Results.Ok();
        });

        itemGroup.MapPut("/{itemId:int}/plan", async (int itemId, UpdateItemPlanDto dto, StageService svc) =>
        {
            try
            {
                await svc.UpdateItemPlanAsync(itemId, dto.PlannedStartDate, dto.PlannedFinishDate);
                return Results.Ok();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        itemGroup.MapPut("/{itemId:int}/actuals", async (int itemId, UpdateItemActualDto dto, StageService svc) =>
        {
            try
            {
                await svc.UpdateItemActualsAsync(itemId, dto.ActualStartDate, dto.ActualFinishDate);
                return Results.Ok();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        itemGroup.MapPut("/{itemId:int}/completion", async (int itemId, UpdateItemCompletionDto dto, StageService svc) =>
        {
            try
            {
                await svc.SetItemCompletionStatusAsync(itemId, dto.IsCompleted);
                return Results.Ok();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        // Stage Review Endpoints
        group.MapPost("/{stageId:int}/request-review", async (
            int stageId, 
            StageReviewDataAccess reviewAccess, 
            ProductionItemStageDataAccess stageAccess,
            ProductionItemDataAccess itemAccess,
            ProductionOrderDataAccess orderAccess,
            StageTypeDataAccess stageTypeAccess,
            INotificationService notificationService,
            HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Results.BadRequest("Invalid user");
            }
            var reviewId = await reviewAccess.CreateReviewRequestAsync(stageId, userId);
            
            // Send notification to assigned QA
            try
            {
                var stage = await stageAccess.GetByIdAsync(stageId);
                if (stage != null && stage.AssignedQaUserId.HasValue)
                {
                    var item = await itemAccess.GetByIdAsync(stage.ProductionOrderItemId);
                    if (item != null)
                    {
                        var order = await orderAccess.GetByIdAsync(item.ProductionOrderId);
                        var allStageTypes = await stageTypeAccess.GetAllAsync();
                        var stageType = allStageTypes.FirstOrDefault(st => st.Id == stage.StageTypeId);
                        
                        if (order != null && stageType != null)
                        {
                            await notificationService.AddNotificationAsync(new Notification
                            {
                                Message = $"PM đã yêu cầu kiểm tra giai đoạn {stageType.Name} cho sản phẩm {item.Code}",
                                Url = $"/production/orders/{order.Id}/items/{item.Id}",
                                RecipientUserId = stage.AssignedQaUserId.Value.ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request
                System.Diagnostics.Debug.WriteLine($"Failed to send notification: {ex.Message}");
            }
            
            return Results.Created($"/api/stages/{stageId}/review/{reviewId}", new { id = reviewId });
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager" });

        group.MapGet("/{stageId:int}/review", async (int stageId, StageReviewDataAccess reviewAccess, ProductionItemStageDataAccess stageAccess) =>
        {
            var review = await reviewAccess.GetLatestReviewByStageAsync(stageId);
            if (review == null) return Results.NotFound();
            var stage = await stageAccess.GetByIdAsync(stageId);
            if (stage == null) return Results.NotFound();
            var criteria = (await reviewAccess.GetCriteriaByStageTypeIdAsync(stage.StageTypeId)).ToList();
            var results = (await reviewAccess.GetCriteriaResultsByReviewIdAsync(review.Id)).ToList();
            var reviewAttachments = GetAttachmentsList(review.Attachments) ?? new List<string>();
            return Results.Ok(new
            {
                review = new
                {
                    review.Id,
                    review.ProductionItemStageId,
                    review.RequestedByUserId,
                    review.ReviewedByUserId,
                    review.Status,
                    review.Notes,
                    Attachments = reviewAttachments,
                    review.RequestedAt,
                    review.ReviewedAt
                },
                criteria = criteria.Select(c =>
                {
                    var result = results.FirstOrDefault(r => r.StageCriteriaId == c.Id);
                    var attachments = result != null ? GetAttachmentsList(result.Attachments) ?? new List<string>() : new List<string>();
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Criteria {c.Id}: Raw={result.Attachments ?? "null"}, Parsed={attachments.Count}");
                    }
                    return new
                    {
                        c.Id,
                        c.Code,
                        c.Name,
                        c.Description,
                        c.CheckType,
                        c.Required,
                        c.OrderIndex,
                        Result = result != null ? new
                        {
                            result.IsPassed,
                            result.Value,
                            result.Notes,
                            result.Severity,
                            result.Content,
                            Attachments = attachments
                        } : null
                    };
                }).OrderBy(c => c.OrderIndex).ThenBy(c => c.Id)
            });
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager,QA,Director" });

        group.MapPost("/{stageId:int}/submit-review", async (
            int stageId, 
            SubmitStageReviewDto dto, 
            StageReviewDataAccess reviewAccess,
            ProductionItemStageDataAccess stageAccess,
            ProductionItemDataAccess itemAccess,
            ProductionOrderDataAccess orderAccess,
            StageTypeDataAccess stageTypeAccess,
            INotificationService notificationService,
            HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Results.BadRequest("Invalid user");
            }
            var review = await reviewAccess.GetLatestReviewByStageAsync(stageId);
            if (review == null || review.Status != "pending")
            {
                return Results.BadRequest("Review not found or already submitted");
            }
            var criteriaResults = dto.CriteriaResults.Select(cr => new AGDPMS.Web.Data.StageReviewCriteriaResult
            {
                StageCriteriaId = cr.CriteriaId,
                IsPassed = cr.IsPassed,
                Value = cr.Value,
                Notes = cr.Notes,
                Severity = cr.Severity,
                Content = cr.Content,
                Attachments = cr.Attachments != null && cr.Attachments.Any()
                    ? System.Text.Json.JsonSerializer.Serialize(cr.Attachments)
                    : null
            }).ToList();
            // Use manual IsPassed from DTO if provided, otherwise calculate from criteria results
            var allPassed = dto.IsPassed ?? criteriaResults.All(cr => cr.IsPassed == true);
            await reviewAccess.SubmitReviewAsync(review.Id, userId, criteriaResults, dto.Notes, allPassed, dto.Attachments);
            
            // Send notification to PM who requested the review
            try
            {
                var stage = await stageAccess.GetByIdAsync(stageId);
                if (stage != null)
                {
                    var item = await itemAccess.GetByIdAsync(stage.ProductionOrderItemId);
                    if (item != null)
                    {
                        var order = await orderAccess.GetByIdAsync(item.ProductionOrderId);
                        var allStageTypes = await stageTypeAccess.GetAllAsync();
                        var stageType = allStageTypes.FirstOrDefault(st => st.Id == stage.StageTypeId);
                        
                        if (order != null && stageType != null)
                        {
                            var resultText = allPassed ? "Đạt" : "Không đạt";
                            await notificationService.AddNotificationAsync(new Notification
                            {
                                Message = $"QA đã hoàn thành kiểm tra giai đoạn {stageType.Name} cho sản phẩm {item.Code}. Kết quả: {resultText}",
                                Url = $"/production/orders/{order.Id}/items/{item.Id}",
                                RecipientUserId = review.RequestedByUserId.ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request
                System.Diagnostics.Debug.WriteLine($"Failed to send notification: {ex.Message}");
            }
            
            return Results.Ok();
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "QA" });

        group.MapGet("/{stageId:int}/latest-review", async (int stageId, StageReviewDataAccess reviewAccess, ProductionItemStageDataAccess stageAccess) =>
        {
            System.Diagnostics.Debug.WriteLine($"GET /latest-review called for stage {stageId}");
            var review = await reviewAccess.GetLatestReviewByStageAsync(stageId);
            if (review == null)
            {
                System.Diagnostics.Debug.WriteLine($"No review found for stage {stageId}");
                return Results.NotFound();
            }
            var stage = await stageAccess.GetByIdAsync(stageId);
            if (stage == null)
            {
                System.Diagnostics.Debug.WriteLine($"Stage {stageId} not found");
                return Results.NotFound();
            }
            var criteria = (await reviewAccess.GetCriteriaByStageTypeIdAsync(stage.StageTypeId)).ToList();
            var results = (await reviewAccess.GetCriteriaResultsByReviewIdAsync(review.Id)).ToList();
            var reviewAttachments = GetAttachmentsList(review.Attachments) ?? new List<string>();
            System.Diagnostics.Debug.WriteLine($"Found {results.Count} criteria results for review {review.Id}");
            foreach (var result in results)
            {
                System.Diagnostics.Debug.WriteLine($"  Criteria {result.StageCriteriaId}: Raw Attachments (type={result.Attachments?.GetType().Name}, length={result.Attachments?.Length ?? 0}) = {result.Attachments ?? "null"}");
                if (!string.IsNullOrEmpty(result.Attachments))
                {
                    // Log first 200 chars to see the format
                    var preview = result.Attachments.Length > 200 ? result.Attachments.Substring(0, 200) + "..." : result.Attachments;
                    System.Diagnostics.Debug.WriteLine($"    Preview: {preview}");
                }
                var attachmentsList = GetAttachmentsList(result.Attachments);
                System.Diagnostics.Debug.WriteLine($"    Parsed Count = {attachmentsList?.Count ?? 0}");
                if (attachmentsList != null && attachmentsList.Any())
                {
                    foreach (var att in attachmentsList)
                    {
                        System.Diagnostics.Debug.WriteLine($"      - {att}");
                    }
                }
            }
            return Results.Ok(new
            {
                review = new
                {
                    review.Id,
                    review.ProductionItemStageId,
                    review.RequestedByUserId,
                    review.ReviewedByUserId,
                    review.Status,
                    review.Notes,
                    Attachments = reviewAttachments,
                    review.RequestedAt,
                    review.ReviewedAt
                },
                criteria = criteria.Select(c =>
                {
                    var result = results.FirstOrDefault(r => r.StageCriteriaId == c.Id);
                    var attachments = result != null ? GetAttachmentsList(result.Attachments) ?? new List<string>() : new List<string>();
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Criteria {c.Id}: Raw={result.Attachments ?? "null"}, Parsed={attachments.Count}");
                    }
                    return new
                    {
                        c.Id,
                        c.Code,
                        c.Name,
                        c.Description,
                        c.CheckType,
                        c.Required,
                        c.OrderIndex,
                        Result = result != null ? new
                        {
                            result.IsPassed,
                            result.Value,
                            result.Notes,
                            result.Severity,
                            result.Content,
                            Attachments = attachments
                        } : null
                    };
                }).OrderBy(c => c.OrderIndex).ThenBy(c => c.Id)
            });
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager,QA,Director" });

        // File upload endpoint for review attachments
        group.MapPost("/upload-attachment", async (HttpRequest request) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest("Request must be multipart/form-data");
            }

            var form = await request.ReadFormAsync();
            var file = form.Files.FirstOrDefault();
            if (file == null)
            {
                return Results.BadRequest("No file uploaded");
            }

            try
            {
                // Save file directly
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".mp4", ".mov", ".avi", ".mkv" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    return Results.BadRequest(new { error = $"Định dạng file không được phép. Chỉ chấp nhận: {string.Join(", ", allowedExtensions)}" });
                }

                if (file.Length > 50 * 1024 * 1024) // 50MB max
                {
                    return Results.BadRequest(new { error = "File vượt quá giới hạn 50MB" });
                }

                var webHostEnvironment = request.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

                // Determine upload folder - use WebRootPath if available, otherwise create in ContentRootPath/wwwroot
                string uploadsFolder;
                if (!string.IsNullOrEmpty(webHostEnvironment.WebRootPath))
                {
                    // Use WebRootPath directly (usually wwwroot folder)
                    uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "review-attachments");
                }
                else
                {
                    // Fallback: create wwwroot/uploads in ContentRootPath if WebRootPath is null
                    var wwwrootPath = Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
                    uploadsFolder = Path.Combine(wwwrootPath, "uploads", "review-attachments");
                }

                // Ensure directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var webPath = $"/uploads/review-attachments/{uniqueFileName}";
                return Results.Ok(new { url = webPath, fileName = file.FileName });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization(new AuthorizeAttribute { Roles = "Production Manager,QA,Director" });

        return app;
    }

    private static List<string>? GetAttachmentsList(string? attachmentsJson)
    {
        if (string.IsNullOrWhiteSpace(attachmentsJson))
        {
            System.Diagnostics.Debug.WriteLine("GetAttachmentsList: attachmentsJson is null or empty");
            return new List<string>();
        }
        System.Diagnostics.Debug.WriteLine($"GetAttachmentsList: Input length={attachmentsJson.Length}");
        System.Diagnostics.Debug.WriteLine($"GetAttachmentsList: Raw content (first 200 chars) = {attachmentsJson.Substring(0, Math.Min(200, attachmentsJson.Length))}");

        // Handle the case where PostgreSQL returns escaped JSON with \u0022
        // Example: "[\u0022/file1.jpg\u0022,\u0022/file2.jpg\u0022]"
        // The string might be double-escaped or have outer quotes
        var processed = attachmentsJson.Trim();

        // Remove outer quotes if present (e.g., "\"[\u0022...\u0022]\"")
        if (processed.StartsWith("\"") && processed.EndsWith("\""))
        {
            processed = processed.Substring(1, processed.Length - 2);
            // Unescape the outer quotes
            processed = processed.Replace("\\\"", "\"");
            System.Diagnostics.Debug.WriteLine($"GetAttachmentsList: Removed outer quotes, length={processed.Length}");
        }

        // Replace \u0022 with " (this is the Unicode escape for double quote)
        if (processed.Contains("\\u0022"))
        {
            processed = processed.Replace("\\u0022", "\"");
            System.Diagnostics.Debug.WriteLine($"GetAttachmentsList: After \\u0022 replacement, length={processed.Length}");
            System.Diagnostics.Debug.WriteLine($"GetAttachmentsList: After replacement (first 200 chars) = {processed.Substring(0, Math.Min(200, processed.Length))}");
        }

        // Also handle other escape sequences
        processed = processed.Replace("\\\"", "\"");
        try
        {
            // Try to deserialize
            var result = System.Text.Json.JsonSerializer.Deserialize<List<string>>(processed);
            if (result != null && result.Any())
            {
                System.Diagnostics.Debug.WriteLine($"GetAttachmentsList: Deserialize succeeded, count = {result.Count}");
                foreach (var item in result)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {item}");
                }
                return result;
            }
            else if (result != null)
            {
                System.Diagnostics.Debug.WriteLine("GetAttachmentsList: Deserialize succeeded but result is empty");
                return result;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetAttachmentsList: Deserialize failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"GetAttachmentsList: Processed string = {processed}");
        }

        // Fallback: Manual parsing for format like [\u0022/file1\u0022,\u0022/file2\u0022]
        try
        {
            var cleaned = processed.Trim();
            if (cleaned.StartsWith("[") && cleaned.EndsWith("]"))
            {
                var inner = cleaned.Substring(1, cleaned.Length - 2).Trim();
                if (!string.IsNullOrEmpty(inner))
                {
                    // Split by comma, but handle escaped commas
                    var items = new List<string>();
                    var parts = inner.Split(',');
                    foreach (var part in parts)
                    {
                        var cleanedPart = part.Trim()
                            .Trim('"')
                            .Trim('\'')
                            .Replace("\\u0022", "\"")
                            .Replace("\\\"", "\"");
                        if (!string.IsNullOrEmpty(cleanedPart))
                        {
                            items.Add(cleanedPart);
                        }
                    }
                    if (items.Any())
                    {
                        System.Diagnostics.Debug.WriteLine($"GetAttachmentsList: Manual parse succeeded, count = {items.Count}");
                        foreach (var item in items)
                        {
                            System.Diagnostics.Debug.WriteLine($"  - {item}");
                        }
                        return items;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetAttachmentsList: Manual parse failed: {ex.Message}");
        }
        System.Diagnostics.Debug.WriteLine("GetAttachmentsList: All parsing methods failed, returning empty list");
        return new List<string>();
    }
}
