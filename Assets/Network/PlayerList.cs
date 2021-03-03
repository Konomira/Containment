using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class PlayerList : MonoBehaviourPunCallbacks
{
	[SerializeField]
	private GameObject playerNamePrefab;

	[SerializeField]
	private Transform container;
	
	private Dictionary<string, GameObject> playerNameInstances = new Dictionary<string, GameObject>();

	private void Awake()
	{
		foreach(var player in PhotonNetwork.CurrentRoom.Players.Values) 
			AddPlayerToList(player);
	}

	private void AddPlayerToList(Player player)
	{
		if(playerNameInstances.ContainsKey(player.NickName)) return;
		
		var instance = Instantiate(playerNamePrefab, container);
		instance.GetComponent<TMP_Text>().text = player.NickName;
		playerNameInstances.Add(player.NickName, instance);
	}
	
	public override void OnPlayerEnteredRoom(Player player) => AddPlayerToList(player);

	public override void OnPlayerLeftRoom(Player player)
	{
		if(!playerNameInstances.ContainsKey(player.NickName)) return;
		
		Destroy(playerNameInstances[player.NickName]);
		playerNameInstances.Remove(player.NickName);
	}
}
