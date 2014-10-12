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
using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.Platform;
using SCANsat;
using UnityEngine;

using palette = SCANsat.SCANpalette;

namespace SCANsat.SCAN_UI
{
	class SCANsettingsUI: MBW
	{
		/* UI: a list of glyphs that are used for something */
		private string[] exmarks = { "✗", "✘", "×", "✖", "x", "X", "∇", "☉", "★", "*", "•", "º", "+" };

		/* UI: time warp names and settings */
		private string[] twnames = { "Off", "Low", "Medium", "High" };
		private int[] twvals = { 1, 7, 11, 20 };

		internal static Rect defaultRect = new Rect(500, 55, 360, 300);

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Settings";
			WindowRect = defaultRect;
			WindowStyle = SCANskins.SCAN_window;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(360), GUILayout.Height(300) };
			Visible = true;
			DragEnabled = true;

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		internal override void OnDestroy()
		{
			
		}


		protected override void DrawWindow(int id)
		{
			versionLabel(id);
			closeBox(id);

			growS();
				fillS(8);
				gui_settings_xmarks(id); 				/* X marker selection */
				fillS(16);
				gui_settings_resources(id);				/* resource details sub-window */
				fillS(16);
				gui_settings_toggle_body_scanning(id);		/* background and body scanning toggles */
				gui_settings_rebuild_kethane(id);
				fillS(16);
				gui_settings_timewarp(id);				/* time warp resolution settings */
				fillS(8);
				GUILayout.Label(gui_settings_numbers(id), SCANskins.SCAN_whiteReadoutLabel);/* sensor/scanning		statistics */
				fillS(16);
				gui_settings_data_resets(id);			/* reset data and/or reset resources */
				fillS(8);
				gui_settings_window_resets(id);			/* reset windows and positions */
				fillS(8);
				# if DEBUG
					gui_settings_window_mapFill(id);
					fillS(8);
				#endif
			stopS();
		}

		private void versionLabel(int id)
		{
			Rect r = new Rect(6, 0, 40, 18);
			GUI.Label(r, SCANversions.SCANsatVersion, SCANskins.SCAN_whiteReadoutLabel);
		}

