#nullable enable
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ThekiFake.Courtroom;

public partial class CourtroomGame : GameManager
{
	public new static CourtroomGame? Current => GameManager.Current as CourtroomGame;
	public const bool DEBUG = true;

	[ConVar.Replicated( "court_typewriter_delay", Help = "How long to wait before revealing message characters in milliseconds" )]
	public int TypewriterDelay { get; set; } = 40;
	
	[Net] public Hud Hud { get; set; }
	
	[Net] public Entity? Judge { get; set; }
	[Net] public Entity? Defendant { get; set; }
	[Net] public Entity? Prosecutor { get; set; }
	[Net] public Entity? Witness { get; set; }
	[Net] public IEntity CurrentSpeaker { get; set; }
	private IList<Message> MessageQueue { get; set; }

	public CourtroomGame()
	{
		MessageQueue = new List<Message>();
		
		if ( Game.IsClient )
		{
			Hud = new Hud();
		}
		
		if ( DEBUG )
		{
		}
	}

	~CourtroomGame()
	{
		Task.Expire();
	}
	
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		var pawn = new CourtroomPawn();
		client.Pawn = pawn;
		pawn.Respawn();
		pawn.DressFromClient( client );
		// pawn.TimeSinceJoined = 0;
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		Sandbox.Services.Stats.Increment( cl, "longest_time_spent", ((CourtroomPawn)cl.Pawn).TimeSinceJoined );
		
		base.ClientDisconnect( cl, reason );
	}

	[ConCmd.Admin("respawn")]
	public static void Respawn()
	{
		(ConsoleSystem.Caller as CourtroomPawn).Respawn();
	}

	public override void Simulate( IClient cl )
	{
	}

	public async Task DoMessage( Message message )
	{
		if ( message.Callout != Callout.None )
		{
			CalloutScreen.Callout( message.Callout );
			await GameTask.DelayRealtimeSeconds( 1.5f );
			CalloutScreen.Hide();
			Log.Info( "I be selecting these amazing texicans" );
		}
		
		Log.Info( message.Client );

		CurrentSpeaker = message.Client?.Pawn ?? Game.LocalPawn;
		ChatBox.SetMessage( message.Client, message.Contents );
		await GameTask.DelayRealtimeSeconds( TypewriterDelay / (float)message.Contents.Length + 0.8f );
		Log.Info( "Done" );
	}

	public void SetRole( string? name, Entity? subject )
	{
		Game.AssertServer();
		if ( name is null ) return;

		switch ( name )
		{
			case "Judge":
				Judge = subject;
				break;
			case "Prosecutor":
				Prosecutor = subject;
				break;
			case "Defendant":
				Defendant = subject;
				break;
			case "Witness":
				Witness = subject;
				break;
		}
	}
	
	public Entity? GetRole( string name )
	{
		return name switch
		{
			"Judge" => Judge,
			"Prosecutor" => Prosecutor,
			"Defendant" => Defendant,
			"Witness" => Witness,
			_ => null
		};
	}

	/// <summary>
	///		This is called on the server when a user chooses their role
	/// </summary>
	[ConCmd.Server]
	public static void BecomePosition( string name )
	{
		var pawn = ConsoleSystem.Caller.Pawn as CourtroomPawn;
		if ( pawn is not { IsValid: true } ) return;
		if ( Current == null ) return;

		var p = Current.GetRole( name );
		if ( p is not null )
		{
			RoleMenu.Notice( To.Single( ConsoleSystem.Caller ), name, Current.GetRole( name )?.Client.Name );
			return;
		}
		Current.SetRole( name, pawn );
		
		pawn.PositionTitle = name;
		pawn.Respawn();
	}

	[ConCmd.Server]
	public static void Unbecome()
	{
		var pawn = ConsoleSystem.Caller.Pawn as CourtroomPawn;
		if ( pawn == null || Current == null ) return;
		Current.SetRole( pawn.PositionTitle, null );
		pawn.PositionTitle = "";
	}

	public static void Speak( string message, Callout callout, IClient client )
	{
		var _m = new Message( message, callout ) { Client = client };
		Current?.MessageQueue.Add( new Message(message, callout)
		{
			Client = client
		} );
		Current?.DoMessage( _m );
	}
	
	[ConCmd.Server]
	public static void Speak( string message, Callout callout )
	{
		Speak( message, callout, ConsoleSystem.Caller );
	}

	[ConCmd.Admin("court_test")]
	public static void TestMessage()
	{
		if ( Current == null ) return;
		Current.CurrentSpeaker = All.OfType<SpawnPoint>().FirstOrDefault() ?? ConsoleSystem.Caller.Pawn;
		Current.DoMessage( new Message( "This is just a test" ));
	}
}
