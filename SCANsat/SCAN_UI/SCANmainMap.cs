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

using System.Text;
using SCANsat.SCAN_Platform;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_UI.UI_Framework;
using UnityEngine;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat.SCAN_UI
{
	class SCANmainMap: SCAN_MBW
	{
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
		private Color32[] cols_height_map_small;
		private Color32[] biomeCache;
		private bool biomeBuilding;
		private bool drawBiome;
		private double[] biomeIndex = new double[360];
		private int scanline;
		private int scanstep;
		private StringBuilder vesselText;
		private StringBuilder infoText;

		protected override void Awake()
		{
			base.Awake();

			WindowCaption = "S.C.A.N. Planetary Mapping";
			WindowRect = sessionRect;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(380), GUILayout.Height(230) };
			WindowStyle = SCANskins.SCAN_window;
			Visible = false;
			DragEnabled = true;
			TooltipMouseOffset = new Vector2d(-10, -25);
			ClampToScreenOffset = new RectOffset(-300, -300, -200, -200);
			infoText = new StringBuilder();
			vesselText = new StringBuilder();
			map_small = new Texture2D(360, 180, TextureFormat.ARGB32, false);
			cols_height_map_small = new Color32[360];
			biomeCache = new Color32[360 * 180];

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
			SCAN_SkinsLibrary.SetCurrentTooltip();
		}

		protected override void Start()
		{
			base.Start();

			Visible = SCANcontroller.controller.mainMapVisibleOld;
			v = FlightGlobals.ActiveVessel;
			data = SCANUtil.getData(v.mainBody);
			if (data == null)
			{
				data = new SCANdata(v.mainBody);
				SCANcontroller.controller.addToBodyData(v.mainBody, data);
			}
			TooltipsEnabled = SCANcontroller.controller.toolTips;

			if (palette.small_redline == null)
			{
				palette.small_redline = new Color32[360];
				for (int i = 0; i < 360; i++)
					palette.small_redline[i] = palette.Red;
			}

			resetImages();
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
				fillS(-6);
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
			Rect r = new Rect(WindowRect.width - 90, 2, 50, 18);

			if (GUI.Button(r, drawBiome ? "Biome" : "Terrain", SCANskins.SCAN_buttonBorderlessSmall))
			{
				drawBiome = !drawBiome;
				resetImages();
			}
			r.x += 50;
			r.y -= 2;
			r.width = 18;
			if (GUI.Button(r, showVesselInfo ? "-" : "+", SCANskins.SCAN_buttonBorderless))
				showVesselInfo = !showVesselInfo;
			r.x += 20;
			r.y += 1;
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				Visible = false;
				SCANcontroller.controller.mainMapVisibleOld = Visible;
			}
		}

		//Draw the map texture
		private void mainMap(int id)
		{
			mapRect = new Rect(10, 20, 360, 180);
			GUI.DrawTexture(mapRect, drawBiome ? drawBiomeMap(sensors) : drawPartialMap(sensors));

			if (data.MapBuilding || data.OverlayBuilding || data.ControllerBuilding)
			{
				flash = (int)(Time.realtimeSinceStartup % 2) == 0;
				SCANuiUtil.drawLabel(new Rect(mapRect.x + 80, mapRect.y + 50, 200, 60), "Building Database...", SCANskins.SCAN_insColorLabel, true, SCANskins.SCAN_insColorLabelShadow, flash, SCANskins.SCAN_insWhiteLabel);
			}

			GUILayout.Space(184);
		}

		//Draw the active scanner display
		private void scannerInfo(int id)
		{
			if (Event.current.type == EventType.Layout)
				SCANuiUtil.InfoText(v, data, notMappingToday, ref infoText);
			
			SCANuiUtil.readableLabel(infoText.ToString(), false);
		}

		//Draw the SCANsat window buttons with icons
		private void windowButtons(int id)
		{
			if (GUILayout.Button(iconWithTT(SCANskins.SCAN_BigMapIcon, "Big Map"), SCANskins.SCAN_windowButton, GUILayout.Height(32), GUILayout.Width(32)))
			{
				SCANcontroller.controller.BigMap.Visible = !SCANcontroller.controller.BigMap.Visible;
				SCANcontroller.controller.bigMapVisibleOld = !SCANcontroller.controller.bigMapVisibleOld;
			}
			if (GUILayout.Button(iconWithTT(SCANskins.SCAN_InstrumentIcon, "Instrument Window"), SCANskins.SCAN_windowButton, GUILayout.Height(32), GUILayout.Width(32)))
			{
				SCANcontroller.controller.instrumentsWindow.Visible = !SCANcontroller.controller.instrumentsWindow.Visible;
			}
			if (GUILayout.Button(iconWithTT(SCANskins.SCAN_ZoomMapIcon, "Zoom Map"), SCANskins.SCAN_windowButton, GUILayout.Height(32), GUILayout.Width(32)))
			{
				SCANcontroller.controller.zoomMap.Visible = !SCANcontroller.controller.zoomMap.Visible;
				if (SCANcontroller.controller.zoomMap.Visible && !SCANcontroller.controller.zoomMap.Initialized)
					SCANcontroller.controller.zoomMap.initializeMap();
			}
			if (GUILayout.Button(iconWithTT(SCANskins.SCAN_OverlayIcon, "Overlay Contral"), SCANskins.SCAN_windowButton, GUILayout.Height(32), GUILayout.Width(32)))
			{
				SCANcontroller.controller.resourceOverlay.Visible = !SCANcontroller.controller.resourceOverlay.Visible;
			}
			if (GUILayout.Button(iconWithTT(SCANskins.SCAN_SettingsIcon, "Settings Menu"), SCANskins.SCAN_windowButton, GUILayout.Height(32), GUILayout.Width(32)))
			{
				SCANcontroller.controller.settingsWindow.Visible = !SCANcontroller.controller.settingsWindow.Visible;
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
					if (sV.vessel.mainBody != v.mainBody)
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

			if (!showVesselInfo)
			{
				SCANuiUtil.drawVesselLabel(r, null, -1, scanV);
				return true;
			}

			float lon = (float)SCANUtil.fixLonShift(scanV.longitude);
			float lat = (float)SCANUtil.fixLatShift(scanV.latitude);

			string units = "";
			if (drawBiome)
			{
				if (SCANUtil.isCovered(lon, lat, data, SCANtype.Biome))
					units = string.Format("; {0}", SCANUtil.getBiomeName(data.Body, lon, lat));
			}
			else
			{
				if (SCANUtil.isCovered(lon, lat, data, SCANtype.Altimetry))
				{
					if (SCANUtil.isCovered(lon, lat, data, SCANtype.AltimetryHiRes))
					{
						float alt = scanV.heightFromTerrain;
						if (alt < 0)
							alt = (float)scanV.altitude;
						units = string.Format("; {0}", SCANuiUtil.distanceString(alt, 100000, 100000000));
					}
					else
					{
						float alt = scanV.heightFromTerrain;
						if (alt < 0)
							alt = (float)scanV.altitude;

						alt = ((int)(alt / 500)) * 500;
						units = string.Format("; {0}", SCANuiUtil.distanceString(alt, 100000, 100000000));
					}
				}
			}

			vesselText.Length = 0;
			vesselText.AppendFormat(string.Format("[{0}] {1} ({2:F1}°,{3:F1}°{4})", i, !string.IsNullOrEmpty(scanV.vesselName) && scanV.vesselName.Length > 26 ? scanV.vesselName.Substring(0, 26) : scanV.vesselName, lat, lon, units));

			if (SCANuiUtil.readableLabel(vesselText.ToString(), b))
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

		private Texture2D drawPartialMap(SCANtype type)
		{
			bool pqsController = data.Body.pqsController != null;

			if (data.ControllerBuilding || data.OverlayBuilding)
			{
				return map_small;
			}

			if (!data.Built)
			{
				if (!data.MapBuilding)
				{
					scanline = 0;
					scanstep = 0;
				}

				data.MapBuilding = true;
				data.generateHeightMap(ref scanline, ref scanstep, 360);
				return map_small;
			}

			int scheme = SCANcontroller.controller.colours;

			for (int ilon = 0; ilon < 360; ilon++)
			{
				if (!pqsController)
				{
					cols_height_map_small[ilon] = palette.lerp(palette.black, palette.white, UnityEngine.Random.value);
					continue;
				}

				Color32 c = palette.Grey;
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
						c = palette.White;
					}
					else if (ilon % 30 == 0 && scanline % 3 == 0)
					{
						c = palette.White;
					}
				}

				if (type != SCANtype.Nothing)
				{
					if (!SCANUtil.isCoveredByAll(ilon, scanline, data, type))
					{
						c = palette.lerp(c, palette.Black, 0.5f);
					}
				}

				cols_height_map_small[ilon] = c;
			}

			map_small.SetPixels32(0, scanline, 360, 1, cols_height_map_small);

			if (scanline < 179)
				map_small.SetPixels32(0, scanline + 1, 360, 1, palette.small_redline);

			scanline++;

			if (scanline >= 180)
				scanline = 0;

			map_small.Apply();

			return map_small;
		}

		private Texture2D drawBiomeMap(SCANtype type)
		{
			bool biomeMap = data.Body.BiomeMap != null;

			if (biomeBuilding)
				buildBiomeCache();

			int scheme = SCANcontroller.controller.colours;

			for (int ilon = 0; ilon < 360; ilon++)
			{
				if (!biomeMap)
				{
					cols_height_map_small[ilon] = palette.lerp(palette.black, palette.white, UnityEngine.Random.value);
					continue;
				}

				Color32 c = biomeCache[scanline * 360 + ilon];
				if (!SCANUtil.isCovered(ilon, scanline, data, SCANtype.Biome))
				{
					c = palette.Grey;
				}

				if (type != SCANtype.Nothing)
				{
					if (!SCANUtil.isCoveredByAll(ilon, scanline, data, type))
					{
						c = palette.lerp(c, palette.Black, 0.5f);
					}
				}

				cols_height_map_small[ilon] = c;
			}

			map_small.SetPixels32(0, scanline, 360, 1, cols_height_map_small);

			if (scanline < 179)
				map_small.SetPixels32(0, scanline + 1, 360, 1, palette.small_redline);

			scanline++;

			if (scanline >= 180)
				scanline = 0;

			map_small.Apply();

			return map_small;
		}

		private void buildBiomeCache()
		{
			for (int i = 0; i < 360; i++)
			{
				double index = SCANUtil.getBiomeIndexFraction(data.Body, i - 180, scanline - 90);
				Color32 c = palette.Grey;

				if (SCANcontroller.controller.biomeBorder && ((i > 0 && biomeIndex[i - 1] != index) || (scanline > 0 && biomeIndex[i] != index)))
				{
					c = palette.White;
				}
				else if (SCANcontroller.controller.useStockBiomes)
				{
					c = SCANUtil.getBiome(data.Body, i - 180, scanline - 90).mapColor;
				}
				else
				{
					c = palette.lerp(SCANcontroller.controller.lowBiomeColor32, SCANcontroller.controller.highBiomeColor32, (float)index);
				}

				biomeCache[scanline * 360 + i] = c;

				biomeIndex[i] = index;
			}

			if (scanline >= 179)
				biomeBuilding = false;
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
			if (drawBiome)
			{
				biomeBuilding = true;
				scanline = 0;
			}
		}

	}
}
