//#define CONFIRM_DELETIONS

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

namespace UIAnimatorCore
{
	[CustomEditor(typeof(UIAnimator))]
	public class UIAnimatorEditor : Editor 
	{
		private UIAnimator m_uiAnimator;

		private double m_lastTime;
		private bool m_animatingPreview = false;
		private bool m_animationFinished = true;

		private List<System.Type> m_availableTransitionStepTypes;
		private string[] m_availableTransitionStepNames;
		private List<System.Type> m_availableLoopAnimStepTypes;
		private string[] m_availableLoopAnimStepNames;
		private int m_currentlySelectedAnimStepTypeIndex = 0;

		private Texture2D m_playbackButtonPlay;
		private Texture2D m_playbackButtonPause;
		private Texture2D m_playbackButtonSkipToEnd;
		private Texture2D m_playbackButtonSkipToStart;
		private Texture2D m_buttonSetStateAsMaster;
		private Texture2D m_buttonResetToMaster;
		private Texture2D m_buttonMiniResetToMaster;

		private Texture2D m_customDropdownInactiveTexture;
		private Texture2D m_customDropdownActiveTexture;

		private bool m_showAnimSetupSettings = false;
		private float m_playbackButtonSize = 40;

		private GUIStyle m_instanceFoldoutPreLabelStyle;

		protected GUIStyle InstanceFoldoutPreLabelStyle
		{
			get {
				if (m_instanceFoldoutPreLabelStyle == null)
				{
					m_instanceFoldoutPreLabelStyle = new GUIStyle(EditorStyles.label);
					m_instanceFoldoutPreLabelStyle.richText = true;
#if UNITY_EDITOR_WIN
					m_instanceFoldoutPreLabelStyle.contentOffset = new Vector2 (0,-4f);
#elif UNITY_EDITOR_OSX
					m_instanceFoldoutPreLabelStyle.contentOffset = new Vector2 (0,-6f); 
#endif
					m_instanceFoldoutPreLabelStyle.fixedHeight = 30f;
				}
				return m_instanceFoldoutPreLabelStyle;
			}
		}

#if UNITY_EDITOR_WIN
		private GUIStyle m_instanceFoldoutPreLabelStyle2;
		protected GUIStyle InstanceFoldoutPreLabelStyle2
		{
			get {
				if (m_instanceFoldoutPreLabelStyle2 == null)
				{
					m_instanceFoldoutPreLabelStyle2 = new GUIStyle(EditorStyles.label);
					m_instanceFoldoutPreLabelStyle2.richText = true;
					m_instanceFoldoutPreLabelStyle2.contentOffset = new Vector2 (1,-8f);
					m_instanceFoldoutPreLabelStyle2.fixedHeight = 30f;
				}
				return m_instanceFoldoutPreLabelStyle2;
			}
		}
#endif

		private GUIStyle m_texturedButtonGUIStyle;
		protected GUIStyle TexturedButtonGUIStyle { get {

				if (m_texturedButtonGUIStyle == null)
				{
					m_texturedButtonGUIStyle = new GUIStyle (EditorStyles.miniButton);
					m_texturedButtonGUIStyle.border = new RectOffset(0,0,0,0);
					m_texturedButtonGUIStyle.margin = new RectOffset (0,0,0,0);
					m_texturedButtonGUIStyle.padding = new RectOffset (0,0,0,0);
				}
				return m_texturedButtonGUIStyle;
			}
		}

		private GUIStyle m_blankGUIStyle;
		protected GUIStyle BlankGUIStyle
		{
			get
			{
				if (m_blankGUIStyle == null)
				{
					m_blankGUIStyle = new GUIStyle();
				}

				return m_blankGUIStyle;
			}
		}

		private GUIStyle m_customFoldoutStyle;
		protected GUIStyle CustomFoldoutStyle
		{
			get {
				if (m_customFoldoutStyle == null)
				{
					m_customFoldoutStyle = new GUIStyle (EditorStyles.foldout);
					m_customFoldoutStyle.fontStyle = FontStyle.Bold;
					m_customFoldoutStyle.fontSize = 14;
					m_customFoldoutStyle.alignment = TextAnchor.MiddleLeft;
					m_customFoldoutStyle.normal.background = null;
					m_customFoldoutStyle.active.background = null;
					m_customFoldoutStyle.focused.background = null;
					m_customFoldoutStyle.hover.background = null;
					m_customFoldoutStyle.onNormal.background = null;
					m_customFoldoutStyle.onActive.background = null;
					m_customFoldoutStyle.onFocused.background = null;
					m_customFoldoutStyle.onHover.background = null;

					m_customFoldoutStyle.padding = new RectOffset (32,0,0,0);
					m_customFoldoutStyle.fixedHeight = 20f;
					m_customFoldoutStyle.contentOffset = new Vector2 (0,-3);
				}
				return m_customFoldoutStyle;
			}
		}

		protected GUIStyle BaseCustomDropdownStyle
		{
			get {
				GUIStyle customDropdown = new GUIStyle (EditorStyles.helpBox);
				customDropdown.border = new RectOffset(28,2,2,2);
				customDropdown.margin = new RectOffset (4,4,4,4);
				customDropdown.padding = new RectOffset (0,0,0,0);
				customDropdown.overflow = new RectOffset (0,0,0,0);
				return customDropdown;
			}
		}

		private GUIStyle m_customDropdownActiveStyle;
		protected GUIStyle CustomDropdownActiveStyle
		{
			get {
				if (m_customDropdownActiveStyle == null || m_customDropdownActiveStyle.normal.background == null)
				{
					m_customDropdownActiveStyle = BaseCustomDropdownStyle;
					m_customDropdownActiveStyle.normal.background = m_customDropdownActiveTexture; 
				}
				return m_customDropdownActiveStyle;
			}
		}

		private GUIStyle m_customDropdownInactiveStyle;
		protected GUIStyle CustomDropdownInactiveStyle
		{
			get {
				if (m_customDropdownInactiveStyle == null || m_customDropdownInactiveStyle.normal.background == null)
				{
					m_customDropdownInactiveStyle = BaseCustomDropdownStyle;
					m_customDropdownInactiveStyle.normal.background = m_customDropdownInactiveTexture; 
				}
				return m_customDropdownInactiveStyle;
			}
		}

		private string m_uiAnimatorRootDirectoryPath = null;
		private string UIAnimatorRootDirectoryPath {
			get {
				if (string.IsNullOrEmpty( m_uiAnimatorRootDirectoryPath ))
				{
					// Get UIAnimator relative folder path
					// Assumes that this script is located in 'UI Animator/Editor'
					MonoScript monoScript = MonoScript.FromScriptableObject (this);
					string currentScriptPath = AssetDatabase.GetAssetPath (monoScript);
					m_uiAnimatorRootDirectoryPath = currentScriptPath.Replace ("Editor/" + monoScript.name + ".cs", "");
				}

				return m_uiAnimatorRootDirectoryPath;
			}
		}

