using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAnimatorCore
{
	[System.Serializable]
	public class AnimStepVariablePositiveFloat : AnimStepVariableFloat
	{
		public AnimStepVariablePositiveFloat()
		{
			m_offsettingEnabled = false;
			m_type = VariableType.Single;
		}

		public AnimStepVariablePositiveFloat(bool a_offsettingEnabled)
		{
			m_offsettingEnabled = a_offsettingEnabled;
			m_type = m_offsettingEnabled ? VariableType.Offset : VariableType.Single;
		}

		public AnimStepVariablePositiveFloat(VariableType a_startType, bool a_offsettingEnabled)
		{
			m_type = a_startType;
			m_offsettingEnabled = a_offsettingEnabled;
		}

		public AnimStepVariablePositiveFloat (float a_startValue, int a_numTargets = 1)
		{
			Initialise( a_startValue );

			m_offsettingEnabled = false;
			m_type = VariableType.Single;
			m_numTargets = a_numTargets;
		}

		public override void SetValue(float a_value)
		{
			m_from = Mathf.Max(a_value, 0);
		}

		public override void SetValues(float a_fromValue, float a_toValue)
		{
			m_from = Mathf.Max(a_fromValue, 0);
			m_to = Mathf.Max(a_toValue, 0);
		}

#if UNITY_EDITOR
		protected override void FromOffsetProperty(SerializedProperty a_sProperty, GUIContent a_propertyLabel, float a_labelWidth)
		{
			base.FromOffsetProperty( a_sProperty, a_propertyLabel, a_labelWidth);

			SerializedProperty fromValueProperty = a_sProperty.FindPropertyRelative ("m_from");
			
			// Clamp to be no less than zero
			fromValueProperty.floatValue = Mathf.Max(fromValueProperty.floatValue, 0);
		}

		protected override void ToOffsetProperty(SerializedProperty a_sProperty)
		{
			base.ToOffsetProperty( a_sProperty);

			SerializedProperty toValueProperty = a_sProperty.FindPropertyRelative ("m_to");

			// Clamp to be no less than zero
			toValueProperty.floatValue = Mathf.Max(toValueProperty.floatValue, 0);
		}

		protected override void FromRawProperty(SerializedProperty a_sProperty, GUIContent a_propertyLabel, float a_labelWidth)
		{
			SerializedProperty fromValueProperty = a_sProperty.FindPropertyRelative ("m_from");

			UIAnimatorHelper.BetterPropertyField (fromValueProperty, a_propertyLabel, a_labelWidth);
			
			// Clamp to be no less than zero
			fromValueProperty.floatValue = Mathf.Max(fromValueProperty.floatValue, 0);
		}

		protected override void ToRawProperty(SerializedProperty a_sProperty)
		{
			SerializedProperty toValueProperty = a_sProperty.FindPropertyRelative ("m_to");

			EditorGUILayout.PropertyField (toValueProperty, new GUIContent(" "));

			// Clamp to be no less than zero
			toValueProperty.floatValue = Mathf.Max(toValueProperty.floatValue, 0);
		}
#endif
	}
}