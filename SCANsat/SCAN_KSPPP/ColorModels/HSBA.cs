using System;
using System.Collections.Generic;
using SCANsat.Platform.Attributes;
using UnityEngine;

namespace SCANsat.Platform.ColorModels
{
	public struct HSBA {
		[Norm(255)] public byte h;	// 360
		[Norm(255)] public byte s;	// 100
		[Norm(255)] public byte b;	// 100
		[Norm(255)] public byte a;	// ???

//
//		#region convenience lambdas
//		private static Func<float,float,bool>		approxEq = Mathf.Approximately;
//		private static Func<float,float,float,float>	minT = (a,b,c)	=> Mathf.Min(Mathf.Min(a,b),c);
//		private static Func<float,float,float,float> 	maxT = (a,b,c)	=> Mathf.Max(Mathf.Max(a,b),c);
//		//private static Func<byte,byte>				 byteRep = x => (byte) (Mathf.Clamp01(x) * 255);
//		//private static Func<byte,byte>				byteRep = x => (byte) x / 255;
//
//
//		//private static Func<byte,byte> l2g = x => Mathf.LinearToGammaSpace(x);
//		//private static Func<byte,byte> g2l = x => Mathf.GammaToLinearSpace(x);
//		#endregion
//
//		#region constructors: base
//		public ColorHSB (byte h, byte s, byte v, byte a) {
//			this.h = h;	// hue
//			this.s = s;	// saturation
//			this.b = v; // value
//			this.a = a; // opacity
//		}
//		#endregion constructors: base
//		#region constructors: derived
//		public ColorHSB (byte h, byte s, byte v) : this(h,s,v,1) { }
//		public ColorHSB (Color c) 									: this(0,0,0,1) { fromColor(c); }
//		public ColorHSB (Color32 c)									: this(0,0,0,1) { fromColor32(c); }
//		#endregion constructors
//
//		#region conversion: ColorHSB <-> Color
//		public ColorHSB fromColor	(Color color) {
//			var min = minT(color.r,color.g,color.b);
//			var max = maxT(color.r,color.g,color.b);
//			var del = max - min;
//
//			if (!approxEq(max,0))	{ 				s = del / max; 							}
//			else 					{ 				s = 0; 		h = 0 ; return this;		} // -> hue is undefined
//			if (approxEq(min,max))	{ b = max;		s = 0;		h = 0 ; return this;		} // -> greyscale image
//
//
//			if      (max == color.r)	h = 		(color.g - color.b) / del;
//			else if (max == color.g)	h = 2+	(color.b - color.r) / del;
//			else if (max == color.b)	h = 4+	(color.r - color.g) / del;
//			else 						throw new IndexOutOfRangeException("max must be r,g, or b");
//
//			h *= 60;
//			if (h < 0) h += 360;
//
//			return this;
//		}
//		public Color	toColor(ColorHSB c) {
//			if (approxEq(s,0))
//				return new Color(this.b,this.b,this.b,a); // -> greyscale
//
//			var sixth = h / 60;
//
//			int   i = Mathf.FloorToInt(sixth);		// [i]nteger part
//			byte r = sixth - i;					// [r]emainder part
//
//			var v = this.b;
//			var p = v * (1 - s);
//			var q = v * (1 - s * r);
//			var t = v * (1 - s * (1 - r));
//
//			var color = new Color(0,0,0, a);
//
//			switch (i) {
//			case 0:		color.r = v; color.g = t; color.b = p; break;
//			case 1:		color.r = q; color.g = v; color.b = p; break;
//			case 2:		color.r = p; color.g = v; color.b = t; break;
//			case 3:		color.r = p; color.g = q; color.b = v; break;
//			case 4:		color.r = t; color.g = p; color.b = v; break;
//			default:	color.r = v; color.g = p; color.b = q; break;
//			}
//			return color;
//		}
//		#endregion
//		#region conversion: ColorHSB <-> Color32
//		public ColorHSB fromColor32 (Color32 c) { return fromColor((Color) c); }
//		public Color32	toColor32 (ColorHSB c) { return (Color32)toColor(c); }
//		#endregion
//		#region conversion: ColorHSB :-> String
//		public string ToString (string format) {
//			return String.Format ("HSVA({0}, {1}, {2}, {3})", new object[] {
//				h.ToString (format),
//				s.ToString (format),
//				b.ToString (format),
//				a.ToString (format)
//			});
//		}
//		public override string ToString () {
//			return String.Format ("HSVA({0:F3}, {1:F3}, {2:F3}, {3:F3})", new object[] {
//				h,
//				s,
//				b,
//				a
//			});
//		}
//		#endregion
//		#region conversion: ColorHSB <-> Vector4
//		public static implicit operator Vector4 (ColorHSB c) {
//			return new Vector4 (c.h, c.s, c.v, c.a);
//		}
//		public static implicit operator ColorHSB (Vector4 v) {
//			return new ColorHSB (v.x, v.y, v.z, v.w);
//		}
//		#endregion
//		#region conversion: ColorHSB <-> Vector3
//		public static implicit operator Vector3 (ColorHSB c) {
//			return new Vector3 (c.h, c.s, c.v);
//		}
//		public static implicit operator ColorHSB (Vector3 v) {
//			return new ColorHSB (v.x, v.y, v.z, 1);
//		}
//		#endregion
//
//		#region interpolation
//		public static ColorHSB Lerp (ColorHSB src, ColorHSB tar, byte t) {
//			t = Mathf.Clamp01(t);
//			byte h,s;
//			if			(approxEq(src.v,0))	{ h = tar.h; s = tar.s; }
//			else if 	(approxEq(tar.v,0)) { h = src.h; s = src.s; }
//			else { if 	(approxEq(tar.s,0)) { h = tar.h; 			}
//				else if (approxEq(src.s,0)) { h = src.h; 			}
//					else {
//						var angle = Mathf.LerpAngle(src.h * 360f, tar.h * 360f, t);
//						while (angle < 0f)	angle += 360f;
//						while (angle > 360f)angle -= 360f;
//						h = angle/360f;
//					}	
//			s = Mathf.Lerp(src.s,tar.s,t);
//			}
//			return new ColorHSB(h,s,Mathf.Lerp(src.v,tar.v,t),Mathf.Lerp(src.a,tar.a,t));
//
//			}
//		public static ColorHSB Grad (ColorHSB src, ColorHSB tar, byte t) {
//			t = Mathf.Clamp01(t);
//			var steps = 100;
//			var colors = 2;
//			var parts = colors - 1;
//			var gradient = new List<ColorHSB>();
//			var gradientIndex = 0;
//			var partSteps = Mathf.Floor(steps / parts);
//			var remainder = steps - (partSteps * parts);
//
//			for (var col = 0; col < parts; col++) {
//				var c1 = src;
//				var c2 = tar;
//				var goCW	= (src.h >= tar.h) ? (    src.h - tar.h) : (1 + src.h - tar.h);
//				var goCCW	= (src.h >= tar.h) ? (1 + tar.h - src.h) : (    tar.h - src.h);
//
//				if (col == parts -1) partSteps += remainder;
//
//				for (var step = 0; step < steps; step++) {
//					var p = step / steps;
//					var h = (goCW <= goCCW) ? src.h + (goCW * p) : src.h - (goCCW * p);
//
//					if (h < 0) h = 1 + h;
//					if (h > 1) h = h - 1;
//
//					var s = ((1 - p) * src.s) + (p * tar.s);
//					var v = ((1 - p) * src.v) + (p * tar.v);
//
//					gradient[gradientIndex] = new ColorHSB(h,s,v,1);
//					gradientIndex++;
//				}
//
//			}
//
//			return new ColorHSB(); }
//		#endregion
//
//		#region indexing
//		public byte this [int i] {
//			get {
//				switch (i) {
//				case 0: return h;
//				case 1: return s;
//				case 2: return b;
//				case 3: return a;
//				default: throw new IndexOutOfRangeException("ColorHSB(): invalid Vector3 index");
//				}
//			}
//			set {
//				switch (i) {
//				case 0: h = value; break;
//				case 1: s = value; break;
//				case 2: b = value; break;
//				case 3: a = value; break;
//				default: throw new IndexOutOfRangeException("ColorHSB(): invalid Vector3 index");
//				}
//			}
//		}
//		#endregion indexing
//
//		#region equality testing (exact and hashing)
//		public override bool Equals (object obj)
//		{
//			if (!(obj is ColorHSB)) { return false; }
//			var color = (ColorHSB)obj;
//			return h.Equals (color.h) 
//				&& s.Equals (color.s)
//				&& b.Equals (color.v)
//				&& a.Equals (color.a);
//		}
//
//		public override int GetHashCode ()
//		{
//			return GetHashCode();
//		}
//		#endregion
//		#region operators: equality
//		public static bool operator == (ColorHSB lhs, ColorHSB rhs) {
//			return lhs == rhs;
//		}
//		public static bool operator != (ColorHSB lhs, ColorHSB rhs) {
//			return lhs != rhs;
//		}
//		#endregion
//		#region operators: arithmetic
//		public static ColorHSB operator + (ColorHSB a, ColorHSB b) {
//			return new ColorHSB (a.h + b.h, a.s + b.s, a.v + b.v, a.a + b.a);
//		}
//		public static ColorHSB operator - (ColorHSB a, ColorHSB b) {
//			return new ColorHSB (a.h - b.h, a.s - b.s, a.v - b.v, a.a - b.a);
//		}
//		public static ColorHSB operator / (ColorHSB a, byte b) {
//			return new ColorHSB (a.h / b, a.s / b, a.v / b, a.a / b);
//		}
//		public static ColorHSB operator * (byte b, ColorHSB a) {
//			return new ColorHSB (a.h * b, a.s * b, a.v * b, a.a * b);
//		}
//		public static ColorHSB operator * (ColorHSB a, byte b) {
//			return new ColorHSB (a.h * b, a.s * b, a.v * b, a.a * b);
//		}
//		public static ColorHSB operator * (ColorHSB a, ColorHSB b) {
//			return new ColorHSB (a.h * b.h, a.s * b.s, a.v * b.v, a.a * b.a);
//		}
//		#endregion
	}
}