		private void OnEnable()
		{
			m_uiAnimator = (UIAnimator) target;

			m_availableTransitionStepTypes = GetListOfSubTypes<TransitionAnimationStep> ();
			m_availableTransitionStepNames = new string[m_availableTransitionStepTypes.Count + 1];

			m_availableTransitionStepNames [0] = "None";

			for (int idx = 0; idx < m_availableTransitionStepTypes.Count; idx++)
			{
				string editorDisplayNameForClass = (string) m_availableTransitionStepTypes[idx].GetProperty ("EditorDisplayName").GetValue(null, null);
				m_availableTransitionStepNames [idx+1] = editorDisplayNameForClass;
			}

			m_availableLoopAnimStepTypes = GetListOfSubTypes<EffectAnimationStep> ();
			m_availableLoopAnimStepNames = new string[m_availableLoopAnimStepTypes.Count];

			for (int idx = 0; idx < m_availableLoopAnimStepTypes.Count; idx++)
			{
				string editorDisplayNameForClass = (string) m_availableLoopAnimStepTypes[idx].GetProperty ("EditorDisplayName").GetValue(null, null);
				m_availableLoopAnimStepNames [idx] = editorDisplayNameForClass;
			}

			m_lastTime = EditorApplication.timeSinceStartup;

			SetAnimStepHideFlags ();

			LoadUITextures ();

			EditorApplication.update += EditorUpdate;
		}

		private void SetAnimStepHideFlags()
		{
			if (m_uiAnimator.AnimationSetups != null)
			{
				for (int setupIdx = 0; setupIdx < m_uiAnimator.AnimationSetups.Count; setupIdx++)
				{
					for (int sIdx = 0; sIdx < m_uiAnimator.AnimationSetups[setupIdx].AnimationStages.Count; sIdx++)
					{
						for (int iIdx = 0; iIdx < m_uiAnimator.AnimationSetups[setupIdx].AnimationStages[sIdx].AnimationInstances.Count; iIdx++)
						{
							for (int aIdx = 0; aIdx < m_uiAnimator.AnimationSetups[setupIdx].AnimationStages[sIdx].AnimationInstances[iIdx].AnimationSteps.Count; aIdx++)
							{
								if (m_uiAnimator.AnimationSetups[setupIdx].AnimationStages[sIdx].AnimationInstances[iIdx].AnimationSteps [aIdx] != null)
								{
									m_uiAnimator.AnimationSetups[setupIdx].AnimationStages[sIdx].AnimationInstances[iIdx].AnimationSteps [aIdx].hideFlags = HideFlags.HideInInspector;
								}
							}
						}
					}
				}
			}
		}

		private void LoadUITextures()
		{
			string editorStyleSuffix = Application.HasProLicense() ? "_light" : "_dark";

			// Load in required ui inspector textures
			m_playbackButtonPlay = AssetDatabase.LoadAssetAtPath(UIAnimatorRootDirectoryPath + "Editor/Icons/playback_button_play" + editorStyleSuffix + ".png", typeof(Texture2D)) as Texture2D;
			m_playbackButtonPause = AssetDatabase.LoadAssetAtPath (UIAnimatorRootDirectoryPath + "Editor/Icons/playback_button_pause" + editorStyleSuffix + ".png", typeof(Texture2D)) as Texture2D;
			m_playbackButtonSkipToEnd = AssetDatabase.LoadAssetAtPath(UIAnimatorRootDirectoryPath + "Editor/Icons/playback_button_stepforward" + editorStyleSuffix + ".png", typeof(Texture2D)) as Texture2D;
			m_playbackButtonSkipToStart = AssetDatabase.LoadAssetAtPath(UIAnimatorRootDirectoryPath + "Editor/Icons/playback_button_stepback" + editorStyleSuffix + ".png", typeof(Texture2D)) as Texture2D;
			m_buttonSetStateAsMaster = AssetDatabase.LoadAssetAtPath(UIAnimatorRootDirectoryPath + "Editor/Icons/apply_state_as_master" + editorStyleSuffix + ".png", typeof(Texture2D)) as Texture2D;
			m_buttonResetToMaster = AssetDatabase.LoadAssetAtPath(UIAnimatorRootDirectoryPath + "Editor/Icons/playback_button_reset" + editorStyleSuffix + ".png", typeof(Texture2D)) as Texture2D;
			m_buttonMiniResetToMaster = AssetDatabase.LoadAssetAtPath(UIAnimatorRootDirectoryPath + "Editor/Icons/playback_button_reset_icon" + editorStyleSuffix + ".png", typeof(Texture2D)) as Texture2D;
			m_customDropdownActiveTexture = AssetDatabase.LoadAssetAtPath(UIAnimatorRootDirectoryPath + "Editor/Icons/custom_dropdown_bar_active" + editorStyleSuffix + ".png", typeof(Texture2D)) as Texture2D;
			m_customDropdownInactiveTexture = AssetDatabase.LoadAssetAtPath(UIAnimatorRootDirectoryPath + "Editor/Icons/custom_dropdown_bar_inactive" + editorStyleSuffix + ".png", typeof(Texture2D)) as Texture2D;

		}

		private void OnDisable()
		{
			EditorApplication.update -= EditorUpdate;
		}

		private void OnDestroy()
		{
			if (target == null && Application.isPlaying == false && m_uiAnimator.AnimationSetups != null)
			{
				// UI Animator component has been manually deleted in the Editor.
				// Destroy all hidden referenced animation step components

				BaseAnimationStep animStep;

				for (int setupIdx = 0; setupIdx < m_uiAnimator.AnimationSetups.Count; setupIdx++)
				{
					for (int sIdx = 0; sIdx < m_uiAnimator.AnimationSetups [setupIdx].AnimationStages.Count; sIdx++)
					{
						if (m_uiAnimator.AnimationSetups [setupIdx].AnimationStages [sIdx].AnimationInstances != null)
						{
							for (int iIdx = 0; iIdx < m_uiAnimator.AnimationSetups [setupIdx].AnimationStages [sIdx].AnimationInstances.Count; iIdx++)
							{
								if (m_uiAnimator.AnimationSetups [setupIdx].AnimationStages [sIdx].AnimationInstances [iIdx].AnimationSteps != null)
								{
									for (int aIdx = 0; aIdx < m_uiAnimator.AnimationSetups [setupIdx].AnimationStages [sIdx].AnimationInstances [iIdx].AnimationSteps.Count; aIdx++)
									{
										animStep = m_uiAnimator.AnimationSetups [setupIdx].AnimationStages [sIdx].AnimationInstances [iIdx].AnimationSteps [aIdx];

										if (animStep == null)
										{
											continue;
										}

										// Force back to its master state
										animStep.SetToMasterState ();

										// Make the component visible
										animStep.hideFlags = HideFlags.None;

										// Destroy it
										DestroyImmediate (animStep);
									}
								}
							}
						}
					}
				}
			}
		}

