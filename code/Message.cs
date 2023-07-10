using System;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace ThekiFake.Courtroom;

// TODO implement gavel pound and such
public enum Callout
{
	None = 0,
	Objection = 1,
	HoldIt = 2,
	TakeThat = 3,
	Order = 4
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

	public static string CalloutString( Callout callout )
	{
		if ( callout == Callout.None ) return "None";
		var s = callout.ToString();
		var builder = new StringBuilder(s.Length * 2).Append( s[0] );
		for ( int i = 1; i < s.Length; i++ )
		{
			if ( char.IsUpper( s[i] ) ) builder.Append( ' ' );
			builder.Append( char.ToLower( s[i] ) );
		}

		builder.Append( '!' );
		return builder.ToString();
	}
}
