#region license
/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 * 
 * SCANdata - encapsulates scanned data for a body
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Contracts;
using FinePrint;
using FinePrint.Contracts;
using FinePrint.Contracts.Parameters;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SCANsat.SCAN_Platform;
using SCANsat.SCAN_Platform.Palettes;
using SCANsat.SCAN_Platform.Palettes.ColorBrewer;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat.SCAN_Data
{
	public class SCANdata
	{
		/* MAP: state */
		private Int32[,] coverage = new Int32[360, 180];
		private float[,] heightmap = new float[360, 180];
		private float[,] kethaneValueMap = new float[360, 180]; //Store kethane cell data in here
		private CelestialBody body;
		private Texture2D map_small = new Texture2D(360, 180, TextureFormat.RGB24, false);
		private SCANterrainConfig terrainConfig;

		/* MAP: options */
		private bool disabled;

		/* MAP: constructor */
		internal SCANdata(CelestialBody b)
		{
			body = b;
			float? clamp = null;
			if (b.ocean)
				clamp = 0;

			terrainConfig = SCANcontroller.getTerrainNode(b.name);

			if (terrainConfig == null)
			{
				terrainConfig = new SCANterrainConfig(SCANconfigLoader.SCANNode.DefaultMinHeightRange, SCANconfigLoader.SCANNode.DefaultMaxHeightRange, clamp, SCANUtil.paletteLoader(SCANconfigLoader.SCANNode.DefaultPalette, 7), 7, false, false, body);
				SCANcontroller.addToTerrainConfigData(body.name, terrainConfig);
			}
		}

		#region Public accessors
		/* Accessors: body-specific variables */
		public Int32[,] Coverage
		{
			get { return coverage; }
			internal set { coverage = value; }
		}

		public float[,] HeightMap
		{
			get { return heightmap; }
			internal set { heightmap = value; }
		}

		public CelestialBody Body
		{
			get { return body; }
		}

		public Texture2D Map
		{
			get { return map_small; }
		}

		public float[,] KethaneValueMap
		{
			get { return kethaneValueMap; }
			set { kethaneValueMap = value; }
		}

		public SCANterrainConfig TerrainConfig
		{
			get { return terrainConfig; }
			internal set { terrainConfig = value; }
		}

		public bool Disabled
		{
			get { return disabled; }
			internal set { disabled = value; }
		}
		#endregion

		#region Anomalies
		/* DATA: anomalies and such */
		private SCANanomaly[] anomalies;

		public SCANanomaly[] Anomalies
		{
			get
			{
				if (anomalies == null)
				{
					PQSCity[] sites = body.GetComponentsInChildren<PQSCity>(true);
					anomalies = new SCANanomaly[sites.Length];
					for (int i = 0; i < sites.Length; ++i)
					{
						anomalies[i] = new SCANanomaly(sites[i].name, body.GetLongitude(sites[i].transform.position), body.GetLatitude(sites[i].transform.position), sites[i]);
					}
				}
				for (int i = 0; i < anomalies.Length; ++i)
				{
					anomalies[i].Known = SCANUtil.isCovered(anomalies[i].Longitude, anomalies[i].Latitude, this, SCANtype.Anomaly);
					anomalies[i].Detail = SCANUtil.isCovered(anomalies[i].Longitude, anomalies[i].Latitude, this, SCANtype.AnomalyDetail);
				}
				return anomalies;
			}
		}

		#endregion

		#region Waypoints

		private List<SCANwaypoint> waypoints;
		private bool waypointsLoaded;

		public void addToWaypoints()
		{
			if (SCANcontroller.controller == null)
				return;

			addToWaypoints(SCANcontroller.controller.LandingTarget);
		}

		public void addToWaypoints(SCANwaypoint w)
		{
			if (waypoints == null)
			{
				waypoints = new List<SCANwaypoint>() { w };
				return;
			}

			if (waypoints.Any(a => a.LandingTarget))
				waypoints.RemoveAll(a => a.LandingTarget);

			waypoints.Add(w);
		}

		public void removeTargetWaypoint()
		{
			if (waypoints == null)
				return;

			if (waypoints.Any(a => a.LandingTarget))
				waypoints.RemoveAll(a => a.LandingTarget);

			SCANcontroller.controller.LandingTarget = null;
		}

		public List<SCANwaypoint> Waypoints
		{
			get
			{
				if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
				{
					if (waypoints == null)
						waypoints = new List<SCANwaypoint>();
				}

				if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER && !SCANcontroller.controller.ContractsLoaded)
					return new List<SCANwaypoint>();
				else if (!waypointsLoaded)
				{
					waypointsLoaded = true;
					if (waypoints == null)
						waypoints = new List<SCANwaypoint>();
					if (ContractSystem.Instance != null)
					{
						var surveys = ContractSystem.Instance.GetCurrentActiveContracts<SurveyContract>();
						for (int i = 0; i < surveys.Length; i++)
						{
							if (surveys[i].targetBody == body)
							{
								for (int j = 0; j < surveys[i].AllParameters.Count(); j++)
								{
									if (surveys[i].AllParameters.ElementAt(j).GetType() == typeof(SurveyWaypointParameter))
									{
										SurveyWaypointParameter s = (SurveyWaypointParameter)surveys[i].AllParameters.ElementAt(j);
										if (s.State == ParameterState.Incomplete)
										{
											SCANwaypoint p = new SCANwaypoint(s);
											if (p.Way != null)
												waypoints.Add(p);
										}
									}
								}
							}
						}

						var stationary = ContractSystem.Instance.GetCurrentActiveContracts<SatelliteContract>();
						for (int i = 0; i < stationary.Length; i++)
						{
							SpecificOrbitParameter orbit = stationary[i].GetParameter<SpecificOrbitParameter>();
							if (orbit == null)
								continue;

							if (orbit.targetBody == body)
							{
								for (int j = 0; j < stationary[i].AllParameters.Count(); j++)
								{
									if (stationary[i].AllParameters.ElementAt(j).GetType() == typeof(StationaryPointParameter))
									{
										StationaryPointParameter s = (StationaryPointParameter)stationary[i].AllParameters.ElementAt(j);
										if (s.State == ParameterState.Incomplete)
										{
											SCANwaypoint p = new SCANwaypoint(s);
											if (p.Way != null)
												waypoints.Add(p);
										}
									}
								}
							}
						}
					}

					if (WaypointManager.Instance() != null)
					{
						var remaining = WaypointManager.Instance().AllWaypoints();
						for (int i = 0; i < remaining.Count; i++)
						{
							Waypoint p = remaining[i];
							if (p.isOnSurface && p.isNavigatable)
							{
								if (p.celestialName == body.GetName())
								{
									if (p.contractReference != null)
									{
										if (p.contractReference.ContractState == Contract.State.Active)
										{
											if (!waypoints.Any(a => a.Way == p))
											{
												waypoints.Add(new SCANwaypoint(p));
											}
										}
									}
									else if (!waypoints.Any(a => a.Way == p))
									{
										waypoints.Add(new SCANwaypoint(p));
									}
								}
							}
						}
					}
				}

				return waypoints;
			}
		}

		#endregion

		#region Scanning coverage
		/* DATA: coverage */
		private int[] coverage_count = Enumerable.Repeat(360 * 180, 32).ToArray();
		internal void updateCoverage()
		{
			for (int i = 0; i < 32; ++i)
			{
				SCANtype t = (SCANtype)(1 << i);
				int cc = 0;
				for (int x = 0; x < 360; ++x)
				{
					for (int y = 0; y < 180; ++y)
					{
						if ((coverage[x, y] & (Int32)t) == 0)
							++cc;
					}
				}
				coverage_count[i] = cc;
			}
		}
		internal int getCoverage(SCANtype type)
		{
			int uncov = 0;
			if ((type & SCANtype.AltimetryLoRes) != SCANtype.Nothing)
				uncov += coverage_count[0];
			if ((type & SCANtype.AltimetryHiRes) != SCANtype.Nothing)
				uncov += coverage_count[1];
			if ((type & SCANtype.Biome) != SCANtype.Nothing)
				uncov += coverage_count[3];
			if ((type & SCANtype.Anomaly) != SCANtype.Nothing)
				uncov += coverage_count[4];
			if ((type & SCANtype.AnomalyDetail) != SCANtype.Nothing)
				uncov += coverage_count[5];
			if ((type & SCANtype.Kethane) != SCANtype.Nothing)
				uncov += coverage_count[6];
			if ((type & SCANtype.MetallicOre) != SCANtype.Nothing)
				uncov += coverage_count[7];
			if ((type & SCANtype.Ore) != SCANtype.Nothing)
				uncov += coverage_count[8];
			if ((type & SCANtype.Kethane_4) != SCANtype.Nothing)
				uncov += coverage_count[9];
			if ((type & SCANtype.Uraninite) != SCANtype.Nothing)
				uncov += coverage_count[10];
			if ((type & SCANtype.Thorium) != SCANtype.Nothing)
				uncov += coverage_count[11];
			if ((type & SCANtype.Alumina) != SCANtype.Nothing)
				uncov += coverage_count[12];
			if ((type & SCANtype.Water) != SCANtype.Nothing)
				uncov += coverage_count[13];
			if ((type & SCANtype.Aquifer) != SCANtype.Nothing)
				uncov += coverage_count[14];
			if ((type & SCANtype.Minerals) != SCANtype.Nothing)
				uncov += coverage_count[15];
			if ((type & SCANtype.Substrate) != SCANtype.Nothing)
				uncov += coverage_count[16];
			if ((type & SCANtype.KEEZO) != SCANtype.Nothing)
				uncov += coverage_count[17];
			if ((type & SCANtype.Karbonite) != SCANtype.Nothing)
				uncov += coverage_count[18];
			if ((type & SCANtype.Regolith_10) != SCANtype.Nothing)
				uncov += coverage_count[19];
			if ((type & SCANtype.Regolith_11) != SCANtype.Nothing)
				uncov += coverage_count[20];
			return uncov;
		}
		
		#endregion

		#region Map Texture
		/* DATA: all hail the red line of scanning */
		private int scanline = 0;
		private int scanstep = 0;
		//Draws the actual map texture
		internal void drawHeightScanline(SCANtype type)
		{
			Color[] cols_height_map_small = map_small.GetPixels(0, scanline, 360, 1);
			for (int ilon = 0; ilon < 360; ilon += 1)
			{
				int scheme = SCANcontroller.controller.colours;
				float val = heightmap[ilon, scanline];
				if (val == 0)
				{ //Some preparation for bigger changes in map caching, automatically calculate elevation for every point on the small map, only display scanned areas
					if (body.pqsController == null)
					{
						heightmap[ilon, scanline] = 0;
						cols_height_map_small[ilon] = palette.lerp(palette.black, palette.white, UnityEngine.Random.value);
						continue;
					}
					else
					{
						// convert to radial vector
						val = (float)SCANUtil.getElevation(body, ilon - 180, scanline - 90);
						if (val == 0)
							val = -0.001f; // this is terrible
						heightmap[ilon, scanline] = val;
					}
				}
				Color c = palette.black;
				if (SCANUtil.isCovered(ilon, scanline, this, SCANtype.Altimetry))
				{ //We check for coverage down here now, after elevation data is collected
					if (SCANUtil.isCovered(ilon, scanline, this, SCANtype.AltimetryHiRes))
						c = palette.heightToColor(val, scheme, this);
					else
						c = palette.heightToColor(val, 1, this);
				}
				else
				{
					c = palette.grey;
					if (scanline % 30 == 0 && ilon % 3 == 0)
					{
						c = palette.white;
					}
					else if (ilon % 30 == 0 && scanline % 3 == 0)
					{
						c = palette.white;
					}
				}
				if (type != SCANtype.Nothing)
				{
					if (!SCANUtil.isCoveredByAll(ilon, scanline, this, type))
					{
						c = palette.lerp(c, palette.black, 0.5f);
					}
				}
				cols_height_map_small[ilon] = c;
			}
			map_small.SetPixels(0, scanline, 360, 1, cols_height_map_small);
			scanline = scanline + 1;
			if (scanline >= 180)
			{
				scanstep += 1;
				scanline = 0;
			}
		}

		//Updates the red scanning line
		internal void updateImages(SCANtype type)
		{
			if (palette.small_redline == null)
			{
				palette.small_redline = new Color[360];
				for (int i = 0; i < 360; i++)
					palette.small_redline[i] = palette.red;
			}
			drawHeightScanline(type);
			if (scanline < 179)
			{
				map_small.SetPixels(0, scanline + 1, 360, 1, palette.small_redline);
			}
			map_small.Apply();
		}
		#endregion

		#region Map Utilities
		/* DATA: debug option to fill in the map */
		internal void fillMap()
		{
			for (int i = 0; i < 360; i++)
			{
				for (int j = 0; j < 180; j++)
				{
					coverage[i, j] |= (Int32)SCANtype.Everything;
				}
			}
		}

		internal void fillResourceMap()
		{
			for (int i = 0; i < 360; i++)
			{
				for (int j = 0; j < 180; j++)
				{
					coverage[i, j] |= (Int32)SCANtype.AllResources;
				}
			}
		}

		/* DATA: reset the map */
		internal void reset()
		{
			coverage = new Int32[360, 180];
			heightmap = new float[360, 180];
			resetImages();
		}
		internal void resetImages()
		{
			// Just draw a simple grid to initialize the image; the map will appear on top of it
			for (int y = 0; y < map_small.height; y++)
			{
				for (int x = 0; x < map_small.width; x++)
				{
					if ((x % 30 == 0 && y % 3 > 0) || (y % 30 == 0 && x % 3 > 0))
					{
						map_small.SetPixel(x, y, palette.white);
					}
					else
					{
						map_small.SetPixel(x, y, palette.grey);
					}
				}
			}
			map_small.Apply();
		}
		internal void resetResources()
		{
			for (int x = 0; x < 360; x++)
			{
				for (int y = 0; y < 180; y++)
				{
					coverage[x, y] &= (int)SCANtype.Everything_SCAN;
				}
			}
		}
		#endregion

		#region Data Serialize/Deserialize

		//Take the Int32[] coverage and convert it to a single dimension byte array
		private byte[] ConvertToByte(Int32[,] iArray)
		{
			byte[] bArray = new byte[360 * 180 * 4];
			int k = 0;
			for (int i = 0; i < 360; i++)
			{
				for (int j = 0; j < 180; j++)
				{
					byte[] bytes = BitConverter.GetBytes(iArray[i, j]);
					for (int m = 0; m < bytes.Length; m++)
					{
						bArray[k++] = bytes[m];
					}
				}
			}
			return bArray;
		}

		//Convert byte array from persistent file to usable Int32[]
		private Int32[,] ConvertToInt(byte[] bArray)
		{
			Int32[,] iArray = new Int32[360, 180];
			int k = 0;
			for (int i = 0; i < 360; i++)
			{
				for (int j = 0; j < 180; j++)
				{
					iArray[i, j] = BitConverter.ToInt32(bArray, k);
					k += 4;
				}
			}
			return iArray;
		}

		//One time conversion of single byte[,] to Int32 to recover old scanning data
		private Int32[,] RecoverToInt(byte[,] bArray)
		{
			Int32[,] iArray = new Int32[360, 180];
			for (int i = 0; i < 360; i++)
			{
				for (int j = 0; j < 180; j++)
				{
					iArray[i, j] = (Int32)bArray[i, j];
				}
			}
			return iArray;
		}

		/* DATA: serialization and compression */
		internal string integerSerialize()
		{
			byte[] bytes = ConvertToByte(Coverage);
			MemoryStream mem = new MemoryStream();
			BinaryFormatter binf = new BinaryFormatter();
			binf.Serialize(mem, bytes);
			string blob = Convert.ToBase64String(SCAN_CLZF2.Compress(mem.ToArray()));
			return blob.Replace("/", "-").Replace("=", "_");
		}

		internal void integerDeserialize(string blob, bool b)
		{
			try
			{
				blob = blob.Replace("-", "/").Replace("_", "=");
				byte[] bytes = Convert.FromBase64String(blob);
				bytes = SCAN_CLZF2.Decompress(bytes);
				MemoryStream mem = new MemoryStream(bytes, false);
				BinaryFormatter binf = new BinaryFormatter();
				if (b)
				{
					byte[,] bRecover = new byte[360, 180];
					bRecover = (byte[,])binf.Deserialize(mem);
					Coverage = RecoverToInt(bRecover);
				}
				else
				{
					byte[] bArray = (byte[])binf.Deserialize(mem);
					Coverage = ConvertToInt(bArray);
				}
			}
			catch (Exception e)
			{
				Coverage = new Int32[360, 180];
				HeightMap = new float[360, 180];
				throw e;
			}
			resetImages();
		}

#endregion

	}
}
