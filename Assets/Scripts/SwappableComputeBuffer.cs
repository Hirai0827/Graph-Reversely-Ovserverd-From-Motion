using System.Runtime.InteropServices;
using UnityEngine;

namespace DefaultNamespace
{
    public class SwappableComputeBuffer<T>
    {
        private ComputeBuffer src;
        private ComputeBuffer dest;
        public ComputeBuffer Src => src;
        public ComputeBuffer Dest => dest;

        public int Count => src.count;

        public SwappableComputeBuffer(int count)
        {
            src = new ComputeBuffer(count, Marshal.SizeOf(typeof(T)));
            dest = new ComputeBuffer(count, Marshal.SizeOf(typeof(T)));
        }

        public void Release()
        {
            src.Release();
            src = null;
            dest.Release();
            dest = null;
        }
            
        public void Swap()
        {
            (src, dest) = (dest, src);
        }
        
        
    }
}