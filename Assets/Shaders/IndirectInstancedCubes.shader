Shader "Custom/IndirectInstancedCubes"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
                float2 uv : TEXCOORD0;
            };

            StructuredBuffer<int> _States;
            int _StatesLength;

            uniform float4x4 _ObjectToWorld;
            int _Width;
            int _Height;
            int _Depth;
            float _Spacing;
            float _Size;
            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata_base v, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                v2f o;
                uint cmdID = GetCommandID(0);
                uint instanceID = GetIndirectInstanceID(svInstanceID);
                uint k = instanceID / (_Width * _Height);
                uint remainder = instanceID % (_Width * _Height);
                uint j = remainder / _Width;
                uint i = remainder % _Width;
                uint4 state = _States[instanceID];
                _ObjectToWorld._11_22_33 *= state * _Size;
                float4 wpos = mul(_ObjectToWorld, v.vertex + float4(i * _Spacing, j * _Spacing, k * _Spacing, 0));
                o.pos = mul(UNITY_MATRIX_VP, wpos);
                o.color = _Color;
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                return i.color * texColor;
            }
            ENDCG
        }
    }
}