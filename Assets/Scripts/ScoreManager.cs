using UnityEngine;

public interface IScoreView
{
	void SetScore(int score);
}

public class ScoreManager
{
	public enum GameEndState { Draw, PlayerWin, AiWin }

	private readonly IScoreView _scoreView;
	private float _startTurnTime;
	private float _playerReactionTime;
	public int Score { get; private set; }

	public ScoreManager(IScoreView scoreView, int score)
	{
		_scoreView = scoreView;
		Score = score;
	}

	public void OnGameStart() => _playerReactionTime = 0;

	public void OnPlayerTurnStart() => _startTurnTime = Time.time;

	public void OnPlayerTurnEnd() => _playerReactionTime += Time.time - _startTurnTime;

	public void OnGameEnd(GameEndState state)
	{
		var gameScore = (int)CalcFinalScore(state);
		Score += gameScore;
		_scoreView.SetScore(Score);
		Debug.Log($"Game Score: {gameScore}, Duration: {_playerReactionTime}");
	}

	private float CalcFinalScore(GameEndState state) => state switch
	{
		GameEndState.Draw => Mathf.Lerp(2, 49, CalcBonus()),
		GameEndState.PlayerWin => Mathf.Lerp(50, 100, CalcBonus()),
		GameEndState.AiWin => 1,
	};

	private float CalcBonus() => 1 - CalcPenalty();

	private float CalcPenalty() => _playerReactionTime switch
	{
		< 10 => 0,
		> 20 => 1,
		_ => Mathf.InverseLerp(10, 20, _playerReactionTime),
	};
}