using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAnimatorCore
{
	[System.Serializable]
	public abstract class EffectAnimationStep : BaseAnimationStep
	{
		public override bool IsEffectStep { get { return true; } }
	}
}