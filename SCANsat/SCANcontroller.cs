#region license
/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANcontroller - scenario module that handles all scanning
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SCANsat.SCAN_UI;
using SCANsat.SCAN_UI.UI_Framework;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Platform;
using SCANsat.SCAN_Platform.Palettes;
using SCANsat.SCAN_Platform.Palettes.ColorBrewer;
using SCANsat.SCAN_Platform.Palettes.FixedColors;
using SCANsat.SCAN_Toolbar;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat
{
	[KSPScenario(ScenarioCreationOptions.AddToAllGames | ScenarioCreationOptions.AddToExistingGames, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION)]
	public class SCANcontroller : ScenarioModule
	{
		public static SCANcontroller controller
		{
			get
			{
				Game g = HighLogic.CurrentGame;
				if (g == null) return null;
				try
				{
					var mod = g.scenarios.FirstOrDefault(m => m.moduleName == typeof(SCANcontroller).Name);
					if (mod != null)
						return (SCANcontroller)mod.moduleRef;
					else
						return null;
				}
				catch (Exception e)
				{
					SCANUtil.SCANlog("Could not find SCANsat Scenario Module: {0}", e);
					return null;
				}
			}
		}

		private static int minScanAlt = 5000;
		private static int maxScanAlt = 500000;
		private static int bestScanAlt = 250000;
		[KSPField(isPersistant = true)]
		public int colours = 0;
		[KSPField(isPersistant = true)]
		public bool map_markers = true;
		[KSPField(isPersistant = true)]
		public bool map_flags = true;
		[KSPField(isPersistant = true)]
		public bool map_waypoints = true;
		[KSPField(isPersistant = true)]
		public bool map_orbit = true;
		[KSPField(isPersistant = true)]
		public bool map_asteroids = true;
		[KSPField(isPersistant = true)]
		public bool map_grid = true;
		[KSPField(isPersistant = true)]
		public bool map_ResourceOverlay = false; //Is the overlay activated for the selected resource
		[KSPField(isPersistant = true)]
		public int projection = 0;
		[KSPField(isPersistant = true)]
		public int map_width = 0;
		[KSPField(isPersistant = true)]
		public int map_x = 100;
		[KSPField(isPersistant = true)]
		public int map_y = 50;
		[KSPField(isPersistant = true)]
		public string anomalyMarker = "✗";
		public string closeBox = "✖";
		[KSPField(isPersistant = true)]
		public bool legend = false;
		[KSPField(isPersistant = true)]
		public bool scan_background = true;
		[KSPField(isPersistant = true)]
		public int timeWarpResolution = 20;
		[KSPField(isPersistant = true)]
		public string resourceSelection;
		[KSPField(isPersistant = true)]
		public bool dataRebuild = true;
		[KSPField(isPersistant = true)]
		public bool mainMapVisible = false;
		[KSPField(isPersistant = true)]
		public bool bigMapVisible = false;
		[KSPField(isPersistant = true)]
		public bool kscMapVisible = false;
		[KSPField(isPersistant = true)]
		public bool toolTips = true;
		[KSPField(isPersistant = true)]
		public bool useStockAppLauncher = true;
		[KSPField(isPersistant = true)]
		public bool resourceBiomeLock = true;
		[KSPField(isPersistant = true)]
		public bool useStockBiomes = false;
		[KSPField(isPersistant = true)]
		public float biomeTransparency = 40;
		[KSPField(isPersistant = true)]
		public bool mechJebTargetSelection = false;
		[KSPField(isPersistant = true)]
		public bool easyModeScanning = true;
		[KSPField(isPersistant = true)]
		public bool needsNarrowBand = true;

		/* Biome and slope colors can't be serialized properly as a KSP Field */
		public Color lowBiomeColor = new Color(0, 0.46f, 0.02345098f, 1);
		public Color highBiomeColor = new Color(0.7f, 0.2388235f, 0, 1);
		public Color lowSlopeColorOne = new Color(0.004705883f, 0.6f, 0.3788235f, 1);
		public Color highSlopeColorOne = new Color(0.9764706f, 1, 0.4627451f, 1);
		public Color lowSlopeColorTwo = new Color(0.9764706f, 1, 0.4627451f, 1);
		public Color highSlopeColorTwo = new Color(0.94f, 0.2727843f, 0.007372549f, 1);

		/* Available resources for overlays; loaded from SCANsat configs; only loaded once */
		private static Dictionary<string, SCANresourceGlobal> masterResourceNodes = new Dictionary<string,SCANresourceGlobal>();

		/* Resource types loaded from SCANsat configs; only needs to be loaded once */
		private static Dictionary<string, SCANresourceType> resourceTypes = new Dictionary<string,SCANresourceType>();

		/* Terrain height and color option containers loaded from SCANsat configs; only needs to be loaded once */
		private static Dictionary<string, SCANterrainConfig> masterTerrainNodes = new Dictionary<string,SCANterrainConfig>();

		/* List of resources currently loaded from resource addons */
		private static List<string> loadedResources = new List<string>();

		/* Primary SCANsat vessel dictionary; loaded every time */
		private Dictionary<Guid, SCANvessel> knownVessels = new Dictionary<Guid, SCANvessel>();

		/* Primary SCANdata dictionary; loaded every time*/
		private Dictionary<string, SCANdata> body_data = new Dictionary<string,SCANdata>();

		/* MechJeb Landing Target Integration */
		private bool mechjebLoaded, targetSelecting, targetSelectingActive;
		private Vector2d landingTargetCoords;
		private CelestialBody landingTargetBody;
		private SCANwaypoint landingTarget;

		/* UI window objects */
		internal SCANmainMap mainMap;
		internal SCANsettingsUI settingsWindow;
		internal SCANinstrumentUI instrumentsWindow;
		internal SCANBigMap BigMap;
		internal SCANkscMap kscMap;
		internal SCANcolorSelection colorManager;

		/* App launcher object */
		internal SCANappLauncher appLauncher;

		/* Used in case the loading process is interupted somehow */
		private bool loaded = false;

		/* Used to make sure all contracts are loaded */
		private bool contractsLoaded = false;

		private bool unDocked, docked = false;
		private Vessel PartFromVessel, PartToVessel, NewVessel, OldVessel;
		private int timer = 0;
		private CelestialBody body = null;
		private bool bodyScanned = false;
		private bool bodyCoverage = false;

		#region Public Accessors

		public SCANdata getData(string bodyName)
		{
			if (body_data.ContainsKey(bodyName))
				return body_data[bodyName];

			return null;
		}

		public SCANdata getData(int index)
		{
			if (body_data.Count >= index)
				return body_data.ElementAt(index).Value;
			else
				SCANUtil.SCANdebugLog("SCANdata dictionary index out of range; something went wrong here...");

			return null;
		}

		public List<SCANdata> GetAllData
		{
			get { return body_data.Values.ToList(); }
		}

		public int GetDataCount
		{
			get { return body_data.Count; }
		}

		/* Use this method to protect against duplicate dictionary keys */
		public void addToBodyData (CelestialBody b, SCANdata data)
		{
			if (!body_data.ContainsKey(b.name))
				body_data.Add(b.name, data);
			else
				Debug.LogError("[SCANsat] Warning: SCANdata Dictionary Already Contains Key of This Type");
		}

		public static List<SCANterrainConfig> EncodeTerrainConfigs
		{
			get
			{
				try
				{
					return masterTerrainNodes.Values.ToList();
				}
				catch (Exception e)
				{
					SCANUtil.SCANlog("Error while saving SCANsat altimetry config data: {0}", e);
				}

				return new List<SCANterrainConfig>();
			}
		}

		public static void setMasterTerrainNodes (List<SCANterrainConfig> terrainConfigs)
		{
			masterTerrainNodes.Clear();
			try
			{
				masterTerrainNodes = terrainConfigs.ToDictionary(a => a.Name, a => a);
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error while loading SCANsat terrain config settings: {0}", e);
			}
		}

		public static SCANterrainConfig getTerrainNode(string name)
		{
			if (masterTerrainNodes.ContainsKey(name))
				return masterTerrainNodes[name];
			else
				SCANUtil.SCANlog("SCANsat terrain config [{0}] cannot be found in master terrain storage list", name);

			return null;
		}

		public static void updateTerrainConfig (SCANterrainConfig t)
		{
			SCANterrainConfig update = getTerrainNode(t.Name);
			if (update != null)
			{
				update.MinTerrain = t.MinTerrain;
				update.MaxTerrain = t.MaxTerrain;
				update.ClampTerrain = t.ClampTerrain;
				update.PalSize = t.PalSize;
				update.PalRev = t.PalRev;
				update.PalDis = t.PalDis;
				update.ColorPal = t.ColorPal;
			}
		}

		public static void addToTerrainConfigData (string name, SCANterrainConfig data)
		{
			if (!masterTerrainNodes.ContainsKey(name))
				masterTerrainNodes.Add(name, data);
			else
				Debug.LogError("[SCANsat] Warning: SCANterrain Data Dictionary Already Contains Key Of This Type");
		}

		public static int MasterResourceCount
		{
			get { return loadedResources.Count; }
		}

		public static List<SCANresourceGlobal> EncodeResourceConfigs
		{
			get
			{
				try
				{
					return masterResourceNodes.Values.ToList();
				}
				catch (Exception e)
				{
					SCANUtil.SCANlog("Error while saving SCANsat resource config data: {0}", e);
				}

				return new List<SCANresourceGlobal>();
			}
		}

		public static void setMasterResourceNodes (List<SCANresourceGlobal> resourceConfigs)
		{
			try
			{
				masterResourceNodes = resourceConfigs.ToDictionary(a => a.Name, a => a);
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error while loading SCANsat resource config settings: {0}", e);
			}
		}

		public static SCANresourceGlobal getResourceNode (string resourceName)
		{
			if (masterResourceNodes.ContainsKey(resourceName))
				return masterResourceNodes[resourceName];
			else
				SCANUtil.SCANlog("SCANsat resource [{0}] cannot be found in master resource storage list", resourceName);

			return null;
		}

		public static SCANresourceGlobal GetFirstResource
		{
			get
			{
				if (masterResourceNodes.Count > 0)
					return masterResourceNodes.ElementAt(0).Value;
				else
					SCANUtil.SCANlog("SCANsat resource storage list is empty; something probably went wrong here...");

				return null;
			}
		}

		public static void updateSCANresource (SCANresourceGlobal r, bool all)
		{
			SCANresourceGlobal update = getResourceNode(r.Name);
			if (update != null)
			{
				update.MinColor = r.MinColor;
				update.MaxColor = r.MaxColor;
				update.Transparency = r.Transparency;
				if (all)
				{
					for (int i = 0; i < update.getBodyCount; i++)
					{
						SCANresourceBody b = update.getBodyConfig(i);
						if (b != null)
						{
							SCANresourceBody bNew = r.getBodyConfig(b.BodyName);
							if (bNew != null)
							{
								b.MinValue = bNew.MinValue;
								b.MaxValue = bNew.MaxValue;
							}
						}
					}
				}
				else
				{
					SCANresourceBody b = update.getBodyConfig(r.CurrentBody.BodyName);
					if (b != null)
					{
						b.MinValue = r.CurrentBody.MinValue;
						b.MaxValue = r.CurrentBody.MaxValue;
					}
				}
			}
		}

		public static void addToResourceData (string name, SCANresourceGlobal res)
		{
			if (!masterResourceNodes.ContainsKey(name))
			{
				masterResourceNodes.Add(name, res);
			}
			else
				Debug.LogError(string.Format("[SCANsat] Warning: SCANResource Dictionary Already Contains Key of This Type: Resource: {0}", name));
		}

		public static SCANresourceType getResourceType (string name)
		{
			if (resourceTypes.ContainsKey(name))
				return resourceTypes[name];
			else
				SCANUtil.SCANlog("SCANsat resource type [{0}] cannot be found in master resource type storage list", name);

			return null;
		}

		public static void addToResourceTypes (string name, SCANresourceType type)
		{
			if (!resourceTypes.ContainsKey(name))
			{
				resourceTypes.Add(name, type);
			}
			else
				Debug.LogError(string.Format("[SCANsat] Warning: SCANResourceType Dictionary Already Contains Key of This Name: SCAN Resource Type: {0}", name));
		}

		public static void addToLoadedResourceNames (string name)
		{
			if (!loadedResources.Contains(name))
				loadedResources.Add(name);
			else
				Debug.LogError(string.Format("[SCANsat] Warning: Loaded Resource List Already Contains Resource Of Name: {0}", name));
		}

		public static List<SCANresourceGlobal> setLoadedResourceList()
		{
			List<SCANresourceGlobal> rList = new List<SCANresourceGlobal>();
			SCANresourceGlobal ore = null;

			foreach (string r in loadedResources)
			{
				if (masterResourceNodes.ContainsKey(r))
				{
					if (r != "Ore")
						rList.Add(masterResourceNodes[r]);
					else
						ore = masterResourceNodes[r];
				}
			}

			if (ore != null)
				rList.Insert(0, ore);

			return rList;
		}

		public List<SCANvessel> Known_Vessels
		{
			get { return knownVessels.Values.ToList(); }
		}

		public int ActiveSensors
		{
			get { return activeSensors; }
		}

		public int ActiveVessels
		{
			get { return activeVessels; }
		}

		public int ActualPasses
		{
			get { return actualPasses; }
		}

		public bool ContractsLoaded
		{
			get { return contractsLoaded; }
		}

		public bool MechJebLoaded
		{
			get { return mechjebLoaded; }
			set { mechjebLoaded = value; }
		}

		public bool TargetSelecting
		{
			get { return targetSelecting; }
			internal set { targetSelecting = value; }
		}

		public bool TargetSelectingActive
		{
			get { return targetSelectingActive; }
			internal set
			{
				if (targetSelecting)
					targetSelectingActive = value;
				else
					targetSelectingActive = false;
			}
		}

		public Vector2d LandingTargetCoords
		{
			get { return landingTargetCoords; }
			internal set { landingTargetCoords = value; }
		}

		public CelestialBody LandingTargetBody
		{
			get { return landingTargetBody; }
			set { landingTargetBody = value; }
		}

		public SCANwaypoint LandingTarget
		{
			get { return landingTarget; }
			set { landingTarget = value; }
		}

		#endregion

		public override void OnLoad(ConfigNode node)
		{
			try
			{
				lowBiomeColor = ConfigNode.ParseColor(node.GetValue("lowBiomeColor"));
				highBiomeColor = ConfigNode.ParseColor(node.GetValue("highBiomeColor"));
				lowSlopeColorOne = ConfigNode.ParseColor(node.GetValue("lowSlopeColorOne"));
				highSlopeColorOne = ConfigNode.ParseColor(node.GetValue("highSlopeColorOne"));
				lowSlopeColorTwo = ConfigNode.ParseColor(node.GetValue("lowSlopeColorTwo"));
				highSlopeColorTwo = ConfigNode.ParseColor(node.GetValue("highSlopeColorTwo"));
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error While Loading SCANsat Colors: {0}", e);
			}


			ConfigNode node_vessels = node.GetNode("Scanners");
			if (node_vessels != null)
			{
				SCANUtil.SCANlog("SCANsat Controller: Loading {0} known vessels", node_vessels.CountNodes);
				foreach (ConfigNode node_vessel in node_vessels.GetNodes("Vessel"))
				{
					Guid id;
					try
					{
						id = new Guid(node_vessel.GetValue("guid"));
					}
					catch (Exception e)
					{
						SCANUtil.SCANlog("Something Went Wrong Loading This SCAN Vessel; Moving On To The Next: {0}", e);
						continue;
					}
					foreach (ConfigNode node_sensor in node_vessel.GetNodes("Sensor"))
					{
						int sensor;
						double fov, min_alt, max_alt, best_alt;
						if (!int.TryParse(node_sensor.GetValue("type"), out sensor))
							sensor = 0;
						if (!double.TryParse(node_sensor.GetValue("fov"), out fov))
							fov = 3;
						if (!double.TryParse(node_sensor.GetValue("min_alt"), out min_alt))
							min_alt = minScanAlt;
						if (!double.TryParse(node_sensor.GetValue("max_alt"), out max_alt))
							max_alt = maxScanAlt;
						if (!double.TryParse(node_sensor.GetValue("best_alt"), out best_alt))
							best_alt = bestScanAlt;
						registerSensor(id, (SCANtype)sensor, fov, min_alt, max_alt, best_alt);
					}
				}
			}

			ConfigNode node_progress = node.GetNode("Progress");
			if (node_progress != null)
			{
				foreach (ConfigNode node_body in node_progress.GetNodes("Body"))
				{
					float min, max, clamp;
					float? clampState = null;
					Palette dataPalette;
					SCANwaypoint target = null;
					string paletteName = "";
					int pSize;
					bool pRev, pDis, disabled;
					string body_name = node_body.GetValue("Name");
					SCANUtil.SCANlog("SCANsat Controller: Loading map for {0}", body_name);
					CelestialBody body = FlightGlobals.Bodies.FirstOrDefault(b => b.name == body_name);
					if (body != null)
					{
						SCANdata data = getData(body.name);
						if (data == null)
							data = new SCANdata(body);
						if (!body_data.ContainsKey(body_name))
							body_data.Add(body_name, data);
						else
							body_data[body_name] = data;
						try
						{
							string mapdata = node_body.GetValue("Map");
							if (dataRebuild)
							{ //On the first load deserialize the "Map" value to both coverage arrays
								data.integerDeserialize(mapdata, true);
							}
							else
							{
								data.integerDeserialize(mapdata, false);
							}
						}
						catch (Exception e)
						{
							SCANUtil.SCANlog("Something Went Wrong Loading Scanning Data; Resetting Coverage: {0}", e);
							data.reset();
							// fail somewhat gracefully; don't make the save unloadable 
						}
						try // Make doubly sure that nothing here can interupt the Scenario Module loading process
						{
							//Verify that saved data types can be converted, revert to default values otherwise
							if (node_body.HasValue("LandingTarget"))
								target = loadWaypoint(node_body.GetValue("LandingTarget"));
							if (bool.TryParse(node_body.GetValue("Disabled"), out disabled))
								data.Disabled = disabled;
							if (!float.TryParse(node_body.GetValue("MinHeightRange"), out min))
								min = data.TerrainConfig.DefaultMinHeight;
							if (!float.TryParse(node_body.GetValue("MaxHeightRange"), out max))
								max = data.TerrainConfig.DefaultMaxHeight;
							if (node_body.HasValue("ClampHeight"))
							{
								if (float.TryParse(node_body.GetValue("ClampHeight"), out clamp))
									clampState = clamp;
							}
							if (!int.TryParse(node_body.GetValue("PaletteSize"), out pSize))
								pSize = data.TerrainConfig.DefaultPaletteSize;
							if (!bool.TryParse(node_body.GetValue("PaletteReverse"), out pRev))
								pRev = data.TerrainConfig.DefaultReverse;
							if (!bool.TryParse(node_body.GetValue("PaletteDiscrete"), out pDis))
								pDis = data.TerrainConfig.DefaultDiscrete;
							if (node_body.HasValue("PaletteName"))
								paletteName = node_body.GetValue("PaletteName");
							dataPalette = SCANUtil.paletteLoader(paletteName, pSize);
							if (dataPalette.hash == PaletteLoader.defaultPalette.hash)
							{
								paletteName = "Default";
								pSize = 7;
							}

							SCANterrainConfig dataTerrainConfig = getTerrainNode(body.name);

							if (dataTerrainConfig == null)
								dataTerrainConfig = new SCANterrainConfig(min, max, clampState, dataPalette, pSize, pRev, pDis, body);
							else
								setNewTerrainConfigValues(dataTerrainConfig, min, max, clampState, dataPalette, pSize, pRev, pDis);

							data.TerrainConfig = dataTerrainConfig;

							if (target != null)
								data.addToWaypoints(target);
						}
						catch (Exception e)
						{
							SCANUtil.SCANlog("Error Loading SCANdata; Reverting To Default Settings: {0}", e);
						}
					}
				}
			}
			dataRebuild = false; //Used for the one-time update to the new integer array

			if (SCANconfigLoader.GlobalResource)
			{
				if (string.IsNullOrEmpty(resourceSelection))
					resourceSelection = masterResourceNodes.ElementAt(0).Key;
				else if (!masterResourceNodes.ContainsKey(resourceSelection))
					resourceSelection = masterResourceNodes.ElementAt(0).Key;
			}
			ConfigNode node_resources = node.GetNode("SCANResources");
			if (node_resources != null)
			{
				foreach(ConfigNode node_resource_type in node_resources.GetNodes("ResourceType"))
				{
					if (node_resource_type != null)
					{
						string name = node_resource_type.GetValue("Resource");
						string lowColor = node_resource_type.GetValue("MinColor");
						string highColor = node_resource_type.GetValue("MaxColor");
						string transparent = node_resource_type.GetValue("Transparency");
						string minMaxValues = node_resource_type.GetValue("MinMaxValues");
						loadCustomResourceValues(minMaxValues, name, lowColor, highColor, transparent);
					}
				}
			}
			loaded = true;
		}

		public override void OnSave(ConfigNode node)
		{
			node.AddValue("lowBiomeColor", ConfigNode.WriteColor(lowBiomeColor));
			node.AddValue("highBiomeColor", ConfigNode.WriteColor(highBiomeColor));
			node.AddValue("lowSlopeColorOne", ConfigNode.WriteColor(lowSlopeColorOne));
			node.AddValue("highSlopeColorOne", ConfigNode.WriteColor(highSlopeColorOne));
			node.AddValue("lowSlopeColorTwo", ConfigNode.WriteColor(lowSlopeColorTwo));
			node.AddValue("highSlopeColorTwo", ConfigNode.WriteColor(highSlopeColorTwo));

			ConfigNode node_vessels = new ConfigNode("Scanners");
			foreach (Guid id in knownVessels.Keys)
			{
				ConfigNode node_vessel = new ConfigNode("Vessel");
				node_vessel.AddValue("guid", id.ToString());
				if (knownVessels[id].vessel != null) node_vessel.AddValue("name", knownVessels[id].vessel.vesselName); // not read
				foreach (SCANsensor sensor in knownVessels[id].sensors.Values)
				{
					ConfigNode node_sensor = new ConfigNode("Sensor");
					node_sensor.AddValue("type", (int)sensor.sensor);
					node_sensor.AddValue("fov", sensor.fov);
					node_sensor.AddValue("min_alt", sensor.min_alt);
					node_sensor.AddValue("max_alt", sensor.max_alt);
					node_sensor.AddValue("best_alt", sensor.best_alt);
					node_vessel.AddNode(node_sensor);
				}
				node_vessels.AddNode(node_vessel);
			}
			node.AddNode(node_vessels);
			if (body_data != null)
			{
				ConfigNode node_progress = new ConfigNode("Progress");
				foreach (string body_name in body_data.Keys)
				{
					ConfigNode node_body = new ConfigNode("Body");
					SCANdata body_scan = body_data[body_name];
					node_body.AddValue("Name", body_name);
					node_body.AddValue("Disabled", body_scan.Disabled);
					SCANwaypoint w = body_scan.Waypoints.FirstOrDefault(a => a.LandingTarget);
					if (w != null)
						node_body.AddValue("LandingTarget", string.Format("{0:N4},{1:N4}", w.Latitude, w.Longitude));
					node_body.AddValue("MinHeightRange", body_scan.TerrainConfig.MinTerrain);
					node_body.AddValue("MaxHeightRange", body_scan.TerrainConfig.MaxTerrain);
					if (body_scan.TerrainConfig.ClampTerrain != null)
						node_body.AddValue("ClampHeight", body_scan.TerrainConfig.ClampTerrain);
					node_body.AddValue("PaletteName", body_scan.TerrainConfig.ColorPal.name);
					node_body.AddValue("PaletteSize", body_scan.TerrainConfig.PalSize);
					node_body.AddValue("PaletteReverse", body_scan.TerrainConfig.PalRev);
					node_body.AddValue("PaletteDiscrete", body_scan.TerrainConfig.PalDis);
					node_body.AddValue("Map", body_scan.integerSerialize());
					node_progress.AddNode(node_body);
				}
				node.AddNode(node_progress);
			}
			if (resourceTypes.Count > 0 && masterResourceNodes.Count > 0)
			{
				ConfigNode node_resources = new ConfigNode("SCANResources");
				foreach (SCANresourceGlobal r in masterResourceNodes.Values)
				{
					if (r != null)
					{
						SCANUtil.SCANdebugLog("Saving Resource: {0}", r.Name);
						ConfigNode node_resource_type = new ConfigNode("ResourceType");
						node_resource_type.AddValue("Resource", r.Name);
						node_resource_type.AddValue("MinColor", ConfigNode.WriteColor(r.MinColor));
						node_resource_type.AddValue("MaxColor", ConfigNode.WriteColor(r.MaxColor));
						node_resource_type.AddValue("Transparency", r.Transparency);

						string rMinMax = saveResources(r);
						node_resource_type.AddValue("MinMaxValues", rMinMax);
						node_resources.AddNode(node_resource_type);
					}
				}
				node.AddNode(node_resources);
			}
		}

		private void Start()
		{
			GameEvents.onVesselSOIChanged.Add(SOIChange);
			GameEvents.onVesselCreate.Add(newVesselCheck);
			GameEvents.onPartCouple.Add(dockingCheck);
			GameEvents.Contract.onContractsLoaded.Add(contractsCheck);
			if (HighLogic.LoadedSceneIsFlight)
			{
				if (!body_data.ContainsKey(FlightGlobals.currentMainBody.name))
					body_data.Add(FlightGlobals.currentMainBody.name, new SCANdata(FlightGlobals.currentMainBody));
				RenderingManager.AddToPostDrawQueue(5, drawTarget);
				try
				{
					mainMap = gameObject.AddComponent<SCANmainMap>();
					settingsWindow = gameObject.AddComponent<SCANsettingsUI>();
					instrumentsWindow = gameObject.AddComponent<SCANinstrumentUI>();
					colorManager = gameObject.AddComponent<SCANcolorSelection>();
					BigMap = gameObject.AddComponent<SCANBigMap>();
				}
				catch (Exception e)
				{
					SCANUtil.SCANlog("Something Went Wrong Initializing UI Objects: {0}", e);
				}
			}
			else if (HighLogic.LoadedSceneHasPlanetarium)
			{
				if (!body_data.ContainsKey(Planetarium.fetch.Home.name))
					body_data.Add(Planetarium.fetch.Home.name, new SCANdata(Planetarium.fetch.Home));
				try
				{
					kscMap = gameObject.AddComponent<SCANkscMap>();
					settingsWindow = gameObject.AddComponent<SCANsettingsUI>();
					colorManager = gameObject.AddComponent<SCANcolorSelection>();
				}
				catch (Exception e)
				{
					SCANUtil.SCANlog("Something Went Wrong Initializing UI Objects: {0}", e);
				}
			}
			if (useStockAppLauncher)
				appLauncher = gameObject.AddComponent<SCANappLauncher>();
		}

		private void Update()
		{
			if (scan_background && loaded)
			{
				scanFromAllVessels();
			}

			if (unDocked || docked)
			{
				if (timer < 30)
					timer++;
				else
				{
					if (unDocked)
					{
						if (NewVessel != null)
						{
							removeVessel(NewVessel);
							addVessel(NewVessel);
							NewVessel = null;
						}

						if (OldVessel != null)
						{
							removeVessel(OldVessel);
							addVessel(OldVessel);
							OldVessel = null;
						}
					}

					if (docked)
					{
						if (PartFromVessel != null)
						{
							removeVessel(PartFromVessel);
							PartFromVessel = null;
						}

						if (PartToVessel != null)
						{
							removeVessel(PartToVessel);
							PartToVessel = null;
						}

						addVessel(FlightGlobals.ActiveVessel);
					}

					unDocked = false;
					docked = false;
					timer = 0;
				}
			}

			if (!HighLogic.LoadedSceneIsFlight && HighLogic.LoadedScene != GameScenes.TRACKSTATION)
				return;

			if (!easyModeScanning)
				return;

			if (body == null)
			{
				if (HighLogic.LoadedSceneIsFlight)
				{
					body = FlightGlobals.ActiveVessel.mainBody;
					bodyScanned = false;
					bodyCoverage = false;
				}
				else if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
				{
					MapObject target = PlanetariumCamera.fetch.target;

					if (target.type != MapObject.MapObjectType.CELESTIALBODY)
					{
						body = null;
						return;
					}

					body = target.celestialBody;
					bodyScanned = false;
					bodyCoverage = false;
				}
			}

			if (bodyScanned)
				return;

			if (!bodyCoverage)
			{
				if (SCANUtil.GetCoverage((int)SCANtype.AllResources, body) >= 100)
				{
					bodyScanned = true;
					return;
				}
				bodyCoverage = true;
			}

			if (ResourceMap.Instance.IsPlanetScanned(body.flightGlobalsIndex))
			{
				SCANdata data = SCANUtil.getData(body);
				if (data == null)
				{
					data = new SCANdata(body);
					addToBodyData(body, data);
				}
				data.fillResourceMap();
				bodyScanned = true;
			}
		}

		private void OnDestroy()
		{
			GameEvents.onVesselSOIChanged.Remove(SOIChange);
			GameEvents.onVesselCreate.Remove(newVesselCheck);
			GameEvents.onPartCouple.Remove(dockingCheck);
			GameEvents.Contract.onContractsLoaded.Remove(contractsCheck);
			if (mainMap != null)
				Destroy(mainMap);
			if (settingsWindow != null)
				Destroy(settingsWindow);
			if (instrumentsWindow != null)
				Destroy(instrumentsWindow);
			if (kscMap != null)
				Destroy(kscMap);
			if (BigMap != null)
				Destroy(BigMap);
			if (appLauncher != null)
				Destroy(appLauncher);
		}

		private void drawTarget()
		{
			if (mechJebTargetSelection)
				return;

			if (!MapView.MapIsEnabled)
				return;

			Vessel v = FlightGlobals.ActiveVessel;

			if (v == null)
				return;

			SCANdata d = getData(v.mainBody.name);

			if (d == null)
				return;

			SCANwaypoint target = d.Waypoints.FirstOrDefault(a => a.LandingTarget);

			if (target == null)
				return;

			SCANuiUtil.drawTargetOverlay(v.mainBody, target.Latitude, target.Longitude, XKCDColors.DarkGreen);
		}

		private void removeVessel(Vessel v)
		{
			if (isVesselKnown(v))
			{
				foreach (SCANtype t in Enum.GetValues(typeof(SCANtype)))
					unregisterSensor(v, t);
			}
		}

		private void addVessel(Vessel v)
		{
			foreach (SCANsat s in v.FindPartModulesImplementing<SCANsat>())
			{
				if (s.scanningNow())
					registerSensor(v.id, (SCANtype)s.sensorType, s.fov, s.min_alt, s.max_alt, s.best_alt);
			}
		}

		private void dockingCheck(GameEvents.FromToAction<Part, Part> Parts)
		{
			PartFromVessel = Parts.from.vessel;
			PartToVessel = Parts.to.vessel;

			docked = true;
		}

		private void newVesselCheck(Vessel v)
		{
			if (v.loaded)
			{
				if (v.Parts.Count > 1)
					NewVessel = v;
				else
					NewVessel = null;
				OldVessel = FlightGlobals.ActiveVessel;

				unDocked = true;
			}
		}

		private void contractsCheck()
		{
			contractsLoaded = true;
		}

		private void SOIChange(GameEvents.HostedFromToAction<Vessel, CelestialBody> VC)
		{
			if (!body_data.ContainsKey(VC.to.name))
				body_data.Add(VC.to.name, new SCANdata(VC.to));
			body = VC.to;
			bodyScanned = false;
			bodyCoverage = false;
		}

		private void setNewTerrainConfigValues(SCANterrainConfig terrain, float min, float max, float? clamp, Palette c, int size, bool reverse, bool discrete)
		{
			terrain.MinTerrain = min;
			terrain.MaxTerrain = max;
			terrain.ClampTerrain = clamp;
			terrain.ColorPal = c;
			terrain.PalSize = size;
			terrain.PalRev = reverse;
			terrain.PalDis = discrete;
		}

		private string saveResources(SCANresourceGlobal resource)
		{
			List<string> sL = new List<string>();
			for (int j = 0; j < resource.getBodyCount; j++)
			{
				SCANresourceBody bodyRes = resource.getBodyConfig(j);
				if (bodyRes != null)
				{
					string a = string.Format("{0}|{1:F3}|{2:F3}", bodyRes.Index, bodyRes.MinValue, bodyRes.MaxValue);
					sL.Add(a);
				}
			}

			return string.Join(",", sL.ToArray());
		}

		private void loadCustomResourceValues(string s, string resource, string low, string high, string trans)
		{
			SCANresourceGlobal r;

			if (masterResourceNodes.ContainsKey(resource))
				r = masterResourceNodes[resource];
			else
				return;

			Color lowColor = new Color();
			Color highColor = new Color();
			float transparent = 0;

			try
			{
				lowColor = ConfigNode.ParseColor(low);
			}
			catch (Exception e)
			{
				lowColor = r.DefaultLowColor;
				SCANUtil.SCANlog("Error in parsing low color for resource [{0}]: ", resource, e);
			}

			try
			{
				highColor = ConfigNode.ParseColor(high);
			}
			catch (Exception e)
			{
				highColor = r.DefaultHighColor;
				SCANUtil.SCANlog("Error in parsing high color for resource [{0}]: ", resource, e);
			}

			if (!float.TryParse(trans, out transparent))
				transparent = r.DefaultTrans;

			r.MinColor = lowColor;
			r.MaxColor = highColor;
			r.Transparency = transparent;

			if (!string.IsNullOrEmpty(s))
			{
				string[] sA = s.Split(',');
				for (int i = 0; i < sA.Length; i++)
				{
					string[] sB = sA[i].Split('|');
					try
					{
						int j = 0;
						float min = 0;
						float max = 0;
						if (!int.TryParse(sB[0], out j))
							continue;
						CelestialBody b;
						if ((b = FlightGlobals.Bodies.FirstOrDefault(a => a.flightGlobalsIndex == j)) != null)
						{
							SCANresourceBody res = r.getBodyConfig(b.name);
							if (res != null)
							{
								if (!float.TryParse(sB[1], out min))
									min = res.DefaultMinValue;
								if (!float.TryParse(sB[2], out max))
									max = res.DefaultMaxValue;
								res.MinValue = min;
								res.MaxValue = max;
							}
							else
								SCANUtil.SCANlog("No resources found assigned for Celestial Body: {0}, skipping...", b.name);
						}
						else
							SCANUtil.SCANlog("No Celestial Body found matching this saved resource value: {0}, skipping...", j);
					}
					catch (Exception e)
					{
						SCANUtil.SCANlog("Something Went Wrong While Loading Custom Resource Settings; Reverting To Default Values: {0}", e);
					}
				}
			}
		}

		private SCANwaypoint loadWaypoint(string s)
		{
			SCANwaypoint w = null;
			string[] a = s.Split(',');
			double lat = 0;
			double lon = 0;
			if (!double.TryParse(a[0], out lat))
				return w;
			if (!double.TryParse(a[1], out lon))
				return w;

			string name = mechJebTargetSelection ? "MechJeb Landing Target" : "Landing Target Site";

			w = new SCANwaypoint(lat, lon, name);

			return w;
		}

		public class SCANsensor
		{
			public SCANtype sensor;
			public double fov;
			public double min_alt, max_alt, best_alt;

			public bool inRange;
			public bool bestRange;
		}

		public class SCANvessel
		{
			public Guid id;
			public Vessel vessel;
			public Dictionary<SCANtype, SCANsensor> sensors = new Dictionary<SCANtype, SCANsensor>();

			public CelestialBody body;
			public double latitude, longitude;
			public int frame;
			public double lastUT;
		}

		internal void registerSensor(Vessel v, SCANtype sensors, double fov, double min_alt, double max_alt, double best_alt)
		{
			registerSensor(v.id, sensors, fov, min_alt, max_alt, best_alt);
			knownVessels[v.id].vessel = v;
			knownVessels[v.id].latitude = SCANUtil.fixLatShift(v.latitude);
			knownVessels[v.id].longitude = SCANUtil.fixLonShift(v.longitude);
		}

		private void registerSensor(Guid id, SCANtype sensors, double fov, double min_alt, double max_alt, double best_alt)
		{
			if (id == null)
				return;
			if (!knownVessels.ContainsKey(id))
				knownVessels[id] = new SCANvessel();
			SCANvessel sv = knownVessels[id];
			sv.id = id;
			sv.vessel = FlightGlobals.Vessels.FirstOrDefault(a => a.id == id);
			if (sv.vessel == null)
			{
				knownVessels.Remove(id);
				return;
			}
			foreach (SCANtype sensor in Enum.GetValues(typeof(SCANtype)))
			{
				if (SCANUtil.countBits((int)sensor) != 1)
					continue;
				if ((sensor & sensors) == SCANtype.Nothing)
					continue;
				double this_fov = fov;
				double this_min_alt = min_alt;
				double this_max_alt = max_alt;
				double this_best_alt = best_alt;
				if (this_max_alt <= 0)
				{
					this_min_alt = 5000;
					this_max_alt = 500000;
					this_best_alt = 200000;
					this_fov = 5;
					if ((sensor & SCANtype.AltimetryHiRes) != SCANtype.Nothing) this_fov = 3;
					if ((sensor & SCANtype.AnomalyDetail) != SCANtype.Nothing)
					{
						this_min_alt = 0;
						this_max_alt = 2000;
						this_best_alt = 0;
						this_fov = 1;
					}
				}
				if (!sv.sensors.ContainsKey(sensor))
					sv.sensors[sensor] = new SCANsensor();
				SCANsensor s = sv.sensors[sensor];
				s.sensor = sensor;
				s.fov = this_fov;
				s.min_alt = this_min_alt;
				s.max_alt = this_max_alt;
				s.best_alt = this_best_alt;
			}
		}

		internal void unregisterSensor(Vessel v, SCANtype sensors)
		{
			if (!knownVessels.ContainsKey(v.id))
				return;

			SCANvessel sv = knownVessels[v.id];
			sv.id = v.id;
			sv.vessel = v;
			foreach (SCANtype sensor in Enum.GetValues(typeof(SCANtype)))
			{
				if ((sensors & sensor) == SCANtype.Nothing)
					continue;
				if (!sv.sensors.ContainsKey(sensor))
					continue;

				sv.sensors.Remove(sensor);
			}
			if (sv.sensors.Count == 0)
			{
				knownVessels.Remove(v.id);
				SCANUtil.SCANdebugLog("Unregister Vessel");
			}
		}

		internal bool isVesselKnown(Guid id, SCANtype sensor)
		{
			if (!knownVessels.ContainsKey(id))
				return false;

			SCANtype all = SCANtype.Nothing;
			foreach (SCANtype s in knownVessels[id].sensors.Keys)
				all |= s;

			return (all & sensor) != SCANtype.Nothing;
		}

		private bool isVesselKnown(Guid id)
		{
			if (!knownVessels.ContainsKey(id))
				return false;

			return knownVessels[id].sensors.Count > 0;
		}

		private bool isVesselKnown(Vessel v)
		{
			if (v.vesselType == VesselType.Debris)
				return false;

			return isVesselKnown(v.id);
		}

		internal SCANsensor getSensorStatus(Vessel v, SCANtype sensor)
		{
			if (!knownVessels.ContainsKey(v.id))
				return null;
			if (!knownVessels[v.id].sensors.ContainsKey(sensor))
				return null;

			return knownVessels[v.id].sensors[sensor];
		}

		internal SCANtype activeSensorsOnVessel(Guid id)
		{
			if (!knownVessels.ContainsKey(id))
				return SCANtype.Nothing;

			SCANtype sensors = SCANtype.Nothing;
			foreach (SCANtype s in knownVessels[id].sensors.Keys)
				sensors |= s;

			return sensors;
		}

		private int i = 0;
		private static int last_scan_frame;
		private static float last_scan_time;
		private static double scan_UT;
		private int activeSensors, activeVessels;
		private static int currentActiveSensor, currentActiveVessel;
		private void scanFromAllVessels()
		{
			if (Time.realtimeSinceStartup - last_scan_time < 1 && Time.realtimeSinceStartup > last_scan_time) return;
			if (last_scan_frame == Time.frameCount) return;
			last_scan_frame = Time.frameCount;
			last_scan_time = Time.realtimeSinceStartup;
			scan_UT = Planetarium.GetUniversalTime();
			currentActiveSensor = 0;
			currentActiveVessel = 0;
			actualPasses = 0;
			maxRes = 0;
			if (body_data.Count > 0)
			{
				var bdata = body_data.ElementAt(i);     //SCANUtil.getData(FlightGlobals.Bodies[i]); //Update coverage for planets one at a time, rather than all together
				bdata.Value.updateCoverage();
				i++;
				if (i >= body_data.Count) i = 0;
			}
			foreach (Vessel v in FlightGlobals.Vessels)
			{
				if (!knownVessels.ContainsKey(v.id)) continue;
				SCANvessel vessel = knownVessels[v.id];
				SCANdata data = SCANUtil.getData(v.mainBody);
				if (data == null)
					continue;
				vessel.vessel = v;

				if (!data.Disabled)
				{
					if (v.mainBody == FlightGlobals.currentMainBody || scan_background)
					{
						if (isVesselKnown(v))
						{
							doScanPass(knownVessels[v.id], scan_UT, scan_UT, vessel.lastUT, vessel.latitude, vessel.longitude);
							++currentActiveVessel;
							currentActiveSensor += knownVessels[v.id].sensors.Count;
						}
					}
				}

				vessel.body = v.mainBody;
				vessel.frame = Time.frameCount;
				vessel.lastUT = scan_UT;
				vessel.latitude = SCANUtil.fixLatShift(v.latitude);
				vessel.longitude = SCANUtil.fixLonShift(v.longitude);
			}
			activeVessels = currentActiveVessel;
			activeSensors = currentActiveSensor;
		}

		private int actualPasses, maxRes;
		private static Queue<double> scanQueue;
		private void doScanPass(SCANvessel vessel, double UT, double startUT, double lastUT, double llat, double llon)
		{
			Vessel v = vessel.vessel;
			SCANdata data = SCANUtil.getData(v.mainBody);
			if (data == null)
				return;
			double soi_radius = v.mainBody.sphereOfInfluence - v.mainBody.Radius;
			double alt = v.altitude, lat = SCANUtil.fixLatShift(v.latitude), lon = SCANUtil.fixLonShift(v.longitude);
			double res = 0;
			Orbit o = v.orbit;
			bool uncovered;

			if (scanQueue == null) scanQueue = new Queue<double>();
			if (scanQueue.Count != 0) scanQueue.Clear();

		loop: // don't look at me like that, I just unrolled the recursion
			if (res > 0)
			{
				if (double.IsNaN(UT)) goto dequeue;
				if (o.ObT <= 0) goto dequeue;
				if (double.IsNaN(o.getObtAtUT(UT))) goto dequeue;
				Vector3d pos = o.getPositionAtUT(UT);
				double rotation = 0;
				if (v.mainBody.rotates)
				{
					rotation = (360 * ((UT - scan_UT) / v.mainBody.rotationPeriod)) % 360;
				}
				alt = v.mainBody.GetAltitude(pos);
				lat = SCANUtil.fixLatShift(v.mainBody.GetLatitude(pos));
				lon = SCANUtil.fixLonShift(v.mainBody.GetLongitude(pos) - rotation);
				if (alt < 0) alt = 0;
				if (res > maxRes) maxRes = (int)res;
			}
			else
			{
				alt = v.heightFromTerrain;
				if (alt < 0) alt = v.altitude;
			}

			if (Math.Abs(lat - llat) < 1 && Math.Abs(lon - llon) < 1 && res > 0) goto dequeue;
			actualPasses++;

			uncovered = res <= 0;
			foreach (SCANsensor sensor in knownVessels[v.id].sensors.Values)
			{
				if (res <= 0)
				{
					if (data.getCoverage(sensor.sensor) > 0) uncovered = false;
				}

				sensor.inRange = false;
				sensor.bestRange = false;
				if (alt < sensor.min_alt) continue;
				if (alt > Math.Min(sensor.max_alt, soi_radius)) continue;
				sensor.inRange = true;

				double fov = sensor.fov;
				double ba = Math.Min(sensor.best_alt, soi_radius);
				if (alt < ba) fov = (alt / ba) * fov;
				else sensor.bestRange = true;

				double surfscale = 600000d / v.mainBody.Radius;
				if (surfscale < 1) surfscale = 1;
				surfscale = Math.Sqrt(surfscale);
				fov *= surfscale;
				if (fov > 20) fov = 20;

				int f = (int)Math.Truncate(fov);
				int f1 = f + (int)Math.Round(fov - f);

				double clampLat;
				double clampLon;
				for (int x = -f; x <= f1; ++x)
				{
					clampLon = lon + x;	// longitude does not need clamping
					/*if (clampLon < 0  ) clampLon = 0; */
					/*if (clampLon > 360) clampLon = 360; */
					for (int y = -f; y <= f1; ++y)
					{
						clampLat = lat + y;
						if (clampLat > 90) clampLat = 90;
						if (clampLat < -90) clampLat = -90;
						SCANUtil.registerPass(clampLon, clampLat, data, sensor.sensor);
					}
				}
			}
			if (uncovered) return;
			/* 
			if(v.mainBody == FlightGlobals.currentMainBody) {
				if(res > 0) data.map_small.SetPixel((int)Math.Round(lon) + 180, (int)Math.Round(lat) + 90, palette.magenta);
				else data.map_small.SetPixel((int)Math.Round(lon) + 180, (int)Math.Round(lat) + 90, palette.yellow);
			}
			*/

			if (vessel.lastUT <= 0) return;
			if (vessel.frame <= 0) return;
			if (v.LandedOrSplashed) return;
			if (res >= timeWarpResolution) goto dequeue;

			if (startUT > UT)
			{
				scanQueue.Enqueue((startUT + UT) / 2);
				scanQueue.Enqueue(startUT);
				scanQueue.Enqueue(UT);
				scanQueue.Enqueue(lat);
				scanQueue.Enqueue(lon);
				scanQueue.Enqueue(res + 1);
			}
			startUT = UT;
			UT = (lastUT + UT) / 2;
			llat = lat;
			llon = lon;
			res = res + 1;
			goto loop;

		dequeue:
			if (scanQueue.Count <= 0) return;
			UT = scanQueue.Dequeue();
			startUT = scanQueue.Dequeue();
			lastUT = scanQueue.Dequeue();
			llat = scanQueue.Dequeue();
			llon = scanQueue.Dequeue();
			res = scanQueue.Dequeue();
			goto loop;
		}

	}
}
