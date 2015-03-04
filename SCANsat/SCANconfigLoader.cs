using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Platform.Palettes;
using SCANsat.SCAN_Platform.Palettes.ColorBrewer;
using SCANsat.SCAN_Platform.Palettes.FixedColors;
using UnityEngine;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat
{
	public class SCANconfig
	{
		private ConfigNode SCANtopNode;
		private Dictionary<int, ConfigNode> subTerrainNodes = new Dictionary<int,ConfigNode>();
		private Dictionary<string, SCANconfig> resourceNodes = new Dictionary<string,SCANconfig>();
		private Dictionary<int, ConfigNode> subResourceNodes = new Dictionary<int,ConfigNode>();

		internal SCANconfig(ConfigNode node)
		{
			SCANtopNode = node;
		}

		internal void addToTerrainNodes(int i, ConfigNode node)
		{
			if (!subTerrainNodes.ContainsKey(i))
				subTerrainNodes.Add(i, node);
			else
				Debug.LogWarning("[SCANsat] Error During Terrain Node Loading; Config Node Already Exists");
		}

		internal void addToResourceNodes(string s, ConfigNode node)
		{
			if (!resourceNodes.ContainsKey(s))
			{
				SCANconfig rNode = new SCANconfig(node);
				resourceNodes.Add(s, rNode);
			}
			else
				Debug.LogWarning("[SCANsat] Error During Resource Node Loading; Config Node Already Exists");
		}

		internal void addToBodyResourceNodes(int i, ConfigNode node)
		{
			if (!subResourceNodes.ContainsKey(i))
				subResourceNodes.Add(i, node);
			else
				Debug.LogWarning("[SCANsat] Error During Body Resource Node Loading; Config Node Already Exists");
		}

		public ConfigNode SCANTopNode
		{
			get { return SCANtopNode; }
		}

		public ConfigNode terrainNode(int i)
		{
			if (subTerrainNodes.ContainsKey(i))
				return subTerrainNodes[i];
			else
				return null;
		}

		public SCANconfig resourceNode(string s)
		{
			if (resourceNodes.ContainsKey(s))
				return resourceNodes[s];
			else
				return null;
		}

		public ConfigNode subResourceNode(int i)
		{
			if (subResourceNodes.ContainsKey(i))
				return subResourceNodes[i];
			else
				return null;
		}
	}

	static class SCANconfigLoader
	{
		private static bool globalResource = false;
		private static bool initialized = false;

		private const string configFile = "/GameData/SCANsat/Resources/SCANcolors.cfg";
		private static readonly string fileLocation = (KSPUtil.ApplicationRootPath + configFile).Replace('\\', '/');

		private static SCANconfig SCANnode;

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
			ConfigNode node = loadTopNode();
			if (node == null)
				return;

			SCANnode = new SCANconfig(node);

			loadSCANtypes();
			loadResources();
			loadSCANcolorSettings();
		}

		internal static void saveConfigNode()
		{
			if (SCANnode != null)
				SCANnode.SCANTopNode.Save(fileLocation);
		}

		private static ConfigNode loadTopNode()
		{
			return GameDatabase.Instance.GetConfigNode("SCANSAT_COLOR_CONFIG");
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
			SCANcontroller.ResourceList = new Dictionary<string, SCANresourceGlobal>();
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

			if (SCANcontroller.ResourceList.Count == 0)
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

		private static void loadSCANcolorSettings()
		{
			loadTerrainConfigs();
			loadBiomeSlopeConfigs();
			if (initialized)
				loadResourceConfigs();
		}

		private static void loadTerrainConfigs()
		{
			SCANcontroller.TerrainConfigData = new Dictionary<string, SCANterrainConfig>();
			ConfigNode altimetryNode = SCANnode.SCANTopNode.GetNode("SCANSAT_ALTIMETRY");
			if (altimetryNode != null)
			{
				float defaultMin, defaultMax;
				string defaultPalette = "Default";
				if (!float.TryParse(altimetryNode.GetValue("defaultMinHeightRange"), out defaultMin))
					defaultMin = -1000f;
				if (!float.TryParse(altimetryNode.GetValue("defaultMaxHeightRange"), out defaultMax))
					defaultMax = 8000f;
				if (altimetryNode.HasValue("defaultPalette"))
					defaultPalette = altimetryNode.GetValue("defaultPalette");

				foreach (ConfigNode terrainNode in GameDatabase.Instance.GetConfigNodes("SCANSAT_PLANETARY_CONFIG"))
				{
					if (terrainNode != null)
					{
						int index, size;
						float min, max, clampf;
						float? clamp;
						bool reverse, discrete;
						string palette = defaultPalette;
						Palette color;
						CelestialBody body;
						if (!int.TryParse(terrainNode.GetValue("index"), out index))
							continue;

						body = FlightGlobals.Bodies.FirstOrDefault(b => b.flightGlobalsIndex == index);

						if (body == null)
							continue;

						if (!float.TryParse(terrainNode.GetValue("minHeightRange"), out min))
							min = defaultMin;
						if (!float.TryParse(terrainNode.GetValue("maxHeightRange"), out max))
							max = defaultMax;
						if (terrainNode.HasValue("clampHeight"))
						{
							if (float.TryParse(terrainNode.GetValue("clampHeight"), out clampf))
								clamp = clampf;
							else
								clamp = null;
						}
						else
							clamp = null;
						if (!int.TryParse(terrainNode.GetValue("paletteSize"), out size))
							size = 7;
						if (!bool.TryParse(terrainNode.GetValue("paletteReverse"), out reverse))
							reverse = false;
						if (!bool.TryParse(terrainNode.GetValue("paletteDiscrete"), out discrete))
							discrete = false;

						color = paletteLoader(palette, size);

						SCANterrainConfig data = new SCANterrainConfig(min, max, clamp, color, size, reverse, discrete, body);

						SCANcontroller.addToTerrainConfigData(body.name, data);

						SCANnode.addToTerrainNodes(index, terrainNode);

						data.setNode(terrainNode);
					}
				}
			}
		}

		private static void loadBiomeSlopeConfigs()
		{
			ConfigNode biomeNode = SCANnode.SCANTopNode.GetNode("SCANSAT_BIOME");
			if (biomeNode != null)
			{
				string lowC, highC;
				Color lowColor = new Color();
				Color highColor = new Color();
				float transparency;
				bool stockBiome;
				if (!float.TryParse(biomeNode.GetValue("biomeTransparency"), out transparency))
					transparency = 40;
				if (!bool.TryParse(biomeNode.GetValue("stockBiomeMap"), out stockBiome))
					stockBiome = false;
				lowC = biomeNode.GetValue("lowBiomeColor");
				if (!SCANUtil.loadColor(ref lowColor, lowC))
					lowColor = palette.xkcd_CamoGreen;
				highC = biomeNode.GetValue("highBiomeColor");
				if (!SCANUtil.loadColor(ref highColor, highC))
					highColor = palette.xkcd_Marigold;

				SCANcontroller.controller.LowBiomeColor = lowColor;
				SCANcontroller.controller.HighBiomeColor = highColor;
				SCANcontroller.controller.useStockBiomes = stockBiome;
				SCANcontroller.controller.biomeTransparency = transparency;
			}
		}

		private static void loadResourceConfigs()
		{
			ConfigNode resourceNode = SCANnode.SCANTopNode.GetNode("SCANSAT_RESOURCE");
			if (resourceNode != null)
			{
				foreach (ConfigNode resourceConfig in resourceNode.GetNodes("SCANSAT_RESOURCE_CONFIG"))
				{
					if (resourceConfig != null)
					{
						string name, lowC, highC;
						Color lowColor = new Color();
						Color highColor = new Color();
						float transparency;
						SCANresourceType rType = null;
						SCANresourceGlobal res = null;

						if (resourceConfig.HasValue("name"))
							name = resourceConfig.GetValue("name");
						else
							continue;

						if (SCANcontroller.ResourceTypes.ContainsKey(name))
							rType = SCANcontroller.ResourceTypes[name];

						if (SCANcontroller.ResourceList.ContainsKey(name))
							res = SCANcontroller.ResourceList[name];

						if (res == null)
							continue;

						lowC = resourceConfig.GetValue("lowResourceColor");
						highC = resourceConfig.GetValue("highResourceColor");
						if (!SCANUtil.loadColor(ref lowColor, lowC))
						{
							if (rType != null)
								lowColor = rType.ColorEmpty;
							else
								lowColor = palette.xkcd_DarkPurple;
						}
						if (!SCANUtil.loadColor(ref highColor, highC))
						{
							if (rType != null)
								highColor = rType.ColorFull;
							else
								highColor = palette.xkcd_Magenta;
						}

						if (!float.TryParse(resourceConfig.GetValue("resourceTransparency"), out transparency))
							transparency = 20;

						res.MinColor = lowColor;
						res.MaxColor = highColor;
						res.Transparency = transparency;

						SCANnode.addToResourceNodes(name, resourceConfig);

						res.setNode(SCANnode.resourceNode(name));

						foreach (ConfigNode planetaryResourceConfig in resourceConfig.GetNodes("RESOURCE_PLANETARY_CONFIG"))
						{
							if (planetaryResourceConfig != null)
							{
								int index;
								float minValue, maxValue;
								CelestialBody body;

								if (!int.TryParse(planetaryResourceConfig.GetValue("index"), out index))
									continue;

								body = FlightGlobals.Bodies.FirstOrDefault(b => b.flightGlobalsIndex == index);

								if (body == null)
									continue;

								if (!res.BodyConfigs.ContainsKey(body.name))
									continue;

								res.CurrentBodyConfig(body.name);

								if (!float.TryParse(planetaryResourceConfig.GetValue("lowResourceCutoff"), out minValue))
									minValue = 0.1f;
								if (!float.TryParse(planetaryResourceConfig.GetValue("highResourceCutoff"), out maxValue))
									maxValue = 10f;

								res.CurrentBody.MinValue = minValue;
								res.CurrentBody.MaxValue = maxValue;

								SCANnode.resourceNode(name).addToBodyResourceNodes(index, planetaryResourceConfig);

								res.CurrentBody.setNode(planetaryResourceConfig);
							}
						}
					}
				}
			}
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
