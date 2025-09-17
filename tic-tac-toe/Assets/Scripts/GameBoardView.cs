using UnityEngine;
using UnityEngine.UI;

public class GameBoardView : MonoBehaviour
{
	[SerializeField] private Image _strikeLine;
	[SerializeField] private Image _diagonalStrikeLine;

	[Tooltip("Images for cells, ordered left to right, top to bottom. 9 are required")]
	[SerializeField] private GameTileView[] _tiles;


	public GameTileView this[int x, int y] => GetTile(x, y);

	public void Init(GameTileView.IListener listener)
	{
		foreach (var tile in _tiles)
		{
			tile.Init(listener);
		}
	}

	public void SetTile(GameBoard.TileState value, int x, int y) => GetTile(x, y).SetState(value);

	private GameTileView GetTile(int x, int y) => _tiles[x + y * GameBoard.WIDTH];

	public void Clear()
	{
		foreach (var tile in _tiles)
		{
			tile.Interactable = true;
			tile.SetState(GameBoard.TileState.Empty);
		}
		_strikeLine?.gameObject.SetActive(false); // TODO rm ?
		_diagonalStrikeLine?.gameObject.SetActive(false); // TODO rm ?
	}

	public void Awake()
	{
		if (_tiles is not {Length: GameBoard.WIDTH * GameBoard.HEIGHT})
			Debug.LogError($"GameBoardView on {name} requires exactly {GameBoard.WIDTH * GameBoard.HEIGHT} cells, found {_tiles?.Length ?? 0}");
	}

	public void ShowStrikeLine(GameBoard.WinShape winShape, int index)
	{
		// TODO
	}
}