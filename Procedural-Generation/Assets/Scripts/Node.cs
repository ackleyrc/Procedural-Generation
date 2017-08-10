using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node
{
	public enum Type
	{
		Wall=0,
		Path,
		Room
	}

	private static int _NODE_ROWS;
	private static int _NODE_COLS;

	public static List<Node> nodes;
	public static Dictionary<int, Node> pathsWithClosedNeighbor;

	public int index;
	public Coords coords;
	public List<Coords.Direction> neighbors;
	public List<Coords.Direction> closedNeighbors;
	public List<Coords.Direction> connections;
	public Type type;
	public bool isDeadEnd;
	public int roomID;

	public Node(Coords coords)
	{
		this.index = coords.row * _NODE_COLS + coords.col;
		this.coords = coords;
		this.neighbors = new List<Coords.Direction>();
		this.closedNeighbors = new List<Coords.Direction>();
		this.connections = new List<Coords.Direction>();
		this.type = Type.Wall;
		this.isDeadEnd = false;
		this.roomID = -1;

		for (int i = 0; i < 4; i++)
		{
			Coords targetCoords = Coords.Displaced(this.coords, (Coords.Direction) i);
			if (0 <= targetCoords.row && targetCoords.row < _NODE_ROWS && 0 <= targetCoords.col && targetCoords.col < _NODE_COLS)
			{
				this.neighbors.Add((Coords.Direction) i);
				this.closedNeighbors.Add((Coords.Direction) i);
			}
		}

		nodes.Add(this);
	}

	public override string ToString() {
		return "Node(" + this.coords.row + ", " + this.coords.col + ")";
	}

	public static void InitializeNodes(int rows, int cols)
	{
		_NODE_ROWS = rows;
		_NODE_COLS = cols;
		Debug.Log ("Node Rows, Cols: " + _NODE_ROWS + ", " + _NODE_COLS);
		nodes = new List<Node> ();
		pathsWithClosedNeighbor = new Dictionary<int, Node> ();
		for (int row = 0; row < _NODE_ROWS; row++) {
			for (int col = 0; col < _NODE_COLS; col++) {
				new Node (new Coords (row, col));
			}
		}
	}

	public static Node GetRandom()
	{
		return nodes[Random.Range(0, GetRows()) * GetCols() + Random.Range(0, GetCols())];
	}

	public static Node GetRandomPathWithClosedNeighbor()
	{
		if (0 < pathsWithClosedNeighbor.Count)
		{
			return pathsWithClosedNeighbor.Values.ElementAt (Random.Range (0, pathsWithClosedNeighbor.Count));
		}
		else
		{
			return null;
		}
	}

	public static Coords GetCoords(int nodeIndex)
	{
		int row = nodeIndex / _NODE_COLS;
		int col = nodeIndex % _NODE_COLS;
		return new Coords (row, col);
	}

	public static int GetIndex(Coords nodeCoords)
	{
		return nodeCoords.row * _NODE_COLS + nodeCoords.col;
	}

	public static int GetRows()
	{
		return _NODE_ROWS;
	}

	public static int GetCols()
	{
		return _NODE_COLS;
	}

	public void ConnectTo(Coords.Direction direction)
	{
		this.connections.Add (direction);
	}
}
