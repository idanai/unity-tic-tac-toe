using System;
using UnityEngine;

public interface ITicTacToeAi
{
	TimeSpan Delay { get; }
	Vector2Int ChooseMove(IReadOnlyGameBoard board);
}