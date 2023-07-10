using System;
using Sandbox;

namespace ThekiFake.Courtroom;

/// <summary>
/// This is the struct used to represent the possible roles/positions you can have ingame (for example the judge or the witness etc).
/// This has just been implemented so there might be areas where this is used in less than optimal ways.
/// </summary>
public struct Role
{
	public IEntity Entity;
	public string Name;
	public bool[] PossibleCallouts;

	public Role( string name )
	{
		Name = name;
		PossibleCallouts = new bool[Enum.GetNames( typeof( Callout ) ).Length];
		PossibleCallouts[0] = true;
	}

	public void SetCanUse( Callout callout )
	{
		PossibleCallouts[(int)callout] = true;
	}
}
