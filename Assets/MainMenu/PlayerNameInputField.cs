using System;
using TMPro;
using UnityEngine;
using static Photon.Pun.PhotonNetwork;
public class PlayerNameInputField : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField inputField;
	
	private const string playerNamePrefKey = "PlayerName";

	private void Awake() => inputField.onEndEdit.AddListener(SetPlayerName);
	
	private void Start()
	{
		var defaultName = string.Empty;

		if(inputField == null) return;

		if(PlayerPrefs.HasKey(playerNamePrefKey))
		{
			defaultName = PlayerPrefs.GetString(playerNamePrefKey);
			inputField.text = defaultName;
		}

		NickName = defaultName;
	}

	private static void SetPlayerName(string value)
	{
		if(string.IsNullOrEmpty(value)) return;

		NickName = value;
		
		PlayerPrefs.SetString(playerNamePrefKey,value);
	}
}
