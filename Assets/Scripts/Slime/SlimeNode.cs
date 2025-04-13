using UnityEngine;
using System.Runtime.InteropServices;

public struct SlimeNode
{
    public Vector2 position;
    public Vector2 velocity;
    public Vector2 force;
    public float mass;
}

public static class SlimeNodeUtility
{
    public static int GetStride()
    {
        return Marshal.SizeOf(typeof(SlimeNode));
    }
}

