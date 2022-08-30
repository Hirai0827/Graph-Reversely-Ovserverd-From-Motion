using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class GPUItemFlower:GPUItem
    {
        private float lifeTime;
        private Vector2 center;
        private float radius;
        public GPUItemFlower(Vector2 center,float radius)
        {
            this.lifeTime = 20.0f;
            this.center = center;
            this.radius = radius;
        }

        public override bool IsUsing => this.lifeTime < 0.0f;
        public override int Size => 4;
        public override GPUInitialData GetInitialData()
        {
            var particles = new List<Particle>();
            var connections = new List<ParticleConnection>();
            int petalCount = 6;
            int petalVertexCount = 3;
            var centerP = new Particle();
            centerP.pos = center;
            centerP.vel = Vector2.zero * 0.25f;
            centerP.time = 10.0f;
            centerP.isActive = 1;
            particles.Add(centerP);
            float phi = Random.Range(0.0f, 1.0f);

            for (int i = 0; i < petalCount; i++)
            {
                float theta = 6.28f * i / petalCount + phi;
                Vector2 dir = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
                for (int j = 0; j < petalVertexCount; j++)
                {
                    var p = new Particle();
                    p.pos = center + dir * (radius * ((float)j / petalVertexCount));
                    p.vel = Vector2.zero;
                    p.time = 20.0f - Random.Range(0.0f,3.0f);
                    p.isActive = 1;
                    particles.Add(p);
                }
            }

            for (int i = 0; i < petalCount; i++)
            {
                int baseIndex = i * petalVertexCount + 1;
                {
                    var c = new ParticleConnection();
                    c.intensity = 4.0f;
                    c.indexA = 0;
                    c.indexB = (uint)baseIndex;
                    c.basePosition = particles[(int)c.indexB].pos - particles[(int)c.indexA].pos;
                    connections.Add(c);
                }
                
                for (int j = 0; j < petalVertexCount-1; j++)
                {
                    var c = new ParticleConnection();
                    c.intensity = 4.0f;
                    c.indexA = (uint)(baseIndex + j);
                    c.indexB = (uint)(baseIndex + j + 1);
                    c.basePosition = particles[(int)c.indexB].pos - particles[(int)c.indexA].pos;
                    connections.Add(c);
                }
            }
            
            for (int i = 0; i < petalCount; i++)
            {
                int baseIndex = i * petalVertexCount + 1;
                {
                    var c = new ParticleConnection();
                    c.intensity = 4.0f;
                    c.indexB = 0;
                    c.indexA = (uint)baseIndex;
                    c.basePosition = particles[(int)c.indexB].pos - particles[(int)c.indexA].pos;
                    connections.Add(c);
                }
                
                for (int j = 0; j < petalVertexCount-1; j++)
                {
                    var c = new ParticleConnection();
                    c.intensity = 4.0f;
                    c.indexB = (uint)(baseIndex + j);
                    c.indexA = (uint)(baseIndex + j + 1);
                    c.basePosition = particles[(int)c.indexB].pos - particles[(int)c.indexA].pos;
                    connections.Add(c);
                }
            }

            return new GPUInitialData(particles.ToArray(),connections.ToArray());
        }

        public override void Update(float deltaTime)
        {
            this.lifeTime -= deltaTime;
        }
    }
}