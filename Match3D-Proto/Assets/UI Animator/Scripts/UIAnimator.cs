using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAnimatorCore
{
	public enum AnimSetupType
	{
		Intro,
		Loop,
		Outro
	}

	public enum PlayTimeMode
	{
		GAME_TIME,
		REAL_TIME
	}

	public enum AnimationPlayMode
	{
		OPTIMAL,
		CONTINUOUS
	}

	[AddComponentMenu("UI/UI Animator", 0)]
	[ExecuteInEditMode]
	public class UIAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField]
		private ActionMatrixData m_playOnEnableAMD = null;

		[SerializeField]
		private ActionMatrixData m_startPoseAMD = null;

		[SerializeField]
		private ActionMatrixData m_afterIntroAMD = new ActionMatrixData(false);

		[SerializeField]
		private ActionMatrixData m_afterLoopAMD = new ActionMatrixData(false);

		[SerializeField]
		private ActionMatrixData m_onPointerEnterAMD = new ActionMatrixData(false);

		[SerializeField]
		private ActionMatrixData m_onPointerExitAMD = new ActionMatrixData(false);

		[SerializeField]
		private PlayTimeMode m_timeMode = PlayTimeMode.REAL_TIME;

		[SerializeField]
		private AnimationPlayMode m_animationPlayMode = AnimationPlayMode.OPTIMAL;

		[SerializeField]
		private List<AnimationSetup> m_animationSetups;

		[SerializeField]
		private float m_timer = 0;

		[SerializeField]
		private int m_currentAnimSetupIndex = 0;

		[SerializeField]
		private Transform m_audioSourceContainer = null;

		[SerializeField]
		private List<AudioSource> m_audioSources = null;

		[SerializeField]
		private bool m_playLoopAnimInfinitely = true;

		[SerializeField]
		private int m_numLoopIterations = 0;

#if UNITY_EDITOR
#pragma warning disable 0414
		[SerializeField]
		private bool m_showExtraSettings = false;
