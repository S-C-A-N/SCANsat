using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SCANsat.Unity.Interfaces;
using SCANsat.Unity.Unity;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Map;
using SCANsat.SCAN_UI.UI_Framework;
using KSP.UI;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat.SCAN_Unity
{
	public class SCAN_UI_BigMap : ISCAN_BigMap
	{
		private bool _isVisible;

		private static SCANmap bigmap;
		private static CelestialBody b;
		private SCANdata data;
		private Vessel v;
		private StringBuilder infoString;
		private StringBuilder infoString2;
		private Dictionary<int, List<List<Vector2d>>> gridLines = new Dictionary<int, List<List<Vector2d>>>();
		
		private SCANresourceGlobal currentResource;
		private List<SCANresourceGlobal> resources;

		private SCAN_BigMap uiElement;

		private static SCAN_UI_BigMap instance;

		public static SCAN_UI_BigMap Instance
		{
			get { return instance; }
		}

		public SCAN_UI_BigMap()
		{
			instance = this;

			resources = SCANcontroller.setLoadedResourceList();

			initializeMap();
		}

		private void initializeMap()
		{
			v = FlightGlobals.ActiveVessel;

			if (b == null)
			{
				if (v == null)
					b = FlightGlobals.Bodies[1];
				else
					b = v.mainBody;
			}

			if (bigmap == null)
			{
				bigmap = new SCANmap(b, true, mapSource.BigMap);
				bigmap.setProjection((MapProjection)SCANcontroller.controller.projection);
				if (SCANcontroller.controller.map_width % 2 != 0)
					SCANcontroller.controller.map_width += 1;
				bigmap.setWidth(SCANcontroller.controller.map_width);
			}

			data = SCANUtil.getData(b);

			if (data == null)
			{
				data = new SCANdata(b);
				SCANcontroller.controller.addToBodyData(b, data);
			}

			bigmap.setBody(b);
		}

		public void OnDestroy()
		{
			SCANcontroller.controller.unloadPQS(bigmap.Body, mapSource.BigMap);
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
				uiElement.UpdateMapTexture(bigmap.getPartialMap());
			
		}

		public void OnGUI()
		{
			if (SCANcontroller.controller.bigMapGrid)
			{
				if (gridLines.Count > 0)
				{
					GL.PushMatrix();
					for (int i = gridLines[0].Count - 1; i >= 0; i--)
					{
						List<Vector2d> points = gridLines[0][i];
						SCANuiUtil.drawGridLines(points, bigmap.MapWidth, 0, 0, SCANuiUtil.blackLineColor);
					}
					for (int i = gridLines[1].Count - 1; i >= 0; i--)
					{
						List<Vector2d> points = gridLines[1][i];
						SCANuiUtil.drawGridLines(points, bigmap.MapWidth, 0, 0, SCANuiUtil.lineColor);
					}
					GL.PopMatrix();
				}
			}
		}

		public void Open()
		{
			uiElement = GameObject.Instantiate(SCAN_UI_Loader.MainMapPrefab).GetComponent<SCAN_BigMap>();

			if (uiElement == null)
				return;

			uiElement.transform.SetParent(UIMasterController.Instance.mainCanvas.transform, false);

			uiElement.setMap(this);

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

		public string Readout
		{
			get { return ""; }
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
					bigmap.setProjection(p);
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

					bigmap.resetMap(t, true, SCANcontroller.controller.bigMapResourceOn);
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
					b = data.Body;
					bigmap.setBody(b);
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
				{
					gridLines = new Dictionary<int, List<List<Vector2d>>>();
					gridLines = SCANuiUtil.drawGridLine(bigmap);
				}
			}
		}

		public bool OrbitToggle
		{
			get { return SCANcontroller.controller.bigMapOrbit; }
			set
			{
				SCANcontroller.controller.bigMapOrbit = value;

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
			get;
			set;
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
			get { return HighLogic.LoadedSceneIsFlight; }
		}

		public bool ShowWaypoint
		{
			get { return HighLogic.LoadedScene != GameScenes.SPACECENTER; }
		}

		public float Scale
		{
			get { return SCAN_Settings_Config.Instance.UIScale; }
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
			set { SCAN_Settings_Config.Instance.BigMapWidth = (int)value.x; }
		}

		public IList<string> Projections
		{
			get { return new List<string>(3) { "Rectangular", "KavrayskiyVII", "Polar" }; }
		}

		public IList<string> MapTypes
		{
			get { return new List<string>(3) { "Terrain", "Slope", "Biome" }; }
		}

		public IList<string> Resources
		{
			get { return new List<string>(resources.Select(r => r.Name)); }
		}

		public IList<string> CelestialBodies
		{
			get { return new List<string>(FlightGlobals.Bodies.Select(b => b.bodyName)); }
		}

		public void RefreshMap()
		{
			SCANcontroller.controller.TargetSelecting = false;
			SCANcontroller.controller.TargetSelectingActive = false;
			bigmap.resetMap(SCANcontroller.controller.bigMapResourceOn);
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
			SCANcontroller.controller.map_width = bigmap.MapWidth;
		}

	}
}
