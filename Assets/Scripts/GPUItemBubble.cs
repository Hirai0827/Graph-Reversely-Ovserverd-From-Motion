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
            this.lifeTime = 10.0f;
            this.center = center;
            this.radius = radius;
        }

        public override bool IsUsing => this.lifeTime < 0.0f;
        public override int Size => 1;
        public override GPUInitialData GetInitialData()
        {
            var particles = new List<Particle>();
            var connections = new List<ParticleConnection>();

            for (int i = 0; i < 8; i++)
            {
                var p = new Particle();
                p.pos = center + new Vector2(Mathf.Cos(6.28f * i / 8.0f), Mathf.Sin(6.28f * i / 8.0f)) * radius;
                p.vel = center + new Vector2(Mathf.Cos(6.28f * i / 8.0f), Mathf.Sin(6.28f * i / 8.0f)) * radius;
                p.time = 10.0f;
                p.isActive = 1;
                particles.Add(p);
            }

            for (int i = 0; i < 8; i++)
            {
                var c = new ParticleConnection();
                c.intensity = 64.0f;
                c.indexA = i;
                c.indexB = (i + 1) % 8;
                c.basePosition = particles[c.indexB].pos - particles[c.indexA].pos;
                connections.Add(c);
            }

            for (int i = 0; i < 8; i++)
            {
                var c = new ParticleConnection();
                c.intensity = 0.1f;
                c.indexB = i;
                c.indexA = (i + 1) % 8;
                c.basePosition = particles[c.indexB].pos - particles[c.indexA].pos;
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