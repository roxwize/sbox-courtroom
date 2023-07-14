using Sandbox;
using System;
using System.Linq;

namespace ThekiFake.Courtroom;

partial class CourtroomPawn : AnimatedEntity
{
	private static readonly Vector3 Rot = new( 0, 0, 1 );
	
	[Net] public string PositionTitle { get; set; }
	public Role? Role => CourtroomGame.Current?.GetRole( PositionTitle );
	[Net] public bool IsSpeaking { get; set; }
	[Net] public TimeSince TimeSinceJoined { get; set; }
	
	public override void Spawn()
	{
		base.Spawn();

		TimeSinceJoined = 0;
		SetModel( "models/citizen/citizen.vmdl" );

		EnableDrawing = true;
	}

	public void Respawn()
	{
		if ( !String.IsNullOrEmpty( PositionTitle ) )
		{
			var spawnpoint = All.OfType<SpawnPoint>().First( t => t.Tags.Has( PositionTitle ) );
			Position = spawnpoint.Position;
			Rotation = spawnpoint.Rotation;
			EnableDrawing = true;
		}
		else
		{
			EnableDrawing = false;
		}
	}

	public void DressFromClient( IClient cl )
	{
		var c = new ClothingContainer();
		c.LoadFromClient( cl );
		c.DressEntity( this );
	}

	// public override void Simulate( IClient cl )
	// {
	// 	base.Simulate( cl );
	// }

	public void Tick()
	{
		
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		var s = CourtroomGame.Current?.CurrentSpeaker;
		s ??= this;
		
		var rot = Rotation.LookAt( s.Position, Vector3.Up ).Angles().WithPitch( 0f ).ToRotation();
		var pos = s.Position + s.Rotation.Forward * 100
		                             + Vector3.Up * 50
									 ;

		if ( CourtroomGame.Current is not { } c ) return;
		if ( !c.SmoothPan )
		{
			Camera.Position = pos;
			Camera.Rotation = rot;
		}
		else
		{
			Camera.Position = Vector3.Lerp( Camera.Position, pos, Time.Delta * 5 );
			Camera.Rotation = Rotation.Lerp( Camera.Rotation, rot, Time.Delta * 7 );
		}

		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 60 );

		Camera.FirstPersonViewer = null;
	}

	public bool CanDoCallout( Callout callout )
	{
		return Role?.PossibleCallouts[(int)callout] ?? false;
	}
}
