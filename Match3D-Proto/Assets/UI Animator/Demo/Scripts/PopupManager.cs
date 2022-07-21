using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIAnimatorCore;

namespace UIAnimatorDemo
{
	public class PopupManager : MonoBehaviour {

		[SerializeField]
		private UIAnimator m_popupUIAnimator = null;

		[SerializeField]
		private PopupButtons m_popupButtons = null;

		private DemoUIManager m_uiManager;

		public void Init(DemoUIManager a_uiManager)
		{
			m_uiManager = a_uiManager;

			m_popupButtons.Init (a_uiManager);
			m_popupUIAnimator.gameObject.SetActive (false);
		}

		public void ShowPopup()
		{
			m_popupUIAnimator.gameObject.SetActive (true);

			m_popupUIAnimator.PlayAnimation (AnimSetupType.Intro);

			if (m_uiManager.PanelManager.CurrentPanel != null)
			{
				m_uiManager.PanelManager.CurrentPanel.UIAnimator.Paused = true;
			}
		}

		public void ClosePopup(System.Action a_onClosed = null)
		{
			m_popupUIAnimator.PlayAnimation (AnimSetupType.Outro, a_onFinish: () =>{

				m_popupUIAnimator.gameObject.SetActive (false);

				if (m_uiManager.PanelManager.CurrentPanel != null)
				{
					m_uiManager.PanelManager.CurrentPanel.UIAnimator.Paused = false;
				}

				if(a_onClosed != null)
				{
					a_onClosed();
				}
			});
		}
	}
}