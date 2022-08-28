Shader "Unlit/RenderConnection"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        CGINCLUDE
        struct Particle
        {
            float2 pos;
            float2 vel;
            float lifeTime;
            int isActive;
        };


        struct ParticleConnection
        {
            uint indexA;
            uint indexB;
            float2 basePosition;
            float intensity;
        };

        StructuredBuffer<Particle> buffer;
        StructuredBuffer<ParticleConnection> connectionBuffer;
        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint instanceId : SV_InstanceID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                ParticleConnection c = connectionBuffer[v.instanceId];
                Particle pA = buffer[c.indexA];
                Particle pB = buffer[c.indexB];
                float2 vecA = (pB.pos - pA.pos);
                float2 vecB = normalize(float2(-vecA.y,vecA.x)) * 0.1;
                float2 pos = (vecA * v.vertex.x + vecB * v.vertex.y) + (pA.pos + pB.pos) * 0.5;
                o.vertex = UnityObjectToClipPos(float4(pos,0.0,1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}