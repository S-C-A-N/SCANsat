#region license
/*  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANresourceSettings - Window for resource scanning options
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 DMagic
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

using System;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_UI.UI_Framework;
using SCANsat.SCAN_Platform;
using UnityEngine;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat.SCAN_UI
{
	class SCANresourceSettings : SCAN_MBW
	{
		internal readonly static Rect defaultRect = new Rect(300, 200, 300, 270);
		private static Rect sessionRect = defaultRect;
		private int mapHeight, biomeMapHeight;
		private float transparency;
		private int interpolationScale;
		private bool popup, warningResource, warningStockResource, warningMMLoaded, controlLock, oldNarrowBand, oldStockScanThreshold, oldInstantScan, oldDisableScan;
		private const string lockID = "resourceSettingLockID";
		private Rect warningRect;
		private string scanThreshold = "";
		private static bool MMWarned = false;

		private string resourceSettingsHelpOverlayWindow = "Open the planetary overlay map control window.";
		private string resourceSettingsHelpBiomeLock = "Circumvents the requirement for stock surface biome scans. SCANsat displays will show the full accuracy for resource abundance with or without any surface biome scans.";
		private string resourceSettingsHelpInstant = "By default, the stock M700 resource scanner's orbital survey will fill in all SCANsat resource maps. This can be disabled, requiring standard SCANsat methods for all resource scanning. Disabled automatically when stock resource scanning is disabled.";
		private string resourceSettingsHelpNarrowBand = "Numerous SCANsat functions require a Narrow-Band resource scanner on-board the current vessel or in orbit of a celestial body for fully accurate resource abundance data.\nDisable this to circumvent these restrictions.";
		private string resourceSettingsHelpDisableStock = "Disables all stock resource scanning functions. SCANsat scanning methods will be required for all resource data. Replaces several stock resource functions with SCANsat tools. These include The right-click readouts, the high resolution narrow-band scanner map, and the planetary overlay maps.";
		private string resourceSettingsHelpResetSCANsatResource = "Resets all SCANsat resource data for the current celestial body. Other SCANsat data is not affected.\nA confirmation window will open before activating.\nCannot be reversed.";
		private string resourceSettingsHelpResetStockResource = "Resets the stock resource scanning coverage for the current celestial body. A reload or scene change may be required for all changes to take effect.\nA confirmation window will open before activating.\nCannot be reversed.";
		private string resourceSettingsHelpOverlayInterpolation = "Change the number of resource abundance measurements used in constructing the planetary overlay and big map resource overlay. Decrease the value to increase the accuracy of the map. Lower values will result in slower map generation.";
		private string resourceSettingsHelpOverlayHeight = "Change the texture size (map width is 2XHeight) used in constructing the planetary overlay and big map resource overlay. Increase the value to increase the quality and accuracy of the map. Higher values will result in slower map generation.";
		private string resourceSettingsHelpOverlayBiomeHeight = "Change the texture size (map width is 2XHeight) used in constructing the planetary overlay biome map. Increase the value to increase the quality and accuracy of the map. Higher values will result in slower map generation.";
		private string resourceSettingsHelpOverlayTransparency = "Create a grey background for planetary overlay resource maps. Used to make clear which sections of the celestial body have been\nscanned but contain no resources.";
		private string resourceSettingsHelpScanThreshold = "A threshold level used to apply the stock resource scan to a celestial body after scanning with SCANsat sensors. This is useful when contracts or other addons require that a stock resource scan be performed. Set a value from 0-100 in the text box and click on the Set button. All celestial bodies will be checked immediately; celestial bodies will also be checked upon loading or a scene change. A reload may be required for the changes to take effect.";
		private string MMwarning = "Warning:\nModule Manager is required for all SCANsat resource scanning";

		protected override void Awake()
		{
			base.Awake();

			WindowCaption = "S.C.A.N. Resources Settings";
			WindowRect = sessionRect;
			WindowStyle = SCANskins.SCAN_window;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(300), GUILayout.Height(270) };
			Visible = false;
			DragEnabled = true;
			TooltipMouseOffset = new Vector2d(-10, -25);
			TooltipMaxWidth = 350;
			TooltipDisplayForSecs = 60;
			ClampToScreenOffset = new RectOffset(-200, -200, -200, -200);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		protected override void Start()
		{
			oldNarrowBand = SCANcontroller.controller.needsNarrowBand;
			oldStockScanThreshold = SCANcontroller.controller.useScanThreshold;
			oldInstantScan = SCANcontroller.controller.easyModeScanning;
			oldDisableScan = SCANcontroller.controller.disableStockResource;

			biomeMapHeight = SCANcontroller.controller.overlayBiomeHeight;
			mapHeight = SCANcontroller.controller.overlayMapHeight;
			transparency = SCANcontroller.controller.overlayTransparency;
			interpolationScale = SCANcontroller.controller.overlayInterpolation;

			scanThreshold = (SCANcontroller.controller.scanThreshold * 100f).ToString("F0");

			TooltipsEnabled = false;

			if (SCANconfigLoader.languagePack != null)
				loadStrings();
		}

		private void loadStrings()
		{
			resourceSettingsHelpOverlayWindow = SCANconfigLoader.languagePack.resourceSettingsHelpOverlayWindow;
			resourceSettingsHelpBiomeLock = SCANconfigLoader.languagePack.resourceSettingsHelpBiomeLock;
			resourceSettingsHelpInstant = SCANconfigLoader.languagePack.resourceSettingsHelpInstant;
			resourceSettingsHelpNarrowBand = SCANconfigLoader.languagePack.resourceSettingsHelpNarrowBand;
			resourceSettingsHelpDisableStock = SCANconfigLoader.languagePack.resourceSettingsHelpDisableStock;
			resourceSettingsHelpResetSCANsatResource = SCANconfigLoader.languagePack.resourceSettingsHelpResetSCANsatResource;
			resourceSettingsHelpResetStockResource = SCANconfigLoader.languagePack.resourceSettingsHelpResetStockResource;
			resourceSettingsHelpOverlayInterpolation = SCANconfigLoader.languagePack.resourceSettingsHelpOverlayInterpolation;
			resourceSettingsHelpOverlayHeight = SCANconfigLoader.languagePack.resourceSettingsHelpOverlayHeight;
			resourceSettingsHelpOverlayBiomeHeight = SCANconfigLoader.languagePack.resourceSettingsHelpOverlayBiomeHeight;
			resourceSettingsHelpOverlayTransparency = SCANconfigLoader.languagePack.resourceSettingsHelpOverlayTransparency;
			resourceSettingsHelpScanThreshold = SCANconfigLoader.languagePack.resourceSettingsHelpScanThreshold;
			MMwarning = SCANconfigLoader.languagePack.resourceSettingsModuleManagerWarning;
		}

		internal void removeControlLocks()
		{
			InputLockManager.RemoveControlLock(lockID);
			controlLock = false;
		}

		protected override void DrawWindowPre(int id)
		{
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
					InputLockManager.RemoveControlLock(lockID);
					controlLock = false;
				}
			}

			if (!popup)
			{
				warningResource = false;
				warningStockResource = false;
			}
		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);				/* Standard version label and close button */
			closeBox(id);

			resourceController(id);
			resourceSettings(id);
			overlayOptions(id);

			warningBox(id);
		}

		protected override void DrawWindowPost(int id)
		{
			if (popup && Event.current.type == EventType.mouseDown && !warningRect.Contains(Event.current.mousePosition))
			{
				popup = false;
			}

			if (!MMWarned && !SCANmainMenuLoader.MMLoaded)
			{
				if (oldInstantScan != SCANcontroller.controller.easyModeScanning)
				{
					oldInstantScan = SCANcontroller.controller.easyModeScanning;
					MMWarned = true;
					popup = !popup;
					warningMMLoaded = !warningMMLoaded;
				}

				if (oldDisableScan != SCANcontroller.controller.disableStockResource)
				{
					oldDisableScan = SCANcontroller.controller.disableStockResource;
					MMWarned = true;
					popup = !popup;
					warningMMLoaded = !warningMMLoaded;
				}
			}

			if (oldNarrowBand != SCANcontroller.controller.needsNarrowBand)
			{
				oldNarrowBand = SCANcontroller.controller.needsNarrowBand;
				if (SCANcontroller.controller.instrumentsWindow != null && oldNarrowBand)
					SCANcontroller.controller.instrumentsWindow.resetResourceList();
			}

			if (oldStockScanThreshold != SCANcontroller.controller.useScanThreshold)
			{
				oldStockScanThreshold = SCANcontroller.controller.useScanThreshold;
				if (oldStockScanThreshold)
				{
					for (int i = 0; i < FlightGlobals.Bodies.Count; i++)
					{
						CelestialBody b = FlightGlobals.Bodies[i];

						SCANcontroller.controller.checkResourceScanStatus(b);
					}
				}
			}

			sessionRect = WindowRect;
		}

		//Draw the version label in the upper left corner
		private void versionLabel(int id)
		{
			Rect r = new Rect(4, 0, 50, 18);
			GUI.Label(r, SCANmainMenuLoader.SCANsatVersion, SCANskins.SCAN_whiteReadoutLabel);
		}

		//Draw the close button in the upper right corner
		private void closeBox(int id)
		{
			Rect r = new Rect(WindowRect.width - 42, 1, 18, 18);
			if (GUI.Button(r, textWithTT("?", "Show Help Tips"), SCANskins.SCAN_closeButton))
			{
				TooltipsEnabled = !TooltipsEnabled;
			}

			r.x += 22;
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				removeControlLocks();
				Visible = false;
				TooltipsEnabled = false;
			}
		}

		private void resourceController(int id)
		{
			if (SCANcontroller.controller.resourceOverlay == null)
				return;

			if (GUILayout.Button(textWithTT("Planetary Overlay Window", resourceSettingsHelpOverlayWindow)))
			{
				SCANcontroller.controller.resourceOverlay.Visible = !SCANcontroller.controller.resourceOverlay.Visible;
			}
		}

		private void resourceSettings(int id)
		{
			GUILayout.Label("Resource Settings", SCANskins.SCAN_headline);
			growE();
				SCANcontroller.controller.resourceBiomeLock = GUILayout.Toggle(SCANcontroller.controller.resourceBiomeLock, textWithTT("Resource Biome Lock", resourceSettingsHelpBiomeLock), SCANskins.SCAN_settingsToggle);
				if (SCANcontroller.controller.disableStockResource)
					GUILayout.Toggle(false, textWithTT("Instant Resource Scan", resourceSettingsHelpInstant + "[Disabled]"), SCANskins.SCAN_settingsToggle);
				else
					SCANcontroller.controller.easyModeScanning = GUILayout.Toggle(SCANcontroller.controller.easyModeScanning, textWithTT("Instant Resource Scan", resourceSettingsHelpInstant), SCANskins.SCAN_settingsToggle);
			stopE();
			growE();
				fillS();
				SCANcontroller.controller.needsNarrowBand = GUILayout.Toggle(SCANcontroller.controller.needsNarrowBand, textWithTT("Requires Narrow Band Scanner", resourceSettingsHelpNarrowBand), SCANskins.SCAN_settingsToggle);
				fillS();
			stopE();
			growE();
				fillS();
				SCANcontroller.controller.disableStockResource = GUILayout.Toggle(SCANcontroller.controller.disableStockResource, textWithTT("Disable Stock Scanning", resourceSettingsHelpDisableStock), SCANskins.SCAN_settingsToggle);
				fillS();
			stopE();
			if (SCANcontroller.controller.disableStockResource)
			{
				growE();
					fillS(20);
					SCANcontroller.controller.useScanThreshold = GUILayout.Toggle(SCANcontroller.controller.useScanThreshold, textWithTT("Stock Scan Threshold: " + SCANcontroller.controller.scanThreshold.ToString("P0"), resourceSettingsHelpScanThreshold), SCANskins.SCAN_settingsToggle, GUILayout.Width(190));

					if (SCANcontroller.controller.useScanThreshold)
					{
						scanThreshold = GUILayout.TextField(scanThreshold, 3, GUILayout.Width(40));

						Rect r = GUILayoutUtility.GetLastRect();

						GUI.Label(r, textWithTT("", resourceSettingsHelpScanThreshold));

						if (GUILayout.Button(textWithTT("Set", resourceSettingsHelpScanThreshold), GUILayout.Width(45)))
						{
							float f = 0;

							if (float.TryParse(scanThreshold, out f))
							{
								f /= 100;

								if (f <= 0f)
									f = 0;
								else if (f >= 1)
									f = 1;

								SCANcontroller.controller.scanThreshold = f;

								for (int i = 0; i < FlightGlobals.Bodies.Count; i++)
								{
									CelestialBody b = FlightGlobals.Bodies[i];

									SCANcontroller.controller.checkResourceScanStatus(b);
								}
							}
						}						
					}
					fillS();
				stopE();
			}
			GUILayout.Label("Resource Scan Data", SCANskins.SCAN_headline);
			if (popup)
			{
				GUILayout.Label("Reset SCANsat Resource Coverage", SCANskins.SCAN_button);
				if (SCANcontroller.controller.disableStockResource)
				{
					fillS(8);
					GUILayout.Label("Reset Stock Resource Scanning", SCANskins.SCAN_button);
				}
			}
			else
			{
				if (GUILayout.Button(textWithTT("Reset SCANsat Resource Coverage", resourceSettingsHelpResetSCANsatResource)))
				{
					popup = !popup;
					warningResource = !warningResource;
				}
				if (SCANcontroller.controller.disableStockResource)
				{
					fillS(8);
					if (GUILayout.Button(textWithTT("Reset Stock Resource Scanning", resourceSettingsHelpResetStockResource)))
					{
						popup = !popup;
						warningStockResource = !warningStockResource;
					}
				}
			}
		}

		private void overlayOptions(int id)
		{
			GUILayout.Label("Overlay Map Quality", SCANskins.SCAN_headline);
			growE();
				GUILayout.Label(textWithTT("Interpolation:", resourceSettingsHelpOverlayInterpolation), SCANskins.SCAN_labelSmallLeft);

				fillS();

				if (GUILayout.Button(textWithTT("-", resourceSettingsHelpOverlayInterpolation), SCANskins.SCAN_buttonSmall, GUILayout.Width(18)))
				{
					interpolationScale = Math.Max(2, interpolationScale / 2);
					refreshMap();
				}
				GUILayout.Label(textWithTT(interpolationScale.ToString(), resourceSettingsHelpOverlayInterpolation), SCANskins.SCAN_labelSmall, GUILayout.Width(36));
				if (GUILayout.Button(textWithTT("+", resourceSettingsHelpOverlayInterpolation), SCANskins.SCAN_buttonSmall, GUILayout.Width(18)))
				{
					interpolationScale = Math.Min(32, interpolationScale * 2);
					refreshMap();
				}
			stopE();

			growE();
				GUILayout.Label(textWithTT("Map Height:", resourceSettingsHelpOverlayHeight), SCANskins.SCAN_labelSmallLeft);

				fillS();

				if (GUILayout.Button(textWithTT("-", resourceSettingsHelpOverlayHeight), SCANskins.SCAN_buttonSmall, GUILayout.Width(18)))
				{
					mapHeight = Math.Max(64, mapHeight / 2);
					refreshMap();
				}
				GUILayout.Label(textWithTT(mapHeight.ToString(), resourceSettingsHelpOverlayHeight), SCANskins.SCAN_labelSmall, GUILayout.Width(36));
				if (GUILayout.Button(textWithTT("+", resourceSettingsHelpOverlayHeight), SCANskins.SCAN_buttonSmall, GUILayout.Width(18)))
				{
					mapHeight = Math.Min(1024, mapHeight * 2);
					refreshMap();
				}
			stopE();

			growE();
				GUILayout.Label(textWithTT("Coverage Transparency:", resourceSettingsHelpOverlayTransparency), SCANskins.SCAN_labelSmallLeft);

				fillS();

				if (GUILayout.Button(textWithTT("-", resourceSettingsHelpOverlayTransparency), SCANskins.SCAN_buttonSmall, GUILayout.Width(18)))
				{
					transparency = Mathf.Max(0f, transparency - 0.1f);
					refreshMap();
				}
				GUILayout.Label(textWithTT(transparency.ToString("P0"), resourceSettingsHelpOverlayTransparency), SCANskins.SCAN_labelSmall, GUILayout.Width(36));
				if (GUILayout.Button(textWithTT("+", resourceSettingsHelpOverlayTransparency), SCANskins.SCAN_buttonSmall, GUILayout.Width(18)))
				{
					transparency = Mathf.Min(1f, transparency + 0.1f);
					refreshMap();
				}
			stopE();

			growE();
				GUILayout.Label(textWithTT("Biome Map Height:", resourceSettingsHelpOverlayBiomeHeight), SCANskins.SCAN_labelSmallLeft);

				fillS();

				if (GUILayout.Button(textWithTT("-", resourceSettingsHelpOverlayBiomeHeight), SCANskins.SCAN_buttonSmall, GUILayout.Width(18)))
				{
					biomeMapHeight = Math.Max(256, biomeMapHeight / 2);
					refreshMap();
				}
				GUILayout.Label(textWithTT(biomeMapHeight.ToString(), resourceSettingsHelpOverlayBiomeHeight), SCANskins.SCAN_labelSmall, GUILayout.Width(36));
				if (GUILayout.Button(textWithTT("+", resourceSettingsHelpOverlayBiomeHeight), SCANskins.SCAN_buttonSmall, GUILayout.Width(18)))
				{
					biomeMapHeight = Math.Min(1024, biomeMapHeight * 2);
					refreshMap();
				}
			stopE();
		}

		//Confirmation boxes for map resets
		private void warningBox(int id)
		{
			if (popup)
			{
				if (warningMMLoaded)
				{
					warningRect = new Rect(WindowRect.width - (WindowRect.width / 2) - 150, WindowRect.height - 125, 300, 115);
					GUI.Box(warningRect, "");
					Rect r = new Rect(warningRect.x + 10, warningRect.y + 5, 280, 70);
					GUI.Label(r, MMwarning, SCANskins.SCAN_headlineSmall);
					r.x += 90;
					r.y += 70;
					r.width = 80;
					r.height = 30;
					if (GUI.Button(r, "OK", SCANskins.SCAN_buttonWarning))
					{
						popup = false;
						warningMMLoaded = false;
					}
				}
				else if (warningResource)
				{
					CelestialBody thisBody = getTargetBody();

					if (thisBody == null)
					{
						popup = false;
						return;
					}
					warningRect = new Rect(WindowRect.width - (WindowRect.width / 2) - 150, WindowRect.height - 125, 300, 90);
					GUI.Box(warningRect, "");
					Rect r = new Rect(warningRect.x + 10, warningRect.y + 5, 280, 40);
					GUI.Label(r, "Erase SCANsat resource data for " + thisBody.theName + "?", SCANskins.SCAN_headlineSmall);
					r.x += 90;
					r.y += 45;
					r.width = 80;
					r.height = 30;
					if (GUI.Button(r, "Confirm", SCANskins.SCAN_buttonWarning))
					{
						popup = false;
						warningResource = false;
						SCANdata data = SCANUtil.getData(thisBody);
						if (data != null)
							data.resetResources();
					}
				}
				else if (warningStockResource)
				{
					CelestialBody thisBody = getTargetBody();

					if (thisBody == null)
					{
						popup = false;
						return;
					}

					warningRect = new Rect(WindowRect.width - (WindowRect.width / 2) - 150, WindowRect.height - 125, 300, 110);
					GUI.Box(warningRect, "");
					Rect r = new Rect(warningRect.x + 10, warningRect.y + 5, 280, 60);
					GUI.Label(r, "Erase stock resource data for " + thisBody.theName + "?", SCANskins.SCAN_headlineSmall);
					r.x += 90;
					r.y += 65;
					r.width = 80;
					r.height = 30;
					if (GUI.Button(r, "Confirm", SCANskins.SCAN_buttonWarning))
					{
						popup = false;
						warningStockResource = false;
						var resources = ResourceScenario.Instance.gameSettings.GetPlanetScanInfo();
						resources.RemoveAll(a => a.PlanetId == thisBody.flightGlobalsIndex);
					}
				}
				else
					popup = false;
			}
		}

		private void refreshMap()
		{
			if (SCANcontroller.controller.resourceOverlay == null)
				return;

			SCANcontroller.controller.resourceOverlay.refreshMap(transparency, mapHeight, interpolationScale, biomeMapHeight);
		}

		private CelestialBody getTargetBody()
		{
			switch (HighLogic.LoadedScene)
			{
				case GameScenes.FLIGHT:
					return FlightGlobals.currentMainBody;
				case GameScenes.SPACECENTER:
					return Planetarium.fetch.Home;
				case GameScenes.TRACKSTATION:
					return SCANUtil.getTargetBody(MapView.MapCamera.target);
				default:
					return null;
			}
		}


	}
}
