using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UIAnimatorCore
{
	[System.Serializable]
	public class AnimationSetup
	{
		[SerializeField]
		private List<AnimationStage> m_animationStages;

		[SerializeField]
		private UnityEvent m_onStartAction = null;

		[SerializeField]
		private UnityEvent m_onFinishedAction = null;

		public List<AnimationStage> AnimationStages { get { return m_animationStages; } }
		public UnityEvent OnFinishedAction { get { return m_onFinishedAction; } }
		public UnityEvent OnStartAction { get { return m_onStartAction; } }

		public AnimationSetup()
		{
			m_animationStages = new List<AnimationStage>(){new AnimationStage()};
		}
	}
}