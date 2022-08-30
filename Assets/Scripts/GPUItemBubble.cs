using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class GPUItemBubble:GPUItem
    {
        private float lifeTime;
        private Vector2 center;
        private float radius;
        public GPUItemBubble(Vector2 center,float radius)
        {
            this.lifeTime = 20.0f;
            this.center = center;
            this.radius = radius;
        }

        public override bool IsUsing => this.lifeTime < 0.0f;
        public override int Size => 2;
        public override GPUInitialData GetInitialData()
        {
            var particles = new List<Particle>();
            var connections = new List<ParticleConnection>();
            int count = 16;

            for (int i = 0; i < count; i++)
            {
                var p = new Particle();
                p.pos = center + new Vector2(Mathf.Cos(6.28f * i / count), Mathf.Sin(6.28f * i / count)) * radius * (1.0f/0.75f);
                p.vel = Vector2.zero;
                p.time = 20.0f - Mathf.Pow(Random.Range(0.0f,1.0f),3) * 5.0f;
                p.isActive = 1;
                particles.Add(p);
            }

            for (int i = 0; i < count; i++)
            {
                var c = new ParticleConnection();
                c.intensity = 16.0f;
                c.indexA = (uint)i;
                c.indexB = (uint)(i + 1) % (uint)count;
                c.basePosition = particles[(int)c.indexB].pos * 0.75f - particles[(int)c.indexA].pos * 0.75f;
                connections.Add(c);
            }

            for (int i = 0; i < count; i++)
            {
                var c = new ParticleConnection();
                c.intensity = 16.0f;
                c.indexB = (uint)i;
                c.indexA = (uint)(i + 1) % (uint)count;
                c.basePosition = particles[(int)c.indexB].pos * 0.75f - particles[(int)c.indexA].pos * 0.75f;
                connections.Add(c);
            }

            return new GPUInitialData(particles.ToArray(),connections.ToArray());
        }

        public override void Update(float deltaTime)
        {
            this.lifeTime -= deltaTime;
        }
    }
}