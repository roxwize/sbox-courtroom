using System;
using Sandbox;

namespace ThekiFake.Courtroom;

/// <summary>
/// This is the struct used to represent the possible roles/positions you can have ingame (for example the judge or the witness etc).
/// This has just been implemented so there might be areas where this is used in less than optimal ways.
/// </summary>
public partial class Role : BaseNetworkable
{
	[Net, Change] public Entity Entity { get; set; }
	[Net] public string Name { get; set; }
	[Net] public Vector3 Position { get; set; }
	[Net] public Rotation Rotation { get; set; }
	public bool[] PossibleCallouts;

	public Role()
	{
		
	}

	public Role( string name )
	{
		Name = name;
		PossibleCallouts = new bool[Enum.GetNames( typeof( Callout ) ).Length];
		PossibleCallouts[0] = true;
	}

	public void OnEntityChanged( Entity oldValue, Entity newValue )
	{
		RoleMenu.Current.StateHasChanged();
		Log.Info( newValue );
	}

	public void SetCanUse( Callout callout )
	{
		PossibleCallouts[(int)callout] = true;
	}

	/*public override string ToString()
	{
		return $"Role \"{Name}\" ( [ {string.Join( ',', PossibleCallouts )} ] {Entity?.IsValid})";
	}*/
}
