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

		internal SCAN_Color_Config(string filepath)
		{
			FilePath = filepath;

			Load();
		}

		public override void OnDecodeFromConfigNode()
		{
			try
			{
				SCANcontroller.MasterTerrainNodes = SCANsat_Altimetry.ToDictionary(a => a.Name, a => a);
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error while loading SCANsat terrain config settings: {0}", e);
			}

			try
			{
				SCANcontroller.MasterResourceNodes = SCANsat_Resources.ToDictionary(a => a.Name, a => a);
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error while loading SCANsat resource config settings: {0}", e);
			}
		}

		public override void OnEncodeToConfigNode()
		{
			try
			{
				SCANsat_Altimetry = SCANcontroller.MasterTerrainNodes.Values.ToList();
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error while saving SCANsat altimetry config data: {0}", e);
			}

			try
			{
				SCANsat_Resources = SCANcontroller.MasterResourceNodes.Values.ToList();
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error while saving SCANsat resource config data: {0}", e);
			}
		}

		public float DefaultMinHeightRange
		{
			get { return defaultMinHeightRange; }
			internal set { defaultMinHeightRange = value; }
		}

		public float DefaultMaxHeightRange
		{
			get { return defaultMaxHeightRange; }
			internal set { defaultMaxHeightRange = value; }
		}

		public string DefaultPalette
		{
			get { return defaultPalette; }
			internal set { defaultPalette = value; }
		}

		public Color LowBiomeColor
		{
			get { return lowBiomeColor; }
			internal set { lowBiomeColor = value; }
		}

		public Color HighBiomeColor
		{
			get { return highBiomeColor; }
			internal set { highBiomeColor = value; }
		}

		public float BiomeTransparency
		{
			get { return biomeTransparency; }
			internal set { biomeTransparency = value; }
		}

		public bool StockBiomeMap
		{
			get { return stockBiomeMap; }
			internal set { stockBiomeMap = value; }
		}

		public Color LowSlopeColor
		{
			get { return lowSlopeColor; }
			internal set { lowSlopeColor = value; }
		}

		public Color HighSlopeColor
		{
			get { return highSlopeColor; }
			internal set { highSlopeColor = value; }
		}
	}
}
