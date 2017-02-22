#region license
/*  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * CanvasFader - Monobehaviour for making smooth fade in and fade out for UI windows
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 DMagic
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

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
		[SerializeField]
		private InputField m_RInputField = null;
		[SerializeField]
		private InputField m_GInputField = null;
		[SerializeField]
		private InputField m_BInputField = null;

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

			if (m_RInputField != null)
				m_RInputField.text = "";

			if (m_GInputField != null)
				m_GInputField.text = "";

			if (m_BInputField != null)
				m_BInputField.text = "";
		}

		public void ColorOne(bool isOn)
		{
			if (m_ColorOne == null)
				return;

			m_ColorOne.IsActive = isOn;

			if (isOn)
			{
				picker.CurrentColor = m_ColorOne.CurrentColor;

				if (m_RInputField != null)
					m_RInputField.text = "";

				if (m_GInputField != null)
					m_GInputField.text = "";

				if (m_BInputField != null)
					m_BInputField.text = "";
			}
		}

		public void ColorTwo(bool isOn)
		{
			if (m_ColorTwo == null)
				return;

			m_ColorTwo.IsActive = isOn;

			if (isOn)
			{
				picker.CurrentColor = m_ColorTwo.CurrentColor;

				if (m_RInputField != null)
					m_RInputField.text = "";

				if (m_GInputField != null)
					m_GInputField.text = "";

				if (m_BInputField != null)
					m_BInputField.text = "";
			}
		}

	}
}
