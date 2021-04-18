Shader "Sprite/SpriteUV"
{
	Properties
	{
		//精灵纹理
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		//UV移动动画_X轴
		AnimatedOffsetUV_X_1("AnimatedOffsetUV_X_1", Range(-1, 1)) = 0.211
		//UV移动动画_Y轴
		AnimatedOffsetUV_Y_1("AnimatedOffsetUV_Y_1", Range(-1, 1)) = 0.211
		//UV移动动画_UV列数
		AnimatedOffsetUV_ZoomX_1("AnimatedOffsetUV_ZoomX_1", Range(1, 30)) = 2.205
		//UV移动动画_UV行数
		AnimatedOffsetUV_ZoomY_1("AnimatedOffsetUV_ZoomY_1", Range(1, 10)) = 2.591
		//UV插值
		_LerpUV_Fade_1("_LerpUV_Fade_1", Range(0, 1)) = 1
		//UV渐淡
		_SpriteFade("SpriteFade", Range(0, 1)) = 1.0

		// required for UI.Mask
		[HideInInspector]_StencilComp("Stencil Comparison", Float) = 8
		[HideInInspector]_Stencil("Stencil ID", Float) = 0
		[HideInInspector]_StencilOp("Stencil Operation", Float) = 0
		[HideInInspector]_StencilWriteMask("Stencil Write Mask", Float) = 255
		[HideInInspector]_StencilReadMask("Stencil Read Mask", Float) = 255
		[HideInInspector]_ColorMask("Color Mask", Float) = 15

	}

	SubShader
	{
		//渲染队列=透明通道             忽略投影                    渲染类型                    预览类型=平面     可以使用精灵图吗
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "true" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }

		ZWrite Off//深度检测关闭
		Blend SrcAlpha OneMinusSrcAlpha//混合效果：透明混合
		Cull Off//背面剔除关闭

		// required for UI.Mask
		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Pass
		{

			CGPROGRAM
			//声明顶点着色器代码
			#pragma vertex vert
			//声明片元着色器代码
			#pragma fragment frag
			//使用低精度
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			//顶点着色器输入结构体
			struct appdata_t {
				float4 vertex   : POSITION;//顶点位置信息
				float4 color    : COLOR;//颜色信息
				float2 texcoord : TEXCOORD0;//纹理坐标集
			};
			//片元着色器输入结构体
			struct v2f
			{
				float2 texcoord  : TEXCOORD0;//纹理坐标集
				float4 vertex   : SV_POSITION;//屏幕坐标系中位置信息
				float4 color    : COLOR;//颜色信息
			};

			//对应最上面材质属性声明
			sampler2D _MainTex;
			float _SpriteFade;
			float AnimatedOffsetUV_X_1;
			float AnimatedOffsetUV_Y_1;
			float AnimatedOffsetUV_ZoomX_1;
			float AnimatedOffsetUV_ZoomY_1;
			float _LerpUV_Fade_1;

			//顶点着色器代码
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				return OUT;
			}

			//New!  UV偏移与扩展处理   UV      X轴上的偏移度 Y轴偏移度       UV列数        UV行数        移动速度
			float2 AnimatedOffsetUV(float2 uv, float offsetx, float offsety, float zoomx, float zoomy)
			{
				uv += float2(offsetx, offsety);//对像素进行偏移，(像素在原有的位置上位移到新的像素位置)
				/*↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓
					uv *= float2(zoomx, zoomy);//先让UV扩展行数与列数
					uv = fmod(uv,1);//再对UV进行取余处理，保证得到UV显示正常
				下面操作扩展↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓*/
				uv = fmod(uv * float2(zoomx, zoomy), 1);
				return uv;
			}
			float4 frag(v2f i) : COLOR
			{
				//UV偏移与扩展处理
				float2 AnimatedOffsetUV_1 = AnimatedOffsetUV(i.texcoord,AnimatedOffsetUV_X_1,AnimatedOffsetUV_Y_1,AnimatedOffsetUV_ZoomX_1,AnimatedOffsetUV_ZoomY_1);
				//插值计算(让第二张UV接近于第一张UV)
				i.texcoord = lerp(i.texcoord,AnimatedOffsetUV_1,_LerpUV_Fade_1);

				//对纹理进行采样
				float4 _MainTex_1 = tex2D(_MainTex,i.texcoord);
				//最终的输出结果
				float4 FinalResult = _MainTex_1;
				FinalResult.rgb *= i.color.rgb;
				//对透明通道进行渐淡处理(乘以渐淡参数)
				FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;

				return FinalResult;
			}

			ENDCG
		}
	}
	Fallback "Sprites/Default"
}