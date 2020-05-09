Shader "Link"
{
    Properties
    {
        _MinMaxBackground ("MinMax Background", Color) = (0,0,0,1)
        _SceneBackground ("Scene Background", Color) = (1,1,1,1)
        _Alpha ("Link Alpha", Range(0,1)) = .3
    }
    Subshader
    {
        // initialise with black or white
        Pass
        {
            Color [_MinMaxBackground]
        }
        // take Min or Max as per Holten (2006) depending on background
        Pass
        {
            BlendOp Max
            // BlendOp Min
        }

        // grab the colors to use later
        GrabPass { "_BackgroundTexture" }

        // wipe the screen
        Pass
        {
            Color [_SceneBackground]
        }

        // apply alpha mask on top of grab pass
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 grabPos : TEXCOORD0;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos); // get texture pos
                return o;
            }

            sampler2D _BackgroundTexture;
            float _Alpha;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 bg = tex2Dproj(_BackgroundTexture, i.grabPos); // get texture col
                bg.a = _Alpha;
                return bg;
            }
            ENDCG
        }
    }
}
