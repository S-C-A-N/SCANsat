﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SCANsat.Unity.Unity
{
	public class SCAN_DropDownElement : MonoBehaviour
	{
		[SerializeField]
		private TextHandler m_ElementTitle = null;
		[SerializeField]
		private Color m_HighlightColor = Color.white;

		private SCAN_DropDown dropdown;
		private string title;

		public void Setup(string element, bool current, SCAN_DropDown parent)
		{
			if (parent == null)
				return;

			dropdown = parent;
			title = element;

			if (m_ElementTitle != null)
			{
				m_ElementTitle.OnTextUpdate.Invoke(element);

				if (current)
					m_ElementTitle.OnColorUpdate.Invoke(m_HighlightColor);
			}
		}

		public void Select()
		{
			dropdown.SetElement(title);
		}

	}
}