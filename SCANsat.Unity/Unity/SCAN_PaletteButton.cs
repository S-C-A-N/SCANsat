using System;
using UnityEngine;
using UnityEngine.UI;

namespace SCANsat.Unity.Unity
{
	public class SCAN_PaletteButton : MonoBehaviour
	{
		[SerializeField]
		private RawImage m_Palette = null;

		private string paletteName;
		private SCAN_ColorAltimetry color;

		public void setup(Texture2D tex, string palette, SCAN_ColorAltimetry c)
		{
			if (m_Palette == null || c == null)
				return;

			m_Palette.texture = tex;
			paletteName = palette;
			color = c;
		}

		public void Select()
		{
			if (color == null)
				return;

			color.SetPalette(paletteName);
		}
	}
}
