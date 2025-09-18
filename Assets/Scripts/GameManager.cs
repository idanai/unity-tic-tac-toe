using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameManager : MonoBehaviour, PlayPerfect.IGameManager, GameMenuView.IListener
{
	[SerializeField] private Transform _boardParent;
	[SerializeField] private Transform _menuParent;

	[SerializeField] private AssetReference _boardAssetRef; // GameBoardView
	[SerializeField] private AssetReference _menuAssetRef; // GameMenu

	private GameBoardView _boardView;
	private GameMenuView _menuView;

	private TicTacToeController _gameController;
	private CancellationTokenSource _runningGameCts;

	public event Action OnGameOver;

	// Note: the assignment defines the player as always playing X
	public UniTask WaitForPlayerTurn()
		=> _gameController.WaitForPlayerXTurnOrEnd().AttachExternalCancellation(destroyCancellationToken);

	public bool IsGameInProgress => _gameController?.IsFinished is true;

	public int GetFinalScore()
	{
		throw new NotImplementedException(); // TODO
	}

	public UniTask LoadNewGameAsync(bool? isUserFirstTurn = null)
	{
		CancelRunningGame(new());
		return Play(isUserFirstTurn, destroyCancellationToken);
	}

	private void CancelRunningGame(CancellationTokenSource newCts)
	{
		if (_runningGameCts is null)
			return;

		_runningGameCts.Cancel();
		_runningGameCts.Dispose();
		_runningGameCts = newCts;
	}

	private async UniTask Play(bool? isPlayerXFirst, CancellationToken cancellationToken)
	{
		if (_gameController is not null)
			return;

		_menuView.HideReplay();

		_gameController = new(
			boardView: _boardView,
			playerX: null, // null for human player
			player0: new SimpleTicTacToeAi(1, 3),
			isPlayerXFirst: isPlayerXFirst ?? UnityEngine.Random.Range(0, 2) is 0);

		using (_gameController)
		{
			await _gameController.Play(cancellationToken);
			OnGameOver?.Invoke();
		}

		_gameController = null;

		_menuView.ShowReplay();
	}

	void GameMenuView.IListener.HandleReplayClick() => Play(null, destroyCancellationToken).Forget();

	private async UniTaskVoid LoadAssets(CancellationToken cancellationToken)
	{
		await UniTask.WhenAll(LoadBoardAsset(cancellationToken), LoadMenuAsset(cancellationToken));
		_menuView.ShowReplay();
	}

	private async UniTask LoadMenuAsset(CancellationToken cancellationToken)
	{
		var go = await _menuAssetRef.InstantiateAsync(_menuParent).ToUniTask(cancellationToken: cancellationToken);
		_menuView = go.GetComponent<GameMenuView>();
		_menuView.Init(this);
	}

	private async UniTask LoadBoardAsset(CancellationToken cancellationToken)
	{
		var go = await _boardAssetRef.InstantiateAsync(_boardParent).ToUniTask(cancellationToken: cancellationToken);
		_boardView = go.GetComponent<GameBoardView>();
		_boardView.Clear();
	}

	private void Awake() => LoadAssets(destroyCancellationToken).Forget();

	private void OnDestroy() => CancelRunningGame(null);
}