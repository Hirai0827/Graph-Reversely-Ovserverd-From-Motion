Shader "Hidden/BodyPix/Visualizer"
{
    CGINCLUDE

    #include "UnityCG.cginc"
    #include "Packages/jp.keijiro.bodypix/Shader/Common.hlsl"

    StructuredBuffer<float4> _Keypoints;
    float _Aspect;

    static const uint bone_connections[12][2] =
    {
        { KEYPOINT_LEFT_HIP,        KEYPOINT_LEFT_SHOULDER  },
        { KEYPOINT_LEFT_ELBOW,      KEYPOINT_LEFT_SHOULDER  },
        { KEYPOINT_LEFT_ELBOW,      KEYPOINT_LEFT_WRIST     },
        { KEYPOINT_LEFT_HIP,        KEYPOINT_LEFT_KNEE      },
        { KEYPOINT_LEFT_KNEE,       KEYPOINT_LEFT_ANKLE     },

        { KEYPOINT_RIGHT_HIP,       KEYPOINT_RIGHT_SHOULDER },
        { KEYPOINT_RIGHT_ELBOW,     KEYPOINT_RIGHT_SHOULDER },
        { KEYPOINT_RIGHT_ELBOW,     KEYPOINT_RIGHT_WRIST    },
        { KEYPOINT_RIGHT_HIP,       KEYPOINT_RIGHT_KNEE     },
        { KEYPOINT_RIGHT_KNEE,      KEYPOINT_RIGHT_ANKLE    },

        { KEYPOINT_LEFT_SHOULDER,   KEYPOINT_RIGHT_SHOULDER },
        { KEYPOINT_LEFT_HIP,        KEYPOINT_RIGHT_HIP      }
    };

    void VertexKeypoints(uint vid : SV_VertexID,
                         uint iid : SV_InstanceID,
                         out float4 position : SV_Position,
                         out float4 color : COLOR)
    {
        float4 key = _Keypoints[iid];

        const float threshold = 0.5;
        float alpha = saturate((key.z - threshold) / (1 - threshold));

        float x = lerp(-1, 1, key.x) / _Aspect;
        float y = lerp(-1, 1, key.y);

        float vx = lerp(-1, 1, vid & 1);
        float vy = lerp(-1, 1, vid < 2 || vid == 5);

        vx *= 0.015 * _ScreenParams.y / _ScreenParams.x;
        vy *= 0.015;

        vx *= alpha;
        vy *= alpha;

        position = float4(x + vx, y + vy, 1, 1);
        color = float4(1, 1, 0, alpha);
    }

    float4 FragmentKeypoints(float4 position : SV_Position,
                             float4 color : COLOR) : SV_Target
    {
        return color;
    }

    void VertexBones(uint vid : SV_VertexID,
                     uint iid : SV_InstanceID,
                     out float4 position : SV_Position,
                     out float4 color : COLOR)
    {
        float4 key = _Keypoints[bone_connections[iid][vid]];

        float x = lerp(-1, 1, key.x) / _Aspect;
        float y = lerp(-1, 1, key.y);

        const float threshold = 0.5;
        bool mask = key.z > threshold;

        position = float4(x, y, 1, 1);
        color = float4(1, 1, 0, mask);
    }

    float4 FragmentBones(float4 position : SV_Position,
                         float4 color : COLOR) : SV_Target
    {
        clip(color.a - 1);
        return color;
    }

    ENDCG

    SubShader
    {
        Tags { "Queue"="Overlay+100" }
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            CGPROGRAM
            #pragma vertex VertexKeypoints
            #pragma fragment FragmentKeypoints
            ENDCG
        }

        Pass
        {
            ZTest Always ZWrite Off Cull Off
            CGPROGRAM
            #pragma vertex VertexBones
            #pragma fragment FragmentBones
            ENDCG
        }
    }
}