using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIAnimatorDemo
{
	partial class PanelNames
	{
		public const string FLASH_BANG_RESULTS = "FLASH_BANG_RESULTS";
	}

	public class FlashBangResults : BasePanel {

		public override string PanelName { get { return PanelNames.FLASH_BANG_RESULTS; } }

		[SerializeField]
		private MenuButtons m_menuButtons = null;

		public override void Init()
		{
			m_menuButtons.Init (m_uiManager);
		}
	}
}