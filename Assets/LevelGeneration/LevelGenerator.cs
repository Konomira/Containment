using System.Collections.Generic;
using Photon.Pun.Demo.Cockpit;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class LevelGenerator : MonoBehaviour
{
	[Header("World Dimensions")]
	public int width;
	public int height;

	[Header("Room Settings")]
	public int minSize;
	public int maxSize;

	public int minRoomCount;
	public int maxRoomCount;

	private int[,] levelData;

	private float minDistBetweenRooms;

	private List<Rect> rooms = new List<Rect>();
	private List<Rect> placedRooms = new List<Rect>();
	private void Awake() => GenerateLevel();

	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.Space))
			GenerateLevel();
	}

	private void OnDrawGizmos()
	{
		for(var x = 0; x < width; x++)
		{
			for(var y = 0; y < height; y++)
			{
				Gizmos.color = levelData[x,y] == 1 ? Color.white : Color.black;

				Gizmos.DrawCube(new Vector3(x, 0, y) + (Vector3.one * 0.5f), Vector3.one * 0.9f);
			}
		}
		
		foreach(var room in placedRooms)
		{
			Gizmos.color = Color.red;
			//TL-TR
			Gizmos.DrawLine(new Vector3(room.xMin, 0, room.yMax), new Vector3(room.xMax, 0, room.yMax));
			//TL-BL
			Gizmos.DrawLine(new Vector3(room.xMin, 0, room.yMax), new Vector3(room.xMin, 0, room.yMin));
			//TR-BR
			Gizmos.DrawLine(new Vector3(room.xMax, 0, room.yMax), new Vector3(room.xMax, 0, room.yMin));
			//BL-BR
			Gizmos.DrawLine(new Vector3(room.xMin, 0, room.yMin), new Vector3(room.xMax, 0, room.yMin));	
		}
	}

	[ContextMenu("Generate New Level")]
	private void GenerateLevel()
	{
		rooms = new List<Rect>();
		placedRooms = new List<Rect>();
		levelData = new int[width, height];
		GenerateRooms();
		PlaceRooms();
		PopulateLevel();
		Debug.Log($"Generated {placedRooms.Count} rooms");
	}



	private void GenerateRooms()
	{
		var roomCount = Random.Range(minRoomCount, maxRoomCount);

		for(var i = 0; i < roomCount; i++)
		{
			var roomWidth = Random.Range(minSize, maxSize);
			var roomHeight = Random.Range(minSize, maxSize);
			rooms.Add(new Rect(new Vector2Int(0,0),new Vector2Int(roomWidth,roomHeight)));
		}
	}

	private void PlaceRooms()
	{
		var maxAttempts = 100;

		var roomsToRemove = new List<Rect>();
		foreach(var room in rooms)
		{
			var attempts = 0;
			var placed = false;
			
			while(!placed && attempts < maxAttempts)
			{
				attempts++;
				var x = Random.Range(0, width  - room.size.x);
				var y = Random.Range(0, height - room.size.y);
				room.Set(x, y, room.width, room.height);
				var overlaps = false;
				
				foreach(var placedRoom in placedRooms)
				{
					if(room.Overlaps(placedRoom) || room.IsWithinDistance(placedRoom, minDistBetweenRooms))
						overlaps = true;
				}

				if(placedRooms.Count == 0 || !overlaps)
				{
					placed = true;
					placedRooms.Add(room);
				}
			}

			if(attempts >= maxAttempts)
				roomsToRemove.Add(room);
		}

		foreach(var roomInfo in roomsToRemove) 
			rooms.Remove(roomInfo);
	}
	
	private void PopulateLevel()
	{
		foreach(var room in placedRooms)
		{
			for(int x = (int)room.min.x; x < room.max.x; x++)
			{
				for(int y = (int)room.min.y; y < room.max.y; y++) 
					levelData[x, y] = 1;
			}
		}
	}
}

public static class RectExtension
{
	public static bool IsWithinDistance(this Rect a, Rect b, float distance) =>
		!(Mathf.Abs(a.max.x - b.min.x) > distance &&
		Mathf.Abs(a.min.x - b.max.x) > distance &&
		Mathf.Abs(a.max.y - b.min.y) > distance &&
		Mathf.Abs(a.min.y - b.max.y) > distance);
}