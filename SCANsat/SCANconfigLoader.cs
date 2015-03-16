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

	static class SCANconfigLoader
	{
		private static bool globalResource = false;
		private static bool initialized = false;

		private const string configFile = "/GameData/SCANsat/Resources/SCANcolors.cfg";
		private static readonly string fileLocation = (KSPUtil.ApplicationRootPath + configFile).Replace('\\', '/');

		private static SCAN_Color_Config SCANnode;

		public static bool GlobalResource
		{
			get { return globalResource; }
		}

		public static bool Initialized
		{
			get { return initialized; }
		}

		internal static void resourceLoader()
		{
			SCANnode = new SCAN_Color_Config(fileLocation);

			loadSCANtypes();
			loadResources();
		}

		private static void loadSCANtypes()
		{
			SCANcontroller.ResourceTypes = new Dictionary<string, SCANresourceType>();
			foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("SCANSAT_SENSOR"))
			{
				string name = "";
				int i = 0;
				string colorFull = "";
				string colorEmpty = "";
				if (node.HasValue("name"))
					name = node.GetValue("name");
				if (node.HasValue("SCANtype"))
					if (!int.TryParse(node.GetValue("SCANtype"), out i))
						continue;
				if (node.HasValue("ColorFull"))
					colorFull = node.GetValue("ColorFull");
				if (node.HasValue("ColorEmpty"))
					colorEmpty = node.GetValue("ColorEmpty");
				if (!SCANcontroller.ResourceTypes.ContainsKey(name) && !string.IsNullOrEmpty(name))
					SCANcontroller.ResourceTypes.Add(name, new SCANresourceType(name, i, colorFull, colorEmpty));
			}
		}

		private static void loadResources() //Repopulates the master resources list with data from config nodes
		{
			SCANcontroller.MasterResourceNodes = new Dictionary<string, SCANresourceGlobal>();
			if (SCANmainMenuLoader.RegolithFound)
			{
				foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("REGOLITH_GLOBAL_RESOURCE"))
				{
					if (node != null)
					{
						SCANresourceGlobal resource = null;
						if ((resource = RegolithGlobalConfigLoad(node)) == null)
							continue;

						foreach (CelestialBody body in FlightGlobals.Bodies)
						{
							SCANresourceBody bodyResource = null;
							foreach (ConfigNode bodyNode in GameDatabase.Instance.GetConfigNodes("REGOLITH_PLANETARY_RESOURCE"))
							{
								bodyResource = RegolithConfigLoad(bodyNode);
								if (bodyResource == null)
									continue;

								if (bodyResource.Body.name == body.name)
								{
									if (bodyResource.Name == resource.Name)
										break;
									else
									{
										bodyResource = null;
										continue;
									}
								}
								bodyResource = null;
							}
							if (bodyResource != null)
							{
								SCANresourceGlobal.addToBodyConfigs(resource, bodyResource.Body.name, bodyResource);
							}

							SCANcontroller.addToResourceData(resource.Name, resource);
						}
					}
				}
			}
			//if (SCANmainMenuLoader.kethaneLoaded)
			//{
			//	foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("KethaneResource"))
			//	{
			//		if (node != null)
			//		{
			//			string name = node.GetValue("Resource");
			//			SCANresourceType type = null;
			//			if ((type = OverlayResourceType(name)) == null)
			//				continue;
			//			Color full = type.ColorFull;
			//			Color empty = type.ColorFull;
			//			float max = 1000000f;
			//			ConfigNode subNode = node.GetNode("Generator");
			//			if (subNode != null)
			//			{
			//				float.TryParse(subNode.GetValue("MaxQuantity"), out max); //Global max quantity
			//				foreach (CelestialBody Body in FlightGlobals.Bodies)
			//				{
			//					bool bodySubValue = false;
			//					float subMax = 1000000f;
			//					foreach (ConfigNode bodySubNode in subNode.GetNodes("Body"))
			//					{
			//						string body = bodySubNode.GetValue("name");
			//						if (body == Body.name)
			//						{
			//							if (bodySubNode.HasValue("MaxQuantity"))
			//							{
			//								float.TryParse(bodySubNode.GetValue("MaxQuantity"), out subMax); //Optional body-specific max quantity
			//								bodySubValue = true;
			//								break;
			//							}
			//							break;
			//						}
			//					}
			//					if (bodySubValue)
			//						max = subMax;
			//					SCANresourceBody resource = new SCANresourceBody(name, Body.name, full, empty, 0f, max, type, SCANresource_Source.Kethane);
			//					SCANcontroller.addToResourceData(name, Body.name, resource);
			//				}
						//}
					//}
				//}
			//}

			if (SCANcontroller.MasterResourceNodes.Count == 0)
				globalResource = false;
			else
				globalResource = true;
		}

		private static SCANresourceGlobal RegolithGlobalConfigLoad(ConfigNode node)
		{
			string name = "";
			int resourceType = 0;
			float min = 0.01f;
			float max = 10;

			if (node.HasValue("ResourceName"))
				name = node.GetValue("ResourceName");
			else
				return null;

			SCANresourceType type = OverlayResourceType(name);
			if (type == null)
				return null;
			if (type.Type == SCANtype.Nothing)
				return null;

			if (!int.TryParse(node.GetValue("ResourceType"), out resourceType))
				return null;
			if (resourceType != 0)
				return null;

			ConfigNode distNode = node.GetNode("Distribution");
			if (distNode != null)
			{
				if (distNode.HasValue("MinAbundance"))
					float.TryParse(distNode.GetValue("MinAbundance"), out min);
				if (distNode.HasValue("MaxAbundance"))
					float.TryParse(distNode.GetValue("MaxAbundance"), out max);
			}
			if (min == max)
				max += 0.001f;

			SCANresourceGlobal res = new SCANresourceGlobal(name, 40, type.ColorEmpty, type.ColorFull, type, 2);

			foreach (CelestialBody b in FlightGlobals.Bodies)
			{
				SCANresourceBody r = new SCANresourceBody(name, b, min, max);
				SCANresourceGlobal.addToBodyConfigs(res, b.name, r);
			}

			if (res != null)
				return res;

			return null;
		}

		private static SCANresourceBody RegolithConfigLoad(ConfigNode node)
		{
			float min = .001f;
			float max = 10f;
			string name = "";
			string body = "";
			CelestialBody b = null;
			int resourceType = 0;

			if (node.HasValue("ResourceName"))
				name = node.GetValue("ResourceName");
			else
				return null;

			if (node.HasValue("PlanetName"))
				body = node.GetValue("PlanetName");
			else
				return null;

			if (!int.TryParse(node.GetValue("ResourceType"), out resourceType))
				return null;
			if (resourceType != 0)
				return null;

			b = FlightGlobals.Bodies.FirstOrDefault(a => a.name == body);

			if (b == null)
				return null;

			ConfigNode distNode = node.GetNode("Distribution");
			if (distNode != null)
			{
				if (distNode.HasValue("MinAbundance"))
					float.TryParse(distNode.GetValue("MinAbundance"), out min);
				if (distNode.HasValue("MaxAbundance"))
					float.TryParse(distNode.GetValue("MaxAbundance"), out max);
			}
			else
				return null;

			if (min == max)
				max += 0.001f;

			SCANresourceBody SCANres = new SCANresourceBody(name, b, min, max);

			if (SCANres != null)
				return SCANres;

			return null;
		}

		private static SCANresourceType OverlayResourceType(string s)
		{
			var resourceType = SCANcontroller.ResourceTypes.FirstOrDefault(r => r.Value.Name == s).Value;
			return resourceType;
		}

		internal static Palette paletteLoader(string name, int size)
		{
			if (name == "Default" || string.IsNullOrEmpty(name))
				return PaletteLoader.defaultPalette;
			else
			{
				try
				{
					if (name == "blackForest" || name == "departure" || name == "northRhine" || name == "mars" || name == "wiki2" || name == "plumbago" || name == "cw1_013" || name == "arctic")
					{
						//Load the fixed size color palette by name through reflection
						var fixedPallete = typeof(FixedColorPalettes);
						var fPaletteMethod = fixedPallete.GetMethod(name);
						var fColorP = fPaletteMethod.Invoke(null, null);
						return (Palette)fColorP;
					}
					else
					{
						//Load the ColorBrewer method by name through reflection
						var brewer = typeof(BrewerPalettes);
						var bPaletteMethod = brewer.GetMethod(name);
						var bColorP = bPaletteMethod.Invoke(null, new object[] { size });
						return (Palette)bColorP;
					}
				}
				catch (Exception e)
				{
					SCANUtil.SCANlog("Error Loading Color Palette; Revert To Default: {0}", e);
					return PaletteLoader.defaultPalette;
				}
			}
		}
	}
}
