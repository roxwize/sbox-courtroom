using System.Threading.Tasks;
using Sandbox.Services;
using Sandbox.UI;

namespace ThekiFake.Courtroom;

public class Leaderboard : Panel
{
	private readonly Leaderboards.Board _board;
	
	public Leaderboard()
	{
		_board = Leaderboards.Get( "most_messages" );
		_board.MaxEntries = 25;
		Update();
	}

	public async void Update()
	{
		DeleteChildren();
		await _board.Refresh();
		foreach ( var entry in _board.Entries )
		{ 
			AddChild<LeaderboardEntry>().Entry = entry;
		}
	}
}
