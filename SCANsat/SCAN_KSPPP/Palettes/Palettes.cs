using System;
using UnityEngine;


namespace SCANsat.Platform.Palettes
{
	public class Palettes
	{
		internal Palette defaultPalette;
		internal Palette[] availablePalettes;
		internal Palette.Kind paletteType;
		int paletteSize;

		public Palettes (Palette[] p, Palette.Kind pK, int i)
		{
			availablePalettes = p;
			paletteType = pK;
			paletteSize = i;
		}
	}
}

