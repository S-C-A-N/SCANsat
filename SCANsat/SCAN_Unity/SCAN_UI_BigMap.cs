using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SCANsat.Unity.Interfaces;
using SCANsat.Unity;
using SCANsat.Unity.Unity;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Map;
using SCANsat.SCAN_UI.UI_Framework;
using KSP.UI;
using FinePrint;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat.SCAN_Unity
{
	public class SCAN_UI_BigMap : ISCAN_BigMap
	{
		private bool _isVisible;

		private static SCANmap bigmap;
		private static CelestialBody body;
		private SCANdata data;
		private Vessel vessel;
		private bool updateMap;
		private StringBuilder infoString;

		private Dictionary<int, List<List<Vector2d>>> gridLines = new Dictionary<int, List<List<Vector2d>>>();
		private Vector2 _mapScreenPosition;

		private SCANresourceGlobal currentResource;
		private List<SCANresourceGlobal> resources;

		private SCAN_BigMap uiElement;

		private int steps = 80;
		private List<SimpleLabelInfo> orbitLabels = new List<SimpleLabelInfo>();

		private static SCAN_UI_BigMap instance;

		public static SCAN_UI_BigMap Instance
		{
			get { return instance; }
		}

		public SCAN_UI_BigMap()
		{
			instance = this;

			resources = SCANcontroller.setLoadedResourceList();

			GameEvents.onVesselChange.Add(vesselChange);
			GameEvents.onVesselWasModified.Add(vesselChange);

			initializeMap();
		}

		private void vesselChange(Vessel V)
		{
			vessel = V;
		}

		private void initializeMap()
		{
			if (HighLogic.LoadedSceneIsFlight)
				vessel = FlightGlobals.ActiveVessel;

			if (body == null)
			{
				for (int i = FlightGlobals.Bodies.Count - 1; i >= 0; i--)
				{
					CelestialBody b = FlightGlobals.Bodies[i];

					if (b.bodyName != SCANcontroller.controller.bigMapBody)
						continue;

					body = b;
					break;
				}

				if (body == null)
				{
					if (vessel == null)
						body = FlightGlobals.Bodies[1];
					else
						body = vessel.mainBody;
				}
			}

			data = SCANUtil.getData(body);

			if (data == null)
			{
				data = new SCANdata(body);
				SCANcontroller.controller.addToBodyData(body, data);
			}

			if (bigmap == null)
			{				
				bigmap = new SCANmap(body, true, mapSource.BigMap);

				MapProjection p = MapProjection.Rectangular;
				mapType t = mapType.Altimetry;

				try
				{
					p = (MapProjection)Enum.Parse(typeof(MapProjection), SCANcontroller.controller.bigMapProjection, true);
					t = (mapType)Enum.Parse(typeof(mapType), SCANcontroller.controller.bigMapType, true);
				}
				catch (Exception e)
				{
					SCANUtil.SCANlog("Error in parsing map projection and/or type\n{0}", e);
					
					p = MapProjection.Rectangular;
					t = mapType.Altimetry;
				}

				bigmap.Projection = p;
				bigmap.MType = t;
				bigmap.ColorMap = SCANcontroller.controller.bigMapColor;

				if (SCAN_Settings_Config.Instance.BigMapWidth % 2 != 0)
					SCAN_Settings_Config.Instance.BigMapWidth += 1;

				bigmap.setWidth(SCAN_Settings_Config.Instance.BigMapWidth);
			}
		}

		public void OnDestroy()
		{
			SCANcontroller.controller.unloadPQS(bigmap.Body, mapSource.BigMap);

			if (uiElement != null)
			{
				uiElement.gameObject.SetActive(false);
				MonoBehaviour.Destroy(uiElement.gameObject);
			}
		}

		public void SetScale(float scale)
		{
			if (uiElement != null)
				uiElement.SetScale(scale);
		}

		public void Update()
		{
			if (!_isVisible || data == null || bigmap == null)
				return;
			
			if (uiElement == null)
				return;

			if (!bigmap.isMapComplete())
				bigmap.getPartialMap();

			if (updateMap)
			{
				updateMap = false;
				uiElement.UpdateMapTexture(bigmap.Map);
			}

			if (OrbitToggle && ShowOrbit)
			{
				UpdateOrbitIcons();
			}
		}

		private void UpdateOrbitIcons()
		{
			if (vessel == null || vessel.mainBody != body || vessel.LandedOrSplashed)
				return;

			Orbit o = vessel.orbit;

			double startUT = Planetarium.GetUniversalTime();
			double UT = startUT;
			Color col;

			for (int i = 0; i < orbitLabels.Count; i++)
			{
				SimpleLabelInfo info = orbitLabels[i];

				if (info == null)
					continue;

				int k = i - steps;

				if (k < 0)
					UT = startUT - (steps + k) * (o.period / steps);
				else
					UT = startUT + k * o.period * (1f / steps);

				if (double.IsNaN(UT))
				{
					info.show = false;
					continue;
				}

				if (UT < o.StartUT && o.StartUT != startUT)
				{
					info.show = false;
					continue;
				}

				if (UT > o.EndUT)
				{
					info.show = false;
					continue;
				}

				if (double.IsNaN(o.getObtAtUT(UT)))
				{
					info.show = false;
					continue;
				}

				Vector3d pos = o.getPositionAtUT(UT);

				double rotation = 0;

				if (body.rotates)
					rotation = (360 * ((UT - startUT) / body.rotationPeriod)) % 360;

				double alt = body.GetAltitude(pos);

				if (alt < 0)
				{
					if (k < 0)
					{
						for (int j = k; j < 0; j++)
						{
							orbitLabels[j + steps].show = false;
						}

						i = steps;
						continue;
					}

					for (int j = k; j < steps; j++)
					{
						orbitLabels[j + steps].show = false;
					}

					break;
				}

				double lo = body.GetLongitude(pos) - rotation;
				double la = body.GetLatitude(pos);

				double lon = (bigmap.projectLongitude(lo, la) + 180) % 360;
				double lat = (bigmap.projectLatitude(lo, la) + 90) % 180;

				lon = bigmap.scaleLongitude(lon);
				lat = bigmap.scaleLatitude(lat);

				if (lat < 0 || lon < 0 || lat > 180 || lon > 360)
				{
					info.show = false;
					continue;
				}

				lon = lon * bigmap.MapWidth / 360;
				lat = lat * bigmap.MapHeight / 180;

				if (k < 0)
					col = palette.cb_orange;
				else
				{
					if (body.atmosphere && body.atmosphereDepth >= alt)
						col = palette.cb_reddishPurple;
					else
						col = palette.cb_skyBlue;
				}

				info.show = true;
				info.color = col;
				info.pos = new Vector2((float)lon, (float)lat);
			}
		}

		public void OnGUI()
		{
			if (GridToggle && Event.current.type == EventType.Repaint)
			{
				if (gridLines.Count > 0)
				{
					Matrix4x4 previousGuiMatrix = GUI.matrix;
					GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Scale * GameSettings.UI_SCALE, Scale * GameSettings.UI_SCALE, 1));

					GL.PushMatrix();
					for (int i = gridLines[0].Count - 1; i >= 0; i--)
					{
						List<Vector2d> points = gridLines[0][i];
						SCANuiUtil.drawGridLines(points, bigmap.MapWidth, _mapScreenPosition.x, _mapScreenPosition.y, SCANuiUtil.blackLineColor);
					}
					for (int i = gridLines[1].Count - 1; i >= 0; i--)
					{
						List<Vector2d> points = gridLines[1][i];
						SCANuiUtil.drawGridLines(points, bigmap.MapWidth, _mapScreenPosition.x, _mapScreenPosition.y, SCANuiUtil.lineColor);
					}
					GL.PopMatrix();

					GUI.matrix = previousGuiMatrix;
				}
			}
		}

		private void SetGridLines()
		{
			if (!GridToggle)
				return;

			gridLines = new Dictionary<int, List<List<Vector2d>>>();
			gridLines = SCANuiUtil.drawGridLine(bigmap);
		}

		public void Open()
		{
			uiElement = GameObject.Instantiate(SCAN_UI_Loader.BigMapPrefab).GetComponent<SCAN_BigMap>();

			if (uiElement == null)
				return;

			uiElement.transform.SetParent(UIMasterController.Instance.mainCanvas.transform, false);

			if (OrbitToggle && ShowOrbit)
			{
				orbitLabels = new List<SimpleLabelInfo>();

				for (int i = 0; i < steps * 2; i++)
				{
					orbitLabels.Add(new SimpleLabelInfo(10, SCAN_UI_Loader.PlanetIcon));
				}

				UpdateOrbitIcons();
			}

			uiElement.setMap(this);

			SetGridLines();

			updateMap = true;

			_isVisible = true;
		}

		public void Close()
		{
			_isVisible = false;

			if (uiElement == null)
				return;

			uiElement.gameObject.SetActive(false);
			MonoBehaviour.Destroy(uiElement.gameObject);
		}

		public string Version
		{
			get { return SCANmainMenuLoader.SCANsatVersion; }
		}

		public string CurrentProjection
		{
			get { return SCANcontroller.controller.bigMapProjection; }
			set
			{
				MapProjection p;

				try
				{
					p = (MapProjection)Enum.Parse(typeof(MapProjection), value, true);

					SCANcontroller.controller.bigMapProjection = value;
					bigmap.Projection = p;

					SetGridLines();

					bigmap.resetMap(SCANcontroller.controller.bigMapResourceOn);
				}
				catch (Exception e)
				{
					SCANUtil.SCANlog("Error in parsing map projection type\n{0}", e);
				}
			}
		}

		public string CurrentMapType
		{
			get { return SCANcontroller.controller.bigMapType; }
			set
			{
				mapType t;

				try
				{
					t = (mapType)Enum.Parse(typeof(mapType), value, true);

					SCANcontroller.controller.bigMapType = value;
					bigmap.MType = t;
					bigmap.resetMap(SCANcontroller.controller.bigMapResourceOn);
				}
				catch (Exception e)
				{
					SCANUtil.SCANlog("Error in parsing map type\n{0}", e);
				}
			}
		}

		public string CurrentResource
		{
			get { return SCANcontroller.controller.bigMapResource; }
			set
			{
				SCANcontroller.controller.bigMapResource = value;
				SCANcontroller.controller.bigMapResourceOn = true;

				if (currentResource.Name != value)
				{
					for (int i = resources.Count - 1; i >= 0; i--)
					{
						SCANresourceGlobal r = resources[i];
						
						if (r.Name != value)
							continue;

						currentResource = r;
						break;
					}
				}

				if (currentResource == null)
					currentResource = SCANcontroller.GetFirstResource;

				currentResource.CurrentBodyConfig(body.name);

				bigmap.Resource = currentResource;
				bigmap.resetMap(SCANcontroller.controller.bigMapResourceOn);
			}
		}

		public string CurrentCelestialBody
		{
			get { return SCANcontroller.controller.bigMapBody; }
			set
			{
				SCANdata bodyData = SCANUtil.getData(value);

				if (bodyData != null)
				{
					data = bodyData;
					body = data.Body;
					bigmap.setBody(body);
					bigmap.resetMap(SCANcontroller.controller.bigMapResourceOn);
					SCANcontroller.controller.bigMapBody = value;
				}
			}
		}

		public bool IsVisible
		{
			get { return _isVisible; }
			set
			{
				_isVisible = value;

				if (!value)
					Close();
			}
		}

		public bool ColorToggle
		{
			get { return SCANcontroller.controller.bigMapColor; }
			set
			{
				SCANcontroller.controller.bigMapColor = value;

				bigmap.ColorMap = value;
				bigmap.resetMap(SCANcontroller.controller.bigMapResourceOn);
			}
		}

		public bool GridToggle
		{
			get { return SCANcontroller.controller.bigMapGrid; }
			set
			{
				SCANcontroller.controller.bigMapGrid = value;

				if (value)
					SetGridLines();
			}
		}

		public bool OrbitToggle
		{
			get { return SCANcontroller.controller.bigMapOrbit; }
			set
			{
				SCANcontroller.controller.bigMapOrbit = value;

				if (value && ShowOrbit)
				{
					orbitLabels = new List<SimpleLabelInfo>();
					
					for (int i = 0; i < steps * 2; i++)
					{
						orbitLabels.Add(new SimpleLabelInfo(10, SCAN_UI_Loader.PlanetIcon));
					}

					UpdateOrbitIcons();
				}
			}
		}

		public bool WaypointToggle
		{
			get { return SCANcontroller.controller.bigMapWaypoint; }
			set
			{
				SCANcontroller.controller.bigMapWaypoint = value;

			}
		}

		public bool AnomalyToggle
		{
			get { return SCANcontroller.controller.bigMapAnomaly; }
			set
			{
				SCANcontroller.controller.bigMapAnomaly = value;

			}
		}

		public bool FlagToggle
		{
			get { return SCANcontroller.controller.bigMapFlag; }
			set
			{
				SCANcontroller.controller.bigMapFlag = value;

			}
		}

		public bool AsteroidToggle
		{
			get { return SCANcontroller.controller.bigMapAsteroid; }
			set
			{
				SCANcontroller.controller.bigMapAsteroid = value;

			}
		}

		public bool LegendToggle
		{
			get { return SCANcontroller.controller.bigMapLegend; }
			set { SCANcontroller.controller.bigMapLegend = value;}
		}

		public bool ResourceToggle
		{
			get { return SCANcontroller.controller.bigMapResourceOn; }
			set
			{
				SCANcontroller.controller.bigMapResourceOn = value;

				bigmap.resetMap(SCANcontroller.controller.bigMapResourceOn);
			}
		}

		public bool ShowOrbit
		{
			get
			{
				return HighLogic.LoadedSceneIsFlight
				&& vessel != null
				&& body != null
				&& vessel.mainBody == body;
			}
		}

		public bool ShowWaypoint
		{
			get { return HighLogic.LoadedScene != GameScenes.SPACECENTER; }
		}

		public bool ShowResource
		{
			get { return SCANcontroller.MasterResourceCount > 1; }
		}

		public int OrbitSteps
		{
			get { return steps * 2; }
		}

		public float Scale
		{
			get { return SCAN_Settings_Config.Instance.UIScale; }
		}

		public Canvas MainCanvas
		{
			get { return UIMasterController.Instance.mainCanvas; }
		}

		public Vector2 Position
		{
			get { return SCAN_Settings_Config.Instance.BigMapPosition; }
			set { SCAN_Settings_Config.Instance.BigMapPosition = value; }
		}

		public Vector2 Size
		{
			get
			{
				float width = SCAN_Settings_Config.Instance.BigMapWidth;
				float height = width / 2;

				return new Vector2(width, height);
			}
			set
			{
				SCAN_Settings_Config.Instance.BigMapWidth = (int)value.x;

				bigmap.setWidth(SCAN_Settings_Config.Instance.BigMapWidth);

				SetGridLines();

				updateMap = true;
			}
		}

		public Texture2D LegendImage
		{
			get
			{
				if (bigmap.MapLegend == null)
					bigmap.MapLegend = new SCANmapLegend();

				if (data == null)
					return null;

				return bigmap.MapLegend.getLegend(data.TerrainConfig.MinTerrain, data.TerrainConfig.MaxTerrain, SCANcontroller.controller.bigMapColor, data.TerrainConfig);
			}
		}

		public Vector2 MapScreenPosition
		{
			get { return _mapScreenPosition; }
			set { _mapScreenPosition = value; }
		}

		public IList<string> Projections
		{
			get { return new List<string>(3) { "Rectangular", "KavrayskiyVII", "Polar" }; }
		}

		public IList<string> MapTypes
		{
			get { return new List<string>(3) { "Altimetry", "Slope", "Biome" }; }
		}

		public IList<string> Resources
		{
			get { return new List<string>(resources.Select(r => r.Name)); }
		}

		public IList<string> CelestialBodies
		{
			get { return new List<string>(SCANcontroller.controller.GetAllData.Select(d => d.Body.bodyName)); }
		}

		public IList<string> LegendLabels
		{
			get
			{
				if (data == null)
					return null;

				string one = string.Format("|\n{0:N0}", (((int)(data.TerrainConfig.MinTerrain / 100)) * 100));

				string two = string.Format("|\n{0:N0}", (((int)((data.TerrainConfig.MinTerrain + (data.TerrainConfig.TerrainRange / 2)) / 100)) * 100));

				string three = string.Format("|\n{0:N0}", (((int)(data.TerrainConfig.MaxTerrain / 100)) * 100));

				return new List<string>(3) { one, two, three };
			}
		}

		public SimpleLabelInfo OrbitInfo(int index)
		{
			if (index < 0 || index >= orbitLabels.Count)
				return null;

			return orbitLabels[index];
		}

		public Vector2 VesselPosition()
		{
			if (vessel == null)
				return new Vector2();

			return VesselPosition(vessel);
		}

		public Vector2 VesselPosition(Guid id)
		{
			Vessel v = null;

			for (int i = FlightGlobals.Vessels.Count - 1; i >= 0; i--)
			{
				v = FlightGlobals.Vessels[i];

				if (v.id == id)
					break;
			}

			return VesselPosition(v);
		}

		public Vector2 VesselPosition(Vessel v)
		{
			if (v == null)
				return new Vector2();

			double lat = SCANUtil.fixLat(bigmap.projectLatitude(v.longitude, v.latitude));
			double lon = SCANUtil.fixLon(bigmap.projectLongitude(v.longitude, v.latitude));
			lat = bigmap.scaleLatitude(lat);
			lon = bigmap.scaleLongitude(lon);

			lon = lon * bigmap.MapWidth / 360;
			lat = lat * bigmap.MapHeight / 180;

			return new Vector2((float)lon, (float)lat);
		}

		public Vector2 MapPosition(double lat, double lon)
		{
			double Lat = SCANUtil.fixLat(bigmap.projectLatitude(lon, lat));
			double Lon = SCANUtil.fixLon(bigmap.projectLongitude(lon, lat));
			Lat = bigmap.scaleLatitude(Lat);
			Lon = bigmap.scaleLongitude(Lon);

			Lon = Lon * bigmap.MapWidth / 360;
			Lat = Lat * bigmap.MapHeight / 180;

			return new Vector2((float)Lon, (float)Lat);
		}

		public Dictionary<Guid, MapLabelInfo> FlagInfoList
		{
			get
			{
				Dictionary<Guid, MapLabelInfo> vessels = new Dictionary<Guid, MapLabelInfo>();

				for (int i = FlightGlobals.Vessels.Count - 1; i >= 0; i--)
				{
					Vessel v = FlightGlobals.Vessels[i];

					if (v == null)
						continue;

					if (v.vesselType != VesselType.Flag)
						continue;

					if (v.mainBody != body)
						continue;

					vessels.Add(v.id, new MapLabelInfo()
					{
						label = "",
						image = SCAN_UI_Loader.FlagIcon,
						pos = VesselPosition(v.id),
						baseColor = ColorToggle ? palette.cb_yellow : palette.cb_skyBlue,
						flash = false,
						width = 32
					});
				}

				return vessels;
			}
		}

		public Dictionary<string, MapLabelInfo> AnomalyInfoList
		{
			get
			{
				Dictionary<string, MapLabelInfo> anomalies = new Dictionary<string, MapLabelInfo>();

				if (data != null)
				{
					for (int i = data.Anomalies.Length - 1; i >= 0; i--)
					{
						SCANanomaly a = data.Anomalies[i];

						if (a == null)
							continue;

						if (!a.Known)
							continue;

						anomalies.Add(a.Name, new MapLabelInfo()
							{
								label = a.Detail ? a.Name : "",
								image = SCAN_UI_Loader.AnomalyIcon,
								pos = MapPosition(a.Latitude, a.Longitude),
								baseColor = ColorToggle ? palette.cb_yellow : palette.cb_skyBlue,
								flash = false,
								width = 20
							});
					}
				}

				return anomalies;
			}
		}

		public Dictionary<int, MapLabelInfo> WaypointInfoList
		{
			get
			{
				Dictionary<int, MapLabelInfo> waypoints = new Dictionary<int, MapLabelInfo>();

				if (data != null)
				{
					for (int i = data.Waypoints.Count - 1; i >= 0; i--)
					{
						SCANwaypoint w = data.Waypoints[i];

						if (w == null)
							continue;

						waypoints.Add(w.Seed, new MapLabelInfo()
						{
							label = "",
							image = SCAN_UI_Loader.WaypointIcon,
							pos = MapPosition(w.Latitude, w.Longitude),
							baseColor = palette.white,
							flash = false,
							width = 20,
							alignBottom = true
						});
					}
				}

				return waypoints;
			}
		}

		public KeyValuePair<Guid, MapLabelInfo> VesselInfo
		{
			get
			{
				if (vessel == null || vessel.mainBody != body)
					return new KeyValuePair<Guid,MapLabelInfo>(new Guid(), new MapLabelInfo() { label = "null" });

				return new KeyValuePair<Guid,MapLabelInfo>(vessel.id, new MapLabelInfo()
				{
					label = "",
					image = SCAN_UI_Loader.VesselIcon(vessel.vesselType),
					pos = VesselPosition(vessel),
					baseColor = ColorToggle ? palette.white : palette.cb_skyBlue,
					flashColor = palette.cb_yellow,
					flash = true,
					width = 22
				});
			}
		}

		public string MapInfo(Vector2 mapPos)
		{
			float mx = mapPos.x;
			float my = mapPos.y * -1f;

			double mlo = (mx * 360 / bigmap.MapWidth) - 180;
			double mla = 90 - (my * 180 / bigmap.MapHeight);

			double mlon = bigmap.unprojectLongitude(mlo, mla);
			double mlat = bigmap.unprojectLatitude(mlo, mla);

			if (mlon >= -180 && mlon <= 180 && mlat >= -90 && mlat <= 90)
				return mouseOverInfo(mlon, mlat);
			else
				return "";
		}

		private string mouseOverInfo(double lon, double lat)
		{
			infoString = StringBuilderCache.Acquire();

			bool altimetry = SCANUtil.isCovered(lon, lat, data, SCANtype.Altimetry);
			bool hires = SCANUtil.isCovered(lon, lat, data, SCANtype.AltimetryHiRes);

			if (SCANUtil.isCovered(lon, lat, data, SCANtype.AltimetryLoRes))
			{
				if (body.pqsController == null)
					infoString.Append(palette.coloredNoQuote(palette.c_bad, "LO "));
				else
					infoString.Append(palette.coloredNoQuote(palette.c_good, "LO "));
			}
			else
				infoString.Append(palette.coloredNoQuote(palette.grey, "LO "));

			if (hires)
			{
				if (body.pqsController == null)
					infoString.Append(palette.coloredNoQuote(palette.c_bad, "HI "));
				else
					infoString.Append(palette.coloredNoQuote(palette.c_good, "HI "));
			}
			else
				infoString.Append(palette.coloredNoQuote(palette.grey, "HI "));

			if (SCANUtil.isCovered(lon, lat, data, SCANtype.Biome))
			{
				if (body.BiomeMap == null)
					infoString.Append(palette.coloredNoQuote(palette.c_bad, "MULTI "));
				else
					infoString.Append(palette.coloredNoQuote(palette.c_good, "MULTI "));
			}
			else
				infoString.Append(palette.coloredNoQuote(palette.grey, "MULTI "));

			if (altimetry)
			{
				infoString.Append("Terrain Height: ");
				infoString.Append(SCANuiUtil.getMouseOverElevation(lon, lat, data, 2, hires));

				if (hires)
				{
					double circum = body.Radius * 2 * Math.PI;
					double eqDistancePerDegree = circum / 360;
					double degreeOffset = 5 / eqDistancePerDegree;

					infoString.Append(" Slope: ");
					infoString.AppendFormat("{0:F1}°", SCANUtil.slope(SCANUtil.getElevation(body, lon, lat), body, lon, lat, degreeOffset));
				}
			}

			if (SCANUtil.isCovered(lon, lat, data, SCANtype.Biome))
			{
				infoString.Append(" Biome: ");
				infoString.Append(SCANUtil.getBiomeName(body, lon, lat));
			}

			if (bigmap.ResourceActive && SCANconfigLoader.GlobalResource && bigmap.Resource != null)
			{
				bool resources = false;
				bool fuzzy = false;

				if (SCANUtil.isCovered(lon, lat, data, bigmap.Resource.SType))
				{
					resources = true;
				}
				else if (SCANUtil.isCovered(lon, lat, data, SCANtype.FuzzyResources))
				{
					resources = true;
					fuzzy = true;
				}

				if (resources)
				{
					infoString.Append(string.Format(" {0}: ", bigmap.Resource.Name));
					infoString.Append(palette.coloredNoQuote(bigmap.Resource.MaxColor, SCANuiUtil.getResourceAbundance(bigmap.Body, lat, lon, fuzzy, bigmap.Resource)));
				}
			}
			
			infoString.AppendLine();
			infoString.AppendFormat("{0} (lat: {1:F2}° lon: {2:F2}°)", SCANuiUtil.toDMS(lat, lon), lat, lon);

			if (SCANcontroller.controller.bigMapWaypoint)
			{
				double range = ContractDefs.Survey.MaximumTriggerRange;
				foreach (SCANwaypoint p in data.Waypoints)
				{
					if (!p.LandingTarget)
					{
						if (p.Root != null)
						{
							if (p.Root.ContractState != Contracts.Contract.State.Active)
								continue;
						}
						if (p.Param != null)
						{
							if (p.Param.State != Contracts.ParameterState.Incomplete)
								continue;
						}

						if (SCANUtil.waypointDistance(lat, lon, 1000, p.Latitude, p.Longitude, 1000, body) <= range)
						{
							infoString.Append(" Waypoint: ");
							infoString.Append(p.Name);
							break;
						}
					}
				}
			}

			return infoString.ToStringAndRelease();
		}

		public void RefreshMap()
		{
			SCANcontroller.controller.TargetSelecting = false;
			SCANcontroller.controller.TargetSelectingActive = false;
			bigmap.resetMap(SCANcontroller.controller.bigMapResourceOn);

			SetGridLines();
		}

		public void OpenMainMap()
		{
			if (SCAN_UI_MainMap.Instance.IsVisible)
				SCAN_UI_MainMap.Instance.Close();
			else
				SCAN_UI_MainMap.Instance.Open();
		}

		public void OpenZoomMap()
		{
			SCANcontroller.controller.zoomMap.Visible = !SCANcontroller.controller.zoomMap.Visible;
			if (SCANcontroller.controller.zoomMap.Visible && !SCANcontroller.controller.zoomMap.Initialized)
				SCANcontroller.controller.zoomMap.initializeMap();
		}

		public void OpenOverlay()
		{
			if (SCAN_UI_Overlay.Instance.IsVisible)
				SCAN_UI_Overlay.Instance.Close();
			else
				SCAN_UI_Overlay.Instance.Open();
		}

		public void OpenInstruments()
		{
			if (SCAN_UI_Instruments.Instance.IsVisible)
				SCAN_UI_Instruments.Instance.Close();
			else
				SCAN_UI_Instruments.Instance.Open();
		}

		public void OpenSettings()
		{
			if (SCAN_UI_Settings.Instance.IsVisible)
				SCAN_UI_Settings.Instance.Close();
			else
				SCAN_UI_Settings.Instance.Open();
		}

		public void ExportMap()
		{
			if (bigmap.isMapComplete())
				bigmap.exportPNG();
		}

		public void setMapWidth(int width)
		{
			if (bigmap == null)
				return;

			bigmap.setWidth(width);
			SCAN_Settings_Config.Instance.BigMapWidth = bigmap.MapWidth;
		}

		public void ResetPosition()
		{
			SCAN_Settings_Config.Instance.BigMapPosition = new Vector2(400, -400);

			if (uiElement != null)
				uiElement.SetPosition(SCAN_Settings_Config.Instance.BigMapPosition);
		}

	}
}
