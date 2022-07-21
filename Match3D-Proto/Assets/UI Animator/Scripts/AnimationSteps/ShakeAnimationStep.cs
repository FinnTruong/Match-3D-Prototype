using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UIAnimatorCore
{
	public class ShakeAnimationStep : EffectAnimationStep
	{
		new public static string EditorDisplayName { get { return "Shake"; } }

		public override string StepTitleDisplay { get { return EditorDisplayName; } }

		public override bool UseEasing { get { return false; } }

		public override bool SetCustomInitialDuration { get { return true; } }

		public override float CustomInitialDuration { get { return 0.3f; } }

		[SerializeField]
		private AnimStepVariablePositiveFloat m_shakeAmount = new AnimStepVariablePositiveFloat(2.5f);

		[SerializeField]
		private Vector2[] m_masterPositions;

		[SerializeField]
		private RectTransform[] m_targetTransforms;

		public AnimStepVariablePositiveFloat ShakeAmount {  get { return m_shakeAmount; } }

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

			m_shakeAmount.Initialise (a_targetObjects.Length);

			RectTransform targetTransform;

			m_masterPositions = new Vector2[a_targetObjects.Length];

			for (int idx = 0; idx < a_targetObjects.Length; idx++)
			{
				if (a_targetObjects [idx] == null)
				{
					continue;
				}

				targetTransform = a_targetObjects [idx].GetComponent<RectTransform> ();

				m_targetTransforms [idx] = targetTransform;

				m_masterPositions[idx] = new Vector2(targetTransform.anchoredPosition.x, targetTransform.anchoredPosition.y);
			}

			return true;
		}

		protected override void SetAnimation(int a_targetIndex, float a_easedProgress)
		{
			if (m_targetTransforms == null || a_targetIndex >= m_targetTransforms.Length || m_targetTransforms[a_targetIndex] == null)
			{
				return;
			}

			if (a_easedProgress == 0 || a_easedProgress == 1)
			{
				m_targetTransforms[a_targetIndex].anchoredPosition = m_masterPositions[a_targetIndex];
			}
			else
			{
				m_targetTransforms[a_targetIndex].anchoredPosition = m_masterPositions[a_targetIndex] + Random.insideUnitCircle * m_shakeAmount.GetValue(a_targetIndex);
			}
		}

#if UNITY_EDITOR
		public override void OnInspectorGUI(int a_stepIndex, AnimSetupType a_animType, SerializedObject a_serializedObject, System.Action a_onAnimatedStateChanged)
		{
			bool setupChanged = false;

			m_shakeAmount.OnInspectorGUI (a_serializedObject.FindProperty ("m_shakeAmount"), ref setupChanged, NumTargets > 1, new GUIContent("Shake Amount"));

			if (setupChanged)
			{
				a_onAnimatedStateChanged ();
			}
		}
#endif
	}
}