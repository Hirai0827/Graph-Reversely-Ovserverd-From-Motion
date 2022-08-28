using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Serialization;

public struct Particle
{
    public Vector2 pos;
    public Vector2 vel;
    public float time;
    public int isActive;
}

//TODO パーティクルに対する外力を定義する
//TODO パーティクルの状態（炎上とか成長とか）

public struct ParticleConnection
{
    public int indexA;
    public int indexB;
    public Vector2 basePosition;
    public float intensity;
}


public class ParticleManager : MonoBehaviour
{
    [SerializeField] private ParticleRenderer renderer;
    [SerializeField] private ComputeShader computeParticleShader;
    [SerializeField] private ComputeShader computeConnectionShader;
    [SerializeField] private ComputeShader computeInteractionShader;
    [SerializeField] private ComputeShader computePoolShader;
    [SerializeField] private ComputeShader sortShader; 
    [SerializeField] private int particleLimit;
    [SerializeField] private int connectionLimit;




    private SwappableComputeBuffer<int> connectionIndexBuffer;
    private SwappableComputeBuffer<Particle> particleBuffer;
    private SwappableComputeBuffer<ParticleConnection> connectionBuffer;
    private ComputeBuffer particleIndexPool;
    private ComputeBuffer connectionIndexPool;
    private ComputeBuffer connectionBeginIndexBuffer;
    
    private ComputeParticleDispatcher dispatcher;
    private ComputeConnectionDispatcher connectionDispatcher;
    private ComputeInteractionDispatcher interactionDispatcher;
    private ComputePoolDispatcher computePoolDispatcher;

    private void InitInternal()
    {
        particleBuffer = new SwappableComputeBuffer<Particle>(particleLimit);
        connectionIndexBuffer = new SwappableComputeBuffer<int>(connectionLimit);
        connectionBuffer = new SwappableComputeBuffer<ParticleConnection>(connectionLimit);
        
        particleIndexPool = new ComputeBuffer(particleLimit, Marshal.SizeOf(typeof(int)), ComputeBufferType.Append);
        connectionIndexPool = new ComputeBuffer(connectionLimit, Marshal.SizeOf(typeof(int)), ComputeBufferType.Append);
        
        connectionBeginIndexBuffer = new ComputeBuffer(particleLimit, Marshal.SizeOf(typeof(int)));
        
        dispatcher = new ComputeParticleDispatcher(computeParticleShader);
        connectionDispatcher = new ComputeConnectionDispatcher(computeConnectionShader,sortShader);
        interactionDispatcher = new ComputeInteractionDispatcher(computeInteractionShader);
        computePoolDispatcher = new ComputePoolDispatcher(computePoolShader);
        
        computePoolDispatcher.DispatchInit(ref particleIndexPool,ref connectionIndexPool);
        connectionDispatcher.DispatchInit(connectionBuffer,connectionIndexBuffer);
        dispatcher.DispatchInit(particleBuffer);
        renderer.Init();
    }

    private void Awake()
    {
        InitInternal();
    }

    private void FixedUpdate()
    {
        UpdateInternal(0.016f);
    }

    private void Update()
    {
        Render();
        if (Input.GetKeyDown(KeyCode.A))
        {
            interactionDispatcher.DispatchDrawCircle(particleBuffer,connectionBuffer,particleIndexPool,connectionIndexPool);
        }
    }


    private void UpdateInternal(float deltaTime)
    {
        
        
        var array = new ParticleConnection[connectionBuffer.Src.count];
        connectionBuffer.Src.GetData(array);
        
        
        Debug.Log(connectionIndexBuffer.Src.count);
        var array2 = new uint[connectionIndexBuffer.Src.count];
        connectionIndexBuffer.Src.GetData(array2);
        var str = "";
        for (int i = 0; i < array2.Length; i++)
        { 
            str += "(" + array[array2[i]].indexA + "," + array[array2[i]].indexB + ")";
           //str += "(" + array[array2[i]].intensity + ")";
        }
        Debug.Log(str);
        
        
        var array3 = new Particle[particleBuffer.Src.count];
        particleBuffer.Src.GetData(array3);
        var str2 = "";
        for (int i = 0; i < array3.Length; i++)
        {
            str2 += array3[i].vel + ",";
        }
        Debug.Log(str2);
        dispatcher.DispatchUpdate(particleBuffer,connectionBuffer,connectionIndexBuffer.Src,connectionBeginIndexBuffer,particleIndexPool,deltaTime);
        connectionDispatcher.DispatchUpdate(connectionIndexBuffer,particleBuffer,connectionBuffer,ref connectionBeginIndexBuffer,ref connectionIndexPool,deltaTime);

        var array4 = new uint[connectionIndexPool.count];
        connectionIndexPool.GetData(array4);
        var str3 = "";
        for (int i = 0; i < array4.Length; i++)
        {
            str3 += array4[i] + ",";
        }
        Debug.Log(str3);
        dispatcher.DispatchUpdate(particleBuffer,connectionBuffer,connectionIndexBuffer.Src,connectionBeginIndexBuffer,particleIndexPool,deltaTime);
        connectionDispatcher.DispatchUpdate(connectionIndexBuffer,particleBuffer,connectionBuffer,ref connectionBeginIndexBuffer,ref connectionIndexPool,deltaTime);

    }

    private void OnDestroy()
    {
        particleBuffer.Release();
        connectionBuffer.Release();
        connectionBeginIndexBuffer.Release();
        connectionIndexBuffer.Release();
        connectionIndexPool.Release();
        particleIndexPool.Release();
    }

    private void Render()
    {
        renderer.Render(particleBuffer,connectionBuffer);
    }

}
