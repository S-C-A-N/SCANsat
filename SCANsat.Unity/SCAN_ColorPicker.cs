using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SCANsat.Unity.HSVPicker.UI;

namespace SCANsat.Unity
{
	public class SCAN_ColorPicker : MonoBehaviour
	{
		[SerializeField]
		private ColorImage m_ColorOne = null;
		[SerializeField]
		private ColorImage m_ColorTwo = null;
		[SerializeField]
		private Image m_OldColorOne = null;
		[SerializeField]
		private Image m_OldColorTwo = null;

		private ColorPicker picker;

		public Color GetColorOne
		{
			get { return m_ColorOne.CurrentColor; }
		}

		public Color GetColorTwo
		{
			get { return m_ColorTwo.CurrentColor; }
		}

		private void Awake()
		{
			picker = GetComponent<ColorPicker>();
		}

		public void Setup(Color one, Color two, bool reset)
		{
			if (picker != null && reset)
				picker.CurrentColor = one;

			if (m_ColorOne != null)
			{
				m_ColorOne.SetColor(one);

				if (reset)
					m_ColorOne.IsActive = true;
			}

			if (m_ColorTwo != null)
				m_ColorTwo.SetColor(two);

			if (m_OldColorOne != null)
				m_OldColorOne.color = one;

			if (m_OldColorTwo != null)
				m_OldColorTwo.color = two;
		}

		public void ColorOne(bool isOn)
		{
			if (m_ColorOne == null)
				return;

			m_ColorOne.IsActive = isOn;

			if (isOn)
				picker.CurrentColor = m_ColorOne.CurrentColor;
		}

		public void ColorTwo(bool isOn)
		{
			if (m_ColorTwo == null)
				return;

			m_ColorTwo.IsActive = isOn;

			if (isOn)
				picker.CurrentColor = m_ColorTwo.CurrentColor;
		}

		public void Apply()
		{
			if (m_OldColorOne != null && m_ColorOne != null)
				m_OldColorOne.color = m_ColorOne.CurrentColor;

			if (m_OldColorTwo != null && m_ColorTwo != null)
				m_OldColorTwo.color = m_ColorTwo.CurrentColor;
		}
	}
}
