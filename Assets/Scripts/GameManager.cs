using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour, PlayPerfect.IGameManager, GameMenu.IListener
{
	[SerializeField] private GameBoardView _boardView;
	[SerializeField] private GameMenu _menu;

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

		_menu.HideReplay();

		_gameController = new(
			boardView: _boardView,
			playerX: null, // null for human player
			player0: new SimpleTicTacToeAi(1, 3),
			isPlayerXFirst: isPlayerXFirst ?? UnityEngine.Random.Range(0, 2) is 0); 

		using (_gameController)
		{
			await _gameController.Play(cancellationToken);
			Debug.Log($"Game finished with state {_gameController.State}");
			OnGameOver?.Invoke();
		}
		_gameController = null;

		_menu.ShowReplay();
	}

	void GameMenu.IListener.HandleReplayClick() => Play(null, destroyCancellationToken).Forget();

	private void Awake() => _menu.Init(this);

	private void OnDestroy() => CancelRunningGame(null);

	private void Start()
	{
		CancelRunningGame(new());
		Play(null, destroyCancellationToken).Forget();
	}
}