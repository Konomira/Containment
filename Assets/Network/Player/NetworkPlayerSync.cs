using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UIElements;
using EventCode = NetworkEvent.EventCode;
using static NetworkEvent;
public class NetworkPlayerSync : MonoBehaviourPunCallbacks
{
	private Dictionary<int, GameObject> networkPlayers = new Dictionary<int, GameObject>();
	
	[SerializeField]
	private GameObject networkPlayerPrefab;
	
	private GameObject localPlayerInstance;
	
	private void Awake()
	{
		localPlayerInstance = gameObject;
		RegisterEvent(EventCode.Movement, SyncPlayerMovement);
		RegisterEvent(EventCode.RequestMovementSync, ReceiveSyncRequest);
	}

	private void OnDestroy()
	{
		UnregisterEvent(EventCode.Movement, SyncPlayerMovement);
		UnregisterEvent(EventCode.RequestMovementSync, ReceiveSyncRequest);
	}

	public override void OnJoinedRoom()
	{
		foreach(var player in PhotonNetwork.CurrentRoom.Players)
		{
			if(player.Value.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) continue;
            
			var playerInstance = Instantiate(networkPlayerPrefab);
			networkPlayers.Add(player.Key, playerInstance);
		}

		var data = new object[] { PhotonNetwork.CurrentRoom.Players.Keys.ToArray() };
		RaiseEvent(EventCode.RequestMovementSync, data);
	}

	public override void OnPlayerEnteredRoom(Player player)
	{
		var playerInstance = Instantiate(networkPlayerPrefab);
		networkPlayers.Add(player.ActorNumber, playerInstance);
	}

	public override void OnPlayerLeftRoom(Player player)
	{
		Destroy(networkPlayers[player.ActorNumber]);
		networkPlayers.Remove(player.ActorNumber);
	}

	private Dictionary<int, Coroutine> networkLerps = new Dictionary<int, Coroutine>();
	
	// Receives movement data for a specific player and syncs their local object
	private void SyncPlayerMovement(object[] data)
	{
		var posData = new PositionData(data);
		
		if(posData.ID == PhotonNetwork.LocalPlayer.ActorNumber) return;

		if(!networkPlayers.ContainsKey(posData.ID)) return;
		// TODO: Add the player if they don't already exist

		var go = networkPlayers[posData.ID];

		//TODO: Remove magic number for player height offset
		if(networkLerps.ContainsKey(posData.ID))
		{
			StopCoroutine(networkLerps[posData.ID]);
			networkLerps[posData.ID] = StartCoroutine(LerpToPosition(go, posData));
		}
		else
			networkLerps.Add(posData.ID, StartCoroutine(LerpToPosition(go, posData)));
		
		go.transform.rotation = Quaternion.Euler(0, posData.YRotation , 0);
	}

	private IEnumerator LerpToPosition(GameObject go, PositionData posData)
	{
		if(go == null) yield break;
		
		while(Vector3.Distance(go.transform.position, posData.Position) > 0.1f)
		{
			go.transform.position =
				Vector3.MoveTowards(go.transform.position,
				                    posData.Position - Vector3.up * 0.9f, 
				                    10.0f * Time.deltaTime);

			yield return null;
		}

		go.transform.position = posData.Position;
	}

	private void ReceiveSyncRequest(object[] data)
	{
		if(!((int[])data[0]).Contains(PhotonNetwork.LocalPlayer.ActorNumber)) return;

		SendPosition();
	}
	
	// Sends local player position
	private void SendPosition()
	{
		var tr = localPlayerInstance.transform;

		var posData = new PositionData
		{
			ID = PhotonNetwork.LocalPlayer.ActorNumber, 
			Position = tr.position, 
			YRotation = tr.rotation.eulerAngles.y,
		};
		
		RaiseEvent(EventCode.Movement, posData.ToObjectArray());
	}

	public struct PositionData
	{
		public int ID;
		public Vector3 Position;
		public float YRotation;

		public PositionData(object[] data)
		{
			ID = (int)data[0];
			Position = (Vector3)data[1];
			YRotation = (float)data[2];
		}

		public object[] ToObjectArray() => new object[] { ID, Position, YRotation };
	}
}
