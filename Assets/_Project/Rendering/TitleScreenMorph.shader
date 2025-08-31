Shader "WWIII/TitleScreenMorph"
{
    Properties
    {
        _MainTex ("War Texture", 2D) = "white" {}
        _TransitionTex ("Transition Texture", 2D) = "white" {}
        _PeaceTex ("Peace Texture", 2D) = "white" {}
        _BlendFactor ("Blend Factor", Range(0, 2)) = 0
        _CrossfadeWidth ("Crossfade Width", Range(0.01, 0.5)) = 0.1
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Overlay"
            "CanUseSpriteAtlas"="True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            sampler2D _TransitionTex;
            sampler2D _PeaceTex;
            float _BlendFactor;
            float _CrossfadeWidth;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 warColor = tex2D(_MainTex, i.uv);
                fixed4 transitionColor = tex2D(_TransitionTex, i.uv);
                fixed4 peaceColor = tex2D(_PeaceTex, i.uv);
                
                fixed4 finalColor;
                
                if (_BlendFactor <= 1.0)
                {
                    // Phase 1: War to Transition
                    float t1 = smoothstep(0.5 - _CrossfadeWidth, 0.5 + _CrossfadeWidth, _BlendFactor);
                    finalColor = lerp(warColor, transitionColor, t1);
                }
                else
                {
                    // Phase 2: Transition to Peace  
                    float t2 = smoothstep(1.5 - _CrossfadeWidth, 1.5 + _CrossfadeWidth, _BlendFactor);
                    finalColor = lerp(transitionColor, peaceColor, t2);
                }
                
                return finalColor;
            }
            ENDCG
        }
    }
}