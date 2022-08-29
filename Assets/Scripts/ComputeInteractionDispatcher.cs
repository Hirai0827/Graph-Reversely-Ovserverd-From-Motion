using UnityEngine;

namespace DefaultNamespace
{

    public struct AppendInfo
    {
        public Particle particle;
    }
    public class ComputeInteractionDispatcher
    {
        private ComputeShader computeShader;

        private const int dispatchSize = 256;

        private int DrawCircleId;


        public ComputeInteractionDispatcher(ComputeShader computeShader)
        {
            this.computeShader = computeShader;
            this.DrawCircleId = this.computeShader.FindKernel("DrawCircle");
        }

        private int GetDispatchCount(int bufferCount)
        {
            if (bufferCount % dispatchSize == 0)
            {
                return bufferCount / dispatchSize;
            }
            return bufferCount / dispatchSize + 1;
        }

        public void DispatchDrawCircle(SwappableComputeBuffer<Particle> particleBuffer,SwappableComputeBuffer<ParticleConnection> connectionBuffer)
        {
            this.computeShader.SetBuffer(DrawCircleId,"particleSrc",particleBuffer.Src);
            this.computeShader.SetBuffer(DrawCircleId,"particleDest",particleBuffer.Dest);
            this.computeShader.SetBuffer(DrawCircleId,"connectionSrc",connectionBuffer.Src);
            this.computeShader.SetBuffer(DrawCircleId,"connectionDest",connectionBuffer.Dest);
            this.computeShader.SetVector("center",new Vector4(Random.Range(-4.0f,4.0f),Random.Range(-4.0f,4.0f),0.0f,0.0f));
            this.computeShader.SetFloat("radius",0.5f);
            this.computeShader.Dispatch(DrawCircleId,1,1,1);
            particleBuffer.Swap();
            connectionBuffer.Swap();
        }

    }
}