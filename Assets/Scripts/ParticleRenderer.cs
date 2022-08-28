using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class ParticleRenderer
{
    [SerializeField] private Shader renderShader;
    [SerializeField] private Shader renderConnectionShader;

    [SerializeField] private Mesh particleMesh;
    [SerializeField] private Mesh connectionMesh;
    
    private Material renderMaterial;
    private Material renderConnectionMaterial;
    
    public void Init()
    {
        renderMaterial = new Material(renderShader);
        renderConnectionMaterial = new Material(renderConnectionShader);
    }

    public void Render(SwappableComputeBuffer<Particle> buffer,SwappableComputeBuffer<ParticleConnection> connectionBuffer)
    {
        renderConnectionMaterial.SetBuffer("buffer",buffer.Src);
        renderConnectionMaterial.SetBuffer("connectionBuffer",connectionBuffer.Src);
        renderMaterial.SetBuffer("buffer",buffer.Src);
        Graphics.DrawMeshInstancedProcedural(particleMesh,0,renderMaterial,new Bounds(Vector3.zero, Vector3.one * 128f),buffer.Count);;
        Graphics.DrawMeshInstancedProcedural(connectionMesh,0,renderConnectionMaterial,new Bounds(Vector3.zero, Vector3.one * 128f),connectionBuffer.Count);
    }
}
