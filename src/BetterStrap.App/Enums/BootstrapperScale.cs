using BetterStrap.Models.Attributes;

namespace BetterStrap.Enums;

public enum BootstrapperScale
{
    [EnumName(StaticName = "Compact")]
    Compact,
    [EnumName(StaticName = "Normal")]
    Normal,
    [EnumName(StaticName = "Large")]
    Large
}
