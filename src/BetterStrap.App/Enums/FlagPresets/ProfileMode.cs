using BetterStrap.Models.Attributes;

namespace BetterStrap.Enums.FlagPresets;

public enum ProfileMode
{
    [EnumName(FromTranslation = "Common.Automatic")]
    Default,
    [EnumName(StaticName = "BetterStraps Official")]
    BetterStrap,
    [EnumName(StaticName = "Stoofs")]
    Stoof
}
