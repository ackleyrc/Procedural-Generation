﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze_v2 : MonoBehaviour
{
	public int numNodeRows = 5;
	public int numNodeCols = 5;

	void Start()
	{
		Node.InitializeNodes (numNodeRows, numNodeCols);
		GenerateMaze ();
		GetComponent<Tiles>().GenerateTiles ();
	}


	public void GenerateMaze()
	{
		// First start at any random node
		Node randomNode = Node.GetRandom();

		// The first node will (initially) be a dead end
		randomNode.isDeadEnd = true;

		// Then generate the first path
		GeneratePath (randomNode);

		// While there remain nodes with unset nodes adjacent, continue to generate paths from them at random
		while (0 < Node.pathsWithClosedNeighbor.Count)
		{
			GeneratePath (Node.GetRandomPathWithClosedNeighbor());
		}
	}


	public void GeneratePath(Node startNode)
	{
		Node currentNode = startNode;
		// Debug.Log ("currentNode: " + currentNode);

		int doNumAttempts = 0;
		int doMaxAttempts = (Node.GetRows() * Node.GetCols());
		do
		{
			doNumAttempts++;

			// Set the current node to path and inform its neighbors
			if (Node.Type.Path != currentNode.type) {
				SetNode(currentNode, Node.Type.Path);
			}

			// Get the potential directions the path could take, i.e. those not already a path
			List<Coords.Direction> potentialDirections = currentNode.closedNeighbors;

			// For Debugging only
			/* string directions = "";
			for (int i = 0; i < potentialDirections.Count; i++) {
				directions += potentialDirections[i] + " ";
			}
			Debug.Log("potentialDirections: " + directions);
			*/

			// If no directions for the path to take, we've reached a dead end
			if (0 == potentialDirections.Count)
			{
				// Debug.Log("potentialDirections.Count: 0");
				currentNode.isDeadEnd = true;
				break;
			}

			// Chose one of these at random
			int randIndex = Random.Range(0, potentialDirections.Count);
			Coords.Direction randomDirection = potentialDirections[randIndex];
			// Debug.Log("randomDirection: " + randomDirection);

			// Create a connection between the current node and that at the choosen direction
			currentNode.ConnectTo(randomDirection);
			Node nextNode = Node.nodes[ Node.GetIndex (Coords.Displaced (currentNode.coords, randomDirection)) ];
			// Debug.Log("nextNode: " + nextNode);
			nextNode.ConnectTo(Coords.Inverse(randomDirection));

			// Update whether currentNode is a dead end as needed
			if (currentNode.isDeadEnd && 1 < currentNode.connections.Count)
			{
				currentNode.isDeadEnd = false;
			}

			// Finally, set the next node as the new current node
			currentNode = nextNode;
		}
		while (doNumAttempts < doMaxAttempts);

	}
		
	public void SetNode(Node node, Node.Type nodeType)
	// public void SetNode(Coords nodeCoords, NodeType nodeType)
	{
		// Set the node accordingly
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
