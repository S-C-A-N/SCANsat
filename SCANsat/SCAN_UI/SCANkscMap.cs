#region license
/* 
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - KSC map window object
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 *
 */
#endregion

using System.Collections.Generic;
using SCANsat.SCAN_Platform;
using SCANsat;
using SCANsat.SCAN_UI.UI_Framework;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Map;
using UnityEngine;

namespace SCANsat.SCAN_UI
{
	class SCANkscMap : SCAN_MBW
	{
		private static SCANmap bigmap;
		private static CelestialBody b;
		private Vessel v;
		private string mapTypeTitle = "";
		private SCANdata data;
		private bool drawGrid, currentGrid, currentColor, lastColor, lastResource, controlLock, waypoints;
		private bool drop_down_open, projection_drop_down, mapType_drop_down, resources_drop_down, planetoid_drop_down;
		//private Texture2D overlay_static;
		private List<SCANresourceGlobal> loadedResources = new List<SCANresourceGlobal>();
		private Dictionary<int, List<List<Vector2d>>> gridLines = new Dictionary<int, List<List<Vector2d>>>();
		private Rect ddRect, zoomCloseRect;
		private Rect rc = new Rect(0, 0, 20, 20);
		private Vector2 scrollP, scrollR;
		private Rect pos_spotmap = new Rect(10f, 10f, 10f, 10f);
		private Rect pos_spotmap_x = new Rect(10f, 10f, 25f, 25f);
		internal readonly static Rect defaultRect = new Rect(250, 60, 780, 460);
		private static Rect sessionRect = defaultRect;
		private const string lockID = "SCANksc_LOCK";

		internal SCANzoomWindow spotMap;

		protected override void Awake()
		{
			WindowCaption = "Map of ";
			WindowRect = sessionRect;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(740), GUILayout.Height(420) };
			WindowStyle = SCANskins.SCAN_window;
			Visible = false;
			DragEnabled = true;
			TooltipMouseOffset = new Vector2d(-10, -25);
			ClampToScreenOffset = new RectOffset(-600, -600, -400, -400);
			waypoints = HighLogic.LoadedScene != GameScenes.SPACECENTER;

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
			SCAN_SkinsLibrary.SetCurrentTooltip();

			removeControlLocks();
		}

		protected override void Start()
		{
			Visible = SCANcontroller.controller.kscMapVisible;
			if (b == null)
				b = Planetarium.fetch.Home;
			if (bigmap == null)
			{
				bigmap = new SCANmap(b, true);
				bigmap.setProjection((MapProjection)SCANcontroller.controller.projection);
				bigmap.setWidth(720);
			}
			currentColor = SCANcontroller.controller.colours == 0;
			lastColor = currentColor;
			lastResource = SCANcontroller.controller.map_ResourceOverlay;
			WindowCaption = string.Format("Map of {0}", b.theName);
			data = SCANUtil.getData(b);
			if (data == null)
			{
				data = new SCANdata(b);
				SCANcontroller.controller.addToBodyData(b, data);
			}
			bigmap.setBody(b);
			if (SCANconfigLoader.GlobalResource)
			{
				loadedResources = SCANcontroller.setLoadedResourceList();
			}
			TooltipsEnabled = SCANcontroller.controller.toolTips;
		}

		protected override void OnDestroy()
		{
			removeControlLocks();
			if (spotMap != null)
				Destroy(spotMap);
		}

		protected override void Update()
		{
			if (HighLogic.LoadedScene != GameScenes.TRACKSTATION)
			{
				v = null;
				return;
			}
			
			MapObject target = PlanetariumCamera.fetch.target;

			if (target.type != MapObject.MapObjectType.VESSEL)
			{
				v = null;
				return;
			}

			v = target.vessel;
		}

		internal void removeControlLocks()
		{
			InputLockManager.RemoveControlLock(lockID);
			controlLock = false;
		}

		//These properties are used by the color selection window to sync color palettes
		public SCANdata Data
		{
			get { return data; }
		}

