using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles : MonoBehaviour
{
	private int _TILE_ROWS;
	private int _TILE_COLS;

	public float TILE_SIZE;

	public enum Type
	{
		Closed=0,
		Open
	}

	private Type[] tiles;

	private GameObject wallObject;
	private GameObject pathObject;

	void Awake()
	{
		wallObject = Resources.Load ("Prefabs/Wall") as GameObject;
		pathObject = Resources.Load ("Prefabs/Path") as GameObject;
	}
	
	public void GenerateTiles()
	{
		// Debug.Log ("GenerateTiles()");
		InitializeTiles ();

		// Set the origin tile
		SetTile (GetCoords (0), Type.Closed);

		// Set the bottom border tiles
		for (int index = 1; index < _TILE_COLS; index++)
		{
			SetTile (GetCoords (index), Type.Closed);
		}

		// Set the left border tiles
		for (int index = _TILE_COLS; index < _TILE_ROWS * _TILE_COLS; index += _TILE_COLS)
		{
			SetTile (GetCoords (index), Type.Closed);
		}

		// For each node, set the four tiles at, North of, East of, and NorthEast of the node
		for (int index = 0; index < Node.nodes.Count; index++)
		{
			// Debug.Log ("index: " + index);
			Node node = Node.nodes [index];
			// Debug.Log ("node: " + node);
			// Debug.Log ("node.type: " + node.type);
			Coords tileCoordsAtNode = GetCoords (node.coords);
			// Debug.Log ("tileCoordsAtNode: " + tileCoordsAtNode);

			if (Node.Type.Path == node.type || Node.Type.Room == node.type)
			{
				SetTile (tileCoordsAtNode, Type.Open);

				bool connectedNorth = node.connections.Contains (Coords.Direction.N);
				if (connectedNorth)
				{
					SetTile (Coords.Displaced (tileCoordsAtNode, Coords.Direction.N), Type.Open);
				}
				else
				{
					SetTile (Coords.Displaced (tileCoordsAtNode, Coords.Direction.N), Type.Closed);
				}

				bool connectedEast = node.connections.Contains (Coords.Direction.E);
				if (connectedEast)
				{
					SetTile (Coords.Displaced (tileCoordsAtNode, Coords.Direction.E), Type.Open);
				}
				else
				{
					SetTile (Coords.Displaced (tileCoordsAtNode, Coords.Direction.E), Type.Closed);
				}

				bool hasNorthNeighbor = node.neighbors.Contains (Coords.Direction.N);
				bool hasEastNeighbor = node.neighbors.Contains (Coords.Direction.E);
				bool isSameRoomNorth = hasNorthNeighbor && node.roomID == Node.nodes [Node.GetIndex (Coords.Displaced (node.coords, Coords.Direction.N))].roomID;
				bool isSameRoomEast = hasEastNeighbor && node.roomID == Node.nodes [Node.GetIndex (Coords.Displaced (node.coords, Coords.Direction.E))].roomID;
				if (Node.Type.Room == node.type && isSameRoomNorth && isSameRoomEast)
				{
					SetTile (Coords.Displaced (tileCoordsAtNode, Coords.Direction.NE), Type.Open);
				}
				else
				{
					SetTile (Coords.Displaced (tileCoordsAtNode, Coords.Direction.NE), Type.Closed);
				}
			}
			else
			{
				SetTile (tileCoordsAtNode, Type.Closed);
				SetTile (Coords.Displaced (tileCoordsAtNode, Coords.Direction.N), Type.Closed);
				SetTile (Coords.Displaced (tileCoordsAtNode, Coords.Direction.E), Type.Closed);
				SetTile (Coords.Displaced (tileCoordsAtNode, Coords.Direction.NE), Type.Closed);
			}
		}
	}

	public void SetTile(Coords coords, Type type)
	{	
		// Debug.Log ("SetTile( " + coords + ", " + type.ToString() + " )");
		int index = coords.row * _TILE_COLS + coords.col;
		tiles [index] = type;
		// Debug.Log ("tiles [" + index + "]: " + tiles [index]);
		Vector3 position = new Vector3 (0, coords.row * TILE_SIZE, coords.col * TILE_SIZE);
		Quaternion rotation = Quaternion.identity;
		Transform parent = this.transform;
		if (Type.Open == type)
		{ 
			// Debug.Log ("Creating Open Tile");
			GameObject tile = GameObject.Instantiate (this.pathObject, position, rotation, parent);
			tile.transform.localScale = Vector3.one * TILE_SIZE;
		}
		else
		{
			// Debug.Log ("Creating Closed Tile");
			GameObject tile = GameObject.Instantiate (this.wallObject, position, rotation, parent);
			tile.transform.localScale = Vector3.one * TILE_SIZE;
		}
	}

	public Coords GetCoords(int tileIndex)
	{
		int row = tileIndex / _TILE_COLS;
		int col = tileIndex % _TILE_COLS;
		return new Coords (row, col);

	}

	public Coords GetCoords(Coords nodeCoords)
	{
		Coords tileCoords = new Coords ((nodeCoords.row * 2) + 1, (nodeCoords.col * 2) + 1);
		return tileCoords;
	}

	private void InitializeTiles()
	{
		_TILE_ROWS = (Node.GetRows() * 2) + 1;
		_TILE_COLS = (Node.GetRows() * 2) + 1;
		Debug.Log ("Tile Rows, Cols: " + _TILE_ROWS + ", " + _TILE_COLS);

		tiles = new Type[_TILE_ROWS * _TILE_COLS];
	}
}
