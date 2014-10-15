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

		public Swatches Swatches { get; private set; }

		public Color32[] colors;

		public List<Color> colors4;
		//public List<uint32> _hexCodes = new List<uint32>();

		public enum Kind {
			Diverging,
			Qualitative,
			Sequential,
			Invertable,
			Unknown,
		}

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
			this.kind = k;
			this.blind = blindSafe;
			this.print = printSafe;
			this.xerox = xeroxSafe;
			this.panel = panelSafe;
		}
		public Palette(Color32[] cs, string name, Kind k, Is blindSafe, Is printSafe, Is xeroxSafe, Is panelSafe) {
			this.colors = cs;
			this.name = name;
			this.kind = k;
			this.blind = blindSafe;
			this.print = printSafe;
			this.xerox = xeroxSafe;
			this.panel = panelSafe;
		}
	}

}