		private void EditorUpdate()
		{
			if (!m_animatingPreview)
			{
				return;
			}

			if (m_uiAnimator.UpdateState ((float) (EditorApplication.timeSinceStartup - m_lastTime)))
			{
				m_animatingPreview = false;
				m_animationFinished = true;
			}

			m_lastTime = EditorApplication.timeSinceStartup;

			// Required to keep the Editor view updating during in-editor previews.
			EditorUtility.SetDirty (target);
		}

		private void DrawAutoPlayOptionMatrix()
		{
			EditorGUILayout.LabelField ("AutoPlay Behaviour Matrix", EditorStyles.boldLabel);

			EditorGUILayout.BeginHorizontal ();

			GUILayout.Space (ActionMatrixData.c_optionMatrixLabelColWidth);
			TextClipping clippingCachedValue = EditorStyles.label.clipping;
			EditorStyles.label.clipping = TextClipping.Overflow;
			EditorGUILayout.LabelField ("Intro", GUILayout.Width(ActionMatrixData.c_optionMatixDataColWidth));
			EditorGUILayout.LabelField ("Loop", GUILayout.Width(ActionMatrixData.c_optionMatixDataColWidth));
			EditorGUILayout.LabelField ("Outro", GUILayout.Width(ActionMatrixData.c_optionMatixDataColWidth));
			EditorGUILayout.LabelField ("None", GUILayout.Width(ActionMatrixData.c_optionMatixDataColWidth));
			EditorGUILayout.LabelField ("Delay", GUILayout.Width(ActionMatrixData.c_optionMatixDataColWidth));
			EditorStyles.label.clipping = clippingCachedValue;

			EditorGUILayout.EndHorizontal ();

			SerializedProperty playOnEnableProperty = serializedObject.FindProperty ("m_playOnEnableAMD");

			ActionMatrixData.OnInspectorGUI (playOnEnableProperty, new GUIContent("OnEnable"));

			if (!playOnEnableProperty.FindPropertyRelative ("m_enabled").boolValue)
			{
				ActionMatrixData.OnInspectorGUI (serializedObject.FindProperty("m_startPoseAMD"), new GUIContent("Start Pose"), a_disableNoneOption: true, a_disableDelayOption: true);
			}

			ActionMatrixData.OnInspectorGUI (serializedObject.FindProperty("m_afterIntroAMD"), new GUIContent("After INTRO"), a_disableIntroOption: true, a_disableLoopOption: false, a_disableOutroOption: false);

			ActionMatrixData.OnInspectorGUI (serializedObject.FindProperty("m_afterLoopAMD"), new GUIContent("After LOOP"), a_disableIntroOption: true, a_disableLoopOption: true, a_disableOutroOption: false);

			ActionMatrixData.OnInspectorGUI (serializedObject.FindProperty("m_onPointerEnterAMD"), new GUIContent("OnPointerEnter"));

			ActionMatrixData.OnInspectorGUI (serializedObject.FindProperty("m_onPointerExitAMD"), new GUIContent("OnPointerExit"));
		}

