#region license
/* 
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - Zoom window object
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 *
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_Platform;
using SCANsat;
using SCANsat.SCAN_UI.UI_Framework;
using SCANsat.SCAN_PartModules;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Map;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;
using UnityEngine;

namespace SCANsat.SCAN_UI
{
	class SCANzoomWindow : SCAN_MBW
	{
		protected SCANmap spotmap;
		private SCANmap bigmap;
		protected CelestialBody b;
		protected SCANdata data;
		protected Vessel v;
		protected SCANresourceGlobal resource;
		protected bool showOrbit, showAnomaly, showWaypoints;
		private bool narrowBand, showInfo, controlLock;
		protected float minZoom = 2;
		protected float maxZoom = 1000;
		private Vector2 dragStart;
		private Vector2d mjTarget = new Vector2d();
		private float resizeW, resizeH;
		private const string lockID = "SCANzoom_LOCK";
		internal readonly static Rect defaultRect = new Rect(50f, 50f, 340f, 240f);
		private static Rect sessionRect = defaultRect;

		protected bool dropDown;
		protected Rect ddRect;
		private Vector2 scrollR;
		protected List<SCANresourceGlobal> loadedResources = new List<SCANresourceGlobal>();
		protected bool resourceOverlay;

		protected bool highDetail;

		protected override void Awake()
		{
			WindowRect = sessionRect;
			WindowSize_Min = new Vector2(310, 180);
			WindowSize_Max = new Vector2(540, 400);
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(340), GUILayout.Height(240) };
			WindowStyle = SCANskins.SCAN_window;
			showInfo = true;
			Visible = false;
			DragEnabled = true;
			ClampEnabled = true;
			TooltipMouseOffset = new Vector2d(-10, -25);
			ClampToScreenOffset = new RectOffset(-200, -200, -160, -160);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
			SCAN_SkinsLibrary.SetCurrentTooltip();

			removeControlLocks();

			Startup();
		}

		protected virtual void Startup()
		{
			//Initialize the map object
			Visible = false;

			if (HighLogic.LoadedSceneIsFlight)
			{
				v = SCANcontroller.controller.BigMap.V;
				b = SCANcontroller.controller.BigMap.Body;
				data = SCANcontroller.controller.BigMap.Data;
			}
			else if (HighLogic.LoadedSceneHasPlanetarium)
			{
				v = null;
				b = SCANcontroller.controller.kscMap.Body;
				data = SCANcontroller.controller.kscMap.Data;
			}

			if (spotmap == null)
			{
				spotmap = new SCANmap(b, false, true);
				spotmap.setSize(320, 240);
			}

			showOrbit = SCANcontroller.controller.map_orbit;
			showAnomaly = SCANcontroller.controller.map_markers;
			resourceOverlay = SCANcontroller.controller.map_ResourceOverlay;

			if (SCANconfigLoader.GlobalResource)
			{
				loadedResources = SCANcontroller.setLoadedResourceList();
			}

			if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
				showWaypoints = false;
			else
				showWaypoints = SCANcontroller.controller.map_waypoints;

			TooltipsEnabled = SCANcontroller.controller.toolTips;

			spotmap.setBody(b);
		}

		protected override void OnDestroy()
		{
			removeControlLocks();
		}

		internal void removeControlLocks()
		{
			InputLockManager.RemoveControlLock(lockID);
			controlLock = false;
		}

		public virtual void setMapCenter(double lat, double lon, bool centering, SCANmap big = null, SCANhiDefCamera camera = null)
		{
			highDetail = centering;
			Visible = true;
			bigmap = big;

			resource = bigmap.Resource;

			SCANcontroller.controller.TargetSelecting = false;
			SCANcontroller.controller.TargetSelectingActive = false;

			if (bigmap.Projection == MapProjection.Polar)
				spotmap.setProjection(MapProjection.Polar);
			else
				spotmap.setProjection(MapProjection.Rectangular);

			if (bigmap.Body != b)
			{
				SCANdata dat = SCANUtil.getData(bigmap.Body);
				if (dat == null)
					dat = new SCANdata(bigmap.Body);

				data = dat;
				b = data.Body;

				spotmap.setBody(b);
			}

			if (SCANconfigLoader.GlobalResource && narrowBand)
			{
				resource = bigmap.Resource;
				spotmap.Resource = resource;
				spotmap.Resource.CurrentBodyConfig(b.name);
			}
	
			if (SCANcontroller.controller.needsNarrowBand && resourceOverlay)
				checkForScanners();

			spotmap.MapScale = 10;

			spotmap.centerAround(lon, lat);

			spotmap.resetMap(bigmap.MType, false, resourceOverlay, narrowBand);
		}

		protected virtual void resetMap(bool checkScanner = false, double lon = 0, double lat = 0, bool withCenter = false)
		{
			if (withCenter)
				spotmap.centerAround(lon, lat);
			else
				spotmap.centerAround(spotmap.CenteredLong, spotmap.CenteredLat);

			SCANcontroller.controller.TargetSelecting = false;
			SCANcontroller.controller.TargetSelectingActive = false;

			if (checkScanner && SCANcontroller.controller.needsNarrowBand && resourceOverlay)
				checkForScanners();

			spotmap.resetMap(resourceOverlay, narrowBand || !checkScanner);
		}

		protected virtual void resyncMap()
		{
			SCANcontroller.controller.TargetSelecting = false;
			SCANcontroller.controller.TargetSelectingActive = false;

			if (bigmap.Projection == MapProjection.Polar)
				spotmap.setProjection(MapProjection.Polar);
			else
				spotmap.setProjection(MapProjection.Rectangular);

			if (bigmap.Body != b)
			{
				SCANdata dat = SCANUtil.getData(bigmap.Body);
				if (dat == null)
					dat = new SCANdata(bigmap.Body);

				data = dat;
				b = data.Body;

				spotmap.setBody(b);
			}

			if (SCANconfigLoader.GlobalResource && narrowBand)
			{
				resource = bigmap.Resource;
				spotmap.Resource = resource;
				spotmap.Resource.CurrentBodyConfig(b.name);
			}

			if (SCANcontroller.controller.needsNarrowBand && resourceOverlay)
				checkForScanners();

			spotmap.centerAround(spotmap.CenteredLong, spotmap.CenteredLat);

			spotmap.resetMap(bigmap.MType, false, resourceOverlay, narrowBand);
		}

		public SCANmap SpotMap
		{
			get { return spotmap; }
		}

		private double inc(double d)
		{
			if (d > 90)
				d = 180 - d;

			return d;
		}

		public virtual void closeMap()
		{
			removeControlLocks();
			Visible = false;
		}

		private void checkForScanners()
		{
			narrowBand = false;
			foreach (Vessel vessel in FlightGlobals.Vessels)
			{
				if (vessel.protoVessel.protoPartSnapshots.Count <= 1)
					continue;

				if (vessel.vesselType == VesselType.Debris || vessel.vesselType == VesselType.Unknown || vessel.vesselType == VesselType.EVA || vessel.vesselType == VesselType.Flag)
					continue;

				if (vessel.mainBody != b)
					continue;

				if (vessel.situation != Vessel.Situations.ORBITING)
					continue;

				if (inc(vessel.orbit.inclination) < Math.Abs(spotmap.CenteredLat) - 10)
					continue;

				var scanners = from pref in vessel.protoVessel.protoPartSnapshots
							   where pref.modules.Any(a => a.moduleName == "ModuleResourceScanner")
							   select pref;

				if (scanners.Count() == 0)
					continue;

				foreach (var p in scanners)
				{
					if (p.partInfo == null)
						continue;

					ConfigNode node = p.partInfo.partConfig;

					if (node == null)
						continue;

					var moduleNodes = from nodes in node.GetNodes("MODULE")
									  where nodes.GetValue("name") == "ModuleResourceScanner"
									  select nodes;

					foreach (ConfigNode moduleNode in moduleNodes)
					{
						if (moduleNode == null)
							continue;

						if (moduleNode.HasValue("MaxAbundanceAltitude"))
						{
							string alt = moduleNode.GetValue("MaxAbundanceAltitude");
							float f = 0;
							if (!float.TryParse(alt, out f))
								continue;

							if (f < vessel.altitude)
								continue;
						}

						if (moduleNode.GetValue("ScannerType") != "0")
							continue;

						if (moduleNode.GetValue("ResourceName") != resource.Name)
							continue;

						if (spotmap.Resource != resource)
						{
							spotmap.Resource = resource;
							spotmap.Resource.CurrentBodyConfig(b.name);
							if (resourceOverlay)
								spotmap.resetMap(resourceOverlay, true);
						}

						if (spotmap.Resource != null)
						{
							narrowBand = true;
							break;
						}
					}

					if (narrowBand)
						break;
				}

				if (narrowBand)
					break;
			}

			if (!narrowBand)
				spotmap.Resource = null;
		}

		protected override void Update()
		{
			if (Visible)
			{
				if (!SCANcontroller.controller.needsNarrowBand && SCANconfigLoader.GlobalResource)
					narrowBand = true;

				if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ready)
					v = FlightGlobals.ActiveVessel;
				else if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
				{
					MapObject target = PlanetariumCamera.fetch.target;

					if (target.type == MapObject.MapObjectType.VESSEL)
						v = target.vessel;
					else
						v = null;
				}
			}
		}

		protected override void DrawWindowPre(int id)
		{
			WindowCaption = SCANuiUtil.toDMS(spotmap.CenteredLat, spotmap.CenteredLong);

			if (IsResizing && !inRepaint())
			{
				if (Input.GetMouseButtonUp(0))
				{
					double scale = spotmap.MapScale;
					IsResizing = false;
					if (resizeW < WindowSize_Min.x)
						resizeW = WindowSize_Min.x;
					else if (resizeW > WindowSize_Max.x)
						resizeW = WindowSize_Max.x;
					if (resizeH < WindowSize_Min.y)
						resizeH = WindowSize_Min.y;
					else if (resizeH > WindowSize_Max.y)
						resizeH = WindowSize_Max.y;

					if ((int)resizeW % 2 != 0)
						resizeW += 1;
					if ((int)resizeH % 2 != 0)
						resizeH += 1;

					if ((int)resizeW % 4 != 0)
						resizeW += 2;
					if ((int)resizeH % 4 != 0)
						resizeH += 2;

					spotmap.setSize((int)resizeW, (int)resizeH);
					spotmap.MapScale = scale;

					resetMap(true);
				}
				else
				{
					float yy = Input.mousePosition.y;
					float xx = Input.mousePosition.x;
					if (Input.mousePosition.y < 0)
						yy = 0;
					if (Input.mousePosition.x < 0)
						xx = 0;

					resizeH += dragStart.y - yy;
					dragStart.y = yy;
					resizeW += xx - dragStart.x;
					dragStart.x = xx;
				}
				if (Event.current.isMouse)
					Event.current.Use();
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
					InputLockManager.RemoveControlLock(lockID);
					controlLock = false;
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
					InputLockManager.RemoveControlLock(lockID);
					controlLock = false;
				}
			}
		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);
			closeBox(id);

			growS();
				topBar(id);
				fillS(28);
				drawMap(id);
				mouseOver(id);
			stopS();

			mapLabels(id);

			drawDropDown(id);
		}

		protected override void DrawWindowPost(int id)
		{
			sessionRect = WindowRect;

			if (SCANcontroller.controller.TargetSelecting && Event.current.type == EventType.mouseDown && !TextureRect.Contains(Event.current.mousePosition))
			{
				SCANcontroller.controller.TargetSelecting = false;
				SCANcontroller.controller.TargetSelectingActive = false;
				data.removeTargetWaypoint();
			}

			if (dropDown && Event.current.type == EventType.mouseUp && !ddRect.Contains(Event.current.mousePosition))
				dropDown = false;
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
			Rect r = new Rect(WindowRect.width - 40, 0, 18, 18);
			if (showInfo)
			{
				if (GUI.Button(r, "-", SCANskins.SCAN_buttonBorderless))
					showInfo = !showInfo;
			}
			else
			{
				if (GUI.Button(r, "+", SCANskins.SCAN_buttonBorderless))
					showInfo = !showInfo;
			}
			r.x += 20;
			r.y += 1;
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				closeMap();
			}
		}

		private void topBar(int id)
		{
			Rect r = new Rect();

			if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
			{
				if (v != null)
				{
					r = new Rect(6, 20, 16, 16);

					showOrbit = GUI.Toggle(r, showOrbit, textWithTT("", "Toggle Orbit"), SCANskins.SCAN_settingsToggle);

					r.x += 16;
					r.width = 40;
					r.height = 20;

					if (GUI.Button(r, iconWithTT(SCANskins.SCAN_OrbitIcon, "Toggle Orbit"), SCANskins.SCAN_buttonBorderless))
					{
						showOrbit = !showOrbit;
					}
				}

				r = new Rect(78, 20, 24, 24);

				if (SCANcontroller.controller.mechJebTargetSelection)
				{
					if (SCANcontroller.controller.MechJebLoaded && SCANcontroller.controller.LandingTargetBody == b)
					{
						if (GUI.Button(r, textWithTT("", "Set MechJeb Target"), SCANskins.SCAN_buttonBorderless))
						{
							SCANcontroller.controller.TargetSelecting = !SCANcontroller.controller.TargetSelecting;
						}

						r.x += 1;
						r.y += 1;
						r.width = r.height = 22;

						Color old = GUI.color;
						GUI.color = palette.red;
						GUI.DrawTexture(r, SCANskins.SCAN_MechJebIcon);
						GUI.color = old;
					}
				}
				else
				{
					if (GUI.Button(r, textWithTT("", "Set Landing Target"), SCANskins.SCAN_buttonBorderless))
					{
						SCANcontroller.controller.TargetSelecting = !SCANcontroller.controller.TargetSelecting;
					}

					r.x += 1;
					r.y += 1;
					r.width = r.height = 22;

					Color old = GUI.color;
					GUI.color = palette.xkcd_PukeGreen;
					GUI.DrawTexture(r, SCANskins.SCAN_TargetIcon);
					GUI.color = old;
				}
			}

			r = new Rect(WindowRect.width / 2 - 58, 20, 26, 26);

			if (GUI.Button(r, iconWithTT(SCANskins.SCAN_ZoomOutIcon, "Zoom Out"), SCANskins.SCAN_buttonBorderless))
			{
				spotmap.MapScale = spotmap.MapScale / 1.25f;
				if (spotmap.MapScale < minZoom)
					spotmap.MapScale = minZoom;
				resetMap();
			}

			r.x += 30;
			r.width = 50;

			if (GUI.Button(r, textWithTT(spotmap.MapScale.ToString("N1") + " X", "Sync To Big Map"), SCANskins.SCAN_buttonBorderless))
			{
				resyncMap();
			}

			r.x += 54;
			r.width = 26;

			if (GUI.Button(r, iconWithTT(SCANskins.SCAN_ZoomInIcon, "Zoom In"), SCANskins.SCAN_buttonBorderless))
			{
				spotmap.MapScale = spotmap.MapScale * 1.25f;
				if (spotmap.MapScale > maxZoom)
					spotmap.MapScale = maxZoom;
				resetMap();
			}

			if (SCANconfigLoader.GlobalResource)
			{
				r = new Rect(WindowRect.width - 100, 20, 24, 24);

				if (GUI.Button(r, iconWithTT(SCANskins.SCAN_ResourceIcon, "Resources"), SCANskins.SCAN_buttonBorderless))
				{
					dropDown = !dropDown;
				}
			}

			if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
			{
				r = new Rect(WindowRect.width - 68, 20, 18, 18);

				showWaypoints = GUI.Toggle(r, showWaypoints, textWithTT("", "Toggle Waypoints"), SCANskins.SCAN_settingsToggle);

				r.x += 13;
				r.width = r.height = 20;

				if (GUI.Button(r, iconWithTT(SCANskins.SCAN_WaypointIcon, "Toggle Waypoints"), SCANskins.SCAN_buttonBorderless))
				{
					showWaypoints = !showWaypoints;
				}
			}

			r = new Rect(WindowRect.width - 35, 20, 18, 18);

			showAnomaly = GUI.Toggle(r, showAnomaly, textWithTT("", "Toggle Anomalies"), SCANskins.SCAN_settingsToggle);

			r.x += 13;
			r.width = r.height = 20;

			if (GUI.Button(r, textWithTT(SCANcontroller.controller.anomalyMarker, "Toggle Anomalies"), SCANskins.SCAN_buttonBorderless))
			{
				showAnomaly = !showAnomaly;
			}
		}

		private void drawMap(int id)
		{
			MapTexture = getMap();

			//A blank label used as a template for the actual map texture
			if (IsResizing)
			{
				//Set minimum map size during re-sizing
				dW = resizeW;
				if (dW < WindowSize_Min.x)
					dW = WindowSize_Min.x;
				else if (dW > WindowSize_Max.x)
					dW = WindowSize_Max.x;
				dH = resizeH;
				if (dH < WindowSize_Min.y)
					dH = WindowSize_Min.y;
				else if (dH > WindowSize_Max.y)
					dH = WindowSize_Max.y;

				GUILayout.Label("", GUILayout.Width(dW), GUILayout.Height(dH));
			}
			else
			{
				GUILayout.Label("", GUILayout.Width(MapTexture.width), GUILayout.Height(MapTexture.height));
			}

			TextureRect = GUILayoutUtility.GetLastRect();
			TextureRect.width = spotmap.MapWidth;
			TextureRect.height = spotmap.MapHeight;

			//Stretches the existing map while re-sizing
			if (IsResizing)
			{
				TextureRect.width = dW;
				TextureRect.height = dH;
				GUI.DrawTexture(TextureRect, MapTexture, ScaleMode.StretchToFill);
			}
			else
			{
				GUI.DrawTexture(TextureRect, MapTexture);
			}
		}

		protected virtual Texture2D getMap()
		{
			return spotmap.getPartialMap();
		}

		private void mouseOver(int id)
		{
			float mx = Event.current.mousePosition.x - TextureRect.x;
			float my = Event.current.mousePosition.y - TextureRect.y;
			bool in_map = false;
			double mlon = 0, mlat = 0;

			//Draw the re-size label in the corner
			Rect resizer = new Rect(WindowRect.width - 24, WindowRect.height - 26, 24, 24);
			GUI.Label(resizer, SCANskins.SCAN_ResizeIcon);

			//Handles mouse positioning and converting to lat/long coordinates
			if (mx >= 0 && my >= 0 && mx <= TextureRect.width && my <= TextureRect.height  /*mx >= 0 && my >= 0 && mx < MapTexture.width && my < MapTexture.height*/)
			{
				double mlo = spotmap.Lon_Offset + (mx / spotmap.MapScale) - 180;
				double mla = spotmap.Lat_Offset + ((TextureRect.height - my) / spotmap.MapScale) - 90;
				mlon = spotmap.unprojectLongitude(mlo, mla);
				mlat = spotmap.unprojectLatitude(mlo, mla);

				if (mlon >= -180 && mlon <= 180 && mlat >= -90 && mlat <= 90)
				{
					in_map = true;
					if (SCANcontroller.controller.TargetSelecting)
					{
						SCANcontroller.controller.TargetSelectingActive = true;
						mjTarget.x = mlon;
						mjTarget.y = mlat;
						SCANcontroller.controller.LandingTargetCoords = mjTarget;
						Rect r = new Rect(mx + TextureRect.x - 11, my + TextureRect.y - 13, 24, 24);
						SCANuiUtil.drawMapIcon(r, SCANcontroller.controller.mechJebTargetSelection ? SCANskins.SCAN_MechJebIcon : SCANskins.SCAN_TargetIcon, true, palette.yellow, true);
					}
				}
				else if (SCANcontroller.controller.TargetSelecting)
					SCANcontroller.controller.TargetSelectingActive = false;

				if (mlat > 90)
				{
					mlon = (mlon + 360) % 360 - 180;
					mlat = 180 - mlat;
				}
				else if (mlat < -90)
				{
					mlon = (mlon + 360) % 360 - 180;
					mlat = -180 - mlat;
				}
			}
			else if (SCANcontroller.controller.TargetSelecting)
				SCANcontroller.controller.TargetSelectingActive = false;

			//Handles mouse click while inside map
			if (!dropDown && Event.current.isMouse)
			{
				if (Event.current.type == EventType.MouseUp)
				{
					//Generate waypoint for MechJeb target
					if (SCANcontroller.controller.TargetSelecting && SCANcontroller.controller.TargetSelectingActive && Event.current.button == 0 && in_map)
					{
						string s = SCANcontroller.controller.mechJebTargetSelection ? "MechJeb Landing Target" : "Landing Target Site";
						SCANwaypoint w = new SCANwaypoint(mlat, mlon, s);
						SCANcontroller.controller.LandingTarget = w;
						data.addToWaypoints();
						SCANcontroller.controller.TargetSelecting = false;
						SCANcontroller.controller.TargetSelectingActive = false;
					}
					//Middle click re-center
					else if (Event.current.button == 2 || (Event.current.button == 1 && GameSettings.MODIFIER_KEY.GetKey()))
					{
						if (in_map)
						{
							resetMap(true, mlon, mlat, highDetail);
						}
					}
					//Right click zoom in
					else if (Event.current.button == 1)
					{
						if (in_map)
						{
							spotmap.MapScale = spotmap.MapScale * 1.25f;
							if (spotmap.MapScale > maxZoom)
								spotmap.MapScale = maxZoom;
							resetMap(true, mlon, mlat, highDetail);
						}
					}
					//Left click zoom out
					else if (Event.current.button == 0)
					{
						if (in_map)
						{
							spotmap.MapScale = spotmap.MapScale / 1.25f;
							if (spotmap.MapScale < minZoom)
								spotmap.MapScale = minZoom;
							resetMap(true, mlon, mlat, highDetail);
						}
					}
					Event.current.Use();
				}

				//Handle clicking inside the re-size button
				else if (Event.current.isMouse
				&& Event.current.type == EventType.MouseDown
				&& Event.current.button == 0
				&& resizer.Contains(Event.current.mousePosition))
				{
					IsResizing = true;
					dragStart.x = Input.mousePosition.x;
					dragStart.y = Input.mousePosition.y;
					resizeW = TextureRect.width;
					resizeH = TextureRect.height;
					Event.current.Use();
				}
			}

			//Draw the actual mouse over info label below the map
			if (SCANcontroller.controller.TargetSelecting)
			{
				SCANuiUtil.readableLabel(SCANcontroller.controller.mechJebTargetSelection ? "MechJeb Landing Guidance Targeting..." : "Landing Site Targeting...", false);
				fillS(-10);
				SCANuiUtil.mouseOverInfoSimple(mlon, mlat, spotmap, data, spotmap.Body, in_map);
			}
			else if (showInfo)
				SCANuiUtil.mouseOverInfoSimple(mlon, mlat, spotmap, data, spotmap.Body, in_map);
			else
				fillS(10);
		}

		private void mapLabels(int id)
		{
			//Draw the orbit overlays
			if (showOrbit && v != null)
			{
				SCANuiUtil.drawOrbit(TextureRect, spotmap, v, spotmap.Body, true);
			}

			SCANuiUtil.drawMapLabels(TextureRect, v, spotmap, data, spotmap.Body, showAnomaly, showWaypoints);
		}

		private void drawDropDown(int id)
		{
			if (!dropDown)
				return;

			ddRect = new Rect(WindowRect.width - 190, 48, 160, 200);
			GUI.Box(ddRect, "");
			for (int i = -1; i < loadedResources.Count; i++)
			{
				scrollR = GUI.BeginScrollView(ddRect, scrollR, new Rect(0, 0, 140, 20 * (loadedResources.Count + 1)));

				Rect r;

				if (i == -1)
				{
					r = new Rect(2, 0, 130, 20);
					if (GUI.Button(r, "Toggle Resources", resourceOverlay ? SCANskins.SCAN_dropDownButtonActive : SCANskins.SCAN_dropDownButton))
					{
						resourceOverlay = !resourceOverlay;

						resetMap(true);
						dropDown = false;
					}
				}
				else
				{
					r = new Rect(2, 20 * (i + 1), 130, 20);
					if (GUI.Button(r, loadedResources[i].Name, resource.Name == loadedResources[i].Name ? SCANskins.SCAN_dropDownButtonActive : SCANskins.SCAN_dropDownButton))
					{
						resourceOverlay = true;

						resource = loadedResources[i];
						spotmap.Resource = resource;
						spotmap.Resource.CurrentBodyConfig(b.name);

						resetMap(true);

						dropDown = false;
					}
				}
				GUI.EndScrollView();
			}
		}

	}
}
