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
    public uint indexA;
    public uint indexB;
    public Vector2 basePosition;
    public float intensity;
    public Color color;
}


public class ParticleManager : MonoBehaviour
{
    [SerializeField] private ParticleRenderer renderer;
    [SerializeField] private ComputeShader computeParticleShader;
    [SerializeField] private ComputeShader computeConnectionShader;
    [SerializeField] private ComputeShader particleSortShader;
    [FormerlySerializedAs("sortShader")] [SerializeField] private ComputeShader edgeSortShader; 
    [SerializeField] private int particleLimit;
    [SerializeField] private int connectionLimit;
    [SerializeField] private Vector2 minCorner;
    [SerializeField] private Vector2 maxCorner;
    [SerializeField] private int cellCountX;
    [SerializeField] private int cellCountY;


    private GPUResourceManager manager;

    private SwappableComputeBuffer<ParticleConnection> connectionBuffer;
    private SwappableComputeBuffer<Particle> particleBuffer;
    
    private SwappableComputeBuffer<int> connectionIndexBuffer;
    private ComputeBuffer connectionBeginIndexBuffer;
    private ComputeBuffer connectionEndIndexBuffer;
    
    private SwappableComputeBuffer<ParticleIndex> particleIndexBuffer;
    private ComputeBuffer particleBeginIndexBuffer;
    private ComputeBuffer particleEndIndexBuffer;
    
    private ComputeConnectionDispatcher connectionDispatcher;
    private ComputeParticleDispatcher particleDispatcher;
    private ParticleSorter particleSorter;

    private void InitInternal()
    {
        manager = new GPUResourceManager(particleLimit,16);
        particleBuffer = new SwappableComputeBuffer<Particle>(particleLimit);
        connectionBuffer = new SwappableComputeBuffer<ParticleConnection>(connectionLimit);
        
        connectionIndexBuffer = new SwappableComputeBuffer<int>(connectionLimit);
        connectionBeginIndexBuffer = new ComputeBuffer(particleLimit, Marshal.SizeOf(typeof(int)));
        connectionEndIndexBuffer = new ComputeBuffer(particleLimit, Marshal.SizeOf(typeof(int)));

        particleIndexBuffer = new SwappableComputeBuffer<ParticleIndex>(connectionLimit);
        particleBeginIndexBuffer = new ComputeBuffer(cellCountX * cellCountY, Marshal.SizeOf(typeof(uint)));
        particleEndIndexBuffer = new ComputeBuffer(cellCountX * cellCountY, Marshal.SizeOf(typeof(uint)));
        
        particleDispatcher = new ComputeParticleDispatcher(computeParticleShader);
        connectionDispatcher = new ComputeConnectionDispatcher(computeConnectionShader,edgeSortShader);

        particleSorter = new ParticleSorter(particleSortShader);
        
        //Initialize
        connectionDispatcher.DispatchInit(connectionBuffer,connectionIndexBuffer);
        particleDispatcher.DispatchInit(particleBuffer);
        renderer.Init();
    }

    private void Awake()
    {
        InitInternal();
    }

    private void FixedUpdate()
    {
        particleSorter.Preprocess(particleBuffer.Src,particleIndexBuffer,new Vector2(-5f,-5f),new Vector2(5f,5f),cellCountX,cellCountY);
        particleSorter.Sort(particleIndexBuffer);
        particleSorter.GenerateIndex(particleIndexBuffer.Src,particleBeginIndexBuffer,particleEndIndexBuffer);
        UpdateInternal(0.008f);
        UpdateInternal(0.008f);
        UpdateInternal(0.008f);
        UpdateInternal(0.008f);
        //LogInfo();
        //LoggingIndexBuffer();
    }

    private void Update()
    {
        Render();
        if (Input.GetKeyDown(KeyCode.A))
        {
            var bubble = new GPUItemFlower(new Vector2(Random.Range(-9.0f,9.0f),Random.Range(-4.0f,4.0f)),Random.Range(0.125f,0.5f));
            var offset = manager.FindEmptySlot(1);
            Debug.Log(offset);
            if (offset == -1)
            {
                return;
            }
            manager.RegisterItem(bubble,offset,particleBuffer,connectionBuffer);
        }
    }


    private void UpdateInternal(float deltaTime)
    {

        particleDispatcher.DispatchUpdate(
            particleBuffer,
            connectionBuffer,
            connectionIndexBuffer.Src,
            connectionBeginIndexBuffer,
            connectionEndIndexBuffer,
            particleIndexBuffer.Src,
            particleBeginIndexBuffer,
            particleEndIndexBuffer,
            deltaTime,
            minCorner,
            maxCorner,cellCountX,cellCountY);
        connectionDispatcher.DispatchUpdate(connectionIndexBuffer,particleBuffer,connectionBuffer,ref connectionBeginIndexBuffer,ref connectionEndIndexBuffer,deltaTime);
    }

    public void LogInfo()
    {
        var array = new ParticleConnection[connectionBuffer.Src.count];
        connectionBuffer.Src.GetData(array);
        
        var str = "";
        for (int i = 0; i < array.Length; i++)
        {
            str += "(" + array[i].indexA + "," + array[i].indexB + ")";
            //str += "(" + array[array2[i]].intensity + ")";
        }
        Debug.Log(str);
        
        //Debug.Log(connectionIndexBuffer.Src.count);
        var array2 = new int[connectionIndexBuffer.Src.count];
        connectionIndexBuffer.Src.GetData(array2);
        str = "";
        for (int i = 0; i < array2.Length; i++)
        {
            str += "[" + array2[i] + ":(" + array[array2[i]].indexA + "," + array[array2[i]].indexB + ")]";
            //str += "(" + array[array2[i]].intensity + ")";
        }
        Debug.Log(str);
        
        
        var array3 = new int[connectionBeginIndexBuffer.count];
        connectionBeginIndexBuffer.GetData(array3);
        var array4 = new int[connectionEndIndexBuffer.count];
        connectionEndIndexBuffer.GetData(array4);
        var str2 = "";
        for (int i = 0; i < array3.Length; i++)
        {
            str2 += "(" + array3[i] + "," + array4[i] + ")";
        }
        Debug.Log(str2);
    }
    
    private void LoggingIndexBuffer()
    {
        var beginIndexArray = new uint[particleBeginIndexBuffer.count];
        var endIndexArray = new uint[particleEndIndexBuffer.count];
        particleBeginIndexBuffer.GetData(beginIndexArray);
        particleEndIndexBuffer.GetData(endIndexArray);
        var str = "";
        for (int i = 0; i < beginIndexArray.Length; i++)
        {
            str += "(" +beginIndexArray[i]+ "," +endIndexArray[i]+ "),";
        }
        Debug.Log(str);
        str = "";
        var indexArray = new ParticleIndex[particleIndexBuffer.Count];
        particleIndexBuffer.Src.GetData(indexArray);
        for (int i = 0; i < indexArray.Length; i++)
        {
            str += "(" + indexArray[i].index +","+ indexArray[i].cellIndex + ")";
        }
        Debug.Log(str);
    }


    private void OnDestroy()
    {
        particleBuffer.Release();
        connectionBuffer.Release();
        connectionIndexBuffer.Release();
        connectionBeginIndexBuffer.Release();
        connectionEndIndexBuffer.Release();
        particleIndexBuffer.Release();
        particleBeginIndexBuffer.Release();
        particleEndIndexBuffer.Release();

    }

    private void Render()
    {
        renderer.Render(particleBuffer,connectionBuffer);
    }

}
