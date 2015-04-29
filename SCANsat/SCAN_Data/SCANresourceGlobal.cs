#region license
/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANresourceGlobal - Serializable object for storing information about a resource
 * 
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

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
		private float resourceTransparency = 20;
		[Persistent]
		private float defaultMinValue = 0.001f;
		[Persistent]
		private float defaultMaxValue = 10f;
		[Persistent]
		private List<SCANresourceBody> Resource_Planetary_Config = new List<SCANresourceBody>();

		private Dictionary<string, SCANresourceBody> masterBodyConfigs = new Dictionary<string,SCANresourceBody>();

		private SCANtype sType;
		private SCANresourceType resourceType;

		private Color defaultLowColor;
		private Color defaultHighColor;
		private float defaultTrans;
		private Texture2D mapOverlay;

		private SCANresourceBody currentBody;

		internal SCANresourceGlobal(string resource, float trans, float defMin, float defMax, Color minC, Color maxC, SCANresourceType t)
		{
			name = resource;
			resourceTransparency = trans;
			lowResourceColor = minC;
			highResourceColor = maxC;
			defaultMinValue = defMin;
			defaultMaxValue = defMax;
			resourceType = t;
			sType = resourceType.Type;

			setDefaultValues();
		}

		public SCANresourceGlobal()
		{
		}

		internal SCANresourceGlobal(SCANresourceGlobal copy)
		{
			name = copy.name;
			resourceTransparency = copy.resourceTransparency;
			lowResourceColor = copy.lowResourceColor;
			highResourceColor = copy.highResourceColor;
			sType = copy.sType;
			resourceType = copy.resourceType;
			masterBodyConfigs = copyBodyConfigs(copy);
			defaultLowColor = copy.defaultLowColor;
			defaultHighColor = copy.defaultHighColor;
			defaultTrans = copy.defaultTrans;
			defaultMinValue = copy.defaultMinValue;
			defaultMaxValue = copy.defaultMaxValue;
		}

		private Dictionary<string, SCANresourceBody> copyBodyConfigs(SCANresourceGlobal c)
		{
			Dictionary<string, SCANresourceBody> newCopy = new Dictionary<string, SCANresourceBody>();
			foreach (SCANresourceBody r in c.masterBodyConfigs.Values)
			{
				SCANresourceBody newR = new SCANresourceBody(r);
				if (!newCopy.ContainsKey(newR.BodyName))
					newCopy.Add(newR.BodyName, newR);
			}

			return newCopy;
		}

		public override void OnDecodeFromConfigNode()
		{
			resourceType = SCANcontroller.getResourceType(name);
			sType = resourceType.Type;

			setDefaultValues();
			try
			{
				masterBodyConfigs = Resource_Planetary_Config.ToDictionary(a => a.BodyName, a => a);
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error while loading SCANsat body resource config settings: {0}", e);
			}
		}

		private void setDefaultValues()
		{
			defaultLowColor = lowResourceColor;
			defaultHighColor = highResourceColor;
			defaultTrans = resourceTransparency;
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

		public void addToBodyConfigs(string s, SCANresourceBody r, bool warn)
		{
			if (!masterBodyConfigs.ContainsKey(s))
				masterBodyConfigs.Add(s, r);
			else if (warn)
				Debug.LogError(string.Format("[SCANsat] Warning: SCANresource Dictionary Already Contains Key Of This Type: [{0}] For Body: [{1}]", r.ResourceName, s));
		}

		public void updateBodyConfig(SCANresourceBody b)
		{
			SCANresourceBody update = getBodyConfig(b.BodyName);
			if (update != null)
			{
				update.MinValue = b.MinValue;
				update.MaxValue = b.MaxValue;
			}
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
				else if (value > 80)
					resourceTransparency = 80;
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

		public float DefaultMinValue
		{
			get { return defaultMinValue; }
			internal set
			{
				if (value >= 0 && value < defaultMaxValue && value <= 100)
					defaultMinValue = value;
			}
		}

		public float DefaultMaxValue
		{
			get { return defaultMaxValue; }
			internal set
			{
				if (value >= 0 && value > defaultMinValue && value <= 100)
					defaultMaxValue = value;
			}
		}

		public SCANtype SType
		{
			get { return sType; }
		}

		public SCANresourceType ResourceType
		{
			get { return resourceType; }
		}

		public int getBodyCount
		{
			get { return masterBodyConfigs.Count; }
		}

		public SCANresourceBody getBodyConfig (string body, bool warn = true)
		{
			if (masterBodyConfigs.ContainsKey(body))
				return masterBodyConfigs[body];
			else if (warn)
				SCANUtil.SCANlog("SCANsat resource celestial body config: [{0}] is empty; something probably went wrong here", body);

			return null;
		}

		public SCANresourceBody getBodyConfig (int i)
		{
			if (masterBodyConfigs.Count >= i)
				return masterBodyConfigs.ElementAt(i).Value;
			else
				SCANUtil.SCANlog("SCANsat resource celestial body config is empty; something probably went wrong here");

			return null;
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

		public Color DefaultLowColor
		{
			get { return defaultLowColor; }
		}

		public Color DefaultHighColor
		{
			get { return defaultHighColor; }
		}

		public float DefaultTrans
		{
			get { return defaultTrans; }
		}

		public Texture2D MapOverlay
		{
			get { return mapOverlay; }
			set { mapOverlay = value; }
		}
	}
}
