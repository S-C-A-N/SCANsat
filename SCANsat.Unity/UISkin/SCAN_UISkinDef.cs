using System;
using UnityEngine;

namespace SCANsat.Unity.UISkin
{
	[Serializable]
	public class SCAN_UISkinDef
	{
		[SerializeField]
		private string m_SkinName = "";
		[SerializeField]
		private SCAN_UIStyle m_Window = new SCAN_UIStyle();
		[SerializeField]
		private SCAN_UIStyle m_Box = new SCAN_UIStyle();
		[SerializeField]
		private SCAN_UIStyle m_Button = new SCAN_UIStyle();
		[SerializeField]
		private SCAN_UIStyle m_Toggle = new SCAN_UIStyle();
		[SerializeField]
		private SCAN_UIStyle m_TextField = new SCAN_UIStyle();
		[SerializeField]
		private SCAN_UIStyle m_HorizontalScrollbar = new SCAN_UIStyle();
		[SerializeField]
		private SCAN_UIStyle m_HorizontalScrollbarThumb = new SCAN_UIStyle();
		[SerializeField]
		private SCAN_UIStyle m_VerticalScrollbar = new SCAN_UIStyle();
		[SerializeField]
		private SCAN_UIStyle m_VerticalScrollbarThumb = new SCAN_UIStyle();
		[SerializeField]
		private SCAN_UIStyle m_HorizontalSlider = new SCAN_UIStyle();
		[SerializeField]
		private SCAN_UIStyle m_HorizontalSliderThumb = new SCAN_UIStyle();
		[SerializeField]
		private SCAN_UIStyle m_VerticalSlider = new SCAN_UIStyle();
		[SerializeField]
		private SCAN_UIStyle m_VerticalSliderThumb = new SCAN_UIStyle();

		public string SkinName
		{
			get { return m_SkinName; }
		}

		public SCAN_UIStyle Window
		{
			get { return m_Window; }
		}

		public SCAN_UIStyle Box
		{
			get { return m_Box; }
		}

		public SCAN_UIStyle Button
		{
			get { return m_Button; }
		}

		public SCAN_UIStyle Toggle
		{
			get { return m_Toggle; }
		}

		public SCAN_UIStyle TextField
		{
			get { return m_TextField; }
		}

		public SCAN_UIStyle HorizontalScrollbar
		{
			get { return m_HorizontalScrollbar; }
		}

		public SCAN_UIStyle HorizontalScrollbarThumb
		{
			get { return m_HorizontalScrollbarThumb; }
		}

		public SCAN_UIStyle VerticalScrollbar
		{
			get { return m_VerticalScrollbar; }
		}

		public SCAN_UIStyle VerticalScrollbarThumb
		{
			get { return m_VerticalScrollbarThumb; }
		}

		public SCAN_UIStyle HorizontalSlider
		{
			get { return m_HorizontalSlider; }
		}

		public SCAN_UIStyle HorizontalSliderThumb
		{
			get { return m_HorizontalSliderThumb; }
		}

		public SCAN_UIStyle VerticalSlider
		{
			get { return m_VerticalSlider; }
		}

		public SCAN_UIStyle VerticalSliderThumb
		{
			get { return m_VerticalSliderThumb; }
		}

	}
}
