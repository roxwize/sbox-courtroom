#nullable enable
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThekiFake.Courtroom;

// TODO: Jury, assistant positions, limit callouts by role, add order in the court/gavel pound or whatever
public partial class CourtroomGame : GameManager
{
	public new static CourtroomGame? Current => GameManager.Current as CourtroomGame;
	public const bool DEBUG = true;

	[ConVar.Replicated( "court_typewriter_delay", Help = "How long to wait before revealing message characters in milliseconds" )]
	public int TypewriterDelay { get; set; } = 40;
	
	[Net] public Hud Hud { get; set; }

	public Role Judge = new("Judge")
	{
		PossibleCallouts = new []
		{
			true, false, false, false, true
		}
	};
	public Role Defendant = new("Defendant")
	{
		PossibleCallouts = new []
		{
			true, true, true, true, false
		}
	};
	public Role Prosecutor = new("Prosecutor")
	{
		PossibleCallouts = new []
		{
			true, true, true, true, false
		}
	};
	public Role Witness = new("Witness")
	{
		PossibleCallouts = new []
		{
			true, false, false, false, false // cant use objections or anything like that. stupid fucking loser
		}
	};
	[Net] public IEntity CurrentSpeaker { get; set; }
	private Queue<Message> MessageQueue { get; set; }
	private bool IsDisplayingMessage;

	public CourtroomGame()
	{
		MessageQueue = new Queue<Message>(); // could maybe use Queue<T> in the future
		
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
		if ( IsDisplayingMessage ) // do we need two variables for this?
		{
			var c = CurrentSpeaker as CourtroomPawn;
			if ( (c?.IsValid ?? false) && c.IsSpeaking )
			{
				c.SetAnimParameter( "voice", (float)((Math.Sin( Time.Tick ) + 1) / 2) );
			}
			return;
		}
		if ( !Game.IsServer ) return;
		if ( MessageQueue.Count == 0 ) return;
		DoMessage( MessageQueue.Dequeue() );
	}

	private async void DoMessage( Message message )
	{
		IsDisplayingMessage = true;
		Log.Info( $"{message.Client.Name}: {message.Contents} {(message.Callout != Callout.None ? "(" + message.Callout + ")" : "")}" );

		CourtroomPawn? e = null;
		if ( message.Client.IsValid ) e = message.Client?.Pawn as CourtroomPawn;
		if ( message.Callout != Callout.None )
		{
			CalloutScreen.Callout( message.Callout );
			await GameTask.DelayRealtimeSeconds( 1.5f );
			CalloutScreen.Hide();
		}

		CurrentSpeaker = e ?? Game.LocalPawn;
		if ( e?.IsValid ?? false ) e.IsSpeaking = true;
		// e?.SetAnimParameter( "voice", 1 ); // (float)((Math.Sin( Time.Delta ) + 1) / 2)
		ChatBox.SetMessage( message.Client, message.Contents );
		await GameTask.DelayRealtimeSeconds( message.Contents.Length * (TypewriterDelay / 1000f) );
		if ( e?.IsValid ?? false ) { e.IsSpeaking = false; e.SetAnimParameter( "voice", 0 ); }
		await GameTask.DelayRealtimeSeconds( 0.8f );
		IsDisplayingMessage = false;
	}

	private void SetRole( string? name, IEntity? subject )
	{
		Game.AssertServer();
		if ( name is null ) return;

		switch ( name )
		{
			case "Judge":
				Judge.Entity = subject;
				break;
			case "Prosecutor":
				Prosecutor.Entity = subject;
				break;
			case "Defendant":
				Defendant.Entity = subject;
				break;
			case "Witness":
				Witness.Entity = subject;
				break;
		}
	}
	
	public Role? GetRole( string name )
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
		if ( p?.Entity is not null )
		{
			RoleMenu.Notice( To.Single( ConsoleSystem.Caller ), name, Current.GetRole( name )?.Entity?.Client.Name );
			return;
		}
		Current.SetRole( name, pawn );
		MessageField.SetPossibleCallouts( p?.PossibleCallouts );

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
		Current?.MessageQueue.Enqueue( new Message(message, callout)
		{
			Client = client
		} );
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
