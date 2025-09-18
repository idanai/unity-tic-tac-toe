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

	public ScoreManager(IScoreView scoreView) => _scoreView = scoreView;

	public void OnGameStart() => _playerReactionTime = 0;

	public void OnPlayerTurnStart() => _startTurnTime = Time.time;

	public void OnPlayerTurnEnd() => _playerReactionTime += Time.time - _startTurnTime;

	public void OnGameEnd(GameEndState state)
	{
		Score += (int)CalcFinalScore(state);
		_scoreView.SetScore(Score);
	}

	private float CalcFinalScore(GameEndState state) => state switch
	{
		GameEndState.Draw => Mathf.Lerp(2, 49, CalcPenalty()),
		GameEndState.PlayerWin => Mathf.Lerp(50, 100, 1 - CalcPenalty()),
		GameEndState.AiWin => 1,
	};

	private float CalcPenalty() => _playerReactionTime switch
	{
		< 10 => 0,
		> 20 => 1,
		_ => Mathf.InverseLerp(10, 20, _playerReactionTime),
	};
}