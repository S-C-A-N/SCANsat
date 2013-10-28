/* 
 * Scientific Committee on Advanced Navigation S.C.A.N. Satellite
 * SCANsat - SCAN RADAR Altimetry Sensor part
 * 
 * Copyright (c)2013 damny; see LICENSE.txt for licensing details.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

public class SCANsat : PartModule
{
	protected SCANcontroller con = SCANcontroller.controller;
	protected Rect pos_infobox = new Rect(10f, 50f, 10f, 10f);
	protected Rect pos_bigmap = new Rect(10f, 10f, 10f, 10f);
	protected bool bigmap;
	protected Texture2D posmap;
	protected string infotext = null;
	protected bool initialized = false;

	public override void OnStart(StartState state) {
		if(state == StartState.Editor) {
			print("[SCANsat] start: in editor");
		} else {
			print("[SCANsat] start: live");
			RenderingManager.AddToPostDrawQueue(3, new Callback(gui_show));
		}
	}

	[KSPField]
	public int sensorType;

	[KSPField(isPersistant = true)]
	protected bool scanning = false;

	[KSPEvent(guiActive = true, guiName = "Start RADAR Scan", active = true)]
	public void startScan() {
		print("Starting RADAR scan, sensor type = " + sensorType.ToString());
		scanning = true;
		if(sensorType > 0) {
			SCANcontroller.controller.registerVesselID(vessel.id, (SCANdata.SCANtype)sensorType);
		}
	}

	[KSPEvent(guiActive = true, guiName = "Stop RADAR Scan", active = true)]
	public void stopScan() {
		scanning = false;
		if(sensorType > 0) {
			SCANcontroller.controller.unregisterVesselID(vessel.id, (SCANdata.SCANtype)sensorType);
		}
	}

	[KSPAction("Start RADAR Scan")]
	public void startScanAction(KSPActionParam param) {
		startScan();
	}

	[KSPAction("Stop RADAR Scan")]
	public void stopScanAction(KSPActionParam param) {
		stopScan();
	}

	[KSPAction("Toggle RADAR Scan")]
	public void toggleScanAction(KSPActionParam param) {
		if(scanning) stopScan();
		else startScan();
	}

	public override void OnUpdate() {
		if(vessel != FlightGlobals.ActiveVessel) return;
		if(!initialized) {
			if(sensorType == 0) {
				Events["startScan"].guiName = "Open Map";
				Events["stopScan"].guiName = "Close Map";
			}
			if(scanning) {
				startScan();
			}
			initialized = true;
		}
		Events["startScan"].active = !scanning;
		Events["stopScan"].active = scanning;
		SCANcontroller.controller.scanFromAllVessels();
	}

	private void addVesselLabel(Rect maprect, int num, Vessel vessel) {
		float lon = (float)(vessel.longitude + 360 + 180) % 360;
		float lat = (float)(vessel.latitude + 180 + 90) % 180;
		lon = lon * maprect.width / 360f;
		lat = maprect.height - lat * maprect.height / 180f;
		string txt = "x " + num.ToString();
		if(num <= 0) txt = "x " + vessel.vesselName;
		Rect r = new Rect(maprect.x + lon - 5, maprect.y + lat - 10, 250f, 25f);
		txt = "<color=\"white\"><b>" + txt + "</b></color>";
		GUI.Label(r, txt);
	}

	private void addAnomalyLabel(Rect maprect, SCANdata.SCANanomaly anomaly) {
		if(!anomaly.known) return;
		float lon = (float)(anomaly.longitude + 360 + 180) % 360;
		float lat = (float)(anomaly.latitude + 180 + 90) % 180;
		lon = lon * maprect.width / 360f;
		lat = maprect.height - lat * maprect.height / 180f;
		string txt = "o " + anomaly.name;
		Rect r = new Rect(maprect.x + lon - 5, maprect.y + lat - 10, 250f, 25f);
		txt = "<color=\"black\"><b>" + txt + "</b></color>";
		GUI.Label(r, txt);
	}

	private void gui_infobox_build(int wid) {
		SCANdata data = SCANcontroller.controller.getData(vessel.mainBody);
		GUILayout.BeginVertical();
		
		GUILayout.Label(data.height_map_small);
		Rect maprect = GUILayoutUtility.GetLastRect();

		if(sensorType != 32) {
			if(vessel.altitude < SCANcontroller.minScanAlt) {
				GUILayout.BeginVertical();
				GUILayout.Label("<b>Too close!</b>");
				GUILayout.EndVertical();
			} else if(vessel.altitude > SCANcontroller.maxScanAlt) {
				GUILayout.BeginVertical();
				GUILayout.Label("<b>Too far away!</b>");
				GUILayout.EndVertical();
			}
		} else if(sensorType > 0) {
			if(vessel.terrainAltitude >= 2000) {
				GUILayout.BeginVertical();
				GUILayout.Label("<b>Too high!</b>");
				GUILayout.EndVertical();
			}
		}

		if(infotext != null) {
			GUILayout.BeginVertical();
			GUILayout.Label(infotext);
			GUILayout.EndVertical();
		}

		GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		if(GUILayout.Button("Big Map")) {
			bigmap = !bigmap;
		}
		if(GUILayout.Button("Forget Map")) {
			data.reset();
		}
		GUILayout.EndHorizontal();

		int count = 1;
		foreach(Vessel v in FlightGlobals.Vessels) {
			if(v == null) continue;
			if(SCANcontroller.controller.isVesselKnown(v.id) || v == FlightGlobals.ActiveVessel) {
				if(v.mainBody == vessel.mainBody) {
					float lon = (float)(v.longitude + 360 + 180) % 360 - 180;
					float lat = (float)(v.latitude + 180 + 90) % 180 - 90;
					string text = "[" + count.ToString() + "] <b>" + v.vesselName + "</b> (lat: " + lat.ToString("F2") + ", lon: " + lon.ToString("F2") + ", alt: " + v.terrainAltitude.ToString("F2") + "m)";
					if(v == FlightGlobals.ActiveVessel) {
						text = "<color=\"#99ff66\">" + text + "</color>";
					}
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Label(text);
					GUILayout.EndHorizontal();
					addVesselLabel(maprect, count, v);
					count += 1;
				}
			}
		}

		GUILayout.EndVertical();
		GUI.DragWindow();
	}

	private void gui_bigmap_build(int wid) {
		GUILayout.BeginVertical();

		SCANdata data = SCANcontroller.controller.getData(vessel.mainBody);
		Texture2D map = data.getPartialBigMap();

		GUILayout.Label(map);
		Rect maprect = GUILayoutUtility.GetLastRect();

		addVesselLabel(maprect, 0, vessel);

		foreach(SCANdata.SCANanomaly anomaly in data.getAnomalies()) {
			addAnomalyLabel(maprect, anomaly);
		}

		GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		GUILayout.BeginHorizontal(GUILayout.Width(300));
		if(GUILayout.Button("Close")) {
			bigmap = false;
		}
		if(SCANcontroller.controller.colours == 0) {
			if(GUILayout.Button("Grey")) {
				SCANcontroller.controller.colours = 1;
				data.resetImages();
				data.resetBigMap();
			}
		} else if(SCANcontroller.controller.colours == 1) {
			if(GUILayout.Button("Colour")) {
				SCANcontroller.controller.colours = 0;
				data.resetImages();
				data.resetBigMap();
			}
		}
		if(GUILayout.Button("Altimetry")) {
			data.resetBigMap(0);
		}
		if(GUILayout.Button("Slope")) {
			data.resetBigMap(1);
		}
		if(GUILayout.Button("Biome")) {
			data.resetBigMap(2);
		}
		GUILayout.EndHorizontal();

		string info = "";
		float mx = Event.current.mousePosition.x - maprect.x;
		float my = Event.current.mousePosition.y - maprect.y;
		if(mx >= 0 && my >= 0 && mx < map.width && my < map.height) {			
			float mlon = (mx * 360f / map.width) - 180;
			float mlat = 90 - (my * 180f / map.height);

			if(data.isCovered(mlon, mlat, SCANdata.SCANtype.AltimetryLoRes)) {
				if(vessel.mainBody.pqsController == null) info += "<color=\"red\">LO</color> ";
				else info += "<color=\"green\">LO</color> ";
			} else info += "<color=\"grey\">LO</color> ";
			if(data.isCovered(mlon, mlat, SCANdata.SCANtype.AltimetryHiRes)) {
				if(vessel.mainBody.pqsController == null) info += "<color=\"red\">HI</color> ";
				else info += "<color=\"green\">HI</color> ";
			} else info += "<color=\"grey\">HI</color> ";
			if(data.isCovered(mlon, mlat, SCANdata.SCANtype.Slope)) {
				if(vessel.mainBody.pqsController == null) info += "<color=\"red\">SLO</color> ";
				else info += "<color=\"green\">SLO</color> ";
			} else info += "<color=\"grey\">SLO</color> ";
			if(data.isCovered(mlon, mlat, SCANdata.SCANtype.Biome)) {
				if(vessel.mainBody.BiomeMap == null || vessel.mainBody.BiomeMap.Map == null) info += "<color=\"red\">BIO</color> ";
				else info += "<color=\"green\">BIO</color> ";
			} else info += "<color=\"grey\">BIO</color> ";
			if(data.isCovered(mlon, mlat, SCANdata.SCANtype.Anomaly)) info += "<color=\"green\">ANOM</color> ";
			else info += "<color=\"grey\">ANOM</color> ";
			if(data.isCovered(mlon, mlat, SCANdata.SCANtype.AnomalyDetail)) info += "<color=\"green\">BTDT</color> ";
			else info += "<color=\"grey\">BTDT</color> ";

			if(data.isCovered(mlon, mlat, SCANdata.SCANtype.AltimetryHiRes)) {
				info += "<b>" + data.getElevation(mlon, mlat).ToString("F2") + "m</b> ";
			}
			if(data.isCovered(mlon, mlat, SCANdata.SCANtype.Biome)) {
				info += data.getBiomeName(mlon, mlat) + " ";
			}

			info += "lon: " + mlon.ToString("F2") + " lat: " + mlat.ToString("F2") + " ";
			// info += mx.ToString("F0") + "," + my.ToString("F0") + " ";

			GUILayout.Label(info);
		}
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
		GUI.DragWindow();
	}

	protected void gui_show() {
		if(!scanning) return;

		Part drawingPart = null;
		foreach(Part p in vessel.Parts) {
			foreach(PartModule m in p.Modules) {
				if(m.name == "SCANsat") {
					if(p != part) return;
					drawingPart = p;
					break;
				}
			}
			if(drawingPart != null) break;
		}

		GUI.skin.label.wordWrap = false;
		GUI.skin.button.wordWrap = false;

		SCANdata data = SCANcontroller.controller.getData(vessel.mainBody);
		data.updateImages();

		SCANdata.SCANtype sensors = SCANcontroller.controller.activeSensorsOnVessel(vessel.id);
		List<string> stext = new List<string>();
		if((sensors & SCANdata.SCANtype.AltimetryHiRes) != 0) {
			stext.Add("RADAR Altimetry (High Resolution)");
		} else if((sensors & SCANdata.SCANtype.Altimetry) != 0) {
			stext.Add("RADAR Altimetry");
		}
		if((sensors & SCANdata.SCANtype.Slope) != 0) {
			stext.Add("RADAR Slope Detection");
		}
		if((sensors & SCANdata.SCANtype.Biome) != 0) {
			stext.Add("SAR Terrain Detection");
		}
		if((sensors & SCANdata.SCANtype.AnomalyDetail) != 0) {
			stext.Add("Close Range Anomaly Detection");
		} else if((sensors & SCANdata.SCANtype.Anomaly) != 0) {
			stext.Add("SAR Anomaly Detection");
		}

		string title = "S.C.A.N. Planetary Mapping";
		if(stext.Count == 1) {
			title = stext[0];
			infotext = null;
		}
		if(stext.Count > 1) {
			infotext = "<b>Active Sensors:</b>\n";
			foreach(string s in stext) {
				infotext += s + "\n";
			}
		}

		pos_infobox = GUILayout.Window(47110001, pos_infobox, gui_infobox_build, title, GUILayout.Width(400), GUILayout.Height(250));

		if(bigmap) {
			string rendering = "";
			if(!data.isBigMapComplete()) rendering += " [rendering]";
			pos_bigmap = GUILayout.Window(47110002, pos_bigmap, gui_bigmap_build, "Map of " + vessel.mainBody.theName + rendering, GUILayout.Width(800), GUILayout.Height(600));
		}
	}
}