		public override void OnInspectorGUI()
		{
//			DrawDefaultInspector ();

			SerializedProperty timerProperty = serializedObject.FindProperty ("m_timer");
			float currentAnimDuration = m_uiAnimator.GetAnimationDuration ();

			bool animInstanceWithDrivenTargets = false;
			
#if CSHARP_7_3_OR_NEWER
			animInstanceWithDrivenTargets = CheckForLayoutDrivenTargets(m_uiAnimator);
#endif

			DrawPlaybackControls (timerProperty, currentAnimDuration, m_uiAnimator.gameObject.activeInHierarchy);

			GUILayout.Space (10);

			SerializedProperty showExtraSettingsProperty = serializedObject.FindProperty ("m_showExtraSettings");

			showExtraSettingsProperty.boolValue = EditorGUILayout.ToggleLeft ("Show General Settings", showExtraSettingsProperty.boolValue);

			if (showExtraSettingsProperty.boolValue)
			{
				EditorGUI.indentLevel++;

				Color cachedBGColor = GUI.backgroundColor;
				GUI.backgroundColor = Color.green;

				EditorGUILayout.BeginVertical(EditorStyles.helpBox);

				GUI.backgroundColor = cachedBGColor;

				DrawAutoPlayOptionMatrix ();

				GUILayout.Space (10);

				EditorGUILayout.LabelField ("Loop Animation Behaviour", EditorStyles.boldLabel);

				EditorGUILayout.BeginHorizontal ();

				SerializedProperty playLoopInfinitelyProperty = serializedObject.FindProperty ("m_playLoopAnimInfinitely");

				EditorGUILayout.PropertyField (playLoopInfinitelyProperty, new GUIContent("Play Infinitely?"));

				if (!playLoopInfinitelyProperty.boolValue)
				{
					EditorGUILayout.PropertyField (serializedObject.FindProperty("m_numLoopIterations"), new GUIContent("Num Iterations"));
				}

				EditorGUILayout.EndHorizontal ();

				GUILayout.Space (10);

				EditorGUILayout.LabelField ("Time Mode", EditorStyles.boldLabel);

				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_timeMode"), GUIContent.none);

				GUILayout.Space (10);

				EditorGUILayout.LabelField (new GUIContent("Play Mode", "Use 'OPTIMAL' by default, unless you're animating LayoutGroup driven UI elements and see animation state issues, then in which case try 'CONTINUOUS'"), EditorStyles.boldLabel);

				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_animationPlayMode"), GUIContent.none);

                if (!animInstanceWithDrivenTargets && m_uiAnimator.PlayMode == AnimationPlayMode.CONTINUOUS)
                {
                    EditorGUILayout.HelpBox("CONTINUOUS mode is only recommended if you're trying to animate RectTransforms which are controlled by LayoutGroups",
                                            MessageType.Info,
                                            wide: true);

                    GUILayout.Space(10);
                }

				GUILayout.Space (5);

				EditorGUILayout.EndVertical();

				EditorGUI.indentLevel--;
			}

			GUILayout.Space (10);

			

			if(animInstanceWithDrivenTargets && m_uiAnimator.PlayMode != AnimationPlayMode.CONTINUOUS)
			{
				EditorGUILayout.HelpBox("Detected UI Layout driven targets. If animation issues encountered, consider using 'CONTINUOUS' PlayMode (in General Settings).",
										MessageType.Info,
										wide: true);

				GUILayout.Space(16);
			}

			SerializedProperty currentAnimSetupIndexProperty = serializedObject.FindProperty ("m_currentAnimSetupIndex");

			EditorGUI.BeginChangeCheck ();

			int currentAnimSetupIndexValue = GUILayout.Toolbar (currentAnimSetupIndexProperty.intValue, new string[]{ "INTRO", "LOOP", "OUTRO" });

			if (EditorGUI.EndChangeCheck ())
			{
				m_uiAnimator.SetAnimType ( (AnimSetupType) currentAnimSetupIndexValue);
				return;
			}

			AnimSetupType animSetupType = (AnimSetupType) currentAnimSetupIndexValue;

			GUILayout.Space (5);

			SerializedProperty animationSetupsListProperty = serializedObject.FindProperty ("m_animationSetups");
			SerializedProperty animationStagesListProperty;
			SerializedProperty animationInstancesListProperty;
			SerializedProperty animationStepsListProperty;
			SerializedProperty animationStageProperty;
			AnimationStage animStage;
			AnimationInstance animInstance;
			float timerOffset = 0;
			List<AnimationStage> currentAnimStages = m_uiAnimator.CurrentAnimStages;
			SerializedProperty currentAnimStagesProperty;
			bool changedUI;

			if (animationSetupsListProperty.arraySize < 3)
			{
				return;
			}

			animationStagesListProperty = animationSetupsListProperty.GetArrayElementAtIndex (currentAnimSetupIndexProperty.intValue);

			m_showAnimSetupSettings = EditorGUILayout.ToggleLeft ("Show '" + animSetupType + "' Settings", m_showAnimSetupSettings);

			GUILayout.Space (5);

			if (m_showAnimSetupSettings)
			{
				EditorGUILayout.PropertyField (animationStagesListProperty.FindPropertyRelative ("m_onStartAction"));
				EditorGUILayout.PropertyField (animationStagesListProperty.FindPropertyRelative ("m_onFinishedAction"));
			}

			currentAnimStagesProperty = animationStagesListProperty.FindPropertyRelative ("m_animationStages");

			int numAnimationInstances = 0;

			for (int sIdx = 0; sIdx < currentAnimStagesProperty.arraySize; sIdx++)
			{
				animStage = currentAnimStages [sIdx];

				animationStageProperty = currentAnimStagesProperty.GetArrayElementAtIndex (sIdx);

				animationInstancesListProperty = animationStageProperty.FindPropertyRelative ("m_animationInstances");

				numAnimationInstances += animationInstancesListProperty.arraySize;

				EditorGUILayout.BeginHorizontal (animStage.InspectorToggleState ? CustomDropdownActiveStyle : CustomDropdownInactiveStyle);

				animStage.InspectorToggleState = EditorGUILayout.Foldout (animStage.InspectorToggleState, "Stage " + (sIdx+1), true, CustomFoldoutStyle);



				// Display the list editing up/down/delete options
				if (UIAnimatorHelper.RenderListEditingButtons (currentAnimStagesProperty, ref sIdx, a_forceAtLeastOne: true,
#if CONFIRM_DELETIONS
					a_confirmBeforeDeletion: true,
#endif
					a_onDeleteEvent: ()=>{

					Undo.RecordObject(m_uiAnimator, "Deleted Animation Stage");

					OnAnimationStageDeleted (animStage);

				}))
				{
					// The list was manipulated in some way.
					serializedObject.ApplyModifiedProperties ();
					return;
				}

				GUILayout.Space (3);

				EditorGUILayout.EndHorizontal();

				GUILayout.Space (3);

				if (animStage.InspectorToggleState)
				{
					GUILayout.Space (3);

					SerializedProperty delayProperty = animationStageProperty.FindPropertyRelative ("m_startDelay");
					EditorGUILayout.BeginHorizontal ();

					UIAnimatorHelper.BetterPropertyField (delayProperty, 75, 110);

					if (delayProperty.floatValue < 0)
					{
						delayProperty.floatValue = 0;
					}

					GUILayout.FlexibleSpace ();

					delayProperty = animationStageProperty.FindPropertyRelative ("m_endDelay");

					UIAnimatorHelper.BetterPropertyField (delayProperty, 75, 110);

					if (delayProperty.floatValue < 0)
					{
						delayProperty.floatValue = 0;
					}

					EditorGUILayout.EndHorizontal ();


					for (int iIdx = 0; iIdx < animationInstancesListProperty.arraySize; iIdx++)
					{
						animInstance = animStage.AnimationInstances [iIdx];

						EditorGUILayout.BeginVertical (EditorStyles.helpBox);

						SerializedProperty animInstanceProperty = animationInstancesListProperty.GetArrayElementAtIndex (iIdx);
						SerializedProperty targetObjectsProperty = animInstanceProperty.FindPropertyRelative ("m_targetObjects");

						EditorGUILayout.BeginHorizontal ();

						TextClipping cachedClippingSetting = EditorStyles.foldout.clipping;
						EditorStyles.foldout.clipping = TextClipping.Clip;
						float cachedLabelWidth = EditorGUIUtility.labelWidth;
						EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - 100;


#if UNITY_EDITOR_OSX
						EditorGUILayout.LabelField ("<size=20>" + (animInstance.InspectorToggleState ? "\u25D2" : "\u25D1") + "</size>", InstanceFoldoutPreLabelStyle, GUILayout.Width(25));

#else
						if(animInstance.InspectorToggleState)
						{
							EditorGUILayout.LabelField ("<size=25>\u25D2</size>", InstanceFoldoutPreLabelStyle2, GUILayout.Width(30));
						}
						else
						{
							EditorGUILayout.LabelField ("<size=14>\u25D1</size>", InstanceFoldoutPreLabelStyle, GUILayout.Width(30));
						}

#endif


						animInstance.InspectorToggleState = EditorGUILayout.Foldout (
																						animInstance.InspectorToggleState,
																						animInstance.Title,
																						true,
																						EditorStyles.label);

						EditorGUIUtility.labelWidth = cachedLabelWidth;
						EditorStyles.foldout.clipping = cachedClippingSetting;

						// Display the list editing up/down/delete options
						if (UIAnimatorHelper.RenderListEditingButtons (animationInstancesListProperty, ref iIdx,
#if CONFIRM_DELETIONS
							a_confirmBeforeDeletion: true,
#endif
							a_onDeleteEvent: ()=>{

								Undo.RecordObject(m_uiAnimator, "Deleted Animation Instance");
								foreach(BaseAnimationStep animStepToDelete in animInstance.AnimationSteps)
								{
									OnAnimationStepDeleted(animStepToDelete);
								}
								return;

							}))
						{
							// The list was manipulated in some way.
							serializedObject.ApplyModifiedProperties ();
							return;
						}



						EditorGUILayout.EndHorizontal ();

						if (animInstance.InspectorToggleState)
						{
							if (animInstance.IsUiAnimatorSubModule)
							{
								GUI.enabled = false;
								EditorGUILayout.PropertyField (animInstanceProperty.FindPropertyRelative ("m_uiAnimatorSubModule"), new GUIContent("Sub Module"));
								GUI.enabled = true;
							}
							else
							{
								changedUI = GUI.changed;

								EditorGUI.indentLevel++;
								
								EditorGUILayout.PropertyField (targetObjectsProperty, new GUIContent("Targets List"), true);
								
								EditorGUI.indentLevel--;
								
								if (!changedUI && GUI.changed)
								{
									if (targetObjectsProperty.arraySize == 0)
									{
										return;
									}
									
									// Reset the anim steps to Master, then re-initialise the variables to the current state, to include any new object targets
									animInstance.RefreshNumTargetsData();
									
									m_uiAnimator.ResetToDefault ();
									
									serializedObject.ApplyModifiedProperties ();
									
									animInstance.SetDefaultValuesFromCurrentState ();
									
									EditorGUILayout.EndVertical ();
									
									return;
								}
							}

							changedUI = GUI.changed;

							SerializedProperty animInstanceDelayProperty = animInstanceProperty.FindPropertyRelative ("m_startDelay");
							EditorGUILayout.PropertyField( animInstanceDelayProperty );

							if (animInstanceDelayProperty.floatValue < 0)
							{
								animInstanceDelayProperty.floatValue = 0;
							}

							if (!changedUI && GUI.changed)
							{
								timerProperty.floatValue = timerOffset + animInstanceDelayProperty.floatValue;

								m_uiAnimator.SetAnimationTimer (timerProperty.floatValue);

								serializedObject.ApplyModifiedProperties ();
								EditorGUILayout.EndVertical ();
								return;
							}

							GUILayout.Space (5f);

							if (animInstance.IsUiAnimatorSubModule)
							{
								EditorGUILayout.LabelField ("Duration", "" + (animInstance.GetDuration(m_uiAnimator.CurrentAnimType) - animInstanceDelayProperty.floatValue));
							}
							else
							{
								animationStepsListProperty = animInstanceProperty.FindPropertyRelative ("m_animationSteps");

								if (animSetupType == AnimSetupType.Intro)
								{
									DrawTransitionStepUI (animSetupType, animationStepsListProperty, animInstance, timerOffset + animInstanceDelayProperty.floatValue);

									DrawEffectStepsUI (animSetupType, animationStepsListProperty, animInstance, timerOffset + animInstanceDelayProperty.floatValue);
								}
								else if (animSetupType == AnimSetupType.Outro)
								{
									float effectStepsTimerOffset = DrawEffectStepsUI (animSetupType, animationStepsListProperty, animInstance, timerOffset + animInstanceDelayProperty.floatValue);

									GUILayout.Space (5f);

									DrawTransitionStepUI (animSetupType, animationStepsListProperty, animInstance, effectStepsTimerOffset + timerOffset + animInstanceDelayProperty.floatValue);
								}
								else if (animSetupType == AnimSetupType.Loop)
								{
									DrawEffectStepsUI (animSetupType, animationStepsListProperty, animInstance, timerOffset + animInstanceDelayProperty.floatValue);
								}
							}
						}

						EditorGUILayout.EndVertical ();
					}

					GUILayout.Space (20);

					DrawTargetDragDropZone ((GameObject[] a_targetObjects) =>
					{
						Undo.RecordObject(m_uiAnimator, "Added new Animation Instance");
						if(a_targetObjects.Length == 1 && a_targetObjects[0].GetComponent<UIAnimator>() != null)
						{
							UIAnimator otherUIAnimator = a_targetObjects[0].GetComponent<UIAnimator>();

							if(otherUIAnimator != m_uiAnimator)
							{
								// Add a special UIAnimator reference version of the Animation Instance
								animStage.AddNewAnimationInstance(otherUIAnimator);

								return;
							}
						}

						animStage.AddNewAnimationInstance(a_targetObjects);

						return;
					});
				}

				GUILayout.Space (10);

				timerOffset += animStage.GetTotalDuration(m_uiAnimator.CurrentAnimType);
			}


			EditorGUILayout.BeginHorizontal ();

			if( currentAnimStagesProperty.arraySize == 1 && numAnimationInstances == 0 && (animSetupType == AnimSetupType.Loop || animSetupType == AnimSetupType.Outro) )
			{
				if( GUILayout.Button("Copy Setup from Intro?", GUILayout.Width(160)) )
				{
					for(int stageIdx=0; stageIdx < m_uiAnimator.AnimationSetups[0].AnimationStages.Count; stageIdx++)
					{
						AnimationStage currentTargetAnimStage;
						AnimationStage currentMasterAnimStage = m_uiAnimator.AnimationSetups[0].AnimationStages[stageIdx];

						if(stageIdx == 0)
						{
							currentTargetAnimStage = m_uiAnimator.AnimationSetups[(int) animSetupType].AnimationStages[0];
						}
						else
						{
							currentTargetAnimStage = new AnimationStage();
							currentAnimStages.Add(currentTargetAnimStage);
						}

						foreach(AnimationInstance masterAnimInstance in currentMasterAnimStage.AnimationInstances)
						{
							currentTargetAnimStage.AddNewAnimationInstance( masterAnimInstance.TargetObjects );
						}
					}
				}
			}

			GUILayout.FlexibleSpace ();

			if(GUILayout.Button("Add Stage", GUILayout.Width(150)))
			{
				Undo.RecordObject(m_uiAnimator, "Added new Animation Stage");
				currentAnimStages.Add(new AnimationStage());
			}

			GUILayout.Space (4);

			EditorGUILayout.EndHorizontal ();

			GUILayout.Space (20);

			DrawPlaybackControls (timerProperty, currentAnimDuration, m_uiAnimator.gameObject.activeInHierarchy);

			GUILayout.Space (20);

			if (m_animatingPreview)
			{
				Repaint ();
			}

			SceneView.RepaintAll ();

			serializedObject.ApplyModifiedProperties();
		}

