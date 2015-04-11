using System;
using System.Collections.Generic;
using System.Linq;
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
		private Color lowSlopeColor = palette.xkcd_PukeGreen;
		[Persistent]
		private Color highSlopeColor = palette.xkcd_Lemon;
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
			SCANUtil.SCANdebugLog("SCANsat Color Config Decode");
			SCANUtil.SCANdebugLog("-------->Default Min Height Range =>   {0}", defaultMinHeightRange);
			SCANUtil.SCANdebugLog("-------->Default Max Height Range =>   {0}", defaultMaxHeightRange);
			SCANUtil.SCANdebugLog("-------->Default Palette          =>   {0}", defaultPalette);
			SCANUtil.SCANdebugLog("-------->Low Biome Color          =>   {0}", lowBiomeColor);
			SCANUtil.SCANdebugLog("-------->High Biome Color         =>   {0}", highBiomeColor);
			SCANUtil.SCANdebugLog("-------->Biome Transparency       =>   {0}", biomeTransparency);
			SCANUtil.SCANdebugLog("-------->Stock Biome              =>   {0}", stockBiomeMap);
			SCANUtil.SCANdebugLog("-------->Low Slope Color          =>   {0}", lowSlopeColor);
			SCANUtil.SCANdebugLog("-------->High Slope Color         =>   {0}", highSlopeColor);
			SCANcontroller.setMasterTerrainNodes(SCANsat_Altimetry);
			SCANcontroller.setMasterResourceNodes(SCANsat_Resources);
		}

		public override void OnEncodeToConfigNode()
		{
			SCANUtil.SCANdebugLog("Saving Config Master Node");
			SCANsat_Altimetry = SCANcontroller.EncodeTerrainConfigs;
			SCANsat_Resources = SCANcontroller.EncodeResourceConfigs;

			if (SCANcontroller.controller != null)
			{
				lowBiomeColor = SCANcontroller.controller.lowBiomeColor;
				highBiomeColor = SCANcontroller.controller.highBiomeColor;
				biomeTransparency = SCANcontroller.controller.biomeTransparency;
				lowSlopeColor = SCANcontroller.controller.lowSlopeColor;
				highSlopeColor = SCANcontroller.controller.highSlopeColor;
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

		public Color LowSlopeColor
		{
			get { return lowSlopeColor; }
		}

		public Color HighSlopeColor
		{
			get { return highSlopeColor; }
		}
	}
}
