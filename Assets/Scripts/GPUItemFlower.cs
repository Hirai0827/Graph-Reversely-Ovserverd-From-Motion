using System.Collections.Generic;
using System.Linq;
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
        public override int Size => 8;
        public override GPUInitialData GetInitialData()
        {
            var particles = new List<Particle>();
            var connections = new List<ParticleConnection>();
            int petalCount = 6;
            int petalVertexCount = 6;
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
                Vector2 norm = new Vector2(dir.y, -dir.x);
                for (int j = 0; j < petalVertexCount/2; j++)
                {
                    var p = new Particle();
                    p.pos = center + dir * (radius * ((float)(j + 1) / (petalVertexCount/2))) + radius * 0.1f * norm;
                    p.vel = Vector2.zero;
                    p.time = 20.0f - Random.Range(0.0f,3.0f);
                    p.isActive = 1;
                    particles.Add(p);
                }
                for (int j = 0; j < petalVertexCount/2; j++)
                {
                    var p = new Particle();
                    p.pos = center + dir * (radius * ((float)(petalVertexCount/2 - j) / (petalVertexCount/2))) - radius * 0.1f * norm;;
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
                    c.intensity = 16.0f;
                    c.indexA = 0;
                    c.indexB = (uint)baseIndex;
                    c.basePosition = particles[(int)c.indexB].pos - particles[(int)c.indexA].pos;
                    connections.Add(c);
                }
                
                for (int j = 0; j < petalVertexCount; j++)
                {
                    var c = new ParticleConnection();
                    c.intensity = 4.0f;
                    c.indexA = (uint)(baseIndex + j);
                    if (j != petalVertexCount - 1)
                    {
                        c.indexB = (uint)(baseIndex + j + 1);
                    }
                    else
                    {
                        c.indexB = (uint)(baseIndex);
                    }
                    c.basePosition = particles[(int)c.indexB].pos - particles[(int)c.indexA].pos;
                    connections.Add(c);
                }
            }
            
            for (int i = 0; i < petalCount; i++)
            {
                int baseIndex = i * petalVertexCount + 1;
                {
                    var c = new ParticleConnection();
                    c.intensity = 16.0f;
                    c.indexB = 0;
                    c.indexA = (uint)baseIndex;
                    c.basePosition = particles[(int)c.indexB].pos - particles[(int)c.indexA].pos;
                    connections.Add(c);
                }
                
                for (int j = 0; j < petalVertexCount; j++)
                {
                    var c = new ParticleConnection();
                    c.intensity = 4.0f;
                    c.indexB = (uint)(baseIndex + j);
                    if (j != petalVertexCount - 1)
                    {
                        c.indexA = (uint)(baseIndex + j + 1);
                    }
                    else
                    {
                        c.indexA = (uint)(baseIndex);
                    }
                    c.basePosition = particles[(int)c.indexB].pos - particles[(int)c.indexA].pos;
                    connections.Add(c);
                }
            }

            particles = particles.Select(x =>
            {
                x.pos = (x.pos - center) * 0.75f + center;
                return x;
            }).ToList();
            return new GPUInitialData(particles.ToArray(),connections.ToArray());
        }

        public override void Update(float deltaTime)
        {
            this.lifeTime -= deltaTime;
        }
    }
}