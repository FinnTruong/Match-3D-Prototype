using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIAnimatorCore;

namespace UIAnimatorDemo
{
	public abstract class BasePanel : MonoBehaviour
	{
		[SerializeField]
		protected UIAnimator m_uiAnimator;

		protected DemoUIManager m_uiManager;

		public abstract string PanelName { get; }

		public virtual bool IsDefaultPanel { get { return false; } }

		public UIAnimator UIAnimator { get { return m_uiAnimator; } }

		public void InitBase(DemoUIManager a_uiManager)
		{
			m_uiManager = a_uiManager;

			Init ();
		}

		public virtual void Init ()
		{

		}

		public virtual void Close()
		{
			gameObject.SetActive (false);
		}

		public virtual void Open()
		{
			gameObject.SetActive (true);
		}
	}
}