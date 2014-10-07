

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SCANsat.Platform;
using SCANsat;
using UnityEngine;
using palette = SCANsat.SCANpalette;

namespace SCANsat.SCAN_UI
{
	class SCANmainMap: MBW
	{
		private const string SCANlockID = "SCANmainMapLock";
		private string infoText;
		private Vessel v;
		private SCANdata data;
		private SCANdata.SCANtype sensors;
		private bool notMappingToday, showVesselInfo;

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Planetary Mapping";
			WindowRect = new Rect(10, 55, 380, 260);
			//WindowOptions = new GUILayoutOption[2] { GUILayout.Width(420), GUILayout.Height(32) };
			WindowStyle = SCANskins.SCAN_window;
			Visible = true;
			DragEnabled = true;

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		internal override void Start()
		{
			
		}

		internal override void OnDestroy()
		{
			
		}

		protected override void DrawWindowPre(int id)
		{
			v = FlightGlobals.ActiveVessel;
			data = SCANUtil.getData(v.mainBody);
			sensors = SCANcontroller.controller.activeSensorsOnVessel(v.id);
		}

		protected override void DrawWindow(int id)
		{
			bool repainting = Event.current.type == EventType.Repaint;

			versionLabel(id);
			topMenu(id);
			growS();
				topButtons(id);
				mainMap(id);
				Rect mapRect = GUILayoutUtility.GetLastRect();
				scannerInfo(id, repainting);
				vesselInfo(id, mapRect);
			stopS();
		}

		private void versionLabel(int id)
		{
			Rect r = new Rect(6, 0, 40, 18);
			GUI.Label(r, SCANversions.SCANsatVersion, SCANskins.SCAN_whiteLabel);
		}

		private void topMenu(int id)
		{
			Rect r = new Rect(WindowRect.width - 40, 0, 18, 18);
			if (showVesselInfo)
			{
				if (GUI.Button(r, "-", SCANskins.SCAN_buttonBorderless))
				{
					WindowRect.height -= (SCANcontroller.controller.knownVessels.Count * 16);
					showVesselInfo = !showVesselInfo;
				}
			}
			else
			{
				if (GUI.Button(r, "+", SCANskins.SCAN_buttonBorderless))
				{
					WindowRect.height += (SCANcontroller.controller.knownVessels.Count * 16);
					showVesselInfo = !showVesselInfo;
				}
			}
			r.x += 16;
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
				SCANUtil.SCANlog("Close Map");
		}

		private void topButtons(int id)
		{
			growE();
			if (GUILayout.Button("Big Map", SCANskins.SCAN_buttonFixed))
			{
				SCANUtil.SCANlog("Open Big Map");
			}
			if (GUILayout.Button("Instruments", SCANskins.SCAN_buttonFixed))
			{
				SCANcontroller.controller.instrumentsWindow.Visible = !SCANcontroller.controller.instrumentsWindow.Visible;
				SCANUtil.SCANlog("Open Instrument");
			}
			if (GUILayout.Button("Settings", SCANskins.SCAN_buttonFixed))
			{
				SCANcontroller.controller.settingsWindow.Visible = !SCANcontroller.controller.settingsWindow.Visible;
				SCANUtil.SCANlog("Open Settings");
			}
			stopE();
		}

		private void mainMap(int id)
		{
			GUILayout.Label(data.map_small);
		}

		private void scannerInfo(int id, bool Repainting)
		{
			GUILayout.Space(-6);
			if (!Repainting)
				infoText = InfoText();

			if (infoText != null)
				SCANuiUtil.readableLabel(infoText, false);

			GUILayout.Space(-8);
		}

		private void vesselInfo(int id, Rect MapRect)
		{
			if (!notMappingToday)
			{
				int count = 1;
				bool active = false;
				foreach (SCANcontroller.SCANvessel sV in SCANcontroller.controller.knownVessels.Values)
				{
					if (sV.vessel == null)
						continue;
					if (sV.vessel.mainBody == v.mainBody)
					{
						if (!showVesselInfo)
						{
							SCANuiUtil.drawVesselLabel(MapRect, null, -1, sV.vessel);
							continue;
						}
						float lon = (float)SCANUtil.fixLonShift(sV.vessel.longitude);
						float lat = (float)SCANUtil.fixLatShift(sV.vessel.latitude);
						float alt = sV.vessel.heightFromTerrain;
						if (alt < 0)
							alt = (float)sV.vessel.altitude;
						string text = string.Format("[{0}] <b>{1}</b> ({2:F1}°,{3:F1}°; {4:N1}m)", count, sV.vessel.vesselName, lat, lon, alt);
						if (sV.vessel == FlightGlobals.ActiveVessel)
							active = true;
						else
							active = false;
						if (SCANuiUtil.readableLabel(text, active))
						{
							if (Event.current.clickCount > 1)
							{
								Event.current.Use();
								FlightGlobals.SetActiveVessel(sV.vessel);
								ScreenMessages.PostScreenMessage(sV.vessel.vesselName, 5, ScreenMessageStyle.UPPER_CENTER);
							}
						}
						SCANuiUtil.drawVesselLabel(MapRect, null, count, sV.vessel);
						count++;
						GUILayout.Space(-10);
					}
				}
			}
		}

		private string InfoText()
		{
			string infotext = "";
			string aoff = "<color=\"grey\">";
			string aon = "<color=\"" + palette.colorHex(palette.c_good) + "\">";
			string abad = "<color=\"" + palette.colorHex(palette.c_bad) + "\">";
			string ac = "</color> ";
			string stat_alo = aon, stat_ahi = aon, stat_biome = aon;

			SCANcontroller.SCANsensor s;

			s = SCANcontroller.controller.getSensorStatus(v, SCANdata.SCANtype.AltimetryLoRes);
			if (s == null)
				stat_alo = aoff;
			else if (!s.inRange)
				stat_alo = abad;
			else if (!s.bestRange && (Time.realtimeSinceStartup % 2 < 1))
				stat_alo = abad;

			s = SCANcontroller.controller.getSensorStatus(v, SCANdata.SCANtype.AltimetryHiRes);
			if (s == null)
				stat_ahi = aoff;
			else if (!s.inRange)
				stat_ahi = abad;
			else if (!s.bestRange && (Time.realtimeSinceStartup % 2 < 1))
				stat_ahi = abad;

			s = SCANcontroller.controller.getSensorStatus(v, SCANdata.SCANtype.Biome);
			if (s == null)
				stat_biome = aoff;
			else if (!s.inRange)
				stat_biome = abad;
			else if (!s.bestRange && (Time.realtimeSinceStartup % 2 < 1))
				stat_biome = abad;

			infotext = stat_alo + "LO" + ac + stat_ahi + "HI" + ac + stat_biome + "MULTI" + ac;

			SCANdata.SCANtype active = SCANcontroller.controller.activeSensorsOnVessel(v.id);
			if (active != SCANdata.SCANtype.Nothing)
			{
				double cov = data.getCoveragePercentage(active);
				infotext += " " + cov.ToString("N1") + "%";
				if (notMappingToday)
				{
					infotext = abad + "NO POWER" + ac;
				}
			}

			return infotext;
		}


	}
}
