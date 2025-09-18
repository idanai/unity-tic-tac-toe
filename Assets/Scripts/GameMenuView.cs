using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuView : MonoBehaviour
{
	public interface IListener
	{
		void HandleReplayClick();
	}

	[SerializeField] private Button _replayButton;

	private IListener _listener;


	public void Init(IListener listener) => _listener = listener;

	public void ShowReplay() => _replayButton.gameObject.SetActive(true);

	public void HideReplay() => _replayButton.gameObject.SetActive(false);

	[UsedImplicitly]
	public void OnReplayClick() => _listener.HandleReplayClick();
}