using System;
using Sandbox;

namespace ThekiFake.Courtroom;

/// <summary>
/// This is hacked together because I'm not using it in production so why the fuck should i bother making it Perfect
/// </summary>
public class CourtroomBot : Bot
{
	private CourtroomPawn _me;
	private TimeSince _timeSinceSaidAnything = 0;
	private Random _random = new();

	private static string wantssay;
	private static bool willsay;
	private static Callout wantssay_callout;
	
	private static string[] _stupidShitThatICanSay =
	{
		"I am robot",
		"6 is a robot",
		"Obtection!!!",
		"I am be thinking and you are be stinking",
		"I am not pleased with your response.",
		"I'm simon !!",
		"COUGH IN Yur fucking shoul;der BITCH",
		"You ever been a roblox",
		"You can not be a serious feller",
		"It's like people dont even play mario before they make mario",
		"Vargskelethor Ahahah Ah Ahahahahahaha Ha",
		"Wake up babe its Thursday",
		"Ouuucuduhhh he hit me :((((",
		"I am super duper spliced :( A litle bit fifled",
		"IM GOING TO LAY TRACK ON YOUR ASS",
		"Pay attention you can try it. Try content choose create",
		"But of course.",
		"Real Dirk Evil Talk",
		"."
	};
	
	[ConCmd.Admin("court_bot_add", Help = "Spawn a custom debug bot")]
	internal static void SpawnMeh()
	{
		Game.AssertServer();
		_ = new CourtroomBot();
	}

	[ConCmd.Admin("court_bot_say")]
	public static void Say( string say, int callout = 0 )
	{
		wantssay = say;
		wantssay_callout = Enum.GetValues<Callout>()[callout];
		willsay = true;
	}

	public override void Tick()
	{
		_me = Client.Pawn as CourtroomPawn;
		if ( _me is null ) return;
		if ( _me.Role is null )
		{
			var g = CourtroomGame.Current;
			if ( g == null ) return;
		
			foreach ( var role in g.Roles )
			{
				if ( role.Value.Entity is not null && (role.Value.Entity?.IsValid ?? false) ) continue;

				g.Roles[role.Key].Entity = _me;
				_me.Role = role.Value;
				_me.Respawn();
				break;
			}
		}

		if ( willsay )
		{
			CourtroomGame.Speak( wantssay, wantssay_callout, Client );
			willsay = false;
		}

		// if ( !(_timeSinceSaidAnything > 5) ) return;
		//
		// var vals = Enum.GetValues<Callout>();
		// Callout callout = Callout.None;
		// if ( _random.Next( 0, 10 ) > 7 ) callout = vals[_random.Next(1, vals.Length)];
		// CourtroomGame.Speak( _stupidShitThatICanSay[_random.Next(0, _stupidShitThatICanSay.Length)], callout, Client );
		// _timeSinceSaidAnything = 0;
	}
}
