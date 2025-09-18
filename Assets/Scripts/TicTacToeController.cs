using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

public class TicTacToeController : GameTileView.IListener, IDisposable
{
	public interface IListener
	{
		void HandlePlayerTurnEnded(GameBoard.TileState playerTile);
	}

	public enum GameLoopState : byte
	{
		Start,
		Continue,
		WinX,
		WinO,
		Draw,
	}

	private readonly IListener _listener;
	private readonly GameBoardView _boardView;
	[CanBeNull] private readonly ITicTacToeAi _playerX;
	[CanBeNull] private readonly ITicTacToeAi _playerO;
	private readonly GameBoard _board = new();
	private readonly CancellationTokenSource _cts = new();
	private UniTaskCompletionSource<Vector2Int> _userInputTcs;
	public GameLoopState State { get; private set; } = GameLoopState.Start;
	public byte Turn { get; private set; }
	public bool IsPlayerXFirst { get; }

	public bool IsFinished => State is not (GameLoopState.Start or GameLoopState.Continue) || IsOutOfTurns;

	private bool IsOutOfTurns => Turn is GameBoard.AREA;

	private bool IsPlayerXTurn => Turn % 2 == (IsPlayerXFirst ? 1 : 0);

	private (ITicTacToeAi ai, GameBoard.TileState tileState) CurrentPlayerInfo
		=> IsPlayerXTurn ? (_playerX, GameBoard.TileState.X) : (_playerO, GameBoard.TileState.O);

	/// <param name="playerX">AI, or null for player</param>
	/// <param name="playerO">AI, or null for player</param>
	public TicTacToeController(
		IListener listener,
		GameBoardView boardView,
		[CanBeNull] ITicTacToeAi playerX,
		[CanBeNull] ITicTacToeAi playerO,
		bool isPlayerXFirst
	) {
		_listener = listener;
		_boardView = boardView;
		_playerX = playerX;
		_playerO = playerO;
		IsPlayerXFirst = isPlayerXFirst;
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
		var lifetimeToken = _cts.Token;

		if (State is not GameLoopState.Start)
			throw new InvalidOperationException("Game already started");

		using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, lifetimeToken);

		do
		{
			State = await IterateGameLoop(cts.Token);
			Debug.Log($"Turn = {Turn}");
		} while (State is GameLoopState.Continue);
	}

	private async UniTask<GameLoopState> IterateGameLoop(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (_board.IsWin(out var winShape, out var winnerTile, out var index))
		{
			_boardView.ShowStrikeLine(winShape, index);
			return winnerTile is GameBoard.TileState.X ? GameLoopState.WinX : GameLoopState.WinO;
		}

		// out of turns, it's a draw
		if (IsOutOfTurns)
			return GameLoopState.Draw;

		Turn++;

		var (ai, tileState) = CurrentPlayerInfo;
		await HandlePlayerTurn(ai, tileState, cancellationToken);
		_listener?.HandlePlayerTurnEnded(tileState);

		return GameLoopState.Continue;
	}

	private async UniTask HandlePlayerTurn(ITicTacToeAi ai, GameBoard.TileState tileState, CancellationToken cancellationToken)
	{
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
	}

	void GameTileView.IListener.HandleClick(GameTileView tile)
	{
		if (_userInputTcs is null)
			return;

		var (ai, _) = CurrentPlayerInfo;
		var isPlayer = ai is null;
		var pos = tile.Position;

		if (!isPlayer)
			return;

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
			throw new ArithmeticException(
				$"{nameof(_board.TryGetTile)} and {nameof(_board.TrySetTile)} disagree on bounds at {pos}");
		}

		_userInputTcs.TrySetResult(pos);
	}

	private void SetTile(Vector2Int pos, GameBoard.TileState value)
	{
		_board.TrySetTile(value, pos.x, pos.y);
		_boardView.SetTile(value, pos.x, pos.y);
	}
}