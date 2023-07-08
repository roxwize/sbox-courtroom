using System;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace ThekiFake.Courtroom;

public enum Callout
{
	None,
	Objection,
	HoldIt,
	TakeThat
}

public struct Message
{
	public IClient Client;
	public readonly string Contents;
	public Callout Callout;

	public Message( string contents, Callout callout = Callout.None )
	{
		Contents = contents;
		Callout = callout;
	}
}
