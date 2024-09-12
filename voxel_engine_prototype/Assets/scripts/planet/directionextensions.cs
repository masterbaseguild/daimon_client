using System;
using UnityEngine;

public static class directionextensions
{
    public static Vector3Int getVector(this direction direction)
    {
        return direction switch
        {
            direction.up => Vector3Int.up,
            direction.down => Vector3Int.down,
            direction.right => Vector3Int.right,
            direction.left => Vector3Int.left,
            direction.forward => Vector3Int.forward,
            direction.backwards => Vector3Int.back,
            _ => throw new Exception("Invalid input direction")
        };
    }
}