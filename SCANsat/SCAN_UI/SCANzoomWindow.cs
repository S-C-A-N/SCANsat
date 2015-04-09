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
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Map;
using UnityEngine;

namespace SCANsat.SCAN_UI
{
	class SCANzoomWindow : SCAN_MBW
	{
		private SCANmap spotmap;
		private CelestialBody b;
		private SCANdata data;
		private Vessel v;
		private bool showOrbit, showAnomaly;
		private Vector2 dragStart, minSize, MaxSize;
		private float resizeW, resizeH;
		internal static Rect defaultRect = new Rect(10f, 10f, 340f, 260f);

		protected override void Awake()
		{
			SCANUtil.SCANdebugLog("Awake SCAN Zoom Window");
			WindowRect = defaultRect;
			minSize = new Vector2(310, 180);
			MaxSize = new Vector2(540, 400);
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(340), GUILayout.Height(260) };
			WindowStyle = SCANskins.SCAN_window;
			Visible = false;
			DragEnabled = true;
			ClampEnabled = true;
			ResizeEnabled = false;
			TooltipMouseOffset = new Vector2d(-10, -25);
			ClampToScreenOffset = new RectOffset(-200, -200, -160, -160);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
			SCAN_SkinsLibrary.SetCurrentTooltip();

			Startup();
		}

		private void Startup()
		{
			SCANUtil.SCANdebugLog("Start SCAN Zoom Window");
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
				spotmap = new SCANmap();
				spotmap.setSize(320, 240);
			}

			showOrbit = SCANcontroller.controller.map_orbit;
			showAnomaly = SCANcontroller.controller.map_markers;

			TooltipsEnabled = SCANcontroller.controller.toolTips;

