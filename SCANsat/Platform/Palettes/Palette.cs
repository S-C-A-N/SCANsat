using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SCANsat.Platform.Palettes
{
	[Serializable]
	public class Palette
	{
		public string name;
		public Kind kind;
		public int size;
		public Texture2D swatch;

		public Swatches Swatches { get; private set; }

		public Color32[] colors;
		public Color32[] colorsReverse;

		public List<Color> colors4;
		//public List<uint32> _hexCodes = new List<uint32>();

		public enum Kind {
			Diverging,
			Qualitative,
			Sequential,
			Invertable,
			Unknown,
		}

		private static string[] getKindNames()
		{
			Kind[] v = (Kind[])Enum.GetValues(typeof(Kind));
			string[] r = new string[v.Length - 2];
			for (int i = 0; i < v.Length - 2; ++i)
				r[i] = v[i].ToString();
			return r;
		}

		public static string[] kindNames = getKindNames();



		public enum Is : ushort { Unsafe = 0, Safe = 1, Unsure = 2, Unknown = 3 }

		public Is blind;
		public Is print;
		public Is xerox;
		public Is panel;

		//public List<Color> colors;
		//public List<uint32> _hexCodes = new List<uint32>();

		public Palette () {
			this.kind 	= Kind.Unknown;
			this.blind	= Is.Unknown;
			this.print	= Is.Unknown;
			this.xerox	= Is.Unknown;
			this.panel	= Is.Unknown;
		}

		public Palette(string name) : this(name,null) {}
		public Palette(string name, IEnumerable<Swatch> cs) {
			this.name = name;
			this.Swatches = new Swatches();

			if (colors != null)	this.Swatches.AddRange(cs);
		}
		public Palette(Color32[] cs, Kind k, Is blindSafe, Is printSafe, Is xeroxSafe, Is panelSafe) {
			this.colors = cs;
			this.colorsReverse = cs;
			Array.Reverse(this.colorsReverse);
			this.kind = k;
			this.blind = blindSafe;
			this.print = printSafe;
			this.xerox = xeroxSafe;
			this.panel = panelSafe;
			this.size = cs.Length;
		}
		public Palette(Color32[] cs, string name, Kind k, Is blindSafe, Is printSafe, Is xeroxSafe, Is panelSafe) {
			this.colors = cs;
			this.colorsReverse = cs;
			Array.Reverse(this.colorsReverse);
			this.name = name;
			this.kind = k;
			this.blind = blindSafe;
			this.print = printSafe;
			this.xerox = xeroxSafe;
			this.panel = panelSafe;
			this.size = cs.Length;
		}
	}

}

