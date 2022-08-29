
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DefaultNamespace;
using UnityEngine;

public abstract class GPUItem
{
    public abstract bool IsUsing { get; }
    public abstract int Size { get; }
    
    public int Offset { get; set; }

    public abstract GPUInitialData GetInitialData();

    public abstract void Update(float deltaTime);
}

public class GPUInitialData
{
    public Particle[] particles;
    public ParticleConnection[] connections;

    public GPUInitialData(Particle[] particles,ParticleConnection[] connections)
    {
        this.particles = particles;
        this.connections = connections;
    }
}

public class GPUResourceManager
{
    private List<GPUItem> items;

    public List<bool> availableAreaList;

    private int bufferSize;
    private int bufferUnitSize;

    public GPUResourceManager(int bufferSize,int bufferUnitSize)
    {
        this.bufferSize = bufferSize;
        this.bufferUnitSize = bufferUnitSize;
        this.items = new List<GPUItem>();
        var unitCount = bufferSize / bufferUnitSize;
        availableAreaList = new List<bool>(unitCount);
        for (int i = 0; i < unitCount; i++)
        {
            availableAreaList.Add(true);
        }
    }

    public int FindEmptySlot(int size)
    {
        for (int i = 0; i < availableAreaList.Count - size; i++)
        {
            bool isOK = true;
            for (int j = 0; j < size; j++)
            {
                if (!availableAreaList[i + j])
                {
                    isOK = false;
                }
            }

            if (isOK)
            {
                return i;
            }
        }

        return -1;
    }

    public void RegisterItem(GPUItem item,int offset,SwappableComputeBuffer<Particle> particleBuffer,SwappableComputeBuffer<ParticleConnection> connectionBuffer)
    {
        for (int i = 0; i < item.Size; i++)
        {
            if (!availableAreaList[i + offset])
            {
                throw new Exception("Not");
            }
            availableAreaList[i + offset] = false;
        }

        item.Offset = offset;
        items.Add(item);
        var data = item.GetInitialData();
        int particleSize = Marshal.SizeOf(typeof(Particle));
        int connectionSize = Marshal.SizeOf(typeof(ParticleConnection));
        var connectionData = data.connections.Select(x =>
        {
            x.indexA += (uint)(offset * bufferUnitSize);
            x.indexB += (uint)(offset * bufferUnitSize);
            return x;
        }).ToArray();
        particleBuffer.Src.SetData(data.particles,0,bufferUnitSize * offset,data.particles.Length);
        connectionBuffer.Src.SetData(connectionData,0,bufferUnitSize * offset, connectionData.Length);
        
    }

    public void Update()
    {
        CheckAvailability();
    }

    public void CheckAvailability()
    {
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (!item.IsUsing)
            {
                //Release
                for (int j = 0; j < item.Size; j++)
                {
                    availableAreaList[j + item.Offset] = true;
                }
                
            }
        }
    }
}