		private bool CheckForLayoutDrivenTargets ( UIAnimator a_uiAnimator )
		{
			for(int setupIdx=0; setupIdx < a_uiAnimator.AnimationSetups.Count; setupIdx++)
			{
				for(int stageIdx=0; stageIdx < a_uiAnimator.AnimationSetups[setupIdx].AnimationStages.Count; stageIdx++)
				{
					for(int instanceIdx=0; instanceIdx < a_uiAnimator.AnimationSetups[setupIdx].AnimationStages[stageIdx].AnimationInstances.Count; instanceIdx++)
					{
						AnimationInstance animInstance = a_uiAnimator.AnimationSetups[setupIdx].AnimationStages[stageIdx].AnimationInstances[instanceIdx];

						if(animInstance.IsUiAnimatorSubModule)
						{
							continue;
						}

						for( int idx=0; idx < animInstance.TargetObjects.Length; idx++)
						{
							if(animInstance.TargetObjects[idx] != null)
							{
								RectTransform targetRectTransform = ((RectTransform) animInstance.TargetObjects[idx].transform);
								if(targetRectTransform != null)
								{
									if( targetRectTransform.DrivenByLayoutGroup() )
									{
										return true;
									}
								}
							}
						}
					}
				}
			}

			return false;
		}

		private void DrawTargetDragDropZone(System.Action<GameObject[]> a_onObjectsAdded)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect ();
			Rect rect = new Rect(0, lastRect.y + lastRect.height + 5, EditorGUIUtility.currentViewWidth, 40);

