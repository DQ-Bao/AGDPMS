using AGDPMS.Shared.Models;
using Dapper;
using System.Data.Odbc;

namespace AGDPMS.Shared.Services;

public class WStarService
{
    public async Task<GetAllWStarProjectResult> GetAllProjectAsync(string? dbPath)
    {
        if (string.IsNullOrEmpty(dbPath)) return new GetAllWStarProjectResult { Success = false, ErrorMessage = "File không tồn tại" };

        string connectionString = $"Driver={{Microsoft Access Driver (*.mdb, *.accdb)}};Dbq={dbPath};Uid=Admin;Pwd=;";
        using var conn = new OdbcConnection(connectionString);
        try
        {
            await conn.OpenAsync();
            string query = @"
            select p.Code, p.CreateDate,
            cav.Code, cav.Description, wintyp.Description as WindowType, cav.Quantity, cav.Location, cav.Width, cav.Height,
            bom.Code, mat.Description, mat.Symbol, matyp.Description as MatType, bom.Num, bom.Length, bom.Width, color.ClrDescription as Color, mat.Unit
            from (((((wsProject as p
            left join wsCavity as cav on p.Code = cav.ProjectCode)
            left join wsWindowType as wintyp on cav.WindowType = wintyp.Code)
            left join wsBOMAccessory as bom on cav.Code = bom.CavityCode)
            left join wsMaterial as mat on bom.Code = mat.Code)
            left join wsMaterialType as matyp on mat.MaterialType = matyp.Code)
            left join wsColor as color on mat.ClrRGB = color.ClrRGB";
            var projectDictionary = new Dictionary<string, WStarProject>();
            var result = await conn.QueryAsync<WStarProject, WStarCavity, WStarBOMAccessory, WStarProject>(
                query,
                (project, cavity, material) =>
                {
                    if (string.IsNullOrEmpty(project?.Code)) return null!;
                    if (!projectDictionary.TryGetValue(project.Code, out var projEntry))
                    {
                        projEntry = project;
                        projectDictionary.Add(projEntry.Code, projEntry);
                    }

                    if (!string.IsNullOrEmpty(cavity?.Code))
                    {
                        var cavEntry = projEntry.Cavities.FirstOrDefault(c => c.Code == cavity.Code);
                        if (cavEntry == null)
                        {
                            cavEntry = cavity;
                            projEntry.Cavities.Add(cavity);
                        }

                        if (!string.IsNullOrEmpty(material?.Code) && !string.IsNullOrEmpty(material?.Symbol))
                        {
                            if (!string.Equals(material.MatType, "Gioang", StringComparison.OrdinalIgnoreCase))
                            {
                                var existing = cavEntry.Materials.FirstOrDefault(m =>
                                    m.Code == material.Code &&
                                    m.Length == material.Length &&
                                    m.Width == material.Width
                                );

                                if (existing != null) existing.Num += material.Num;
                                else cavEntry.Materials.Add(material);
                            }
                            else // Group all the gasket with the same code regardless of length
                            {
                                var existing = cavEntry.Materials.FirstOrDefault(m =>
                                    m.Code == material.Code &&
                                    string.Equals(material.MatType, "Gioang", StringComparison.OrdinalIgnoreCase)
                                );
                                if (existing != null) existing.Length += (material.Length * material.Num) / 1000.0;
                                else
                                {
                                    material.Num = 1;
                                    material.Length = (material.Length * material.Num) / 1000.0;
                                    cavEntry.Materials.Add(material);
                                }
                            }
                        }
                    }
                    return projEntry;
                },
                splitOn: "Code");
            return new GetAllWStarProjectResult { Success = true, Projects = [..projectDictionary.Values] };
        }
        catch (Exception ex)
        {
            return new GetAllWStarProjectResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}

public sealed class GetAllWStarProjectResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public IEnumerable<WStarProject> Projects { get; set; } = [];
}