/* 
 * Scientific Committee on Advanced Navigation S.C.A.N. Satellite
 * SCANsat - SCAN RADAR Altimetry Sensor part
 * 
 * Copyright (c)2013 damny; see LICENSE.txt for licensing details.
 */

using System;
using UnityEngine;

public class SCANsat : PartModule
{
	protected SCANcontroller con = SCANcontroller.controller;
	protected Rect pos_infobox = new Rect(10f, 50f, 10f, 10f);
	protected Rect pos_bigmap = new Rect(10f, 10f, 10f, 10f);
	protected bool bigmap, minimap;
	protected Texture2D posmap;
	protected bool scanning = false;

	public override void OnStart(StartState state) {
		if(state == StartState.Editor) {
			print("[SCANsat] start: in editor");
		} else {
			print("[SCANsat] start: live");
			RenderingManager.AddToPostDrawQueue(3, new Callback(gui_show));
		}
	}

	[KSPEvent(guiActive = true, guiName = "Start RADAR Scan", active = true)]
	public void startScan() {
		scanning = true;
		SCANcontroller.controller.registerVesselID(vessel.id);
	}

	[KSPEvent(guiActive = true, guiName = "Stop RADAR Scan", active = true)]
	public void stopScan() {
		scanning = false;
		SCANcontroller.controller.unregisterVesselID(vessel.id);
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
		GUI.Label(r, txt);
	}

	private void gui_infobox_build(int wid) {
		SCANdata data = SCANcontroller.controller.getData(vessel.mainBody);

		GUI.skin = HighLogic.Skin;
		GUILayout.BeginVertical();
		
		GUILayout.Label(data.height_map_small);
		Rect maprect = GUILayoutUtility.GetLastRect();

		if(vessel.altitude < SCANcontroller.minScanAlt) {
			GUILayout.BeginVertical();
			GUILayout.Label("Too close!");
			GUILayout.EndVertical();
		} else if(vessel.altitude > SCANcontroller.maxScanAlt) {
			GUILayout.BeginVertical();
			GUILayout.Label("Too far away!");
			GUILayout.EndVertical();
		}

		GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		if(GUILayout.Button("Big Map")) {
			bigmap = !bigmap;
		}
		if(GUILayout.Button("Rebuild Big Map")) {
			data.resetBigMap();
			bigmap = true;
		}
		if(GUILayout.Button("Forget Map")) {
			data.reset();
		}
		GUILayout.EndHorizontal();

		int count = 1;
		foreach(Vessel v in FlightGlobals.Vessels) {
			if(v == null) continue;
			if(SCANcontroller.controller.isVesselKnown(v.id)) {
				if(v.mainBody == vessel.mainBody) {
					float lon = (float)(v.longitude + 360 + 180) % 360 - 180;
					float lat = (float)(v.latitude + 180 + 90) % 180 - 90;
					string comment = "";
					if(v.altitude < SCANcontroller.minScanAlt) comment = "; too low";
					if(v.altitude > SCANcontroller.maxScanAlt) comment = "; too high";
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Label("[" + count.ToString() + "] " + v.vesselName + " (lat: " + lat.ToString("F2") + ", lon: " + lon.ToString("F2") + comment + ")");
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
		GUI.skin = HighLogic.Skin;
		GUILayout.BeginVertical();

		SCANdata data = SCANcontroller.controller.getData(vessel.mainBody);

		GUILayout.Label(data.getPartialBigMap(vessel.mainBody));
		Rect maprect = GUILayoutUtility.GetLastRect();

		addVesselLabel(maprect, 0, vessel);

		GUILayout.EndVertical();
		GUI.DragWindow();
	}

	protected void gui_show() {
		if(!scanning) return;

		SCANdata data = SCANcontroller.controller.getData(vessel.mainBody);
		data.updateImages();

		pos_infobox = GUILayout.Window(47110001, pos_infobox, gui_infobox_build, "S.C.A.N. RADAR Altimetry", GUILayout.Width(400), GUILayout.Height(250));

		if(bigmap) {
			pos_bigmap = GUILayout.Window(47110002, pos_bigmap, gui_bigmap_build, "Map of " + vessel.mainBody.theName, GUILayout.Width(800), GUILayout.Height(600));
		}
	}
}
