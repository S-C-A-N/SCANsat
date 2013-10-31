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
	private static Rect pos_spotmap = new Rect(10f, 10f, 10f, 10f), pos_spotmap_x = new Rect(10f, 10f, 25f, 25f);
	private static SCANmap bigmap, spotmap;
	private static bool bigmap_visible;
	private static bool gui_active;
	private static int gui_frame_ping, gui_frame_draw;
	private static Callback guicb = new Callback(gui_show);
	private static GUIStyle style_label = new GUIStyle();
	private static GUIStyle style_button;

	// colourblind barrier-free colours, according to Masataka Okabe and Kei Ito
	// http://jfly.iam.u-tokyo.ac.jp/color/
	public static Color cb_orange = new Color(0.9f, 0.6f, 0f);			// orange
	public static Color cb_skyBlue = new Color(0.35f, 0.7f, 0.9f);		// sky blue
	public static Color cb_bluishGreen = new Color(0f, 0.6f, 0.5f);		// bluish green
	public static Color cb_yellow = new Color(0.95f, 0.9f, 0.25f);		// yellow
	public static Color cb_blue = new Color(0f, 0.45f, 0.7f);			// blue
	public static Color cb_vermillion = new Color(0.8f, 0.4f, 0f);		// vermillion
	public static Color cb_reddishPurple = new Color(0.8f, 0.6f, 0.7f);	// reddish purple

	public static string colorHex(Color c) {
		return String.Format("#{0:X2}{1:X2}{2:X2}", c.r * 255, c.g * 255, c.b * 255);
	}

	public static string colored(Color c, string text) {
		text = "<color=\""+colorHex(c)+"\">" + text + "</color>";
		return text;
	}

	private static void drawLabel(Rect r, Color c, string txt) {
		if(txt.Length < 1) return;
		style_label.normal.textColor = Color.black;
		Vector2 sz = style_label.CalcSize(new GUIContent(txt.Substring(0, 1)));
		r.x -= sz.x / 2;
		r.y -= sz.y / 2;
		r.x -= 1; GUI.Label(r, txt, style_label);
		r.x += 2; GUI.Label(r, txt, style_label); r.x -= 1; 
		r.y -= 1; GUI.Label(r, txt, style_label);
		r.y += 2; GUI.Label(r, txt, style_label); r.y -= 1;
		style_label.normal.textColor = c;
		GUI.Label(r, txt, style_label);
	}

	private static void addVesselLabel(Rect maprect, SCANmap map, int num, Vessel vessel) {
		double lon = (vessel.longitude + 360 + 180) % 360;
		double lat = (vessel.latitude + 180 + 90) % 180;
		if(map != null) {
			lat = (map.projectLatitude(vessel.longitude, vessel.latitude) + 90) % 180;
			lon = (map.projectLongitude(vessel.longitude, vessel.latitude) + 180) % 360;
		}
		lon = lon * maprect.width / 360f;
		lat = maprect.height - lat * maprect.height / 180f;
		string txt = "☀ " + num.ToString();
		if(num <= 0) txt = "☀ " + vessel.vesselName;
		Rect r = new Rect(maprect.x + (float)lon, maprect.y + (float)lat, 250f, 25f);
		if(SCANcontroller.controller.colours == 1) drawLabel(r, cb_skyBlue, txt);
		else drawLabel(r, Color.white, txt);
	}

	private static void addAnomalyLabel(Rect maprect, SCANmap map, SCANdata.SCANanomaly anomaly) {
		if(!anomaly.known) return;
		double lon = (anomaly.longitude + 360 + 180) % 360;
		double lat = (anomaly.latitude + 180 + 90) % 180;
		if(map != null) {
			lat = (map.projectLatitude(anomaly.longitude, anomaly.latitude) + 90) % 180;
			lon = (map.projectLongitude(anomaly.longitude, anomaly.latitude) + 180) % 360;
		}
		lon = lon * maprect.width / 360f;
		lat = maprect.height - lat * maprect.height / 180f;
		string txt = "✗ " + anomaly.name;
		if(!anomaly.detail) txt = "✗ Anomaly";
		Rect r = new Rect(maprect.x + (float)lon, maprect.y + (float)lat, 250f, 25f);
		if(SCANcontroller.controller.colours == 1) drawLabel(r, cb_orange, txt);
		else drawLabel(r, cb_yellow, txt);
	}


	private static string toDMS(double thing, string neg, string pos) {
		string dms = "";
		if(thing >= 0) neg = pos;
		thing = Math.Abs(thing);
		dms += Math.Floor(thing).ToString() + "°";
		thing = (thing-Math.Floor(thing)) * 60;
		dms += Math.Floor(thing).ToString() + "'";
		thing = (thing - Math.Floor(thing)) * 60;
		dms += thing.ToString("F2") + "\"";
		dms += neg;
		return dms;
	}

	private static string toDMS(double lat, double lon) {
		return toDMS(lat, "S", "N") + " " + toDMS(lon, "W", "E");
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
					float alt = v.heightFromTerrain;
					if(alt < 0) alt = (float)v.altitude;
					string text = "[" + count.ToString() + "] <b>" + v.vesselName + "</b> (" + lat.ToString("F1") + "," + lon.ToString("F1") +"; " + alt.ToString("N1") + "m)";
					if(v == FlightGlobals.ActiveVessel) {
						text = "<color=\"#99ff66\">" + text + "</color>";
					}
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Label(text);
					GUILayout.EndHorizontal();
					addVesselLabel(maprect, null, count, v);
					count += 1;
				}
			}
		}

		GUILayout.EndVertical();
		GUI.DragWindow();
	}

	private static void mapPosAtT(Rect maprect, ref Rect r, Vessel vessel, double dT) {
		Vector3d pos = vessel.orbit.getPositionAtUT(Planetarium.GetUniversalTime() + dT);
		double rotation = 0;
		if(vessel.mainBody.rotates) {
			rotation = (360 * (dT / vessel.mainBody.rotationPeriod)) % 360;
		}
		double lo = (vessel.mainBody.GetLongitude(pos) - rotation);
		double la = (vessel.mainBody.GetLatitude(pos));
		double lon = (bigmap.projectLongitude(lo, la) + 180) % 360;
		double lat = (bigmap.projectLatitude(lo, la) + 90) % 180;
		lon = lon * maprect.width / 360f;
		lat = maprect.height - lat * maprect.height / 180f;
		r.x = maprect.x + (float)lon;
		r.y = maprect.y + (float)lat;
	}

	private static void drawOrbit(Rect maprect, Vessel vessel) {
		if(vessel.LandedOrSplashed) return;
		Orbit o = vessel.orbit;
		double startUT = Planetarium.GetUniversalTime();
		double UT = startUT;
		int steps = 100; // increase for neater lines, decrease for better speed indication
		bool ath = false;
		if(vessel.mainBody.atmosphere) {
			if(vessel.mainBody.maxAtmosphereAltitude >= vessel.altitude) {
				ath = true;
			}
		}
		Rect r = new Rect(0, 0, 50f, 50f);
		Color col;
		// project the last and the current orbital period onto the map
		for(int i=-steps; i<steps; ++i) {
			if(i < 0) UT = startUT - (steps + i) * (o.period / steps);
			else UT = startUT + i * o.period * 1f / steps;
			if(UT < o.StartUT && o.StartUT != startUT) continue;
			if(UT > o.EndUT) continue;
			Vector3d pos = o.getPositionAtUT(UT);
			double rotation = 0;
			if(vessel.mainBody.rotates) {
				rotation = (360 * ((UT - startUT) / vessel.mainBody.rotationPeriod)) % 360;
			}
			double alt = (vessel.mainBody.GetAltitude(pos));
			if(alt < 0) {
				if(i < 0) {
					i = 0;
					continue;
				}
				break;
			}
			double lo = (vessel.mainBody.GetLongitude(pos) - rotation);
			double la = (vessel.mainBody.GetLatitude(pos));
			double lon = (bigmap.projectLongitude(lo, la) + 180) % 360;
			double lat = (bigmap.projectLatitude(lo, la) + 90) % 180;
			lon = lon * maprect.width / 360f;
			lat = maprect.height - lat * maprect.height / 180f;
			r.x = maprect.x + (float)lon;
			r.y = maprect.y + (float)lat;
			col = cb_skyBlue;
			string comment = "";
			if(i < 0) {
				col = cb_orange;
			} else {
				if(vessel.mainBody.atmosphere) {
					if(vessel.mainBody.maxAtmosphereAltitude >= alt) {
						if(!ath) {
							ath = true;
							comment += "re-entry";
						}
						col = cb_yellow;
					}
				}
			}
			drawLabel(r, col, "•"); // this is a terrible way to draw lines
			drawLabel(r, col, comment); 
		}

		// show apoapsis and periapsis
		mapPosAtT(maprect, ref r, vessel, o.timeToAp);
		drawLabel(r, cb_skyBlue, "ap=" + o.ApA.ToString("N1"));
		mapPosAtT(maprect, ref r, vessel, o.timeToPe);
		drawLabel(r, cb_skyBlue, "pe=" + o.PeA.ToString("N1"));

		// predict equatorial crossings for the next 100 loops
		double TAAN = 360f - o.argumentOfPeriapsis;	// true anomaly at ascending node
		double TADN = (TAAN + 180) % 360;			// true anomaly at descending node
		double MAAN = meanForTrue(TAAN, o.eccentricity);
		double MADN = meanForTrue(TADN, o.eccentricity);
		double tAN = (((MAAN - o.meanAnomaly*Mathf.Rad2Deg + 360) % 360) / 360f * o.period + startUT);
		double tDN = (((MADN - o.meanAnomaly*Mathf.Rad2Deg + 360) % 360) / 360f * o.period + startUT);
		col = Color.magenta;
		if(SCANcontroller.controller.colours == 1) col = cb_yellow;
		for(int i=0; i<100; ++i) {
			double UTAN = tAN + o.period * i;
			double UTDN = tDN + o.period * i;
			Vector3d pAN = o.getPositionAtUT(UTAN);
			Vector3d pDN = o.getPositionAtUT(UTDN);
			double rotAN = 0, rotDN = 0;
			if(vessel.mainBody.rotates) {
				rotAN = ((360 * ((UTAN - startUT) / vessel.mainBody.rotationPeriod)) % 360);
				rotDN = ((360 * ((UTDN - startUT) / vessel.mainBody.rotationPeriod)) % 360);
			}
			double loAN = vessel.mainBody.GetLongitude(pAN) - rotAN;
			double loDN = vessel.mainBody.GetLongitude(pDN) - rotDN;
			double laAN = 0; // vessel.mainBody.GetLatitude(pAN); // for debugging
			double laDN = 0; // vessel.mainBody.GetLatitude(pDN);
			double lonAN = ((bigmap.projectLongitude(loAN, laAN) + 180) % 360) * maprect.width / 360f;
			double lonDN = ((bigmap.projectLongitude(loDN, laDN) + 180) % 360) * maprect.width / 360f;
			double latAN = ((bigmap.projectLatitude(loAN, laAN) + 90) % 180) * maprect.height / 180f;
			double latDN = ((bigmap.projectLatitude(loDN, laDN) + 90) % 180) * maprect.height / 180f;
			r.x = maprect.x + (float)lonAN;
			r.y = maprect.y + (float)latAN;
			drawLabel(r, col, "↑");
			r.x = maprect.x + (float)lonDN;
			r.y = maprect.y + (float)latDN;
			drawLabel(r, col, "↓");
		}
	}

	private static double meanForTrue(double TA, double e) {
		TA = TA * Mathf.Deg2Rad;
		double EA = Math.Acos((e+Math.Cos(TA))/(1+e*Math.Cos(TA)));
		if(TA > Math.PI) EA = 2 * Math.PI - EA;
		double MA = EA - e * Math.Sin(EA);
		// the mean anomaly isn't really an angle, but I'm a simple person
		return MA * Mathf.Rad2Deg;
	}

	private static void drawGrid(Rect maprect) {
		Rect r = new Rect(0, 0, 50f, 50f);
		for(double lat=-85; lat<90; lat += 5) {
			for(double lon=-175; lon<180; lon += 5) {
				if(lat % 30 == 0 || lon % 30 == 0) {
					r.x = maprect.x + (float)((bigmap.projectLongitude(lon, lat) + 180) % 360) * maprect.width / 360f;
					r.y = maprect.y + (float)((bigmap.projectLatitude(lon, lat) + 90) % 180) * maprect.height / 180f;
					drawLabel(r, Color.white, ".");
				}
			}
		}
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

		if(SCANcontroller.controller.map_orbit) {
			drawOrbit(maprect, vessel);
		}
		if(SCANcontroller.controller.map_grid) {
			drawGrid(maprect);
		}

		if(spotmap != null) {
			spotmap.setBody(vessel.mainBody);
			GUI.Box(pos_spotmap, spotmap.getPartialMap());
			pos_spotmap_x.x = pos_spotmap.x + pos_spotmap.width + 4;
			pos_spotmap_x.y = pos_spotmap.y;
			style_button.normal.textColor = cb_vermillion;
			if(GUI.Button(pos_spotmap_x, "✗", style_button)) {
				spotmap = null;
			}
		}

		GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		GUILayout.BeginHorizontal(GUILayout.Width(300));
		if(GUILayout.Button("Close")) {
			bigmap_visible = false;
		}

		GUILayout.BeginVertical();
		style_button.normal.textColor = SCANcontroller.controller.colours == 1 ? cb_skyBlue : Color.white;
		if(GUILayout.Button("Grey", style_button)) {
			SCANcontroller.controller.colours = 1;
			data.resetImages();
			bigmap.resetMap();
		}
		style_button.normal.textColor = SCANcontroller.controller.colours == 0 ? cb_skyBlue : Color.white;
		if(GUILayout.Button("Colour", style_button)) {
			SCANcontroller.controller.colours = 0;
			data.resetImages();
			bigmap.resetMap();
		}
		GUILayout.EndVertical();

		GUILayout.BeginVertical();
		style_button.normal.textColor = bigmap.mapmode == 0 ? cb_skyBlue : Color.white;
		if(GUILayout.Button("Altimetry", style_button)) {
			bigmap.resetMap(0);
		}
		style_button.normal.textColor = bigmap.mapmode == 1 ? cb_skyBlue : Color.white;
		if(GUILayout.Button("Slope", style_button)) {
			bigmap.resetMap(1);
		}
		style_button.normal.textColor = bigmap.mapmode == 2 ? cb_skyBlue : Color.white;
		if(GUILayout.Button("Biome", style_button)) {
			bigmap.resetMap(2);
		}
		GUILayout.EndVertical();

		GUILayout.BeginVertical();
		style_button.normal.textColor = SCANcontroller.controller.map_markers ? cb_skyBlue : Color.white;
		if(GUILayout.Button("Markers", style_button)) {
			SCANcontroller.controller.map_markers = !SCANcontroller.controller.map_markers;
		}
		style_button.normal.textColor = SCANcontroller.controller.map_orbit ? cb_skyBlue : Color.white;
		if(GUILayout.Button("Orbit", style_button)) {
			SCANcontroller.controller.map_orbit = !SCANcontroller.controller.map_orbit;
		}
		style_button.normal.textColor = SCANcontroller.controller.map_grid ? cb_skyBlue : Color.white;
		if(GUILayout.Button("Grid", style_button)) {
			SCANcontroller.controller.map_grid = !SCANcontroller.controller.map_grid;
		}
		GUILayout.EndVertical();

		GUILayout.BeginVertical();
		for(int i=0; i<SCANmap.projectionNames.Length; ++i) {
			style_button.normal.textColor = (int)bigmap.projection == i ? cb_skyBlue : Color.white;
			if(GUILayout.Button(SCANmap.projectionNames[i], style_button)) {
				bigmap.setProjection((SCANmap.MapProjection)i);
				SCANcontroller.controller.projection = i;
			}
		}
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		string info = "";
		float mx = Event.current.mousePosition.x - maprect.x;
		float my = Event.current.mousePosition.y - maprect.y;
		if(mx >= 0 && my >= 0 && mx < map.width && my < map.height) {		
			double mlo = (mx * 360f / map.width) - 180;
			double mla = 90 - (my * 180f / map.height);
			double mlon = bigmap.unprojectLongitude(mlo, mla);
			double mlat = bigmap.unprojectLatitude(mlo, mla);

			bool in_spotmap = false;
			if(spotmap != null) {
				if(mx >= pos_spotmap.x-maprect.x && my >= pos_spotmap.y-maprect.y && mx <= pos_spotmap.x+pos_spotmap.width-maprect.x && my <= pos_spotmap.y+pos_spotmap.height-maprect.y) {
					in_spotmap = true;
					mlon = spotmap.lon_offset + ((mx-pos_spotmap.x+maprect.x) / spotmap.mapscale) - 180;
					mlat = spotmap.lat_offset + ((pos_spotmap.height-(my-pos_spotmap.y+maprect.y)) / spotmap.mapscale) - 90;
				}
			}

			if(mlon >= -180 && mlon < 180 && mlat >= -90 && mlat < 90) {
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
					info += "<b>" + data.getElevation(mlon, mlat).ToString("N2") + "m</b> ";
				} else if(data.isCovered(mlon, mlat, SCANdata.SCANtype.AltimetryLoRes)) {
					info += "<b>~" + (((int)data.getElevation(mlon, mlat) / 500) * 500).ToString() + "m</b> ";
				}
				if(data.isCovered(mlon, mlat, SCANdata.SCANtype.Biome)) {
					info += data.getBiomeName(mlon, mlat) + " ";
				}
				info += "\n" + toDMS(mlat, mlon) + " (lat: " + mlat.ToString("F2") + " lon: " + mlon.ToString("F2") + ") ";
				//info += mx.ToString("F0") + "," + my.ToString("F0") + " ";
				if(in_spotmap) info += " " + spotmap.mapscale.ToString("F1") + "x";
			}

			if(Event.current.isMouse) {
				if(Event.current.type == EventType.MouseUp) {
					if(Event.current.button == 1) {
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
				} else if(Event.current.button == 0) {
					if(spotmap != null) {
						if(in_spotmap) {
							spotmap.mapscale = spotmap.mapscale / 1.25f;
							if(spotmap.mapscale < 10) spotmap.mapscale = 10;
							spotmap.resetMap(spotmap.mapmode);
							Event.current.Use();
						}
					}
				}
			}
		}
		GUILayout.Label(info);
		GUILayout.EndHorizontal();

		addVesselLabel(maprect, bigmap, 0, vessel);

		if(SCANcontroller.controller.map_markers) {
			foreach(SCANdata.SCANanomaly anomaly in data.getAnomalies()) {
				addAnomalyLabel(maprect, bigmap, anomaly);
			}
		}

		GUILayout.EndVertical();
		GUI.DragWindow();
	}

	public static void gui_show() {
		bool repainting = Event.current.type == EventType.Repaint;
		Vessel vessel = FlightGlobals.ActiveVessel;

		if(Time.frameCount - gui_frame_ping > 5) {
			gui_active = false;
			RenderingManager.RemoveFromPostDrawQueue(3, guicb);
		}
		gui_frame_draw = Time.frameCount;

		if(style_button == null) {
			style_button = new GUIStyle(GUI.skin.button);
		}

		GUI.skin.label.wordWrap = false;
		GUI.skin.button.wordWrap = false;

		SCANdata.SCANtype sensors = SCANcontroller.controller.activeSensorsOnVessel(vessel.id);
		SCANdata data = SCANcontroller.controller.getData(vessel.mainBody);
		data.updateImages(sensors);

		if(!repainting) { // Unity gets confused if the layout changes between layout and repaint events
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
			if(vth > 2000 || vth < 0) {
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
			if(bigmap == null) {
				bigmap = new SCANmap();
				bigmap.setProjection((SCANmap.MapProjection)SCANcontroller.controller.projection);
			}
			bigmap.setBody(vessel.mainBody);
			bigmap.setSize(0, 0);
			string rendering = "";
			if(!bigmap.isMapComplete()) rendering += " [rendering]";
			pos_bigmap = GUILayout.Window(47110002, pos_bigmap, gui_bigmap_build, "Map of " + vessel.mainBody.theName + rendering, GUILayout.Width(800), GUILayout.Height(600));
		}
	}

	public static void gui_ping() {
		if(Time.frameCount - gui_frame_draw > 5) {
			// UI isn't working, try turning it off and on again
			RenderingManager.RemoveFromPostDrawQueue(3, guicb);
			gui_active = false;
		}
		gui_frame_ping = Time.frameCount;
		if(!gui_active) {
			RenderingManager.AddToPostDrawQueue(3, guicb);
			gui_active = true;
		}
	}
}
