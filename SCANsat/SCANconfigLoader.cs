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
	static class SCANconfigLoader
	{
		private static bool globalResource = false;

		public static bool GlobalResource
		{
			get { return globalResource; }
		}

		internal static void resourceLoader()
		{
			loadSCANtypes();
			loadResources();
			loadSCANcolorSettings();
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
			SCANcontroller.ResourceList = new Dictionary<string, Dictionary<string, SCANresource>>();
			if (SCANmainMenuLoader.RegolithFound)
			{
				foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("REGOLITH_GLOBAL_RESOURCE"))
				{
					if (node != null)
					{
						SCANresource resource = null;
						if ((resource = RegolithConfigLoad(node)) == null)
							continue;
						foreach (CelestialBody body in FlightGlobals.Bodies)
						{
							SCANresource bodyResource = null;
							foreach (ConfigNode bodyNode in GameDatabase.Instance.GetConfigNodes("REGOLITH_PLANETARY_RESOURCE"))
							{
								bodyResource = RegolithConfigLoad(bodyNode);
								if (bodyResource == null)
									continue;
								if (string.IsNullOrEmpty(bodyResource.Body))
								{
									bodyResource = null;
									continue;
								}
								if (bodyResource.Body == body.name)
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
							if (bodyResource == null)
							{
								SCANcontroller.addToResourceData(resource.Name, body.name, SCANresource.resourceCopy(resource));
							}
							else
							{
								SCANcontroller.addToResourceData(bodyResource.Name, bodyResource.Body, bodyResource);
							}
						}
					}
				}
			}
			if (SCANmainMenuLoader.kethaneLoaded)
			{
				foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("KethaneResource"))
				{
					if (node != null)
					{
						string name = node.GetValue("Resource");
						SCANresourceType type = null;
						if ((type = OverlayResourceType(name)) == null)
							continue;
						Color full = type.ColorFull;
						Color empty = type.ColorFull;
						float max = 1000000f;
						ConfigNode subNode = node.GetNode("Generator");
						if (subNode != null)
						{
							float.TryParse(subNode.GetValue("MaxQuantity"), out max); //Global max quantity
							foreach (CelestialBody Body in FlightGlobals.Bodies)
							{
								bool bodySubValue = false;
								float subMax = 1000000f;
								foreach (ConfigNode bodySubNode in subNode.GetNodes("Body"))
								{
									string body = bodySubNode.GetValue("name");
									if (body == Body.name)
									{
										if (bodySubNode.HasValue("MaxQuantity"))
										{
											float.TryParse(bodySubNode.GetValue("MaxQuantity"), out subMax); //Optional body-specific max quantity
											bodySubValue = true;
											break;
										}
										break;
									}
								}
								if (bodySubValue)
									max = subMax;
								SCANresource resource = new SCANresource(name, Body.name, full, empty, 0f, max, type, SCANresource_Source.Kethane);
								SCANcontroller.addToResourceData(name, Body.name, resource);
							}
						}
					}
				}
			}
			if (SCANcontroller.ResourceList.Count == 0)
				globalResource = false;
			else
				globalResource = true;
		}

		private static SCANresource RegolithConfigLoad(ConfigNode node)
		{
			float min = .001f;
			float max = 10f;
			string name = "";
			string body = "";
			int resourceType = 0;
			if (node.HasValue("ResourceName"))
				name = node.GetValue("ResourceName");
			else
				return null;
			SCANresourceType type = OverlayResourceType(name);
			if (type == null)
				return null;
			if (type.Type == SCANtype.Nothing)
				return null;
			if (node.HasValue("PlanetName"))
				body = node.GetValue("PlanetName");
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
			SCANresource SCANres = new SCANresource(name, body, type.ColorFull, type.ColorEmpty, min, max, type, SCANresource_Source.Regolith);
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
			loadResourceConfigs();
		}

		private static void loadTerrainConfigs()
		{
			SCANcontroller.TerrainConfigData = new Dictionary<string, SCANterrainConfig>();
			ConfigNode altimetryNode = GameDatabase.Instance.GetConfigNode("SCANSAT_ALTIMETRY");
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
					}
				}
			}
		}

		private static void loadBiomeSlopeConfigs()
		{
			ConfigNode biomeNode = GameDatabase.Instance.GetConfigNode("SCANSAT_BIOME");
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
			}
		}

		private static void loadResourceConfigs()
		{
			SCANcontroller.ResourceConfigData = new Dictionary<string, SCANresourceGlobal>();
			ConfigNode resourceNode = GameDatabase.Instance.GetConfigNode("SCANSAT_RESOURCE");
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
						if (resourceConfig.HasValue("name"))
							name = resourceConfig.GetValue("name");
						else
							continue;
						lowC = resourceConfig.GetValue("lowResourceColor");
						highC = resourceConfig.GetValue("highResourceColor");
						if (!SCANUtil.loadColor(ref lowColor, lowC))
							lowColor = lowColor;
						if (!SCANUtil.loadColor(ref highColor, highC))
							highColor = highColor;
						if (!float.TryParse(resourceConfig.GetValue("resourceTransparency"), out transparency))
							transparency = 20;

						foreach (ConfigNode planetaryResourceConfig in resourceConfig.GetNodes("RESOURCE_PLANETARY_CONFIG"))
						{
							if (planetaryResourceConfig != null)
							{
								string bodyName;
								int index;
								float minValue, maxValue;
								CelestialBody body;

								if (planetaryResourceConfig.HasValue("name"))
									bodyName = planetaryResourceConfig.GetValue("name");
								else
									continue;

								if (!int.TryParse(planetaryResourceConfig.GetValue("index"), out index))
									continue;

								body = FlightGlobals.Bodies.FirstOrDefault(b => b.flightGlobalsIndex == index);

								if (body == null)
									continue;

								if (!float.TryParse(planetaryResourceConfig.GetValue(""), out minValue))
									minValue = 0.1f;
								if (!float.TryParse(planetaryResourceConfig.GetValue(""), out maxValue))
									maxValue = 10f;
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
