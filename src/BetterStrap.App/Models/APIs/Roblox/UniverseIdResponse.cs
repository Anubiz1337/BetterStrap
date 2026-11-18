using System.Text.Json.Serialization;

namespace BetterStrap.Models.APIs.Roblox;

public class UniverseIdResponse
{
    [JsonPropertyName("universeId")]
    public long UniverseId { get; set; }
}
