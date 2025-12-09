using System.Data;
using Dapper;
using AGDPMS.Shared.Models;

namespace AGDPMS.Web.Data;

public class StageReviewDataAccess(IDbConnection conn)
{
    public Task<int> CreateReviewRequestAsync(int stageId, int requestedByUserId) => conn.ExecuteScalarAsync<int>(@"
        insert into stage_reviews(production_item_stage_id, requested_by_user_id, status, requested_at)
        values (@StageId, @RequestedByUserId, 'pending', now())
        returning id",
        new { StageId = stageId, RequestedByUserId = requestedByUserId });

    public Task<StageReview?> GetLatestReviewByStageAsync(int stageId) => conn.QueryFirstOrDefaultAsync<StageReview>(@"
        select id as Id, production_item_stage_id as ProductionItemStageId,
               requested_by_user_id as RequestedByUserId, reviewed_by_user_id as ReviewedByUserId,
               status as Status, notes as Notes, attachments as Attachments,
               requested_at as RequestedAt, reviewed_at as ReviewedAt,
               created_at as CreatedAt, updated_at as UpdatedAt
        from stage_reviews
        where production_item_stage_id = @StageId
        order by created_at desc
        limit 1",
        new { StageId = stageId });

    public Task<StageReview?> GetReviewByIdAsync(int reviewId) => conn.QueryFirstOrDefaultAsync<StageReview>(@"
        select id as Id, production_item_stage_id as ProductionItemStageId,
               requested_by_user_id as RequestedByUserId, reviewed_by_user_id as ReviewedByUserId,
               status as Status, notes as Notes, attachments as Attachments,
               requested_at as RequestedAt, reviewed_at as ReviewedAt,
               created_at as CreatedAt, updated_at as UpdatedAt
        from stage_reviews
        where id = @Id",
        new { Id = reviewId });

    public async Task SubmitReviewAsync(int reviewId, int reviewedByUserId, List<StageReviewCriteriaResult> criteriaResults, string? notes, bool allPassed, List<string>? attachments)
    {
        if (conn.State != System.Data.ConnectionState.Open)
        {
            conn.Open();
        }
        
        using var trans = conn.BeginTransaction();
        try
        {
            var status = allPassed ? "passed" : "failed";
            
            // Update review
            var attachmentsJson = attachments != null && attachments.Any()
                ? System.Text.Json.JsonSerializer.Serialize(attachments)
                : null;

            await conn.ExecuteAsync(@"
                update stage_reviews
                set reviewed_by_user_id = @ReviewedByUserId,
                    status = @Status,
                    notes = @Notes,
                    attachments = @Attachments,
                    reviewed_at = now(),
                    updated_at = now()
                where id = @Id",
                new { Id = reviewId, ReviewedByUserId = reviewedByUserId, Status = status, Notes = notes, Attachments = attachmentsJson },
                trans);

            // Delete old results
            await conn.ExecuteAsync(@"
                delete from stage_review_criteria_results
                where stage_review_id = @ReviewId",
                new { ReviewId = reviewId },
                trans);

            // Insert new results
            foreach (var result in criteriaResults)
            {
                var resultAttachmentsJson = result.Attachments != null && result.Attachments.Any() 
                    ? System.Text.Json.JsonSerializer.Serialize(result.Attachments) 
                    : null;
                await conn.ExecuteAsync(@"
                    insert into stage_review_criteria_results(stage_review_id, stage_criteria_id, is_passed, value, notes, severity, content, attachments, created_at)
                    values (@ReviewId, @CriteriaId, @IsPassed, @Value, @Notes, @Severity, @Content, @Attachments, now())",
                    new { 
                        ReviewId = reviewId, 
                        CriteriaId = result.StageCriteriaId, 
                        IsPassed = result.IsPassed, 
                        Value = result.Value, 
                        Notes = result.Notes,
                        Severity = result.Severity,
                        Content = result.Content,
                        Attachments = resultAttachmentsJson
                    },
                    trans);
            }

            // If all passed, mark stage as completed
            if (allPassed)
            {
                var review = await GetReviewByIdAsync(reviewId);
                if (review != null)
                {
                    await conn.ExecuteAsync(@"
                        update production_item_stages
                        set is_completed = true,
                            completed_at = now(),
                            updated_at = now()
                        where id = @StageId and is_completed = false",
                        new { StageId = review.ProductionItemStageId },
                        trans);
                }
            }

            trans.Commit();
        }
        catch
        {
            trans.Rollback();
            throw;
        }
    }

    public Task<IEnumerable<StageReviewCriteriaResult>> GetCriteriaResultsByReviewIdAsync(int reviewId) => conn.QueryAsync<StageReviewCriteriaResult>(@"
        select id as Id, stage_review_id as StageReviewId, stage_criteria_id as StageCriteriaId,
               is_passed as IsPassed, value as Value, notes as Notes,
               severity as Severity, content as Content, attachments as Attachments,
               created_at as CreatedAt, updated_at as UpdatedAt
        from stage_review_criteria_results
        where stage_review_id = @ReviewId
        order by id",
        new { ReviewId = reviewId });

    public Task<IEnumerable<StageCriteria>> GetCriteriaByStageTypeIdAsync(int stageTypeId) => conn.QueryAsync<StageCriteria>(@"
        select id as Id, stage_type_id as StageTypeId, code as Code, name as Name,
               description as Description, check_type as CheckType, required as Required,
               order_index as OrderIndex, is_active as IsActive,
               created_at as CreatedAt, updated_at as UpdatedAt
        from stage_criteria
        where stage_type_id = @StageTypeId and is_active = true
        order by order_index, id",
        new { StageTypeId = stageTypeId });
}

public class StageReview
{
    public int Id { get; set; }
    public int ProductionItemStageId { get; set; }
    public int RequestedByUserId { get; set; }
    public int? ReviewedByUserId { get; set; }
    public string Status { get; set; } = string.Empty; // pending, in_progress, passed, failed
    public string? Notes { get; set; }
    public string? Attachments { get; set; }
    public DateTime? RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class StageReviewCriteriaResult
{
    public int Id { get; set; }
    public int StageReviewId { get; set; }
    public int StageCriteriaId { get; set; }
    public bool? IsPassed { get; set; }
    public string? Value { get; set; }
    public string? Notes { get; set; }
    public string? Severity { get; set; }
    public string? Content { get; set; }
    public string? Attachments { get; set; } // JSON string
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class StageCriteria
{
    public int Id { get; set; }
    public int StageTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CheckType { get; set; } = "boolean"; // boolean, numeric, text, select
    public bool Required { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

