using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAnimatorCore
{
	public class TransformAnimationStep : TransitionAnimationStep
	{
		new public static string EditorDisplayName { get { return "Transform"; } }

		public override string StepTitleDisplay { get { return EditorDisplayName; } }

		[SerializeField]
		private AnimStepVariableVector3 m_position = new AnimStepVariableVector3(a_offsettingEnabled: true);

		[SerializeField]
		private AnimStepVariableVector3 m_scale = new AnimStepVariableVector3();

		[SerializeField]
		private AnimStepVariableVector3 m_rotation = new AnimStepVariableVector3();

		[SerializeField]
		private RectTransform[] m_targetTransforms;

		private Vector3 m_tmpVec3;


		public AnimStepVariableVector3 Position { get { return m_position; } }
		public AnimStepVariableVector3 Scale { get { return m_scale; } }
		public AnimStepVariableVector3 Rotation { get { return m_rotation; } }

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

			Vector3[] m_masterPositions = new Vector3[a_targetObjects.Length];
			Vector3[] m_masterScales = new Vector3[a_targetObjects.Length];
			Vector3[] m_masterRotations = new Vector3[a_targetObjects.Length];

			for (int idx = 0; idx < a_targetObjects.Length; idx++)
			{
				if (a_targetObjects [idx] == null)
				{
					continue;
				}

				targetTransform = a_targetObjects [idx].GetComponent<RectTransform> ();

				m_targetTransforms [idx] = targetTransform;

				m_masterPositions[idx] = new Vector3(targetTransform.anchoredPosition.x, targetTransform.anchoredPosition.y, targetTransform.localPosition.z);
				m_masterScales [idx] = targetTransform.localScale;
				m_masterRotations[idx] = targetTransform.localRotation.eulerAngles;
			}

			if (!a_onlyMasterValues)
			{
				m_position.Initialise (m_masterPositions);
				m_scale.Initialise (m_masterScales);
				m_rotation.Initialise (m_masterRotations);
			}
			else
			{
				m_position.UpdateMasterValues (m_masterPositions);
				m_scale.UpdateMasterValues (m_masterScales);
				m_rotation.UpdateMasterValues (m_masterRotations);
			}

			return true;
		}

		protected override void SetAnimation(int a_targetIndex, float a_easedProgress)
		{
			if (m_targetTransforms == null || a_targetIndex >= m_targetTransforms.Length || m_targetTransforms[a_targetIndex] == null)
			{
				return;
			}

			m_tmpVec3 = m_position.GetValueLerpedToMaster (a_targetIndex, a_easedProgress);

			m_targetTransforms[a_targetIndex].anchoredPosition = m_tmpVec3;
			m_targetTransforms[a_targetIndex].localPosition = new Vector3 (m_targetTransforms[a_targetIndex].localPosition.x, m_targetTransforms[a_targetIndex].localPosition.y, m_tmpVec3.z);

			m_targetTransforms [a_targetIndex].localScale = m_scale.GetValueLerpedToMaster (a_targetIndex, a_easedProgress);
			m_targetTransforms[a_targetIndex].localRotation = Quaternion.Euler( m_rotation.GetValueLerpedToMaster (a_targetIndex, a_easedProgress) );
		}

#if UNITY_EDITOR
		public override void OnInspectorGUI(int a_stepIndex, AnimSetupType a_animType, SerializedObject a_serializedObject, System.Action a_onAnimatedStateChanged)
		{
			bool setupChanged = false;
			string animVariablePrefix = a_animType == AnimSetupType.Intro ? "Start" : "End";

			m_position.OnInspectorGUI (a_serializedObject.FindProperty ("m_position"), ref setupChanged, NumTargets > 1, new GUIContent(animVariablePrefix + " Position"));
			m_scale.OnInspectorGUI (a_serializedObject.FindProperty ("m_scale"), ref setupChanged, NumTargets > 1, new GUIContent(animVariablePrefix + " Scale"));
			m_rotation.OnInspectorGUI (a_serializedObject.FindProperty ("m_rotation"), ref setupChanged, NumTargets > 1, new GUIContent(animVariablePrefix + " Rotation"));

			if (setupChanged)
			{
				a_onAnimatedStateChanged ();
			}
		}
#endif
	}
}