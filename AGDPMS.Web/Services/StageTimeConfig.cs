using System;

namespace AGDPMS.Web.Services;

/// <summary>
/// Fixed time configuration (in minutes) for automatic stage time estimation.
/// These values are deliberately simple and can be tuned later or moved to appsettings.
/// </summary>
public static class StageTimeConfig
{
    // Common overheads (minutes) applied once per item per stage
    public const double SetupMinutes = 3.0;
    public const double FinishMinutes = 1.0;

    // Aluminum related
    public const double CutAlMinutesPerUnit = 0.5;          // per aluminum BOM quantity
    public const double MillLockMinutesPerGroup = 2.0;      // per (AlQty / 4) group
    public const double CutFlushMinutesPerUnit = 0.4;       // per aluminum BOM quantity

    // Corner related
    public const double CornerCutMinutesPerCorner = 1.0;    // DOOR_CORNER_CUT
    public const double AssembleFrameMinutesPerCorner = 2.5;// ASSEMBLE_FRAME

    // Glass related
    public const double CutGlassMinutesPerSquareMeter = 0.2; // 0.2 min per m2 (12s)
    public const double GlassInstallMinutesPerUnit = 3.0;     // per glass pane

    // Gasket
    public const double GasketMinutesPerUnit = 0.8;          // per gasket BOM quantity

    // Accessories
    public const double AccessoryMinutesPerUnit = 1.5;       // per accessory BOM quantity

    // Silicon (per meter)
    public const double SiliconMinutesPerMeter = 0.1;        // 6 seconds per meter

    // For now we assume 4 corners per rectangular item.
    public const int DefaultCornerCountPerItem = 4;
}


