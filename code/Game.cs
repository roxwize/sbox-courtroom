using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThekiFake.Courtroom;

// TODO: Add order in the court/gavel pound or whatever
// ReSharper disable once ClassNeverInstantiated.Global
public partial class CourtroomGame : GameManager
{
	public new static CourtroomGame Current => GameManager.Current as CourtroomGame;

	[ConVar.Replicated( "court_typewriter_delay", Help = "How long to wait before revealing message characters in milliseconds" )]
	public int TypewriterDelay { get; set; } = 40;
	
	[Net] public Hud Hud { get; set; }

	[Net] public IDictionary<string, Role> Roles { get; set; }
	[Net] public IEntity CurrentSpeaker { get; set; }
	private Queue<Message> MessageQueue { get; set; }
	private bool IsDisplayingMessage;
	[Net, Predicted] public bool SmoothPan { get; set; } = false;

	public CourtroomGame()
	{
		Roles = new Dictionary<string, Role>();
		MessageQueue = new Queue<Message>();
		
		if ( Game.IsClient )
		{
			Hud = new Hud();
		}
	}

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();

		// fill our Roles dictionary with the spawnpoints the map has
		var i = 0;
		foreach ( var spawnPoint in All.OfType<SpawnPoint>() )
		{
			if ( spawnPoint is CourtroomSpawnPoint p ) Roles.Add( p.RoleName, p.Role );
			else Roles.Add( $"Spawnpoint {++i}", new Role($"Spawnpoint {i}")
			{
				Name = $"Spawnpoint {i}",
				PossibleCallouts = new [] { true, true, true, true, false },
				Position = spawnPoint.Position,
				Rotation = spawnPoint.Rotation
			} );
		}
		Log.Info( $"Successfully loaded {Roles.Count} roles" );
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
		(ConsoleSystem.Caller as CourtroomPawn)?.Respawn();
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

		CourtroomPawn e = null;
		if ( message.Client.IsValid ) e = message.Client?.Pawn as CourtroomPawn;
		if ( message.Callout != Callout.None )
		{
			CalloutScreen.Callout( message.Callout );
			await GameTask.DelayRealtimeSeconds( 1.5f );
			CalloutScreen.Hide();
			SmoothPan = true;
		}

		CurrentSpeaker = e ?? Game.LocalPawn;
		if ( e?.IsValid ?? false ) e.IsSpeaking = true;
		// e?.SetAnimParameter( "voice", 1 ); // (float)((Math.Sin( Time.Delta ) + 1) / 2)
		ChatBox.SetMessage( message.Client, message.Contents );
		await GameTask.DelayRealtimeSeconds( message.Contents.Length * (TypewriterDelay / 1000f) );
		if ( e?.IsValid ?? false ) { e.IsSpeaking = false; e.SetAnimParameter( "voice", 0 ); }
		await GameTask.DelayRealtimeSeconds( 0.8f );
		IsDisplayingMessage = false;
		SmoothPan = false;
	}

	/// <summary>
	///		This is called on the server when a user chooses their role
	/// </summary>
	[ConCmd.Server]
	public static void BecomePosition( string name )
	{
		BecomePosition( ConsoleSystem.Caller, name );
	}

	public static void BecomePosition( IClient cl, string name )
	{
		var pawn = cl.Pawn as CourtroomPawn;
		if ( pawn is not { IsValid: true } ) return;
		if ( Current == null ) return;
		
		var p = Current.Roles[name];
		if ( !cl.IsBot && p.Entity is not null )
		{
			RoleMenu.Notice( To.Single( cl ), name, Current.Roles[name].Entity?.Client.Name );
			return;
		}
		
		p.Entity = pawn;
		MessageField.SetPossibleCallouts( p.PossibleCallouts );
		
		pawn.Role = p;
		pawn.Respawn();
	}

	[ConCmd.Server]
	public static void Unbecome()
	{
		var pawn = ConsoleSystem.Caller.Pawn as CourtroomPawn;
		if ( pawn == null || Current == null ) return;
		Log.Info( Current.Roles[pawn.Role.Name] );
		Current.Roles[pawn.Role.Name].Entity = null;
		pawn.Role = null;
		pawn.Respawn();
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
		Current.CurrentSpeaker = All.OfType<CourtroomSpawnPoint>().FirstOrDefault() ?? ConsoleSystem.Caller.Pawn;
		Current.DoMessage( new Message( "This is just a test" ));
	}

	[ConCmd.Server("court_debug_roles")]
	public static void LogRoles()
	{
		if ( Current == null ) return;
		var i = 0;
		foreach ( var role in Current.Roles )
		{
			Log.Info( $"{i++} ({role.Key}): {role.Value}" );
		}
	}
}
