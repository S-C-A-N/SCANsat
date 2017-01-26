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

		private SCAN_Overlay parent;
		private string resource;
		private bool active;

		public string Resource
		{
			get { return resource; }
		}

		public void SetResource(string name, SCAN_Overlay p, bool isOn)
		{
			if (m_ResourceName == null || p == null)
				return;

			parent = p;
			resource = name;

			m_ResourceName.OnTextUpdate.Invoke(name);

			if (isOn && p.OverlayInterface.DrawResource)
			{
				m_ResourceName.OnColorUpdate.Invoke(p.ActiveColor);
				m_ResourceName.SetNormalColor(p.ActiveColor);
			}
		}

		public void DrawResource()
		{
			if (parent == null)
				return;

			active = !active;

			parent.SetResource(resource, active);

			if (m_ResourceName != null)
				m_ResourceName.SetNormalColor(parent.ActiveColor);
		}

		public void Inactivate()
		{
			active = false;

			if (m_ResourceName != null)
			{
				m_ResourceName.OnColorUpdate.Invoke(parent.NormalColor);
				m_ResourceName.SetNormalColor(parent.NormalColor);
			}
		}
	}
}
