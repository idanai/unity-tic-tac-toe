using JetBrains.Annotations;
using UnityEngine;

public class GameTileView : MonoBehaviour
{
	public interface IListener
	{
		void HandleClick(GameTileView tile);
	}


	[SerializeField] private UnityEngine.UI.Button _button;
	[field:SerializeField] public Vector2Int Position { get; private set; }

	[Header("Resources")]
	[SerializeField] private Sprite _xImage;
	[SerializeField] private Sprite _oImage;


	private IListener _listener;


	public bool Interactable
	{
		get => _button.interactable;
		set => _button.interactable = value;
	}

	public void Init(IListener listener) => _listener = listener;

	public void SetState(GameBoard.TileState state)
	{
		var sprite = state switch
		{
			GameBoard.TileState.X => _xImage,
			GameBoard.TileState.O => _oImage,
			_ => null,
		};

		_button.image.sprite = sprite;

		var color = _button.image.color;
		color.a = sprite ? 1 : 0;
		_button.image.color = color;

		Interactable = state is GameBoard.TileState.Empty;
	}

	[UsedImplicitly]
	public void OnClick() => _listener?.HandleClick(this);
}