using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SampleRenderer
{
    [SerializeField]
    private Shader shader;
    [SerializeField]
    private Mesh mesh;
    private Material material;

    public void Init()
    {
        material = new Material(shader);
    }
    public void Render(ComputeBuffer particleBuffer,ComputeBuffer indexBuffer)
    {
        material.SetBuffer("particleBuffer",particleBuffer);
        material.SetBuffer("indexBuffer",indexBuffer);
        Graphics.DrawMeshInstancedProcedural(mesh,0,material,new Bounds(Vector3.zero,Vector3.one * 100.0f),particleBuffer.count);
    }
}
