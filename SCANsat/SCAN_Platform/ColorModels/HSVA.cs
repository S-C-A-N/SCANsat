using System;
using System.Collections.Generic;
using SCANsat.SCAN_Platform.Attributes;
using UnityEngine;

namespace SCANsat.SCAN_Platform.ColorModels
{
	public struct HSVA {
		[Norm(1.0f)] public float h;
		[Norm(1.0f)] public float s;
		[Norm(1.0f)] public float v;
		[Norm(1.0f)] public float a;


		#region convenience lambdas
		private static Func<float,float,bool>		approxEq = Mathf.Approximately;
		private static Func<float,float,float,float>	minT = (a,b,c)	=> Mathf.Min(Mathf.Min(a,b),c);
		private static Func<float,float,float,float> 	maxT = (a,b,c)	=> Mathf.Max(Mathf.Max(a,b),c);
		//private static Func<float,byte>				 byteRep = x => (byte) (Mathf.Clamp01(x) * 255);
		//private static Func<byte,float>				floatRep = x => (float) x / 255;


		//private static Func<float,float> l2g = x => Mathf.LinearToGammaSpace(x);
		//private static Func<float,float> g2l = x => Mathf.GammaToLinearSpace(x);
		#endregion

		#region constructors: base
		public HSVA (float h, float s, float v, float a) {
			this.h = h;	// hue
			this.s = s;	// saturation
			this.v = v; // value
			this.a = a; // opacity
		}
		#endregion constructors: base
		#region constructors: derived
		public HSVA (float h, float s, float v) : this(h,s,v,1) { }
		public HSVA (Color c) 									: this(0,0,0,1) { fromColor(c); }
		public HSVA (Color32 c)									: this(0,0,0,1) { fromColor32(c); }
		#endregion constructors

		#region conversion: ColorHSV <-> Color
		public HSVA fromColor	(Color color) {
			var min = minT(color.r,color.g,color.b);
			var max = maxT(color.r,color.g,color.b);
			var del = max - min;

			if (!approxEq(max,0))	{ 				s = del / max; 							}
			else 					{ 				s = 0; 		h = 0 ; return this;		} // -> hue is undefined
			if (approxEq(min,max))	{ v = max;		s = 0;		h = 0 ; return this;		} // -> greyscale image


			if      (max == color.r)	h = 	(color.g - color.b) / del;
			else if (max == color.g)	h = 2+	(color.b - color.r) / del;
			else if (max == color.b)	h = 4+	(color.r - color.g) / del;
			else 						throw new IndexOutOfRangeException("max must be r,g, or b");

			h *= 60;
			if (h < 0) h += 360;

			return this;
		}
		public Color	toColor(HSVA c) {
			if (approxEq(s,0))
				return new Color(this.v,this.v,this.v,a); // -> greyscale

			var sixth = h / 60;

			int   i = Mathf.FloorToInt(sixth);		// [i]nteger part
			float r = sixth - i;					// [r]emainder part

			var v = this.v;
			var p = v * (1 - s);
			var q = v * (1 - s * r);
			var t = v * (1 - s * (1 - r));

			var color = new Color(0,0,0, a);

			switch (i) {
			case 0:		color.r = v; color.g = t; color.b = p; break;
			case 1:		color.r = q; color.g = v; color.b = p; break;
			case 2:		color.r = p; color.g = v; color.b = t; break;
			case 3:		color.r = p; color.g = q; color.b = v; break;
			case 4:		color.r = t; color.g = p; color.b = v; break;
			default:	color.r = v; color.g = p; color.b = q; break;
			}
			return color;
		}
		#endregion
		#region conversion: ColorHSV <-> Color32
		public HSVA fromColor32 (Color32 c) { return fromColor((Color) c); }
		public Color32	toColor32 (HSVA c) { return (Color32)toColor(c); }
		#endregion
		#region conversion: ColorHSV :-> String
		public string ToString (string format) {
			return String.Format ("HSVA({0}, {1}, {2}, {3})", new object[] {
				h.ToString (format),
				s.ToString (format),
				v.ToString (format),
				a.ToString (format)
			});
		}
		public override string ToString () {
			return String.Format ("HSVA({0:F3}, {1:F3}, {2:F3}, {3:F3})", new object[] {
				h,
				s,
				v,
				a
			});
		}
		#endregion
		#region conversion: ColorHSV <-> Vector4
		public static implicit operator Vector4 (HSVA c) {
			return new Vector4 (c.h, c.s, c.v, c.a);
		}
		public static implicit operator HSVA (Vector4 v) {
			return new HSVA (v.x, v.y, v.z, v.w);
		}
		#endregion
		#region conversion: ColorHSV <-> Vector3
		public static implicit operator Vector3 (HSVA c) {
			return new Vector3 (c.h, c.s, c.v);
		}
		public static implicit operator HSVA (Vector3 v) {
			return new HSVA (v.x, v.y, v.z, 1);
		}
		#endregion

