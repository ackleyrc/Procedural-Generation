using System.Collections;
using System.Collections.Generic;

public class Coords
{
	public enum Direction 
	{
		N=0,
		S,
		E,
		W,
		NW,
		SE,
		NE,
		SW
	}

	public static Direction Inverse(Direction dir)
	{
		int dirInt = (int)dir;
		if (0 == dirInt % 2)
		{
			return (Direction)(dirInt + 1);
		}
		else
		{
			return (Direction)(dirInt - 1);
		}
	}

	private static Dictionary<Direction, Coords> displacement = new Dictionary<Direction, Coords>()
	{
		{Direction.N, new Coords(1, 0)},
		{Direction.S, new Coords(-1, 0)},
		{Direction.E, new Coords(0, 1)},
		{Direction.W, new Coords(0, -1)},
		{Direction.NW, new Coords(1, -1)},
		{Direction.SE, new Coords(-1, 1)},
		{Direction.NE, new Coords(1, 1)},
		{Direction.SW, new Coords(-1, -1)},
	};

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

	public static Coords operator *(Coords a, int c)
	{
		return new Coords(a.row * c, a.col * c);
	}

	public override string ToString() {
		return "Coords(" + this.row + ", " + this.col + ")";
	}

	public static Coords Displaced(Coords coords, Direction direction)
	{
		return coords + displacement [direction];
	}
}
