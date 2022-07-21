using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIAnimatorCore
{
	[System.Serializable]
	public class UIGraphicRendererTargetData
	{
		[SerializeField]
		private Graphic[] m_graphics;

		[SerializeField]
		private float[] m_masterAlphas;

		public int NumRenderers { get { return m_graphics != null ? m_graphics.Length : 0; } }

		public UIGraphicRendererTargetData(Graphic[] a_graphics, UIGraphicRendererTargetData a_masterSetup = null)
		{
			m_graphics = new Graphic[a_graphics.Length];

			for(int idx=0; idx < a_graphics.Length; idx++)
			{
				m_graphics[idx] = a_graphics[idx];
			}

			m_masterAlphas = new float[NumRenderers];

			for(int idx=0; idx < NumRenderers; idx++)
			{
				if(a_masterSetup != null && a_masterSetup.m_masterAlphas != null && a_masterSetup.m_masterAlphas.Length > 0 )
				{
					m_masterAlphas[idx] = a_masterSetup.m_masterAlphas[ Mathf.Min(idx, a_masterSetup.m_masterAlphas.Length - 1) ];
				}
				else
				{
					m_masterAlphas[idx] = GetRendererAlpha(idx);
				}
			}
		}

		public float GetRendererAlpha (int a_rendererIndex)
		{
			if (m_graphics == null || a_rendererIndex >= m_graphics.Length || m_graphics [a_rendererIndex] == null)
			{
				return 1f;
			}

			return m_graphics [a_rendererIndex].color.a;
		}

		public void SetRendererAlpha (int a_rendererIndex, float a_alphaValue)
		{
			if (m_graphics == null || a_rendererIndex >= m_graphics.Length || m_graphics [a_rendererIndex] == null)
			{
				return;
			}

			Color cachedColor = m_graphics [a_rendererIndex].color;
			cachedColor.a = a_alphaValue;
			m_graphics [a_rendererIndex].color = cachedColor;
		}

		public void SetToMasterValues()
		{
			for (int idx = 0; idx < NumRenderers; idx++)
			{
				SetRendererAlpha (idx, m_masterAlphas[idx]);
			}
		}

		public void SetFadeProgress(float a_animatedAlpha, float a_progress)
		{
			for (int idx = 0; idx < NumRenderers; idx++)
			{
				SetRendererAlpha(idx, Mathf.LerpUnclamped (a_animatedAlpha, m_masterAlphas[idx], a_progress));
			}
		}
	}
}