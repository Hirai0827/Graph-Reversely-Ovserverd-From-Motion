using System;
using System.Runtime.InteropServices;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;


public class SampleDistributor : MonoBehaviour
{
    
    [SerializeField] private ComputeShader sortShader;
    [SerializeField] private ComputeShader particleShader;
    [SerializeField] private SampleRenderer renderer;

    [SerializeField] private int count;
    [SerializeField] private int cellCountX;
    [SerializeField] private int cellCountY;

    private SwappableComputeBuffer<Particle> particleBuffer;
    private SwappableComputeBuffer<ParticleIndex> indexBuffer;
    private ComputeBuffer beginIndexBuffer;
    private ComputeBuffer endIndexBuffer;
    private ParticleSorter sorter;
    private ParticleController controller;


    private void Init()
    {
        particleBuffer = new SwappableComputeBuffer<Particle>(count);
        indexBuffer = new SwappableComputeBuffer<ParticleIndex>(count);
        beginIndexBuffer = new ComputeBuffer(cellCountX * cellCountY, Marshal.SizeOf(typeof(uint)));
        endIndexBuffer = new ComputeBuffer(cellCountX * cellCountY, Marshal.SizeOf(typeof(uint)));
        sorter = new ParticleSorter(sortShader);
        controller = new ParticleController(particleShader);

        var particles = new Particle[count];
        for (int i = 0; i < count; i++)
        {
            var P = new Particle();
            P.pos = new Vector2(Random.Range(-5f, 5f),Random.Range(-5f, 5f));
            P.vel = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            P.time = Random.Range(0f, 20f);
            particles[i] = P;
        }

        particleBuffer.Src.SetData(particles);
        renderer.Init();
    }

    private void Awake()
    {
        Init();
    }

    private void FixedUpdate()
    {
        sorter.Preprocess(particleBuffer.Src,indexBuffer,new Vector2(-5f,-5f),new Vector2(5f,5f),cellCountX,cellCountY);
        sorter.Sort(indexBuffer);
        sorter.GenerateIndex(indexBuffer.Src,beginIndexBuffer,endIndexBuffer);
        controller.Update(particleBuffer,indexBuffer.Src,beginIndexBuffer,endIndexBuffer,cellCountX,cellCountY);
        //LoggingIndexBuffer();
    }

    private void OnDestroy()
    {
        indexBuffer.Release();
        particleBuffer.Release();
        beginIndexBuffer.Release();
        endIndexBuffer.Release();
    }

    private void LoggingIndexBuffer()
    {
        Debug.Log(beginIndexBuffer.count);
        var beginIndexArray = new uint[beginIndexBuffer.count];
        var endIndexArray = new uint[endIndexBuffer.count];
        beginIndexBuffer.GetData(beginIndexArray);
        endIndexBuffer.GetData(endIndexArray);
        var str = "";
        for (int i = 0; i < beginIndexArray.Length; i++)
        {
            str += "(" +beginIndexArray[i]+ "," +endIndexArray[i]+ "),";
        }
        Debug.Log(str);
        str = "";
        var indexArray = new ParticleIndex[indexBuffer.Count];
        indexBuffer.Src.GetData(indexArray);
        for (int i = 0; i < indexArray.Length; i++)
        {
            str += "(" + indexArray[i].index +","+ indexArray[i].cellIndex + ")";
        }
        Debug.Log(str);
    }


    private void Update()
    {
        renderer.Render(particleBuffer.Src,indexBuffer.Src);
    }
}
