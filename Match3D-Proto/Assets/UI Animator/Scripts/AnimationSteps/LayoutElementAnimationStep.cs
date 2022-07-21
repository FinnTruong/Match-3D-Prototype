using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAnimatorCore
{
	public class LayoutElementAnimationStep : TransitionAnimationStep
	{
		new public static string EditorDisplayName { get { return "LayoutElement Animator"; } }

		public override string StepTitleDisplay { get { return EditorDisplayName; } }

		[SerializeField]
		private LayoutElementParam m_widthParamToAnimate = LayoutElementParam.MIN;

		[SerializeField]
		private LayoutElementParam m_heightParamToAnimate = LayoutElementParam.MIN;

		[SerializeField]
		private AnimStepVariablePositiveFloat m_widthScale = new AnimStepVariablePositiveFloat( a_startValue: 1 );

		[SerializeField]
		private AnimStepVariablePositiveFloat m_heightScale = new AnimStepVariablePositiveFloat( a_startValue: 1 );

		[SerializeField]
		private LayoutElement[] m_targetLayoutElements;

		[SerializeField] float[] masterMinWidths;
		[SerializeField] float[] masterMinHeights;
		[SerializeField] float[] masterPrefferedWidths;
		[SerializeField] float[] masterPrefferedHeights;
		[SerializeField] float[] masterFlexibleWidths;
		[SerializeField] float[] masterFlexibleHeights;

#if UNITY_EDITOR
		[SerializeField] bool m_validSetup = true;
#endif

		private Vector3 m_tmpVec3;

		public AnimStepVariablePositiveFloat WidthScale { get { return m_widthScale; } }
		public AnimStepVariablePositiveFloat HeightScale { get { return m_heightScale; } }

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
			m_targetLayoutElements = new LayoutElement[a_targetObjects.Length];

			if (a_targetObjects.Length == 0)
			{
#if UNITY_EDITOR
				m_validSetup = true;
#endif
				return false;
			}

			LayoutElement targetLayoutElement;

#if UNITY_EDITOR
			m_validSetup = false;
#endif

			masterMinWidths = new float[a_targetObjects.Length];
			masterMinHeights = new float[a_targetObjects.Length];
			masterPrefferedWidths = new float[a_targetObjects.Length];
			masterPrefferedHeights = new float[a_targetObjects.Length];
			masterFlexibleWidths = new float[a_targetObjects.Length];
			masterFlexibleHeights = new float[a_targetObjects.Length];

			for (int idx = 0; idx < a_targetObjects.Length; idx++)
			{
				if (a_targetObjects [idx] == null)
				{
					continue;
				}

				targetLayoutElement = a_targetObjects [idx].GetComponent<LayoutElement> ();

				if(targetLayoutElement == null)
				{
					continue;
				}

				m_targetLayoutElements [idx] = targetLayoutElement;

				masterMinWidths[idx] = targetLayoutElement.minWidth;
				masterMinHeights[idx] = targetLayoutElement.minHeight;
				masterPrefferedWidths[idx] = targetLayoutElement.preferredWidth;
				masterPrefferedHeights[idx] = targetLayoutElement.preferredHeight;
				masterFlexibleWidths[idx] = targetLayoutElement.flexibleWidth;
				masterFlexibleHeights[idx] = targetLayoutElement.flexibleHeight;
#if UNITY_EDITOR
				m_validSetup = true;
#endif
			}

			
			if (!a_onlyMasterValues)
			{
				m_widthScale.Initialise(a_targetObjects.Length, 1, 1);
				m_heightScale.Initialise(a_targetObjects.Length, 1, 1);
			}
			else
			{
				m_widthScale.Initialise(a_targetObjects.Length);
				m_heightScale.Initialise(a_targetObjects.Length);
			}

			return true;
		}

		protected override void SetAnimation(int a_targetIndex, float a_easedProgress)
		{
			if (m_targetLayoutElements == null || a_targetIndex >= m_targetLayoutElements.Length || m_targetLayoutElements[a_targetIndex] == null)
			{
				return;
			}

			float masterWidth = GetMasterWidth(m_widthParamToAnimate, a_targetIndex);
			float masterHeight = GetMasterHeight(m_heightParamToAnimate, a_targetIndex);
			
			m_targetLayoutElements[a_targetIndex].SetWidth( m_widthParamToAnimate, Mathf.LerpUnclamped( m_widthScale.GetValue (a_targetIndex) * masterWidth, masterWidth, a_easedProgress) );
			m_targetLayoutElements[a_targetIndex].SetHeight( m_heightParamToAnimate, Mathf.LerpUnclamped( m_heightScale.GetValue (a_targetIndex) * masterHeight, masterHeight, a_easedProgress) );
		}

		private float GetMasterWidth( LayoutElementParam a_param, int a_targetIndex )
		{
			switch(a_param)
			{
				case LayoutElementParam.MIN:
					return masterMinWidths[a_targetIndex];
				case LayoutElementParam.FLEXIBLE:
					return masterFlexibleWidths[a_targetIndex];
				case LayoutElementParam.PREFFERED:
				default:
					return masterPrefferedWidths[a_targetIndex];
			}
		}

		private float GetMasterHeight( LayoutElementParam a_param, int a_targetIndex )
		{
			switch(a_param)
			{
				case LayoutElementParam.MIN:
					return masterMinHeights[a_targetIndex];
				case LayoutElementParam.FLEXIBLE:
					return masterFlexibleHeights[a_targetIndex];
				case LayoutElementParam.PREFFERED:
				default:
					return masterPrefferedHeights[a_targetIndex];
			}
		}

		#region public API methods
		public void SetWidthParamToAnimate(LayoutElementParam a_param)
		{
			m_widthParamToAnimate = a_param;
		}

		public void SetHeightParamToAnimate(LayoutElementParam a_param)
		{
			m_heightParamToAnimate = a_param;
		}
		#endregion

#if UNITY_EDITOR
		public override void OnInspectorGUI(int a_stepIndex, AnimSetupType a_animType, SerializedObject a_serializedObject, System.Action a_onAnimatedStateChanged)
		{
			if( !m_validSetup )
			{
				EditorGUILayout.HelpBox("No LayoutElement's found on any of the target GameObjects.\nApply LayoutElement's and re-apply the LayoutElement Transition Step.", MessageType.Warning);
			}

			bool setupChanged = false;
			string animVariablePrefix = a_animType == AnimSetupType.Intro ? "Start" : "End";

			EditorGUILayout.PropertyField(a_serializedObject.FindProperty("m_widthParamToAnimate"));
			EditorGUILayout.PropertyField(a_serializedObject.FindProperty("m_heightParamToAnimate"));

			m_widthScale.OnInspectorGUI( a_serializedObject.FindProperty ("m_widthScale"), ref setupChanged, NumTargets > 1, new GUIContent(animVariablePrefix + " Width Scale") );
			m_heightScale.OnInspectorGUI( a_serializedObject.FindProperty ("m_heightScale"), ref setupChanged, NumTargets > 1, new GUIContent(animVariablePrefix + " Height Scale") );

			EditorGUILayout.Space();

			if (setupChanged)
			{
				a_onAnimatedStateChanged ();
			}
		}
#endif
	}
}