using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UIAnimatorCore;

namespace UIAnimatorDemo
{
	public class AudioSetting : MonoBehaviour, IPointerClickHandler
	{
		public Image m_audioEnabledState;

		public Image m_audioMutedState;

		private bool m_isAudioCurrentlyEnabled = true;

		private const string c_audioEnabledStatePlayerPrefKey = "UIAnimatorDemo_AudioEnabled";

		private void Awake ()
		{
			// Get the persisted state of the audio setting from the PlayerPrefs
			SetAudioPlayingState( PlayerPrefs.GetInt (c_audioEnabledStatePlayerPrefKey, 1) == 1 );
		}
		
		public void OnPointerClick(PointerEventData eventData)
		{
			SetAudioPlayingState ( !m_isAudioCurrentlyEnabled );
		}

		private void SetAudioPlayingState(bool a_isAudioEnabled)
		{
			m_isAudioCurrentlyEnabled = a_isAudioEnabled;

			// Setup the correct sprite to show on the button
			m_audioEnabledState.gameObject.SetActive(m_isAudioCurrentlyEnabled);
			m_audioMutedState.gameObject.SetActive(!m_isAudioCurrentlyEnabled);

			// Set the UIAmimator's audio mute state
			UIAnimator.SetUIAudioState(m_isAudioCurrentlyEnabled);

			// Persist the state in PlayerPrefs
			PlayerPrefs.SetInt (c_audioEnabledStatePlayerPrefKey, m_isAudioCurrentlyEnabled ? 1 : 0);
		}
	}
}