/****************************************************************************
 *
 * Copyright (c) 2015 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if !UNITY_EDITOR && UNITY_ANDROID

using UnityEngine;
using System;


namespace CriMana.Detail
{
	public static partial class AutoResisterRendererResourceFactories
	{
		[RendererResourceFactoryPriority(5060)]
		public class RendererResourceFactoryAndroidSofdecPrimeYuvLegacyGles2 : RendererResourceFactory
		{
			public override RendererResource CreateRendererResource(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
			{
				bool isCodecSuitable = movieInfo.codecType == CodecType.SofdecPrime || movieInfo.codecType == CodecType.VP9;
				bool isGraphicsApiSuitable = (UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2 ||
											  UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Vulkan);
				bool isSuitable = isCodecSuitable && isGraphicsApiSuitable;
				return isSuitable
					? new RendererResourceAndroidSofdecPrimeYuvLegacyGles2(playerId, movieInfo, additive, userShader)
					: null;
			}

			protected override void OnDisposeManaged()
			{
			}

			protected override void OnDisposeUnmanaged()
			{
			}
		}
	}




	public class RendererResourceAndroidSofdecPrimeYuvLegacyGles2 : RendererResource
	{
		private int width, height;
		private int chromaWidth, chromaHeight;
		private int alphaWidth, alphaHeight;
		private bool    useUserShader;
		CodecType       codecType;

		private Vector4     movieTextureST = Vector4.zero;
		private Vector4     movieChromaTextureST = Vector4.zero;
		private Vector4     movieAlphaTextureST = Vector4.zero;

		private Texture2D[][] textures;
		private RenderTexture[] renderTextures;
		private IntPtr[][]  nativeTextures;

		private Int32   numTextureSets = 2;
		private Int32   currentTextureSet = 0;
		private Int32   drawTextureSet = -1;
		private Int32   playerID;
		private bool    isStoppingForSeek = false;

		public RendererResourceAndroidSofdecPrimeYuvLegacyGles2(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
		{
			CalculateTextureSize(ref width, ref height, (int)movieInfo.width, (int)movieInfo.height, movieInfo.codecType, false);
			CalculateTextureSize(ref chromaWidth, ref chromaHeight, (int)movieInfo.width, (int)movieInfo.height, movieInfo.codecType, true);

			hasAlpha        = movieInfo.hasAlpha;
			this.additive   = additive;
			useUserShader   = userShader != null;

			if (userShader != null) {
				shader = userShader;
			} else {
				string shaderName = "CriMana/SofdecPrimeYuv";
				shader = Shader.Find(shaderName);
			}

			UpdateMovieTextureST(movieInfo.dispWidth, movieInfo.dispHeight);

			TextureFormat format = TextureFormat.Alpha8;
			textures = new Texture2D[numTextureSets][];
			for (int i = 0; i < numTextureSets; i++) {
				textures[i] = new Texture2D[4];
			}
			nativeTextures = new IntPtr[numTextureSets][];

			for (int i = 0; i < numTextureSets; i++) {
				textures[i][0] = new Texture2D(width, height, format, false, true);
				textures[i][0].wrapMode = TextureWrapMode.Clamp;
				textures[i][1] = new Texture2D(chromaWidth, chromaHeight, format, false, true);
				textures[i][1].wrapMode = TextureWrapMode.Clamp;
				textures[i][2] = new Texture2D(chromaWidth, chromaHeight, format, false, true);
				textures[i][2].wrapMode = TextureWrapMode.Clamp;

				nativeTextures[i] = new IntPtr[4];
				nativeTextures[i][0] = textures[i][0].GetNativeTexturePtr();
				nativeTextures[i][1] = textures[i][1].GetNativeTexturePtr();
				nativeTextures[i][2] = textures[i][2].GetNativeTexturePtr();
				if (hasAlpha) {
					textures[i][3] = new Texture2D(alphaWidth, alphaHeight, format, false, true);
					textures[i][3].wrapMode = TextureWrapMode.Clamp;
					nativeTextures[i][3] = textures[i][3].GetNativeTexturePtr();
				} else {
					textures[i][3] = null;
					nativeTextures[i][3] = IntPtr.Zero;
				}
			}
			codecType = movieInfo.codecType;
			playerID = playerId;
		}


		protected override void OnDisposeManaged()
		{}


		protected override void OnDisposeUnmanaged()
		{
			for (int i = 0; i < numTextureSets; i++) {
				DisposeTextures(textures[i]);
			}
			textures = null;
			nativeTextures = null;
		}

		public override bool IsPrepared()
		{ return true; }


		public override bool ContinuePreparing()
		{ return true; }


		public override bool IsSuitable(int playerId, MovieInfo movieInfo, bool additive, Shader userShader) {
			int w = 0, h = 0;
			CalculateTextureSize(ref w, ref h, (int)movieInfo.width, (int)movieInfo.height, movieInfo.codecType, false);
			bool isCodecSuitable = movieInfo.codecType == codecType;
			bool isSizeSuitable = (width == w) && (height == h);
			bool isAlphaSuitable = hasAlpha == movieInfo.hasAlpha;
			bool isAdditiveSuitable = this.additive == additive;
			bool isShaderSuitable = this.useUserShader ? (userShader == shader) : true;
			return isCodecSuitable && isSizeSuitable && isAlphaSuitable && isAdditiveSuitable && isShaderSuitable;
		}


        public override bool OnPlayerStopForSeek() {
			isStoppingForSeek = (drawTextureSet >= 0);
			return true;
		}

		public override void AttachToPlayer(int playerId) {
			drawTextureSet = -1;
		}

		public override bool UpdateFrame(int playerId, FrameInfo frameInfo, ref bool frameDrop) {
			bool isFrameUpdated = CRIWAREF463354B(playerId, 0, null, frameInfo, ref frameDrop);
			if (isFrameUpdated && !frameDrop) {
				UpdateMovieTextureST(frameInfo.dispWidth, frameInfo.dispHeight);
				drawTextureSet = currentTextureSet;
				currentTextureSet = (currentTextureSet + 1) % numTextureSets;
				isStoppingForSeek = false;
			}
			return isFrameUpdated;
		}

		public override bool UpdateMaterial(Material material) {
			if (isStoppingForSeek)
				return true;

			if (drawTextureSet < 0)
				return false;

			if (currentMaterial != material) {
				currentMaterial = material;
				SetupStaticMaterialProperties();
			}

			material.SetTexture("_TextureY", textures[drawTextureSet][0]);
			material.SetTexture("_TextureU", textures[drawTextureSet][1]);
			material.SetTexture("_TextureV", textures[drawTextureSet][2]);
			material.SetVector("_MovieTexture_ST", movieTextureST);
			material.SetVector("_MovieChromaTexture_ST", movieChromaTextureST);
			if (hasAlpha) {
				material.SetTexture("_TextureA", textures[drawTextureSet][3]);
				material.SetVector("_MovieAlphaTexture_ST", movieAlphaTextureST);
			}

			return true;
		}


		private void UpdateMovieTextureST(System.UInt32 dispWidth, System.UInt32 dispHeight) {
			float uScale = (dispWidth != width) ? (float)(dispWidth - 1) / width : 1.0f;
			float vScale = (dispHeight != height) ? (float)(dispHeight - 1) / height : 1.0f;
			movieTextureST.x = uScale;
			movieTextureST.y = -vScale;
			movieTextureST.z = 0.0f;
			movieTextureST.w = vScale;

			uScale = (dispWidth != chromaWidth * 2) ?
					 (float)(dispWidth / 2 - 1) / (chromaWidth * 2) * 2 :
					 1.0f;
			vScale = (dispHeight != chromaHeight * 2) ?
					 (float)(dispHeight / 2 - 1) / (chromaHeight * 2) * 2 :
					 1.0f;
			movieChromaTextureST.x = uScale;
			movieChromaTextureST.y = -vScale;
			movieChromaTextureST.z = 0.0f;
			movieChromaTextureST.w = vScale;

			if (hasAlpha) {
				uScale = (dispWidth != alphaWidth) ? (float)(dispWidth - 1) / alphaWidth : 1.0f;
				vScale = (dispHeight != alphaHeight) ? (float)(dispHeight - 1) / alphaHeight : 1.0f;
				movieAlphaTextureST.x = uScale;
				movieAlphaTextureST.y = -vScale;
				movieAlphaTextureST.z = 0.0f;
				movieAlphaTextureST.w = vScale;
			}
		}

		public override void UpdateTextures()
		{
			if (drawTextureSet < 0)
				return;

			int numTextures = 3;
			if (hasAlpha) {
				numTextures = 4;
			}

			CRIWARE9B186887(playerID, numTextures, nativeTextures[drawTextureSet]);
		}

		private static void CalculateTextureSize(ref int w, ref int h, int videoWidth, int videoHeight, CodecType type, bool isChroma) {
			if (type == CodecType.SofdecPrime) {
				if (!isChroma) {
					w = Ceiling32(Ceiling16(CeilingWith(videoWidth, 8)));
					h = CeilingWith(videoHeight, 8);
				} else {
					w = Ceiling32(Ceiling16(CeilingWith(videoWidth, 8)) / 2);
					h = CeilingWith(videoHeight, 8) / 2;
				}

			} else if (type == CodecType.VP9) {
				if (!isChroma) {
					w = CeilingWith(CeilingWith(videoWidth, 2), 8);
					h = CeilingWith(CeilingWith(videoHeight, 2), 8);
				} else {
					w = CeilingWith(CeilingWith(videoWidth, 2) / 2, 8);
					h = CeilingWith(CeilingWith(videoHeight, 2) / 2, 8);
				}
			}
		}
	}
}


#endif
