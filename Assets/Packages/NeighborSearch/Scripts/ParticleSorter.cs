using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;


public struct ParticleIndex
{
    public uint index;
    public uint cellIndex;
}

public class ParticleSorter
{
    private ComputeShader particleSortShader;
    private int swapKid;
    private int preprocessKid;
    private int resetIndexKid;
    private int reindexKid;

    public int GetDispatchSize(int bufferSize)
    {
        if (bufferSize % 256 == 0)
        {
            return bufferSize / 256;
        }

        return bufferSize / 256 + 1;
    }
    public ParticleSorter(ComputeShader particleSortShader)
    {
        this.particleSortShader = particleSortShader;
        swapKid = this.particleSortShader.FindKernel("Swap");
        preprocessKid = this.particleSortShader.FindKernel("Preprocess");
        resetIndexKid = this.particleSortShader.FindKernel("ResetIndex");
        reindexKid = this.particleSortShader.FindKernel("Reindex");
    }

    public void Preprocess(ComputeBuffer particleBuffer,
        SwappableComputeBuffer<ParticleIndex> indexBuffer,
        Vector2 minCorner,
        Vector2 maxCorner,
        int cellCountX,
        int cellCountY)
    {
        particleSortShader.SetBuffer(preprocessKid,"particleBuffer",particleBuffer);
        particleSortShader.SetBuffer(preprocessKid,"indexBuffer",indexBuffer.Dest);
        particleSortShader.SetVector("minCorner",minCorner);
        particleSortShader.SetVector("maxCorner",maxCorner);
        particleSortShader.SetInt("cellCountX",cellCountX);
        particleSortShader.SetInt("cellCountY",cellCountY);
        particleSortShader.Dispatch(preprocessKid,GetDispatchSize(indexBuffer.Count),1,1);
        indexBuffer.Swap();
    }

    public void GenerateIndex(ComputeBuffer indexBuffer,ComputeBuffer beginIndexBuffer,ComputeBuffer endIndexBuffer)
    {
        int cellCount = beginIndexBuffer.count;
        particleSortShader.SetInt("cellCount",cellCount);
        particleSortShader.SetBuffer(resetIndexKid,"beginIndexBuffer",beginIndexBuffer);
        particleSortShader.SetBuffer(resetIndexKid,"endIndexBuffer",endIndexBuffer);
        particleSortShader.Dispatch(resetIndexKid,GetDispatchSize(cellCount),1,1);

        particleSortShader.SetBuffer(reindexKid,"indexBuffer",indexBuffer);
        particleSortShader.SetBuffer(reindexKid, "beginIndexBuffer", beginIndexBuffer);
        particleSortShader.SetBuffer(reindexKid,"endIndexBuffer", endIndexBuffer);
        particleSortShader.Dispatch(reindexKid,GetDispatchSize(indexBuffer.count),1,1);
    }

    public void Sort(SwappableComputeBuffer<ParticleIndex> indexBuffer)
    {
        var isPowOfTwo = IsPowOfTwo(indexBuffer.Count, out var iterCount);
        if (!isPowOfTwo)
        {
            throw new ArgumentException("the number of buffer elements must be pow of 2.");
        }

        for (int x = 0; x < iterCount; x++)
        {
            for (int y = 0; y <= x; y++)
            {
                Swap(indexBuffer,x,y);
                // var array = new float[buffer.Count];
                // buffer.Src.GetData(array);
                // var str = "";
                // for (int i = 0; i < array.Length; i++)
                // {
                //     str += array[i] + ",";
                // }

            }
        }
    }

    private void Swap(SwappableComputeBuffer<ParticleIndex> buffer,int mainIter,int subIter)
    {
        particleSortShader.SetBuffer(swapKid,"src",buffer.Src);
        particleSortShader.SetBuffer(swapKid,"dest",buffer.Dest);
        particleSortShader.SetInt("mainIter",mainIter);
        particleSortShader.SetInt("subIter",subIter);
        particleSortShader.SetInt("count",buffer.Count);
        particleSortShader.Dispatch(swapKid,buffer.Count / 256 + 1,1,1);
        buffer.Swap();
    }

    private bool IsPowOfTwo(int x,out int a)
    {
        a = 0;
        while (x >= 2)
        {
            if (x % 2 != 0)
            {
                return false;
            }
            a++;
            x /= 2;
        }

        return true;
    }
}
