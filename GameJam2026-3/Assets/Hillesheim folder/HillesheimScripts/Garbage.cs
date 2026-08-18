using System;
using UnityEngine;

public enum GarbageType
{
    plastic,
    paper,
    glass,
    metal
}
public class Garbage : MonoBehaviour
{
    public GarbageType garbageType;
}