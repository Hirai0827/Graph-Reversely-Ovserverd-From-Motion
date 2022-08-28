using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class BitonicSort
{
    [SerializeField] private ComputeShader bitonicShader;
    private int swapKid;
    public BitonicSort(ComputeShader bitonicShader)
    {
        this.bitonicShader = bitonicShader;
    }

    public void Init()
    {
        swapKid = bitonicShader.FindKernel("Swap");
    }

    public void Sort(SwappableComputeBuffer<int> buffer,ComputeBuffer compareTargetBuffer)
    {
        var isPowOfTwo = IsPowOfTwo(buffer.Count, out var iterCount);
        if (!isPowOfTwo)
        {
            throw new ArgumentException("the number of buffer elements must be pow of 2.");
        }

        for (int x = 0; x < iterCount; x++)
        {
            for (int y = 0; y <= x; y++)
            {
                Swap(buffer,compareTargetBuffer,x,y);
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

    private void Swap(SwappableComputeBuffer<int> buffer,ComputeBuffer compareTargetBuffer,int mainIter,int subIter)
    {
        bitonicShader.SetBuffer(swapKid,"src",buffer.Src);
        bitonicShader.SetBuffer(swapKid,"dest",buffer.Dest);
        bitonicShader.SetBuffer(swapKid,"connectionBuffer",compareTargetBuffer);
        bitonicShader.SetInt("mainIter",mainIter);
        bitonicShader.SetInt("subIter",subIter);
        bitonicShader.SetInt("count",buffer.Count);
        bitonicShader.Dispatch(swapKid,buffer.Count / 256 + 1,1,1);
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
