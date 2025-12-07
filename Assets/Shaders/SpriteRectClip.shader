Shader "Sprites/RectClip"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        _ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
        _ClipRectEnabled ("Clip Rect Enabled", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            fixed4 _Color;
            fixed4 _RendererColor;
            float4 _Flip;
            float4 _ClipRect;
            float _ClipRectEnabled;
            float _EnableExternalAlpha;

            float4 UnityFlipSprite(float4 inPos, float4 flip)
            {
                return float4(inPos.xy * flip.xy, inPos.z, inPos.w);
            }

            fixed4 SampleSpriteTexture(float2 uv)
            {
                fixed4 color = tex2D(_MainTex, uv);

            #if ETC1_EXTERNAL_ALPHA
                color.a = tex2D(_AlphaTex, uv).r;
            #endif

                return color;
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

            #ifdef UNITY_INSTANCING_ENABLED
                IN.color = IN.color * _RendererColor;
            #endif

                float4 vertex = UnityFlipSprite(IN.vertex, _Flip);
                OUT.worldPosition = mul(unity_ObjectToWorld, vertex);
                OUT.vertex = UnityObjectToClipPos(vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;

            #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
            #endif

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;

                if (_ClipRectEnabled > 0.5)
                {
                    float2 worldPos = IN.worldPosition.xy;
                    float2 minRect = _ClipRect.xy;
                    float2 maxRect = _ClipRect.zw;
                    
                    float insideX = step(minRect.x, worldPos.x) * step(worldPos.x, maxRect.x);
                    float insideY = step(minRect.y, worldPos.y) * step(worldPos.y, maxRect.y);
                    float inside = insideX * insideY;
                    
                    clip(inside - 0.001);
                    c.a *= inside;
                }

                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}

