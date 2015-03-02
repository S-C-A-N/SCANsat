using System;
using UnityEngine;

namespace SCANsat.SCAN_Data
{
	public class SCANresourceConfig
	{
		private string name;
		private CelestialBody body;
		private float minValue;
		private float maxValue;
		private float transparency;
		private Color minColor;
		private Color maxColor;

		internal SCANresourceConfig(string resource, CelestialBody b, float min, float max, float trans, Color minC, Color maxC)
		{
			name = resource;
			body = b;
			minValue = min;
			maxValue = max;
			transparency = trans;
			minColor = minC;
			maxColor = maxC;
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
		}

		public float MaxValue
		{
			get { return maxValue; }
		}

		public float Transparency
		{
			get { return transparency; }
		}

		public Color MinColor
		{
			get { return minColor; }
		}

		public Color MaxColor
		{
			get { return maxColor; }
		}
	}
}
