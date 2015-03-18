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
		private string resourceName;
		[Persistent]
		private string bodyName;
		[Persistent]
		private int index;
		[Persistent]
		private float lowResourceCutoff;
		[Persistent]
		private float highResourceCutoff;

		private CelestialBody body;
		private float defaultMinValue, defaultMaxValue;

		internal SCANresourceBody(string rName, CelestialBody Body, float min, float max)
		{
			resourceName = rName;
			body = Body;
			bodyName = body.name;
			index = body.flightGlobalsIndex;
			lowResourceCutoff = defaultMinValue = min;
			highResourceCutoff = defaultMaxValue = max;
		}

		internal SCANresourceBody(SCANresourceBody copy)
		{
			resourceName = copy.resourceName;
			bodyName = copy.bodyName;
			index = copy.index;
			lowResourceCutoff = copy.lowResourceCutoff;
			highResourceCutoff = copy.highResourceCutoff;
			body = copy.body;
			defaultMinValue = copy.defaultMinValue;
			defaultMaxValue = copy.defaultMaxValue;
		}

		public override void OnDecodeFromConfigNode()
		{
			body = FlightGlobals.Bodies.FirstOrDefault(b => b.flightGlobalsIndex == index);
		}

		public string BodyName
		{
			get { return bodyName; }
		}

		public string ResourceName
		{
			get { return resourceName; }
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
