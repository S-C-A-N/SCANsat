using System;
using UnityEngine;

namespace SCANsat.Unity.UISkin
{
	[Serializable]
	public class SCAN_UIStyle
	{
		[SerializeField]
		private string m_Name = "";
		[SerializeField]
		private SCAN_UIStyleState m_Normal = new SCAN_UIStyleState();
		[SerializeField]
		private SCAN_UIStyleState m_Highlight = new SCAN_UIStyleState();
		[SerializeField]
		private SCAN_UIStyleState m_Active = new SCAN_UIStyleState();
		[SerializeField]
		private SCAN_UIStyleState m_CheckMark = new SCAN_UIStyleState();

		public string Name
		{
			get { return m_Name; }
		}

		public SCAN_UIStyleState Normal
		{
			get { return m_Normal; }
		}

		public SCAN_UIStyleState Highlight
		{
			get { return m_Highlight; }
		}

		public SCAN_UIStyleState Active
		{
			get { return m_Active; }
		}
		public SCAN_UIStyleState CheckMark
		{
			get { return m_CheckMark; }
		}
	}
}
