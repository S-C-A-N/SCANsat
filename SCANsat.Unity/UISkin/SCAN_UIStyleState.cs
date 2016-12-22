using System;
using UnityEngine;

namespace SCANsat.Unity.UISkin
{
	[Serializable]
	public class SCAN_UIStyleState
	{
		[SerializeField]
		private Sprite m_Background = null;

		public Sprite Background
		{
			get { return m_Background; }
		}
	}
}
