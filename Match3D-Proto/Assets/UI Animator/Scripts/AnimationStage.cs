using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIAnimatorCore
{
	[System.Serializable]
	public class AnimationStage
	{
		[SerializeField]
		private List<AnimationInstance> m_animationInstances;

		[SerializeField]
		private float m_startDelay = 0;

		[SerializeField]
		private float m_endDelay = 0;

		private double m_timer = 0;
		private bool m_finishedFlag = false;
		private bool m_finishedStage = false;
		private float m_cachedTotalDuration = 0;

#if UNITY_EDITOR
		[SerializeField]
		private bool m_inspectorToggleState = true;

		public bool InspectorToggleState { get { return m_inspectorToggleState; } set { m_inspectorToggleState = value; } }
#endif

		public List<AnimationInstance> AnimationInstances { get { return m_animationInstances; } }

		public float StartDelay { get { return m_startDelay; } }
		public float EndDelay { get { return m_endDelay; } }

		public float GetTotalDuration(AnimSetupType a_animType)
		{
			return m_startDelay + GetInstancesDuration(a_animType) + m_endDelay;
		}

		public float GetInstancesDuration(AnimSetupType a_animType)
		{
			float longestDuration = 0;
			float instanceDuration;

			for (int iIdx = 0; iIdx < m_animationInstances.Count; iIdx++)
			{
				instanceDuration = m_animationInstances [iIdx].GetDuration(a_animType);

				if (instanceDuration > longestDuration)
				{
					longestDuration = instanceDuration;
				}
			}

			return longestDuration;
		}

		public AnimationStage()
		{
			m_animationInstances = new List<AnimationInstance>();
		}

		public void AddNewAnimationInstance(GameObject[] a_animTargets)
		{
			m_animationInstances.Add (new AnimationInstance(a_animTargets));
		}

		public void AddNewAnimationInstance(UIAnimator a_uiAnimator)
		{
			m_animationInstances.Add (new AnimationInstance(a_uiAnimator));
		}

		public void ResetToStart(AnimSetupType a_animType)
		{
			m_timer = 0;
			m_finishedStage = false;
			m_cachedTotalDuration = GetTotalDuration(a_animType);

			for (int iIdx = 0; iIdx < m_animationInstances.Count; iIdx++)
			{
				m_animationInstances [iIdx].ResetToStart (a_animType);
			}
		}

		public void ResetToEnd(AnimSetupType a_animType)
		{
			m_cachedTotalDuration = GetTotalDuration(a_animType);
			m_timer = m_cachedTotalDuration;
			m_finishedStage = true;

			for (int iIdx = 0; iIdx < m_animationInstances.Count; iIdx++)
			{
				m_animationInstances [iIdx].ResetToEnd (a_animType);
			}
		}

		public void SetAnimationTimer( AnimSetupType a_animType, float a_timerValue, bool a_forceLinearTimings = false)
		{
			m_cachedTotalDuration = GetTotalDuration(a_animType);
			m_timer = a_timerValue;

			for (int iIdx = 0; iIdx < m_animationInstances.Count; iIdx++)
			{
				m_animationInstances [iIdx].SetAnimationTimer (a_animType, a_timerValue, a_forceLinearTimings);
			}
		}

		public bool UpdateState(UIAnimator a_uiAnimatorRef, AnimSetupType a_animType, AnimationPlayMode a_playMode, float a_deltaTime)
		{
			m_timer += a_deltaTime;

			if (a_playMode == AnimationPlayMode.CONTINUOUS || m_timer > m_startDelay)
			{
				m_finishedFlag = true;

				if (!m_finishedStage && m_animationInstances != null)
				{
					for (int iIdx = 0; iIdx < m_animationInstances.Count; iIdx++)
					{
						if (!m_animationInstances [iIdx].UpdateState (a_uiAnimatorRef, a_animType, a_playMode, m_timer > m_startDelay ? a_deltaTime : 0))
						{
							m_finishedFlag = false;
						}
					}
				}

				if (m_finishedFlag)
				{
					m_finishedStage = true;
				}

				if (m_finishedStage && m_timer > m_cachedTotalDuration)
				{
					return true;
				}
			}

			return false;
		}
	}
}