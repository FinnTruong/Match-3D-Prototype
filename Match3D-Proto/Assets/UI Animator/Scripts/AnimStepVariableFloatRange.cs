using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAnimatorCore
{
	[System.Serializable]
	public class AnimStepVariableFloatRange : AnimStepVariableFloat
	{
		[SerializeField]
		private float m_minValue;

		[SerializeField]
		private float m_maxValue;

		public AnimStepVariableFloatRange(float a_minValue, float a_maxValue)
		{
			m_minValue = a_minValue;
			m_maxValue = a_maxValue;

			m_offsettingEnabled = false;
			m_type = VariableType.Single;
		}

		public AnimStepVariableFloatRange(float a_minValue, float a_maxValue, bool a_offsettingEnabled)
		{
			m_minValue = a_minValue;
			m_maxValue = a_maxValue;

			m_offsettingEnabled = a_offsettingEnabled;
			m_type = m_offsettingEnabled ? VariableType.Offset : VariableType.Single;
		}

		public AnimStepVariableFloatRange(float a_minValue, float a_maxValue, VariableType a_startType, bool a_offsettingEnabled)
		{
			m_minValue = a_minValue;
			m_maxValue = a_maxValue;

			m_type = a_startType;
			m_offsettingEnabled = a_offsettingEnabled;
		}

		public AnimStepVariableFloatRange (float a_minValue, float a_maxValue, float a_startValue, int a_numTargets = 1)
		{
			m_minValue = a_minValue;
			m_maxValue = a_maxValue;

			Initialise( a_startValue );

			m_offsettingEnabled = false;
			m_type = VariableType.Single;
			m_numTargets = a_numTargets;
		}

#if UNITY_EDITOR
		protected override void FromOffsetProperty(SerializedProperty a_sProperty, GUIContent a_propertyLabel, float a_labelWidth)
		{
			float cachedLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = a_labelWidth;

			SerializedProperty fromValueProperty = a_sProperty.FindPropertyRelative ("m_from");
			fromValueProperty.floatValue = EditorGUILayout.Slider (a_propertyLabel, fromValueProperty.floatValue - m_masterValues[0], m_minValue, m_maxValue) + m_masterValues[0];

			EditorGUIUtility.labelWidth = cachedLabelWidth;
		}

		protected override void ToOffsetProperty(SerializedProperty a_sProperty)
		{
			SerializedProperty toValueProperty = a_sProperty.FindPropertyRelative ("m_to");

			toValueProperty.floatValue = EditorGUILayout.Slider(toValueProperty.floatValue - m_masterValues[0], m_minValue, m_maxValue) + m_masterValues[0];
		}

		protected override void FromRawProperty(SerializedProperty a_sProperty, GUIContent a_propertyLabel, float a_labelWidth)
		{
			SerializedProperty fromValueProperty = a_sProperty.FindPropertyRelative ("m_from");

			float cachedLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = a_labelWidth;
			fromValueProperty.floatValue = EditorGUILayout.Slider(a_propertyLabel, fromValueProperty.floatValue, m_minValue, m_maxValue);
			EditorGUIUtility.labelWidth = cachedLabelWidth;
		}

		protected override void ToRawProperty(SerializedProperty a_sProperty)
		{
			SerializedProperty toValueProperty = a_sProperty.FindPropertyRelative ("m_to");

			toValueProperty.floatValue = EditorGUILayout.Slider("...to", toValueProperty.floatValue, m_minValue, m_maxValue);
		}
#endif
	}
}