using BetterStrap.Models.Attributes;

namespace BetterStrap.Enums.FlagPresets;

public enum LightingMode
{
    Default,
    Voxel,
    ShadowMap,
    Future,
    [EnumName(StaticName = "Unified (Phase 4)")]
    Unified
}
