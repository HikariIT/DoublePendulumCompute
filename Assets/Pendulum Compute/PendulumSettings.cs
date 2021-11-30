using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Simulations settings", menuName = "Pendulum Simulation Settings")]
public class PendulumSettings : ScriptableObject
{
	[Header("Simulation Settings")]
	[Min(1)] public int stepsPerFrame = 1;
	public int width = 1920;
	public int height = 1080;
	public int objectQuantity = 100;
	public int size = 2;
	public float G = 9.81f;
	public float damp = 0.0001f;

	[Header("Trail Settings")]
	public float diffuseRate = 1;

	[Header("Pendulum Settings")]
	public Vector2 lengths = new Vector2(100, 100);
	public Vector2 masses = new Vector2(10, 10);
	public Vector2 initialAngles = new Vector2(-90, -90);
	public Vector2 angleOffsets = new Vector2(0.001f, 0.001f);
}
