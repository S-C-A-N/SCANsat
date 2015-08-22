#region license
/* 
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - Main map window object
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 *
 */
#endregion

using SCANsat.SCAN_Platform;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_UI.UI_Framework;
using UnityEngine;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat.SCAN_UI
{
	class SCANmainMap: SCAN_MBW
	{
		private string infoText;
		private Vessel v;
		private SCANdata data;
		private SCANtype sensors;
		private bool notMappingToday; //Unused out-of-power bool
		private Rect mapRect;
		private static bool showVesselInfo = true;
		internal static readonly Rect defaultRect = new Rect(10, 55, 380, 230);
		private static Rect sessionRect = defaultRect;
		private bool flash;
		private Texture2D map_small;
		private Color[] cols_height_map_small;
		private int scanline;
		private int scanstep;

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Planetary Mapping";
			WindowRect = sessionRect;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(380), GUILayout.Height(230) };
			WindowStyle = SCANskins.SCAN_window;
			Visible = false;
			DragEnabled = true;
			TooltipMouseOffset = new Vector2d(-10, -25);
			ClampToScreenOffset = new RectOffset(-300, -300, -200, -200);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
			SCAN_SkinsLibrary.SetCurrentTooltip();
		}

		protected override void Start()
		{
			Visible = SCANcontroller.controller.mainMapVisible;
			v = FlightGlobals.ActiveVessel;
			data = SCANUtil.getData(v.mainBody);
			if (data == null)
			{
				data = new SCANdata(v.mainBody);
				SCANcontroller.controller.addToBodyData(v.mainBody, data);
			}
			TooltipsEnabled = SCANcontroller.controller.toolTips;
		}

		protected override void DrawWindowPre(int id)
		{
			//Keep the map updated with the current vessel location and status
			v = FlightGlobals.ActiveVessel;
			data = SCANUtil.getData(v.mainBody);
			if (data == null)
			{
				data = new SCANdata(v.mainBody);
				SCANcontroller.controller.addToBodyData(v.mainBody, data);
			}
			sensors = SCANcontroller.controller.activeSensorsOnVessel(v.id);
		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);				/* Standard version label and close button */
			topMenu(id);
			growS();
				mainMap(id);				/* Draws the main map texture */
				growE();
					scannerInfo(id);		/* Draws the scanner indicators */
					windowButtons(id);		/* Draw the buttons for other SCANsat windows */
				stopE();
				fillS(-8);
				vesselInfo(id);				/* Shows info for any SCANsat vessels */
			stopS();
		}

		protected override void DrawWindowPost(int id)
		{
			sessionRect = WindowRect;
		}

		//Print the version number
		private void versionLabel(int id)
		{
			Rect r = new Rect(6, 0, 50, 18);
			GUI.Label(r, SCANmainMenuLoader.SCANsatVersion, SCANskins.SCAN_whiteReadoutLabel);
		}

		//Draw the top menu items
		private void topMenu(int id)
		{
			Rect r = new Rect(WindowRect.width - 40, 0, 18, 18);
			if (showVesselInfo)
			{
				if (GUI.Button(r, "-", SCANskins.SCAN_buttonBorderless))
					showVesselInfo = !showVesselInfo;
			}
			else
			{
				if (GUI.Button(r, "+", SCANskins.SCAN_buttonBorderless))
					showVesselInfo = !showVesselInfo;
			}
			r.x += 20;
			r.y += 1;
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				Visible = false;
				SCANcontroller.controller.mainMapVisible = Visible;
			}
		}

		//Draw the map texture
		private void mainMap(int id)
		{
			mapRect = new Rect(10, 20, 360, 180);
			GUI.DrawTexture(mapRect, drawPartialMap(sensors));

			if (data.Building || data.ExternalBuilding)
			{
				flash = (int)(Time.realtimeSinceStartup % 2) == 0;
				SCANuiUtil.drawLabel(new Rect(mapRect.x + 80, mapRect.y + 50, 200, 60), "Building Database...", SCANskins.SCAN_insColorLabel, true, SCANskins.SCAN_insColorLabelShadow, flash, SCANskins.SCAN_insWhiteLabel);
			}

			GUILayout.Space(184);
		}

		//Draw the active scanner display
		private void scannerInfo(int id)
		{
			bool repainting = Event.current.type == EventType.Repaint;
			if (!repainting)
				infoText = SCANuiUtil.InfoText(v, data, notMappingToday);

			if (infoText != null)
				SCANuiUtil.readableLabel(infoText, false);
		}

		//Draw the SCANsat window buttons with icons
		private void windowButtons(int id)
		{
			//fillS();
			if (GUILayout.Button(iconWithTT(SCANskins.SCAN_BigMapIcon, "Big Map"), SCANskins.SCAN_windowButton, GUILayout.Height(32), GUILayout.Width(32)))
			{
				SCANcontroller.controller.BigMap.Visible = !SCANcontroller.controller.BigMap.Visible;
				SCANcontroller.controller.bigMapVisible = !SCANcontroller.controller.bigMapVisible;
			}
			if (GUILayout.Button(iconWithTT(SCANskins.SCAN_InstrumentIcon, "Instrument Window"), SCANskins.SCAN_windowButton, GUILayout.Height(32), GUILayout.Width(32)))
			{
				SCANcontroller.controller.instrumentsWindow.Visible = !SCANcontroller.controller.instrumentsWindow.Visible;
			}
			if (GUILayout.Button(iconWithTT(SCANskins.SCAN_SettingsIcon, "Settings Menu"), SCANskins.SCAN_windowButton, GUILayout.Height(32), GUILayout.Width(32)))
			{
				SCANcontroller.controller.settingsWindow.Visible = !SCANcontroller.controller.settingsWindow.Visible;
			}
			if (GUILayout.Button(iconWithTT(SCANskins.SCAN_ColorIcon, "Color Control"), SCANskins.SCAN_windowButton, GUILayout.Height(32), GUILayout.Width(32)))
			{
				SCANcontroller.controller.colorManager.Visible = !SCANcontroller.controller.colorManager.Visible;
			}
			if (GUILayout.Button(iconWithTT(SCANskins.SCAN_OverlayIcon, "Overlay Contral"), SCANskins.SCAN_windowButton, GUILayout.Height(32), GUILayout.Width(32)))
			{
				SCANcontroller.controller.resourceOverlay.Visible = !SCANcontroller.controller.resourceOverlay.Visible;
			}
		}

		//Draw the vessel location and alt info
		private void vesselInfo(int id)
		{
			if (!notMappingToday)
			{
				int count = 2;
				vesselInfo(v, mapRect, 1, true);
				foreach (SCANcontroller.SCANvessel sV in SCANcontroller.controller.Known_Vessels)
				{
					if (sV.vessel == FlightGlobals.ActiveVessel)
						continue;
					if (vesselInfo(sV.vessel, mapRect, count, false))
						count++;
				}
			}
		}

		//Method to handle vessel info
		private bool vesselInfo(Vessel scanV, Rect r, int i, bool b)
		{
			if (scanV == null)
				return false;
			if (scanV.mainBody == v.mainBody)
			{
				if (!showVesselInfo)
				{
					SCANuiUtil.drawVesselLabel(r, null, -1, scanV);
					return true;
				}
				float lon = (float)SCANUtil.fixLonShift(scanV.longitude);
				float lat = (float)SCANUtil.fixLatShift(scanV.latitude);
				float alt = scanV.heightFromTerrain;
				if (alt < 0)
					alt = (float)scanV.altitude;
				string text = string.Format("[{0}] {1} ({2:F1}°,{3:F1}°; {4:N1}m)", i, scanV.vesselName, lat, lon, alt);
				if (SCANuiUtil.readableLabel(text, b))
				{
					if (Event.current.clickCount > 1)
					{
						Event.current.Use();
						FlightGlobals.SetActiveVessel(scanV);
						ScreenMessages.PostScreenMessage(scanV.vesselName, 5, ScreenMessageStyle.UPPER_CENTER);
					}
				}
				SCANuiUtil.drawVesselLabel(r, null, i, scanV);
				fillS(-10);
				return true;
			}
			return false;
		}

		internal Texture2D drawPartialMap(SCANtype type)
		{
			if (map_small == null)
			{
				map_small = new Texture2D(360, 180, TextureFormat.ARGB32, false);
				resetImages();
			}

			if (cols_height_map_small == null)
			{
				cols_height_map_small = new Color[360];
			}

			if (data.ExternalBuilding)
			{
				return map_small;
			}

			if (!data.Built)
			{
				data.Building = true;
				data.generateHeightMap(ref scanline, ref scanstep, 360);
				return map_small;
			}

			if (palette.small_redline == null)
			{
				palette.small_redline = new Color[360];
				for (int i = 0; i < 360; i++)
					palette.small_redline[i] = palette.red;
			}

			int scheme = SCANcontroller.controller.colours;

			for (int ilon = 0; ilon < 360; ilon++)
			{
				if (data.Body.pqsController == null)
				{
					cols_height_map_small[ilon] = palette.lerp(palette.black, palette.white, UnityEngine.Random.value);
					continue;
				}

				Color c = palette.grey;
				float val = data.HeightMapValue(data.Body.flightGlobalsIndex, ilon, scanline);
				if (SCANUtil.isCovered(ilon, scanline, data, SCANtype.Altimetry))
				{
					if (SCANUtil.isCovered(ilon, scanline, data, SCANtype.AltimetryHiRes))
						c = palette.heightToColor(val, scheme, data.TerrainConfig);
					else
						c = palette.heightToColor(val, 1, data.TerrainConfig);
				}
				else
				{
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
					if (!SCANUtil.isCoveredByAll(ilon, scanline, data, type))
					{
						c = palette.lerp(c, palette.black, 0.5f);
					}
				}

				cols_height_map_small[ilon] = c;
			}

			map_small.SetPixels(0, scanline, 360, 1, cols_height_map_small);

			if (scanline < 179)
				map_small.SetPixels(0, scanline + 1, 360, 1, palette.small_redline);

			scanline++;

			if (scanline >= 180)
				scanline = 0;

			map_small.Apply();

			return map_small;
		}

		public Texture2D Map
		{
			get { return map_small; }
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

	}
}
