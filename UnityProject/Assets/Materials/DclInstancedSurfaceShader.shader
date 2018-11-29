Shader "Instanced/DclInstancedSurfaceShader" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma target 4.0
        // Physically based Standard lighting model
        #pragma surface surf Standard addshadow fullforwardshadows vertex:vert
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup
        

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
            float4 customColor;
            float4 posSrc; 
        };

    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        StructuredBuffer<float4> positionBuffer;
        StructuredBuffer<float4> colorBuffer;
        StructuredBuffer<float4> scaleBuffer;
        StructuredBuffer<float4x4> matrixBuffer;
    #endif

        //void vert (inout appdata_full v, out Input o, uint instanceID : SV_InstanceID) 
        //v2f vert(appdata v, uint instanceID : SV_InstanceID)
        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            UNITY_SETUP_INSTANCE_ID(v);//not sure if this is needed

            o.posSrc = v.vertex;
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            o.customColor = colorBuffer[unity_InstanceID]; //float4(1, 0, 0, 1);//

            float4 pos_temp = positionBuffer[unity_InstanceID];
            o.posSrc.w= pos_temp.w;
            #endif
            
        }

        void rotate2D(inout float2 v, float r)
        {
            float s, c;
            sincos(r, s, c);
            v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
        }

        void setup()
        {
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            float4 data = positionBuffer[unity_InstanceID];
            float4 scale = scaleBuffer[unity_InstanceID];
            float4x4 matrix_rotate = matrixBuffer[unity_InstanceID];

            //float rotation = data.w * data.w * _Time.y * 0.5f;
            //rotate2D(data.xz, rotation);
            //float4 pos_temp = mul(matrix_rotate, data.xyzw);//data.xyz*matrix_rotate;
            //pos_temp/=pos_temp.w;

            float4x4 matrix_scale;
            matrix_scale._11_21_31_41 = float4(scale.x, 0, 0, 0);
            matrix_scale._12_22_32_42 = float4(0, scale.y, 0, 0);
            matrix_scale._13_23_33_43 = float4(0, 0, scale.z, 0);
            matrix_scale._14_24_34_44 = float4(0, 0, 0, 1);

            float4x4 matrix_move;
            matrix_move._11_21_31_41 = float4(1, 0, 0, 0);
            matrix_move._12_22_32_42 = float4(0, 1, 0, 0);
            matrix_move._13_23_33_43 = float4(0, 0, 1, 0);
            matrix_move._14_24_34_44 = float4(data.xyz, 1);


            // unity_ObjectToWorld._11_21_31_41 = float4(scale.x, 0, 0, 0);
            // unity_ObjectToWorld._12_22_32_42 = float4(0, scale.y, 0, 0);
            // unity_ObjectToWorld._13_23_33_43 = float4(0, 0, scale.z, 0);
            // unity_ObjectToWorld._14_24_34_44 = float4(data.xyz, 1);

            unity_ObjectToWorld = mul(matrix_rotate, matrix_scale);
            unity_ObjectToWorld = mul(matrix_move, unity_ObjectToWorld);

            unity_WorldToObject = unity_ObjectToWorld;
            unity_WorldToObject._14_24_34 *= -1;
            unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;


        #endif
        }

        half _Glossiness;
        half _Metallic;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            //o.Albedo = c.rgb * colorBuffer[unity_InstanceID];
            //o.Albedo = c.rgb*IN.customColor;
            float y = IN.posSrc.y*IN.posSrc.w;
            //float f = step( 0.49f , IN.posSrc.y )*step(0.4f, abs(IN.posSrc.x))*step(0.4f, abs(IN.posSrc.z));  //顶面四个角
            //float f = step( IN.posSrc.w/2-10.0f , y )*(step(0.4f, abs(IN.posSrc.x))+step(0.4f, abs(IN.posSrc.z)));  //顶面四个角
            float f = step( IN.posSrc.w/2 , y )*(step(0.4f, abs(IN.posSrc.x))+step(0.4f, abs(IN.posSrc.z)));  //顶面四条边
            f = saturate(f);
            f = 1.0f-f;
            o.Albedo = c.rgb*IN.customColor*f;
            //o.Albedo = c.rgb*IN.customColor;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}