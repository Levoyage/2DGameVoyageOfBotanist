Shader "Custom/PixelShadowShader"  
{  
    Properties  
    {  
        _MainTex ("Texture", 2D) = "white" {}  
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 0.5)  
        _ShadowOffset ("Shadow Offset", Vector) = (1, 1, 0) // 可根据需求调整  
    }  
    SubShader  
    {  
        Tags { "RenderType"="Transparent" }  
        LOD 200  

        Pass  
        {  
            CGPROGRAM  
            #pragma vertex vert  
            #pragma fragment frag  

            #include "UnityCG.cginc"  

            struct appdata_t  
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
            float4 _MainTex_ST;  
            float4 _ShadowColor;  
            float4 _ShadowOffset;  

            v2f vert (appdata_t v)  
            {  
                v2f o;  
                o.vertex = UnityObjectToClipPos(v.vertex);  
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);  
                return o;  
            }  

            fixed4 frag (v2f i) : SV_Target  
            {  
                fixed4 texColor = tex2D(_MainTex, i.uv);  

                // 计算阴影的UV偏移  
                float2 shadowUV = i.uv + _ShadowOffset.xy * _MainTex_ST.xy;  

                // 获取阴影颜色  
                fixed4 shadowTex = tex2D(_MainTex, shadowUV);  
                fixed4 shadow = _ShadowColor * (1 - shadowTex.a); // 使用主纹理的透明度调整阴影颜色  

                return texColor + shadow; // 合并纹理与伪影  
            }  
            ENDCG  
        }  
    }  
}