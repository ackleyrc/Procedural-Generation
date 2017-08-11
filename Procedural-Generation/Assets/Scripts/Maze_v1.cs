using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze_v1 : MonoBehaviour {

	public struct Coords
	{
		public int row;
		public int col;

		public Coords(int row, int col)
		{
			this.row = row;
			this.col = col;
		}

		public static Coords operator +(Coords a, Coords b)
		{
			return new Coords(a.row + b.row, a.col + b.col);
		}

		public static Coords operator -(Coords a, Coords b)
		{
			return new Coords(a.row - b.row, a.col - b.col);
		}

		public static Coords operator *(Coords a, int i)
		{
			return new Coords(a.row * i, a.col * i);
		}

		public override string ToString() {
			return "Coords(" + this.row + ", " + this.col + ")";
		}
	}

	public enum Direction 
	{
		N=0,
		E,
		S,
		W,
		NE,
		SE,
		SW,
		NW
	}

	public enum Tile
	{
		Wall=0,
		Path
	}

	private Dictionary<Direction, Coords> displacement = new Dictionary<Direction, Coords>()
	{
		{Direction.N, new Coords(1, 0)},
		{Direction.E, new Coords(0, 1)},
		{Direction.S, new Coords(-1, 0)},
		{Direction.W, new Coords(0, -1)},
		{Direction.NE, new Coords(1, 1)},
		{Direction.SE, new Coords(-1, 1)},
		{Direction.SW, new Coords(-1, -1)},
		{Direction.NW, new Coords(1, -1)},
	};


	private float _TILE_SIZE = 1f;
	public int NODE_ROWS = 30;
	public int NODE_COLS = 30;
	private int _GRID_ROWS;
	private int _GRID_COLS;

	private Tile[] grid;
	private List<int> setNodes;
	private List<int> unsetNodes;

	private GameObject wallObject;
	private GameObject pathObject;

	void Awake()
	{
		wallObject = Resources.Load ("Prefabs/Wall") as GameObject;
		pathObject = Resources.Load ("Prefabs/Path") as GameObject;
	}

	void Start()
	{
		_GRID_ROWS = NODE_ROWS * 2 + 1; // These should be odd numbers
		_GRID_COLS = NODE_COLS * 2 + 1; // These should be odd numbers
		grid = new Tile[_GRID_ROWS * _GRID_COLS];
		setNodes = new List<int> ();
		unsetNodes = new List<int> ();
		for (int row = 1; row < _GRID_ROWS; row += 2)
		{
			for (int col = 1; col < _GRID_COLS; col += 2)
			{
				unsetNodes.Add (row * _GRID_COLS + col);
			}
		}
		GenerateMaze ();
		GenerateTiles ();
	}

	public void GenerateMaze()
	{
		Coords randomCoords;
		while (unsetNodes.Count > 0)
		{
			if (0 == setNodes.Count)
			{
				randomCoords = GetCoords (unsetNodes [Random.Range (0, unsetNodes.Count)]);
			}
			else if (0 < unsetNodes.Count)
			{
				randomCoords = GetCoords (setNodes [Random.Range (0, setNodes.Count)]);
			}
			else
			{
				return;
			}
			GeneratePath (randomCoords);
		}
	}

	public void GeneratePath(Coords start)
	{
		bool continuePath = true;
		Coords currentCoords = start;
		if (Tile.Path != GetTile(currentCoords)) {
			SetTile (currentCoords, Tile.Path);
		}

		int doNumAttempts = 0;
		int doMaxAttempts = _GRID_ROWS * _GRID_COLS;
		do
		{
			doNumAttempts++;
			List<int> potentialDirections = new List<int> { 0, 1, 2, 3 };
			while (potentialDirections.Count > 0)
			{
				int randIndex = Random.Range(0, potentialDirections.Count);
				Direction randomDirection = (Direction) potentialDirections[randIndex];
				if (false == IsValid (randomDirection, currentCoords)) {
					potentialDirections.Remove ((int)randomDirection);
				} else {
					SetTile (currentCoords + displacement [randomDirection], Tile.Path);
					SetTile (currentCoords + (displacement [randomDirection] * 2), Tile.Path);
					currentCoords = currentCoords + (displacement [randomDirection] * 2);
					break;
				}
			}

			if (0 == potentialDirections.Count)
			{
				continuePath = false;
			}
		}
		while (continuePath && doNumAttempts < doMaxAttempts);
	}

	public bool IsValid(Direction dir, Coords curCoords)
	{
		Coords targetCoords = curCoords + (displacement [dir] * 2);
		if (targetCoords.row < 0 || targetCoords.row >= _GRID_ROWS || targetCoords.col < 0 || targetCoords.col >= _GRID_COLS)
		{
			// targetCoords outside grid
			return false;
		}
		else if (Tile.Path == GetTile (targetCoords))
		{
			// targetCoords is already a path
			return false;
		}
		else
		{
			for (int i = 0; i < System.Enum.GetNames (typeof(Direction)).Length; i++)
			{
				Coords targetNeighorCoords = targetCoords + displacement [(Direction)i];
				if (targetNeighorCoords.row < 0 || targetNeighorCoords.row >= _GRID_ROWS || targetNeighorCoords.col < 0 || targetNeighorCoords.col >= _GRID_COLS) {
					continue;
				}
				if (Tile.Path == GetTile(targetNeighorCoords)) {
					return false;
				}
			}
			return true;
		}
	}

	public void GenerateTiles()
	{
		Transform parent = this.transform;
		for (int row = 0; row < _GRID_ROWS; row++)
		{
			for (int col = 0; col < _GRID_COLS; col++)
			{
				Vector3 position = new Vector3 (0, row * _TILE_SIZE, col * _TILE_SIZE);
				Quaternion rotation = Quaternion.identity;
				if (Tile.Path == grid [row * _GRID_COLS + col])
				{ 
					GameObject tile = GameObject.Instantiate (this.pathObject, position, rotation, parent);
					tile.transform.localScale = Vector3.one * _TILE_SIZE;
				}
				else
				{
					GameObject tile = GameObject.Instantiate (this.wallObject, position, rotation, parent);
					tile.transform.localScale = Vector3.one * _TILE_SIZE;
				}
			}
		}
	}

	public void SetTile(Coords coords, Tile tileType)
	{
		int index = coords.row * _GRID_COLS + coords.col;
		grid [index] = tileType;
		if (Tile.Path == tileType) {
			if (1 == coords.row % 2 && 1 == coords.col % 2)
			{
				if (!setNodes.Contains (index))
				{
					setNodes.Add (index);
				}
				if (unsetNodes.Contains (index))
				{
					unsetNodes.Remove (index);
				}
			}
		}
	}

	public Tile GetTile(Coords coords)
	{
		return grid [coords.row * _GRID_COLS + coords.col];
	}

	public Coords GetCoords(int gridIndex)
	{
		int row = gridIndex / _GRID_COLS;
		int col = gridIndex % _GRID_COLS;
		return new Coords (row, col);
	}
}
