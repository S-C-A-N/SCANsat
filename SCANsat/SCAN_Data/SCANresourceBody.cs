using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SCANsat.SCAN_Platform;

namespace SCANsat.SCAN_Data
{
	public class SCANresourceBody : SCAN_ConfigNodeStorage
	{
		[Persistent]
		private string name;
		[Persistent]
		private int index;
		[Persistent]
		private float lowResourceCutoff;
		[Persistent]
		private float highResourceCutoff;
		private CelestialBody body;
		private float defaultMinValue, defaultMaxValue;

		internal SCANresourceBody(string n, CelestialBody Body, float min, float max)
		{
			name = n;
			body = Body;
			index = body.flightGlobalsIndex;
			lowResourceCutoff = defaultMinValue = min;
			highResourceCutoff = defaultMaxValue = max;
		}

		public static SCANresourceBody resourceCopy(SCANresourceBody r)
		{
			SCANresourceBody res = new SCANresourceBody(r.name, r.body, r.lowResourceCutoff, r.highResourceCutoff);
			return res;
		}

		public string Name
		{
			get { return name; }
		}

		public CelestialBody Body
		{
			get { return body; }
		}

		public float MinValue
		{
			get { return lowResourceCutoff; }
			internal set
			{
				if (value >= 0 && value < highResourceCutoff && value < 100)
					lowResourceCutoff = value;
			}
		}

		public float MaxValue
		{
			get { return highResourceCutoff; }
			internal set
			{
				if (value >= 0 && value > lowResourceCutoff && value <= 100)
					highResourceCutoff = value;
			}
		}

		public float DefaultMinValue
		{
			get { return defaultMinValue; }
		}

		public float DefaultMaxValue
		{
			get { return defaultMaxValue; }
		}
	}
}
