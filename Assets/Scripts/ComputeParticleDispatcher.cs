using UnityEngine;

namespace DefaultNamespace
{
    public class ComputeParticleDispatcher
    {
        private ComputeShader computeShader;

        private const string InitKernelName = "Init";
        private const string UpdateKernelName = "Update";
        
        private const string BufferSrcId = "src";
        private const string BufferDestId = "dest";
        private const string ConnectionBufferId = "connectionBuffer";
        private const string connectionIndexBufferId = "connectionIndexBuffer";
        private const string connectionBeginIndexBufferId = "connectionBeginIndexBuffer";
        private const string particleIndexPoolId = "particleIndexPool";
        private const string DeltaTimeId = "deltaTime";
        private const string MaxParticleId = "maxParticle";
        private const string MaxConnectionId = "maxConnection";

        private const int dispatchSize = 256;

        private int initKernelId;
        private int updateKernelId;
        

        public ComputeParticleDispatcher(ComputeShader computeShader)
        {
            this.computeShader = computeShader;
            this.initKernelId = this.computeShader.FindKernel(InitKernelName);
            this.updateKernelId = this.computeShader.FindKernel(UpdateKernelName);
        }

        private int GetDispatchCount(int bufferCount)
        {
            if (bufferCount % dispatchSize == 0)
            {
                return bufferCount / dispatchSize;
            }
            return bufferCount / dispatchSize + 1;
        }

        public void DispatchInit(SwappableComputeBuffer<Particle> buffer)
        {
            var dispatchCount = GetDispatchCount(buffer.Count);
            
            this.computeShader.SetBuffer(initKernelId,BufferSrcId,buffer.Src);
            this.computeShader.SetBuffer(initKernelId,BufferDestId,buffer.Dest);
            this.computeShader.Dispatch(initKernelId,dispatchCount,1,1);
            buffer.Swap();
        }

        public void DispatchUpdate(SwappableComputeBuffer<Particle> buffer,
            SwappableComputeBuffer<ParticleConnection> connectionBuffer,
            ComputeBuffer connectionIndexBuffer,
            ComputeBuffer connectionBeginIndexBuffer,
            ComputeBuffer particleIndexPool,
            float deltaTime)
        {
            var dispatchCount = GetDispatchCount(buffer.Count);
            
            this.computeShader.SetBuffer(updateKernelId,BufferSrcId,buffer.Src);
            this.computeShader.SetBuffer(updateKernelId,BufferDestId,buffer.Dest);
            this.computeShader.SetBuffer(updateKernelId,ConnectionBufferId,connectionBuffer.Src);
            this.computeShader.SetBuffer(updateKernelId,connectionIndexBufferId,connectionIndexBuffer);
            this.computeShader.SetBuffer(updateKernelId,connectionBeginIndexBufferId,connectionBeginIndexBuffer);
            this.computeShader.SetBuffer(updateKernelId,particleIndexPoolId,particleIndexPool);
            this.computeShader.SetFloat(DeltaTimeId,deltaTime);
            this.computeShader.SetInt(MaxConnectionId,connectionBuffer.Count);
            this.computeShader.SetInt(MaxParticleId,buffer.Count);
            this.computeShader.Dispatch(updateKernelId,dispatchCount,1,1);
            buffer.Swap();
        }
    }
}