#region license
/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCAN_Color_Config - serializable object that stores settings in an external file
 * 
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

using System.Collections.Generic;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Platform;
using SCANsat.SCAN_Platform.Palettes;
using SCANsat.SCAN_Platform.Palettes.ColorBrewer;
using SCANsat.SCAN_Platform.Palettes.FixedColors;
using UnityEngine;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat
{
	public class SCAN_Color_Config : SCAN_ConfigNodeStorage
	{

		[Persistent]
		private float defaultMinHeightRange = -1000;
		[Persistent]
		private float defaultMaxHeightRange = 8000;
		[Persistent]
		private float rangeAboveMaxHeight = 10000;
		[Persistent]
		private float rangeBelowMinHeight = 10000;
		[Persistent]
		private string defaultPalette = "Default";
		[Persistent]
		private Color lowBiomeColor = palette.xkcd_CamoGreen;
		[Persistent]
		private Color highBiomeColor = palette.xkcd_Marigold;
		[Persistent]
		private float biomeTransparency = 40;
		[Persistent]
		private bool stockBiomeMap = false;
		[Persistent]
		private Color bottomLowSlopeColor = palette.xkcd_PukeGreen;
		[Persistent]
		private Color bottemHighSlopeColor = palette.xkcd_Lemon;
		[Persistent]
		private Color topLowSlopeColor = palette.xkcd_Lemon;
		[Persistent]
		private Color topHighSlopeColor = palette.xkcd_OrangeRed;
		[Persistent]
		private List<SCANterrainConfig> SCANsat_Altimetry = new List<SCANterrainConfig>();
		[Persistent]
		private List<SCANresourceGlobal> SCANsat_Resources = new List<SCANresourceGlobal>();

		internal SCAN_Color_Config(string filepath, string node)
		{
			FilePath = filepath;
			TopNodeName = filepath + "/" + node;

			Load();
		}

		public override void OnDecodeFromConfigNode()
		{
			SCANcontroller.setMasterTerrainNodes(SCANsat_Altimetry);
			SCANcontroller.setMasterResourceNodes(SCANsat_Resources);
		}

		public override void OnEncodeToConfigNode()
		{
			SCANUtil.SCANlog("Saving SCANsat configuration file...");
			SCANUtil.SCANlog("SCANcolors.cfg saved to ---> {0}", FilePath);
			SCANsat_Altimetry = SCANcontroller.EncodeTerrainConfigs;
			SCANsat_Resources = SCANcontroller.EncodeResourceConfigs;

			if (SCANcontroller.controller != null)
			{
				lowBiomeColor = SCANcontroller.controller.lowBiomeColor;
				highBiomeColor = SCANcontroller.controller.highBiomeColor;
				biomeTransparency = SCANcontroller.controller.biomeTransparency;
				bottomLowSlopeColor = SCANcontroller.controller.lowSlopeColorOne;
				bottemHighSlopeColor = SCANcontroller.controller.highSlopeColorOne;
				topLowSlopeColor = SCANcontroller.controller.lowSlopeColorTwo;
				topHighSlopeColor = SCANcontroller.controller.highSlopeColorTwo;
			}
		}

		public float DefaultMinHeightRange
		{
			get { return defaultMinHeightRange; }
		}

		public float DefaultMaxHeightRange
		{
			get { return defaultMaxHeightRange; }
		}

		public float RangeAboveMaxHeight
		{
			get { return rangeAboveMaxHeight; }
		}

		public float RangeBelowMinHeight
		{
			get { return rangeBelowMinHeight; }
		}

		public string DefaultPalette
		{
			get { return defaultPalette; }
		}

		public Color LowBiomeColor
		{
			get { return lowBiomeColor; }
		}

		public Color HighBiomeColor
		{
			get { return highBiomeColor; }
		}

		public float BiomeTransparency
		{
			get { return biomeTransparency; }
		}

		public bool StockBiomeMap
		{
			get { return stockBiomeMap; }
		}

		public Color BottomLowSlopeColor
		{
			get { return bottomLowSlopeColor; }
		}

		public Color BottomHighSlopeColor
		{
			get { return bottemHighSlopeColor; }
		}

		public Color TopLowSlopeColor
		{
			get { return topLowSlopeColor; }
		}

		public Color TopHighSlopeColor
		{
			get { return topHighSlopeColor; }
		}
	}
}
