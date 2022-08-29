using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DefaultNamespace;
using UnityEngine;

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
}


public class ParticleManager : MonoBehaviour
{
    [SerializeField] private ParticleRenderer renderer;
    [SerializeField] private ComputeShader computeParticleShader;
    [SerializeField] private ComputeShader computeConnectionShader;
    [SerializeField] private ComputeShader sortShader; 
    [SerializeField] private int particleLimit;
    [SerializeField] private int connectionLimit;


    private GPUResourceManager manager;

    private SwappableComputeBuffer<int> connectionIndexBuffer;
    private SwappableComputeBuffer<Particle> particleBuffer;
    private SwappableComputeBuffer<ParticleConnection> connectionBuffer;
    private ComputeBuffer connectionBeginIndexBuffer;
    private ComputeBuffer connectionEndIndexBuffer;
    
    private ComputeParticleDispatcher particleDispatcher;
    private ComputeConnectionDispatcher connectionDispatcher;

    private void InitInternal()
    {
        manager = new GPUResourceManager(particleLimit,16);
        particleBuffer = new SwappableComputeBuffer<Particle>(particleLimit);
        connectionIndexBuffer = new SwappableComputeBuffer<int>(connectionLimit);
        connectionBuffer = new SwappableComputeBuffer<ParticleConnection>(connectionLimit);

        connectionBeginIndexBuffer = new ComputeBuffer(particleLimit, Marshal.SizeOf(typeof(int)));
        connectionEndIndexBuffer = new ComputeBuffer(particleLimit, Marshal.SizeOf(typeof(int)));
        
        particleDispatcher = new ComputeParticleDispatcher(computeParticleShader);
        connectionDispatcher = new ComputeConnectionDispatcher(computeConnectionShader,sortShader);
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
        UpdateInternal(0.016f);
        //LogInfo();
    }

    private void Update()
    {
        Render();
        if (Input.GetKeyDown(KeyCode.A))
        {
            var bubble = new GPUItemBubble(new Vector2(Random.Range(-4.0f,4.0f),Random.Range(-4.0f,4.0f)),0.25f);
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

        particleDispatcher.DispatchUpdate(particleBuffer,connectionBuffer,connectionIndexBuffer.Src,connectionBeginIndexBuffer,connectionEndIndexBuffer,deltaTime);
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

    private void OnDestroy()
    {
        particleBuffer.Release();
        connectionBuffer.Release();
        connectionBeginIndexBuffer.Release();
        connectionIndexBuffer.Release();
    }

    private void Render()
    {
        renderer.Render(particleBuffer,connectionBuffer);
    }

}
