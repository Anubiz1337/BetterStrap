using BetterStrapClient.WebServer.Enums;

namespace BetterStrapClient.WebServer.Models;

internal class FriendRequest
{
	public int Inviter { get; set; }

	public int Invitee { get; set; }

	public FriendStatus? Status { get; set; }
}
