using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

public class TicTacToeController : GameTileView.IListener, IDisposable
{
	public enum GameLoopState : byte { Start, Continue, X, O, Draw };

	private readonly GameBoardView _boardView;
	[CanBeNull] private readonly ITicTacToeAi _playerX;
	[CanBeNull] private readonly ITicTacToeAi _player0;
	// state
	private readonly GameBoard _board = new();
	private readonly CancellationTokenSource _cts = new();
	private UniTaskCompletionSource<Vector2Int> _userInputTcs;
	private byte _turn; // up to 9 turns
	private readonly bool _isPlayerXFirst;

	public GameLoopState State { get; private set; } = GameLoopState.Start;

	private bool IsPlayerXTurn => _turn % 2 == (_isPlayerXFirst ? 1 : 0);

	private (ITicTacToeAi ai, GameBoard.TileState tileState) CurrentPlayerInfo
		=> IsPlayerXTurn ? (_playerX, GameBoard.TileState.X) : (_player0, GameBoard.TileState.O);

	/// <param name="playerX">AI, or null for player</param>
	/// <param name="player0">AI, or null for player</param>
	public TicTacToeController(GameBoardView boardView, [CanBeNull] ITicTacToeAi playerX, [CanBeNull] ITicTacToeAi player0, bool isPlayerXFirst)
	{
		_boardView = boardView;
		_playerX = playerX;
		_player0 = player0;
		_isPlayerXFirst = isPlayerXFirst;
		boardView.Clear();
		boardView.Init(this);
	}

	public void Dispose()
	{
		_userInputTcs?.TrySetCanceled();
		_cts.Cancel();
		_cts.Dispose();
	}

	/// <summary>Starts a new game. DO NOT CALL MORE THAN ONCE on the same instance</summary>
	public async UniTask Play(CancellationToken cancellationToken = default)
	{
		if (State is not GameLoopState.Start)
			throw new InvalidOperationException("Game already started");

		using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);
		do
		{
			State = await IterateGameLoop(cts.Token);
			Debug.Log($"Turn = {_turn}");
		} while (State is GameLoopState.Continue);
	}

	private async UniTask<GameLoopState> IterateGameLoop(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (_board.IsWin(out var winShape, out var winner, out var index))
		{
			_boardView.ShowStrikeLine(winShape, index);
			return winner is GameBoard.TileState.X ? GameLoopState.X : GameLoopState.O;
		}

		if (_turn is GameBoard.AREA)
		{
			// out of turns, it's a draw
			return GameLoopState.Draw;
		}

		_turn++;

		var (ai, tileState) = CurrentPlayerInfo;

		Vector2Int pos;
		if (ai is null)
		{
			_userInputTcs = new();
			pos = await _userInputTcs.Task;
			_userInputTcs = null;
		}
		else
		{
			var delay = UniTask.Delay(ai.Delay, cancellationToken: cancellationToken);
			pos = ai.ChooseMove(_board);
			await delay;
		}
		SetTile(pos, tileState);
		return GameLoopState.Continue;
	}

	void GameTileView.IListener.HandleClick(GameTileView tile)
	{
		if (_userInputTcs is null)
			return;

		var (ai, _) = CurrentPlayerInfo;
		var isPlayer = ai is null;
		var pos = tile.Position;

		if (!isPlayer)
		{
			Debug.LogError($"Clicked on tile when it's AI's turn at {pos}");
			return;
		}

		if (!_board.TryGetTile(out var value, pos.x, pos.y))
		{
			Debug.LogError($"Clicked out of bounds cell at {pos}");
			return;
		}

		if (value is not GameBoard.TileState.Empty)
			return;

		// should always succeed since TryGetCell worked
		if (!_board.TrySetTile(GameBoard.TileState.X, pos.x, pos.y))
		{
			throw new ArithmeticException($"{nameof(_board.TryGetTile)} and {nameof(_board.TrySetTile)} disagree on bounds at {pos}");
		}

		_userInputTcs.TrySetResult(pos);
	}

	private void SetTile(Vector2Int pos, GameBoard.TileState value)
	{
		_board.TrySetTile(value, pos.x, pos.y);
		_boardView.SetTile(value, pos.x, pos.y);
	}
}
