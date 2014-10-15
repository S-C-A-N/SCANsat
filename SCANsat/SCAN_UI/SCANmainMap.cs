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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SCANsat.Platform;
using SCANsat;
using UnityEngine;
using palette = SCANsat.SCAN_UI.SCANpalette;

namespace SCANsat.SCAN_UI
{
	class SCANmainMap: SCAN_MBW
	{
		private const string SCANlockID = "SCANmainMapLock";
		private string infoText;
		private Vessel v;
		private SCANdata data;
		private SCANdata.SCANtype sensors;
		private bool notMappingToday;
		private Rect mapRect;
		private static bool showVesselInfo = true;
		internal static Rect defaultRect = new Rect(10, 55, 380, 260);

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Planetary Mapping";
			WindowRect = defaultRect;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(380), GUILayout.Height(260) };
			WindowStyle = SCANskins.SCAN_window;
			Visible = false;
			DragEnabled = true;
			ClampToScreenOffset = new RectOffset(-300, -300, -200, -200);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		internal override void Start()
		{
			Visible = SCANcontroller.controller.mainMapVisible;
			//GameEvents.onVesselSOIChanged.Add(soiChanged);
			v = FlightGlobals.ActiveVessel;
			data = SCANUtil.getData(v.mainBody);
		}

		internal override void OnDestroy()
		{
			//GameEvents.onVesselSOIChanged.Remove(soiChanged);
		}

		protected override void DrawWindowPre(int id)
		{
			v = FlightGlobals.ActiveVessel;
			data = SCANUtil.getData(v.mainBody);
			sensors = SCANcontroller.controller.activeSensorsOnVessel(v.id);
			data.updateImages(sensors);
		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);
			topMenu(id);
			growS();
				topButtons(id);				/* Buttons for other SCANsat windows */
				mainMap(id);				/* Draws the main map texture */
				scannerInfo(id);			/* Draws the scanner indicators */
				vesselInfo(id);				/* Shows info for any SCANsat vessels */
			stopS();
		}

		//Print the version number
		private void versionLabel(int id)
		{
			Rect r = new Rect(6, 0, 40, 18);
			GUI.Label(r, SCANversions.SCANsatVersion, SCANskins.SCAN_whiteReadoutLabel);
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
			r.x += 16;
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				Visible = false;
				SCANcontroller.controller.mainMapVisible = Visible;
			}
		}

		//Draw the buttons to control other windows
		private void topButtons(int id)
		{
			fillS(4);
			growE();
			if (GUILayout.Button("Big Map", SCANskins.SCAN_buttonFixed))
			{
				SCANcontroller.controller.bigMap.Visible = !SCANcontroller.controller.bigMap.Visible;
				SCANcontroller.controller.bigMapVisible = !SCANcontroller.controller.bigMapVisible;
			}
			if (GUILayout.Button("Instruments", SCANskins.SCAN_buttonFixed))
			{
				SCANcontroller.controller.instrumentsWindow.Visible = !SCANcontroller.controller.instrumentsWindow.Visible;
			}
			if (GUILayout.Button("Settings", SCANskins.SCAN_buttonFixed))
			{
				SCANcontroller.controller.settingsWindow.Visible = !SCANcontroller.controller.settingsWindow.Visible;
			}
			stopE();
		}

		//Draw the map texture
		private void mainMap(int id)
		{
			GUILayout.Label(data.map_small);
			mapRect = GUILayoutUtility.GetLastRect();
		}

		//Draw the active scanner display
		private void scannerInfo(int id)
		{
			bool repainting = Event.current.type == EventType.Repaint;
			fillS(-6);
			if (!repainting)
				infoText = SCANuiUtil.InfoText(v, data, notMappingToday);

			if (infoText != null)
				SCANuiUtil.readableLabel(infoText, false);
			fillS(-8);
		}

		//Draw the vessel location and alt info
		private void vesselInfo(int id)
		{
			if (!notMappingToday)
			{
				int count = 2;
				vesselInfo(v, mapRect, 1, true);
				foreach (SCANcontroller.SCANvessel sV in SCANcontroller.controller.knownVessels.Values)
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

		//private void soiChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> VC)
		//{
		//	data = SCANUtil.getData(VC.to);
		//}

	}
}
