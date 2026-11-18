using System.Text.Json.Serialization;

namespace BetterStrapClient.WebServer;

internal class AssetLocation
{
	[JsonPropertyName("location")]
	public string? Location { get; set; }
}
