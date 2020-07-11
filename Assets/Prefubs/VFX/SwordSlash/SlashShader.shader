// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SlashShader"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_MainTexture("MainTexture", 2D) = "white" {}
		_Opacity("Opacity", Float) = 20
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Vectorif("Vector if", Vector) = (0.56,0,1,0)
		_Vector0("Vector 0", Vector) = (0,0,0,0)
		_Emisiontex("Emision tex", 2D) = "white" {}
		_Emission("Emission", Float) = 0
		_Color0("Color 0", Color) = (0,0,0,0)
		_Float1("Float 1", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#define ASE_NEEDS_FRAG_COLOR


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					float4 ase_texcoord1 : TEXCOORD1;
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					float4 ase_texcoord3 : TEXCOORD3;
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform float4 _Color0;
				uniform sampler2D _Emisiontex;
				uniform float4 _Emisiontex_ST;
				uniform float _Float1;
				uniform float _Emission;
				uniform sampler2D _MainTexture;
				uniform float4 _MainTexture_ST;
				uniform float _Opacity;
				uniform sampler2D _TextureSample0;
				uniform float4 _Vector0;
				uniform float4 _TextureSample0_ST;
				uniform float3 _Vectorif;
				float3 HSVToRGB( float3 c )
				{
					float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
					float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
					return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
				}
				
				float3 RGBToHSV(float3 c)
				{
					float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
					float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
					float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
					float d = q.x - min( q.w, q.y );
					float e = 1.0e-10;
					return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
				}


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					o.ase_texcoord3 = v.ase_texcoord1;

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float2 uv_Emisiontex = i.texcoord.xy * _Emisiontex_ST.xy + _Emisiontex_ST.zw;
					float3 hsvTorgb44 = RGBToHSV( tex2D( _Emisiontex, uv_Emisiontex ).rgb );
					float4 uv127 = i.ase_texcoord3;
					uv127.xy = i.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
					float3 hsvTorgb45 = HSVToRGB( float3(( hsvTorgb44.x + uv127.z ),hsvTorgb44.y,hsvTorgb44.z) );
					float3 desaturateInitialColor48 = hsvTorgb45;
					float desaturateDot48 = dot( desaturateInitialColor48, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar48 = lerp( desaturateInitialColor48, desaturateDot48.xxx, _Float1 );
					float4 _Vector2 = float4(-0.3,1,-3,1);
					float3 temp_cast_1 = (_Vector2.x).xxx;
					float3 temp_cast_2 = (_Vector2.y).xxx;
					float3 temp_cast_3 = (_Vector2.z).xxx;
					float3 temp_cast_4 = (_Vector2.w).xxx;
					float3 clampResult38 = clamp( (temp_cast_3 + (desaturateVar48 - temp_cast_1) * (temp_cast_4 - temp_cast_3) / (temp_cast_2 - temp_cast_1)) , float3( 0,0,0 ) , float3( 1,1,1 ) );
					float2 uv_MainTexture = i.texcoord.xy * _MainTexture_ST.xy + _MainTexture_ST.zw;
					float clampResult5 = clamp( ( tex2D( _MainTexture, uv_MainTexture ).a * _Opacity ) , 0.0 , 1.0 );
					float2 appendResult21 = (float2(_Vector0.z , _Vector0.w));
					float4 uv0_TextureSample0 = i.texcoord;
					uv0_TextureSample0.xy = i.texcoord.xy * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
					float2 panner22 = ( 1.0 * _Time.y * appendResult21 + uv0_TextureSample0.xy);
					float2 break24 = panner22;
					float2 appendResult25 = (float2(break24.x , ( uv127.w + break24.y )));
					float t16 = uv0_TextureSample0.w;
					float w15 = uv0_TextureSample0.z;
					float ifLocalVar12 = 0;
					if( ( tex2D( _TextureSample0, appendResult25 ).r * t16 ) >= w15 )
					ifLocalVar12 = _Vectorif.y;
					else
					ifLocalVar12 = _Vectorif.z;
					float4 appendResult6 = (float4(( ( _Color0 * i.color ) + ( float4( clampResult38 , 0.0 ) * _Emission * i.color ) ).rgb , ( i.color.a * clampResult5 * ifLocalVar12 )));
					

					fixed4 col = appendResult6;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18100
32;223;1306;806;2220.031;846.8822;2.406297;True;False
Node;AmplifyShaderEditor.Vector4Node;19;-2342.012,87.54875;Inherit;False;Property;_Vector0;Vector 0;5;0;Create;True;0;0;False;0;False;0,0,0,0;0,0,0.08,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;29;-1629.414,-797.7695;Inherit;True;Property;_Emisiontex;Emision tex;6;0;Create;True;0;0;False;0;False;-1;1a0bf6f88f9d62a4a86b87b8b8c27662;1a0bf6f88f9d62a4a86b87b8b8c27662;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;14;-1946.41,93.68095;Inherit;False;0;9;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;21;-1938.775,287.4316;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;22;-1615.817,296.8472;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;27;-1353.062,-177;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RGBToHSVNode;44;-1289.414,-762.5977;Inherit;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;47;-1045.58,-627.4966;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;24;-1368.208,295.9466;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.HSVToRGBNode;45;-881.8434,-739.3389;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;49;-811.1157,-587.5436;Inherit;False;Property;_Float1;Float 1;9;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-1041.389,364.9787;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;16;-1608.152,186.907;Float;False;t;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;25;-904.5744,296.4615;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;37;-626.4094,-502.9576;Float;False;Constant;_Vector2;Vector 2;7;0;Create;True;0;0;False;0;False;-0.3,1,-3,1;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DesaturateOpNode;48;-557.9136,-741.5959;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-269.1033,-30.4082;Inherit;False;Property;_Opacity;Opacity;1;0;Create;True;0;0;False;0;False;20;20;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;15;-1610.751,99.80695;Float;False;w;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;17;-432.1677,339.5628;Inherit;False;16;t;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-421.6344,-226.5196;Inherit;True;Property;_MainTexture;MainTexture;0;0;Create;True;0;0;False;0;False;-1;dcd0d3956fcc9e146a2d55864a91afb2;dcd0d3956fcc9e146a2d55864a91afb2;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;34;-258.0772,-616.5917;Inherit;False;5;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;1,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;9;-554.1143,142.6302;Inherit;True;Property;_TextureSample0;Texture Sample 0;2;0;Create;True;0;0;False;0;False;-1;7c70a4204c86bbf41b3fc5056860c4d4;7c70a4204c86bbf41b3fc5056860c4d4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;13;-185.0993,390.916;Inherit;False;Property;_Vectorif;Vector if;4;0;Create;True;0;0;False;0;False;0.56,0,1;0.7,0,1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;18;-185.9678,269.3627;Inherit;False;15;w;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;38;-12.52262,-610.0612;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;41;-329.2209,-801.1512;Inherit;False;Property;_Color0;Color 0;8;0;Create;True;0;0;False;0;False;0,0,0,0;0,0.1448787,0.3301886,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;40;-90.32368,-449.365;Inherit;False;Property;_Emission;Emission;7;0;Create;True;0;0;False;0;False;0;4.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;7;-124.8471,-337.6696;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-152.873,171.2904;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-79.87292,-114.1283;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;5;77.24554,-115.2751;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;115.7791,-721.1512;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;166.9763,-578.8649;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;12;99.03415,171.2904;Inherit;True;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;42;331.7791,-611.1512;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;315.525,-230.8007;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;6;479.9161,-338.911;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;20;-1933.041,-27.97493;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-518.0826,414.5591;Inherit;False;Property;_Float0;Float 0;3;0;Create;True;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;23;-1608.305,-78.7457;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;698.8867,-338.0272;Float;False;True;-1;2;ASEMaterialInspector;0;7;SlashShader;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;True;2;False;-1;True;True;True;True;False;0;False;-1;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;;0
WireConnection;21;0;19;3
WireConnection;21;1;19;4
WireConnection;22;0;14;0
WireConnection;22;2;21;0
WireConnection;44;0;29;0
WireConnection;47;0;44;1
WireConnection;47;1;27;3
WireConnection;24;0;22;0
WireConnection;45;0;47;0
WireConnection;45;1;44;2
WireConnection;45;2;44;3
WireConnection;26;0;27;4
WireConnection;26;1;24;1
WireConnection;16;0;14;4
WireConnection;25;0;24;0
WireConnection;25;1;26;0
WireConnection;48;0;45;0
WireConnection;48;1;49;0
WireConnection;15;0;14;3
WireConnection;34;0;48;0
WireConnection;34;1;37;1
WireConnection;34;2;37;2
WireConnection;34;3;37;3
WireConnection;34;4;37;4
WireConnection;9;1;25;0
WireConnection;38;0;34;0
WireConnection;10;0;9;1
WireConnection;10;1;17;0
WireConnection;3;0;2;4
WireConnection;3;1;4;0
WireConnection;5;0;3;0
WireConnection;43;0;41;0
WireConnection;43;1;7;0
WireConnection;39;0;38;0
WireConnection;39;1;40;0
WireConnection;39;2;7;0
WireConnection;12;0;10;0
WireConnection;12;1;18;0
WireConnection;12;2;13;2
WireConnection;12;3;13;2
WireConnection;12;4;13;3
WireConnection;42;0;43;0
WireConnection;42;1;39;0
WireConnection;8;0;7;4
WireConnection;8;1;5;0
WireConnection;8;2;12;0
WireConnection;6;0;42;0
WireConnection;6;3;8;0
WireConnection;20;0;19;1
WireConnection;20;1;19;2
WireConnection;23;0;20;0
WireConnection;1;0;6;0
ASEEND*/
//CHKSM=99FF771C4DC9074658E7F8575042B477ECC73051