			spotmap.setBody(b);
		}

		protected override void Start()
		{
			//SCANUtil.SCANdebugLog("Start SCAN Zoom Window");
			////Initialize the map object
			//Visible = false;
			//if (HighLogic.LoadedSceneIsFlight)
			//{
			//	v = SCANcontroller.controller.BigMap.V;
			//	b = SCANcontroller.controller.BigMap.Body;
			//	data = SCANcontroller.controller.BigMap.Data;
			//}
			//else if (HighLogic.LoadedSceneHasPlanetarium)
			//{
			//	v = null;
			//	b = SCANcontroller.controller.kscMap.Body;
			//	data = SCANcontroller.controller.kscMap.Data;
			//}
			//if (spotmap == null)
			//{
			//	spotmap = new SCANmap();
			//	spotmap.setSize(180, 180);
			//}

			//showOrbit = SCANcontroller.controller.map_orbit;
			//showAnomaly = SCANcontroller.controller.map_markers;

			//TooltipsEnabled = SCANcontroller.controller.toolTips;

			//spotmap.setBody(b);
		}

		public void setMapCenter(double lat, double lon, mapType t)
		{
			SCANUtil.SCANdebugLog("Centering Zoom Window");
			Visible = true;
			spotmap.MapScale = 10;
			spotmap.centerAround(lon, lat);
			spotmap.resetMap(t, false);
		}

		protected override void DrawWindowPre(int id)
		{
			WindowCaption = SCANuiUtil.toDMS(spotmap.CenteredLat, spotmap.CenteredLong);

			//Re-sizing code; moved here from SCAN_MBW
			if (IsResizing && !inRepaint())
			{
				if (Input.GetMouseButtonUp(0))
				{
					double scale = spotmap.MapScale;
					IsResizing = false;
					if (resizeW < minSize.x)
						resizeW = minSize.x;
					else if (resizeW > MaxSize.x)
						resizeW = MaxSize.x;
					if (resizeH < minSize.y)
						resizeH = minSize.y;
					else if (resizeH > MaxSize.y)
						resizeH = MaxSize.y;

					spotmap.setSize((int)resizeW, (int)resizeH);
					spotmap.MapScale = scale;
					spotmap.resetMap(spotmap.MType, false);
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
		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);
			closeBox(id);

			growS();
				topBar(id);
				drawMap(id);
				mouseOver(id);
			stopS();

			mapLabels(id);
		}

		protected override void DrawWindowPost(int id)
		{
			
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
			Rect r = new Rect(WindowRect.width - 20, 0, 18, 18);
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				Visible = false;
			}
		}

		private void topBar(int id)
		{
			growE();
			showOrbit = GUILayout.Toggle(showOrbit, textWithTT("", "Toggle Orbit"));

			Rect d = GUILayoutUtility.GetLastRect();
			d.x += 30;
			d.y += 2;
			d.width = 40;
			d.height = 20;

			GUILayout.Label("", GUILayout.Width(30));

			if (GUI.Button(d, iconWithTT(SCANskins.SCAN_OrbitIcon, "Toggle Orbit"), SCANskins.SCAN_buttonBorderless))
			{
				showOrbit = !showOrbit;
			}

			GUILayout.FlexibleSpace();

			if (GUILayout.Button(iconWithTT(SCANskins.SCAN_FlagIcon, "Zoom Out"), SCANskins.SCAN_windowButton, GUILayout.Width(24), GUILayout.Height(24)))
			{
				spotmap.MapScale = spotmap.MapScale / 1.25f;
				if (spotmap.MapScale < 2)
					spotmap.MapScale = 2;
				spotmap.resetMap(); /* Add map type */
			}

			GUILayout.Label(spotmap.MapScale.ToString("N1") + " X", SCANskins.SCAN_whiteLabelCenter, GUILayout.Width(40), GUILayout.Height(24));

			if (GUILayout.Button(iconWithTT(SCANskins.SCAN_FlagIcon, "Zoom In"), SCANskins.SCAN_windowButton, GUILayout.Width(24), GUILayout.Height(24)))
			{
				spotmap.MapScale = spotmap.MapScale * 1.25f;
				spotmap.resetMap(); /* Add map type */
			}

			GUILayout.FlexibleSpace();

			showAnomaly = GUILayout.Toggle(showAnomaly, textWithTT("", "Toggle Anomalies"));

			d = GUILayoutUtility.GetLastRect();
			d.x += 32;
			d.y += 2;
			d.width = 20;
			d.height = 20;

			if (GUI.Button(d, textWithTT(SCANcontroller.controller.anomalyMarker, "Toggle Anomalies"), SCANskins.SCAN_buttonBorderless))
			{
				showAnomaly = !showAnomaly;
			}

			GUILayout.Space(30);

			stopE();

			//d.x = 15;
			//d.y = WindowRect.height - 28;
			//d.width = 24;
			//d.height = 24;

			//if (GUI.Button(d, iconWithTT(SCANskins.SCAN_ScreenshotIcon, "Export Map"), SCANskins.SCAN_windowButton))
			//{
			//	if (spotmap.isMapComplete())
			//		spotmap.exportPNG();
			//}
		}

		private void drawMap(int id)
		{
			MapTexture = spotmap.getPartialMap();

			//A blank label used as a template for the actual map texture
			if (IsResizing)
			{
				//Set minimum map size during re-sizing
				dW = resizeW;
				if (dW < minSize.x)
					dW = minSize.x;
				else if (dW > MaxSize.x)
					dW = MaxSize.x;
				dH = resizeH;
				if (dH < minSize.y)
					dH = minSize.y;
				else if (dH > MaxSize.y)
					dH = MaxSize.y;

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
				in_map = true;
				double mlo = spotmap.Lon_Offset + (mx / spotmap.MapScale) - 180;
				double mla = spotmap.Lat_Offset + ((TextureRect.height - my) / spotmap.MapScale) - 90;
				mlon = spotmap.unprojectLongitude(mlo, mla);
				mlat = spotmap.unprojectLatitude(mlo, mla);

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

			//Handles mouse click while inside map; opens zoom map or zooms in further
			if (Event.current.isMouse)
			{
				if (Event.current.type == EventType.MouseUp)
				{
					//Middle click re-center
					if (Event.current.button == 2 || (Event.current.button == 1 && GameSettings.MODIFIER_KEY.GetKey()))
					{
						if (in_map)
						{
							if (spotmap.isMapComplete())
							{
								spotmap.centerAround(mlon, mlat);
								spotmap.resetMap(spotmap.MType, false);
								Event.current.Use();
							}
						}
					}
					//Right click zoom in
					else if (Event.current.button == 1)
					{
						if (in_map)
						{
							if (spotmap.isMapComplete())
							{
								spotmap.MapScale = spotmap.MapScale * 1.25f;
								spotmap.centerAround(mlon, mlat);
								spotmap.resetMap(spotmap.MType, false);
							}
						}
					}
					//Left click zoom out
					else if (Event.current.button == 0)
					{
						if (in_map)
						{
							if (spotmap.isMapComplete())
							{
								spotmap.MapScale = spotmap.MapScale / 1.25f;
								if (spotmap.MapScale < 2)
									spotmap.MapScale = 2;
								spotmap.centerAround(mlon, mlat);
								spotmap.resetMap(spotmap.MType, false);
								Event.current.Use();
							}
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
			SCANuiUtil.mouseOverInfoSimple(mlon, mlat, spotmap, data, b, in_map);
		}

		private void mapLabels(int id)
		{
			//Draw the orbit overlays
			if (showOrbit)
			{
				SCANuiUtil.drawOrbit(TextureRect, spotmap, v, b);
			}

			SCANuiUtil.drawMapLabels(TextureRect, v, spotmap, data, b, showAnomaly);
		}

	}
}
