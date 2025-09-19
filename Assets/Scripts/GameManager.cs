using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameManager : MonoBehaviour, PlayPerfect.IGameManager, TicTacToeController.IListener, GameMenuView.IListener
{
	[Header("References")]
	[SerializeField] private Transform _boardParent;
	[SerializeField] private Transform _menuParent;
	[SerializeField] private Transform _hudParent;

	[Header("Assets")]
	[SerializeField] private AssetReference _boardAssetRef; // GameBoardView
	[SerializeField] private AssetReference _menuAssetRef; // GameMenu
	[SerializeField] private AssetReference _hudAssetRef; // HUDView

	[Header("Settings")]
	[SerializeField] private string _saveKey = "GameSave";
	[SerializeField] private SimpleComputerPlayer _ai;

	private GameBoardView _boardView;
	private GameMenuView _menuView;
	private HudView _hudView;

	private readonly ISaveSystem _saveSystem = new TextSaveSystem();
	private TicTacToeController _gameController;
	private ScoreManager _scoreManager;
	private CancellationTokenSource _runningGameCts;
	private UniTaskCompletionSource _playerTurnTcs;

	public event Action OnGameOver;

	// Note: the assignment defines the player as always playing X
	public UniTask WaitForPlayerTurn() => _playerTurnTcs.Task;

	public bool IsGameInProgress => _gameController?.IsFinished is true;

	public int GetFinalScore() => _scoreManager.Score;

	public UniTask LoadNewGameAsync(bool? isUserFirstTurn = null)
	{
		CancelRunningGame(new());
		return Play(isUserFirstTurn, destroyCancellationToken);
	}

	private void CancelRunningGame(CancellationTokenSource newCts)
	{
		if (_runningGameCts is not null)
		{
			_runningGameCts.Cancel();
			_runningGameCts.Dispose();
		}
		_runningGameCts = newCts;
	}

	/// <param name="cancellationToken">Quits the game</param>
	private UniTask Play(bool? isPlayerXFirst, CancellationToken cancellationToken)
	{
		if (_gameController is not null)
			return default;

		_menuView.HideReplay();

		_gameController = new(
			listener: this,
			boardView: _boardView,
			playerX: null, // null for human player
			playerO: _ai,
			isPlayerXFirst: isPlayerXFirst ?? UnityEngine.Random.Range(0, 2) is 0);

		return StartPlay(cancellationToken);
	}

	private async UniTask StartPlay(CancellationToken cancellationToken)
	{
		using var _ = cancellationToken.RegisterWithoutCaptureExecutionContext(() =>
		{
			_playerTurnTcs?.TrySetCanceled(cancellationToken);
			_playerTurnTcs = null;
			HandleGameEnd();
			_gameController?.Dispose();
			_gameController = null;
		});


		using (_gameController)
		{
			cancellationToken.ThrowIfCancellationRequested();
			_playerTurnTcs = new();

			_hudView.SetCurrentPlayer(_gameController.IsPlayerXFirst ? GameBoard.TileState.X : GameBoard.TileState.O);
			_scoreManager.OnGameStart();

			await _gameController.Play(cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();

			_scoreManager.OnGameEnd(_gameController.State switch
			{
				TicTacToeController.GameLoopState.Draw => ScoreManager.GameEndState.Draw,
				TicTacToeController.GameLoopState.WinX => ScoreManager.GameEndState.PlayerWin,
				TicTacToeController.GameLoopState.WinO => ScoreManager.GameEndState.AiWin,
				_ => throw new InvalidOperationException("Game ended in invalid state"),
			});
			_hudView.SetCurrentPlayer(GameBoard.TileState.Empty);
			HandleGameEnd();
		}
		_gameController = null;

		_playerTurnTcs?.TrySetResult();
		_playerTurnTcs = null;

		_menuView.ShowReplay();
	}

	private void HandleGameEnd()
	{
		_saveSystem.Save(_saveKey, new GameSaveData
		{
			Board = _gameController.GetBoard(),
			Score = GetFinalScore(),
			Turn = _gameController.Turn,
			State = _gameController.State,
			IsPlayerXFirst = _gameController.IsPlayerXFirst,
		});
		Debug.Log(GetFinalScore());
		OnGameOver?.Invoke();
	}

	void GameMenuView.IListener.HandleReplayClick() => Play(null, destroyCancellationToken).Forget();

	void TicTacToeController.IListener.HandlePlayerTurnEnded(GameBoard.TileState playerTile)
	{
		switch (playerTile)
		{
			case GameBoard.TileState.O:
				_hudView.SetCurrentPlayer(GameBoard.TileState.X);
				_scoreManager.OnPlayerTurnStart();
				_playerTurnTcs?.TrySetResult();
				_playerTurnTcs = new();
				break;

			case GameBoard.TileState.X:
				_hudView.SetCurrentPlayer(GameBoard.TileState.O);
				_scoreManager.OnPlayerTurnEnd();
				break;
		}
	}

	private async UniTaskVoid LoadAssets(CancellationToken cancellationToken)
	{
		await UniTask.WhenAll(
			LoadBoardAsset(cancellationToken),
			LoadMenuAsset(cancellationToken),
			LoadHudAsset(cancellationToken));

		_menuView.HideReplay();
		if (_saveSystem.TryLoad(_saveKey, out GameSaveData saveData))
		{
			_scoreManager.SetScore(saveData.Score);
			_gameController = new(this, _boardView, saveData, null, new SimpleComputerPlayer());
			StartPlay(destroyCancellationToken).Forget();
		}
		else
		{
			LoadNewGameAsync().Forget();
		}
	}

	private async UniTask LoadBoardAsset(CancellationToken cancellationToken)
	{
		var go = await _boardAssetRef.InstantiateAsync(_boardParent).ToUniTask(cancellationToken: cancellationToken);
		_boardView = go.GetComponent<GameBoardView>();
		_boardView.Clear();
	}

	private async UniTask LoadMenuAsset(CancellationToken cancellationToken)
	{
		var go = await _menuAssetRef.InstantiateAsync(_menuParent).ToUniTask(cancellationToken: cancellationToken);
		_menuView = go.GetComponent<GameMenuView>();
		_menuView.Init(this);
	}

	private async UniTask LoadHudAsset(CancellationToken cancellationToken)
	{
		var go = await _hudAssetRef.InstantiateAsync(_hudParent).ToUniTask(cancellationToken: cancellationToken);
		_hudView = go.GetComponent<HudView>();
		_scoreManager = new(_hudView);
	}

	private void Awake() => LoadAssets(destroyCancellationToken).Forget();

	private void OnDestroy() => CancelRunningGame(null);
}