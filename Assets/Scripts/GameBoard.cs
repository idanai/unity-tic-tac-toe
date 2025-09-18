using System;

public interface IReadOnlyGameBoard
{
	GameBoard.TileState this[int x, int y] { get; }

	bool TryGetTile(out GameBoard.TileState value, int x, int y);
}

// can potentially be a wrapper-struct of array, and IReadOnlyGameBoard can be a wrapper struct of that
public class GameBoard : IReadOnlyGameBoard
{
	// types

	public enum TileState : byte { Empty = 0, X = 1, O = 2 }

	public enum WinShape : byte { Horizontal, Vertical, ForwardSlash, BackSlash }

	// constants

	public const int WIDTH = 3;
	public const int HEIGHT = 3;
	public const int AREA = WIDTH * HEIGHT;

	// data

	private readonly TileState[] _tiles = new TileState[AREA];

	// methods

	public void Clear() => Array.Clear(_tiles, 0, _tiles.Length);

	public TileState this[int x, int y]
	{
		get => _tiles[GetIndex(x, y)];
		set => _tiles[GetIndex(x, y)] = value;
	}

	public bool TryGetTile(out TileState value, int x, int y)
	{
		if (!TryGetIndex(out var index, x, y))
		{
			value = default;
			return false;
		}

		value = _tiles[index];
		return true;
	}

	public bool TrySetTile(TileState value, int x, int y)
	{
		if (!TryGetIndex(out var index, x, y))
			return false;

		_tiles[index] = value;
		return true;
	}

	private static bool TryGetIndex(out int index, int x, int y)
	{
		if (!CheckBounds(x, y))
		{
			index = default;
			return false;
		}

		index = GetIndex(x, y);
		return true;
	}

	public static int GetIndex(int x, int y) => x + y * WIDTH;

	private static bool CheckBounds(int x, int y)
		=> x is >= 0 and < WIDTH && y is >= 0 and < HEIGHT;

	/// <summary>Checks if there's a win on the board</summary>
	/// <param name="winShape">diagonal / horizontal / vertical</param>
	/// <param name="winner">tile of the winner</param>
	/// <param name="index">index in the row / column, depending on "winShape"</param>
	/// <returns></returns>
	public bool IsWin(out WinShape winShape, out TileState winner, out int index)
	{
		return CheckRows(out winShape, out winner, out index)
			|| CheckColumns(out winShape, out winner, out index)
			|| CheckDiagonals(out winShape, out winner, out index);
	}

	private bool CheckRows(out WinShape winShape, out TileState winner, out int index)
	{
		for (var y = 0; y < HEIGHT; y++)
		{
			var value = _tiles[GetIndex(0, y)];

			if (value is TileState.Empty)
				break;

			if (_tiles[GetIndex(1, y)] == value && _tiles[GetIndex(2, y)] == value)
			{
				winShape = WinShape.Horizontal;
				winner = value;
				index = y;
				return true;
			}
		}

		return ReturnLose(out winShape, out winner, out index);
	}

	private bool CheckColumns(out WinShape winShape, out TileState winner, out int index)
	{
		for (var x = 0; x < WIDTH; x++)
		{
			var value = _tiles[GetIndex(x, 0)];

			if (value is TileState.Empty)
				break;

			if (_tiles[GetIndex(x, 1)] == value && _tiles[GetIndex(x, 2)] == value)
			{
				winShape = WinShape.Vertical;
				winner = value;
				index = x;
				return true;
			}
		}

		return ReturnLose(out winShape, out winner, out index);
	}

	private bool CheckDiagonals(out WinShape winShape, out TileState winner, out int index)
	{
		const int centerIndex = 1 * WIDTH + 1;
		const int topLeftIndex = 0;
		const int bottomRightIndex = 2 * WIDTH + 2;
		const int topRightIndex = 2;
		const int bottomLeftIndex = 2 * WIDTH;

		index = centerIndex;
		winner = _tiles[centerIndex];

		if (winner is not TileState.Empty)
		{
			if (_tiles[topLeftIndex] == winner && _tiles[bottomRightIndex] == winner)
			{
				winShape = WinShape.BackSlash;
				return true;
			}

			if (_tiles[topRightIndex] == winner && _tiles[bottomLeftIndex] == winner)
			{
				winShape = WinShape.ForwardSlash;
				return true;
			}
		}

		return ReturnLose(out winShape, out winner, out index);
	}

	private static bool ReturnLose(out WinShape winShape, out TileState winner, out int index)
	{
		winShape = default;
		winner = default;
		index = default;
		return false;
	}
}