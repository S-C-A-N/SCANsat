/* 
 * Scientific Committee on Advanced Navigation S.C.A.N. Satellite
 * SCANsat - User Interface
 * 
 * Copyright (c)2013 damny; see LICENSE.txt for licensing details.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

public class SCANui {
	private static string infotext = null, title = "S.C.A.N. Planetary Mapping";
	private static Rect pos_infobox = new Rect(10f, 55f, 10f, 10f);
	private static Rect pos_bigmap = new Rect(100f, 100f, 10f, 10f);
	private static Rect pos_spotmap = new Rect(10f, 10f, 10f, 10f);
	private static SCANmap bigmap, spotmap;
	private static bool bigmap_visible;
	private static bool gui_active;
	private static int gui_frame;
	private static Callback guicb = new Callback(gui_show);

	private static void addVesselLabel(Rect maprect, int num, Vessel vessel) {
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

	private static void addAnomalyLabel(Rect maprect, SCANdata.SCANanomaly anomaly) {
		if(!anomaly.known) return;
		float lon = (float)(anomaly.longitude + 360 + 180) % 360;
		float lat = (float)(anomaly.latitude + 180 + 90) % 180;
		lon = lon * maprect.width / 360f;
		lat = maprect.height - lat * maprect.height / 180f;
		string txt = "o " + anomaly.name;
		if(!anomaly.detail) txt = "o Anomaly";
		Rect r = new Rect(maprect.x + lon - 5, maprect.y + lat - 10, 250f, 25f);
		txt = "<color=\"black\"><b>" + txt + "</b></color>";
		GUI.Label(r, txt);
	}

	private static void gui_infobox_build(int wid) {
		bool repainting = Event.current.type == EventType.Repaint;
		Vessel vessel = FlightGlobals.ActiveVessel;
		SCANdata data = SCANcontroller.controller.getData(vessel.mainBody);
		GUILayout.BeginVertical();

		GUILayout.Label(data.map_small);
		Rect maprect = GUILayoutUtility.GetLastRect();
		maprect.width = data.map_small.width;
		maprect.height = data.map_small.height;

		if(infotext != null) {
			GUILayout.BeginVertical();
			GUILayout.Label(infotext);
			GUILayout.EndVertical();
		}

		GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		if(GUILayout.Button("Big Map")) {
			bigmap_visible = !bigmap_visible;
			if(!bigmap_visible) spotmap = null;
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

	private static void gui_bigmap_build(int wid) {
		bool repainting = Event.current.type == EventType.Repaint;
		Vessel vessel = FlightGlobals.ActiveVessel;
		GUILayout.BeginVertical();

		SCANdata data = SCANcontroller.controller.getData(vessel.mainBody);
		Texture2D map = bigmap.getPartialMap();

		GUILayout.Label(map);
		Rect maprect = GUILayoutUtility.GetLastRect();
		maprect.width = map.width;
		maprect.height = map.height;

		GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		GUILayout.BeginHorizontal(GUILayout.Width(300));
		if(GUILayout.Button("Close")) {
			bigmap_visible = false;
		}
		if(SCANcontroller.controller.colours == 0) {
			if(GUILayout.Button("Grey")) {
				SCANcontroller.controller.colours = 1;
				data.resetImages();
				bigmap.resetMap();
			}
		} else if(SCANcontroller.controller.colours == 1) {
			if(GUILayout.Button("Colour")) {
				SCANcontroller.controller.colours = 0;
				data.resetImages();
				bigmap.resetMap();
			}
		}
		if(GUILayout.Button("Altimetry")) {
			bigmap.resetMap(0);
		}
		if(GUILayout.Button("Slope")) {
			bigmap.resetMap(1);
		}
		if(GUILayout.Button("Biome")) {
			bigmap.resetMap(2);
		}
		GUILayout.EndHorizontal();

		string info = "";
		float mx = Event.current.mousePosition.x - maprect.x;
		float my = Event.current.mousePosition.y - maprect.y;
		if(mx >= 0 && my >= 0 && mx < map.width && my < map.height) {		
			float mlon = (mx * 360f / map.width) - 180;
			float mlat = 90 - (my * 180f / map.height);

			bool in_spotmap = false;
			if(spotmap != null) {
				if(mx >= pos_spotmap.x-maprect.x && my >= pos_spotmap.y-maprect.y && mx <= pos_spotmap.x+pos_spotmap.width-maprect.x && my <= pos_spotmap.y+pos_spotmap.height-maprect.y) {
					in_spotmap = true;
					mlon = spotmap.lon_offset + ((mx-pos_spotmap.x+maprect.x) / spotmap.mapscale) - 180;
					mlat = spotmap.lat_offset + ((pos_spotmap.height-(my-pos_spotmap.y+maprect.y)) / spotmap.mapscale) - 90;
				}
			}
			
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
			} else if(data.isCovered(mlon, mlat, SCANdata.SCANtype.AltimetryLoRes)) {
				info += "<b>~" + (((int)data.getElevation(mlon, mlat)/500)*500).ToString() + "m</b> ";
			}
			if(data.isCovered(mlon, mlat, SCANdata.SCANtype.Biome)) {
				info += data.getBiomeName(mlon, mlat) + " ";
			}
			info += "lon: " + mlon.ToString("F2") + " lat: " + mlat.ToString("F2") + " ";
			//info += mx.ToString("F0") + "," + my.ToString("F0") + " ";
			if(in_spotmap) info += " " + spotmap.mapscale.ToString("F1") + "x";

			if(Event.current.isMouse) {
				if(Event.current.button == 1) {
					if(Event.current.type == EventType.MouseUp) {
						if(spotmap == null) {
							spotmap = new SCANmap();
							spotmap.setSize(180, 180);
						}
						if(in_spotmap) {
							spotmap.mapscale = spotmap.mapscale * 1.25f;
						} else {
							spotmap.mapscale = 10;
						}
						spotmap.centerAround(mlon, mlat);
						spotmap.resetMap(bigmap.mapmode);
						pos_spotmap.width = 180;
						pos_spotmap.height = 180;
						if(!in_spotmap) {
							pos_spotmap.x = Event.current.mousePosition.x - pos_spotmap.width / 2;
							pos_spotmap.y = Event.current.mousePosition.y - pos_spotmap.height / 2;
							if(mx > maprect.width / 2) pos_spotmap.x -= pos_spotmap.width;
							else pos_spotmap.x += pos_spotmap.height;
							pos_spotmap.x = Math.Max(maprect.x, Math.Min(maprect.x + maprect.width - pos_spotmap.width, pos_spotmap.x));
							pos_spotmap.y = Math.Max(maprect.y, Math.Min(maprect.y + maprect.height - pos_spotmap.height, pos_spotmap.y));
						}
					}
					Event.current.Use();
				}
			}
		}
		GUILayout.Label(info);
		GUILayout.EndHorizontal();
				
		addVesselLabel(maprect, 0, vessel);
		foreach(SCANdata.SCANanomaly anomaly in data.getAnomalies()) {
			addAnomalyLabel(maprect, anomaly);
		}

		if(spotmap != null) {
			spotmap.setBody(vessel.mainBody);
			GUI.Box(pos_spotmap, spotmap.getPartialMap());
		}

		GUILayout.EndVertical();
		GUI.DragWindow();
	}

	public static void gui_show() {
		bool repainting = Event.current.type == EventType.Repaint;
		Vessel vessel = FlightGlobals.ActiveVessel;

		if(Time.frameCount - gui_frame > 5) {
			gui_active = false;
			RenderingManager.RemoveFromPostDrawQueue(3, guicb);
		}

		GUI.skin.label.wordWrap = false;
		GUI.skin.button.wordWrap = false;

		SCANdata data = SCANcontroller.controller.getData(vessel.mainBody);
		data.updateImages();

		if(!repainting) { // Unity gets confused if the layout changes between layout and repaint events
			SCANdata.SCANtype sensors = SCANcontroller.controller.activeSensorsOnVessel(vessel.id);

			infotext = "";

			double valt = FlightGlobals.ActiveVessel.altitude;
			double vth = FlightGlobals.ActiveVessel.heightFromTerrain;
			string aoff = "<color=\"grey\">";
			string aon = "<color=\"green\">";
			string abad = "<color=\"orange\">";
			string ano = "<color=\"red\">";
			string ac = "</color> ";
			string stat_alo = aon, stat_ahi = aon, stat_slo = aon, stat_biome = aon, stat_ano = aon, stat_btdt = aon;

			if(valt < 5000 || valt > 500000) {
				stat_alo = abad;
				stat_ahi = abad;
				stat_slo = abad;
				stat_ano = abad;
				stat_biome = abad;
			}
			if(FlightGlobals.ActiveVessel.mainBody.BiomeMap == null || FlightGlobals.ActiveVessel.mainBody.BiomeMap.Map == null) {
				stat_biome = ano;
			}
			if(vth > 2000) {
				stat_btdt = abad;
			}

			if((sensors & SCANdata.SCANtype.AltimetryLoRes) == 0) stat_alo = aoff;
			if((sensors & SCANdata.SCANtype.AltimetryHiRes) == 0) stat_ahi = aoff;
			if((sensors & SCANdata.SCANtype.Slope) == 0) stat_slo = aoff;
			if((sensors & SCANdata.SCANtype.Biome) == 0) stat_biome = aoff;
			if((sensors & SCANdata.SCANtype.Anomaly) == 0) stat_ano = aoff;
			if((sensors & SCANdata.SCANtype.AnomalyDetail) == 0) stat_btdt = aoff;

			if(stat_btdt == aon) stat_ano = aon;

			infotext = stat_alo + "LO" + ac + stat_ahi + "HI" + ac + stat_slo + "SLO" + ac + stat_biome + "BIO" + ac + stat_ano + "ANOM" + ac + stat_btdt + "BTDT" + ac;
			title = "S.C.A.N. Planetary Mapping";
		}

		pos_infobox = GUILayout.Window(47110001, pos_infobox, gui_infobox_build, title, GUILayout.Width(400), GUILayout.Height(250));

		if(bigmap_visible) {
			if(bigmap == null) bigmap = new SCANmap();
			bigmap.setBody(vessel.mainBody);
			bigmap.setSize(0, 0);
			string rendering = "";
			if(!bigmap.isMapComplete()) rendering += " [rendering]";
			pos_bigmap = GUILayout.Window(47110002, pos_bigmap, gui_bigmap_build, "Map of " + vessel.mainBody.theName + rendering, GUILayout.Width(800), GUILayout.Height(600));
		}
	}

	public static void gui_ping() {
		if(!gui_active) {
			RenderingManager.AddToPostDrawQueue(3, guicb);
			gui_active = true;
		}
		gui_frame = Time.frameCount;
	}
}
