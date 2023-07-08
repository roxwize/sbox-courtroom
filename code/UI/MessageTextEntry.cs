using Sandbox;
using Sandbox.UI;

namespace ThekiFake.Courtroom;

public class MessageTextEntry : TextEntry
{
	public new MessageField Parent;
	
	public override void OnButtonEvent( ButtonEvent e )
	{
		if ( e.Button == "enter" && Text.Length > 1 ) Event.Run( CourtEvent.MessageSent );
	}
}
