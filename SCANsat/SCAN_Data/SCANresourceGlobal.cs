using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCANsat.SCAN_Data
{
	public class SCANresourceGlobal
	{
		private string name;
		private float transparency;
		private Color minColor;
		private Color maxColor;
		private SCANtype sType;
		private SCANresourceType resourceType;
		private SCANresource_Source source;
		private Dictionary<string, SCANresource> bodyConfigs;

		internal SCANresourceGlobal(string resource, float trans, Color minC, Color maxC, SCANtype t, SCANresourceType rType, SCANresource_Source s)
		{
			name = resource;
			transparency = trans;
			minColor = minC;
			maxColor = maxC;
			sType = t;
			resourceType = rType;
			source = s;
		}

		public static void addToBodyConfigs(SCANresourceGlobal G, string s, SCANresource r)
		{
			if (!G.bodyConfigs.ContainsKey(s))
				G.bodyConfigs.Add(s, r);
			else
				Debug.LogError("[SCANsat] Warning: SCANresource Dictionary Already Contains Key Of This Type");
		}

		public string Name
		{
			get { return name; }
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

		public SCANtype SType
		{
			get { return sType; }
		}

		public SCANresourceType ResourceType
		{
			get { return resourceType; }
		}

		public SCANresource_Source Source
		{
			get { return source; }
		}

		public Dictionary<string, SCANresource> BodyConfigs
		{
			get { return bodyConfigs; }
		}
	}
}
