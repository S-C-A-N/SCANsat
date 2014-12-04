#region license
/* 
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - New Big map window object
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
	class SCANnewBigMap : SCAN_MBW
	{
		private static SCANmap bigmap;
		private static CelestialBody b;
		private static string mapTypeTitle = "";
		internal SCANdata data;
		private Vessel v;
		private double startUT;
		private bool drawGrid, currentGrid, currentColor, lastColor;
		private bool drop_down_open, projection_drop_down, mapType_drop_down, resources_drop_down, planetoid_drop_down;
		private Texture2D overlay_static;
		private Rect ddRect;
		private Rect rc = new Rect(0, 0, 20, 20);
		private Vector2 scrollP, scrollR;
		//private Rect pos_spotmap = new Rect(10f, 10f, 10f, 10f);
		//private Rect pos_spotmap_x = new Rect(10f, 10f, 25f, 25f);
		internal static Rect defaultRect = new Rect(250, 60, 780, 460);

		protected override void Awake()
		{
			WindowCaption = "Map of ";
			WindowRect = defaultRect;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(740), GUILayout.Height(420) };
			WindowStyle = SCANskins.SCAN_window;
			Visible = false;
			DragEnabled = true;
			ClampEnabled = false;
			ResizeEnabled = true;
			//ClampToScreenOffset = new RectOffset(-600, -600, -400, -400);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		internal override void Start()
		{
			//Initialize the map object
			Visible = SCANcontroller.controller.bigMapVisible;
			if (v == null)
				v = FlightGlobals.ActiveVessel;
			if (b == null)
				b = v.mainBody;
			if (bigmap == null)
			{
				bigmap = new SCANmap(b);
				bigmap.setProjection((SCANmap.MapProjection)SCANcontroller.controller.projection);
				bigmap.setWidth(SCANcontroller.controller.map_width);
				WindowRect.x = SCANcontroller.controller.map_x;
				WindowRect.y = SCANcontroller.controller.map_y;
			}
			else
			{
				SCANcontroller.controller.map_x = (int)WindowRect.x;
				SCANcontroller.controller.map_y = (int)WindowRect.y;
			}
			if (SCANcontroller.controller.resourceOverlayType == 1)
				SCANcontroller.controller.map_ResourceOverlay = false;
			currentColor = SCANcontroller.controller.colours == 0;
			lastColor = currentColor;
			WindowCaption = string.Format("Map of {0}", b.theName);
			data = SCANUtil.getData(b);
			if (data == null)
			{
				data = new SCANdata(b);
				SCANcontroller.controller.addToBodyData(b, data);
			}
			bigmap.setBody(b);
			bigmap.resource = SCANcontroller.controller.ResourceList[SCANcontroller.controller.resourceSelection];
		}

		protected override void DrawWindowPre(int id)
		{
			if (bigmap != null)
				mapTypeTitle = SCANmap.mapTypeNames[bigmap.mapmode];
			else
				mapTypeTitle = "";

			WindowCaption = string.Format("{0} Map of {1}", mapTypeTitle, b.theName);

			//Disable any errant drop down menus
			if (!drop_down_open)
			{
				projection_drop_down = false;
				mapType_drop_down = false;
				resources_drop_down = false;
				planetoid_drop_down = false;
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
					toggleBar(id);	/* Toggle options along left side - control overlay options - *Replace buttons with		textures*    */
					fillS(60);
					mapDraw(id);	/* Draw the main map texture */
				stopE();
				growE();
					fillS(120);
					legendBar(id);	/* Draw the mouseover info and legend bar along the bottom */
				stopE();
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

			//if (SCANcontroller.controller.globalOverlay) //Update selected resource
			//	bigmap.setResource(SCANcontroller.controller.ResourcesList[SCANcontroller.controller.gridSelection].Name);

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

			if (currentGrid != SCANcontroller.controller.map_grid)
			{
				currentGrid = SCANcontroller.controller.map_grid;
				drawGrid = true;
			}
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
				SCANcontroller.controller.bigMapVisible = Visible;
			}
		}

		//Draw the drop down buttons along the top of the map - Used to control map types
		private void topMenu(int id)
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
				if (SCANcontroller.controller.GlobalResourceOverlay)
				{
					fillS(60);
					if (GUILayout.Button("Resources", SCANskins.SCAN_buttonFixed, GUILayout.MaxWidth(100)))
					{
						resources_drop_down = !resources_drop_down;
						drop_down_open = !drop_down_open;
					}
				}
				//fillS();
				//if (GUILayout.Button("Planetoid", SCANskins.SCAN_buttonFixed, GUILayout.MaxWidth(100)))
				//{
				//	planetoid_drop_down = !planetoid_drop_down;
				//	drop_down_open = !drop_down_open;
				//}
				fillS(20);
			stopE();
		}

		//Draw the overlay options along the left side of the map texture
		private void toggleBar(int id)
		{
			growS();

				currentColor = GUILayout.Toggle(currentColor, "");

				Rect d = GUILayoutUtility.GetLastRect();
				d.x += 44;
				d.y += 2;
				d.width = 24;
				d.height = 24;

				if (GUI.Button(d, SCANskins.SCAN_ColorWheelIcon, SCANskins.SCAN_buttonBorderless))
				{
					currentColor = !currentColor;
				}

				fillS();

				SCANcontroller.controller.map_grid = GUILayout.Toggle(SCANcontroller.controller.map_grid, "");

				d = GUILayoutUtility.GetLastRect();
				d.x += 34;
				d.y += 2;
				d.width = 48;
				d.height = 24;

				if (GUI.Button(d, SCANskins.SCAN_GridIcon, SCANskins.SCAN_buttonBorderless))
				{
					SCANcontroller.controller.map_grid = !SCANcontroller.controller.map_grid;
				}

				fillS();

				SCANcontroller.controller.map_orbit = GUILayout.Toggle(SCANcontroller.controller.map_orbit, "");

				d = GUILayoutUtility.GetLastRect();
				d.x += 34;
				d.y += 2;
				d.width = 48;
				d.height = 24;

				if (GUI.Button(d, SCANskins.SCAN_OrbitIcon, SCANskins.SCAN_buttonBorderless))
				{
					SCANcontroller.controller.map_orbit = !SCANcontroller.controller.map_orbit;
				}

				fillS();

				SCANcontroller.controller.map_markers = GUILayout.Toggle(SCANcontroller.controller.map_markers, "");

				d = GUILayoutUtility.GetLastRect();
				d.x += 44;
				d.y += 2;
				d.width = 24;
				d.height = 24;

				if (GUI.Button(d, SCANcontroller.controller.anomalyMarker, SCANskins.SCAN_buttonBorderless))
				{
					SCANcontroller.controller.map_markers = !SCANcontroller.controller.map_markers;
				}

				fillS();

				SCANcontroller.controller.map_flags = GUILayout.Toggle(SCANcontroller.controller.map_flags, "");

				d = GUILayoutUtility.GetLastRect();
				d.x += 44;
				d.y += 2;
				d.width = 24;
				d.height = 24;

				if (GUI.Button(d, SCANskins.SCAN_FlagIcon, SCANskins.SCAN_buttonBorderless))
				{
					SCANcontroller.controller.map_flags = !SCANcontroller.controller.map_flags;
				}

				fillS();

				SCANcontroller.controller.map_asteroids = GUILayout.Toggle(SCANcontroller.controller.map_asteroids, "");

				d = GUILayoutUtility.GetLastRect();
				d.x += 44;
				d.y += 2;
				d.width = 24;
				d.height = 24;

				if (GUI.Button(d, SCANskins.SCAN_AsteroidIcon, SCANskins.SCAN_buttonBorderless))
				{
					SCANcontroller.controller.map_asteroids = !SCANcontroller.controller.map_asteroids;
				}

				fillS();

				SCANcontroller.controller.legend = GUILayout.Toggle(SCANcontroller.controller.legend, "");

				d = GUILayoutUtility.GetLastRect();
				d.x += 34;
				d.y += 2;
				d.width = 48;
				d.height = 24;

				if (GUI.Button(d, SCANskins.SCAN_LegendIcon, SCANskins.SCAN_buttonBorderless))
				{
					SCANcontroller.controller.legend = !SCANcontroller.controller.legend;
				}

				fillS();

				if (SCANcontroller.controller.GlobalResourceOverlay)
				{
					if (GUILayout.Button("Resources", SCANskins.SCAN_buttonFixed))
					{
						SCANcontroller.controller.map_ResourceOverlay = !SCANcontroller.controller.map_ResourceOverlay;
						bigmap.resetMap();
					}
				}

			stopS();

			//Make a 2x2 grid for all four windows using icons instead of text; use tooltips
			Rect s = new Rect(20, WindowRect.height - 80, 80, 20);

			//if (GUI.Button(s, "Small Map", SCANskins.SCAN_buttonFixed))
			//{
			//	SCANcontroller.controller.mainMap.Visible = !SCANcontroller.controller.mainMap.Visible;
			//}

			//if (GUI.Button(s, "Instruments", SCANskins.SCAN_buttonFixed))
			//{
			//	SCANcontroller.controller.instrumentsWindow.Visible = !SCANcontroller.controller.instrumentsWindow.Visible;
			//}

			if (GUI.Button(s, "Settings", SCANskins.SCAN_buttonFixed))
			{
				SCANcontroller.controller.settingsWindow.Visible = !SCANcontroller.controller.settingsWindow.Visible;
			}

			s.y += 30;
			s.height = 38;

			if (GUI.Button(s, "Color\nControl", SCANskins.SCAN_buttonFixed))
			{
				SCANcontroller.controller.colorManager.Visible = !SCANcontroller.controller.colorManager.Visible;
			}
		}

		private void mapDraw(int id)
		{
			MapTexture = bigmap.getPartialMap();

			//A blank label used as a template for the actual map texture
			GUILayout.Label("", GUILayout.Width(MapTexture.width), GUILayout.Height(MapTexture.height));

			TextureRect = GUILayoutUtility.GetLastRect();
			TextureRect.width = bigmap.mapwidth;
			TextureRect.height = bigmap.mapheight;

			//The background texture for the map
			if (overlay_static == null)
			{
				overlay_static = new Texture2D((int)bigmap.mapwidth, (int)bigmap.mapheight, TextureFormat.ARGB32, false);
				drawGrid = true;
			}

			//Generate the grid texture
			if (drawGrid)
			{
				SCANuiUtil.clearTexture(overlay_static);
				if (SCANcontroller.controller.map_grid)
				{
					SCANuiUtil.drawGrid(TextureRect, bigmap, overlay_static);
				}
				overlay_static.Apply();
				drawGrid = false;
			}

			//Stretches the existing map while re-sizing
			if (IsDragging)
			{
				TextureRect.width = dW;
				TextureRect.height = dH;
				GUI.DrawTexture(TextureRect, MapTexture, ScaleMode.StretchToFill);
			}
			else
			{
				GUI.DrawTexture(TextureRect, MapTexture);
			}

			if (overlay_static != null)
			{
				GUI.DrawTexture(TextureRect, overlay_static, ScaleMode.StretchToFill);
			}

			//Add the North/South labels to the polar projection
			if (bigmap.projection == SCANmap.MapProjection.Polar)
			{
				rc.x = TextureRect.x + TextureRect.width / 2 - TextureRect.width / 8;
				rc.y = TextureRect.y + TextureRect.height / 8;
				SCANuiUtil.drawLabel(rc, "S", false, true, true);
				rc.x = TextureRect.x + TextureRect.width / 2 + TextureRect.width / 8;
				SCANuiUtil.drawLabel(rc, "N", false, true, true);
			}

			//Draw the orbit overlays
			if (SCANcontroller.controller.map_orbit)
			{
				SCANuiUtil.drawOrbit(TextureRect, bigmap, v, startUT, overlay_static);
			}
		}

		//Draw the altitude legend bar along the bottom
		private void legendBar(int id)
		{
			growS();
			float mx = Event.current.mousePosition.x - TextureRect.x;
			float my = Event.current.mousePosition.y - TextureRect.y;
			bool in_map = false;//, in_spotmap = false;
			double mlon = 0, mlat = 0;

			if (mx >= 0 && my >= 0 && mx < MapTexture.width && my < MapTexture.height)
			{
				double mlo = (mx * 360f / MapTexture.width) - 180;
				double mla = 90 - (my * 180f / MapTexture.height);
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
				SCANuiUtil.drawLegend(data);
			stopS();
		}

		//Draw the map overlay labels
		private void mapLabels(int id)
		{
			SCANuiUtil.drawMapLabels(TextureRect, null, bigmap, data, b);
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
				ddRect = new Rect(WindowRect.width - 290, 45, 120, 160);
				GUI.Box(ddRect, "", SCANskins.SCAN_dropDownBox);
				for (int i = 0; i < SCANcontroller.controller.ResourceList.Count; i++)
				{
					scrollR = GUI.BeginScrollView(ddRect, scrollR, new Rect(0, 0, 100, 20 * SCANcontroller.controller.ResourceList.Count));
					Rect r = new Rect(2, 20 * i, 96, 20);
					if (GUI.Button(r, SCANcontroller.controller.ResourceList.ElementAt(i).Key, SCANskins.SCAN_dropDownButton))
					{
						bigmap.resource = SCANcontroller.controller.ResourceList.ElementAt(i).Value;
						SCANcontroller.controller.resourceSelection = bigmap.resource.Name;
						if (SCANcontroller.controller.ResourceList.ElementAt(i).Value.Source == SCANdata.SCANResource_Source.Kethane)
							SCANcontroller.controller.resourceOverlayType = 1;
						else
							SCANcontroller.controller.resourceOverlayType = 0;
						drop_down_open = false;
					}
					GUI.EndScrollView();
				}
			}

			//else if (planetoid_drop_down)
			//{
			//	int j = 0;
			//	ddRect = new Rect(WindowRect.width - 140, 45, 100, 160);
			//	GUI.Box(ddRect, "", SCANskins.SCAN_dropDownBox);
			//	for (int i = 0; i < FlightGlobals.Bodies.Count; i++)
			//	{
			//		scrollP = GUI.BeginScrollView(ddRect, scrollP, new Rect(0, 0, 80, (20 * SCANcontroller.Body_Data.Count) + 1));
			//		if (SCANcontroller.Body_Data.ContainsKey(FlightGlobals.Bodies[i].name))
			//		{
			//			Rect r = new Rect(2, 20 * j, 76, 20);
			//			if (GUI.Button(r, FlightGlobals.Bodies[i].name, SCANskins.SCAN_dropDownButton))
			//			{
			//				CelestialBody newB = FlightGlobals.Bodies[i];
			//				SCANdata newData = SCANUtil.getData(newB);
			//				if (newData != null)
			//				{
			//					data = newData;
			//					b = newB;
			//					bigmap.setBody(b);
			//				}
			//				drop_down_open = false;
			//			}
			//			j++;
			//		}
			//		GUI.EndScrollView();
			//	}
			//}
			else
				drop_down_open = false;

		}

	}
}
