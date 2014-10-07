

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using palette = SCANsat.SCANpalette;
using UnityEngine;

namespace SCANsat.SCAN_UI
{
	static class SCANuiUtil
	{

		internal static bool readableLabel(string text, bool active)//, GUIStyle s)
		{
			string textpoor = Regex.Replace(text, "<[^>]*>", "");
			GUI.color = palette.black;
			GUILayout.Label(textpoor, SCANskins.SCAN_shadowLabel);
			Rect r = GUILayoutUtility.GetLastRect();
			r.x += 2;
			GUI.Label(r, textpoor, SCANskins.SCAN_shadowLabel);
			r.x -= 1;
			r.y -= 1;
			GUI.Label(r, textpoor, SCANskins.SCAN_shadowLabel);
			r.y += 2;
			GUI.Label(r, textpoor, SCANskins.SCAN_shadowLabel);
			r.y -= 1;
			GUI.color = palette.white;
			if (active)
				GUI.Label(r, text, SCANskins.SCAN_colorLabel);
			else
				GUI.Label(r, text, SCANskins.SCAN_inactiveLabel);
			if (!Event.current.isMouse)
				return false;
			return r.Contains(Event.current.mousePosition);
		}

		internal static void drawVesselLabel(Rect maprect, SCANmap map, int num, Vessel vessel)
		{
			bool flash = false;
			double lon = SCANUtil.fixLon(vessel.longitude);
			double lat = SCANUtil.fixLat(vessel.latitude);
			if (map != null)
			{
				lat = (map.projectLatitude(vessel.longitude, vessel.latitude) + 90) % 180;
				lon = (map.projectLongitude(vessel.longitude, vessel.latitude) + 180) % 360;
				lat = map.scaleLatitude(lat);
				lon = map.scaleLongitude(lon);
				if (lat < 0 || lon < 0 || lat > 180 || lon > 360)
					return;
			}
			lon = lon * maprect.width / 360f;
			lat = maprect.height - lat * maprect.height / 180f;
			string txt = num.ToString();
			if (num == 0)
				txt = vessel.vesselName;
			else if (num < 0)
				txt = "";
			Rect r = new Rect(maprect.x + (float)lon, maprect.y + (float)lat, 250f, 25f);
			Color col = palette.white;
			if (SCANcontroller.controller.colours == 1 && vessel != FlightGlobals.ActiveVessel)
				col = palette.cb_skyBlue;
			if (vessel == FlightGlobals.ActiveVessel && (int)(Time.realtimeSinceStartup % 2) == 0)
			{
				flash = true;
				col = palette.cb_yellow;
			}
			int sz = 16;
			if (vessel.vesselType == VesselType.Flag)
				sz = 24;
			Icon.drawOrbitIcon((int)r.x, (int)r.y, Icon.orbitIconForVesselType(vessel.vesselType), col, sz, true);
			if (maprect.width < 360)
				return;
			r.x += 12;
			drawLabel(r, txt, flash, true, true);
		}

		internal static void drawLabel(Rect r, string txt, bool Flash, bool aligned, bool outline)
		{
			if (txt.Length < 1)
				return;
			if (aligned)
			{
				Vector2 sz = SCANskins.SCAN_orbitalLabelOn.CalcSize(new GUIContent(txt.Substring(0, 1)));
				r.x -= sz.x / 2;
				r.y -= sz.y / 2;
			}
			if (outline)
			{
				r.x -= 1;
				GUI.Label(r, txt, SCANskins.SCAN_shadowLabel);
				r.x += 2;
				GUI.Label(r, txt, SCANskins.SCAN_shadowLabel);
				r.x -= 1;
				r.y -= 1;
				GUI.Label(r, txt, SCANskins.SCAN_shadowLabel);
				r.y += 2;
				GUI.Label(r, txt, SCANskins.SCAN_shadowLabel);
				r.y -= 1;
			}
			if (Flash)
				GUI.Label(r, txt, SCANskins.SCAN_orbitalLabelOn);
			else
				GUI.Label(r, txt, SCANskins.SCAN_orbitalLabelOff);
		}

		internal static void drawGrid(Rect maprect, SCANmap map, Texture2D overlay_static)
		{
			int x, y;
			for (double lat = -90; lat < 90; lat += 2)
			{
				for (double lon = -180; lon < 180; lon += 2)
				{
					if (lat % 30 == 0 || lon % 30 == 0)
					{
						x = (int)(map.mapscale * ((map.projectLongitude(lon, lat) + 180) % 360));
						y = (int)(map.mapscale * ((map.projectLatitude(lon, lat) + 90) % 180));
						drawDot(x, y, palette.white, overlay_static);
					}
				}
			}
		}

