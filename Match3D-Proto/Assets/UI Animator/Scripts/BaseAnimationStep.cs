using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAnimatorCore
{
	[System.Serializable]
	public abstract class BaseAnimationStep : MonoBehaviour
	{
		private readonly float MIN_DURATION = 0.001f;

		[SerializeField]
		protected AnimStepVariablePositiveFloat m_delay = new AnimStepVariablePositiveFloat ();

		[SerializeField]
		protected AnimStepVariablePositiveFloat m_duration = new AnimStepVariablePositiveFloat(1);

		[SerializeField]
		protected EasingEquation m_easing = EasingEquation.Linear;

		[SerializeField]
		protected AudioClipData[] m_audioClipDatas = null;

		[SerializeField]
		private int m_numTargets = 1;

		[SerializeField]
		private bool m_waitForPreviousComplete = false;

#if UNITY_EDITOR
		[SerializeField]
		private bool m_grabbedInitialDefaultStateData = false;

		[SerializeField]
		private bool m_inspectorToggleState = true;

		public bool InspectorToggleState { get { return m_inspectorToggleState; } set { m_inspectorToggleState = value; } }
#endif

		private float[] m_timers;

		private UIAnimator m_parentUIAnimator = null;

		public static string EditorDisplayName { get { return "Anim Step"; } }

		public abstract string StepTitleDisplay { get; }

		public virtual bool UseEasing { get { return true; } }

		public virtual bool IsEffectStep { get { return false; } }

		public virtual bool SetCustomInitialDuration { get { return false; } }

		public virtual float CustomInitialDuration { get { return 1; } }

		public int NumTargets { get { return m_numTargets; } }

		public float TotalExecutionDuration { get { return m_delay.LargestValue + Mathf.Max(m_duration.LargestValue, MIN_DURATION); } }

		public bool WaitForPreviousComplete { get { return m_waitForPreviousComplete; } }

		public UIAnimator ParentUIAnimator {
			get {
				if (m_parentUIAnimator == null)
				{
					m_parentUIAnimator = GetComponent<UIAnimator>();
				}
				return m_parentUIAnimator;
			}
		}

		public AudioClipData[] AudioClipDatas { get { return m_audioClipDatas; } }

		public AnimStepVariablePositiveFloat Delay {  get { return m_delay; } }

		public AnimStepVariablePositiveFloat Duration { get { return m_duration; } }

		public float GetTotalExecutionDuration (int a_targetIndex)
		{
			return m_delay.GetValue (a_targetIndex) + Mathf.Max(m_duration.GetValue (a_targetIndex), MIN_DURATION);
		}

		#region public API methods
		public bool HasStarted(int a_targetIndex)
		{
			CheckTimerValueArray ();

			return m_timers [a_targetIndex] > 0;
		}

		public bool IsFinished(int a_targetIndex)
		{
			CheckTimerValueArray ();
			
			return (m_timers[a_targetIndex] - m_delay.GetValue(a_targetIndex)) >= Mathf.Max(m_duration.GetValue(a_targetIndex), MIN_DURATION);
		}

		public bool IsCompletelyFinished()
		{
			CheckTimerValueArray ();

			for (int idx = 0; idx < m_timers.Length; idx++)
			{
				if(!IsFinished(idx))
				{
					return false;
				}
			}

			return true;
		}

		public void SetEasing(EasingEquation a_easing)
		{
			m_easing = a_easing;
		}
		#endregion

		protected abstract void SetAnimation (int a_targetIndex, float a_easedProgress);

		protected abstract bool SetStepAllValuesFromCurrentState (GameObject[] a_targetObjects);

		protected abstract bool SetStepDefaultValuesFromCurrentState (GameObject[] a_targetObjects);

		public void SetNumTargets(int a_numTargets)
		{
			m_numTargets = a_numTargets;

			CheckTimerValueArray ();
		}

		public bool SetAllValuesFromCurrentState (GameObject[] a_targetObjects)
		{
			m_numTargets = a_targetObjects.Length;

			CheckTimerValueArray ();

			InitBaseVariables (m_numTargets);

			return SetStepAllValuesFromCurrentState (a_targetObjects);
		}

		public bool SetDefaultValuesFromCurrentState (GameObject[] a_targetObjects)
		{
			m_numTargets = a_targetObjects.Length;

			CheckTimerValueArray ();

			InitBaseVariables (m_numTargets);

			return SetStepDefaultValuesFromCurrentState (a_targetObjects);
		}

		private void CheckTimerValueArray ()
		{
			if (m_timers == null || m_timers.Length != m_numTargets)
			{
				m_timers = new float[m_numTargets];
			}
		}

		private void SetTimerValues(float a_value)
		{
			CheckTimerValueArray ();

			for (int idx = 0; idx < m_timers.Length; idx++)
			{
				m_timers [idx] = a_value;
			}
		}

		private void InitBaseVariables (int a_numTargets)
		{
			m_delay.Initialise (a_numTargets);
			m_duration.Initialise (a_numTargets);

			if (m_audioClipDatas != null)
			{
				// Make sure to initialise all audioClipData's with latest numTargets
				for (int idx = 0; idx < m_audioClipDatas.Length; idx++)
				{
					m_audioClipDatas [idx].UpdateNumTargets (a_numTargets);
				}
			}
		}

		public void ResetToStart(AnimSetupType a_animType)
		{
			for (int idx = 0; idx < m_numTargets; idx++)
			{
				SetAnimationTimer (idx, a_animType, 0);
			}

			// Reset AudioClipData's
			if (m_audioClipDatas != null)
			{
				for (int idx = 0; idx < m_audioClipDatas.Length; idx++)
				{
					m_audioClipDatas [idx].ResetAll ();
				}
			}
		}

		public void ResetToEnd (AnimSetupType a_animType)
		{
			SetTimerValues( TotalExecutionDuration );

			for (int idx = 0; idx < m_numTargets; idx++)
			{
				SetAnimationProgress (idx, a_animType, 1);
			}
		}

		public void SetToMasterState()
		{
			for (int idx = 0; idx < m_numTargets; idx++)
			{
				SetAnimationTimer (idx, AnimSetupType.Outro, 0);
			}
		}

		public void SetAsMasterState(GameObject[] a_targetObjects)
		{
			SetDefaultValuesFromCurrentState (a_targetObjects);

			SetToMasterState ();
		}

		public void UpdateState( UIAnimator a_uiAnimatorRef, int a_targetIndex, AnimSetupType a_animType, AnimationPlayMode a_playMode, float a_deltaTime)
		{
			m_timers[a_targetIndex] += a_deltaTime;

			if (a_playMode == AnimationPlayMode.OPTIMAL && m_delay.GetValue(a_targetIndex) > 0 && m_timers[a_targetIndex] <= m_delay.GetValue(a_targetIndex))
			{
				// Animation is still waiting the allocated 'delay'
				return;
			}

			float postDelayStepTimer = (m_timers [a_targetIndex] - m_delay.GetValue (a_targetIndex));

			if (m_audioClipDatas != null && m_audioClipDatas.Length > 0)
			{
				// Check for whether an audio clip needs to be played
				for (int idx = 0; idx < m_audioClipDatas.Length; idx++)
				{
					if (m_audioClipDatas [idx].Clip != null && !m_audioClipDatas [idx].HasAudioClipActivated(a_targetIndex))
					{
						if((m_audioClipDatas [idx].TriggerPoint == AudioClipData.CLIP_TRIGGER_POINT.START_OF_ANIM_STEP && postDelayStepTimer > m_audioClipDatas [idx].Delay.GetValue(a_targetIndex))
							|| (m_audioClipDatas [idx].TriggerPoint == AudioClipData.CLIP_TRIGGER_POINT.END_OF_ANIM_STEP && postDelayStepTimer > m_duration.GetValue (a_targetIndex) - m_audioClipDatas [idx].Delay.GetValue(a_targetIndex)))
						{
							a_uiAnimatorRef.TriggerAudioClip (m_audioClipDatas [idx], a_targetIndex);
							
							m_audioClipDatas [idx].MarkAudioClipActivated (a_targetIndex);
						}
					}
				}
			}

			SetAnimationProgress (a_targetIndex, a_animType, postDelayStepTimer / Mathf.Max(m_duration.GetValue(a_targetIndex), MIN_DURATION));
		}

		public void SetAnimationProgress (int a_targetIndex, AnimSetupType a_animType, float a_progress)
		{
			if (!IsEffectStep && a_animType == AnimSetupType.Outro)
			{
				a_progress = 1f - a_progress;
			}

			if (a_progress < 0)
			{
				a_progress = 0;
			}
			else if(a_progress > 1)
			{
				a_progress = 1;
			}

			if (UseEasing)
			{
				a_progress = EasingManager.GetEaseProgress (m_easing, (float) a_progress);
			}

			SetAnimation (a_targetIndex, a_progress);
		}

		public void SetAnimationTimer(int a_targetIndex, AnimSetupType a_animType, float a_timerValue, bool a_forceLinearTimings = false)
		{
			CheckTimerValueArray ();

			m_timers [a_targetIndex] = a_timerValue;

			if (a_timerValue - m_delay.GetValue(a_forceLinearTimings ? 0 : a_targetIndex) < 0)
			{
				SetAnimationProgress (a_targetIndex, a_animType, 0);
			}
			else
			{
				SetAnimationProgress (a_targetIndex, a_animType, (a_timerValue - m_delay.GetValue(a_forceLinearTimings ? 0 : a_targetIndex)) / Mathf.Max(m_duration.GetValue(a_forceLinearTimings ? 0 : a_targetIndex), MIN_DURATION));
			}
		}

#if UNITY_EDITOR
		public void BaseOnInspectorGUI(GameObject[] a_targetObjects, int a_stepIndex, AnimSetupType a_animType, SerializedObject a_serializedObject, System.Action a_onAnimatedStateChanged)
		{
			if (m_grabbedInitialDefaultStateData == false)
			{
				m_duration = new AnimStepVariablePositiveFloat(SetCustomInitialDuration ? CustomInitialDuration : 1);

				m_grabbedInitialDefaultStateData = SetAllValuesFromCurrentState (a_targetObjects);

				a_serializedObject.Update ();
			}

			if(a_serializedObject.targetObject.GetType() != GetType())
			{
				return;
			}

			if (a_stepIndex > 0)
			{
				EditorGUILayout.PropertyField (a_serializedObject.FindProperty ("m_waitForPreviousComplete"), new GUIContent("Wait For All Finish?"));
			}

			OnInspectorGUI (
				a_stepIndex,
				a_animType,
				a_serializedObject,
				a_onAnimatedStateChanged: () =>
				{
					a_serializedObject.ApplyModifiedProperties ();
					a_onAnimatedStateChanged();
				}
			);

			bool setupChanged = false;

			m_delay.OnInspectorGUI (a_serializedObject.FindProperty ("m_delay"), ref setupChanged, m_numTargets > 1, new GUIContent("Delay"));
			m_duration.OnInspectorGUI (a_serializedObject.FindProperty ("m_duration"), ref setupChanged, m_numTargets > 1, new GUIContent("Duration"));

			if (UseEasing)
			{
				EditorGUILayout.PropertyField (a_serializedObject.FindProperty ("m_easing"));
			}

			EditorGUILayout.BeginHorizontal ();

			EditorGUILayout.LabelField ("Add Audio Clip?", GUILayout.Width(95));

			SerializedProperty audioClipDatasProperty = a_serializedObject.FindProperty ("m_audioClipDatas");

			if (GUILayout.Button ("+", GUILayout.Width(20), GUILayout.Height(14)))
			{
				audioClipDatasProperty.InsertArrayElementAtIndex (audioClipDatasProperty.arraySize);

				a_serializedObject.ApplyModifiedProperties ();

				EditorGUILayout.EndHorizontal ();

				m_audioClipDatas [m_audioClipDatas.Length - 1].Init (m_numTargets);

				return;
			}

			EditorGUILayout.EndHorizontal ();

			if(audioClipDatasProperty.arraySize > 0)
			{
				SerializedProperty audioClipDataInstanceProperty;
				bool instanceDeleted = false;

				EditorGUILayout.BeginVertical (EditorStyles.helpBox);

				for (int idx = 0; idx < audioClipDatasProperty.arraySize; idx++)
				{
					instanceDeleted = false;
					audioClipDataInstanceProperty = audioClipDatasProperty.GetArrayElementAtIndex (idx);

					EditorGUILayout.BeginHorizontal ();

					SerializedProperty foldoutStateProperty = audioClipDataInstanceProperty.FindPropertyRelative ("m_editorFoldoutState");
					foldoutStateProperty.boolValue = EditorGUILayout.Foldout (foldoutStateProperty.boolValue, "Audio Clip #" + (idx+1), true);

					if (GUILayout.Button ("x", GUILayout.Width(20), GUILayout.Height(14)))
					{
						audioClipDatasProperty.DeleteArrayElementAtIndex (idx);
						instanceDeleted = true;
					}

					EditorGUILayout.EndHorizontal ();

					if (!instanceDeleted && foldoutStateProperty.boolValue)
					{
						EditorGUILayout.PropertyField (audioClipDataInstanceProperty.FindPropertyRelative ("m_clip"));

						SerializedProperty triggerPointProperty = audioClipDataInstanceProperty.FindPropertyRelative ("m_triggerPoint");

						EditorGUILayout.PropertyField (triggerPointProperty, new GUIContent("Play When?"));

						GUIContent offsetLabelContent;

						if (((AudioClipData.CLIP_TRIGGER_POINT)triggerPointProperty.enumValueIndex) == AudioClipData.CLIP_TRIGGER_POINT.START_OF_ANIM_STEP)
						{
							offsetLabelContent = new GUIContent ("Delay Time");
						}
						else
						{
							offsetLabelContent = new GUIContent ("Offset Time");
						}

						m_audioClipDatas[idx].Delay.OnInspectorGUI (audioClipDataInstanceProperty.FindPropertyRelative ("m_delay"), ref setupChanged, m_numTargets > 1, offsetLabelContent);

						m_audioClipDatas[idx].OffsetTime.OnInspectorGUI (audioClipDataInstanceProperty.FindPropertyRelative ("m_offsetTime"), ref setupChanged, m_numTargets > 1, new GUIContent("Clip Offset"));

						m_audioClipDatas[idx].Pitch.OnInspectorGUI (audioClipDataInstanceProperty.FindPropertyRelative ("m_pitch"), ref setupChanged, m_numTargets > 1, new GUIContent("Pitch"));
						m_audioClipDatas[idx].Volume.OnInspectorGUI (audioClipDataInstanceProperty.FindPropertyRelative ("m_volume"), ref setupChanged, m_numTargets > 1, new GUIContent("Volume"));
					}
				}

				EditorGUILayout.EndVertical ();
			}

			a_serializedObject.ApplyModifiedProperties ();
		}

		public abstract void OnInspectorGUI (int a_stepIndex, AnimSetupType a_animType, SerializedObject a_serializedObject, System.Action a_onAnimatedStateChanged);

		protected bool DrawPropertyField(SerializedObject a_serializedObject, string a_propertyName, ref bool a_hasChanged, GUIContent a_customLabel = null, bool a_showLabel = true, params GUILayoutOption[] a_options)
		{
			SerializedProperty sProperty = a_serializedObject.FindProperty (a_propertyName);

			if (sProperty != null)
			{
				EditorGUI.BeginChangeCheck();
				
				if (a_customLabel != null)
				{
					EditorGUILayout.PropertyField (sProperty, a_customLabel, a_options);
				}
				else
				{
					if (!a_showLabel)
					{
						EditorGUILayout.PropertyField (sProperty, GUIContent.none, a_options);
					}
					else
					{
						EditorGUILayout.PropertyField (sProperty, a_options);
					}
				}
				
				if (EditorGUI.EndChangeCheck ())
				{
					a_hasChanged = true;
					return true;
				}
			}

			return false;
		}

		protected bool DrawAnimStepVariablePropertyField(SerializedObject a_serializedObject, string a_propertyName, ref bool a_hasChanged, GUIContent a_customLabel = null, bool a_showLabel = true, params GUILayoutOption[] a_options)
		{
			SerializedProperty sProperty = a_serializedObject.FindProperty (a_propertyName);

			if (sProperty != null)
			{
				SerializedProperty variableProperty = sProperty.FindPropertyRelative ("m_animatedFrom");

				EditorGUI.BeginChangeCheck();

				if (a_customLabel != null)
				{
					EditorGUILayout.PropertyField (variableProperty, a_customLabel, a_options);
				}
				else
				{
					if (!a_showLabel)
					{
						EditorGUILayout.PropertyField (variableProperty, GUIContent.none, a_options);
					}
					else
					{
						EditorGUILayout.PropertyField (variableProperty, a_options);
					}
				}

				if (EditorGUI.EndChangeCheck ())
				{
					a_hasChanged = true;
					return true;
				}
			}

			return false;
		}
#endif
	}
}