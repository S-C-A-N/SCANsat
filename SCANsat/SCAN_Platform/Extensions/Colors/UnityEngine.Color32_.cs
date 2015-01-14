using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SG = System.Globalization;


namespace UnityEngine
{
	public static class Color32_
	{
		private const double SOME_THING_R = 0.2126;
		private const double SOME_THING_G = 0.7175;
		private const double SOME_THING_B = 0.0722;

		private const SG.NumberStyles  HEX_STYLE   = SG.NumberStyles.HexNumber;

		public static Color32 Random (this Color color, byte minClamp = 127)
		{
			var r		= (byte) UnityEngine.Random.Range(minClamp, byte.MaxValue);
			var g		= (byte) UnityEngine.Random.Range(minClamp, byte.MaxValue);
			var b		= (byte) UnityEngine.Random.Range(minClamp, byte.MaxValue);
			return new Color32 (r, g, b, byte.MaxValue);
		}


		public static string ToHex(this Color32 c) {
			return	c.r.ToString("X2")
				+	c.g.ToString("X2")
				+	c.b.ToString("X2");
		}

		public static Color32 FromHex(this Color32 c, string s) {
			byte r = byte.Parse ( s.Substring (0,2) , HEX_STYLE);
			byte g = byte.Parse ( s.Substring (2,2) , HEX_STYLE);
			byte b = byte.Parse ( s.Substring (4,2) , HEX_STYLE);
			return new Color32(r,g,b,255);
		}

		public static double Luminance(this Color32 c) {
			return SOME_THING_R * (c.r / 255d)
				+  SOME_THING_G * (c.g / 255d)
				+  SOME_THING_B * (c.b / 255d);
		}
	}
}

