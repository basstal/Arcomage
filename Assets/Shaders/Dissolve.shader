Shader "Particles/Dissolve Surface"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _DissolveTexture("Dissolve Texture", 2D) = "white" {}
        _EdgeColour1 ("Edge Colour Inner", Color) = (1.0, 1.0, 1.0, 1.0)
        _EdgeColour2 ("Edge Colour Outer", Color) = (1.0, 1.0, 1.0, 1.0)
        _Edges ("Edge width", Range( 0.0, 1.0)) = 0.1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull off
            Lighting off
            ZWrite off
            Fog {Mode off}
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float agePercent : float;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _DissolveTexture;
            float4 _EdgeColour1;
            float4 _EdgeColour2;
            float _Edges;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv.xy, _MainTex);
                o.agePercent = v.uv.z;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float cutout = tex2D(_DissolveTexture, i.uv).r;
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                float agePercent = i.agePercent;
                
                if (cutout < agePercent)
                {
                    discard;
                }
                   
                if(cutout < col.a && cutout < agePercent + _Edges)
                {
                    col = lerp(_EdgeColour1, _EdgeColour2, (cutout - agePercent)/_Edges);
                }
                
                return col;
            }
            ENDCG
        }
    }
}
