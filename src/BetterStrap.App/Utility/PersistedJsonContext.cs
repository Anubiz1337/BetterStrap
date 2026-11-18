using System.Collections.Generic;
using System.Text.Json.Serialization;
using BetterStrap.Models.Persistable;

namespace BetterStrap.Utility;

[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(State))]
[JsonSerializable(typeof(RobloxState))]
[JsonSerializable(typeof(DownloadStats))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(BetterStrap.Models.RobloxWindowIconOverride))]
[JsonSerializable(typeof(List<BetterStrap.Models.RobloxWindowIconOverride>))]
internal sealed partial class PersistedJsonContext : JsonSerializerContext
{
}
