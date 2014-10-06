

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
				GUI.Label(r, text, SCANskins.SCAN_label);
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

	}
}
