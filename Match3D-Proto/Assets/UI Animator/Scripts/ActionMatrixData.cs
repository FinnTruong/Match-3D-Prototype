using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAnimatorCore
{
	[System.Serializable]
	public class ActionMatrixData
	{
		[SerializeField]
		private bool m_enabled = true;

		[SerializeField]
		private AnimSetupType m_animType = AnimSetupType.Intro;

		[SerializeField]
		private float m_delay = 0;

		public bool IsEnabled { get { return m_enabled; } set { m_enabled = value; } }
		public AnimSetupType AnimType { get { return m_animType; } set { m_animType = value; } }
		public float Delay { get { return m_delay; } set { m_delay = value; } }

		public ActionMatrixData()
		{

		}

		public ActionMatrixData(bool a_enabledByDefault)
		{
			m_enabled = a_enabledByDefault;
		}

#if UNITY_EDITOR
		public const float c_optionMatrixLabelColWidth = 110;
		public const float c_optionMatixDataColWidth = 40;

		public static void OnInspectorGUI(SerializedProperty a_serializedProperty, GUIContent a_label)
		{
			OnInspectorGUI (a_serializedProperty, a_label, false, false, false, false, false);
		}

		public static void OnInspectorGUI(SerializedProperty a_serializedProperty, GUIContent a_label, bool a_disableIntroOption, bool a_disableLoopOption, bool a_disableOutroOption)
		{
			OnInspectorGUI (a_serializedProperty, a_label, a_disableIntroOption, a_disableLoopOption, a_disableOutroOption, false, false);
		}

		public static void OnInspectorGUI(SerializedProperty a_serializedProperty, GUIContent a_label, bool a_disableNoneOption, bool a_disableDelayOption)
		{
			OnInspectorGUI (a_serializedProperty, a_label, false, false, false, a_disableNoneOption, a_disableDelayOption);
		}

		public static void OnInspectorGUI(SerializedProperty a_serializedProperty, GUIContent a_label, bool a_disableIntroOption, bool a_disableLoopOption, bool a_disableOutroOption, bool a_disableNoneOption, bool a_disableDelayOption)
		{
			EditorGUILayout.BeginHorizontal ();

			SerializedProperty enabledProperty = a_serializedProperty.FindPropertyRelative ("m_enabled");
			SerializedProperty animTypeProperty = a_serializedProperty.FindPropertyRelative ("m_animType");

			EditorGUILayout.LabelField (a_label, GUILayout.Width(c_optionMatrixLabelColWidth));

			// INTRO OPTION
			if (a_disableIntroOption)
			{
				GUI.enabled = false;
				EditorGUILayout.Toggle (false, GUILayout.Width (c_optionMatixDataColWidth));
				GUI.enabled = true;

				if (animTypeProperty.enumValueIndex == 0)
				{
					animTypeProperty.enumValueIndex = 1;
				}
			}
			else
			{
				if (EditorGUILayout.Toggle (enabledProperty.boolValue && animTypeProperty.enumValueIndex == 0, GUILayout.Width (c_optionMatixDataColWidth)))
				{
					enabledProperty.boolValue = true;
					animTypeProperty.enumValueIndex = 0;
				}
			}

			// LOOP OPTION
			if (a_disableLoopOption)
			{
				GUI.enabled = false;
				EditorGUILayout.Toggle (false, GUILayout.Width (c_optionMatixDataColWidth));
				GUI.enabled = true;

				if (animTypeProperty.enumValueIndex == 1)
				{
					animTypeProperty.enumValueIndex = 2;
				}
			}
			else
			{
				if (EditorGUILayout.Toggle (enabledProperty.boolValue && animTypeProperty.enumValueIndex == 1, GUILayout.Width (c_optionMatixDataColWidth)))
				{
					enabledProperty.boolValue = true;
					animTypeProperty.enumValueIndex = 1;
				}
			}

			// OUTRO OPTION
			if (a_disableOutroOption)
			{
				GUI.enabled = false;
				EditorGUILayout.Toggle (false, GUILayout.Width (c_optionMatixDataColWidth));
				GUI.enabled = true;

				if (animTypeProperty.enumValueIndex == 2)
				{
					animTypeProperty.enumValueIndex = 0;
				}
			}
			else
			{
				if (EditorGUILayout.Toggle (enabledProperty.boolValue && animTypeProperty.enumValueIndex == 2, GUILayout.Width (c_optionMatixDataColWidth)))
				{
					enabledProperty.boolValue = true;
					animTypeProperty.enumValueIndex = 2;
				}
			}

			// NONE OPTION
			if (a_disableNoneOption)
			{
				GUI.enabled = false;
				EditorGUILayout.Toggle (false, GUILayout.Width (c_optionMatixDataColWidth));
				GUI.enabled = true;
			}
			else
			{
				if (EditorGUILayout.Toggle (!enabledProperty.boolValue, GUILayout.Width (c_optionMatixDataColWidth)))
				{
					enabledProperty.boolValue = false;
				}
			}

			if (!a_disableDelayOption)
			{
				GUI.enabled = enabledProperty.boolValue;
				EditorGUILayout.PropertyField (a_serializedProperty.FindPropertyRelative ("m_delay"), GUIContent.none, GUILayout.Width(c_optionMatixDataColWidth));
				GUI.enabled = true;
			}

			EditorGUILayout.EndHorizontal ();
		}
#endif
	}
}