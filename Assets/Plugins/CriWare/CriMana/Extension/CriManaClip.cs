/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_2018_1_OR_NEWER && CRIWARE_TIMELINE_1_OR_NEWER

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace CriTimeline.Mana
{
	public class CriManaClip : PlayableAsset , ITimelineClipAsset
	{
		private struct MovieInfoStruct {
			public UInt32 width;                /**< ムービ最大幅（８の倍数） */
			public UInt32 height;               /**< ムービ最大高さ（８の倍数） */
			public UInt32 dispWidth;            /**< 表示したい映像の横ピクセル数（左端から） */
			public UInt32 dispHeight;           /**< 表示したい映像の縦ピクセル数（上端から） */
			public UInt32 framerateN;           /**< 有理数形式フレームレート(分子) framerate [x1000] = framerateN / framerateD */
			public UInt32 framerateD;           /**< 有理数形式フレームレート(分母) framerate [x1000] = framerateN / framerateD */
			public UInt32 totalFrames;      /**< 総フレーム数 */
			public CriMana.CodecType _codecType;
			public CriMana.CodecType _alphaCodecType;
		}

		public readonly Guid guid = Guid.NewGuid();
		public string m_moviePath = "";
		public TextAsset m_movieData = null;
		public bool m_loopWithinClip = false;
		public bool m_useOnMemoryPlayback = false;
		public GCHandle gcHandle = default(GCHandle);
		[SerializeField] private double m_movieFrameRate = 0.0;
		[SerializeField] private double m_clipDuration = 0.0;

		public CriManaBehaviour m_manaBehaviour = new CriManaBehaviour();
		private MovieInfoStruct? m_movieInfoStruct = null;

		private MovieInfoStruct? StructToMovieInfo(CriMana.MovieInfo movieInfo){
			if (movieInfo == null) {
				return null;
			}

			MovieInfoStruct infoStruct = new MovieInfoStruct {
				width = movieInfo.width,
				height = movieInfo.height,
				dispWidth = movieInfo.dispWidth,
				dispHeight = movieInfo.dispHeight,
				framerateN = movieInfo.framerateN,
				framerateD = movieInfo.framerateD,
				totalFrames = movieInfo.totalFrames,
				_codecType = movieInfo.codecType,
				_alphaCodecType = movieInfo.alphaCodecType,
			};

			return infoStruct;
		}

		public ClipCaps clipCaps {
			get { return ClipCaps.Looping; }
		}

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
			return ScriptPlayable<CriManaBehaviour>.Create(graph, m_manaBehaviour);
		}

		public void ReplaceMovieInfo(CriMana.MovieInfo movieInfo) {
			MovieInfoStruct? movieInfoStruct = StructToMovieInfo(movieInfo);
			if (movieInfo == null || movieInfoStruct.Equals(m_movieInfoStruct)) {
				return;
			}

			m_movieInfoStruct = movieInfoStruct;
			m_movieFrameRate = (m_movieInfoStruct.Value.framerateN / (double)m_movieInfoStruct.Value.framerateD);
			m_clipDuration = (m_movieInfoStruct.Value.totalFrames * 1000.0 / (double)m_movieInfoStruct.Value.framerateN);
		}

		public bool IsSameMovie(CriMana.MovieInfo movieInfo) {
			if (!IsMovieInfoReady || movieInfo == null) {
				return false;
			}

			MovieInfoStruct? movieInfoStruct = StructToMovieInfo(movieInfo);
			return movieInfoStruct.Equals(m_movieInfoStruct);
		}

		public bool IsMovieInfoReady {
			get { return (m_movieInfoStruct != null); }
		}

		public int GetSeekFrame(double seekTimeSec, bool loop) {
			if (m_movieInfoStruct != null) {
				double seekFrame = seekTimeSec * m_movieFrameRate;
				if (loop == false) {
					seekFrame = Math.Min(seekFrame, m_movieInfoStruct.Value.totalFrames - 1);
				} else {
					seekFrame %= m_movieInfoStruct.Value.totalFrames;
				}
				return (int)seekFrame;
			} else {
				return 0;
			}
		}

		public override double duration {
			get {
				return m_clipDuration > 0.0 ? m_clipDuration : 10.0;
			}
		}
	}

}


#endif