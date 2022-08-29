using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class ComputeConnectionDispatcher
{
    private ComputeShader computeShader;
    private BitonicSort sorter;

    private const string InitKernelName = "Init";
    private const string UpdateKernelName = "Update";
    private const string ResetIndexKernelName = "ResetIndex";
    private const string ReindexKernelName = "ReIndex";
    private const string BufferSrcId = "src";
    private const string BufferDestId = "dest";
    private const string DeltaTimeId = "deltaTime";
    private const string ParticleBufferId = "particleBuffer";
    private const string IndexBufferId = "indexBuffer";
    private const string BeginIndexBufferId = "beginIndexBuffer";
    private const string EndIndexBufferId = "endIndexBuffer";
    private const string ConnectionIndexPoolId = "connectionIndexPool";
    

    private const int dispatchSize = 256;

    private int initKernelId;
    private int updateKernelId;
    private int resetIndexKernelId;
    private int reindexKernelId;
        

    public ComputeConnectionDispatcher(ComputeShader computeShader,ComputeShader sortShader)
    {
        this.computeShader = computeShader;
        this.sorter = new BitonicSort(sortShader);
        this.initKernelId = this.computeShader.FindKernel(InitKernelName);
        this.updateKernelId = this.computeShader.FindKernel(UpdateKernelName);
        this.resetIndexKernelId = this.computeShader.FindKernel(ResetIndexKernelName);
        this.reindexKernelId = this.computeShader.FindKernel(ReindexKernelName);
    }

    private int GetDispatchCount(int bufferCount)
    {
        if (bufferCount % dispatchSize == 0)
        {
            return bufferCount / dispatchSize;
        }
        return bufferCount / dispatchSize + 1;
    }

    public void DispatchInit(SwappableComputeBuffer<ParticleConnection> buffer,SwappableComputeBuffer<int> indexbuffer)
    {
        var dispatchCount = GetDispatchCount(buffer.Count);
            
        this.computeShader.SetBuffer(initKernelId,BufferSrcId,buffer.Src);
        this.computeShader.SetBuffer(initKernelId,BufferDestId,buffer.Dest);
        this.computeShader.SetBuffer(initKernelId,IndexBufferId,indexbuffer.Dest);
        this.computeShader.Dispatch(initKernelId,dispatchCount,1,1);
        indexbuffer.Swap();
        buffer.Swap();
    }

    public void DispatchUpdate(SwappableComputeBuffer<int> connectionIndexBuffer,
        SwappableComputeBuffer<Particle> particleBuffer,
        SwappableComputeBuffer<ParticleConnection> connectionBuffer,
        ref ComputeBuffer connectionIndexBeginBuffer,
        ref ComputeBuffer connectionEndIndexBuffer,
        float deltaTime)
    {
        var dispatchCount = GetDispatchCount(connectionBuffer.Count);
            
        this.computeShader.SetBuffer(updateKernelId,BufferSrcId,connectionBuffer.Src);
        this.computeShader.SetBuffer(updateKernelId,BufferDestId,connectionBuffer.Dest);
        this.computeShader.SetBuffer(updateKernelId,ParticleBufferId,particleBuffer.Src);
        this.computeShader.SetFloat(DeltaTimeId,deltaTime);
        this.computeShader.Dispatch(updateKernelId,dispatchCount,1,1);
        connectionBuffer.Swap();
        SortAndReIndex(connectionIndexBuffer,connectionBuffer,ref connectionIndexBeginBuffer,ref connectionEndIndexBuffer);
        
    }

    public void SortAndReIndex(SwappableComputeBuffer<int> connectionIndexBuffer,
        SwappableComputeBuffer<ParticleConnection> connectionBuffer,
        ref ComputeBuffer connectionBeginIndexBuffer,
        ref ComputeBuffer connectionEndIndexBuffer)
    {
        var dispatchCount = GetDispatchCount(connectionBuffer.Count);
        sorter.Sort(connectionIndexBuffer,connectionBuffer.Src);
        
        this.computeShader.SetBuffer(resetIndexKernelId,EndIndexBufferId,connectionEndIndexBuffer);
        this.computeShader.SetBuffer(resetIndexKernelId,BeginIndexBufferId,connectionBeginIndexBuffer);
        this.computeShader.Dispatch(resetIndexKernelId,GetDispatchCount(connectionBeginIndexBuffer.count),1,1);
        
        this.computeShader.SetBuffer(reindexKernelId,IndexBufferId,connectionIndexBuffer.Src);
        this.computeShader.SetBuffer(reindexKernelId,BufferSrcId,connectionBuffer.Src);
        this.computeShader.SetBuffer(reindexKernelId,BeginIndexBufferId,connectionBeginIndexBuffer);
        this.computeShader.SetBuffer(reindexKernelId,EndIndexBufferId,connectionEndIndexBuffer);
        this.computeShader.SetInt("maxConnection",connectionBuffer.Count);
        this.computeShader.Dispatch(reindexKernelId,dispatchCount,1,1);
        
    }
}
