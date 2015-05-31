//using System;
//using System.Collections.Generic;
//using System.Collections;
//using System.Collections.ObjectModel;
//using UnityEngine;
//using System.Diagnostics;
//using System.Runtime.InteropServices;

//namespace SCANsat.SCAN_Platform.Palettes
//{
//	public class Swatches : ICollection<Swatch>
//	{
//		private List<Swatch> _swatches = new List<Swatch>();

//		public delegate void Has(object sender, Swatch color);

//		public event EventHandler Cleared;
//		public event Has Grown;
//		public event Has Shrunk;

//		public void AddRange(IEnumerable<Swatch> colors)
//		{
//			_swatches.AddRange(colors);
//			foreach (Swatch c in colors) { if (this.Grown != null) this.Grown(this, c); }
//		}
//		public void Add(Swatch color) {
//			_swatches.Add(color);
//			if (this.Grown != null)	this.Grown(this, color);
//		}
//		public bool Remove(Swatch color) {
//			if (this.Shrunk != null) this.Shrunk(this, color);
//			return _swatches.Remove(color);
//		}
//		public void Clear() {
//			_swatches.Clear();
//			if (this.Cleared != null) this.Cleared(this, new EventArgs());
//		}

//		public bool Contains (Swatch color) {
//			return _swatches.Contains(color);
//		}

//		public void CopyTo(Swatch[] colors, int index) {
//			_swatches.CopyTo(colors,index);
//		}

//		public int Count {
//			get { return _swatches.Count; }
//		}

//		public bool IsReadOnly {
//			get { return ((ICollection<Swatch>) _swatches).IsReadOnly; }
//		}

//		public ReadOnlyCollection<Swatch> AsReadOnly() {
//			return _swatches.AsReadOnly();
//		}

//		public IEnumerator<Swatch> GetEnumerator() {
//			return _swatches.GetEnumerator();
//		}

//		IEnumerator IEnumerable.GetEnumerator() {
//			return _swatches.GetEnumerator();
//		}
//	}

//	public class ChangedEventArgs<T> : EventArgs {
//		public T OldV { get; private set; }
//		public T NewV { get; private set; }

//		public ChangedEventArgs(T oldV, T newV) {
//			this.OldV = oldV;
//			this.NewV = newV;
//		}
//	}



//	public class Swatch { 
//		public string Name { get; set; }
//		public Color Color { get; set; }
//		//public _color _col;

//		//[StructLayout(LayoutKind.Explicit)]
//		//public struct _color {
//		//	[FieldOffset(0)] public Color				AsRGBAf;
//		//	[FieldOffset(0)] public Color32			AsRGBAb;
//		//	[FieldOffset(0)] public ColorModels.HSVA	AsHSVA;
//		//	[FieldOffset(0)] public ColorModels.HSBA	AsHSBA;
//		//	[FieldOffset(0)] public ColorModels.HSLA  AsHSLA;
//		//	[FieldOffset(0)] public ColorModels.CMYK	AsCMYK;
//		//}
//		internal Texture2D tex = new Texture2D(1,1);

//		public Swatch(string name) : this(name, new Color()) {}

//		public Swatch(string name, Color c) {
//			this.Name = name;
//			this.Color = c;
//		}
//	}
//}

