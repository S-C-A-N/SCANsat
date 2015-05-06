#region license
/* 
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - Settings menu window object
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 *
 */
#endregion

using SCANsat.SCAN_Toolbar;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_UI.UI_Framework;
using SCANsat.SCAN_Platform;
using SCANsat.SCAN_Platform.Palettes;
using UnityEngine;

using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat.SCAN_UI
{
	class SCANsettingsUI: SCAN_MBW
	{
		/* UI: a list of glyphs that are used for something */
		private string[] exmarks = { "✗", "✘", "×", "✖", "x", "X", "∇", "☉", "★", "*", "•", "º", "+" };

		/* UI: time warp names and settings */
		private string[] twnames = { "Off", "Low", "Medium", "High" };
		private int[] twvals = { 1, 6, 9, 15 };
		private bool popup, warningResource, warningBoxOne, warningBoxAll, controlLock;
		private Rect warningRect;
		private const string lockID = "settingLockID";
		private bool oldTooltips, stockToolbar;

		internal readonly static Rect defaultRect = new Rect(Screen.width - (Screen.width / 2) - 180, 100, 360, 300);

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Settings";
			WindowRect = defaultRect;
			WindowStyle = SCANskins.SCAN_window;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(360), GUILayout.Height(300) };
			Visible = false;
			DragEnabled = true;
			TooltipMouseOffset = new Vector2d(-10, -25);
			ClampToScreenOffset = new RectOffset(-280, -280, -600, -600);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");

			removeControlLocks();
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

		protected override void Start()
		{
			oldTooltips = TooltipsEnabled = SCANcontroller.controller.toolTips;
			stockToolbar = SCANcontroller.controller.useStockAppLauncher;
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
				warningBoxOne = false;
				warningBoxAll = false;
				warningResource = false;
			}
		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);				/* Standard version label and close button */
			closeBox(id);

			growS();
				gui_settings_xmarks(id); 				/* X marker selection */
				gui_settings_toggle_body_scanning(id);	/* background and body scanning toggles */
				gui_settings_timewarp(id);				/* time warp resolution settings */
				gui_settings_numbers(id);				/* sensor/scanning		statistics */
				gui_settings_resources(id);				/* resource scanning options */
				gui_settings_window_resets_tooltips(id);/* reset windows and positions and toggle tooltips*/
				gui_settings_data_resets(id);			/* reset data and/or reset resources */
				# if DEBUG
					gui_settings_window_mapFill(id);	/* debug option to fill in maps */
				#endif
			stopS();

			warningBox(id);						/* Warning box for deleting map data */
		}

		protected override void DrawWindowPost(int id)
		{
			if (popup && Event.current.type == EventType.mouseDown && !warningRect.Contains(Event.current.mousePosition))
			{
				popup = false;
			}

			if (oldTooltips != SCANcontroller.controller.toolTips)
			{
				oldTooltips = SCANcontroller.controller.toolTips;
				TooltipsEnabled = SCANcontroller.controller.toolTips;
				if (HighLogic.LoadedSceneIsFlight)
				{
					SCANcontroller.controller.BigMap.TooltipsEnabled = SCANcontroller.controller.toolTips;
					SCANcontroller.controller.mainMap.TooltipsEnabled = SCANcontroller.controller.toolTips;
					if (SCANcontroller.controller.BigMap.spotMap != null)
						SCANcontroller.controller.BigMap.spotMap.TooltipsEnabled = SCANcontroller.controller.toolTips;
				}
				if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
				{
					SCANcontroller.controller.kscMap.TooltipsEnabled = SCANcontroller.controller.toolTips;
					if (SCANcontroller.controller.kscMap.spotMap != null)
						SCANcontroller.controller.kscMap.spotMap.TooltipsEnabled = SCANcontroller.controller.toolTips;
				}
			}

			if (stockToolbar != SCANcontroller.controller.useStockAppLauncher)
			{
				stockToolbar = SCANcontroller.controller.useStockAppLauncher;
				if (stockToolbar)
					SCANcontroller.controller.appLauncher = gameObject.AddComponent<SCANappLauncher>();
				else
					Destroy(SCANcontroller.controller.appLauncher);
			}
		}

		//Draw the version label in the upper left corner
		private void versionLabel(int id)
		{
			Rect r = new Rect(6, 0, 50, 18);
			GUI.Label(r, SCANmainMenuLoader.SCANsatVersion, SCANskins.SCAN_whiteReadoutLabel);
		}

		//Draw the close button in the upper right corner
		private void closeBox(int id)
		{
			Rect r = new Rect(WindowRect.width - 20, 1, 18, 18);
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				removeControlLocks();
				Visible = false;
			}
		}

		//Choose anomaly marker icon
		private void gui_settings_xmarks(int id)
		{
			fillS(8);
			GUILayout.Label("Anomaly Marker", SCANskins.SCAN_headline);
			growE();
			for (int i = 0; i < exmarks.Length; ++i)
			{
				if (SCANcontroller.controller.anomalyMarker == exmarks[i])
				{
					if (GUILayout.Button(exmarks[i], SCANskins.SCAN_closeButton))
						SCANcontroller.controller.anomalyMarker = exmarks[i];
				}
				else
				{
					if (GUILayout.Button(exmarks[i], SCANskins.SCAN_buttonBorderless))
						SCANcontroller.controller.anomalyMarker = exmarks[i];
				}
			}
			stopE();
			fillS(16);
		}

		//Control background scanning options
		private void gui_settings_toggle_body_scanning(int id)
		{

			GUILayout.Label("Background Scanning", SCANskins.SCAN_headline);
			// scan background
			SCANcontroller.controller.scan_background = GUILayout.Toggle(SCANcontroller.controller.scan_background, "Scan all active celestials", SCANskins.SCAN_settingsToggle);
			// scanning for individual SoIs
			growE();
			int count = 0;
			foreach (SCANdata data in SCANcontroller.controller.GetAllData)
			{
				if (count == 0) growS();
					data.Disabled = !GUILayout.Toggle(!data.Disabled, string.Format("{0} ({1:N1}%)", data.Body.name, SCANUtil.getCoveragePercentage(data, SCANtype.Nothing)), SCANskins.SCAN_settingsToggle);
				switch (count)
				{
					case 5: stopS(); count = 0; break;
					default: ++count; break;
				}
			}
			if (count != 0)
				stopS(); ;
			stopE();
		}

		//Control scanning resolution
		private void gui_settings_timewarp(int id)
		{
			GUILayout.Label("Time Warp Resolution", SCANskins.SCAN_headline);
			growE();

			for (int i = 0; i < twnames.Length; ++i)
			{
				if (SCANcontroller.controller.timeWarpResolution == twvals[i])
				{
					if (GUILayout.Button(twnames[i], SCANskins.SCAN_buttonActive))
						SCANcontroller.controller.timeWarpResolution = twvals[i];
				}
				else
				{
					if (GUILayout.Button(twnames[i]))
						SCANcontroller.controller.timeWarpResolution = twvals[i];
				}
			}
			stopE();
			fillS(8);
		}

		//Display the total number of SCANsat sensors and scanning passes
		/* Needs to be clarified for users */
		private void gui_settings_numbers(int id)
		{
			string s = 	"Vessels: " + SCANcontroller.controller.ActiveVessels.ToString() +
						" Sensors: " + SCANcontroller.controller.ActiveSensors +
						" Passes: " + SCANcontroller.controller.ActualPasses.ToString();
			GUILayout.Label(s, SCANskins.SCAN_whiteReadoutLabel);
			fillS(16);
		}

		//Reset databases
		private void gui_settings_data_resets(int id)
		{
			CelestialBody thisBody = null;
			if (HighLogic.LoadedSceneIsFlight)
				thisBody = FlightGlobals.currentMainBody;
			else if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
				thisBody = Planetarium.fetch.Home;
			else if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
				thisBody = getTargetBody(MapView.MapCamera.target);
			if (thisBody == null)
				return;

			GUILayout.Label("Data Management", SCANskins.SCAN_headline);
			growE();
			if (popup)
			{
				GUILayout.Label("Reset map of " + thisBody.theName, SCANskins.SCAN_button);
				GUILayout.Label("Reset <b>all</b> data", SCANskins.SCAN_button);
			}
			else
			{
				if (GUILayout.Button("Reset map of " + thisBody.theName))
				{
					popup = !popup;
					warningBoxOne = !warningBoxOne;
				}
				if (GUILayout.Button("Reset <b>all</b> data"))
				{
					popup = !popup;
					warningBoxAll = !warningBoxAll;
				}
			}
			stopE();
			fillS(8);
		}

		private CelestialBody getTargetBody(MapObject target)
		{
			if (target.type == MapObject.MapObjectType.CELESTIALBODY)
			{
				return target.celestialBody;
			}
			else if (target.type == MapObject.MapObjectType.MANEUVERNODE)
			{
				return target.maneuverNode.patch.referenceBody;
			}
			else if (target.type == MapObject.MapObjectType.VESSEL)
			{
				return target.vessel.mainBody;
			}

			return null;
		}

		//Resets all window positions, tooltip toggle
		private void gui_settings_window_resets_tooltips(int id)
		{
			GUILayout.Label("Settings", SCANskins.SCAN_headline);
			growE();
				SCANcontroller.controller.useStockAppLauncher = GUILayout.Toggle(SCANcontroller.controller.useStockAppLauncher, "Stock Toolbar", SCANskins.SCAN_settingsToggle);

				SCANcontroller.controller.toolTips = GUILayout.Toggle(SCANcontroller.controller.toolTips, "Tooltips", SCANskins.SCAN_settingsToggle);

				if (SCANmainMenuLoader.MechJebLoaded)
					SCANcontroller.controller.mechJebTargetSelection = GUILayout.Toggle(SCANcontroller.controller.mechJebTargetSelection, "MechJeb Target Selection", SCANskins.SCAN_settingsToggle);
			stopE();
			fillS(8);
			if (popup)
			{
				GUILayout.Label("Reset window positions", SCANskins.SCAN_button);
			}
			else
			{
				if (GUILayout.Button("Reset window positions"))
				{
					if (HighLogic.LoadedSceneIsFlight)
					{
						SCANuiUtil.resetMainMapPos();
						SCANuiUtil.resetBigMapPos();
						SCANuiUtil.resetInstUIPos();
						SCANuiUtil.resetSettingsUIPos();
						SCANuiUtil.resetColorMapPos();
					}
					else
					{
						SCANuiUtil.resetKSCMapPos();
						SCANuiUtil.resetColorMapPos();
						SCANuiUtil.resetSettingsUIPos();
					}
				}
			}
			fillS(8);
		}

		private void gui_settings_resources(int id)
		{
			GUILayout.Label("Resource Settings", SCANskins.SCAN_headline);
			growE();
				SCANcontroller.controller.resourceBiomeLock = GUILayout.Toggle(SCANcontroller.controller.resourceBiomeLock, "Resource Biome Lock", SCANskins.SCAN_settingsToggle);
				SCANcontroller.controller.easyModeScanning = GUILayout.Toggle(SCANcontroller.controller.easyModeScanning, "Instant Resource Scan", SCANskins.SCAN_settingsToggle);
			stopE();
			growE();
				fillS();
				SCANcontroller.controller.needsNarrowBand = GUILayout.Toggle(SCANcontroller.controller.needsNarrowBand,		"Zoom Requires Narrow Band Scanner", SCANskins.SCAN_settingsToggle);
				fillS();
			stopE();
			if (popup)
			{
				GUILayout.Label("Reset Resource Coverage", SCANskins.SCAN_button);
			}
			else
			{
				if (GUILayout.Button("Reset Resource Coverage"))
				{
					popup = !popup;
					warningResource = !warningResource;
				}
			}
		}

		//Debugging option to fill in SCAN maps
		private void gui_settings_window_mapFill(int id)
		{
			growE();
			CelestialBody thisBody = null;
			if (HighLogic.LoadedSceneIsFlight)
				thisBody = FlightGlobals.currentMainBody;
			else if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
				thisBody = Planetarium.fetch.Home;
			else if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
				thisBody = getTargetBody(MapView.MapCamera.target);
			if (thisBody == null)
				return;
			if (GUILayout.Button("Fill SCAN map of " + thisBody.theName))
			{
				SCANdata data = SCANUtil.getData(thisBody);
				if (data == null)
				{
					data = new SCANdata(thisBody);
					SCANcontroller.controller.addToBodyData(thisBody, data);
				}
				data.fillMap();
			}
			if (GUILayout.Button("Fill SCAN map for all planets"))
			{
				foreach (CelestialBody b in FlightGlobals.Bodies)
				{
					SCANdata data = SCANUtil.getData(b);
					if (data == null)
					{
						data = new SCANdata(b);
						SCANcontroller.controller.addToBodyData(b, data);
					}
					data.fillMap();
				}
			}
			stopE();
			fillS(8);
		}

		//Confirmation boxes for map resets
		private void warningBox(int id)
		{
			if (popup)
			{
				if (warningBoxOne)
				{
					CelestialBody thisBody = null;
					if (HighLogic.LoadedSceneIsFlight)
						thisBody = FlightGlobals.currentMainBody;
					else if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
						thisBody = Planetarium.fetch.Home;
					else if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
						thisBody = getTargetBody(MapView.MapCamera.target);
					if (thisBody == null)
						return;
					warningRect = new Rect(WindowRect.width - (WindowRect.width / 2) - 150, WindowRect.height - 125, 300, 90);
					GUI.Box(warningRect, "");
					Rect r = new Rect(warningRect.x + 10, warningRect.y + 5, 280, 40);
					GUI.Label(r, "Erase all data for " + thisBody.theName + "?", SCANskins.SCAN_headlineSmall);
					r.x += 90;
					r.y += 45;
					r.width = 80;
					r.height = 30;
					if (GUI.Button(r, "Confirm", SCANskins.SCAN_buttonWarning))
					{
						popup = false;
						warningBoxOne = false;
						SCANdata data = SCANUtil.getData(thisBody);
						if (data != null)
							data.reset();
					}
				}
				else if (warningBoxAll)
				{
					warningRect = new Rect(WindowRect.width - (WindowRect.width / 2) - 120, WindowRect.height - 160, 240, 90);
					GUI.Box(warningRect, "");
					Rect r = new Rect(warningRect.x + 10, warningRect.y + 5, 220, 40);
					GUI.Label(r, "Erase <b>all</b> data ?", SCANskins.SCAN_headlineSmall);
					r.x += 70;
					r.y += 45;
					r.width = 80;
					r.height = 30;
					if (GUI.Button(r, "Confirm", SCANskins.SCAN_buttonWarning))
					{
						popup = false;
						warningBoxAll = false;
						foreach (SCANdata data in SCANcontroller.controller.GetAllData)
						{
							data.reset();
						}
					}
				}
				else if (warningResource)
				{
					CelestialBody thisBody = null;
					if (HighLogic.LoadedSceneIsFlight)
						thisBody = FlightGlobals.currentMainBody;
					else if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
						thisBody = Planetarium.fetch.Home;
					else if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
						thisBody = getTargetBody(MapView.MapCamera.target);
					if (thisBody == null)
						return;
					warningRect = new Rect(WindowRect.width - (WindowRect.width / 2) - 150, WindowRect.height - 125, 300, 90);
					GUI.Box(warningRect, "");
					Rect r = new Rect(warningRect.x + 10, warningRect.y + 5, 280, 40);
					GUI.Label(r, "Erase resource data for " + thisBody.theName + "?", SCANskins.SCAN_headlineSmall);
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
				else
					popup = false;
			}
		}

	}
}
