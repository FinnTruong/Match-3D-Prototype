using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIAnimatorDemo
{
	partial class PanelNames
	{
		public const string HIGHSCORE_RESULTS = "HIGHSCORE_RESULTS";
	}

	public class HighscoreResults : BasePanel {

		public override string PanelName { get { return PanelNames.HIGHSCORE_RESULTS; } }

		[SerializeField]
		private MenuButtons m_menuButtons = null;

		public override void Init()
		{
			m_menuButtons.Init (m_uiManager);
		}
	}
}