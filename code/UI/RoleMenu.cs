using Sandbox;

namespace ThekiFake.Courtroom;

public partial class RoleMenu
{
	[ClientRpc]
	public static void Notice( string name, string fuckingLoserWhoTookThePosition )
	{
		Current.ErrorLabel.Text = $"The {name} position is taken by {fuckingLoserWhoTookThePosition}! Try another.";
	}
}
