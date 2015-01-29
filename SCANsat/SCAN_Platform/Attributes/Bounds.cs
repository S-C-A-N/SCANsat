using System;
using System.Runtime.InteropServices;

namespace SCANsat.SCAN_Platform.Attributes
{
	[StructLayout(LayoutKind.Explicit)]
	public struct AValue {
		[FieldOffset(0)]	public bool aBool;
		[FieldOffset(0)]	public byte aByte;
		[FieldOffset(0)]	public char aChar;
		[FieldOffset(0)]	public float aFloat;
		[FieldOffset(0)]	public double aDouble;
		[FieldOffset(0)]	public int aInt;
		[FieldOffset(0)]	public long aLong;
		[FieldOffset(0)]	public short aShort;
	}

	[AttributeUsage((AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue | AttributeTargets.Method),AllowMultiple = true)]
	public class LowerBoundAttribute : Attribute {
		private AValue floor = new AValue();
		public LowerBoundAttribute (bool b) 	{ floor.aBool = b; }
		public LowerBoundAttribute (bool[] b)	{ }
		public LowerBoundAttribute (byte b)		{ floor.aByte = b; }
		public LowerBoundAttribute (byte[] b)	{ }
		public LowerBoundAttribute (char c)		{ floor.aChar = c; }
		public LowerBoundAttribute (char[] c)	{ }
		public LowerBoundAttribute (float f)	{ floor.aFloat = f; }
		public LowerBoundAttribute (float[] f)	{ }
		public LowerBoundAttribute (double d)	{ floor.aDouble = d; }
		public LowerBoundAttribute (double[] d)	{ }
		public LowerBoundAttribute (int i)		{ floor.aInt = i; }
		public LowerBoundAttribute (int[] i)	{ }
		public LowerBoundAttribute (long l)		{ floor.aLong = l; }
		public LowerBoundAttribute (long[] l)	{ }
		public LowerBoundAttribute (short s)	{ floor.aShort = s; }
		public LowerBoundAttribute (short[] s)	{ }
		public LowerBoundAttribute (string s)	{ }
		public LowerBoundAttribute (string[] s)	{ }
		public LowerBoundAttribute (object o)	{ }
		public LowerBoundAttribute (object[] o)	{ }
		public LowerBoundAttribute (Type t)		{ }
		public LowerBoundAttribute (Type[] t)	{ }
	}
	[AttributeUsage((AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue | AttributeTargets.Method),AllowMultiple = true)]
	public class UpperBoundAttribute : Attribute {
		private AValue roof = new AValue();
		public UpperBoundAttribute (bool b) 	{ roof.aBool = b; }
		public UpperBoundAttribute (bool[] b)	{ }
		public UpperBoundAttribute (byte b)		{ roof.aByte = b; }
		public UpperBoundAttribute (byte[] b)	{ }
		public UpperBoundAttribute (char c)		{ roof.aChar = c; }
		public UpperBoundAttribute (char[] c)	{ }
		public UpperBoundAttribute (float f)	{ roof.aFloat = f; }
		public UpperBoundAttribute (float[] f)	{ }
		public UpperBoundAttribute (double d)	{ roof.aDouble = d; }
		public UpperBoundAttribute (double[] d)	{ }
		public UpperBoundAttribute (int i)		{ roof.aInt = i; }
		public UpperBoundAttribute (int[] i)	{ }
		public UpperBoundAttribute (long l)		{ roof.aLong = l; }
		public UpperBoundAttribute (long[] l)	{ }
		public UpperBoundAttribute (short s)	{ roof.aShort = s; }
		public UpperBoundAttribute (short[] s)	{ }
		public UpperBoundAttribute (string s)	{ }
		public UpperBoundAttribute (string[] s)	{ }
		public UpperBoundAttribute (object o)	{ }
		public UpperBoundAttribute (object[] o)	{ }
		public UpperBoundAttribute (Type t)		{ }
		public UpperBoundAttribute (Type[] t)	{ }
	}
	[AttributeUsage((AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue | AttributeTargets.Method),AllowMultiple = true, Inherited = true)]
	public class BoundedAttribute : Attribute {
		internal AValue floor = new AValue();
		internal AValue roof = new AValue();

		//public BoundedAttribute ( ) { }
		public BoundedAttribute (bool l		, bool u) 	{ floor.aBool = l;		roof.aBool = u; }
		public BoundedAttribute (byte l		, byte u)	{ floor.aByte = l;		roof.aByte = u; }
		public BoundedAttribute (char l		, char u)	{ floor.aChar = l;		roof.aChar = u; }
		public BoundedAttribute (float l	, float u)	{ floor.aFloat = l;		roof.aFloat = u; }
		public BoundedAttribute (double l	, double u)	{ floor.aDouble = l;	roof.aDouble = u; }
		public BoundedAttribute (int l		, int u)	{ floor.aInt = l;		roof.aInt = u; }
		public BoundedAttribute (long l		, long u)	{ floor.aLong = l;		roof.aLong = u; }
		public BoundedAttribute (short l	, short u)	{ floor.aShort = l;		roof.aShort = u; }
		public BoundedAttribute (string l	, string u)	{ }
		public BoundedAttribute (object l	, object u)	{ }
		public BoundedAttribute (Type l		, Type u)	{ }
	}
	[AttributeUsage((AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue | AttributeTargets.Method),AllowMultiple = true, Inherited = true)]
	public class NormAttribute : BoundedAttribute {
		public NormAttribute ( float f  ) : base(0.0f,f) { }
		public NormAttribute ( double d ) : base (0.0d,d) { }
	}
}

