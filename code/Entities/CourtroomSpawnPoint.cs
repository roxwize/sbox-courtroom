using Editor;
using Sandbox;

namespace ThekiFake.Courtroom;

/// <summary>
/// These spawnpoints are identical to regular ones, but you can define which role they represent. Typically you'll want a judge, prosecutor, defendant, and witness in your map.
/// </summary>
[HammerEntity]
[Title("Courtroom Spawnpoint")]
public class CourtroomSpawnPoint : SpawnPoint
{
	[Property] public string RoleName { get; set; }
	[Property] public bool CanDoObjection { get; set; }
	[Property] public bool CanDoHoldIt { get; set; }
	[Property] public bool CanDoTakeThat { get; set; }
	[Property] public bool CanDoOrder { get; set; }

	public Role Role => new (RoleName)
	{
		Position = Position,
		Rotation = Rotation,
		PossibleCallouts = new []
		{
			true, CanDoObjection, CanDoHoldIt, CanDoTakeThat, CanDoOrder
		}
	};
}
