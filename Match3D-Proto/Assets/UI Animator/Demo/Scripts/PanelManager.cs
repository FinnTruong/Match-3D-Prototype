using System.Collections.Generic;
using UnityEngine;
using UIAnimatorCore;

namespace UIAnimatorDemo
{
	public class PanelManager : MonoBehaviour {

		private Dictionary<string, BasePanel> m_panelsLookup;

		private BasePanel m_currentPanel;

		public BasePanel CurrentPanel { get { return m_currentPanel; } }

		public void Init(DemoUIManager a_uiManager)
		{
			BasePanel[] m_panels = GetComponentsInChildren<BasePanel> (includeInactive: true);

			m_panelsLookup = new Dictionary<string, BasePanel> ();

			BasePanel defaultPanel = null;

			for (int idx = 0; idx < m_panels.Length; idx++)
			{
				m_panelsLookup.Add (m_panels [idx].PanelName, m_panels [idx]);

				m_panels [idx].InitBase (a_uiManager);

				m_panels [idx].gameObject.SetActive (false);

				if (m_panels [idx].IsDefaultPanel)
				{
					defaultPanel = m_panels [idx];
				}
			}

			if (defaultPanel != null)
			{
				// Open default panel straight away
				OpenPanel(defaultPanel.PanelName);
			}
		}
		

		public void OpenPanel(string a_panelName, bool a_playOutro = true)
		{
			if (m_currentPanel == null)
			{
				OpenPanelWithName (a_panelName);
				return;
			}

			if (a_playOutro && m_currentPanel.UIAnimator != null)
			{
				m_currentPanel.UIAnimator.PlayAnimation (AnimSetupType.Outro, () =>
				{
					// Close now that the OUTRO anim has finished
					m_currentPanel.Close();

					OpenPanelWithName (a_panelName);
				});
			}
			else
			{
				// Close the current active panel immediately
				m_currentPanel.Close();

				OpenPanelWithName (a_panelName);
			}
		}

		private void OpenPanelWithName(string a_panelName)
		{
			if (m_panelsLookup.ContainsKey (a_panelName))
			{
				m_currentPanel = m_panelsLookup [a_panelName];

				m_currentPanel.Open ();

				m_currentPanel.UIAnimator.PlayAnimation (AnimSetupType.Intro);
			}
			else
			{
				Debug.LogWarning ("OpenPanelWithName() - No panel found with name '" + a_panelName + "'");
			}
		}
	}
}