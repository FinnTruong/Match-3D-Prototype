using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIAnimatorCore
{
	[System.Serializable]
	public class AudioClipData
	{
		public enum CLIP_TRIGGER_POINT
		{
			START_OF_ANIM_STEP,
			END_OF_ANIM_STEP
		}

#if UNITY_EDITOR
#pragma warning disable 0414
		[SerializeField]
		private bool m_editorFoldoutState = false;
#pragma warning restore 0414
#endif

		[SerializeField]
		private AudioClip m_clip = null;

		[SerializeField]
		private CLIP_TRIGGER_POINT m_triggerPoint = CLIP_TRIGGER_POINT.START_OF_ANIM_STEP;

		[SerializeField]
		private AnimStepVariablePositiveFloat m_delay;

		[SerializeField]
		private AnimStepVariablePositiveFloat m_offsetTime;

		[SerializeField]
		private AnimStepVariablePositiveFloat m_volume;

		[SerializeField]
		private AnimStepVariablePositiveFloat m_pitch;

		private List<bool> m_clipActivatedStates = null;

		public AudioClip Clip { get { return m_clip; } }
		public CLIP_TRIGGER_POINT TriggerPoint { get { return m_triggerPoint; } }
		public AnimStepVariablePositiveFloat Delay { get { return m_delay; } }
		public AnimStepVariablePositiveFloat OffsetTime { get { return m_offsetTime; } }
		public AnimStepVariablePositiveFloat Volume { get { return m_volume; } }
		public AnimStepVariablePositiveFloat Pitch { get { return m_pitch; } }

		public void Init(int a_numTargets)
		{
			m_delay = new AnimStepVariablePositiveFloat (0, a_numTargets);
			m_offsetTime = new AnimStepVariablePositiveFloat (0, a_numTargets);
			m_volume = new AnimStepVariablePositiveFloat (1, a_numTargets);
			m_pitch = new AnimStepVariablePositiveFloat (1, a_numTargets);
		}

		public void UpdateNumTargets(int a_numTargets)
		{
			m_delay.Initialise (a_numTargets);
			m_offsetTime.Initialise (a_numTargets);
			m_volume.Initialise (a_numTargets);
			m_pitch.Initialise (a_numTargets);
		}

		#region public API methods
		public void SetAudioClip(AudioClip a_audioClip)
		{
			m_clip = a_audioClip;
		}

		public void SetPlayWhen(CLIP_TRIGGER_POINT a_triggerPoint)
		{
			m_triggerPoint = a_triggerPoint;
		}

		#endregion

		public bool HasAudioClipActivated(int a_targetIndex)
		{
			if (m_clipActivatedStates == null || a_targetIndex >= m_clipActivatedStates.Count)
			{
				return false;
			}

			return m_clipActivatedStates[a_targetIndex];
		}

		public void MarkAudioClipActivated(int a_targetIndex)
		{
			if (m_clipActivatedStates == null)
			{
				m_clipActivatedStates = new List<bool> ();
			}

			if (a_targetIndex >= m_clipActivatedStates.Count)
			{
				// Increase list size to cover the requested target index
				for (int idx = m_clipActivatedStates.Count; idx < (a_targetIndex + 1); idx++)
				{
					m_clipActivatedStates.Add (false);
				}
			}

			m_clipActivatedStates [a_targetIndex] = true;
		}

		public void ResetAll()
		{
			if (m_clipActivatedStates == null)
			{
				return;
			}

			for (int idx = 0; idx < m_clipActivatedStates.Count; idx++)
			{
				m_clipActivatedStates [idx] = false;
			}
		}
	}
}