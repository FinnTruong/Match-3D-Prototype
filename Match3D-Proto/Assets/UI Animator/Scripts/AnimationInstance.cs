using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIAnimatorCore
{
	[System.Serializable]
	public class AnimationInstance
	{
		[SerializeField]
		private GameObject[] m_targetObjects;

		[SerializeField]
		private List<BaseAnimationStep> m_animationSteps;

		[SerializeField]
		private int[] m_targetStepIndexes;

		[SerializeField]
		private UIAnimator m_uiAnimatorSubModule;

		[SerializeField]
		private float m_startDelay = 0;

#if UNITY_EDITOR
		[SerializeField]
		private bool m_inspectorToggleState = true;

		public bool InspectorToggleState { get { return m_inspectorToggleState; } set { m_inspectorToggleState = value; } }
#endif

		private double m_timer = 0;
		private bool m_finishedFlag = false;

		public GameObject[] TargetObjects { get { return m_targetObjects; } }
		public List<BaseAnimationStep> AnimationSteps { get { return m_animationSteps; } }
		public bool IsUiAnimatorSubModule { get { return m_uiAnimatorSubModule != null; } }

		public string Title {
			get {
				if (IsUiAnimatorSubModule)
				{
					return "[UI Animator] '" + m_uiAnimatorSubModule.name + "'";
				}

				string titleString = string.Empty;

				GameObject targetGO;
				if (m_targetObjects != null)
				{
					for (int tIdx = 0; tIdx < m_targetObjects.Length; tIdx++)
					{
						targetGO = m_targetObjects [tIdx];

						if (targetGO != null)
						{
							titleString += (tIdx > 0 ? ", " : "") + "'" + targetGO.name + "'";
						}
					}
				}

				if(titleString == string.Empty)
				{
					titleString = "*No Target*";
				}

				return titleString;
			}
		}

		public AnimationInstance(GameObject[] a_animInstanceTargets)
		{
			m_targetObjects = a_animInstanceTargets;
			CheckTargetIndexesArray();
			m_animationSteps = new List<BaseAnimationStep>();
		}

		public AnimationInstance(UIAnimator a_uiAnimatorSubModule)
		{
			m_uiAnimatorSubModule = a_uiAnimatorSubModule;
		}

		public float GetDuration(AnimSetupType a_animType)
		{
			float duration = m_startDelay;

			if (IsUiAnimatorSubModule)
			{
				duration += m_uiAnimatorSubModule.GetAnimationDuration (a_animType);
			}
			else
			{
				for (int sIdx = 0; sIdx < m_animationSteps.Count; sIdx++)
				{
					if (m_animationSteps [sIdx] != null)
					{
						duration += m_animationSteps [sIdx].TotalExecutionDuration;
					}
				}
			}

			return duration;
		}

		public void AddNewAnimationStep(BaseAnimationStep a_newAnimStep)
		{
			m_animationSteps.Add (a_newAnimStep);
		}

		public void AddNewAnimationStep(BaseAnimationStep a_newAnimStep, int a_index)
		{
			m_animationSteps.Insert (a_index, a_newAnimStep);
		}

		public void ResetToStart(AnimSetupType a_animType)
		{
			m_timer = 0;

			if (IsUiAnimatorSubModule)
			{
				m_uiAnimatorSubModule.ResetToStart (a_animType);
			}
			else
			{
				if (m_animationSteps != null)
				{
					for (int sIdx = m_animationSteps.Count - 1; sIdx >= 0; sIdx--)
					{
						m_animationSteps [sIdx].ResetToStart (a_animType);
					}
				}
				
				CheckTargetIndexesArray ();
				
				for (int idx = 0; idx < m_targetStepIndexes.Length; idx++)
				{
					m_targetStepIndexes [idx] = 0;
				}
			}

		}

		public void ResetToEnd(AnimSetupType a_animType)
		{
			m_timer = GetDuration(a_animType);

			if (IsUiAnimatorSubModule)
			{
				m_uiAnimatorSubModule.ResetToEnd (a_animType);
			}
			else
			{
				if (m_animationSteps != null)
				{
					for (int aIdx = 0; aIdx < m_animationSteps.Count; aIdx++)
					{
						m_animationSteps [aIdx].ResetToEnd (a_animType);
					}
				}

				CheckTargetIndexesArray ();

				for (int idx = 0; idx < m_targetStepIndexes.Length; idx++)
				{
					m_targetStepIndexes [idx] = m_animationSteps.Count;
				}
			}
		}

		public void SetAsMasterState()
		{
			if (!IsUiAnimatorSubModule)
			{
				if (m_animationSteps != null)
				{
					for (int aIdx = 0; aIdx < m_animationSteps.Count; aIdx++)
					{
						m_animationSteps [aIdx].SetAsMasterState (m_targetObjects);
					}
				}
			}
		}

		public bool UpdateState(UIAnimator a_uiAnimatorRef, AnimSetupType a_animType, AnimationPlayMode a_playMode, float a_deltaTime)
		{
			m_timer += a_deltaTime;

			if (a_playMode == AnimationPlayMode.CONTINUOUS || m_timer > m_startDelay)
			{
				if (IsUiAnimatorSubModule)
				{
					return m_uiAnimatorSubModule.UpdateState (a_animType, a_deltaTime, a_isPrimaryAnimator: false);
				}

				m_finishedFlag = true;

				for (int tIdx = 0; tIdx < m_targetObjects.Length; tIdx++)
				{
					if (m_targetStepIndexes [tIdx] < m_animationSteps.Count)
					{
						if (m_targetStepIndexes [tIdx] > 0 && m_animationSteps [m_targetStepIndexes [tIdx]].WaitForPreviousComplete && !m_animationSteps [m_targetStepIndexes [tIdx]].HasStarted (tIdx))
						{
							if (!m_animationSteps [m_targetStepIndexes [tIdx] - 1].IsCompletelyFinished ())
							{
								// Need to wait until previous anim step is completely finished
								m_finishedFlag = false;
								continue;
							}
						}

						m_animationSteps [m_targetStepIndexes [tIdx]].UpdateState (a_uiAnimatorRef, tIdx, a_animType, a_playMode, m_timer > m_startDelay ? a_deltaTime : 0);

						if (m_animationSteps [m_targetStepIndexes [tIdx]].IsFinished (tIdx))
						{
							// Finished this anim step, progress to next
							m_targetStepIndexes [tIdx]++;
						}

						m_finishedFlag = false;
					}
				}

				return m_finishedFlag;
			}

			return false;
		}

		public void SetAnimationTimer( AnimSetupType a_animType, float a_timerValue, bool a_forceLinearTimings = false)
		{
			m_timer = a_timerValue;

			if (IsUiAnimatorSubModule)
			{
				m_uiAnimatorSubModule.SetAnimationTimer (a_animType, a_timerValue - m_startDelay);
				return;
			}

			float offset;

			for (int tIdx = 0; tIdx < m_targetObjects.Length; tIdx++)
			{
				offset = m_startDelay;

				for (int aIdx = 0; aIdx < m_animationSteps.Count; aIdx++)
				{
					if (aIdx > 0)
					{
						if (a_forceLinearTimings || m_animationSteps [aIdx].WaitForPreviousComplete)
						{
							offset += m_animationSteps [aIdx - 1].TotalExecutionDuration;
						}
						else
						{
							offset += m_animationSteps [aIdx - 1].GetTotalExecutionDuration(tIdx);
						}
					}

					if (a_timerValue - offset >= 0 || !m_animationSteps [aIdx].IsEffectStep)
					{
						m_animationSteps [aIdx].SetAnimationTimer (tIdx, a_animType, a_timerValue - offset, a_forceLinearTimings);
					}
				}
			}
		}

		public void SetAllValuesFromCurrentState()
		{
			if (IsUiAnimatorSubModule || m_animationSteps == null)
			{
				return;
			}

			CheckTargetIndexesArray ();

			for (int idx = 0; idx < m_animationSteps.Count; idx++)
			{
				m_animationSteps [idx].SetAllValuesFromCurrentState (m_targetObjects);
			}
		}

		public void SetDefaultValuesFromCurrentState()
		{
			if (IsUiAnimatorSubModule || m_animationSteps == null)
			{
				return;
			}

			CheckTargetIndexesArray ();

			for (int idx = 0; idx < m_animationSteps.Count; idx++)
			{
				m_animationSteps [idx].SetDefaultValuesFromCurrentState (m_targetObjects);
			}
		}

		public void RefreshNumTargetsData()
		{
			if (IsUiAnimatorSubModule || m_animationSteps == null)
			{
				return;
			}

			CheckTargetIndexesArray ();

			for (int idx = 0; idx < m_animationSteps.Count; idx++)
			{
				m_animationSteps [idx].SetNumTargets (m_targetObjects.Length);
			}
		}

		private void CheckTargetIndexesArray()
		{
			if (m_targetStepIndexes == null || m_targetStepIndexes.Length != m_targetObjects.Length)
			{
				m_targetStepIndexes = new int[m_targetObjects.Length];
			}
		}
	}
}