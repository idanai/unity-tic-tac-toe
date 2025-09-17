using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[SerializeField] private GameBoardView _boardView;

	private TicTacToeController _gameController;

	private void Start()
	{
		if (_gameController is not null)
			return;

		_gameController = new(_boardView, playerX: null, player0: new BadTicTacToeAi(), isPlayerXFirst: Random.Range(0, 2) is 0);
		UniTask.Create(async () =>
		{
			using (_gameController)
			{
				await _gameController.Play(destroyCancellationToken);
				Debug.Log($"Game finished with state {_gameController.State}");
			}
			_gameController = null;
		});
	}
}