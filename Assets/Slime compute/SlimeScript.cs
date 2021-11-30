using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Random = System.Random;

public struct Agent
{
    public Vector2 position;
    public float angle;
}

public enum SpawnMode
{
    CentralDisperse,
    RandomDistribution,
    CircleDisperse,
    AlongsideWall
}

public class SlimeScript : MonoBehaviour
{
    private Random rnd;
    public ComputeShader computeShader;
    public RenderTexture renderTexture, diffuseTexture;
    public SpawnMode spawnMode;
    public int width = 128;
    public int height = 128;
    public int numAgents = 16;
    public int stepsPerFrame;
    public int sensorAngleDegrees = 45;
    public int sensorSize = 2;
    public float moveSpeed;
    public float turnSpeed = 1f;

    private Agent[] agents;


    uint hash(uint state)
    {
        state ^= 2747636419u;
        state *= 2654435769u;
        state ^= state >> 16;
        state *= 2654435769u;
        state ^= state >> 16;
        state *= 2654435769u;
        return state;
    }

    private void CreateAgent(int x)
    {
        
        Agent newAgent = new Agent();

        float angle;

        switch (spawnMode)
        {
            case SpawnMode.CentralDisperse:
                newAgent.position = new Vector2(width / 2, height / 2);
                newAgent.angle = (float)Math.PI * 2 * x / numAgents;
                break;

            case SpawnMode.RandomDistribution:
                newAgent.position = new Vector2(rnd.Next(0, width), rnd.Next(0, height));
                newAgent.angle = (float)rnd.NextDouble() * (float)Math.PI * 2;
                break;

            case SpawnMode.CircleDisperse:
                angle = (float)rnd.NextDouble() * (float)Math.PI * 2;
                newAgent.position = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 200 + new Vector2(width / 2, height / 2);
                newAgent.angle = angle + (float)Math.PI;
                break;

            case SpawnMode.AlongsideWall:
                newAgent.position = new Vector2(rnd.Next(0, 10), rnd.Next(0, height));
                newAgent.angle = 0f + (float) rnd.NextDouble() / 10;
                break;

        }
                
        agents[x] = newAgent;
    }

    private void Start()
    {
        rnd = new Random();
        renderTexture = new RenderTexture(width, height, 0);
        renderTexture.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
        diffuseTexture = new RenderTexture(width, height, 0);
        diffuseTexture.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;

        renderTexture.enableRandomWrite = true;
        diffuseTexture.enableRandomWrite = true;

        renderTexture.Create();
        diffuseTexture.Create();

        agents = new Agent[numAgents];

        for (int i = 0; i < numAgents; i++)
        {
            CreateAgent(i);
        }

        computeShader.SetInt("numAgents", numAgents);
        computeShader.SetInt("width", renderTexture.width);
        computeShader.SetInt("height", renderTexture.height);
        computeShader.SetInt("sensorSize", sensorSize);

        computeShader.SetFloat("PI", (float) Math.PI);
        computeShader.SetFloat("diffuseSpeed", 1f);
        computeShader.SetFloat("decayRate", 0.01f);
        computeShader.SetFloat("sensorOffsetDistance", 10f);
        computeShader.SetFloat("sensorAngleDegrees", sensorAngleDegrees);
        computeShader.SetFloat("turnSpeed", turnSpeed);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(renderTexture, dest);
    }

    void Update()
    {
        for (int i = 0; i < stepsPerFrame; i++)
        {
            Simulate();
        }
    }

    void Simulate()
    {
        if (renderTexture)
        {
            int updateKernel = computeShader.FindKernel("Update");
            int diffuseKernel = computeShader.FindKernel("Diffuse");
            int totalSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Agent));

            ComputeBuffer agentBuffer = new ComputeBuffer(numAgents, totalSize);
            agentBuffer.SetData(agents);

            computeShader.SetFloat("deltaTime", Time.deltaTime);
            computeShader.SetFloat("time", Time.time);
            computeShader.SetFloat("moveSpeed", moveSpeed);

            computeShader.SetBuffer(updateKernel, "agents", agentBuffer);
            computeShader.SetTexture(updateKernel, "TrailMap", renderTexture);
            computeShader.SetTexture(diffuseKernel, "TrailMap", renderTexture);
            computeShader.SetTexture(diffuseKernel, "DiffuseMap", diffuseTexture);

            computeShader.Dispatch(updateKernel, numAgents, 1, 1);
            computeShader.Dispatch(diffuseKernel, width / 8, height / 8, 1);

            Graphics.Blit(diffuseTexture, renderTexture);

            agentBuffer.GetData(agents);
            agentBuffer.Dispose();
        }
    }
}
