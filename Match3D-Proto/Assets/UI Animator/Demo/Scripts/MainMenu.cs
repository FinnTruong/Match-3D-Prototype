using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIAnimatorDemo
{
	partial class PanelNames
	{
		public const string MAIN_MENU = "MAIN_MENU";
	}

	public class MainMenu : BasePanel {

		public override bool IsDefaultPanel { get { return true; } }

		public override string PanelName { get { return PanelNames.MAIN_MENU; } }

		[SerializeField]
		private MenuButtons m_menuButtons = null;

		public override void Init()
		{
			m_menuButtons.Init (m_uiManager);
		}
	}
}