

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
		private SCANmap bigmap; //, spotmap;
		private CelestialBody b;
		private SCANdata data;
		//private double startUT;
		private bool drawGrid;
		private bool drop_down_open, projection_drop_down, mapType_drop_down, resources_drop_down, planetoid_drop_down;
		private Texture2D overlay_static, map;
		private Rect ddRect, maprect;
		//private Rect rc = new Rect(0, 0, 0, 0);
		//private Rect pos_spotmap = new Rect(10f, 10f, 10f, 10f);
		//private Rect pos_spotmap_x = new Rect(10f, 10f, 25f, 25f);

		protected override void Awake()
		{
			WindowCaption = "Map of ";
			WindowRect = new Rect(250, 120, 740, 420);
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(740), GUILayout.Height(420) };
			WindowStyle = SCANskins.SCAN_window;
			Visible = true;
			DragEnabled = true;

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		internal override void Start()
		{
			//Initialize the map object
			b = Planetarium.fetch.Home;
			if (bigmap == null)
			{
				bigmap = new SCANmap(b);
				bigmap.setProjection((SCANmap.MapProjection)SCANcontroller.controller.projection);
				bigmap.setWidth(720);
				WindowRect.x = SCANcontroller.controller.map_x;
				WindowRect.y = SCANcontroller.controller.map_y;
			}
			else
			{
				SCANcontroller.controller.map_x = (int)WindowRect.x;
				SCANcontroller.controller.map_y = (int)WindowRect.y;
			}
			WindowCaption = string.Format("Map of {0}", b.theName);
			bigmap.setBody(b);
		}

		internal override void OnDestroy()
		{
			
		}

		protected override void DrawWindowPre(int id)
		{
			WindowCaption = string.Format("Map of {0}", b.theName);
			data = SCANUtil.getData(b);

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

			topMenu(id);

			growE();

			toggleBar(id);

			mapDraw(id);

			stopE();

			legendBar(id);

			stopS();

			if (drop_down_open)
				dropDown(id);
		}

		protected override void DrawWindowPost(int id)
		{
			//Close the drop down menu if the window is clicked anywhere else
			if (drop_down_open && Event.current.type == EventType.mouseDown && !ddRect.Contains(Event.current.mousePosition))
				drop_down_open = false;
		}

		private void versionLabel(int id)
		{
			Rect r = new Rect(6, 0, 40, 18);
			GUI.Label(r, SCANversions.SCANsatVersion, SCANskins.SCAN_whiteLabel);
		}

		private void closeBox(int id)
		{
			Rect r = new Rect(WindowRect.width - 20, 0, 18, 18);
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				Visible = false;
				SCANUtil.SCANlog("Close KSC Map");
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

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Resources", SCANskins.SCAN_buttonFixed, GUILayout.MaxWidth(100)))
			{
				resources_drop_down = !resources_drop_down;
				drop_down_open = !drop_down_open;
			}

			fillS(40);

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

			if (GUILayout.Button("Res_Overlay", SCANskins.SCAN_buttonFixed))
			{
				SCANcontroller.controller.map_ResourceOverlay = !SCANcontroller.controller.map_ResourceOverlay;
				bigmap.resetMap();
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
				Rect rc = new Rect();
				rc.x = maprect.x + maprect.width / 2 - maprect.width / 8;
				rc.y = maprect.y + maprect.height / 8;
				SCANuiUtil.drawLabel(rc, "S", true, true, false);
				rc.x = maprect.x + maprect.width / 2 + maprect.width / 8;
				SCANuiUtil.drawLabel(rc, "N", true, true, false);
			}
		}

		//Draw the altitude legend bar along the bottom
		private void legendBar (int id)
		{
			growE();
			fillS(110);
			growS();
			SCANuiUtil.drawLegend(bigmap);
			float mx = Event.current.mousePosition.x - maprect.x;
			float my = Event.current.mousePosition.y - maprect.y;
			string info = SCANuiUtil.mouseOverInfo(mx, my, bigmap, map, data, maprect, b);
			SCANuiUtil.readableLabel(info, true);
			stopS();
			stopE();
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
				ddRect = new Rect(WindowRect.width - 130, 45, 100, FlightGlobals.Bodies.Count * 21);
				GUI.Box(ddRect, "", SCANskins.SCAN_dropDownBox);
				for (int i = 0; i < FlightGlobals.Bodies.Count; i++)
				{
					Rect r = new Rect(ddRect.x + 2, ddRect.y + (20 * i), ddRect.width - 4, 20);
					if (GUI.Button(r, FlightGlobals.Bodies[i].name, SCANskins.SCAN_dropDownButton))
					{
						b = FlightGlobals.Bodies[i];
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
