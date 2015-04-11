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
	public static class SCANconfigLoader
	{
		private static bool globalResource = false;
		private static bool initialized = false;

		private const string configFile = "SCANsat/Resources/SCANcolors";
		private const string configNodeName = "SCAN_Color_Config";

		private static SCAN_Color_Config SCANnode;

		public static SCAN_Color_Config SCANNode
		{
			get { return SCANnode; }
		}

		public static bool GlobalResource
		{
			get { return globalResource; }
		}

		public static bool Initialized
		{
			get { return initialized; }
		}

		internal static void configLoader()
		{
			loadSCANtypes();

			SCANnode = new SCAN_Color_Config(configFile, configNodeName);

			loadResources();
		}

		private static void loadSCANtypes()
		{
			foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("SCANSAT_SENSOR"))
			{
				string name = "";
				int i = 0;
				if (node.HasValue("name"))
					name = node.GetValue("name");
				if (node.HasValue("SCANtype"))
					if (!int.TryParse(node.GetValue("SCANtype"), out i))
						continue;

				SCANcontroller.addToResourceTypes(name, new SCANresourceType(name, i));
			}
		}

		private static void loadResources()
		{
			if (SCANmainMenuLoader.RegolithFound)
			{
				foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("REGOLITH_GLOBAL_RESOURCE"))
				{
					if (node != null)
					{
						string name = "";
						int resourceType;

						if (node.HasValue("ResourceName"))
							name = node.GetValue("ResourceName");
						else
							continue;

						if (!int.TryParse(node.GetValue("ResourceType"), out resourceType))
							continue;
						if (resourceType != 0)
							continue;

						SCANresourceGlobal resource = SCANcontroller.getResourceNode(name);

						if (resource != null)
						{
							foreach (CelestialBody b in FlightGlobals.Bodies)
							{
								SCANresourceBody rBody = resource.getBodyConfig(b.name);
								if (rBody == null)
								{
									SCANresourceBody bodyResource = RegolithConfigLoad(b, resource);

									if (bodyResource != null)
										resource.addToBodyConfigs(b.name, bodyResource, true);
									else
										resource.addToBodyConfigs(b.name, new SCANresourceBody(resource.Name, b, resource.DefaultMinValue, resource.DefaultMaxValue), true);
								}
							}
							SCANcontroller.addToLoadedResourceNames(resource.Name);
						}
						else
						{
							if ((resource = RegolithGlobalConfigLoad(node)) == null)
								continue;

							foreach (CelestialBody body in FlightGlobals.Bodies)
							{
								SCANresourceBody bodyResource = RegolithConfigLoad(body, resource);

								if (bodyResource != null)
									resource.updateBodyConfig(bodyResource);
							}

							SCANcontroller.addToLoadedResourceNames(resource.Name);
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

			if (SCANcontroller.MasterResourceCount == 0)
				globalResource = false;
			else
				globalResource = true;
		}

		private static SCANresourceGlobal RegolithGlobalConfigLoad(ConfigNode node)
		{
			string name = "";
			int resourceType = 0;
			float min = 0.001f;
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
				{
					if (!float.TryParse(distNode.GetValue("MinAbundance"), out min))
						min = 0.001f;
				}
				if (distNode.HasValue("MaxAbundance"))
				{
					if (!float.TryParse(distNode.GetValue("MaxAbundance"), out max))
						max = 0.1f;
				}
			}
			if (min == max)
				max += 0.001f;

			SCANresourceGlobal res = new SCANresourceGlobal(name, 20, min, max, palette.magenta, palette.cb_reddishPurple, type, 2);

			foreach (CelestialBody b in FlightGlobals.Bodies)
			{
				SCANresourceBody r = new SCANresourceBody(name, b, min, max);
				res.addToBodyConfigs(b.name, r, true);
			}

			return res;
		}

		private static SCANresourceBody RegolithConfigLoad(CelestialBody body, SCANresourceGlobal r)
		{
			SCANresourceBody bodyResource = null;

			foreach (ConfigNode bodyNode in GameDatabase.Instance.GetConfigNodes("REGOLITH_PLANETARY_RESOURCE"))
			{
				if (bodyNode != null)
				{
					bodyResource = RegolithConfigLoad(bodyNode);
					if (bodyResource == null)
						continue;

					if (bodyResource.Body.name == body.name)
					{
						if (bodyResource.ResourceName == r.Name)
							break;
						else
						{
							bodyResource = null;
							continue;
						}
					}
					bodyResource = null;
				}
			}

			return bodyResource;
		}

		private static SCANresourceBody RegolithConfigLoad(ConfigNode node)
		{
			float min = 0.001f;
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
				{
					if (!float.TryParse(distNode.GetValue("MinAbundance"), out min))
						min = 0.001f;
				}
				if (distNode.HasValue("MaxAbundance"))
				{
					if (!float.TryParse(distNode.GetValue("MaxAbundance"), out max))
						max = 10f;
				}
			}
			else
				return null;

			if (min == max)
				max += 0.001f;

			SCANresourceBody bodyRes = new SCANresourceBody(name, b, min, max);

			return bodyRes;
		}

		private static SCANresourceType OverlayResourceType(string s)
		{
			return SCANcontroller.getResourceType(s);
		}
	}
}
