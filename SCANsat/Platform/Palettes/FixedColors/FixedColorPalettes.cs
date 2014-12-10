using System;

using UnityEngine;

namespace SCANsat.Platform.Palettes.FixedColors
{
	public static class FixedColorPalettes
	{
		static Func<byte, byte, byte, Color32> RGB = (r, g, b) => new Color32(r, g, b, 255);

		/* Wikipedia color scheme licensed under Creative Commons Attribution-Share Alike 3.0 Unported license
		 * Black Forest Scheme - Schwarzwald-topographie - http://commons.wikimedia.org/wiki/File:Schwarzwald-topographie.png
		 * */
		public static Palette blackForest()
		{
			Color32[] c;
			c = new[] { RGB(176, 243, 190), RGB(224, 251, 178), RGB(184, 222, 118), RGB(39, 165, 42), RGB(52, 136, 60), RGB(156,164,41), RGB(248,176,4), RGB(192,74,2), RGB(135,8,0), RGB(116,24,5), RGB(108,42,10), RGB(125,74,43), RGB(156,129,112), RGB(181,181,181), RGB(218,216,218)};
			return new Palette(c, "Black Forest", Palette.Kind.Fixed, (Palette.Is)3, (Palette.Is)3, (Palette.Is)3, (Palette.Is)3);
		}

		/* Wikipedia color scheme licensed under Creative Commons Attribution-Share Alike 3.0 Unported license
		 * Pakistan Scheme - http://commons.wikimedia.org/wiki/User:Jarke
		 * */
		public static Palette pakistan()
		{
			Color32[] c;
			c = new[] { RGB(218,228,201), RGB(233,234,194), RGB(240,236,209), RGB(242,227,189), RGB(237,207,162), RGB(226,197,150), RGB(202,186,224), RGB(223,214,236), RGB(243,243,255) };
			return new Palette(c, "Pakistan", Palette.Kind.Fixed, (Palette.Is)3, (Palette.Is)3, (Palette.Is)3, (Palette.Is)3);
		}

		//Size *8*
		public static Palette lithuania()
		{
			Color32[] c;
			c = new[] { RGB(74, 173, 90), RGB(116, 195, 83), RGB(181, 214, 99), RGB(222, 222, 99), RGB(255, 231, 16), RGB(255, 206, 8), RGB(255, 156, 8), RGB(255, 123, 16) };
			return new Palette(c, "Lithuania", Palette.Kind.Fixed, (Palette.Is)3, (Palette.Is)3, (Palette.Is)3, (Palette.Is)3);
		}

		/* Wikipedia color scheme licensed under Creative Commons Attribution-Share Alike 3.0 Unported license
		 * Mars color scheme by PZmaps - http://commons.wikimedia.org/wiki/User:PZmaps
		 * */
		public static Palette mars()
		{
			Color32[] c;
			c = new[] { RGB(120, 65, 20), RGB(141, 84, 43), RGB(164, 114, 66), RGB(188, 144, 89), RGB(211, 174, 112), RGB(235, 205, 136), RGB(219, 185, 120), RGB(204, 165, 105), RGB(189, 145, 90), RGB(174, 125, 75), RGB(159, 110, 70), RGB(160, 120, 80), RGB(174, 134, 100), RGB(189, 163, 140), RGB(204, 191, 180), RGB(220, 212, 205), RGB(240, 232, 225), RGB(255, 255, 255) };
			return new Palette(c, "Mars", Palette.Kind.Fixed, (Palette.Is)3, (Palette.Is)3, (Palette.Is)3, (Palette.Is)3);
		}

		//A public domain Wikipedia color scheme - http://soliton.vm.bytemark.co.uk/pub/cpt-city/wkp/template/index.html
		public static Palette wiki2()
		{
			Color32[] c;
			c = new[] { RGB(113, 171, 216), RGB(216, 242, 254), RGB(148, 191, 139), RGB(239, 235, 192), RGB(170, 135, 83), RGB(245, 244, 242) };
			return new Palette(c, "Wiki2", Palette.Kind.Fixed, (Palette.Is)3, (Palette.Is)3, (Palette.Is)3, (Palette.Is)3);
		}

		/* Wikipedia color scheme licensed under Creative Commons Attribution-Share Alike 3.0 Unported license
		 * Plumbago Scheme- http://en.wikipedia.org/wiki/File:AYool_topography_15min.png
		 * */
		public static Palette plumbago()
		{
			Color32[] c;
			c = new[] { RGB(151, 0, 176), RGB(23, 0, 151), RGB(203, 254, 254), RGB(0, 168, 0), RGB(254, 254, 126), RGB(87, 36, 36), RGB(203, 101, 203), RGB(228, 190, 228) };
			return new Palette(c, "Plumbago", Palette.Kind.Fixed, (Palette.Is)3, (Palette.Is)3, (Palette.Is)3, (Palette.Is)3);
		}

		/* A freely available color scheme by Meghan Miller
		 * http://soliton.vm.bytemark.co.uk/pub/cpt-city/cw/index.html
		 * */
		public static Palette cw1_013()
		{
			Color32[] c;
			c = new[] { RGB(97, 65, 78), RGB(226, 87, 68), RGB(252, 255, 67), RGB(86, 118, 157), RGB(167, 214, 255) };
			return new Palette(c, "cw1_013", Palette.Kind.Fixed, (Palette.Is)3, (Palette.Is)3, (Palette.Is)3, (Palette.Is)3);
		}

		/* A freely available color scheme from Arendal
		 * http://soliton.vm.bytemark.co.uk/pub/cpt-city/arendal/copying.html
		 * */
		public static Palette arctic()
		{
			Color32[] c;
			c = new[] { RGB(44, 53, 99), RGB(197, 197, 206), RGB(150, 210, 131), RGB(174, 223, 135), RGB(195, 230, 138), RGB(218, 237, 142), RGB(226, 233, 137), RGB(232, 217, 119), RGB(238, 200, 102), RGB(231, 183, 89), RGB(196, 167, 89), RGB(174, 158, 89) };
			return new Palette(c, "Arctic", Palette.Kind.Fixed, (Palette.Is)3, (Palette.Is)3, (Palette.Is)3, (Palette.Is)3);
		}

	}
}
