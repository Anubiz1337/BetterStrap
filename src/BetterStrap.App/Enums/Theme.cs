using BetterStrap.Models.Attributes;

namespace BetterStrap.Enums;

public enum Theme
{
    [EnumName(FromTranslation = "Common.SystemDefault")]
    Default,
    Dark,
    Light,
    BetterStrap,
    UltraGray,
    Berry,
    Blue,
    Cyan,
    Green,
    Orange,
    Pink,
    Purple,
    Red,
    Yellow,
    Custom
}
