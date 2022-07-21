using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAnimatorCore
{
	public class PopAnimationStep : TransitionAnimationStep
	{
		new public static string EditorDisplayName { get { return "Pop"; } }

		public override string StepTitleDisplay { get { return EditorDisplayName; } }

		public override bool SetCustomInitialDuration { get { return true; } }

		public override float CustomInitialDuration { get { return 0.5f; } }

		public override bool UseEasing { get { return false; } }

		private AnimationCurve m_animCurve = new AnimationCurve(new Keyframe(0,0), new Keyframe(0.66f,1.22f), new Keyframe(1,1));

		[SerializeField]
		private Vector3[] m_masterScales;

		[SerializeField]
		private RectTransform[] m_targetTransforms;

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

			RectTransform targetTransform;

			m_masterScales = new Vector3[a_targetObjects.Length];

			for (int idx = 0; idx < a_targetObjects.Length; idx++)
			{
				if (a_targetObjects [idx] == null)
				{
					continue;
				}

				targetTransform = a_targetObjects [idx].GetComponent<RectTransform> ();

				m_targetTransforms [idx] = targetTransform;

				m_masterScales [idx] = targetTransform.localScale;
			}

			return true;
		}

		protected override void SetAnimation(int a_targetIndex, float a_easedProgress)
		{
			if (m_targetTransforms == null || a_targetIndex >= m_targetTransforms.Length || m_targetTransforms[a_targetIndex] == null)
			{
				return;
			}

			m_targetTransforms [a_targetIndex].localScale = Vector3.LerpUnclamped(Vector3.zero, m_masterScales[a_targetIndex], m_animCurve.Evaluate (a_easedProgress));
		}

#if UNITY_EDITOR
		public override void OnInspectorGUI(int a_stepIndex, AnimSetupType a_animType, SerializedObject a_serializedObject, System.Action a_onAnimatedStateChanged)
		{
		}
#endif
	}
}