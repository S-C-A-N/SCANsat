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
using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.Platform;
using SCANsat;
using UnityEngine;

namespace SCANsat.SCAN_UI
{
	class SCANkscMap: MBW
	{
		private static SCANmap bigmap;
		private static CelestialBody b;
		private SCANdata data;
		//private double startUT;
		private bool drawGrid, spaceCenterLock, trackingStationLock;
		private bool drop_down_open, projection_drop_down, mapType_drop_down, resources_drop_down, planetoid_drop_down;
		private Texture2D overlay_static, map;
		private Rect ddRect, maprect;
		private Rect rc = new Rect(0, 0, 20, 20);
		//private Rect pos_spotmap = new Rect(10f, 10f, 10f, 10f);
		//private Rect pos_spotmap_x = new Rect(10f, 10f, 25f, 25f);
		internal static Rect defaultRect = new Rect(250, 60, 780, 460);
		private const string lockID = "SCANksc_LOCK";

		protected override void Awake()
		{
			WindowCaption = "Map of ";
			WindowRect = defaultRect;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(740), GUILayout.Height(420) };
			WindowStyle = SCANskins.SCAN_window;
			Visible = false;
			DragEnabled = true;
			ClampToScreenOffset = new RectOffset(-600, -600, -400, -400);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");

			InputLockManager.RemoveControlLock(lockID);
		}

		internal override void Start()
		{
			//Initialize the map object
			Visible = SCANcontroller.controller.kscMapVisible;
			if (b == null)
				b = Planetarium.fetch.Home;
			if (bigmap == null)
			{
				bigmap = new SCANmap(b);
				bigmap.setProjection((SCANmap.MapProjection)SCANcontroller.controller.projection);
				bigmap.setWidth(720);
				//WindowRect.x = SCANcontroller.controller.map_x;
				//WindowRect.y = SCANcontroller.controller.map_y;
			}
			else
			{
				//SCANcontroller.controller.map_x = (int)WindowRect.x;
				//SCANcontroller.controller.map_y = (int)WindowRect.y;
			}
			if (SCANcontroller.controller.resourceOverlayType == 1)
				SCANcontroller.controller.map_ResourceOverlay = false;
			WindowCaption = string.Format("Map of {0}", b.theName);
			data = SCANUtil.getData(b);
			bigmap.setBody(b);
		}

		internal override void OnDestroy()
		{
			if (InputLockManager.lockStack.ContainsKey(lockID))
				EditorLogic.fetch.Unlock(lockID);
		}

