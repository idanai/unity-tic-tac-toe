using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour, GameMenu.IListener
{
	[SerializeField] private GameBoardView _boardView;
	[SerializeField] private GameMenu _menu;

	private TicTacToeController _gameController;

	private void Awake()
	{
		_menu.Init(this);
	}

	private void Start()
	{
		Play(destroyCancellationToken).Forget();
	}

	private async UniTaskVoid Play(CancellationToken cancellationToken)
	{
		_menu.HideReplay();
		if (_gameController is not null)
			return;

		_gameController = new(
			boardView: _boardView,
			playerX: null, // null for human player
			player0: new SimpleTicTacToeAi(1, 3),
			isPlayerXFirst: Random.Range(0, 2) is 0);

		using (_gameController)
		{
			await _gameController.Play(cancellationToken);
			Debug.Log($"Game finished with state {_gameController.State}");
		}
		_gameController = null;

		_menu.ShowReplay();
	}

	void GameMenu.IListener.HandleReplayClick() => Play(destroyCancellationToken).Forget();
}