using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAnimatorCore
{
	[System.Serializable]
	public class AnimStepVariableVector3 : BaseAnimStepVariable
	{
		[SerializeField]
		private Vector3 m_from;

		[SerializeField]
		private Vector3 m_to;

		[SerializeField]
		private Vector3[] m_masterValues;

		/// <summary>
		/// This is both the 'From' value and the value used when only a single value.
		/// </summary>
		public Vector3 Value {
			get
			{
				if (m_type == VariableType.Offset)
				{
					return m_from - m_masterValues[0];
				}
				else
				{
					return m_from;
				}
			}
		}

		public Vector3 ToValue
		{
			get
			{
				if (m_type == VariableType.Offset)
				{
					return m_to - m_masterValues[0];
				}
				else
				{
					return m_to;
				}
			}
		}

		public AnimStepVariableVector3()
		{
			m_offsettingEnabled = false;
			m_type = VariableType.Single;
		}

		public AnimStepVariableVector3(VariableType a_startType)
		{
			m_offsettingEnabled = false;
			m_type = a_startType;
		}

		public AnimStepVariableVector3(bool a_offsettingEnabled)
		{
			m_offsettingEnabled = a_offsettingEnabled;
			m_type = m_offsettingEnabled ? VariableType.Offset : VariableType.Single;
		}

		public AnimStepVariableVector3(VariableType a_startType, bool a_offsettingEnabled)
		{
			m_type = a_startType;
			m_offsettingEnabled = a_offsettingEnabled;
		}

		public void Initialise( Vector3[] a_masterValues)
		{
			m_from = a_masterValues[0];
			m_to = a_masterValues[0];

			m_masterValues = a_masterValues;
		}

		public void UpdateMasterValues( Vector3[] a_masterValues)
		{
			m_masterValues = a_masterValues;
		}

		public void SetValue(Vector3 a_value)
		{
			if (m_type == VariableType.Offset)
			{
				m_from = a_value + m_masterValues[0];
			}
			else
			{
				m_from = a_value;
			}
		}

		public void SetValues(Vector3 a_fromValue, Vector3 a_toValue)
		{
			if (m_type == VariableType.Offset)
			{
				m_from = a_fromValue + m_masterValues[0];
				m_to = a_toValue + m_masterValues[0];
			}
			else
			{
				m_from = a_fromValue;
				m_to = a_toValue;
			}
		}

		public Vector3 GetValue(int a_targetIndex)
		{
			if (m_type == VariableType.Single)
			{
				return m_from;
			}
			else if(m_type == VariableType.Offset)
			{
				return m_masterValues[a_targetIndex] + (m_from - m_masterValues[0]);
			}
			else if(m_type == VariableType.Range && m_masterValues.Length > 1)
			{
				return m_masterValues[a_targetIndex] + Vector3.LerpUnclamped (m_from - m_masterValues[0], m_to - m_masterValues[0], (float) a_targetIndex / (m_masterValues.Length - 1));
			}

			return m_from;
		}

		public Vector3 GetValueLerpedToMaster(int a_targetIndex, float a_progress)
		{
			return Vector3.LerpUnclamped( GetValue (a_targetIndex), m_masterValues[a_targetIndex], a_progress);
		}

#if UNITY_EDITOR
		protected override void FromOffsetProperty(SerializedProperty a_sProperty, GUIContent a_propertyLabel, float a_labelWidth)
		{
			float cachedLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = a_labelWidth;

			SerializedProperty fromValueProperty = a_sProperty.FindPropertyRelative ("m_from");
			fromValueProperty.vector3Value = EditorGUILayout.Vector3Field (a_propertyLabel, fromValueProperty.vector3Value - m_masterValues[0]) + m_masterValues[0];

			EditorGUIUtility.labelWidth = cachedLabelWidth;
		}

		protected override void ToOffsetProperty(SerializedProperty a_sProperty)
		{
			SerializedProperty toValueProperty = a_sProperty.FindPropertyRelative ("m_to");
			toValueProperty.vector3Value = EditorGUILayout.Vector3Field (new GUIContent(" "), toValueProperty.vector3Value - m_masterValues[0]) + m_masterValues[0];
		}
#endif
	}
}