#pragma warning restore 0414
#endif

		private const float MAX_FRAME_DELTA = 1f / 20f;

		private System.Action m_onFinishAction = null;

		private int m_currentLoopIterationCount = 0;
		private int m_currentStageIndex = 0;
		private bool m_playingAnimation = false;
		private float m_lastRealtime;
		private bool m_paused = false;
		private bool m_resetAudioEnabledStateChange = false;

		private static bool s_isAudioEnabled = true;
		private static bool s_audioEnabledStateChanged = false;

		public List<AnimationSetup> AnimationSetups { get { return m_animationSetups; } }
		public List<AnimationStage> CurrentAnimStages {
			get {
				if (m_animationSetups != null && m_animationSetups.Count > m_currentAnimSetupIndex)
				{
					return m_animationSetups [m_currentAnimSetupIndex].AnimationStages;
				}

				return null;
			}
		}
		public PlayTimeMode TimeMode { get { return m_timeMode; } set { m_timeMode = value; } }
		public AnimationPlayMode PlayMode { get { return m_animationPlayMode; } set { m_animationPlayMode = value; } }
		public float Timer { get { return m_timer; } }
		public AnimSetupType CurrentAnimType { get { return (AnimSetupType) m_currentAnimSetupIndex; } }

		public bool IsPlaying
		{
			get
			{
				return m_playingAnimation;
			}
		}

		public bool Paused
		{
			get
			{
				return m_paused;
			}
			set
			{
				m_paused = value;
			}
		}

		private void OnEnable ()
		{
			if (Application.isPlaying)
			{
				if (m_playOnEnableAMD.IsEnabled)
				{
					PlayAnimation (m_playOnEnableAMD.AnimType, m_playOnEnableAMD.Delay);
				}
				else
				{
					// Set to the specified starting anim state
					CheckDataInit ();

					SetAnimType (m_startPoseAMD.AnimType);
				}
			}
		}

		private void Start ()
		{
			CheckDataInit ();

			if (Application.isPlaying)
			{
				ResetToStart ();
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (m_onPointerEnterAMD.IsEnabled)
			{
				PlayAnimation (m_onPointerEnterAMD.AnimType, m_onPointerEnterAMD.Delay);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (m_onPointerExitAMD.IsEnabled)
			{
				PlayAnimation (m_onPointerExitAMD.AnimType, m_onPointerExitAMD.Delay);
			}
		}

		private void CheckDataInit()
		{
			if (m_animationSetups == null || m_animationSetups.Count < 3)
			{
				// initialise with two blank animationsetup instances; one for INTRO, and for OUTRO
				m_animationSetups = new List<AnimationSetup> (){ new AnimationSetup(), new AnimationSetup(), new AnimationSetup()};
			}
		}

		private void LateUpdate ()
		{
			if (Application.isPlaying == false)
			{
				return;
			}

			if (m_playingAnimation && !m_paused)
			{
				if (UpdateState (m_timeMode == PlayTimeMode.REAL_TIME ? (Time.realtimeSinceStartup - m_lastRealtime) : Time.deltaTime))
				{
					AnimSetupType finishedAnimType = CurrentAnimType;

					m_playingAnimation = false;

					if (m_onFinishAction != null)
					{
						m_onFinishAction ();
						m_onFinishAction = null;
					}

					if (m_animationSetups [m_currentAnimSetupIndex].OnFinishedAction != null)
					{
						m_animationSetups [m_currentAnimSetupIndex].OnFinishedAction.Invoke();
					}

					if (finishedAnimType == AnimSetupType.Intro && m_afterIntroAMD.IsEnabled)
					{
						PlayAnimation (m_afterIntroAMD.AnimType, m_afterIntroAMD.Delay);
					}
					else if(finishedAnimType == AnimSetupType.Loop && m_afterLoopAMD.IsEnabled)
					{
						PlayAnimation (m_afterLoopAMD.AnimType, m_afterLoopAMD.Delay);
					}
				}

				if (m_timeMode == PlayTimeMode.REAL_TIME)
				{
					m_lastRealtime = Time.realtimeSinceStartup;
				}
			}

			if (m_resetAudioEnabledStateChange)
			{
				s_audioEnabledStateChanged = false;
				m_resetAudioEnabledStateChange = false;
			}

			if (s_audioEnabledStateChanged && m_audioSources != null)
			{
				for (int idx = 0; idx < m_audioSources.Count; idx++)
				{
					if (m_audioSources [idx].isPlaying)
					{
						m_audioSources [idx].mute = !s_isAudioEnabled;
					}
				}

				// Delay the disabling of the static changed state by a frame, to allow other active UIAnimator instances a chance to mute/unmute their audioSources
				m_resetAudioEnabledStateChange = true;
			}
		}

		public void SetPlayOnEnable(bool a_playOnEnable, AnimSetupType a_animToPlay)
		{
			SetPlayOnEnable (a_playOnEnable, a_animToPlay, 0);
		}

		public void SetPlayOnEnable(bool a_playOnEnable, AnimSetupType a_animToPlay, float a_delay)
		{
			m_playOnEnableAMD.IsEnabled = a_playOnEnable;
			m_playOnEnableAMD.AnimType = a_animToPlay;
			m_playOnEnableAMD.Delay = a_delay;
		}

		public void SetPlayAfterIntro(bool a_playAfterIntro, AnimSetupType a_animToPlay)
		{
			SetPlayAfterIntro (a_playAfterIntro, a_animToPlay, 0);
		}

		public void SetPlayAfterIntro(bool a_playAfterIntro, AnimSetupType a_animToPlay, float a_delay)
		{
			m_afterIntroAMD.IsEnabled = a_playAfterIntro;
			m_afterIntroAMD.AnimType = a_animToPlay;
			m_afterIntroAMD.Delay = a_delay;
		}

		public void SetPlayAfterLoop(bool a_playAfterLoop, AnimSetupType a_animToPlay)
		{
			SetPlayAfterLoop (a_playAfterLoop, a_animToPlay, 0);
		}

		public void SetPlayAfterLoop(bool a_playAfterLoop, AnimSetupType a_animToPlay, float a_delay)
		{
			m_afterLoopAMD.IsEnabled = a_playAfterLoop;
			m_afterLoopAMD.AnimType = a_animToPlay;
			m_afterLoopAMD.Delay = a_delay;
		}

		public void SetPlayOnPointerEnter(bool a_playOnPointerEnter, AnimSetupType a_animToPlay)
		{
			SetPlayOnPointerEnter (a_playOnPointerEnter, a_animToPlay, 0);
		}

		public void SetPlayOnPointerEnter(bool a_playOnPointerEnter, AnimSetupType a_animToPlay, float a_delay)
		{
			m_onPointerEnterAMD.IsEnabled = a_playOnPointerEnter;
			m_onPointerEnterAMD.AnimType = a_animToPlay;
			m_onPointerEnterAMD.Delay = a_delay;
		}

		public void SetPlayOnPointerExit(bool a_playOnPointerExit, AnimSetupType a_animToPlay)
		{
			SetPlayOnPointerExit (a_playOnPointerExit, a_animToPlay, 0);
		}

		public void SetPlayOnPointerExit(bool a_playOnPointerExit, AnimSetupType a_animToPlay, float a_delay)
		{
			m_onPointerExitAMD.IsEnabled = a_playOnPointerExit;
			m_onPointerExitAMD.AnimType = a_animToPlay;
			m_onPointerExitAMD.Delay = a_delay;
		}

		public void PlayAnimation (AnimSetupType a_animType)
		{
			PlayAnimation (a_animType, 0, null);
		}

		public void PlayAnimation (AnimSetupType a_animType, float a_delay)
		{
			PlayAnimation (a_animType, a_delay, null);
		}

		public void PlayAnimation (AnimSetupType a_animType, System.Action a_onFinish)
		{
			PlayAnimation (a_animType, 0, a_onFinish);
		}

		public void PlayAnimation (AnimSetupType a_animType, float a_delay, System.Action a_onFinish)
		{
			CheckDataInit ();

			SetAnimType (a_animType);

			m_onFinishAction = a_onFinish;

			if (a_delay > 0)
			{
				StartCoroutine (PlayAnimationAfterDelay (a_animType, a_delay, a_onFinish));
			}
			else
			{
				PlayCurrentAnimationSetup ();
			}
		}

		private IEnumerator PlayAnimationAfterDelay(AnimSetupType a_animType, float a_delay, System.Action a_onFinish)
		{
			yield return new WaitForSeconds (a_delay);

			PlayCurrentAnimationSetup ();
		}

		private void PlayCurrentAnimationSetup()
		{
			if (m_timeMode == PlayTimeMode.REAL_TIME)
			{
				m_lastRealtime = Time.realtimeSinceStartup;
			}

			m_playingAnimation = true;

			if (m_animationSetups [m_currentAnimSetupIndex].OnStartAction != null)
			{
				m_animationSetups [m_currentAnimSetupIndex].OnStartAction.Invoke();
			}
		}

		private List<AnimationStage> GetAnimationStepStages(AnimSetupType a_animType)
		{
			int setupIndex = (int) a_animType;

			if (m_animationSetups != null && m_animationSetups.Count > setupIndex)
			{
				return m_animationSetups [setupIndex].AnimationStages;
			}

			return null;
		}

		public void SetAnimType(AnimSetupType a_animType)
		{
			m_currentAnimSetupIndex = (int) a_animType;

			ResetToDefault ();

			if (Application.isPlaying)
			{
				ResetToStart();
			}
		}

		public void GrabCurrentStateAsMaster()
		{
			if (m_animationSetups != null)
			{
				for (int setupIdx = 0; setupIdx < m_animationSetups.Count; setupIdx++)
				{
					for (int sIdx = 0; sIdx < m_animationSetups[setupIdx].AnimationStages.Count; sIdx++)
					{
						if (m_animationSetups[setupIdx].AnimationStages[sIdx].AnimationInstances != null)
						{
							for (int aIdx = 0; aIdx < m_animationSetups[setupIdx].AnimationStages[sIdx].AnimationInstances.Count; aIdx++)
							{
								m_animationSetups [setupIdx].AnimationStages [sIdx].AnimationInstances [aIdx].SetAsMasterState ();
							}
						}
					}
				}
			}
		}

		public void ResetToDefault()
		{
			ResetToStart (AnimSetupType.Outro);
			ResetToStart (AnimSetupType.Loop);
			ResetToEnd (AnimSetupType.Intro);
		}

		public void ResetToStart()
		{
			ResetToStart (CurrentAnimType);
		}

		public void ResetToStart(AnimSetupType a_animSetupType)
		{
			m_currentStageIndex = 0;
			m_timer = 0;

			if (m_animationSetups == null || (int) a_animSetupType >= m_animationSetups.Count)
			{
				return;
			}

			if (a_animSetupType != AnimSetupType.Loop)
			{
				m_currentLoopIterationCount = 0;
			}

			List<AnimationStage> currentAnimStages = m_animationSetups [(int) a_animSetupType].AnimationStages;

			if (currentAnimStages != null)
			{
				for (int sIdx = 0; sIdx < currentAnimStages.Count; sIdx++)
				{
					currentAnimStages [sIdx].ResetToStart (a_animSetupType);
				}
			}

			if (!Application.isPlaying)
			{
				ForceStopAllAudioSources ();
			}
		}

		public void ResetToEnd()
		{
			ResetToEnd (CurrentAnimType);
		}

		public void ResetToEnd(AnimSetupType a_animSetupType)
		{
			if (m_animationSetups == null || (int) a_animSetupType >= m_animationSetups.Count)
			{
				return;
			}

			List<AnimationStage> currentAnimStages = m_animationSetups [(int) a_animSetupType].AnimationStages;

			m_currentStageIndex = currentAnimStages.Count - 1;
			m_timer = GetAnimationDuration ();

			if (currentAnimStages != null)
			{
				for (int sIdx = 0; sIdx < currentAnimStages.Count; sIdx++)
				{
					currentAnimStages [sIdx].ResetToEnd (a_animSetupType);
				}
			}
		}

		public void ForceStopAllAudioSources()
		{
			if (m_audioSources != null)
			{
				// Ensure all audioSources are not playing
				for (int idx = 0; idx < m_audioSources.Count; idx++)
				{
					if (m_audioSources [idx] == null)
					{
						// Dead reference. Delete it.
						m_audioSources.RemoveAt(idx);
						idx--;
						continue;
					}

					if (m_audioSources [idx].isPlaying)
					{
						m_audioSources [idx].Stop ();
					}
				}
			}
		}

		public static void SetUIAudioState(bool a_audioIsPlaying)
		{
			if (s_isAudioEnabled == a_audioIsPlaying)
			{
				return;
			}

			s_isAudioEnabled = a_audioIsPlaying;

			s_audioEnabledStateChanged = true;
		}
			
		public bool UpdateState(AnimSetupType a_animType, float a_deltaTime)
		{
			m_currentAnimSetupIndex = (int)a_animType;

			return UpdateState (a_deltaTime, a_isPrimaryAnimator: true);
		}

		public bool UpdateState(AnimSetupType a_animType, float a_deltaTime, bool a_isPrimaryAnimator)
		{
			m_currentAnimSetupIndex = (int)a_animType;

			return UpdateState (a_deltaTime, a_isPrimaryAnimator);
		}

		public bool UpdateState(float a_deltaTime)
		{
			return UpdateState (a_deltaTime, a_isPrimaryAnimator: true);
		}

		public bool UpdateState(float a_deltaTime, bool a_isPrimaryAnimator)
		{
			if (a_deltaTime > MAX_FRAME_DELTA)
			{
				a_deltaTime = MAX_FRAME_DELTA;
			}

			List<AnimationStage> currentAnimStages = CurrentAnimStages;

			if (currentAnimStages == null)
			{
				return true;
			}

			if (currentAnimStages.Count <= m_currentStageIndex)
			{
				// Animation has ended
				if (CurrentAnimType == AnimSetupType.Loop && a_isPrimaryAnimator && (m_playLoopAnimInfinitely || m_numLoopIterations <= 0 || m_currentLoopIterationCount < m_numLoopIterations - 1))
				{
					// Looping the Loop anim. Reset and let play again.
					ResetToStart();

					m_currentLoopIterationCount++;
				}
				else
				{
					// Animation finished
					return true;
				}
			}

			m_timer += a_deltaTime;

			if (currentAnimStages[m_currentStageIndex].UpdateState(this, CurrentAnimType, m_animationPlayMode, a_deltaTime))
			{
				m_currentStageIndex++;
			}

#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				SceneView.RepaintAll ();
			}
#endif
			
			return false;
		}

		public void SetAnimationTimer(AnimSetupType a_animType, float a_timerValue, bool a_forceLinearTimings = false)
		{
			m_currentAnimSetupIndex = (int) a_animType;

			SetAnimationTimer (a_timerValue, a_forceLinearTimings);
		}

		public void SetAnimationTimer(float a_timerValue, bool a_forceLinearTimings = false)
		{
			List<AnimationStage> currentAnimStages = CurrentAnimStages;

			if (currentAnimStages == null)
			{
				return;
			}

			m_timer = a_timerValue;

			AnimSetupType currentAnimSetupType = CurrentAnimType;
			float timerOffset = a_timerValue;
			float stageDuration = 0;

			m_currentStageIndex = 0;

			for (int sIdx = 0; sIdx < currentAnimStages.Count; sIdx++)
			{
				stageDuration = currentAnimStages [sIdx].GetTotalDuration(currentAnimSetupType);

				if (timerOffset > stageDuration)
				{
					// Set this stage to complete state
					currentAnimStages [sIdx].ResetToEnd( currentAnimSetupType );
				}
				else if (timerOffset >= 0)
				{
					// Part way through this anim stage
					currentAnimStages [sIdx].SetAnimationTimer(currentAnimSetupType, timerOffset - currentAnimStages [sIdx].StartDelay, a_forceLinearTimings);

					m_currentStageIndex = sIdx;
				}
				else
				{
					// Set this stage to disabled/not-started state
					currentAnimStages [sIdx].ResetToStart( currentAnimSetupType );
				}

				timerOffset -= stageDuration;
			}
		}

		public void TriggerAudioClip(AudioClipData a_audioClipData, int a_targetIndex)
		{
			if (a_audioClipData.Clip == null)
			{
				Debug.LogWarning ("TriggerAudioClip() called with AudioClipData missing an AudioClip reference.");
				return;
			}

			if (m_audioSources == null)
			{
				m_audioSources = new List<AudioSource> ();
			}

			AudioSource audioSource = null;

			for (int idx = 0; idx < m_audioSources.Count; idx++)
			{
				if (m_audioSources [idx] == null)
				{
					// Dead link. Remove it.
					m_audioSources.RemoveAt(idx);
					idx--;
					continue;
				}

				if (!m_audioSources [idx].isPlaying)
				{
					audioSource = m_audioSources [idx];
					break;
				}
			}

			if (audioSource == null)
			{
				// No available unused AudioSource's. Need to add a new one.
				audioSource = GetNewAudioSource();
			}

			// Play the clip
			audioSource.clip = a_audioClipData.Clip;
			audioSource.playOnAwake = false;
			audioSource.spatialBlend = 0;	// 2D Sound
			audioSource.volume = a_audioClipData.Volume.GetValue(a_targetIndex);
			audioSource.pitch = a_audioClipData.Pitch.GetValue (a_targetIndex);
			audioSource.time = a_audioClipData.OffsetTime.GetValue(a_targetIndex);
			audioSource.mute = !s_isAudioEnabled;

			audioSource.Play ();
		}

		private AudioSource GetNewAudioSource()
		{
			if (m_audioSourceContainer == null)
			{
				m_audioSourceContainer = (new GameObject ("UI Animator - Audio")).transform;
				m_audioSourceContainer.SetParent (transform);
				m_audioSourceContainer.localPosition = Vector3.zero;
			}

			GameObject audioSourceGameObject = new GameObject ("UI Animator - AudioSource");
			audioSourceGameObject.transform.SetParent (m_audioSourceContainer);
			audioSourceGameObject.transform.localPosition = Vector3.zero;

			AudioSource newAudioSource = (AudioSource) audioSourceGameObject.AddComponent (typeof(AudioSource));
			m_audioSources.Add (newAudioSource);

			return newAudioSource;
		}

		public void AddNewAnimationInstance(GameObject[] a_newTargets, int a_stageIndex = -1)
		{
			List<AnimationStage> currentAnimStages = CurrentAnimStages;

			if (currentAnimStages.Count == 0)
			{
				currentAnimStages.Add ( new AnimationStage() );
			}

			if (a_stageIndex < 0 || a_stageIndex >= currentAnimStages.Count)
			{
				a_stageIndex = currentAnimStages.Count - 1;
			}

			currentAnimStages [a_stageIndex].AddNewAnimationInstance (a_newTargets);
		}


		public float GetAnimationDuration()
		{
			return GetAnimationDuration (CurrentAnimType);
		}

		public float GetAnimationDuration(AnimSetupType a_animType)
		{
			CheckDataInit ();

			List<AnimationStage> currentAnimStages = m_animationSetups [(int) a_animType].AnimationStages;

			float totalDuration = 0;

			if (currentAnimStages != null)
			{
				for (int sIdx = 0; sIdx < currentAnimStages.Count; sIdx++)
				{
					totalDuration += currentAnimStages [sIdx].GetTotalDuration(a_animType);
				}
			}

			return totalDuration;
		}
	}
}
