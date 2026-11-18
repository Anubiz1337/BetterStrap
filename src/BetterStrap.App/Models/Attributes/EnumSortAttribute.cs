using System;

namespace BetterStrap.Models.Attributes;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class EnumSortAttribute : Attribute
{
    public int Order { get; set; }
}