		protected override void DrawWindowPre(int id)
		{
			WindowCaption = string.Format("Map of {0}", b.theName);

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
				if (WindowRect.Contains(mousePos) && !spaceCenterLock)
				{
					InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS | ControlTypes.KSC_FACILITIES | ControlTypes.KSC_UI, lockID);
					spaceCenterLock = true;
				}
				else if (!WindowRect.Contains(mousePos) && spaceCenterLock)
				{
					InputLockManager.RemoveControlLock(lockID);
					spaceCenterLock = false;
				}
			}

			//Lock tracking scene click through
			if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && !trackingStationLock)
				{
					InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS | ControlTypes.TRACKINGSTATION_ALL, lockID);
					trackingStationLock = true;
				}
				else if (!WindowRect.Contains(mousePos) && trackingStationLock)
				{
					InputLockManager.RemoveControlLock(lockID);
					trackingStationLock = false;
				}
			}
		}

		//The primary GUI method
		protected override void DrawWindow(int id)
		{
			versionLabel(id);
			closeBox(id);

			growS();
				topMenu(id);		/* Top row of buttons - used to control the map types */
				growE();
					toggleBar(id);	/* Toggle options along left side - control overlay options - *Replace buttons with textures* */
					mapDraw(id);	/* Draw the main map texture */
				stopE();
				legendBar(id);		/* Draw the mouseover info and legend bar along the bottom */
			stopS();

			mapLabels(id);			/* Draw the vessel/anomaly icons on the map */
			if (drop_down_open)
				dropDown(id);		/* Draw the drop down menus if any are open */
		}

		protected override void DrawWindowPost(int id)
		{
			//Close the drop down menu if the window is clicked anywhere else
			if (drop_down_open && Event.current.type == EventType.mouseDown && !ddRect.Contains(Event.current.mousePosition))
				drop_down_open = false;

			if (SCANcontroller.controller.globalOverlay) //Update selected resource
				bigmap.setResource(SCANcontroller.ResourcesList[SCANcontroller.controller.gridSelection].name);
		}

		//Draw version label in upper left corner
		private void versionLabel(int id)
		{
			Rect r = new Rect(6, 0, 40, 18);
			GUI.Label(r, SCANversions.SCANsatVersion, SCANskins.SCAN_whiteReadoutLabel);
		}

		//Draw the close button in upper right corner
		private void closeBox(int id)
		{
			Rect r = new Rect(WindowRect.width - 20, 0, 18, 18);
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				Visible = false;
				SCANcontroller.controller.kscMapVisible = Visible;
			}
		}

		//Draw the drop down buttons along the top of the map - Used to control map types
		private void topMenu (int id)
		{
			growE();

			fillS(100);

			if (GUILayout.Button("Projection", SCANskins.SCAN_buttonFixed, GUILayout.MaxWidth(100)))
			{
				projection_drop_down = !projection_drop_down;
				drop_down_open = !drop_down_open;
			}

			fillS(40);

			if (GUILayout.Button("Map Type", SCANskins.SCAN_buttonFixed, GUILayout.MaxWidth(90)))
			{
				mapType_drop_down = !mapType_drop_down;
				drop_down_open = !drop_down_open;
			}

			fillS(60);

			if (GUILayout.Button("Update Map", SCANskins.SCAN_buttonFixed, GUILayout.MaxWidth(90)))
			{
				bigmap.resetMap();
			}

			if (SCANcontroller.controller.globalOverlay && SCANcontroller.controller.resourceOverlayType == 0)
			{
				fillS(60);
				if (GUILayout.Button("Resources", SCANskins.SCAN_buttonFixed, GUILayout.MaxWidth(100)))
				{
					resources_drop_down = !resources_drop_down;
					drop_down_open = !drop_down_open;
				}
			}

			fillS();

			if (GUILayout.Button("Planetoid", SCANskins.SCAN_buttonFixed, GUILayout.MaxWidth(100)))
			{
				planetoid_drop_down = !planetoid_drop_down;
				drop_down_open = !drop_down_open;
			}

			fillS(20);

			stopE();
		}

		//Draw the overlay options along the left side of the map texture
		private void toggleBar (int id)
		{
			growS();

			if (GUILayout.Button("Color", SCANskins.SCAN_buttonFixed))
			{
				if (SCANcontroller.controller.colours == 0)
					SCANcontroller.controller.colours = 1;
				else
					SCANcontroller.controller.colours = 0;
				data.resetImages();
				bigmap.resetMap();
			}

			fillS();

			if (GUILayout.Button("Grid", SCANskins.SCAN_buttonFixed))
			{
				SCANcontroller.controller.map_grid = !SCANcontroller.controller.map_grid;
				drawGrid = true;
			}

			fillS();

			if (GUILayout.Button("Markers", SCANskins.SCAN_buttonFixed))
			{
				SCANcontroller.controller.map_markers = !SCANcontroller.controller.map_markers;
			}

			fillS();

			if (GUILayout.Button("Flags", SCANskins.SCAN_buttonFixed))
			{
				SCANcontroller.controller.map_flags = !SCANcontroller.controller.map_flags;
			}

			fillS();

			if (GUILayout.Button("Asteroids", SCANskins.SCAN_buttonFixed))
			{
				SCANcontroller.controller.map_asteroids = !SCANcontroller.controller.map_asteroids;
			}

			fillS();

			if (GUILayout.Button("Legend", SCANskins.SCAN_buttonFixed))
			{
				SCANcontroller.controller.legend = !SCANcontroller.controller.legend;
			}

			fillS();

			if (SCANcontroller.controller.globalOverlay && SCANcontroller.controller.resourceOverlayType == 0)
			{
				if (GUILayout.Button("Res_Overlay", SCANskins.SCAN_buttonFixed))
				{
					SCANcontroller.controller.map_ResourceOverlay = !SCANcontroller.controller.map_ResourceOverlay;
					bigmap.resetMap();
				}
			}

			stopS();
		}

		//Draw the actual map texture
		private void mapDraw (int id)
		{
			map = bigmap.getPartialMap();

			GUILayout.Label("", GUILayout.Width(map.width), GUILayout.Height(map.height));

			maprect = GUILayoutUtility.GetLastRect();
			maprect.width = bigmap.mapwidth;
			maprect.height = bigmap.mapheight;

			if (overlay_static == null)
			{
				overlay_static = new Texture2D((int)bigmap.mapwidth, (int)bigmap.mapheight, TextureFormat.ARGB32, false);
				drawGrid = true;
			}

			if (drawGrid)
			{
				SCANuiUtil.clearTexture(overlay_static);
				if (SCANcontroller.controller.map_grid)
				{
					SCANuiUtil.drawGrid(maprect, bigmap, overlay_static);
				}
				overlay_static.Apply();
				drawGrid = false;
			}

			GUI.DrawTexture(maprect, map);

			if (overlay_static != null)
			{
				GUI.DrawTexture(maprect, overlay_static, ScaleMode.StretchToFill);
			}

			if (bigmap.projection == SCANmap.MapProjection.Polar)
			{
				rc.x = maprect.x + maprect.width / 2 - maprect.width / 8;
				rc.y = maprect.y + maprect.height / 8;
				SCANuiUtil.drawLabel(rc, "S", false, true, true);
				rc.x = maprect.x + maprect.width / 2 + maprect.width / 8;
				SCANuiUtil.drawLabel(rc, "N", false, true, true);
			}
		}

		//Draw the altitude legend bar along the bottom
		private void legendBar (int id)
		{
			growE();
			fillS(110);
			growS();
			float mx = Event.current.mousePosition.x - maprect.x;
			float my = Event.current.mousePosition.y - maprect.y;
			bool in_map = false;//, in_spotmap = false;
			double mlon = 0, mlat = 0;

			if (mx >= 0 && my >= 0 && mx < map.width && my < map.height)
			{
				double mlo = (mx * 360f / map.width) - 180;
				double mla = 90 - (my * 180f / map.height);
				mlon = bigmap.unprojectLongitude(mlo, mla);
				mlat = bigmap.unprojectLatitude(mlo, mla);

				//if (spotmap != null)
				//{
				//	if (mx >= pos_spotmap.x - maprect.x && my >= pos_spotmap.y - maprect.y && mx <= pos_spotmap.x + pos_spotmap.width - maprect.x && my <= pos_spotmap.y + pos_spotmap.height - maprect.y)
				//	{
				//		in_spotmap = true;
				//		mlon = spotmap.lon_offset + ((mx - pos_spotmap.x + maprect.x) / spotmap.mapscale) - 180;
				//		mlat = spotmap.lat_offset + ((pos_spotmap.height - (my - pos_spotmap.y + maprect.y)) / spotmap.mapscale) - 90;
				//		if (mlat > 90)
				//		{
				//			mlon = (mlon + 360) % 360 - 180;
				//			mlat = 180 - mlat;
				//		}
				//		else if (mlat < -90)
				//		{
				//			mlon = (mlon + 360) % 360 - 180;
				//			mlat = -180 - mlat;
				//		}
				//	}
				//}

				if (mlon >= -180 && mlon <= 180 && mlat >= -90 && mlat <= 90)
				{
					in_map = true;
				}
			}
			
			SCANuiUtil.mouseOverInfo(mlon, mlat, bigmap, data, b, in_map);
			if (bigmap.mapmode == 0 && SCANcontroller.controller.legend)
				SCANuiUtil.drawLegend();
			stopS();
			stopE();
		}

		//Draw the map overlay labels
		private void mapLabels (int id)
		{
			SCANuiUtil.drawMapLabels(maprect, null, bigmap, data, b);
		}

		//Draw the drop down menus if any have been opened
		private void dropDown(int id)
		{
			if (projection_drop_down)
			{
				ddRect = new Rect(110, 45, 100, 70);
				GUI.Box(ddRect, "", SCANskins.SCAN_dropDownBox);
				for (int i = 0; i < SCANmap.projectionNames.Length; ++i)
				{
					Rect r = new Rect(ddRect.x + 2, ddRect.y + (24 * i), ddRect.width - 4, 20);
					if (GUI.Button(r, SCANmap.projectionNames[i], SCANskins.SCAN_dropDownButton))
					{
						bigmap.setProjection((SCANmap.MapProjection)i);
						SCANcontroller.controller.projection = i;
						drawGrid = true;
						drop_down_open = false;
					}
				}
			}

			else if (mapType_drop_down)
			{
				ddRect = new Rect(270, 45, 70, 70);
				GUI.Box(ddRect, "", SCANskins.SCAN_dropDownBox);
				for (int i = 0; i < SCANmap.mapTypeNames.Length; i++)
				{
					Rect r = new Rect(ddRect.x + 2, ddRect.y + (24 * i), ddRect.width - 4, 20);
					if (GUI.Button(r, SCANmap.mapTypeNames[i], SCANskins.SCAN_dropDownButton))
					{
						bigmap.resetMap(i, 0);
						drop_down_open = false;
					}
				}
			}

			else if (resources_drop_down)
			{
				ddRect = new Rect(WindowRect.width - 274, 45, 100, SCANcontroller.ResourcesList.Count * 20);
				GUI.Box(ddRect, "", SCANskins.SCAN_dropDownBox);
				for (int i = 0; i < SCANcontroller.ResourcesList.Count; i++)
				{
					Rect r = new Rect(ddRect.x + 2, ddRect.y + (24 * i), ddRect.width - 4, 20);
					if (GUI.Button(r, SCANcontroller.ResourcesList[i].name, SCANskins.SCAN_dropDownButton))
					{
						SCANcontroller.controller.gridSelection = i;
						drop_down_open = false;
					}
				}
			}

			else if (planetoid_drop_down)
			{
				ddRect = new Rect(WindowRect.width - 130, 45, 100, FlightGlobals.Bodies.Count * 20);
				GUI.Box(ddRect, "", SCANskins.SCAN_dropDownBox);
				for (int i = 0; i < FlightGlobals.Bodies.Count; i++)
				{
					Rect r = new Rect(ddRect.x + 2, ddRect.y + (20 * i), ddRect.width - 4, 20);
					if (GUI.Button(r, FlightGlobals.Bodies[i].name, SCANskins.SCAN_dropDownButton))
					{
						b = FlightGlobals.Bodies[i];
						data = SCANUtil.getData(b);
						bigmap.setBody(b);
						drop_down_open = false;
					}
				}
			}

			else
				drop_down_open = false;

		}

	}
}
