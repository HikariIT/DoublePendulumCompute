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

	[Header("Trail Settings")]
	public float diffuseRate = 1;

	[Header("Pendulum Settings")]
	public float length = 100;
}