		private static void drawDot(int x, int y, Color c, Texture2D tex)
		{
			tex.SetPixel(x, y, c);
			tex.SetPixel(x - 1, y, palette.black);
			tex.SetPixel(x + 1, y, palette.black);
			tex.SetPixel(x, y - 1, palette.black);
			tex.SetPixel(x, y + 1, palette.black);
		}

		internal static void drawMapLabels(Rect maprect, Vessel vessel, SCANmap map, SCANdata data)
		{
			foreach (Vessel v in FlightGlobals.Vessels)
			{
				if (v.mainBody == vessel.mainBody)
				{
					if (v.vesselType == VesselType.Flag && SCANcontroller.controller.map_flags)
					{
						drawVesselLabel(maprect, map, 0, v);
					}
					if (v.vesselType == VesselType.SpaceObject && SCANcontroller.controller.map_asteroids)
					{
						drawVesselLabel(maprect, map, 0, v);
					}
				}
			}
			if (SCANcontroller.controller.map_markers)
			{
				foreach (SCANdata.SCANanomaly anomaly in data.getAnomalies())
				{
					drawAnomalyLabel(maprect, map, anomaly);
				}
			}
			drawVesselLabel(maprect, map, 0, vessel);
		}

		internal static void drawAnomalyLabel(Rect maprect, SCANmap map, SCANdata.SCANanomaly anomaly)
		{
			if (!anomaly.known)
				return;
			double lon = (anomaly.longitude + 360 + 180) % 360;
			double lat = (anomaly.latitude + 180 + 90) % 180;
			if (map != null)
			{
				lat = (map.projectLatitude(anomaly.longitude, anomaly.latitude) + 90) % 180;
				lon = (map.projectLongitude(anomaly.longitude, anomaly.latitude) + 180) % 360;
				lat = map.scaleLatitude(lat);
				lon = map.scaleLongitude(lon);
				if (lat < 0 || lon < 0 || lat > 180 || lon > 360)
					return;
			}
			lon = lon * maprect.width / 360f;
			lat = maprect.height - lat * maprect.height / 180f;
			string txt = SCANcontroller.controller.anomalyMarker + " " + anomaly.name;
			if (!anomaly.detail)
				txt = SCANcontroller.controller.anomalyMarker + " Anomaly";
			Rect r = new Rect(maprect.x + (float)lon, maprect.y + (float)lat, 250f, 25f);
			drawLabel(r, txt, true, true, true);
		}

		/* FIXME: This may use assumed, shared, static constants with Legend stuff in other SCANsat files */
		internal static void drawLegendLabel(Rect r, float val, float min, float max)
		{
			if (val < min || val > max)
				return;
			float scale = r.width * 1f / (max - min);
			float x = r.x + scale * (val - min);
			Rect lr = new Rect(x, r.y + r.height / 4, r.width - x, r.height);
			drawLabel(lr, "|", true, true, false);
			string txt = val.ToString("N0");
			GUIContent c = new GUIContent(txt);
			Vector2 dim = SCANskins.SCAN_whiteLabel.CalcSize(c);
			lr.y += dim.y * 0.25f;
			lr.x -= dim.x / 2;
			if (lr.x < r.x)
				lr.x = r.x;
			if (lr.x + dim.x > r.x + r.width)
				lr.x = r.x + r.width - dim.x;
			drawLabel(lr, txt, false, true, false);
		}

		/* FIXME: This uses assumed, shared, static constants with Legend stuff in other SCANsat files */
		internal static void drawLegend(SCANmap bigmap)
		{
			if (bigmap.mapmode == 0 && SCANcontroller.controller.legend)
			{
				GUILayout.Label("", GUILayout.ExpandWidth(true));
				Rect r = GUILayoutUtility.GetLastRect();
				r.width -= 64;
				GUI.DrawTexture(r, SCANmap.getLegend(-1500f, 9000f, SCANcontroller.controller.colours));
				for (float val = -1000f; val < 9000f; val += 1000f)
				{
					drawLegendLabel(r, val, -1500f, 9000f);
				}
			}
		}

		internal static string distanceString(double dist)
		{
			if (dist < 5000)
				return dist.ToString("N1") + "m";
			return (dist / 1000d).ToString("N3") + "km";
		}

		internal static void resetMainMapPos()
		{

		}

		internal static void resetSettingsUIPos()
		{

		}

		internal static void resetInstUIPos()
		{

		}

		internal static void resetBigMapPos()
		{

		}

		internal static void resetKSCMapPos()
		{

		}

	}
}