		public static SCANmap BigMap
		{
			get { return bigmap; }
		}

		public CelestialBody Body
		{
			get { return b; }
		}

		protected override void DrawWindowPre(int id)
		{
			if (bigmap != null)
				mapTypeTitle = SCANmapType.mapTypeNames[(int)bigmap.MType];
			else
				mapTypeTitle = "";

			WindowCaption = string.Format("{0} Map of {1}", mapTypeTitle, bigmap.Body.theName);

			//Disable any errant drop down menus
			if (!drop_down_open)
			{
				projection_drop_down = false;
				mapType_drop_down = false;
				resources_drop_down = false;
				planetoid_drop_down = false;
			}

			//Lock space center click through
			if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && !controlLock)
				{
					InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS | ControlTypes.KSC_ALL, lockID);
					controlLock = true;
				}
				else if (!WindowRect.Contains(mousePos) && controlLock)
				{
					removeControlLocks();
				}
			}

			//Lock tracking scene click through
			if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && !controlLock)
				{
					InputLockManager.SetControlLock(ControlTypes.TRACKINGSTATION_UI, lockID);
					controlLock = true;
				}
				else if (!WindowRect.Contains(mousePos) && controlLock)
				{
					removeControlLocks();
				}
			}
		}

		//The primary GUI method
		protected override void DrawWindow(int id)
		{
			versionLabel(id);		/* Standard version label and close button */
			closeBox(id);

			growS();
				topMenu(id);		/* Top row of buttons - used to control the map types */
				growE();
					toggleBar(id);	/* Toggle options along left side - control overlay options - *Replace buttons with textures* */
					fillS(60);
					mapDraw(id);	/* Draw the main map texture */
				stopE();
				growE();
					fillS(160);
					growS();
						mouseOver(id);		/* Handle all mouse-over info and zoom-map code */
						legendBar(id);		/* Draw the mouseover info and legend bar along the bottom */
					stopS();
				stopE();
			stopS();

			drawOrbit(id);			/* Draw the vessel's orbit if in the tracking station */
			mapLabels(id);			/* Draw the vessel/anomaly icons on the map */
			if (drop_down_open)
				dropDown(id);		/* Draw the drop down menus if any are open */
		}

		protected override void DrawWindowPost(int id)
		{
			//Close the drop down menu if the window is clicked anywhere else
			if (drop_down_open && Event.current.type == EventType.mouseDown && !ddRect.Contains(Event.current.mousePosition))
				drop_down_open = false;

			//Used to update the black and white/color status
			if (lastColor != currentColor)
			{
				lastColor = currentColor;
				if (SCANcontroller.controller.colours == 0)
					SCANcontroller.controller.colours = 1;
				else
					SCANcontroller.controller.colours = 0;
				data.resetImages();
				bigmap.resetMap();
			}

			//Updates the grid overlay status
			if (currentGrid != SCANcontroller.controller.map_grid)
			{
				currentGrid = SCANcontroller.controller.map_grid;
				drawGrid = true;
			}

			//Update selected resource status
			if (lastResource != SCANcontroller.controller.map_ResourceOverlay)
			{
				lastResource = SCANcontroller.controller.map_ResourceOverlay;
				bigmap.resetMap();
			}

			sessionRect = WindowRect;
		}

		//Draw version label in upper left corner
		private void versionLabel(int id)
		{
			Rect r = new Rect(6, 0, 50, 18);
			GUI.Label(r, SCANmainMenuLoader.SCANsatVersion, SCANskins.SCAN_whiteReadoutLabel);
		}

		//Draw the close button in upper right corner
		private void closeBox(int id)
		{
			Rect r = new Rect(WindowRect.width - 20, 1, 18, 18);
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				removeControlLocks();
				Visible = false;
				SCANcontroller.controller.kscMapVisible = Visible;
			}
		}

		//Draw the drop down buttons along the top of the map - Used to control map types
		private void topMenu (int id)
		{
			growE();

			fillS(100);

			if (GUILayout.Button("Projection", GUILayout.MaxWidth(100)))
			{
				projection_drop_down = !projection_drop_down;
				drop_down_open = !drop_down_open;
			}

			fillS(40);

			if (GUILayout.Button("Map Type", GUILayout.MaxWidth(90)))
			{
				mapType_drop_down = !mapType_drop_down;
				drop_down_open = !drop_down_open;
			}

			fillS();
			if (GUILayout.Button(iconWithTT(SCANskins.SCAN_RefreshIcon, "Refresh Map"), SCANskins.SCAN_buttonBorderless, GUILayout.MaxWidth(34), GUILayout.MaxHeight(28)))
			{
				bigmap.resetMap();
			}
			fillS();

			if (SCANconfigLoader.GlobalResource)
			{
				if (GUILayout.Button("Resources", GUILayout.MaxWidth(90)))
				{
					resources_drop_down = !resources_drop_down;
					drop_down_open = !drop_down_open;
				}
				fillS(40);
			}

			if (GUILayout.Button("Celestial Body", GUILayout.MaxWidth(110)))
			{
				planetoid_drop_down = !planetoid_drop_down;
				drop_down_open = !drop_down_open;
			}

			fillS(20);

			stopE();
		}

		//Draw the overlay options along the left side of the map texture
		private void toggleBar(int id)
		{
			growS();

				currentColor = GUILayout.Toggle(currentColor, textWithTT("", "Toggle Color"));

				Rect d = GUILayoutUtility.GetLastRect();
				d.x += 34;
				d.y += 2;
				d.width = 48;
				d.height = 24;

				if (GUI.Button(d, iconWithTT(SCANskins.SCAN_ColorToggleIcon, "Toggle Color"), SCANskins.SCAN_buttonBorderless))
				{
					currentColor = !currentColor;
				}

				fillS();

				SCANcontroller.controller.map_grid = GUILayout.Toggle(SCANcontroller.controller.map_grid, textWithTT("", "Toggle Grid"));

				d = GUILayoutUtility.GetLastRect();
				d.x += 34;
				d.y += 2;
				d.width = 48;
				d.height = 24;

				if (GUI.Button(d, iconWithTT(SCANskins.SCAN_GridIcon, "Toggle Grid"), SCANskins.SCAN_buttonBorderless))
				{
					SCANcontroller.controller.map_grid = !SCANcontroller.controller.map_grid;
				}

				if (v != null)
				{
					fillS();

					SCANcontroller.controller.map_orbit = GUILayout.Toggle(SCANcontroller.controller.map_orbit, textWithTT("", "Toggle Orbit"));

					d = GUILayoutUtility.GetLastRect();
					d.x += 34;
					d.y += 2;
					d.width = 48;
					d.height = 24;

					if (GUI.Button(d, iconWithTT(SCANskins.SCAN_OrbitIcon, "Toggle Orbit"), SCANskins.SCAN_buttonBorderless))
					{
						SCANcontroller.controller.map_orbit = !SCANcontroller.controller.map_orbit;
					}

					fillS();
				}
				else
					fillS();

				if (waypoints)
				{
					SCANcontroller.controller.map_waypoints = GUILayout.Toggle(SCANcontroller.controller.map_waypoints, textWithTT("", "Toggle Waypoints"));

					d = GUILayoutUtility.GetLastRect();
					d.x += 44;
					d.y += 2;
					d.width = 24;
					d.height = 24;

					if (GUI.Button(d, iconWithTT(SCANskins.SCAN_WaypointIcon, "Toggle Waypoints"), SCANskins.SCAN_buttonBorderless))
					{
						SCANcontroller.controller.map_waypoints = !SCANcontroller.controller.map_waypoints;
					}

					fillS();
				}

				SCANcontroller.controller.map_markers = GUILayout.Toggle(SCANcontroller.controller.map_markers, textWithTT("", "Toggle Anomalies"));

				d = GUILayoutUtility.GetLastRect();
				d.x += 44;
				d.y += 2;
				d.width = 24;
				d.height = 24;

				if (GUI.Button(d, textWithTT(SCANcontroller.controller.anomalyMarker, "Toggle Anomalies"), SCANskins.SCAN_buttonBorderless))
				{
					SCANcontroller.controller.map_markers = !SCANcontroller.controller.map_markers;
				}

				fillS();

				SCANcontroller.controller.map_flags = GUILayout.Toggle(SCANcontroller.controller.map_flags, textWithTT("", "Toggle Flags"));

				d = GUILayoutUtility.GetLastRect();
				d.x += 44;
				d.y += 2;
				d.width = 24;
				d.height = 24;

				if (GUI.Button(d, iconWithTT(SCANskins.SCAN_FlagIcon, "Toggle Flags"), SCANskins.SCAN_buttonBorderless))
				{
					SCANcontroller.controller.map_flags = !SCANcontroller.controller.map_flags;
				}

				fillS();

				SCANcontroller.controller.map_asteroids = GUILayout.Toggle(SCANcontroller.controller.map_asteroids, textWithTT("", "Toggle	Asteroids"));

				d = GUILayoutUtility.GetLastRect();
				d.x += 44;
				d.y += 2;
				d.width = 24;
				d.height = 24;

				if (GUI.Button(d, iconWithTT(SCANskins.SCAN_AsteroidIcon, "Toggle Asteroids"), SCANskins.SCAN_buttonBorderless))
				{
					SCANcontroller.controller.map_asteroids = !SCANcontroller.controller.map_asteroids;
				}

				fillS();

				SCANcontroller.controller.legend = GUILayout.Toggle(SCANcontroller.controller.legend, textWithTT("", "Toggle Legend"));

				d = GUILayoutUtility.GetLastRect();
				d.x += 34;
				d.y += 2;
				d.width = 48;
				d.height = 24;

				if (GUI.Button(d, iconWithTT(SCANskins.SCAN_LegendIcon, "Toggle Legend"), SCANskins.SCAN_buttonBorderless))
				{
					SCANcontroller.controller.legend = !SCANcontroller.controller.legend;
				}

				if (SCANconfigLoader.GlobalResource)
				{
					fillS();

					SCANcontroller.controller.map_ResourceOverlay = GUILayout.Toggle(SCANcontroller.controller.map_ResourceOverlay, textWithTT  ("", "Toggle Resources"));

					d = GUILayoutUtility.GetLastRect();
					d.x += 44;
					d.y += 2;
					d.width = 24;
					d.height = 24;

					if (GUI.Button(d, iconWithTT(SCANskins.SCAN_ResourceIcon, "Toggle Resources"), SCANskins.SCAN_buttonBorderless))
					{
						SCANcontroller.controller.map_ResourceOverlay = !SCANcontroller.controller.map_ResourceOverlay;
					}
				}
			stopS();

			//Open settings windows using icons instead of text; use tooltips
			Rect s = new Rect(10, WindowRect.height - 42, 32, 32);

			if (GUI.Button(s, iconWithTT(SCANskins.SCAN_SettingsIcon, "Settings Menu"), SCANskins.SCAN_windowButton))
			{
				SCANcontroller.controller.settingsWindow.Visible = !SCANcontroller.controller.settingsWindow.Visible;
				SCANcontroller.controller.settingsWindow.removeControlLocks();
			}

			s.x += 40;

			if (GUI.Button(s, iconWithTT(SCANskins.SCAN_ColorIcon, "Color Control"), SCANskins.SCAN_windowButton))
			{
				SCANcontroller.controller.colorManager.Visible = !SCANcontroller.controller.colorManager.Visible;
				SCANcontroller.controller.colorManager.removeControlLocks();
			}

			s.x = WindowRect.width - 66;

			if (GUI.Button(s, iconWithTT(SCANskins.SCAN_ScreenshotIcon, "Export Map"), SCANskins.SCAN_windowButton))
			{
				if (bigmap.isMapComplete())
					bigmap.exportPNG();
			}
		}

		//Draw the actual map texture
		private void mapDraw (int id)
		{
			MapTexture = bigmap.getPartialMap();

			GUILayout.Label("", GUILayout.Width(MapTexture.width), GUILayout.Height(MapTexture.height));

			TextureRect = GUILayoutUtility.GetLastRect();
			TextureRect.width = bigmap.MapWidth;
			TextureRect.height = bigmap.MapHeight;

			if (drawGrid)
			{
				gridLines = new Dictionary<int, List<List<Vector2d>>>();
				gridLines = SCANuiUtil.drawGridLine(TextureRect, bigmap);
				drawGrid = false;
			}

			GUI.DrawTexture(TextureRect, MapTexture);

			if (bigmap.Projection == MapProjection.Polar)
			{
				rc.x = TextureRect.x + TextureRect.width / 2 - TextureRect.width / 8;
				rc.y = TextureRect.y + TextureRect.height / 8;
				SCANuiUtil.drawLabel(rc, "S", false, true, true);
				rc.x = TextureRect.x + TextureRect.width / 2 + TextureRect.width / 8;
				SCANuiUtil.drawLabel(rc, "N", false, true, true);
			}

			if (SCANcontroller.controller.map_grid)
			{
				if (gridLines.Count > 0)
				{
					GL.PushMatrix();
					foreach (List<Vector2d> points in gridLines[0])
					{
						SCANuiUtil.drawGridLines(points, bigmap.MapWidth, TextureRect.x, TextureRect.y, SCANuiUtil.blackLineColor);
					}
					foreach (List<Vector2d> points in gridLines[1])
					{
						SCANuiUtil.drawGridLines(points, bigmap.MapWidth, TextureRect.x, TextureRect.y, SCANuiUtil.lineColor);
					}
					GL.PopMatrix();
				}
			}
		}

		//Display info for mouse over in the map and handle the zoom map
		private void mouseOver(int id)
		{
			float mx = Event.current.mousePosition.x - TextureRect.x;
			float my = Event.current.mousePosition.y - TextureRect.y;
			bool in_map = false;
			double mlon = 0, mlat = 0;

			//Handles mouse positioning and converting to lat/long coordinates
			if (mx >= 0 && my >= 0 && mx < MapTexture.width && my < MapTexture.height)
			{
				double mlo = (mx * 360f / MapTexture.width) - 180;
				double mla = 90 - (my * 180f / MapTexture.height);
				mlon = bigmap.unprojectLongitude(mlo, mla);
				mlat = bigmap.unprojectLatitude(mlo, mla);

				if (mlon >= -180 && mlon <= 180 && mlat >= -90 && mlat <= 90)
				{
					in_map = true;
				}
			}

			//Handles mouse click while inside map; opens zoom map or zooms in further
			if (Event.current.isMouse && !ddRect.Contains(Event.current.mousePosition) && !zoomCloseRect.Contains(Event.current.mousePosition))
			{
				if (Event.current.type == EventType.MouseUp)
				{
					if (Event.current.button == 1)
					{
						if (in_map)
						{
							if (spotMap == null)
							{
								spotMap = gameObject.AddComponent<SCANzoomWindow>();
							}
							spotMap.setMapCenter(mlat, mlon, bigmap);
						}
					}
					Event.current.Use();
				}
			}

			//Draw the actual mouse over info label below the map
			SCANuiUtil.mouseOverInfo(mlon, mlat, bigmap, data, bigmap.Body, in_map);
		}

		//Draw the altitude legend bar along the bottom
		private void legendBar(int id)
		{
			if (bigmap.MType == mapType.Altimetry && SCANcontroller.controller.legend)
			{
				if (bigmap.MapLegend == null)
					bigmap.MapLegend = new SCANmapLegend();
				bigmap.MapLegend.Legend = bigmap.MapLegend.getLegend(data.TerrainConfig.MinTerrain, data.TerrainConfig.MaxTerrain, SCANcontroller.controller.colours, data);
				SCANuiUtil.drawLegend(data, bigmap.MapLegend);
			}
		}

		//Draw the orbit overlays
		private void drawOrbit(int id)
		{
			if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				if (SCANcontroller.controller.map_orbit && v != null)
				{
					SCANuiUtil.drawOrbit(TextureRect, bigmap, v, bigmap.Body);
				}
			}
		}

		//Draw the map overlay labels
		private void mapLabels (int id)
		{
			bool showWaypoints = false;
			if (waypoints)
				showWaypoints = SCANcontroller.controller.map_waypoints;

			SCANuiUtil.drawMapLabels(TextureRect, v, bigmap, data, bigmap.Body, SCANcontroller.controller.map_markers, showWaypoints);
		}

		//Draw the drop down menus if any have been opened
		private void dropDown(int id)
		{
			if (projection_drop_down)
			{
				ddRect = new Rect(110, 45, 100, 70);
				GUI.Box(ddRect, "");
				for (int i = 0; i < SCANmapProjection.projectionNames.Length; ++i)
				{
					Rect r = new Rect(ddRect.x + 2, ddRect.y + (24 * i), ddRect.width - 4, 20);
					if (GUI.Button(r, SCANmapProjection.projectionNames[i], SCANskins.SCAN_dropDownButton))
					{
						bigmap.setProjection((MapProjection)i);
						bigmap.resetMap();
						SCANcontroller.controller.projection = i;
						drawGrid = true;
						drop_down_open = false;
					}
				}
			}

			else if (mapType_drop_down)
			{
				ddRect = new Rect(270, 45, 70, 70);
				GUI.Box(ddRect, "");
				for (int i = 0; i < SCANmapType.mapTypeNames.Length; i++)
				{
					Rect r = new Rect(ddRect.x + 2, ddRect.y + (24 * i), ddRect.width - 4, 20);
					if (GUI.Button(r, SCANmapType.mapTypeNames[i], SCANskins.SCAN_dropDownButton))
					{
						bigmap.resetMap((mapType)i, true);
						drop_down_open = false;
					}
				}
			}

			else if (resources_drop_down)
			{
				ddRect = new Rect(WindowRect.width - 290, 45, 120, 160);
				GUI.Box(ddRect, "");
				for (int i = 0; i < loadedResources.Count; i++)
				{
					scrollR = GUI.BeginScrollView(ddRect, scrollR, new Rect(0, 0, 100, 20 * loadedResources.Count));
					Rect r = new Rect(2, 20 * i, 96, 20);
					if (GUI.Button(r, loadedResources[i].Name, SCANskins.SCAN_dropDownButton))
					{
						bigmap.Resource = loadedResources[i];
						bigmap.Resource.CurrentBodyConfig(bigmap.Body.name);

						SCANcontroller.controller.resourceSelection = bigmap.Resource.Name;

						if (SCANcontroller.controller.map_ResourceOverlay)
							bigmap.resetMap();

						drop_down_open = false;
					}
					GUI.EndScrollView();
				}
			}

			else if (planetoid_drop_down)
			{
				int j = 0;
				ddRect = new Rect(WindowRect.width - 130, 45, 100, 160);
				GUI.Box(ddRect, "");
				for (int i = 0; i < SCANcontroller.controller.GetDataCount; i++)
				{
					scrollP = GUI.BeginScrollView(ddRect, scrollP, new Rect(0, 0, 80, (20 * SCANcontroller.controller.GetDataCount) + 1));

					SCANdata dropDownData = SCANcontroller.controller.getData(i);
					if (dropDownData != null)
					{
						Rect r = new Rect(2, 20 * j, 76, 20);
						if (GUI.Button(r, dropDownData.Body.name, SCANskins.SCAN_dropDownButton))
						{
							data = dropDownData;
							b = data.Body;
							bigmap.setBody(data.Body);
							bigmap.resetMap();
							drop_down_open = false;
						}
						j++;
					}
					GUI.EndScrollView();
				}
			}

			else
				drop_down_open = false;
		}

	}
}
