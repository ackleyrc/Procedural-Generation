using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilesDemo : MonoBehaviour
{
	private int _TILE_ROWS;
	private int _TILE_COLS;

	public float TILE_SIZE;

	public enum Type
	{
		Closed=0,
		Open
	}

	private GameObject[] tiles;
	private GameObject tileObject;

	void Awake()
	{
		tileObject = Resources.Load ("Prefabs/Tile") as GameObject;
	}

	public void InitializeTiles()
	{
		_TILE_ROWS = (Node.GetRows() * 2) + 1;
		_TILE_COLS = (Node.GetRows() * 2) + 1;
		Debug.Log ("Tile Rows, Cols: " + _TILE_ROWS + ", " + _TILE_COLS);

		tiles = new GameObject[_TILE_ROWS * _TILE_COLS];
		for (int row = 0; row < _TILE_ROWS; row++) {
			for (int col = 0; col < _TILE_COLS; col++) {
				SetTile (new Coords (row, col));
			}
		}
	}

	public void SetTile(Coords coords)
	{	
		int index = GetIndex(coords);
		Vector3 position = new Vector3 (0, coords.row * TILE_SIZE, coords.col * TILE_SIZE);
		Quaternion rotation = Quaternion.identity;
		Transform parent = this.transform;
		GameObject tile = GameObject.Instantiate (this.tileObject, position, rotation, parent);
		tile.transform.localScale = Vector3.one * TILE_SIZE;
		tiles [index] = tile;
	}

	public void ChangeDefault(Coords tileCoords, Color color)
	{
		GetTile (tileCoords).GetComponent<Fade> ().ChangeDefault (color);
	}

	public void ChangeColor (Coords tileCoords, Color color, Fade.Transition transition)
	{
		GetTile (tileCoords).GetComponent<Fade> ().ChangeColor (color, transition);
	}

	public GameObject GetTile(Coords tileCoords)
	{
		return tiles [GetIndex (tileCoords)];
	}

	public int GetIndex(Coords tileCoords)
	{
		return tileCoords.row * _TILE_COLS + tileCoords.col;;
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
}
