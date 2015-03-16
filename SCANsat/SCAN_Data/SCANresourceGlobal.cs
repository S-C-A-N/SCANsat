using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using SCANsat.SCAN_Platform;

namespace SCANsat.SCAN_Data
{
	public class SCANresourceGlobal : SCAN_ConfigNodeStorage
	{
		[Persistent]
		private string name;
		[Persistent]
		private Color lowResourceColor;
		[Persistent]
		private Color highResourceColor;
		[Persistent]
		private float resourceTransparency;
		[Persistent]
		private List<SCANresourceBody> Resource_Planetary_Config = new List<SCANresourceBody>();

		private SCANtype sType;
		private SCANresourceType resourceType;
		private SCANresource_Source source;
		private Dictionary<string, SCANresourceBody> masterBodyConfigs;
		private SCANresourceBody currentBody;

		internal SCANresourceGlobal(string resource, float trans, Color minC, Color maxC, SCANresourceType t, int S)
		{
			name = resource;
			resourceTransparency = trans;
			lowResourceColor = minC;
			highResourceColor = maxC;
			resourceType = t;
			sType = resourceType.Type;
			source = (SCANresource_Source)S;
		}

		internal SCANresourceGlobal(SCANresourceGlobal copy)
		{
			name = copy.name;
			resourceTransparency = copy.resourceTransparency;
			lowResourceColor = copy.lowResourceColor;
			highResourceColor = copy.highResourceColor;
			sType = copy.sType;
			resourceType = copy.resourceType;
			source = copy.source;
			masterBodyConfigs = copy.masterBodyConfigs;
		}

		public override void OnDecodeFromConfigNode()
		{
			try
			{
				masterBodyConfigs = Resource_Planetary_Config.ToDictionary(a => a.Name, a => a);
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error while loading SCANsat body resource config settings: {0}", e);
			}
		}

		public override void OnEncodeToConfigNode()
		{
			try
			{
				Resource_Planetary_Config = masterBodyConfigs.Values.ToList();
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error while saving SCANsat altimetry config data: {0}", e);
			}
		}

		public static void addToBodyConfigs(SCANresourceGlobal G, string s, SCANresourceBody r)
		{
			if (!G.masterBodyConfigs.ContainsKey(s))
				G.masterBodyConfigs.Add(s, r);
			else
				Debug.LogError("[SCANsat] Warning: SCANresource Dictionary Already Contains Key Of This Type");
		}

		public string Name
		{
			get { return name; }
		}

		public float Transparency
		{
			get { return resourceTransparency; }
			internal set
			{
				if (value < 0)
					resourceTransparency = 0;
				else if (value > 100)
					resourceTransparency = 100;
				else
					resourceTransparency = value;
			}
		}

		public Color MinColor
		{
			get { return lowResourceColor; }
			internal set { lowResourceColor = value; }
		}

		public Color MaxColor
		{
			get { return highResourceColor; }
			internal set { highResourceColor = value; }
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
			get { return masterBodyConfigs; }
		}

		public void CurrentBodyConfig(string body)
		{
			if (masterBodyConfigs.ContainsKey(body))
				currentBody = masterBodyConfigs[body];
			else
				currentBody = masterBodyConfigs.ElementAt(0).Value;
		}

		public SCANresourceBody CurrentBody
		{
			get { return currentBody; }
		}
	}
}
