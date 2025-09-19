using System;
using PlayPerfect;

[Serializable]
public struct GameSaveData
{
	public GameBoard.TileState[] Board;
	public int Score;
	public byte Turn;
	public TicTacToeController.GameLoopState State;
	public bool IsPlayerXFirst;
	public float PlayerReactionTime;

	public GameSaveData(IGameManager manager, TicTacToeController controller, ScoreManager score)
	{
		Board = controller.GetBoard();
		score = score;
	}
}