using AGDPMS.Shared.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using System.Data.Odbc;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace AGDPMS.Shared.Services;

public class WStarService
{
    public async Task<GetAllWStarProjectResult> GetAllProjectAsync(string? dbPath)
    {
        if (string.IsNullOrEmpty(dbPath)) return new GetAllWStarProjectResult { Success = false, ErrorMessage = "File không tồn tại" };

        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return await GetProjectsUsingOdbcAsync(dbPath);
            else
                return await GetProjectsUsingMdbToolsAsync(dbPath);
        }
        catch (Exception ex)
        {
            return new GetAllWStarProjectResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private static async Task<GetAllWStarProjectResult> GetProjectsUsingOdbcAsync(string dbPath)
    {
        string connectionString = $"Driver={{Microsoft Access Driver (*.mdb, *.accdb)}};Dbq={dbPath};Uid=Admin;Pwd=;";
#pragma warning disable CA1416
        using var conn = new OdbcConnection(connectionString);
#pragma warning restore CA1416
        await conn.OpenAsync();
        string query = @"
        select p.Code, p.CreateDate,
        cav.Code, cav.Description, wintyp.Description as WindowType, cav.Quantity, cav.Location, cav.Width, cav.Height,
        bom.Code, mat.Description, mat.Symbol, matyp.Description as MatType, matven.Description as MatVendor, bom.Num, bom.Length, bom.Width, color.ClrDescription as Color, mat.Unit
        from ((((((wsProject as p
        left join wsCavity as cav on p.Code = cav.ProjectCode)
        left join wsWindowType as wintyp on cav.WindowType = wintyp.Code)
        left join wsBOMAccessory as bom on cav.Code = bom.CavityCode)
        left join wsMaterial as mat on bom.Code = mat.Code)
        left join wsMaterialType as matyp on mat.MaterialType = matyp.Code)
        left join wsVendor as matven on mat.VendorCode = matven.Code)
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
                    AddOrMergeMaterial(cavEntry, material);
                }
                return projEntry;
            },
            splitOn: "Code");
        return new GetAllWStarProjectResult { Success = true, Projects = [..projectDictionary.Values] };
    }

    private static async Task<GetAllWStarProjectResult> GetProjectsUsingMdbToolsAsync(string dbPath)
    {
        var projectTable = await ExportTableAsync(dbPath, "wsProject");
        var cavityTable = await ExportTableAsync(dbPath, "wsCavity");
        var bomTable = await ExportTableAsync(dbPath, "wsBOMAccessory");
        var materialTable = await ExportTableAsync(dbPath, "wsMaterial");
        var matTypeTable = await ExportTableAsync(dbPath, "wsMaterialType");
        var vendorTable = await ExportTableAsync(dbPath, "wsVendor");
        var colorTable = await ExportTableAsync(dbPath, "wsColor");
        var windowTypeTable = await ExportTableAsync(dbPath, "wsWindowType");
        var matTypes = matTypeTable.ToDictionary(r => r["Code"], r => r.GetValueOrDefault("Description", string.Empty));
        var vendors = vendorTable.ToDictionary(r => r["Code"], r => r.GetValueOrDefault("Description", string.Empty));
        var colors = colorTable.ToDictionary(r => r["ClrRGB"], r => r.GetValueOrDefault("ClrDescription", string.Empty));
        var winTypes = windowTypeTable.ToDictionary(r => r["Code"], r => r.GetValueOrDefault("Description", string.Empty));
        var projects = new List<WStarProject>();

        foreach (var projRow in projectTable)
        {
            var project = new WStarProject
            {
                Code = projRow["Code"],
                CreateDate = DateTime.TryParse(projRow.GetValueOrDefault("CreateDate"), out var dt) ? dt : DateTime.MinValue,
            };

            var cavities = cavityTable.Where(c => c.GetValueOrDefault("ProjectCode") == project.Code);
            foreach (var cavRow in cavities)
            {
                var cavity = new WStarCavity
                {
                    Code = cavRow["Code"],
                    Description = cavRow.GetValueOrDefault("Description", string.Empty),
                    WindowType = winTypes.GetValueOrDefault(cavRow.GetValueOrDefault("WindowType", string.Empty)),
                    Quantity = int.TryParse(cavRow.GetValueOrDefault("Quantity"), out var q) ? q : 0,
                    Location = cavRow.GetValueOrDefault("Location", string.Empty),
                    Width = double.TryParse(cavRow.GetValueOrDefault("Width"), out var w) ? w : 0,
                    Height = double.TryParse(cavRow.GetValueOrDefault("Height"), out var h) ? h : 0
                };

                var boms = bomTable.Where(b => b.GetValueOrDefault("CavityCode") == cavity.Code);
                foreach (var bomRow in boms)
                {
                    var matRow = materialTable.FirstOrDefault(m => m["Code"] == bomRow["Code"]);
                    if (matRow == null) continue;
                    var material = new WStarBOMAccessory
                    {
                        Code = matRow["Code"],
                        Description = matRow.GetValueOrDefault("Description", string.Empty),
                        Symbol = matRow.GetValueOrDefault("Symbol", string.Empty),
                        MatType = matTypes.GetValueOrDefault(matRow.GetValueOrDefault("MaterialType", string.Empty), string.Empty),
                        MatVendor = vendors.GetValueOrDefault(matRow.GetValueOrDefault("VendorCode", string.Empty), string.Empty),
                        Num = int.TryParse(bomRow.GetValueOrDefault("Num"), out var num) ? num : 0,
                        Length = double.TryParse(bomRow.GetValueOrDefault("Length"), out var len) ? len : 0,
                        Width = double.TryParse(bomRow.GetValueOrDefault("Width"), out var width) ? width : 0,
                        Color = colors.GetValueOrDefault(matRow.GetValueOrDefault("ClrRGB", string.Empty)),
                        Unit = matRow.GetValueOrDefault("Unit", string.Empty)
                    };
                    AddOrMergeMaterial(cavity, material);
                }
                project.Cavities.Add(cavity);
            }
            projects.Add(project);
        }
        return new GetAllWStarProjectResult { Success = true, Projects = projects };
    }

    private static void AddOrMergeMaterial(WStarCavity cavity, WStarBOMAccessory material)
    {
        if (string.IsNullOrEmpty(material?.Code) || string.IsNullOrEmpty(material?.Symbol)) return;
        if (!string.Equals(material.MatType, "Gioang", StringComparison.OrdinalIgnoreCase))
        {
            var existing = cavity.Materials.FirstOrDefault(m =>
                m.Code == material.Code &&
                m.Length == material.Length &&
                m.Width == material.Width
            );
            if (existing != null) existing.Num += material.Num;
            else cavity.Materials.Add(material);
        }
        else // Group all the gasket with the same code regardless of length
        {
            var existing = cavity.Materials.FirstOrDefault(m =>
                m.Code == material.Code &&
                string.Equals(material.MatType, "Gioang", StringComparison.OrdinalIgnoreCase)
            );
            if (existing != null) existing.Length += (material.Length * material.Num) / 1000.0;
            else
            {
                material.Num = 1;
                material.Length = (material.Length * material.Num) / 1000.0;
                cavity.Materials.Add(material);
            }
        }
    }

    private static async Task<List<Dictionary<string, string>>> ExportTableAsync(string dbPath, string tableName)
    {
        var csv = await RunCommandAsync("mdb-export", $"\"{dbPath}\" \"{tableName}\"");
        using var reader = new StringReader(csv);
        using var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true });
        var records = new List<Dictionary<string, string>>();
        await foreach (var record in csvReader.GetRecordsAsync<dynamic>())
        {
            var dict = ((IDictionary<string, object>)record).ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? string.Empty);
            records.Add(dict);
        }
        return records;
    }

    private static async Task<string> RunCommandAsync(string command, string args)
    {
#pragma warning disable CA1416
    using var process = Process.Start(new ProcessStartInfo
    {
        FileName = command,
        Arguments = args,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    }) ?? throw new InvalidOperationException($"Failed to start process for command '{command}'.");

    string output = await process.StandardOutput.ReadToEndAsync();
    string error = await process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    if (process.ExitCode != 0) throw new Exception($"Command '{command} {args}' failed with error: {error}");
#pragma warning restore CA1416
    return output.Trim();
}
}

public sealed class GetAllWStarProjectResult
{
    public required bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public IEnumerable<WStarProject> Projects { get; set; } = [];
}