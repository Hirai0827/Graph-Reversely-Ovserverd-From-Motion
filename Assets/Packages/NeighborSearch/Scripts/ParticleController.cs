using DefaultNamespace;
using UnityEngine;

public class ParticleController
{
    private ComputeShader computeShader;

    private int UpdateKid;

    public ParticleController(ComputeShader computeShader)
    {
        this.computeShader = computeShader;
        this.UpdateKid = this.computeShader.FindKernel("Update");
    }

    public int GetDispatchSize(int size)
    {
        if (size % 256 == 0)
        {
            return size / 256;
        }

        return size / 256 + 1;
    }

    public void Update(SwappableComputeBuffer<Particle> particleBuffer,ComputeBuffer indexBuffer,ComputeBuffer beginIndexBuffer,ComputeBuffer endIndexBuffer,int cellCountX,int cellCountY)
    {
        computeShader.SetBuffer(UpdateKid,"src",particleBuffer.Src);
        computeShader.SetBuffer(UpdateKid,"dest",particleBuffer.Dest);
        computeShader.SetBuffer(UpdateKid,"indexBuffer",indexBuffer);
        computeShader.SetBuffer(UpdateKid,"beginIndexBuffer",beginIndexBuffer);
        computeShader.SetBuffer(UpdateKid,"endIndexBuffer",endIndexBuffer);
        computeShader.SetInt("cellCountX",cellCountX);
        computeShader.SetInt("cellCountY",cellCountY);
        
        computeShader.Dispatch(UpdateKid,GetDispatchSize(particleBuffer.Count),1,1);
        
        particleBuffer.Swap();
    }
    
    
}