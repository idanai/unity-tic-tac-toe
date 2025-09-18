using UnityEngine;
using UnityEngine.UI;

// Heads Up Display View
public class HudView : MonoBehaviour, IScoreView
{
	[SerializeField] private GameTileView _tile;
	[SerializeField] private Text _scoreText;
	[SerializeField] private string _scoreFormat = "Score: {0}";

	public void SetScore(int score) => _scoreText.text = string.Format(_scoreFormat, score);

	public void SetCurrentPlayer(GameBoard.TileState playerTile) => _tile.SetState(playerTile);
}