		#region interpolation
		public static HSVA Lerp (HSVA src, HSVA tar, float t) {
			t = Mathf.Clamp01(t);
			float h,s;
			if			(approxEq(src.v,0))	{ h = tar.h; s = tar.s; }
			else if 	(approxEq(tar.v,0)) { h = src.h; s = src.s; }
			else { if 	(approxEq(tar.s,0)) { h = tar.h; 			}
				else if (approxEq(src.s,0)) { h = src.h; 			}
					else {
						var angle = Mathf.LerpAngle(src.h * 360f, tar.h * 360f, t);
						while (angle < 0f)	angle += 360f;
						while (angle > 360f)angle -= 360f;
						h = angle/360f;
					}	
			s = Mathf.Lerp(src.s,tar.s,t);
			}
			return new HSVA(h,s,Mathf.Lerp(src.v,tar.v,t),Mathf.Lerp(src.a,tar.a,t));

			}
		public static HSVA Grad (HSVA src, HSVA tar, float t) {
			t = Mathf.Clamp01(t);
			var steps = 100;
			var colors = 2;
			var parts = colors - 1;
			var gradient = new List<HSVA>();
			var gradientIndex = 0;
			var partSteps = Mathf.Floor(steps / parts);
			var remainder = steps - (partSteps * parts);

			for (var col = 0; col < parts; col++) {
				var c1 = src;
				var c2 = tar;
				var goCW	= (src.h >= tar.h) ? (    src.h - tar.h) : (1 + src.h - tar.h);
				var goCCW	= (src.h >= tar.h) ? (1 + tar.h - src.h) : (    tar.h - src.h);

				if (col == parts -1) partSteps += remainder;

				for (var step = 0; step < steps; step++) {
					var p = step / steps;
					var h = (goCW <= goCCW) ? src.h + (goCW * p) : src.h - (goCCW * p);

					if (h < 0) h = 1 + h;
					if (h > 1) h = h - 1;

					var s = ((1 - p) * src.s) + (p * tar.s);
					var v = ((1 - p) * src.v) + (p * tar.v);

					gradient[gradientIndex] = new HSVA(h,s,v,1);
					gradientIndex++;
				}

			}

			return new HSVA(); }
		#endregion

		#region indexing
		public float this [int i] {
			get {
				switch (i) {
				case 0: return h;
				case 1: return s;
				case 2: return v;
				case 3: return a;
				default: throw new IndexOutOfRangeException("ColorHSV(): invalid Vector3 index");
				}
			}
			set {
				switch (i) {
				case 0: h = value; break;
				case 1: s = value; break;
				case 2: v = value; break;
				case 3: a = value; break;
				default: throw new IndexOutOfRangeException("ColorHSV(): invalid Vector3 index");
				}
			}
		}
		#endregion indexing

		#region equality testing (exact and hashing)
		public override bool Equals (object obj)
		{
			if (!(obj is HSVA)) { return false; }
			var color = (HSVA)obj;
			return h.Equals (color.h) 
				&& s.Equals (color.s)
				&& v.Equals (color.v)
				&& a.Equals (color.a);
		}

		public override int GetHashCode ()
		{
			return GetHashCode();
		}
		#endregion
		#region operators: equality
		public static bool operator == (HSVA lhs, HSVA rhs) {
			return lhs == rhs;
		}
		public static bool operator != (HSVA lhs, HSVA rhs) {
			return lhs != rhs;
		}
		#endregion
		#region operators: arithmetic
		public static HSVA operator + (HSVA a, HSVA b) {
			return new HSVA (a.h + b.h, a.s + b.s, a.v + b.v, a.a + b.a);
		}
		public static HSVA operator - (HSVA a, HSVA b) {
			return new HSVA (a.h - b.h, a.s - b.s, a.v - b.v, a.a - b.a);
		}
		public static HSVA operator / (HSVA a, float b) {
			return new HSVA (a.h / b, a.s / b, a.v / b, a.a / b);
		}
		public static HSVA operator * (float b, HSVA a) {
			return new HSVA (a.h * b, a.s * b, a.v * b, a.a * b);
		}
		public static HSVA operator * (HSVA a, float b) {
			return new HSVA (a.h * b, a.s * b, a.v * b, a.a * b);
		}
		public static HSVA operator * (HSVA a, HSVA b) {
			return new HSVA (a.h * b.h, a.s * b.s, a.v * b.v, a.a * b.a);
		}
		#endregion
	}
}

