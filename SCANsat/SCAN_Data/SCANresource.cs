#region license
/*
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 * 
 * SCANresource - Stores info on resources pulled from their respective addons and SCANsat configs
 *
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
*/
#endregion

using System;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;
using UnityEngine;

namespace SCANsat.SCAN_Data
{
	public enum SCANresource_Source
	{
		Kethane = 1,
		Regolith = 2,
	}

	public class SCANresource
	{
		internal SCANresource(string n, string Body, Color full, Color empty, float min, float max, SCANresourceType t, SCANresource_Source s)
		{
			name = n;
			body = Body;
			fullColor = full;
			emptyColor = empty;
			minValue = defaultMinValue = min;
			maxValue = defaultMaxValue = max;
			resourceType = t;
			type = resourceType.Type;
			source = s;
		}

		private string name;
		private string body;
		private Color fullColor, emptyColor;
		private float minValue, maxValue, defaultMinValue, defaultMaxValue;
		private float transparency = 0.4f;
		private SCANtype type;
		private SCANresourceType resourceType;
		private SCANresource_Source source;

		public string Name
		{
			get { return name; }
		}

		public string Body
		{
			get { return body; }
		}

		public Color FullColor
		{
			get { return fullColor; }
			internal set { fullColor = value; }
		}

		public Color EmptyColor
		{
			get { return emptyColor; }
			internal set { emptyColor = value; }
		}

		public SCANtype Type
		{
			get { return type; }
		}

		public SCANresourceType ResourceType
		{
			get { return resourceType; }
		}

		public float MinValue
		{
			get { return minValue; }
			internal set
			{
				if (value >= 0 && value < maxValue)
					minValue = value;
			}
		}
		
		public float MaxValue
		{
			get { return maxValue; }
			internal set
			{
				if (value >= 0 && value > minValue)
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

		public float Transparency
		{
			get { return transparency; }
			internal set
			{
				if (value >= 0 && value <= 100)
					transparency = value / 100;
			}
		}

		public SCANresource_Source Source
		{
			get { return source; }
		}
	}

	public class SCANresourceType
	{
		private string name;
		private SCANtype type;
		private Color colorFull, colorEmpty;

		internal SCANresourceType(string s, int i, string Full, string Empty)
		{
			name = s;
			type = (SCANtype)i;
			if ((type & SCANtype.Everything_SCAN) != SCANtype.Nothing)
			{
				Debug.LogWarning("[SCANsat] Attempt To Override Default SCANsat Sensors; Resetting Resource Scanner Type To 0");
				type = SCANtype.Nothing;
			}
			try
			{
				colorFull = ConfigNode.ParseColor(Full);
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Color Format Incorrect; Reverting To Default Full Resource Color: {0}", e);
				colorFull = palette.cb_reddishPurple;
			}
			try
			{
				colorEmpty = ConfigNode.ParseColor(Empty);
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Color Format Incorrect; Reverting To Default Empty Resource Color: {0}", e);
				colorEmpty = palette.magenta;
			}
		}

		public string Name
		{
			get { return name; }
		}

		public SCANtype Type
		{
			get { return type;}
		}

		public Color ColorFull
		{
			get { return colorFull; }
		}

		public Color ColorEmpty
		{
			get { return colorEmpty; }
		}

	}
}
