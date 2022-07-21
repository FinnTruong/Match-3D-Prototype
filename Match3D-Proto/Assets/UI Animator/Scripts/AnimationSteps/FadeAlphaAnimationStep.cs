using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAnimatorCore
{
	public class FadeAlphaAnimationStep : TransitionAnimationStep
	{
		new public static string EditorDisplayName { get { return "Fade"; } }

		public override string StepTitleDisplay { get { return EditorDisplayName; } }

		[SerializeField]
		private UIGraphicRendererTargetData[] m_targetGraphicsData;

		[SerializeField]
		[Range(0,1)]
		private float m_animatedAlpha = 0.2f;

		protected override bool SetStepAllValuesFromCurrentState(GameObject[] a_targetObjects)
		{
			GrabRendererReferences (a_targetObjects);

			return true;
		}

		protected override bool SetStepDefaultValuesFromCurrentState(GameObject[] a_targetObjects)
		{
			GrabRendererReferences (a_targetObjects);

			return true;
		}

		protected override void SetAnimation(int a_targetIndex, float a_easedProgress)
		{
			if (m_targetGraphicsData == null)
			{
				return;
			}

			m_targetGraphicsData [a_targetIndex].SetFadeProgress (m_animatedAlpha, a_easedProgress);
		}

		private void GrabRendererReferences(GameObject[] a_targetObjects)
		{
			// TODO: Stop this always getting called twice on adding a new step

			m_targetGraphicsData = new UIGraphicRendererTargetData[a_targetObjects.Length];

			for (int tIdx = 0; tIdx < a_targetObjects.Length; tIdx++)
			{
				if (a_targetObjects [tIdx] == null)
				{
					continue;
				}

				m_targetGraphicsData [tIdx] = new UIGraphicRendererTargetData ( a_targetObjects[tIdx].GetComponentsInChildren<Graphic> (true) );
			}
		}

		#region public API methods
		public void SetAlpha(float a_value)
		{
			m_animatedAlpha = Mathf.Clamp01(a_value);
		}
		#endregion

#if UNITY_EDITOR
		public override void OnInspectorGUI(int a_stepIndex, AnimSetupType a_animType, SerializedObject a_serializedObject, System.Action a_onAnimatedStateChanged)
		{
			bool stateChanged = false;

			string namePrefix = a_animType == AnimSetupType.Intro ? "Start" : "End";

			DrawPropertyField(a_serializedObject, "m_animatedAlpha", ref stateChanged, new GUIContent(namePrefix + " Alpha"));

			if (stateChanged)
			{
				a_onAnimatedStateChanged ();
			}
		}
#endif
	}
}
