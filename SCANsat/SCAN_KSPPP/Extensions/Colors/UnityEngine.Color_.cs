using System;
using System.Reflection;
using System.Collections.Generic;


using SG = System.Globalization;
using Log = SCANsat.Platform.Logging.ConsoleLogger;

namespace UnityEngine
{
	public static class Color_ {

		private static Func<float,float,float,float>	minT = (a,b,c)	=> Mathf.Min(Mathf.Min(a,b),c);
		private static Func<float,float,float,float> 	maxT = (a,b,c)	=> Mathf.Max(Mathf.Max(a,b),c);
		private static Func<float,float,bool>		approxEq = Mathf.Approximately;

		public static Dictionary<Color,string> knownColors;

		private const float SOME_THING_R = 0.2126f;
		private const float SOME_THING_G = 0.7175f;
		private const float SOME_THING_B = 0.0722f;
		private const SG.NumberStyles  HEX_STYLE   = SG.NumberStyles.HexNumber;

		public static float Luminance(this Color c) {
			return SOME_THING_R * (c.r)
				+  SOME_THING_G * (c.g)
				+  SOME_THING_B * (c.b);
		}

		public static Color Random (this Color color, float minClamp = 0.5f)
		{
			var randCol = UnityEngine.Random.onUnitSphere * 3;
			randCol.x		= Mathf.Clamp(randCol.x, minClamp, 1f);
			randCol.y		= Mathf.Clamp(randCol.y, minClamp, 1f);
			randCol.z		= Mathf.Clamp(randCol.z, minClamp, 1f);
			return new Color(randCol.x, randCol.y, randCol.z, 1f);
		}

		public static float Brightness(this Color c) {
			float maxv = maxT(c.r,c.g,c.b);
			float minv = minT(c.r,c.g,c.b);
			return (maxv + minv);
		}

		public static float Saturation(this Color c) {
			float maxv = maxT(c.r,c.g,c.b);
			float minv = minT(c.r,c.g,c.b);
			float sum = maxv + minv;

			if (approxEq(minv,maxv))	return 0.0f;
			if (sum > 1f)				sum = 2f - sum;

			return (maxv - minv) / sum;
		}


		public static string ToHex(this Color c) {
			return	c.r.ToString("X2")
				+	c.g.ToString("X2")
				+	c.b.ToString("X2");
		}

		public static Color FromHex(this Color c, string s) {
			byte r = byte.Parse ( s.Substring (0,2) , HEX_STYLE);
			byte g = byte.Parse ( s.Substring (2,2) , HEX_STYLE);
			byte b = byte.Parse ( s.Substring (4,2) , HEX_STYLE);
			return new Color(r/255f,g/255f,b/255f,1);

		}

		public static void initColorTable() {
			
			switch (knownColors == null) {
				case true: knownColors = new Dictionary<Color, string>(); break;
				case false: return;
			}
			
			// find all of the Colors that UnityEngine.Color exports.
			foreach (var prop in typeof(Color).GetProperties (BindingFlags.Public | BindingFlags.Static)) {
				if (prop.DeclaringType.IsAbstract)						continue;
				if (prop.GetGetMethod ().ReturnType != typeof(Color))	continue;
				
				var name = "Color." + prop.Name;
				Color col = (Color) prop.GetValue(null,null);
				
				if (knownColors.ContainsKey (col)) {
					var collision = "";
					if (knownColors.TryGetValue (col, out collision))
						Log.Now("{0} -> {1} and {2}", col, collision, name);
				} else {
					knownColors.Add (col,name);
					Log.Now("{0} -> {1}", col, name);
				}
			}
			
			// find all of the Colors that XKCDColors exports.
			foreach(var prop in typeof(XKCDColors).GetProperties()) {
				if (prop.GetGetMethod ().ReturnType != typeof(Color)) continue;
				
				var name = "XKCDColor." + prop.Name;
				Color col = (Color) prop.GetValue(null,null);
				
				if (knownColors.ContainsKey (col)) {
					var collision = "";
					if (knownColors.TryGetValue (col, out collision))
						Log.Now("{0} -> {1} and {2}", col, collision, name);
				} else {
					knownColors.Add (col,name);
					Log.Now("{0} -> {1}", col, name);
				}
			}
			
		}
	}
}

