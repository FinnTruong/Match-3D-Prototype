using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAnimatorCore
{
	public class PulseAnimationStep : EffectAnimationStep
	{
		new public static string EditorDisplayName { get { return "Pulse"; } }

		public override string StepTitleDisplay { get { return EditorDisplayName; } }

		public override bool UseEasing { get { return false; } }

		public override bool SetCustomInitialDuration { get { return true; } }

		public override float CustomInitialDuration { get { return 0.2f; } }

		[SerializeField]
		private AnimStepVariablePositiveFloat m_pulseScale = new AnimStepVariablePositiveFloat(1.25f);

		[SerializeField]
		private AnimStepVariableFloat m_rotationAngle = new AnimStepVariableFloat(0);

		[SerializeField]
		private Vector3[] m_masterScales;

		[SerializeField]
		private Vector3[] m_masterRotations;

		[SerializeField]
		private RectTransform[] m_targetTransforms;

		private float m_tempEasedProgress;

		public AnimStepVariablePositiveFloat PulseScale {  get { return m_pulseScale; } }
		public AnimStepVariableFloat RotationAngle { get { return m_rotationAngle; } }

		protected override bool SetStepAllValuesFromCurrentState(GameObject[] a_targetObjects)
		{
			return InitialiseVariableStates (a_targetObjects, a_onlyMasterValues: false);
		}

		protected override bool SetStepDefaultValuesFromCurrentState(GameObject[] a_targetObjects)
		{
			return InitialiseVariableStates (a_targetObjects, a_onlyMasterValues: true);
		}

		private bool InitialiseVariableStates(GameObject[] a_targetObjects, bool a_onlyMasterValues)
		{
			m_targetTransforms = new RectTransform[a_targetObjects.Length];

			if (a_targetObjects.Length == 0)
			{
				return false;
			}

			m_pulseScale.Initialise (a_targetObjects.Length);
			m_rotationAngle.Initialise (a_targetObjects.Length);

			RectTransform targetTransform;

			m_masterScales = new Vector3[a_targetObjects.Length];
			m_masterRotations = new Vector3[a_targetObjects.Length];

			for (int idx = 0; idx < a_targetObjects.Length; idx++)
			{
				if (a_targetObjects [idx] == null)
				{
					continue;
				}

				targetTransform = a_targetObjects [idx].GetComponent<RectTransform> ();

				m_targetTransforms [idx] = targetTransform;

				m_masterScales [idx] = targetTransform.localScale;
				m_masterRotations[idx] = targetTransform.localRotation.eulerAngles;
			}

			return true;
		}

		protected override void SetAnimation(int a_targetIndex, float a_easedProgress)
		{
			if (m_targetTransforms == null || a_targetIndex >= m_targetTransforms.Length || m_targetTransforms[a_targetIndex] == null)
			{
				return;
			}

			if (a_easedProgress < 0.5f)
			{
				m_tempEasedProgress = EasingManager.GetEaseProgress (EasingEquation.SineEaseOut, (float)a_easedProgress/0.5f);
				
				m_targetTransforms[a_targetIndex].localScale = Vector3.LerpUnclamped (m_masterScales[a_targetIndex], m_masterScales[a_targetIndex] * m_pulseScale.GetValue(a_targetIndex), m_tempEasedProgress);
				m_targetTransforms[a_targetIndex].localRotation = Quaternion.Euler(Vector3.LerpUnclamped (m_masterRotations[a_targetIndex], m_masterRotations[a_targetIndex] + new Vector3(0,0,m_rotationAngle.GetValue(a_targetIndex)), m_tempEasedProgress));
			}
			else
			{
				m_tempEasedProgress = EasingManager.GetEaseProgress (EasingEquation.SineEaseIn, (float)(a_easedProgress - 0.5f) / 0.5f);
				
				m_targetTransforms[a_targetIndex].localScale = Vector3.LerpUnclamped (m_masterScales[a_targetIndex] * m_pulseScale.GetValue(a_targetIndex), m_masterScales[a_targetIndex], m_tempEasedProgress);
				m_targetTransforms[a_targetIndex].localRotation = Quaternion.Euler(Vector3.LerpUnclamped (m_masterRotations[a_targetIndex] + new Vector3(0,0,m_rotationAngle.GetValue(a_targetIndex)), m_masterRotations[a_targetIndex], m_tempEasedProgress));
			}
		}

#if UNITY_EDITOR
		public override void OnInspectorGUI(int a_stepIndex, AnimSetupType a_animType, SerializedObject a_serializedObject, System.Action a_onAnimatedStateChanged)
		{
			bool setupChanged = false;

			m_pulseScale.OnInspectorGUI (a_serializedObject.FindProperty("m_pulseScale"), ref setupChanged, NumTargets > 1, new GUIContent("Pulse Scale"));

			m_rotationAngle.OnInspectorGUI (a_serializedObject.FindProperty("m_rotationAngle"), ref setupChanged, NumTargets > 1, new GUIContent("Rotation Angle"));

			if (setupChanged)
			{
				a_onAnimatedStateChanged ();
			}
		}
#endif
	}
}