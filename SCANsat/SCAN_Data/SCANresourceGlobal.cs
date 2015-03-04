using System;
using System.Linq;
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
		private Dictionary<string, SCANresourceBody> bodyConfigs;
		private SCANresourceBody currentBody;
		private SCANconfig node;

		internal SCANresourceGlobal(string resource, float trans, Color minC, Color maxC, SCANresourceType t, int S)
		{
			name = resource;
			transparency = trans;
			minColor = minC;
			maxColor = maxC;
			resourceType = t;
			sType = resourceType.Type;
			source = (SCANresource_Source)S;
		}

		internal SCANresourceGlobal(SCANresourceGlobal copy)
		{
			name = copy.name;
			transparency = copy.transparency;
			minColor = copy.minColor;
			maxColor = copy.maxColor;
			sType = copy.sType;
			resourceType = copy.resourceType;
			source = copy.source;
			bodyConfigs = copy.bodyConfigs;
		}

		internal void setNode(SCANconfig n)
		{
			node = n;
		}

		internal void setNode()
		{
			node.SCANTopNode.SetValue("lowResourceColor", minColor.ToHex());
			node.SCANTopNode.SetValue("highResourceColor", maxColor.ToHex());
			node.SCANTopNode.SetValue("resourceTransparency", transparency.ToString("F0"));
		}

		public static void addToBodyConfigs(SCANresourceGlobal G, string s, SCANresourceBody r)
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
			internal set
			{
				if (value < 0)
					transparency = 0;
				else if (value > 100)
					transparency = 100;
				else
					transparency = value;
			}
		}

		public Color MinColor
		{
			get { return minColor; }
			internal set { minColor = value; }
		}

		public Color MaxColor
		{
			get { return maxColor; }
			internal set { maxColor = value; }
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

		public Dictionary<string, SCANresourceBody> BodyConfigs
		{
			get { return bodyConfigs; }
		}

		public void CurrentBodyConfig(string body)
		{
			if (bodyConfigs.ContainsKey(body))
				currentBody = bodyConfigs[body];
			else
				currentBody = bodyConfigs.ElementAt(0).Value;
		}

		public SCANresourceBody CurrentBody
		{
			get { return currentBody; }
		}
	}
}
