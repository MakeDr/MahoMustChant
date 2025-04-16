using UnityEngine;
using System.Runtime.InteropServices; // For StructLayout

// Node structure for C# - MUST match layout and padding of HLSL struct
[StructLayout(LayoutKind.Sequential)]
public struct SlimeNode
{
    public Vector2 position; // 8 bytes
    public Vector2 velocity; // 8 bytes
    public float mass;     // 4 bytes
    // --- Padding to reach 32 bytes total ---
    public float padding1; // 4 bytes (Total 24)
    public float padding2; // 4 bytes (Total 28)
    public float padding3; // 4 bytes (Total 32)
}

// Core Data structure for C# - MUST match layout of HLSL struct
[StructLayout(LayoutKind.Sequential)]
public struct CoreBufferData
{
    // Using Vector4 for easy 16-byte alignment per member
    public Vector4 posRotFlags; // xy: position, z: rotation(rad), w: collisionFlags (packed uint as float)
    public Vector4 normalMana;  // xy: relevantCollisionNormal, z: mana, w: unused/padding
    // Total size should be 16 + 16 = 32 bytes
}