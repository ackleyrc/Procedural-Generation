﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeDemo : MonoBehaviour
{
	public int numNodeRows = 5;
	public int numNodeCols = 5;

	public int targetSegmentLengthMin = 1;
	public int targetPathLengthMax = 25;

	TilesDemo tiles;

	void Awake()
	{
		tiles = GetComponent<TilesDemo> ();
	}

	void Start()
	{
		Node.InitializeNodes (numNodeRows, numNodeCols);
		tiles.InitializeTiles ();
		StartCoroutine (GenerateMaze ());
	}


	public IEnumerator GenerateMaze()
	{
		// First start at any random node
		Node randomNode = Node.GetRandom();

		// The first node will (initially) be a dead end
		randomNode.isDeadEnd = true;

		// Then generate the first path
		yield return StartCoroutine (GeneratePath (randomNode));

		// While there remain nodes with unset nodes adjacent, continue to generate paths from them at random
		while (0 < Node.pathsWithClosedNeighbor.Count)
		{
			yield return StartCoroutine (GeneratePath (Node.GetRandomPathWithClosedNeighbor()));
		}
	}


	public IEnumerator GeneratePath(Node startNode)
	{
		Node currentNode = startNode;
		int currentSegmentLength = 0;
		Coords.Direction? prevDirection = null;

		int doNumAttempts = 0;
		int doMaxAttempts = targetPathLengthMax; // (Node.GetRows() * Node.GetCols());
		do
		{
			doNumAttempts++;
			// Debug.Log("doNumAttempts: " + doNumAttempts);
			// Debug.Log("currentSegmentLength: " + currentSegmentLength);

			// Set the current node to path and inform its neighbors
			if (Node.Type.Path != currentNode.type)
			{
				SetNode(currentNode, Node.Type.Path);

				// Update the connecting tile color to white
				foreach (Coords.Direction connection in currentNode.connections) {
					tiles.ChangeDefault(Coords.Displaced(tiles.GetCoords (currentNode.coords), connection), Color.white);
					tiles.ChangeColor(Coords.Displaced(tiles.GetCoords (currentNode.coords), connection), Color.white, Fade.Transition.Immediate);
				}
			}

			// Update the current tile color to show new path node
			if (currentNode.isDeadEnd)
			{
				tiles.ChangeDefault (tiles.GetCoords (currentNode.coords), Color.magenta);
			}
			else
			{
				tiles.ChangeDefault (tiles.GetCoords (currentNode.coords), Color.white);
			}
			tiles.ChangeColor(tiles.GetCoords (currentNode.coords), Color.yellow, Fade.Transition.Immediate);
			yield return DemoManager.instance.currentWait;

			// Get the potential directions the path could take, i.e. those not already a path
			List<Coords.Direction> potentialDirections = currentNode.closedNeighbors;

			// For Debugging only
			string directions = "";
			for (int i = 0; i < potentialDirections.Count; i++) {
				directions += potentialDirections[i] + " ";
			}
			// Debug.Log("potentialDirections: " + directions);

			// Update the neighboring tiles to show which potential directions exist
			if (0 < potentialDirections.Count)
			{
				foreach (Coords.Direction dir in potentialDirections)
				{
					tiles.ChangeColor(Coords.Displaced(tiles.GetCoords (currentNode.coords), (Coords.Direction) dir, 1), Color.green, Fade.Transition.Immediate);
					tiles.ChangeColor(Coords.Displaced(tiles.GetCoords (currentNode.coords), (Coords.Direction) dir, 2), Color.green, Fade.Transition.Immediate);
				}
				yield return DemoManager.instance.currentWait;
			}

			// If no directions for the path to take, we've reached a dead end
			if (0 == potentialDirections.Count)
			{
				currentNode.isDeadEnd = true;
				// Debug.Log(currentNode + " isDeadEnd");

				// Update the neighboring tiles to show that no potential directions exist
				for (int dir = 0; dir < 4; dir++) {
					tiles.ChangeColor(Coords.Displaced(tiles.GetCoords (currentNode.coords), (Coords.Direction) dir), Color.red, Fade.Transition.Immediate);
				}

				// Update the current tile to show that it is now designated a dead end
				tiles.ChangeColor(tiles.GetCoords (currentNode.coords), Color.magenta, Fade.Transition.Gradual);

				break;
			}
				
			// Debug.Log("Previous Direction: " + prevDirection);
			Coords.Direction nextDirection;
			if (prevDirection == null || 
				!currentNode.neighbors.Contains(prevDirection.Value) || 
				!currentNode.closedNeighbors.Contains(prevDirection.Value) ||
				currentSegmentLength > targetSegmentLengthMin)
			{
				// Chose one of these at random
				// Debug.Log("Choosing next direction at random");
				int randIndex = Random.Range(0, potentialDirections.Count);
				nextDirection = potentialDirections[randIndex];
			}
			else
			{
				// Debug.Log("Continuing in the same direction");
				nextDirection = prevDirection.Value;
			}

			// Increment the currentSegmentLength if continuing in the same direction
			if (prevDirection == null || nextDirection == prevDirection)
			{
				currentSegmentLength++;
				// Debug.Log("New currentSegmentLength: " + currentSegmentLength);
			}
			else
			{
				currentSegmentLength = 0;
			}

			// Debug.Log("nextDirection: " + nextDirection);

			if (doNumAttempts < doMaxAttempts)
			{
				// Create a connection between the current node and that at the choosen direction
				currentNode.ConnectTo(nextDirection);
				Node nextNode = Node.nodes[ Node.GetIndex (Coords.Displaced (currentNode.coords, nextDirection)) ];
				nextNode.ConnectTo(Coords.Inverse(nextDirection));

				// Update whether currentNode is a dead end as needed
				if (currentNode.isDeadEnd && 1 < currentNode.connections.Count)
				{
					currentNode.isDeadEnd = false;
					tiles.ChangeColor(tiles.GetCoords (currentNode.coords), Color.white, Fade.Transition.Gradual);
				}

				// Finally, set the next node as the new current node
				currentNode = nextNode;
				prevDirection = nextDirection;
			}
			else
			{
				currentNode.isDeadEnd = true;

				// Update the previous tiles to show that max path length has been reached
				tiles.ChangeColor(Coords.Displaced(tiles.GetCoords (currentNode.coords), Coords.Inverse(prevDirection.Value), 1), Color.red, Fade.Transition.Immediate);
				tiles.ChangeColor(Coords.Displaced(tiles.GetCoords (currentNode.coords), Coords.Inverse(prevDirection.Value), 2), Color.red, Fade.Transition.Immediate);

				// Update the current tile to show that it is now designated a dead end
				tiles.ChangeColor(tiles.GetCoords (currentNode.coords), Color.magenta, Fade.Transition.Gradual);
			}
		}
		while (doNumAttempts < doMaxAttempts);
		yield return DemoManager.instance.currentWait;
	}

	public void SetNode(Node node, Node.Type nodeType)
	{
		node.type = nodeType;

		// Remove this node from its neighbors' lists of closed neighbors, if setting to open
		if (Node.Type.Path == nodeType || Node.Type.Room == nodeType)
		{
			// Add this node to the set of nodes with closed neighbors if not already there
			// This method should only get called by GeneratePath once at maximum per node
			if (Node.Type.Path == nodeType && node.closedNeighbors.Count > 0)
			{
				if (!Node.pathsWithClosedNeighbor.ContainsKey (node.index))
				{
					Node.pathsWithClosedNeighbor.Add (node.index, node);
				}
			}

			// Removing node from neighbors' closedNeighbors
			foreach (Coords.Direction neighborDirection in node.neighbors)
			{
				// Get the neighboring node in each neighboring direction
				Coords neighborCoords = Coords.Displaced(node.coords, neighborDirection);
				Node neighbor = Node.nodes [Node.GetIndex (neighborCoords)];

				if (neighbor.closedNeighbors.Count > 0)
				{
					// Remove the originating node from the neighbors' closedNeighbors lists
					Coords.Direction inverseDirection = Coords.Inverse (neighborDirection);
					if (neighbor.closedNeighbors.Contains (inverseDirection))
					{
						neighbor.closedNeighbors.Remove (inverseDirection);

						// If no more closed neighbors, remove this node from the global list
						if (0 == neighbor.closedNeighbors.Count)
						{
							Node.pathsWithClosedNeighbor.Remove (Node.GetIndex (neighbor.coords));
						}
					}
				}
			}
		}
	}
}
