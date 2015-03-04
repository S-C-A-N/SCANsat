using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCANsat.SCAN_Data
{
	public class SCANresourceBody
	{
		internal SCANresourceBody(string n, CelestialBody Body, float min, float max)
		{
			name = n;
			body = Body;
			minValue = defaultMinValue = min;
			maxValue = defaultMaxValue = max;
		}

		private string name;
		private CelestialBody body;
		private float minValue, maxValue, defaultMinValue, defaultMaxValue;
		private ConfigNode node;

		internal void setNode(ConfigNode n)
		{
			node = n;
		}

		internal void updateNode()
		{
			node.SetValue("lowResourceCutoff", minValue.ToString("F1"));
			node.SetValue("highResourceCutoff", maxValue.ToString("F1"));
		}

		public static SCANresourceBody resourceCopy(SCANresourceBody r)
		{
			SCANresourceBody res = new SCANresourceBody(r.name, r.body, r.minValue, r.maxValue);
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
			get { return minValue; }
			internal set
			{
				if (value >= 0 && value < maxValue && value < 100)
					minValue = value;
			}
		}

		public float MaxValue
		{
			get { return maxValue; }
			internal set
			{
				if (value >= 0 && value > minValue && value <= 100)
					maxValue = value;
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
