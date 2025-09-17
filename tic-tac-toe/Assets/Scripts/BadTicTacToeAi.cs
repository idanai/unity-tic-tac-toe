using UnityEngine;

// Predictable AI that always chooses the first available tile. Good for debugging
public class BadTicTacToeAi : ITicTacToeAi
{
	public Vector2Int ChooseMove(IReadOnlyGameBoard board)
	{
		for (var y = 0; y < GameBoard.HEIGHT; y++)
		{
			for (var x = 0; x < GameBoard.WIDTH; x++)
			{
				if (board[x, y] is GameBoard.TileState.Empty)
					return new(x, y);
			}
		}
		throw new System.InvalidOperationException("No moves available");
	}
}