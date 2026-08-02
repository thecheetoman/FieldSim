Shader "Custom/WhiteToColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TargetColor ("Target Color", Color) = (1,0,0,1) // Default to red
        _Sensitivity ("White Sensitivity", Range(0.0, 1.0)) = 0.95
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

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
            float4 _MainTex_ST;
            float4 _TargetColor;
            float _Sensitivity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample original color from source texture
                fixed4 texColor = tex2D(_MainTex, i.uv);
                
                // Calculate average brightness/luminance of the pixel
                float brightness = (texColor.r + texColor.g + texColor.b) / 3.0;
                
                // If brightness is greater than sensitivity threshold, blend to target color
                float isWhite = step(_Sensitivity, brightness);
                
                // Lerp between original and target color (keeps alpha channel intact)
                fixed4 finalColor = lerp(texColor, _TargetColor, isWhite);
                finalColor.a = texColor.a; 
                
                return finalColor;
            }
            ENDCG
        }
    }
}
