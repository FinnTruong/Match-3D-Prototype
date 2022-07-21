using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIAnimatorCore;

namespace UIAnimatorDemo
{
    public class RuntimeAnimationController : MonoBehaviour
    {
        [SerializeField]
        private Slider m_yPosSlider;

        [SerializeField]
        private Slider m_scaleSlider;

        [SerializeField]
        private Slider m_rotationSlider;

        [SerializeField]
        private float m_yPosMin = 0;

        [SerializeField]
        private float m_yPosMax = 100;

        [SerializeField]
        private float m_scaleMin = 0;

        [SerializeField]
        private float m_scaleMax = 1;

        [SerializeField]
        private float m_rotationMin = 0;

        [SerializeField]
        private float m_rotationMax = 360;

        [SerializeField]
        private UIAnimator m_uiAnimator;

        [SerializeField]
        private TransformAndFadeAlphaAnimationStep m_transformAndFadeAnimStep;

        // Start is called before the first frame update
        void Start()
        {
            m_yPosSlider.onValueChanged.AddListener( OnYPosSliderValueChanged );
            m_scaleSlider.onValueChanged.AddListener( OnScaleSliderValueChanged );
            m_rotationSlider.onValueChanged.AddListener( OnRotationSliderValueChanged );

        }

        private void OnYPosSliderValueChanged(float a_value)
        {
            Vector3 posValue = m_transformAndFadeAnimStep.Position.Value;

            m_transformAndFadeAnimStep.Position.SetValue( new Vector3(posValue.x, Mathf.Lerp(m_yPosMin, m_yPosMax, a_value), posValue.z) );

            m_uiAnimator.PlayAnimation(AnimSetupType.Intro);
        }

        private void OnScaleSliderValueChanged(float a_value)
        {
            float scaleValue = Mathf.Lerp(m_scaleMin, m_scaleMax, a_value);

            m_transformAndFadeAnimStep.Scale.SetValue(new Vector3(scaleValue, scaleValue, scaleValue));

            m_uiAnimator.PlayAnimation(AnimSetupType.Intro);
        }

        private void OnRotationSliderValueChanged(float a_value)
        {
            m_transformAndFadeAnimStep.Rotation.SetValue(new Vector3(0, 0, Mathf.Lerp(m_rotationMin, m_rotationMax, a_value)));

            m_uiAnimator.PlayAnimation(AnimSetupType.Intro);
        }
    }
}