		private void closeBox(int id)
		{
			Rect r = new Rect(WindowRect.width - 20, 0, 18, 18);
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				Visible = false;
				SCANUtil.SCANlog("Close Settings Window");
			}
		}

		private void gui_settings_xmarks(int id)
		{
			// anomaly marker & close widget
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
		}

		private void gui_settings_resources(int id)
		{
			GUILayout.Label("Resources Overlay", SCANskins.SCAN_headline);
			if (SCANcontroller.ResourcesList.Count > 0)
			{
				if (SCANcontroller.controller.globalOverlay != GUILayout.Toggle(SCANcontroller.controller.globalOverlay, "Activate Resource Overlay"))
				{ //global toggle for resource overlay
					SCANcontroller.controller.globalOverlay = !SCANcontroller.controller.globalOverlay;
					//if (bigmap != null) bigmap.resetMap();
				}
			}

			growE();
			if (GUILayout.Button("Kethane Resources")) //select from two resource types, populates the list below
			{
				SCANcontroller.controller.resourceOverlayType = 1;
				SCANcontroller.controller.Resources(FlightGlobals.currentMainBody);
				if (SCANcontroller.ResourcesList.Count > 0)
					SCANcontroller.controller.globalOverlay = true;
				//if (bigmap != null) bigmap.resetMap();
			}

			if (GUILayout.Button("ORSX Resources"))
			{
				SCANcontroller.controller.resourceOverlayType = 0;
				SCANcontroller.controller.Resources(FlightGlobals.currentMainBody);
				if (SCANcontroller.ResourcesList.Count > 0)
					SCANcontroller.controller.globalOverlay = true;
				//if (bigmap != null) bigmap.resetMap();
			}
			stopE();
			if (SCANcontroller.ResourcesList.Count == 0)
			{
				fillS(5);
				GUILayout.Label("No Resources Found", SCANskins.SCAN_headline);
			}
			growE();
			SCANcontroller.controller.gridSelection = GUILayout.SelectionGrid(SCANcontroller.controller.gridSelection, SCANcontroller.ResourcesList.Select(a => a.name).ToArray(), 4); //select resource to display
			stopE();
		}

		private void gui_settings_toggle_body_scanning(int id)
		{

			GUILayout.Label("Background Scanning", SCANskins.SCAN_headline);
			// scan background
			SCANcontroller.controller.scan_background = GUILayout.Toggle(SCANcontroller.controller.scan_background, "Scan all active celestials");
			// scanning for individual SoIs
			growE();
			int count = 0;
			foreach (CelestialBody body in FlightGlobals.Bodies)
			{
				if (count == 0) growS();
				SCANdata data = SCANUtil.getData(body);
				data.disabled = !GUILayout.Toggle(!data.disabled, body.bodyName + " (" + data.getCoveragePercentage(SCANdata.SCANtype.Nothing).ToString("N1") + "%)"); //No longer updates while the suttings menu is open
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

		private void gui_settings_rebuild_kethane(int id)
		{ //Move this function into the settings menu
			if (SCANcontroller.controller.resourceOverlayType == 1 && SCANcontroller.controller.globalOverlay)
			{ //Rebuild the Kethane database
				if (GUILayout.Button("Rebuild Kethane Grid Database"))
					SCANcontroller.controller.kethaneRebuild = !SCANcontroller.controller.kethaneRebuild;
			}
		}

		private String gui_settings_numbers(int id)
		{
			return "Sensors: " + SCANcontroller.activeSensors +
					" Vessels: " + SCANcontroller.activeVessels.ToString() +
					" Passes: " + SCANcontroller.controller.actualPasses.ToString();
		}

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
					if (GUILayout.Button(twnames[i], SCANskins.SCAN_buttonFixed))
						SCANcontroller.controller.timeWarpResolution = twvals[i];
				}
			}

			stopE();
		}

		private void gui_settings_data_resets(int id)
		{
			CelestialBody thisBody = FlightGlobals.currentMainBody;
			GUILayout.Label("Data Management", SCANskins.SCAN_headline);
			growE();
			if (GUILayout.Button("Reset map of " + thisBody.theName))
			{
				SCANdata data = SCANUtil.getData(thisBody);
				data.reset();
			}
			if (GUILayout.Button("Reset <b>all</b> data"))
			{
				foreach (SCANdata data in SCANcontroller.body_data.Values)
				{
					data.reset();
				}
			}
			stopE();
		}

		private void gui_settings_window_resets(int id)
		{
			if (GUILayout.Button("Reset window positions", SCANskins.SCAN_buttonFixed))
			{
				if (HighLogic.LoadedSceneIsFlight)
				{
					SCANuiUtil.resetMainMapPos();
					SCANuiUtil.resetBigMapPos();
					SCANuiUtil.resetInstUIPos();
					SCANuiUtil.resetSettingsUIPos();
				}
				else
				{
					SCANuiUtil.resetKSCMapPos();
				}
			}
		}

		private void gui_settings_window_mapFill(int id)
		{
			growE();
			CelestialBody thisBody = FlightGlobals.currentMainBody;
			if (GUILayout.Button("Fill SCAN map of " + thisBody.theName, SCANskins.SCAN_buttonFixed))
			{
				SCANdata data = SCANUtil.getData(thisBody);
				data.fillMap();
			}
			if (GUILayout.Button("Fill SCAN map for all planets", SCANskins.SCAN_buttonFixed))
			{
				foreach (CelestialBody b in FlightGlobals.Bodies)
				{
					SCANdata data = SCANUtil.getData(b);
					data.fillMap();
				}
			}
			stopE();
		}

	}
}
