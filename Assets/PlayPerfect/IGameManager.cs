using Cysharp.Threading.Tasks;

namespace PlayPerfect
{
	public interface IGameManager
	{
		/// <summary> 
		/// An event that triggers when the User finished or quit the game 
		/// </summary> 
		event System.Action OnGameOver;

		/// <summary> 
		/// Waiting for the end of the Computer Turn or the End of the Game 
		/// </summary> 
		UniTask WaitForPlayerTurn();

		/// <summary> 
		/// A property to check if the game is still in progress or has ended 
		/// </summary> 
		bool IsGameInProgress { get; }

		/// <summary> 
		/// Gets the score of the currently finished game 
		/// 
		/// Score is calculated as a function of Time spent on turns and victory state. 
		/// </summary> 
		/// <returns> The score </returns> 
		int GetFinalScore();

		/// <summary> 
		/// Loads the game assets and sets the state to the initial state of a game. 
		/// </summary> 
		/// <param name="isUserFirstTurn"> Determines whose first turn is it, in case of null it will be randomised</param>
		UniTask LoadNewGameAsync(bool? isUserFirstTurn = null);
	}
}