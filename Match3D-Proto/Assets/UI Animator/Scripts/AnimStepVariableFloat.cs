using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAnimatorCore
{
	[System.Serializable]
	public class AnimStepVariableFloat : BaseAnimStepVariable
	{
		[SerializeField]
		protected float m_from;

		[SerializeField]
		protected float m_to;

		[SerializeField]
		protected float[] m_masterValues;

		[SerializeField]
		protected int m_numTargets;

		public float LargestValue { get { return m_type == VariableType.Range ? ( m_from > m_to ? m_from : m_to ) : m_from; } }

		/// <summary>
		/// This is both the 'From' value and the value used when only a single value.
		/// </summary>
		public float Value
		{
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

		public float ToValue
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

		public AnimStepVariableFloat()
		{
			m_offsettingEnabled = false;
			m_type = VariableType.Single;
		}

		public AnimStepVariableFloat(bool a_offsettingEnabled)
		{
			m_offsettingEnabled = a_offsettingEnabled;
			m_type = m_offsettingEnabled ? VariableType.Offset : VariableType.Single;
		}

		public AnimStepVariableFloat(VariableType a_startType, bool a_offsettingEnabled)
		{
			m_type = a_startType;
			m_offsettingEnabled = a_offsettingEnabled;
		}

		public AnimStepVariableFloat (float a_startValue, int a_numTargets = 1)
		{
			Initialise( a_startValue );

			m_offsettingEnabled = false;
			m_type = VariableType.Single;
			m_numTargets = a_numTargets;
		}

		public void Initialise( float a_startValue)
		{
			m_from = a_startValue;
			m_to = a_startValue;

			m_masterValues = new float[] { a_startValue };
			m_numTargets = 1;
		}

		public void Initialise( int a_numTargets )
		{
			m_numTargets = a_numTargets;
		}

		public void Initialise( int a_numTargets, float a_from, float a_to)
		{
			m_numTargets = a_numTargets;
			m_from = a_from;
			m_to = a_to;
		}

		public virtual void SetValue(float a_value)
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

		public virtual void SetValues(float a_fromValue, float a_toValue)
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

		public float GetValue(int a_targetIndex)
		{
			if (m_type == VariableType.Single || (m_type == VariableType.Range && m_numTargets == 1))
			{
				return m_from;
			}
			else if(m_type == VariableType.Offset)
			{
				return m_masterValues[a_targetIndex] + (m_from - m_masterValues[0]);
			}
			else if(m_type == VariableType.Range)
			{
				if (m_masterValues.Length > 1)
				{
					return m_masterValues [a_targetIndex] + Mathf.LerpUnclamped (m_from - m_masterValues [0], m_to - m_masterValues [0], (float)a_targetIndex / (m_masterValues.Length - 1));
				}
				else
				{
					return Mathf.LerpUnclamped (m_from, m_to, (float)a_targetIndex / (m_numTargets - 1));
				}
			}

			return m_from;
		}

#if UNITY_EDITOR
		protected override void FromOffsetProperty(SerializedProperty a_sProperty, GUIContent a_propertyLabel, float a_labelWidth)
		{
			float cachedLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = a_labelWidth;

			SerializedProperty fromValueProperty = a_sProperty.FindPropertyRelative ("m_from");
			fromValueProperty.floatValue = EditorGUILayout.FloatField (a_propertyLabel, fromValueProperty.floatValue - m_masterValues[0]) + m_masterValues[0];

			EditorGUIUtility.labelWidth = cachedLabelWidth;
		}

		protected override void ToOffsetProperty(SerializedProperty a_sProperty)
		{
			SerializedProperty toValueProperty = a_sProperty.FindPropertyRelative ("m_to");
			toValueProperty.floatValue = EditorGUILayout.FloatField (new GUIContent(" "), toValueProperty.floatValue - m_masterValues[0]) + m_masterValues[0];
		}
#endif
	}
}