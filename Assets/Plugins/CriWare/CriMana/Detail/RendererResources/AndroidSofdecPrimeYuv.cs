/****************************************************************************
 *
 * Copyright (c) 2015-2018 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if !UNITY_EDITOR && UNITY_ANDROID

using UnityEngine;
using System;


namespace CriMana.Detail
{
	public static partial class AutoResisterRendererResourceFactories
	{
		[RendererResourceFactoryPriority(5050)]
		public class RendererResourceFactoryAndroidSofdecPrimeYuv : RendererResourceFactory
		{
			public override RendererResource CreateRendererResource(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
			{
				bool isCodecSuitable = movieInfo.codecType == CodecType.SofdecPrime || movieInfo.codecType == CodecType.VP9;
				bool isGraphicsApiSuitable = (UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3);
				bool isSuitable = isCodecSuitable && isGraphicsApiSuitable;
				return isSuitable
					? new RendererResourceAndroidSofdecPrimeYuv(playerId, movieInfo, additive, userShader)
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




	public class RendererResourceAndroidSofdecPrimeYuv : RendererResource
	{
		private int     width, height;
		private int     chromaWidth, chromaHeight;
		private int     alphaWidth, alphaHeight;
		private bool    useUserShader;
		private CodecType   codecType;

#if !UNITY_5_6_OR_NEWER || UNITY_5_6_0 || UNITY_5_6_1
		private const TextureFormat   format = TextureFormat.Alpha8;
#else
		private const TextureFormat   format = TextureFormat.R8;
#endif
		private Vector4     movieTextureST = Vector4.zero;
		private Vector4     movieChromaTextureST = Vector4.zero;
		private Vector4     movieAlphaTextureST = Vector4.zero;

		private Texture2D[] textures;
		private RenderTexture[] renderTextures;
		private System.IntPtr[] nativePtrs;

		private Int32       playerID;
		private bool        areTexturesUpdated = false;
		private bool        isFrameUpdated = false;
		private bool        isStoppingForSeek = false;
		private bool        isStartTriggered = true;


		public RendererResourceAndroidSofdecPrimeYuv(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
		{
			CalculateTextureSize(ref width, ref height, (int)movieInfo.width, (int)movieInfo.height, movieInfo.codecType, false);
			CalculateTextureSize(ref chromaWidth, ref chromaHeight, (int)movieInfo.width, (int)movieInfo.height, movieInfo.codecType, true);

			this.additive   = additive;
			hasAlpha    = movieInfo.hasAlpha;
			useUserShader   = userShader != null;
			codecType       = movieInfo.codecType;

			if (userShader != null) {
				shader = userShader;
			} else {
				string shaderName = "CriMana/AndroidSofdecPrimeYuv";
				shader = Shader.Find(shaderName);
			}

			if (hasAlpha) {
				CalculateTextureSize(ref alphaWidth, ref alphaHeight, (int)movieInfo.width, (int)movieInfo.height, movieInfo.alphaCodecType, false);
			}

			UpdateMovieTextureST(movieInfo.dispWidth, movieInfo.dispHeight);

			int numTextures = hasAlpha ? 4 : 3;
			textures = new Texture2D[numTextures];
			nativePtrs = new IntPtr[numTextures];

			playerID = playerId;
		}


		protected override void OnDisposeManaged()
		{
		}


		protected override void OnDisposeUnmanaged()
		{
			DisposeTextures(textures);
			DisposeTextures(renderTextures);

			textures = null;
			renderTextures = null;
		}


		public override bool IsPrepared()
		{
			return areTexturesUpdated && isFrameUpdated;
		}


		public override bool ContinuePreparing()
		{ return true; }


		public override bool IsSuitable(int playerId, MovieInfo movieInfo, bool additive, Shader userShader)
		{
			int w = 0, h = 0;
			CalculateTextureSize(ref w, ref h, (int)movieInfo.width, (int)movieInfo.height, movieInfo.codecType, false);
			bool isCodecSuitable    = movieInfo.codecType == codecType;
			bool isSizeSuitable     = (width == w) && (height == h);
			bool isAlphaSuitable    = hasAlpha == movieInfo.hasAlpha;
			bool isAdditiveSuitable = this.additive == additive;
			bool isShaderSuitable   = this.useUserShader ? (userShader == shader) : true;
			return isCodecSuitable && isSizeSuitable && isAlphaSuitable && isAdditiveSuitable && isShaderSuitable;
		}


		public override bool OnPlayerStopForSeek()
		{
			isStoppingForSeek = true;
			isStartTriggered = false;

			if (renderTextures != null || textures[0] == null) {
				return true;
			}

			renderTextures = new RenderTexture[hasAlpha ? 4 : 3];

			for (int i = 0; i < renderTextures.Length; i++) {
				Texture2D baseTexture = textures[i];
				RenderTexture texture = new RenderTexture(baseTexture.width, baseTexture.height, 0, RenderTextureFormat.R8);
				texture.Create();

				Graphics.Blit(baseTexture, texture);
				renderTextures[i] = texture;
			}

			forceUpdateMaterialTextures(renderTextures);

			DisposeTextures(textures);

			return true;
		}


		public override void OnPlayerStart() {
			isStartTriggered = true;
		}


		private void forceUpdateMaterialTextures(Texture[] newTextures) {
			if (currentMaterial != null) {
				currentMaterial.SetTexture("_TextureY", newTextures[0]);
				currentMaterial.SetTexture("_TextureU", newTextures[1]);
				currentMaterial.SetTexture("_TextureV", newTextures[2]);
				if (hasAlpha) {
					currentMaterial.SetTexture("_TextureA", newTextures[3]);
				}
			}
		}


		public override void AttachToPlayer(int playerId)
		{
			DisposeTextures(textures);
			areTexturesUpdated = false;
			isFrameUpdated = false;
		}


		public override bool UpdateFrame(int playerId, FrameInfo frameInfo, ref bool frameDrop)
		{
			int numTextures = hasAlpha ? 4 : 3;
			bool updated = CRIWAREF463354B(playerId, numTextures, null, frameInfo, ref frameDrop);
			if (updated && !frameDrop) {
				UpdateMovieTextureST(frameInfo.dispWidth, frameInfo.dispHeight);
			}
			isFrameUpdated |= updated;
			return updated;
		}


		public override bool UpdateMaterial(Material material)
		{
			if (areTexturesUpdated) {
				if (textures[0] != null) {
					if (currentMaterial != material) {
						currentMaterial = material;
						SetupStaticMaterialProperties();
					}
					material.SetVector("_MovieTexture_ST", movieTextureST);
					material.SetVector("_MovieChromaTexture_ST", movieChromaTextureST);

					if (!isStoppingForSeek) {
						material.SetTexture("_TextureY", textures[0]);
						material.SetTexture("_TextureU", textures[1]);
						material.SetTexture("_TextureV", textures[2]);
						if (hasAlpha) {
							material.SetVector("_MovieAlphaTexture_ST", movieAlphaTextureST);
							material.SetTexture("_TextureA", textures[3]);
						}
						DisposeTextures(renderTextures);
						renderTextures = null;
					}
				}
				return true;
			} else {
				return renderTextures != null;
			}
		}

		private void UpdateMovieTextureST(System.UInt32 dispWidth, System.UInt32 dispHeight)
		{
			float uScale = (dispWidth != width) ? (float)(dispWidth - 1) / width : 1.0f;
			float vScale = (dispHeight != height) ? (float)(dispHeight - 1) / height : 1.0f;
			movieTextureST.x = uScale;
			movieTextureST.y = -vScale;
			movieTextureST.z = 0.0f;
			movieTextureST.w = vScale;

			uScale = (dispWidth != chromaWidth * 2) ? (float)(dispWidth / 2 - 1) / (chromaWidth * 2) * 2 : 1.0f;
			vScale = (dispHeight != chromaHeight * 2) ? (float)(dispHeight / 2 - 1) / (chromaHeight * 2) * 2 : 1.0f;
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
			int numTextures = hasAlpha ? 4 : 3;
			nativePtrs[0] = IntPtr.Zero;
			bool updated = CRIWARE9B186887(playerID, numTextures, nativePtrs);
			areTexturesUpdated |= updated;

			if (updated && nativePtrs[0] != System.IntPtr.Zero && isStartTriggered) {
				if (textures[0] == null) {
					textures[0] = Texture2D.CreateExternalTexture(width, height, format, false, false, nativePtrs[0]);
					textures[0].wrapMode = TextureWrapMode.Clamp;
					textures[1] = Texture2D.CreateExternalTexture(chromaWidth, chromaHeight, format, false, false, nativePtrs[1]);
					textures[1].wrapMode = TextureWrapMode.Clamp;
					textures[2] = Texture2D.CreateExternalTexture(chromaWidth, chromaHeight, format, false, false, nativePtrs[2]);
					textures[2].wrapMode = TextureWrapMode.Clamp;
					if (hasAlpha) {
						textures[3] = Texture2D.CreateExternalTexture(alphaWidth, alphaHeight, format, false, false, nativePtrs[3]);
						textures[3].wrapMode = TextureWrapMode.Clamp;
					}
				} else {
					for (int i = 0; i < textures.Length; i++) {
						textures[i].UpdateExternalTexture(nativePtrs[i]);
					}
				}
				isStoppingForSeek = false;
			}
		}

		private static void CalculateTextureSize(ref int w, ref int h, int videoWidth, int videoHeight, CodecType type, bool isChroma)
		{
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
					h = CeilingWith(videoHeight, 2);
				} else {
					w = CeilingWith(CeilingWith(videoWidth, 2) / 2, 8);
					h = CeilingWith(videoHeight, 2) / 2;
				}
			}
		}

	} // class
} // namespace


#endif
