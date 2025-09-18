using System;
using UnityEngine;
using Random = UnityEngine.Random;

// Predictable AI that always chooses the first available tile. Good for debugging
[Serializable]
public class SimpleComputerPlayer : ITicTacToeAi
{
	[SerializeField] private float _minDelaySeconds = 1;
	[SerializeField] private float _maxDelaySeconds = 3;

	public TimeSpan Delay => TimeSpan.FromSeconds(Random.Range(_minDelaySeconds, _maxDelaySeconds));

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

		throw new InvalidOperationException("No moves available");
	}
}