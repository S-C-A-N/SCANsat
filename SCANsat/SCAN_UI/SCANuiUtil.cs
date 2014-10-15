#region license
/* 
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - UI Utilities methods
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
using palette = SCANsat.SCANpalette;
using UnityEngine;

namespace SCANsat.SCAN_UI
{
	static class SCANuiUtil
	{

		#region Text labels

		//Generates a text label with a black border
		//Basically generates several offset black labels with a color label on top of it
		internal static bool readableLabel(string text, bool active)
		{
			string textpoor = Regex.Replace(text, "<[^>]*>", "");
			GUI.color = palette.black;
			GUILayout.Label(textpoor, SCANskins.SCAN_shadowReadoutLabel);
			Rect r = GUILayoutUtility.GetLastRect();
			r.x += 2;
			GUI.Label(r, textpoor, SCANskins.SCAN_shadowReadoutLabel);
			r.x -= 1;
			r.y -= 1;
			GUI.Label(r, textpoor, SCANskins.SCAN_shadowReadoutLabel);
			r.y += 2;
			GUI.Label(r, textpoor, SCANskins.SCAN_shadowReadoutLabel);
			r.y -= 1;
			GUI.color = palette.white;
			if (active)
				GUI.Label(r, text, SCANskins.SCAN_readoutLabel);
			else
				GUI.Label(r, text, SCANskins.SCAN_whiteReadoutLabel);
			if (!Event.current.isMouse)
				return false;
			return r.Contains(Event.current.mousePosition);
		}

		//Used to label icons drawn over the map; similar to readableLabel
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
				GUI.Label(r, txt, SCANskins.SCAN_shadowReadoutLabel);
				r.x += 2;
				GUI.Label(r, txt, SCANskins.SCAN_shadowReadoutLabel);
				r.x -= 1;
				r.y -= 1;
				GUI.Label(r, txt, SCANskins.SCAN_shadowReadoutLabel);
				r.y += 2;
				GUI.Label(r, txt, SCANskins.SCAN_shadowReadoutLabel);
				r.y -= 1;
			}
			if (Flash)
				GUI.Label(r, txt, SCANskins.SCAN_orbitalLabelOn);
			else
				GUI.Label(r, txt, SCANskins.SCAN_orbitalLabelOff);
		}

		#endregion

		#region UI Utilities

		//Generates a string with info from mousing over the map
		internal static void mouseOverInfo(double lon, double lat, SCANmap mapObj, SCANdata data, CelestialBody body, bool b)
		{
			string info = "";
			string posInfo = "";

			if (b)
			{
				if (SCANUtil.isCovered(lon, lat, data, SCANdata.SCANtype.AltimetryLoRes))
				{
					if (body.pqsController == null)
						info += palette.colored(palette.c_ugly, "LO ");
					else
						info += palette.colored(palette.c_good, "LO ");
				}
				else
					info += palette.colored(palette.grey, "LO ");
				if (SCANUtil.isCovered(lon, lat, data, SCANdata.SCANtype.AltimetryHiRes))
				{
					if (body.pqsController == null)
						info += palette.colored(palette.c_ugly, "HI ");
					else
						info += palette.colored(palette.c_good, "HI ");
				}
				else
					info += palette.colored(palette.grey, "HI ");
				if (SCANUtil.isCovered(lon, lat, data, SCANdata.SCANtype.Biome))
				{
					if (body.BiomeMap == null || body.BiomeMap.Map == null)
						info += palette.colored(palette.c_ugly, "MULTI ");
					else
						info += palette.colored(palette.c_good, "MULTI ");
				}
				else
					info += palette.colored(palette.grey, "MULTI ");
				if (SCANUtil.isCovered(lon, lat, data, SCANdata.SCANtype.AltimetryHiRes))
				{
					info += SCANUtil.getElevation(body, lon, lat).ToString("N2") + "m ";
				}
				else if (SCANUtil.isCovered(lon, lat, data, SCANdata.SCANtype.AltimetryLoRes))
				{
					info += (((int)SCANUtil.getElevation(body, lon, lat) / 500) * 500).ToString() + "m ";
				}
				if (SCANUtil.isCovered(lon, lat, data, SCANdata.SCANtype.Biome))
				{
					info += SCANUtil.getBiomeName(body, lon, lat) + " ";
				}

				if (SCANcontroller.controller.map_ResourceOverlay && SCANcontroller.controller.globalOverlay) //Adds selected resource amount to big map legend
				{
					if (SCANcontroller.controller.resourceOverlayType == 0 && SCANreflection.ORSXFound)
					{
						if (SCANUtil.isCovered(lon, lat, data, mapObj.resource.type))
						{
							double amount = SCANUtil.ORSOverlay(lon, lat, mapObj.body.flightGlobalsIndex, mapObj.resource.name);
							string label;
							if (mapObj.resource.linear) //Make sure that ORS values are handled correctly based on which scale type they use
								label = (amount * 100).ToString("N1") + " %";
							else
								label = (amount * 1000000).ToString("N1") + " ppm";
							info += palette.colored(mapObj.resource.fullColor, mapObj.resource.name + ": " + label);
						}
					}
					else if (SCANcontroller.controller.resourceOverlayType == 1)
					{
						if (SCANUtil.isCovered(lon, lat, data, mapObj.resource.type))
						{
							double amount = data.kethaneValueMap[SCANUtil.icLON(lon), SCANUtil.icLAT(lat)];
							if (amount < 0) amount = 0d;
							info += palette.colored(mapObj.resource.fullColor, mapObj.resource.name + ": " + amount.ToString("N1"));
						}
					}
				}

				posInfo += string.Format("{0} (lat: {1:F2} lon: {2:F2})", toDMS(lat, lon), lat, lon);
			}
			//else
			//{
			//	info += " " + mlat.ToString("F") + " " + mlon.ToString("F"); // uncomment for debugging projections
			//}

			//Draw the readout info labels
			readableLabel(info, false);
			MBW.fillS(-10);
			readableLabel(posInfo, false);
		}

		//Method to handle active scanner display
		internal static string InfoText(Vessel v, SCANdata data, bool b)
		{
			string infotext = "";

			SCANcontroller.SCANsensor s;

			//Check here for each sensor; if active, in range, and at the ideal altitude
			s = SCANcontroller.controller.getSensorStatus(v, SCANdata.SCANtype.AltimetryLoRes);
			if (s == null)
				infotext += palette.colored(palette.grey, "LO");
			else if (!s.inRange)
				infotext += palette.colored(palette.c_bad, "LO");
			else if (!s.bestRange && (Time.realtimeSinceStartup % 2 < 1))
				infotext += palette.colored(palette.c_bad, "LO");
			else
				infotext += palette.colored(palette.c_good, "LO");

			s = SCANcontroller.controller.getSensorStatus(v, SCANdata.SCANtype.AltimetryHiRes);
			if (s == null)
				infotext += palette.colored(palette.grey, " HI");
			else if (!s.inRange)
				infotext += palette.colored(palette.c_bad, " HI");
			else if (!s.bestRange && (Time.realtimeSinceStartup % 2 < 1))
				infotext += palette.colored(palette.c_bad, " HI");
			else
				infotext += palette.colored(palette.c_good, " HI");

			s = SCANcontroller.controller.getSensorStatus(v, SCANdata.SCANtype.Biome);
			if (s == null)
				infotext += palette.colored(palette.grey, " MULTI");
			else if (!s.inRange)
				infotext += palette.colored(palette.c_bad, " MULTI");
			else if (!s.bestRange && (Time.realtimeSinceStartup % 2 < 1))
				infotext += palette.colored(palette.c_bad, " MULTI");
			else
				infotext += palette.colored(palette.c_good, " MULTI");

			s = SCANcontroller.controller.getSensorStatus(v, SCANdata.SCANtype.AnomalyDetail);
			if (s == null)
				infotext += palette.colored(palette.grey, " BTDT");
			else if (!s.inRange)
				infotext += palette.colored(palette.c_bad, " BTDT");
			else if (!s.bestRange && (Time.realtimeSinceStartup % 2 < 1))
				infotext += palette.colored(palette.c_bad, " BTDT");
			else
				infotext += palette.colored(palette.c_good, " BTDT");

			//Get coverage percentage for all active scanners on the vessel
			SCANdata.SCANtype active = SCANcontroller.controller.activeSensorsOnVessel(v.id);
			if (active != SCANdata.SCANtype.Nothing)
			{
				double cov = data.getCoveragePercentage(active);
				infotext += string.Format(" {0:N1}%", cov);
				if (b)
				{
					infotext = palette.colored(palette.c_bad, "NO POWER");
				}
			}

			return infotext;
		}

		/* UI: conversions to and from DMS */
		/* FIXME: These do not belong here. And they are only used once! */
		internal static string toDMS(double thing, string neg, string pos)
		{
			string dms = "";
			if (thing >= 0)
				neg = pos;
			thing = Math.Abs(thing);
			dms += Math.Floor(thing).ToString() + "°";
			thing = (thing - Math.Floor(thing)) * 60;
			dms += Math.Floor(thing).ToString() + "'";
			thing = (thing - Math.Floor(thing)) * 60;
			dms += thing.ToString("F2") + "\"";
			dms += neg;
			return dms;
		}

		internal static string toDMS(double lat, double lon)
		{
			return string.Format("{0} {1}", toDMS(lat, "S", "N"), toDMS(lon, "W", "E"));
		}

		internal static string distanceString(double dist, double cutoff)
		{
			if (dist < cutoff)
				return string.Format("{0:N1}m", dist);
			return string.Format("{0:N2}km", dist / 1000d);
		}

		internal static void clearTexture(Texture2D tex)
		{
			Color[] pix = tex.GetPixels();
			for (int i = 0; i < pix.Length; ++i)
				pix[i] = palette.clear;
			tex.SetPixels(pix);
		}

		//Reset window positions;
		internal static void resetMainMapPos()
		{
			SCANcontroller.controller.mainMap.resetWindowPos(SCANmainMap.defaultRect);
		}

		internal static void resetSettingsUIPos()
		{
			SCANcontroller.controller.settingsWindow.resetWindowPos(SCANsettingsUI.defaultRect);
		}

		internal static void resetInstUIPos()
		{
			SCANcontroller.controller.instrumentsWindow.resetWindowPos(SCANinstrumentUI.defaultRect);
		}

		internal static void resetBigMapPos()
		{
			SCANcontroller.controller.bigMap.resetWindowPos(SCANbigMap.defaultRect);
		}

		internal static void resetKSCMapPos()
		{
			SCANcontroller.controller.kscMap.resetWindowPos(SCANkscMap.defaultRect);
		}

		#endregion

		#region Texture/Icon labels

		//Method to actually draw vessel labels on the map
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

		//Handles various map labels; probably should be split up into multiple methods
		internal static void drawMapLabels(Rect maprect, Vessel vessel, SCANmap map, SCANdata data, CelestialBody body)
		{
			//This section handles flag and asteroid labels
			foreach (Vessel v in FlightGlobals.Vessels)
			{
				if (v.mainBody == body)
				{
					if (MapView.OrbitIconsMap != null)
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
			}
			//This section handles anomaly labels
			if (SCANcontroller.controller.map_markers)
			{
				foreach (SCANdata.SCANanomaly anomaly in data.getAnomalies())
				{
					drawAnomalyLabel(maprect, map, anomaly);
				}
			}
			if (vessel != null)
				drawVesselLabel(maprect, map, 0, vessel);
		}

		//Method to draw anomaly labels on the map
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
			drawLabel(lr, "|", false, false, true);
			string txt = val.ToString("N0");
			GUIContent c = new GUIContent(txt);
			Vector2 dim = SCANskins.SCAN_whiteReadoutLabel.CalcSize(c);
			lr.y += dim.y * 0.25f;
			lr.x -= dim.x / 2;
			if (lr.x < r.x)
				lr.x = r.x;
			if (lr.x + dim.x > r.x + r.width)
				lr.x = r.x + r.width - dim.x;
			drawLabel(lr, txt, false, false, true);
		}

		/* FIXME: This uses assumed, shared, static constants with Legend stuff in other SCANsat files */
		internal static void drawLegend()
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

		#endregion

		#region Overlays

		//Setup the lat/long grid
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

		//Draw the actual dot textures for the grid
		private static void drawDot(int x, int y, Color c, Texture2D tex)
		{
			tex.SetPixel(x, y, c);
			tex.SetPixel(x - 1, y, palette.black);
			tex.SetPixel(x + 1, y, palette.black);
			tex.SetPixel(x, y - 1, palette.black);
			tex.SetPixel(x, y + 1, palette.black);
		}

		//Draw the orbit overlay; needs to be replaced with the method used by SCANsatRPM
		internal static void drawOrbit(Rect maprect, SCANmap map, Vessel vessel, double startUT, Texture2D overlay_static)
		{
			int eqh = 16;

			if (vessel.LandedOrSplashed)
				return;
			bool lite = maprect.width < 400;
			Orbit o = vessel.orbit;
			startUT = Planetarium.GetUniversalTime();
			double UT = startUT;
			int steps = 100; // increase for neater lines, decrease for better speed indication
			bool ath = false;
			if (vessel.mainBody.atmosphere)
			{
				if (vessel.mainBody.maxAtmosphereAltitude >= vessel.altitude)
				{
					ath = true;
				}
			}
			Rect r = new Rect(0, 0, 50f, 50f);
			Color col;
			// project the last and the current orbital period onto the map
			for (int i = -steps; i < steps; ++i)
			{
				if (i < 0)
					UT = startUT - (steps + i) * (o.period / steps);
				else
					UT = startUT + i * o.period * 1f / steps;
				if (double.IsNaN(UT))
					continue;
				if (UT < o.StartUT && o.StartUT != startUT)
					continue;
				if (UT > o.EndUT)
					continue;
				if (double.IsNaN(o.getObtAtUT(UT)))
					continue;
				Vector3d pos = o.getPositionAtUT(UT);
				double rotation = 0;
				if (vessel.mainBody.rotates)
				{
					rotation = (360 * ((UT - startUT) / vessel.mainBody.rotationPeriod)) % 360;
				}
				double alt = (vessel.mainBody.GetAltitude(pos));
				if (alt < 0)
				{
					if (i < 0)
					{
						i = 0;
						continue;
					}
					break;
				}
				double lo = (vessel.mainBody.GetLongitude(pos) - rotation);
				double la = (vessel.mainBody.GetLatitude(pos));
				double lon = (map.projectLongitude(lo, la) + 180) % 360;
				double lat = (map.projectLatitude(lo, la) + 90) % 180;
				lat = map.scaleLatitude(lat);
				lon = map.scaleLongitude(lon);
				if (lat < 0 || lon < 0 || lat > 180 || lon > 360)
					continue;
				lon = lon * maprect.width / 360f;
				lat = maprect.height - lat * maprect.height / 180f;
				r.x = maprect.x + (float)lon;
				r.y = maprect.y + (float)lat;
				col = palette.cb_skyBlue;
				if (i < 0)
				{
					col = palette.cb_orange;
				}
				else
				{
					if (vessel.mainBody.atmosphere)
					{
						if (vessel.mainBody.maxAtmosphereAltitude >= alt)
						{
							if (!ath)
							{
								ath = true;
								// do something when it flips?
							}
							col = palette.cb_reddishPurple;
						}
					}
				}
				Icon.drawOrbitIcon((int)r.x, (int)r.y, Icon.OrbitIcon.Planet, col, 8, false);
			}

			// show apoapsis and periapsis
			if (o.ApA > 0 && mapPosAtT(maprect, map, ref r, vessel, o, o.timeToAp, startUT))
			{
				Icon.drawOrbitIcon((int)r.x, (int)r.y, Icon.OrbitIcon.Ap, palette.cb_skyBlue, 32, true);
				r.x += 24;
				r.y -= 12;
				if (!lite)
					drawLabel(r, o.ApA.ToString("N1"), true, true, true);
			}
			if (o.PeA > 0 && mapPosAtT(maprect, map, ref r, vessel, o, o.timeToPe, startUT))
			{
				Icon.drawOrbitIcon((int)r.x, (int)r.y, Icon.OrbitIcon.Pe, palette.cb_skyBlue, 32, true);
				r.x += 24;
				r.y -= 12;
				if (!lite)
					drawLabel(r, o.PeA.ToString("N1"), true, true, true);
			}

			if (lite)
				return;

			// show first maneuver node
			if (vessel.patchedConicSolver.maneuverNodes.Count > 0)
			{
				ManeuverNode n = vessel.patchedConicSolver.maneuverNodes[0];
				if (n.patch == vessel.orbit && n.nextPatch != null && n.nextPatch.activePatch && n.UT > startUT - o.period && mapPosAtT(maprect, map, ref r, vessel, o, n.UT - startUT, startUT))
				{
					col = palette.cb_reddishPurple;
					if (SCANcontroller.controller.colours != 1)
						col = palette.xkcd_PurplyPink;
					Icon.drawOrbitIcon((int)r.x, (int)r.y, Icon.OrbitIcon.ManeuverNode, col, 32, true);
					Orbit nuo = n.nextPatch;
					for (int i = 0; i < steps; ++i)
					{
						double T = n.UT - startUT + i * nuo.period / steps;
						if (T + startUT > nuo.EndUT)
							break;
						if (mapPosAtT(maprect, map, ref r, vessel, nuo, T, startUT))
						{
							Icon.drawOrbitIcon((int)r.x, (int)r.y, Icon.OrbitIcon.Planet, col, 8, false);
						}
					}
					if (nuo.patchEndTransition == Orbit.PatchTransitionType.ESCAPE)
					{
						Icon.drawOrbitIcon((int)r.x, (int)r.y, Icon.OrbitIcon.Exit, col, 32, true);
					}
					else if (nuo.patchEndTransition == Orbit.PatchTransitionType.ENCOUNTER)
					{
						Icon.drawOrbitIcon((int)r.x, (int)r.y, Icon.OrbitIcon.Encounter, col, 32, true);
					}
					if (nuo.timeToAp > 0 && n.UT + nuo.timeToAp < nuo.EndUT && mapPosAtT(maprect, map, ref r, vessel, nuo, n.UT - startUT + nuo.timeToAp, startUT))
					{
						Icon.drawOrbitIcon((int)r.x, (int)r.y, Icon.OrbitIcon.Ap, col, 32, true);
					}
					if (nuo.timeToPe > 0 && n.UT + nuo.timeToPe < nuo.EndUT && mapPosAtT(maprect, map, ref r, vessel, nuo, n.UT - startUT + nuo.timeToPe, startUT))
					{
						Icon.drawOrbitIcon((int)r.x, (int)r.y, Icon.OrbitIcon.Pe, col, 32, true);
					}
				}
			}

			if (o.PeA < 0)
				return;
			if (overlay_static == null)
				return;
			if (map.projection == SCANmap.MapProjection.Polar)
				return;

			if (SCANbigMap.eq_frame <= 0)
			{
				// predict equatorial crossings for the next 100 loops
				double TAAN = 360f - o.argumentOfPeriapsis;	// true anomaly at ascending node
				double TADN = (TAAN + 180) % 360;			// true anomaly at descending node
				double MAAN = meanForTrue(TAAN, o.eccentricity);
				double MADN = meanForTrue(TADN, o.eccentricity);
				double tAN = (((MAAN - o.meanAnomaly * Mathf.Rad2Deg + 360) % 360) / 360f * o.period + startUT);
				double tDN = (((MADN - o.meanAnomaly * Mathf.Rad2Deg + 360) % 360) / 360f * o.period + startUT);

				if (SCANbigMap.eq_an_map == null || SCANbigMap.eq_dn_map == null || SCANbigMap.eq_an_map.Length != overlay_static.width)
				{
					SCANbigMap.eq_an_map = new int[overlay_static.width];
					SCANbigMap.eq_dn_map = new int[overlay_static.width];
				}
				if (SCANbigMap.eq_map == null || SCANbigMap.eq_map.width != SCANbigMap.eq_an_map.Length)
				{
					SCANbigMap.eq_map = new Texture2D(SCANbigMap.eq_an_map.Length, eqh, TextureFormat.ARGB32, false);
				}
				for (int i = 0; i < SCANbigMap.eq_an_map.Length; ++i)
				{
					SCANbigMap.eq_an_map[i] = 0;
					SCANbigMap.eq_dn_map[i] = 0;
				}
				for (int i = 0; i < 100; ++i)
				{
					double UTAN = tAN + o.period * i;
					double UTDN = tDN + o.period * i;
					if (double.IsNaN(UTAN) || double.IsNaN(UTDN))
						continue;
					Vector3d pAN = o.getPositionAtUT(UTAN);
					Vector3d pDN = o.getPositionAtUT(UTDN);
					double rotAN = 0, rotDN = 0;
					if (vessel.mainBody.rotates)
					{
						rotAN = ((360 * ((UTAN - startUT) / vessel.mainBody.rotationPeriod)) % 360);
						rotDN = ((360 * ((UTDN - startUT) / vessel.mainBody.rotationPeriod)) % 360);
					}
					double loAN = vessel.mainBody.GetLongitude(pAN) - rotAN;
					double loDN = vessel.mainBody.GetLongitude(pDN) - rotDN;
					int lonAN = (int)(((map.projectLongitude(loAN, 0) + 180) % 360) * SCANbigMap.eq_an_map.Length / 360f);
					int lonDN = (int)(((map.projectLongitude(loDN, 0) + 180) % 360) * SCANbigMap.eq_dn_map.Length / 360f);
					if (lonAN >= 0 && lonAN < SCANbigMap.eq_an_map.Length)
						SCANbigMap.eq_an_map[lonAN] += 1;
					if (lonDN >= 0 && lonDN < SCANbigMap.eq_dn_map.Length)
						SCANbigMap.eq_dn_map[lonDN] += 1;
				}
				Color[] pix = SCANbigMap.eq_map.GetPixels(0, 0, SCANbigMap.eq_an_map.Length, eqh);
				Color cAN = palette.cb_skyBlue, cDN = palette.cb_orange;
				for (int y = 0; y < eqh; ++y)
				{
					Color lc = palette.clear;
					for (int x = 0; x < SCANbigMap.eq_an_map.Length; ++x)
					{
						Color c = palette.clear;
						float scale = 0;
						if (y < eqh / 2)
						{
							c = cDN;
							scale = SCANbigMap.eq_dn_map[x];
						}
						else
						{
							c = cAN;
							scale = SCANbigMap.eq_an_map[x];
						}
						if (scale >= 1)
						{
							if (y == 0 || y == eqh - 1)
							{
								c = palette.black;
							}
							else
							{
								if (lc == palette.clear)
									pix[y * SCANbigMap.eq_an_map.Length + x - 1] = palette.black;
								scale = Mathf.Clamp(scale - 1, 0, 10) / 10f;
								c = palette.lerp(c, palette.white, scale);
							}
						}
						else
						{
							c = palette.clear;
							if (lc != palette.clear && lc != palette.black)
								c = palette.black;
						}
						pix[y * SCANbigMap.eq_an_map.Length + x] = c;
						lc = c;
					}
				}
				SCANbigMap.eq_map.SetPixels(0, 0, SCANbigMap.eq_an_map.Length, eqh, pix);
				SCANbigMap.eq_map.Apply();
				SCANbigMap.eq_frame = 4;
			}
			else
			{
				SCANbigMap.eq_frame -= 1;
			}

			if (SCANbigMap.eq_map != null)
			{
				r.x = maprect.x;
				r.y = maprect.y + maprect.height / 2 + -SCANbigMap.eq_map.height / 2;
				r.width = SCANbigMap.eq_map.width;
				r.height = SCANbigMap.eq_map.height;
				GUI.DrawTexture(r, SCANbigMap.eq_map);
			}
		}

		/* UI: This isn't really a UI function, so it doesn't belong here.
		 * 		This is more of a time-projection mathematical function. */
		/* FIXME: possible relocation */
		private static bool mapPosAtT(Rect maprect, SCANmap map, ref Rect r, Vessel vessel, Orbit o, double dT, double startUT)
		{
			double UT = startUT + dT;
			if (double.IsNaN(UT))
				return false;
			try
			{
				if (double.IsNaN(o.getObtAtUT(UT)))
					return false;
				Vector3d pos = o.getPositionAtUT(UT);
				double rotation = 0;
				if (vessel.mainBody.rotates)
				{
					rotation = (360 * (dT / vessel.mainBody.rotationPeriod)) % 360;
				}
				double lo = (vessel.mainBody.GetLongitude(pos) - rotation);
				double la = (vessel.mainBody.GetLatitude(pos));
				double lon = (map.projectLongitude(lo, la) + 180) % 360;
				double lat = (map.projectLatitude(lo, la) + 90) % 180;
				lat = map.scaleLatitude(lat);
				lon = map.scaleLongitude(lon);
				if (lat < 0 || lon < 0 || lat > 180 || lon > 360)
					return false;
				lon = lon * maprect.width / 360f;
				lat = maprect.height - lat * maprect.height / 180f;
				r.x = maprect.x + (float)lon;
				r.y = maprect.y + (float)lat;
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		internal static double meanForTrue(double TA, double e)
		{
			TA = TA * Mathf.Deg2Rad;
			double EA = Math.Acos((e + Math.Cos(TA)) / (1 + e * Math.Cos(TA)));
			if (TA > Math.PI)
				EA = 2 * Math.PI - EA;
			double MA = EA - e * Math.Sin(EA);
			// the mean anomaly isn't really an angle, but I'm a simple person
			return MA * Mathf.Rad2Deg;
		}

		#endregion

	}
}