			if (rect.Contains(Event.current.mousePosition))
			{
				switch (Event.current.type)
				{
				case EventType.DragUpdated:

					if (IsValidDragPayload ())
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					}
					else
						DragAndDrop.visualMode = DragAndDropVisualMode.None;
					break;

				case EventType.DragPerform:
					var droppedGameObjectsList = new List<GameObject> ();
					for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
					{
						if (IsValidDragObject (DragAndDrop.objectReferences [i]))
						{
							droppedGameObjectsList.Add (DragAndDrop.objectReferences [i] as GameObject);
						}
					}
					a_onObjectsAdded (droppedGameObjectsList.ToArray ());
					break;
				}
			}




			GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.Height(40));
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label ("Drag & Drop UI Objects here");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
		}

		private bool IsValidDragPayload()
		{
			foreach (var v in DragAndDrop.objectReferences)
			{
				if (IsValidDragObject(v))
				{
					return true;
				}
			}
			return false;
		}

		private bool IsValidDragObject(UnityEngine.Object a_object)
		{
			var type = a_object.GetType();

			if (type == typeof(GameObject))
			{
				if (!AssetDatabase.Contains( a_object ) )
				{
					// In Hieararchy
					if (((GameObject)a_object).transform.GetType () == typeof(RectTransform))
					{
						return true;
					}

					return false;
				}
				else
				{
					// In Project View
					return false;
				}
			}
//			else
//			{
//				Debug.Log ("Not a RectTransform : " + type);
//			}

			return false;
		}

		private void DrawEffectStepAddOption(System.Action<BaseAnimationStep> a_onNewStepAdded)
		{
			BaseAnimationStep newAnimStepComponent = null;

			EditorGUILayout.BeginHorizontal ();

			string[] animStepNames = m_availableLoopAnimStepNames;

			if (m_currentlySelectedAnimStepTypeIndex >= animStepNames.Length)
			{
				m_currentlySelectedAnimStepTypeIndex = animStepNames.Length - 1;
			}

			m_currentlySelectedAnimStepTypeIndex = EditorGUILayout.Popup (m_currentlySelectedAnimStepTypeIndex, animStepNames);

			if (GUILayout.Button ("Add Effect", GUILayout.Width(150)))
			{
				Undo.RecordObject(m_uiAnimator, "Effect Step added");

				Type newStepType = m_availableLoopAnimStepTypes [m_currentlySelectedAnimStepTypeIndex];

				newAnimStepComponent = (BaseAnimationStep) Undo.AddComponent( m_uiAnimator.gameObject, newStepType);

				newAnimStepComponent.hideFlags = HideFlags.HideInInspector;

				m_uiAnimator.ResetToDefault ();

				a_onNewStepAdded (newAnimStepComponent);
			}

			EditorGUILayout.EndHorizontal ();
		}

		private void DrawTransitionStepUI(AnimSetupType a_animSetupType, SerializedProperty animationStepsListProperty, AnimationInstance a_animInstance, float a_timerOffset)
		{
			BaseAnimationStep transitionAnimStep = null;
			int stepIndex = 0;

			if (a_animSetupType != AnimSetupType.Loop && a_animInstance.AnimationSteps.Count > 0)
			{
				stepIndex = a_animSetupType == AnimSetupType.Intro ? 0 : a_animInstance.AnimationSteps.Count - 1;
				transitionAnimStep = a_animInstance.AnimationSteps [stepIndex];
			}

			EditorGUILayout.BeginHorizontal ();

			EditorGUILayout.LabelField ("Transition Step", EditorStyles.boldLabel);

			string[] animStepNames = m_availableTransitionStepNames;

			int currentSelectedIndex = 0;
			int newSelectedIndex = 0;

			if (transitionAnimStep != null)
			{
				currentSelectedIndex = m_availableTransitionStepTypes.IndexOf (transitionAnimStep.GetType()) + 1;
			}

			newSelectedIndex = EditorGUILayout.Popup (currentSelectedIndex, animStepNames);

			if (newSelectedIndex != currentSelectedIndex
#if CONFIRM_DELETIONS
				&& (currentSelectedIndex == 0 || EditorUtility.DisplayDialog("Sure?", "Are you sure you want to replace the \"" + transitionAnimStep.StepTitleDisplay + "\" step?", "Yes", "No"))
#endif
				)
			{
				// Transition step changed
				Undo.RecordObject(m_uiAnimator, "Transition Step Changed");

				// Remove existing Transition Step
				if (transitionAnimStep != null && !transitionAnimStep.IsEffectStep)
				{
					a_animInstance.AnimationSteps.Remove(transitionAnimStep);

					OnAnimationStepDeleted (transitionAnimStep);
					
					// array entry needs to be NULL for the Delete method to actually work, otherwise it will also just set it to null.
					animationStepsListProperty.GetArrayElementAtIndex (stepIndex).objectReferenceValue = null;
					animationStepsListProperty.DeleteArrayElementAtIndex (stepIndex);
				}

				// Add new transition effect step, if not 'NONE'
				if (newSelectedIndex > 0)
				{
					Type newStepType = m_availableTransitionStepTypes [newSelectedIndex - 1];

					BaseAnimationStep newAnimStepComponent = (BaseAnimationStep) Undo.AddComponent (m_uiAnimator.gameObject, newStepType);
					newAnimStepComponent.hideFlags = HideFlags.HideInInspector;
	
					m_uiAnimator.ResetToDefault ();
	
					stepIndex = a_animSetupType == AnimSetupType.Intro ? 0 : a_animInstance.AnimationSteps.Count;

					a_animInstance.AddNewAnimationStep (newAnimStepComponent, stepIndex);

					animationStepsListProperty.InsertArrayElementAtIndex (stepIndex);
					animationStepsListProperty.GetArrayElementAtIndex (stepIndex).objectReferenceValue = newAnimStepComponent;
					
					a_animInstance.SetDefaultValuesFromCurrentState ();
				}

				serializedObject.ApplyModifiedProperties ();
				EditorGUILayout.EndHorizontal ();
				return;
			}

			EditorGUILayout.EndHorizontal ();


			EditorGUILayout.Space ();


			if (transitionAnimStep != null && !transitionAnimStep.IsEffectStep && animationStepsListProperty.arraySize > stepIndex)
			{
				UnityEngine.Object animStepObjectRef = animationStepsListProperty.GetArrayElementAtIndex (stepIndex).objectReferenceValue;
				
				if (animStepObjectRef == null)
				{
					return;
				}
				
				SerializedObject animationStepSO = new SerializedObject (animStepObjectRef);
				
				EditorGUILayout.BeginVertical (EditorStyles.helpBox);
				
				EditorGUILayout.BeginHorizontal ();

				GUILayout.Space (10);

				transitionAnimStep.InspectorToggleState = EditorGUILayout.Foldout(
																					transitionAnimStep.InspectorToggleState,
																					(transitionAnimStep.InspectorToggleState ? "\u25BC" : "\u25B6") + " " + transitionAnimStep.StepTitleDisplay,
																					true,
																					EditorStyles.label);


				DrawDraggableComponentReferenceIcon( transitionAnimStep );

				GUILayout.Space(5);

				if (GUILayout.Button (new GUIContent(m_buttonMiniResetToMaster, "Reset Step to Default"), TexturedButtonGUIStyle, GUILayout.Width(20), GUILayout.Height(18)))
				{
					if(EditorUtility.DisplayDialog("Resetting Configuration", "Are you sure you want to reset the configuration of \"" + transitionAnimStep.StepTitleDisplay + "\" to it's default state?", "Yes", "No"))
					{
						transitionAnimStep.SetToMasterState ();
						transitionAnimStep.SetAllValuesFromCurrentState (a_animInstance.TargetObjects);
						
						Debug.Log ("Reset '" + transitionAnimStep.StepTitleDisplay + "' back to default");
					}
				}
				
				EditorGUILayout.EndHorizontal ();
				
				if (transitionAnimStep.InspectorToggleState)
				{
					GUILayout.Space (5);
					
					transitionAnimStep.BaseOnInspectorGUI (
						a_animInstance.TargetObjects,
						stepIndex,
						a_animSetupType,
						animationStepSO,
						a_onAnimatedStateChanged: () =>
						{
							m_uiAnimator.SetAnimationTimer (a_timerOffset + (a_animSetupType == AnimSetupType.Outro ? transitionAnimStep.TotalExecutionDuration : 0), a_forceLinearTimings: true);
						}
					);
				}
				else
				{
					animationStepSO.ApplyModifiedProperties ();
				}
				
				EditorGUILayout.EndVertical ();
				
				GUILayout.Space (5);
			}
		}

		private float DrawEffectStepsUI(AnimSetupType a_animSetupType, SerializedProperty animationStepsListProperty, AnimationInstance a_animInstance, float a_timerOffset)
		{
			EditorGUILayout.LabelField ("Effect Steps", EditorStyles.boldLabel);
			EditorGUILayout.Space ();

			BaseAnimationStep animStep = null;

			bool isFirstEffectStep = true;
			float additionalTimerOffset = 0;

			UnityEngine.Object animStepObjectRef;
			for (int sIdx = 0; sIdx < animationStepsListProperty.arraySize; sIdx++)
			{
				animStep = a_animInstance.AnimationSteps[sIdx];

				if (animStep != null && animStep.IsEffectStep)
				{
					animStepObjectRef = animationStepsListProperty.GetArrayElementAtIndex (sIdx).objectReferenceValue;

					SerializedObject animationStepSO = new SerializedObject (animStepObjectRef);

					EditorGUILayout.BeginVertical (EditorStyles.helpBox);

					EditorGUILayout.BeginHorizontal ();

					GUILayout.Space (10);

					animStep.InspectorToggleState = EditorGUILayout.Foldout (
																			animStep.InspectorToggleState,
																			(animStep.InspectorToggleState ? "\u25BC" : "\u25B6") + " " + animStep.StepTitleDisplay,
																			true,
																			EditorStyles.label);


					DrawDraggableComponentReferenceIcon(animStep);

					GUILayout.Space(5);


					if (GUILayout.Button (new GUIContent(m_buttonMiniResetToMaster, "Reset Step to Default"), TexturedButtonGUIStyle, GUILayout.Width(20), GUILayout.Height(18)))
					{
						if(EditorUtility.DisplayDialog("Resetting Configuration", "Are you sure you want to reset the configuration of \"" + animStep.StepTitleDisplay + "\" to it's default state?", "Yes", "No"))
						{
							animStep.SetToMasterState ();
							animStep.SetAllValuesFromCurrentState (a_animInstance.TargetObjects);

							Debug.Log ("Reset '" + animStep.StepTitleDisplay + "' back to default");
						}
					}

					// Display the list editing up/down/delete options
					if (UIAnimatorHelper.RenderListEditingButtons (animationStepsListProperty, ref sIdx, a_forceDisableMoveUp: isFirstEffectStep,
#if CONFIRM_DELETIONS
						a_confirmBeforeDeletion: true,
#endif
						a_onDeleteEvent: ()=>{

							Undo.RecordObject(m_uiAnimator, "Deleted Animation Step");
							OnAnimationStepDeleted (animStep);
						}))
					{
						// The list was manipulated in some way.
						serializedObject.ApplyModifiedProperties ();
						return 0;
					}

					EditorGUILayout.EndHorizontal ();

					if (animStep.InspectorToggleState )
					{
						GUILayout.Space (5);

						animStep.BaseOnInspectorGUI (
							a_animInstance.TargetObjects,
							sIdx,
							a_animSetupType,
							animationStepSO,
							a_onAnimatedStateChanged: () =>
							{
								m_uiAnimator.SetAnimationTimer (
												a_timerValue: (animStep.GetTotalExecutionDuration(0) / 2f)
																+ additionalTimerOffset
																+ a_timerOffset,
												a_forceLinearTimings: true);
							}
						);
					}
					else
					{
						animationStepSO.ApplyModifiedProperties ();
					}

					EditorGUILayout.EndVertical ();

					GUILayout.Space (5);

					isFirstEffectStep = false;
				}

				additionalTimerOffset += animStep.TotalExecutionDuration;
			}

			DrawEffectStepAddOption (a_onNewStepAdded: (BaseAnimationStep a_newAnimStep) => {

				int insertIndex = 0;

				if(a_animInstance.AnimationSteps.Count > 0)
				{
					if(a_animSetupType == AnimSetupType.Outro)
					{
						insertIndex = a_animInstance.AnimationSteps.Count;

						if(!a_animInstance.AnimationSteps[insertIndex - 1].IsEffectStep)
						{
							insertIndex--;
						}
					}
					else
					{
						// Put it at the end
						insertIndex = a_animInstance.AnimationSteps.Count;
					}
				}

				a_animInstance.AddNewAnimationStep (a_newAnimStep, insertIndex);

				serializedObject.ApplyModifiedProperties();

				a_animInstance.SetDefaultValuesFromCurrentState ();

			});

			return additionalTimerOffset;
		}

		private void DrawDraggableComponentReferenceIcon( UnityEngine.Object a_referencedObject )
		{
			GUIContent gContent = EditorGUIUtility.IconContent("cs Script Icon");

			if(gContent == null)
			{
				gContent = new GUIContent("[c#]");
			}

			gContent.tooltip = a_referencedObject.GetType().ToString();

			GUILayout.Box(gContent, BlankGUIStyle, GUILayout.Width(20), GUILayout.Height(20));

			Rect lastRect = GUILayoutUtility.GetLastRect();

			if (lastRect.Contains(Event.current.mousePosition))
			{
				if (Event.current.type == EventType.MouseDrag)
				{
					// Start an Object reference drag for Inspector assignments
					// Note: Once a Drag is started, the event type will become 'DragUpdated' instead of 'MouseDrag', so this logic only gets calls once.
					DragAndDrop.StartDrag("Animation Step Reference Drag");
					DragAndDrop.objectReferences = new UnityEngine.Object[1] { a_referencedObject };
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				}
			}
		}

		private void DrawPlaybackControls(SerializedProperty a_timerProperty, float a_currentAnimDuration, bool a_guiActive = true)
		{
			GUI.enabled = a_guiActive;

			EditorGUILayout.BeginHorizontal ();

			if (GUILayout.Button (new GUIContent(m_buttonResetToMaster, "Reset To Default State"), GUILayout.Width(m_playbackButtonSize), GUILayout.Height(m_playbackButtonSize)))
			{
				m_animatingPreview = false;
				m_uiAnimator.ResetToDefault ();

				// Required to keep the Editor view updating during in-editor previews.
				EditorUtility.SetDirty (target);
			}

			GUILayout.FlexibleSpace ();

			if (GUILayout.Button (new GUIContent(m_playbackButtonSkipToStart, "Reset To Start Of Animation"), GUILayout.Width(m_playbackButtonSize), GUILayout.Height(m_playbackButtonSize)))
			{
				m_animatingPreview = false;
				m_uiAnimator.ResetToStart ();

				// Required to keep the Editor view updating during in-editor previews.
				EditorUtility.SetDirty (target);
			}

			if (GUILayout.Button (m_animatingPreview ? new GUIContent(m_playbackButtonPause, "Pause Animation") : new GUIContent(m_playbackButtonPlay, "Preview Animation"), GUILayout.Width(m_playbackButtonSize), GUILayout.Height(m_playbackButtonSize)))
			{
				if (m_animatingPreview)
				{
					// Pause the animation
					m_animatingPreview = false;
				}
				else
				{
					// Play/resume the animation
					m_lastTime = EditorApplication.timeSinceStartup;
					m_animatingPreview = true;

					if (m_animationFinished)
					{
						m_uiAnimator.ResetToStart ();
					}

					m_animationFinished = false;
				}
			}

			if (GUILayout.Button (new GUIContent(m_playbackButtonSkipToEnd, "Skip To End Of Animation"), GUILayout.Width(m_playbackButtonSize), GUILayout.Height(m_playbackButtonSize)))
			{
				m_animatingPreview = false;
				m_uiAnimator.ResetToEnd();

				// Required to keep the Editor view updating during in-editor previews.
				EditorUtility.SetDirty (target);
			}

			GUILayout.FlexibleSpace ();

			if (GUILayout.Button (new GUIContent(m_buttonSetStateAsMaster, "Set As Default State"), GUILayout.Width(m_playbackButtonSize), GUILayout.Height(m_playbackButtonSize)))
			{
				if(EditorUtility.DisplayDialog("Updating *Default* state of the UI layout", "Are you sure you want to save the current state of all the target UI objects as the *Default* state, to animate to and from?\n\nThis should be used when you've changed the UI, and want the UI Animator to cache this new state of the UI. Otherwise, it will still try to animate to/from the old layout.", "Yes", "No"))
				{
					m_uiAnimator.GrabCurrentStateAsMaster ();
				}
			}

			EditorGUILayout.EndHorizontal ();


			// Draw animation slider
			EditorGUILayout.BeginHorizontal ();

			EditorGUI.BeginChangeCheck ();

			float sliderTimerValue = EditorGUILayout.Slider ((float) Mathf.Min(a_timerProperty.floatValue, a_currentAnimDuration), 0, a_currentAnimDuration);

			if (EditorGUI.EndChangeCheck ())
			{
				a_timerProperty.floatValue = sliderTimerValue;

				m_uiAnimator.SetAnimationTimer (sliderTimerValue);
			}

			EditorGUILayout.LabelField ("/ " + Math.Round( a_currentAnimDuration, 2) + "s", GUILayout.Width(50));

			EditorGUILayout.EndHorizontal ();

			GUI.enabled = true;
		}

		private void OnAnimationStageDeleted(AnimationStage a_deletedAnimStage)
		{
			// Reset the state of all affected targets to their master state

			for (int iIdx = 0; iIdx < a_deletedAnimStage.AnimationInstances.Count; iIdx++)
			{
				for (int sIdx = 0; sIdx < a_deletedAnimStage.AnimationInstances[iIdx].AnimationSteps.Count; sIdx++)
				{
					OnAnimationStepDeleted (a_deletedAnimStage.AnimationInstances[sIdx].AnimationSteps[sIdx]);
				}
			}
		}

		private void OnAnimationStepDeleted(BaseAnimationStep a_deletedAnimStep)
		{
			// Reset the state of all affected targets to their master state
			a_deletedAnimStep.SetToMasterState();

			// Destroy the component
			Undo.DestroyObjectImmediate( a_deletedAnimStep );
		}

		private List<System.Type> GetListOfSubTypes<T>() where T : class
		{
			List<System.Type> objects = new List<System.Type>();
			foreach (Type type in Assembly.GetAssembly(typeof(T)).GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)) ))
			{
				objects.Add(type);
			}
			return objects;
		}
	}
}