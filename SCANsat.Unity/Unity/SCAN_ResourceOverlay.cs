using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SCANsat.Unity.Unity
{
	public class SCAN_ResourceOverlay : MonoBehaviour
	{
		[SerializeField]
		private TextHandler m_ResourceName = null;
		[SerializeField]
		private Toggle m_ResourceToggle = null;

		private SCAN_Overlay parent;
		private string resource;
		private bool loaded;

		public void SetResource(string name, SCAN_Overlay p, bool isOn)
		{
			if (m_ResourceName == null || p == null)
				return;

			parent = p;
			resource = name;

			if (m_ResourceToggle != null)
				m_ResourceToggle.isOn = isOn;

			m_ResourceName.OnTextUpdate.Invoke(name);

			loaded = true;
		}

		public void ToggleResource(bool isOn)
		{
			if (!loaded || parent == null)
				return;

			parent.SetResource(resource, isOn);
		}
	}
}
