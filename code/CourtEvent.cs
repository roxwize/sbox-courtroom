namespace Sandbox;

public static partial class CourtEvent
{
	public const string MessageSent = "message.sent";

	public class MessageSentAttribute : EventAttribute
	{
		public MessageSentAttribute() : base( MessageSent ) {}
	}
}
