using System;
using ThekiFake.Courtroom;

namespace Sandbox;

/// <summary>
/// This is hacked together because I'm not using it in production so why the fuck should i bother making it Perfect
/// </summary>
public class CourtroomBot : Bot
{
	private CourtroomPawn _me;
	private TimeSince _timeSinceSaidAnything = 0;
	private Random _random = new();
	
	private static string[] _stupidShitThatICanSay = new[]
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

	public override void Tick()
	{
		_me = Client.Pawn as CourtroomPawn;
		if ( _me is null ) return;
		if ( _me.PositionTitle is null )
		{
			var g = CourtroomGame.Current;
			if ( g == null ) return;
		
			if ( g.Witness.Entity == null ) { g.Witness.Entity = Client.Pawn; _me.PositionTitle = "Witness"; _me.Respawn(); return; }
			if ( g.Defendant.Entity == null ) { g.Defendant.Entity = Client.Pawn; _me.PositionTitle = "Defendant"; _me.Respawn(); return; }
			if ( g.Prosecutor.Entity == null ) { g.Prosecutor.Entity = Client.Pawn; _me.PositionTitle = "Prosecutor"; _me.Respawn(); return; }
			if ( g.Judge.Entity == null ) { g.Judge.Entity = Client.Pawn; _me.PositionTitle = "Judge"; _me.Respawn(); return; }
		}

		if ( !(_timeSinceSaidAnything > 5) ) return;

		var vals = Enum.GetValues<Callout>();
		Callout callout = Callout.None;
		if ( _random.Next( 0, 10 ) > 7 ) callout = vals[_random.Next(1, vals.Length)];
		CourtroomGame.Speak( _stupidShitThatICanSay[_random.Next(0, _stupidShitThatICanSay.Length)], callout, Client );
		_timeSinceSaidAnything = 0;
	}
}
