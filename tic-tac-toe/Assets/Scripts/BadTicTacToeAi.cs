using System;
using UnityEngine;
using Random = UnityEngine.Random;

// Predictable AI that always chooses the first available tile. Good for debugging
public class BadTicTacToeAi : ITicTacToeAi
{
	private readonly float _minDelaySeconds;
	private readonly float _maxDelaySeconds;

	public BadTicTacToeAi(float minDelaySeconds = 1, float maxDelaySeconds = 3)
	{
		_minDelaySeconds = minDelaySeconds;
		_maxDelaySeconds = maxDelaySeconds;
	}

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