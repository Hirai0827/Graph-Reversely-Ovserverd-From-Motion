using UnityEngine;

namespace DefaultNamespace
{
    public class ComputePoolDispatcher
    {
        private ComputeShader computeShader;

        private const string InitKernelName = "Init";

        private const int dispatchSize = 256;

        private int initKernelId;
        

        public ComputePoolDispatcher(ComputeShader computeShader)
        {
            this.computeShader = computeShader;
            this.initKernelId = this.computeShader.FindKernel(InitKernelName);
        }

        private int GetDispatchCount(int bufferCount)
        {
            if (bufferCount % dispatchSize == 0)
            {
                return bufferCount / dispatchSize;
            }
            return bufferCount / dispatchSize + 1;
        }

        public void DispatchInit(ref ComputeBuffer particlePool,ref ComputeBuffer connectionPool)
        {
            this.computeShader.SetBuffer(initKernelId,"particlePool",particlePool);
            this.computeShader.SetBuffer(initKernelId,"connectionPool",connectionPool);
            this.computeShader.Dispatch(initKernelId,GetDispatchCount(particlePool.count),1,1);
        }
        
        
    }
}