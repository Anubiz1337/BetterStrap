using BetterStrap.Models.Attributes;

namespace BetterStrap.Enums;

public enum BootstrapperStyle
{
    VistaDialog,
    LegacyDialog2008,
    LegacyDialog2011,
    ProgressDialog,
    ClassicFluentDialog,
    ByfronDialog,
    [EnumName(StaticName = "BetterStrap")]
    FluentDialog,
    FluentAeroDialog,
    CustomDialog
}
