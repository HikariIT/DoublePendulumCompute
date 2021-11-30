using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public struct Pendulum
{
    public float angle;
    public float length;
    public Vector2 vel;
    public Vector2 acc;
}

public class DoublePendulumSimulation : MonoBehaviour
{
    public ComputeShader compute;
    public PendulumSettings settings;

    private RenderTexture displayTexture;
    private RenderTexture pendulumTexture;
    private RenderTexture trailTexture;

    const int pendulumKernel = 0;
    const int trailKernel = 1;

    private Pendulum[] pendulums;

    // Start is called before the first frame update
    void Start()
    {
        Init();
        transform.GetComponentInChildren<MeshRenderer>().material.mainTexture = displayTexture;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < settings.stepsPerFrame; i++)
        {
            SimulationStep();
        }
    }

    void Init()
    {
        CreateRenderTexture(ref displayTexture, settings.width, settings.height);
        CreateRenderTexture(ref pendulumTexture, settings.width, settings.height);
        CreateRenderTexture(ref trailTexture, settings.width, settings.height);

        compute.SetTexture(pendulumKernel, "PendulumTexture", pendulumTexture);
        compute.SetTexture(trailKernel, "PendulumTexture", pendulumTexture);
        compute.SetTexture(trailKernel, "TrailTexture", trailTexture);

        compute.SetInt("width", settings.width);
        compute.SetInt("height", settings.height);
        compute.SetInt("quantity", settings.objectQuantity);
        compute.SetInt("pendulumSize", settings.size);

        compute.SetFloat("PI", Mathf.PI);
        compute.SetFloat("G", settings.G);

        pendulums = new Pendulum[settings.objectQuantity];

        for (int i = 0; i < settings.objectQuantity; i++)
        {
            CreatePendulum(i);
        }
    }

    void CreatePendulum(int i)
    {
        Pendulum pendulum = new Pendulum();
        pendulum.angle = Mathf.PI / 2;
        pendulum.length = settings.length * (100 - i * 0.2f) / 100;
        pendulums[i] = pendulum;
    }

    void CreateRenderTexture(ref RenderTexture texture, int width, int height)
    {
        if (texture != null)
        {
            texture.Release();
        }
        texture = new RenderTexture(width, height, 0);
        texture.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
        texture.enableRandomWrite = true;
        texture.Create();
    }

    void Dispatch(ComputeShader cs, int numIterationsX, int numIterationsY = 1, int numIterationsZ = 1, int kernelIndex = 0)
    {
        compute.GetKernelThreadGroupSizes(kernelIndex, out uint x, out uint y, out uint z);
        Vector3Int threadGroupSizes = new Vector3Int((int)x, (int)y, (int)z);

        int numGroupsX = Mathf.CeilToInt(numIterationsX / (float)threadGroupSizes.x);
        int numGroupsY = Mathf.CeilToInt(numIterationsY / (float)threadGroupSizes.y);
        int numGroupsZ = Mathf.CeilToInt(numIterationsZ / (float)threadGroupSizes.y);

        cs.Dispatch(kernelIndex, numGroupsX, numGroupsY, numGroupsZ);
    }

    void SimulationStep()
    {
        compute.SetFloat("deltaTime", Time.deltaTime);
        compute.SetFloat("time", Time.time);

        int totalSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Pendulum));

        ComputeBuffer pendulumBuffer = new ComputeBuffer(settings.objectQuantity, totalSize);
        pendulumBuffer.SetData(pendulums);

        compute.SetBuffer(pendulumKernel, "pendulums", pendulumBuffer);

        Dispatch(compute, settings.width, settings.height, 1, trailKernel);
        Dispatch(compute, settings.objectQuantity, 1, 1, pendulumKernel);

        Graphics.Blit(pendulumTexture, displayTexture);

        pendulumBuffer.GetData(pendulums);
        pendulumBuffer.Dispose();
    }
}
