using System.Threading;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace ThekiFake.Courtroom;

[StyleSheet("/UI/CalloutScreen.scss")]
public partial class CalloutScreen : Panel
{
	public static CalloutScreen Current;
	public Image Image;

	public Texture Objection;

	public CalloutScreen()
	{
		Current = this;
		Image = Add.Image();
	}

	[ClientRpc]
	public static void Callout( Callout callout )
	{
		if ( Current == null ) return;
		Current.Show();
		Sound.FromScreen( "exclaim" );
		switch ( callout )
		{
			case Courtroom.Callout.Objection:
				Sound.FromScreen( "objection" );
				Current.Image?.SetProperty( "src", "/textures/objection.png" );
				break;
			case Courtroom.Callout.HoldIt:
				Sound.FromScreen( "holdit" );
				Current.Image?.SetProperty( "src", "/textures/holdit.png" );
				// Current.Image.Texture = Objection;
				break;
			case Courtroom.Callout.TakeThat:
				Sound.FromScreen( "takethat" );
				Current.Image?.SetProperty( "src", "/textures/takethat.png" );
				// Current.Image.Texture = Objection;
				break;
			case Courtroom.Callout.None:
			default:
				// We shouldn't be here !!!! GO AWAY!!!!!!!!!!!!!
				Log.Warning( "There's no callout to display, NOOB!!!!!!!" );
				break;
		}
	}

	public void Show()
	{
		RemoveClass( "hidden" );
	}
	
	[ClientRpc]
	public static void Hide()
	{
		Current?.AddClass( "hidden" );
	}
}
