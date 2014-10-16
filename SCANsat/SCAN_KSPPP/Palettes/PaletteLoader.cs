using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCANsat.Platform.Palettes
{
	static class PaletteLoader
	{
		private static List<Palette> palettes = new List<Palette>();

		internal static void generatePalettes(Palette.Kind Kind, int Size)
		{
			palettes.Clear();
			if (Kind == Palette.Kind.Diverging)
			{
				palettes.Add(ColorBrewer.Palettes.Spectral(Size));
				palettes.Add(ColorBrewer.Palettes.RdYlGn(Size));
				palettes.Add(ColorBrewer.Palettes.RdBu(Size));
				palettes.Add(ColorBrewer.Palettes.PiYG(Size));
				palettes.Add(ColorBrewer.Palettes.PRGn(Size));
				palettes.Add(ColorBrewer.Palettes.RdYlBu(Size));
				palettes.Add(ColorBrewer.Palettes.BrBG(Size));
				palettes.Add(ColorBrewer.Palettes.RdGy(Size));
				palettes.Add(ColorBrewer.Palettes.PuOr(Size));
			}
			else if (Kind == Palette.Kind.Qualitative)
			{
				palettes.Add(ColorBrewer.Palettes.Set2(Size));
				palettes.Add(ColorBrewer.Palettes.Accent(Size));
				palettes.Add(ColorBrewer.Palettes.Set1(Size));
				palettes.Add(ColorBrewer.Palettes.Set3(Size));
				palettes.Add(ColorBrewer.Palettes.Dark2(Size));
				palettes.Add(ColorBrewer.Palettes.Paired(Size));
				palettes.Add(ColorBrewer.Palettes.Pastel2(Size));
				palettes.Add(ColorBrewer.Palettes.Pastel1(Size));
			}
			else if (Kind == Palette.Kind.Sequential)
			{
				palettes.Add(ColorBrewer.Palettes.OrRd(Size));
				palettes.Add(ColorBrewer.Palettes.BuPu(Size));
				palettes.Add(ColorBrewer.Palettes.Oranges(Size));
				palettes.Add(ColorBrewer.Palettes.BuGn(Size));
				palettes.Add(ColorBrewer.Palettes.YlOrBr(Size));
				palettes.Add(ColorBrewer.Palettes.YlGn(Size));
				palettes.Add(ColorBrewer.Palettes.Reds(Size));
				palettes.Add(ColorBrewer.Palettes.RdPu(Size));
				palettes.Add(ColorBrewer.Palettes.Greens(Size));
				palettes.Add(ColorBrewer.Palettes.YlGnBu(Size));
				palettes.Add(ColorBrewer.Palettes.Purples(Size));
				palettes.Add(ColorBrewer.Palettes.GnBu(Size));
				palettes.Add(ColorBrewer.Palettes.Greys(Size));
				palettes.Add(ColorBrewer.Palettes.YlOrRd(Size));
				palettes.Add(ColorBrewer.Palettes.PuRd(Size));
				palettes.Add(ColorBrewer.Palettes.Blues(Size));
				palettes.Add(ColorBrewer.Palettes.PuBuGn(Size));
			}
		}
	}
}
