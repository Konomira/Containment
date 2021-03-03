using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
	[SerializeField]
	private Button button;
	
	private void Awake() => button.onClick.AddListener(GameLauncher.Connect);
}
