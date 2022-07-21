using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAnimatorCore
{
	[System.Serializable]
	public abstract class BaseAnimStepVariable
	{
		public enum VariableType
		{
			Offset,
			Single,
			Range
		}

		[SerializeField]
		protected VariableType m_type;

		[SerializeField]
		protected bool m_offsettingEnabled = false;

#if UNITY_EDITOR
		protected abstract void FromOffsetProperty(SerializedProperty a_sProperty, GUIContent a_propertyLabel, float a_labelWidth);
		protected abstract void ToOffsetProperty(SerializedProperty a_sProperty);

		protected virtual void FromRawProperty(SerializedProperty a_sProperty, GUIContent a_propertyLabel, float a_labelWidth)
		{
			UIAnimatorHelper.BetterPropertyField (a_sProperty.FindPropertyRelative ("m_from"), a_propertyLabel, a_labelWidth);
		}

		protected virtual void ToRawProperty(SerializedProperty a_sProperty)
		{
			EditorGUILayout.PropertyField (a_sProperty.FindPropertyRelative ("m_to"), new GUIContent(" "));
		}

		private const float TYPE_VAR_GUI_WIDTH = 55f; 

		public bool OnInspectorGUI(SerializedProperty a_sProperty, ref bool a_hasChanged, bool a_hasMultiTargets, GUIContent a_customLabel = null)
		{
			if (a_sProperty == null)
			{
				return false;
			}

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.BeginHorizontal ();

			SerializedProperty typeProperty = a_sProperty.FindPropertyRelative ("m_type");

			if (a_hasMultiTargets)
			{
				EditorGUILayout.BeginHorizontal (GUILayout.Width (TYPE_VAR_GUI_WIDTH));

				if (m_offsettingEnabled)
				{
					EditorGUILayout.PropertyField (typeProperty, GUIContent.none);
				}
				else
				{
					typeProperty.enumValueIndex = EditorGUILayout.Popup (typeProperty.enumValueIndex - 1, new string[] { VariableType.Single.ToString (), VariableType.Range.ToString () }) + 1;
				}

				EditorGUILayout.EndHorizontal ();
			}

			GUIContent propertyLabel = a_customLabel != null ? a_customLabel : new GUIContent (a_sProperty.displayName);
			float labelWidth = (a_hasMultiTargets ? EditorGUIUtility.labelWidth - TYPE_VAR_GUI_WIDTH : EditorGUIUtility.labelWidth);

			if (((VariableType)typeProperty.enumValueIndex) == VariableType.Offset)
			{
				FromOffsetProperty (a_sProperty, propertyLabel, labelWidth);
			}
			else if(((VariableType)typeProperty.enumValueIndex) == VariableType.Range)
			{
				if (m_offsettingEnabled)
				{
					FromOffsetProperty (a_sProperty, propertyLabel, labelWidth);
				}
				else
				{
					FromRawProperty (a_sProperty, propertyLabel, labelWidth);
				}
			}
			else if (((VariableType)typeProperty.enumValueIndex) == VariableType.Single)
			{
				FromRawProperty (a_sProperty, propertyLabel, labelWidth);
			}

			EditorGUILayout.EndHorizontal ();


			if (a_hasMultiTargets && ((VariableType) typeProperty.enumValueIndex) == VariableType.Range)
			{
				EditorGUILayout.BeginHorizontal (GUILayout.Height(20));

				// Annoyingly, this seems to be required to keep this inspector row at the same layout as the others...
				EditorGUILayout.BeginHorizontal ( GUILayout.Width (1));
				EditorGUILayout.EndHorizontal ();

				if (m_offsettingEnabled)
				{
					ToOffsetProperty (a_sProperty);
				}
				else
				{
					ToRawProperty (a_sProperty);
				}

				EditorGUILayout.EndHorizontal ();
			}



			if (EditorGUI.EndChangeCheck ())
			{
				a_hasChanged = true;
				return true;
			}

			return false;
		}
#endif
	}
}