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
using System.Text.RegularExpressions;
using FinePrint;
using SCANsat.SCAN_Platform;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Map;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;
using UnityEngine;

namespace SCANsat.SCAN_UI.UI_Framework
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

		//A smaller font-size, simpler label method
		internal static void drawLabel(Rect r, string txt, bool aligned, bool left)
		{
			if (txt.Length < 1)
				return;
			if (aligned)
			{
				Vector2 sz = SCANskins.SCAN_labelSmallLeft.CalcSize(new GUIContent(txt.Substring(0, 1)));
				r.x -= sz.x / 2;
				r.y -= sz.y / 2;
			}
			if (left)
				GUI.Label(r, txt, SCANskins.SCAN_labelSmallLeft);
			else
				GUI.Label(r, txt, SCANskins.SCAN_labelSmallRight);
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
				if (SCANUtil.isCovered(lon, lat, data, SCANtype.AltimetryLoRes))
				{
					if (body.pqsController == null)
						info += palette.colored(palette.c_ugly, "LO ");
					else
						info += palette.colored(palette.c_good, "LO ");
				}
				else
					info += palette.colored(palette.grey, "LO ");
				if (SCANUtil.isCovered(lon, lat, data, SCANtype.AltimetryHiRes))
				{
					if (body.pqsController == null)
						info += palette.colored(palette.c_ugly, "HI ");
					else
						info += palette.colored(palette.c_good, "HI ");
				}
				else
					info += palette.colored(palette.grey, "HI ");
				if (SCANUtil.isCovered(lon, lat, data, SCANtype.Biome))
				{
					if (body.BiomeMap == null)
						info += palette.colored(palette.c_ugly, "MULTI ");
					else
						info += palette.colored(palette.c_good, "MULTI ");
				}
				else
					info += palette.colored(palette.grey, "MULTI ");
				if (SCANUtil.isCovered(lon, lat, data, SCANtype.AltimetryHiRes))
				{
					info += SCANUtil.getElevation(body, lon, lat).ToString("N2") + "m ";
				}
				else if (SCANUtil.isCovered(lon, lat, data, SCANtype.AltimetryLoRes))
				{
					info += (((int)SCANUtil.getElevation(body, lon, lat) / 500) * 500).ToString() + "m ";
				}
				if (SCANUtil.isCovered(lon, lat, data, SCANtype.Biome))
				{
					info += SCANUtil.getBiomeName(body, lon, lat) + " ";
				}

				if (SCANcontroller.controller.map_ResourceOverlay && SCANconfigLoader.GlobalResource && mapObj.Resource != null) //Adds selected resource amount to big map legend
				{
					if (SCANUtil.isCovered(lon, lat, data, mapObj.Resource.SType))
					{
						double amount = SCANUtil.ResourceOverlay(lat, lon, mapObj.Resource.Name, mapObj.Body);
						string label;
						if (amount < 0)
							label = "Unknown";
						else
						{
							if (amount > 1)
								amount = 1;
							label = amount.ToString("P2");
						}
						info += palette.colored(mapObj.Resource.MaxColor, mapObj.Resource.Name + ": " + label + " ");
					}
				}

				if (SCANcontroller.controller.map_waypoints && WaypointManager.Instance() != null)
				{
					double range = ContractDefs.Survey.MaximumTriggerRange;
					foreach (SCANwaypoint p in data.Waypoints)
					{
						if (!p.LandingTarget)
						{
							if (p.Root != null)
							{
								if (p.Root.ContractState != Contracts.Contract.State.Active)
									continue;
							}
							if (p.Param != null)
							{
								if (p.Param.State != Contracts.ParameterState.Incomplete)
									continue;
							}

							if (WaypointManager.Instance().Distance(lat, lon, 1000, p.Latitude, p.Longitude, 1000, body) <= range)
							{
								info += p.Name + " ";
								break;
							}
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
			SCAN_MBW.fillS(-10);
			readableLabel(posInfo, false);
		}

		internal static void mouseOverInfoSimple(double lon, double lat, SCANmap mapObj, SCANdata data, CelestialBody body, bool b)
		{
			string info = "";
			string posInfo = "";

			if (b)
			{
				if (SCANUtil.isCovered(lon, lat, data, SCANtype.AltimetryHiRes))
				{
					info += SCANUtil.getElevation(body, lon, lat).ToString("N0") + "m ";
				}
				else if (SCANUtil.isCovered(lon, lat, data, SCANtype.AltimetryLoRes))
				{
					info += (((int)SCANUtil.getElevation(body, lon, lat) / 500) * 500).ToString() + "m ";
				}
				if (SCANUtil.isCovered(lon, lat, data, SCANtype.Biome))
				{
					info += SCANUtil.getBiomeName(body, lon, lat) + " ";
				}

				if (SCANcontroller.controller.map_ResourceOverlay && SCANconfigLoader.GlobalResource && mapObj.Resource != null) //Adds selected resource amount to big map legend
				{
					if (SCANUtil.isCovered(lon, lat, data, mapObj.Resource.SType))
					{
						double amount = SCANUtil.ResourceOverlay(lat, lon, mapObj.Resource.Name, mapObj.Body);
						string label;
						if (amount < 0)
							label = "Unknown";
						else
						{
							if (amount > 1)
								amount = 1;
							label = amount.ToString("P2");
						}
						info += palette.colored(mapObj.Resource.MaxColor, mapObj.Resource.Name + ": " + label + " ");
					}
				}

				if (SCANcontroller.controller.map_waypoints && WaypointManager.Instance() != null)
				{
					double range = ContractDefs.Survey.MaximumTriggerRange;
					foreach (SCANwaypoint p in data.Waypoints)
					{
						if (!p.LandingTarget)
						{
							if (p.Root != null)
							{
								if (p.Root.ContractState != Contracts.Contract.State.Active)
									continue;
							}
							if (p.Param != null)
							{
								if (p.Param.State != Contracts.ParameterState.Incomplete)
									continue;
							}

							if (WaypointManager.Instance().Distance(lat, lon, 1000, p.Latitude, p.Longitude, 1000, body) <= range)
							{
								info += p.Name + " ";
								break;
							}
						}
					}
				}

				posInfo += string.Format("{0} ({1:F2}°,{2:F2}°)", toDMS(lat, lon), lat, lon);
			}

			//Draw the readout info labels
			readableLabel(info, false);
			SCAN_MBW.fillS(-10);
			readableLabel(posInfo, false);
		}

		//Method to handle active scanner display
		internal static string InfoText(Vessel v, SCANdata data, bool b)
		{
			string infotext = "";

			SCANcontroller.SCANsensor s;

			//Check here for each sensor; if active, in range, and at the ideal altitude
			s = SCANcontroller.controller.getSensorStatus(v, SCANtype.AltimetryLoRes);
			if (s == null)
				infotext += palette.colored(palette.grey, "LO");
			else if (!s.inRange)
				infotext += palette.colored(palette.c_bad, "LO");
			else if (!s.bestRange && (Time.realtimeSinceStartup % 2 < 1))
				infotext += palette.colored(palette.c_bad, "LO");
			else
				infotext += palette.colored(palette.c_good, "LO");

			s = SCANcontroller.controller.getSensorStatus(v, SCANtype.AltimetryHiRes);
			if (s == null)
				infotext += palette.colored(palette.grey, " HI");
			else if (!s.inRange)
				infotext += palette.colored(palette.c_bad, " HI");
			else if (!s.bestRange && (Time.realtimeSinceStartup % 2 < 1))
				infotext += palette.colored(palette.c_bad, " HI");
			else
				infotext += palette.colored(palette.c_good, " HI");

			s = SCANcontroller.controller.getSensorStatus(v, SCANtype.Biome);
			if (s == null)
				infotext += palette.colored(palette.grey, " MULTI");
			else if (!s.inRange)
				infotext += palette.colored(palette.c_bad, " MULTI");
			else if (!s.bestRange && (Time.realtimeSinceStartup % 2 < 1))
				infotext += palette.colored(palette.c_bad, " MULTI");
			else
				infotext += palette.colored(palette.c_good, " MULTI");

			s = SCANcontroller.controller.getSensorStatus(v, SCANtype.AnomalyDetail);
			if (s == null)
				infotext += palette.colored(palette.grey, " BTDT");
			else if (!s.inRange)
				infotext += palette.colored(palette.c_bad, " BTDT");
			else if (!s.bestRange && (Time.realtimeSinceStartup % 2 < 1))
				infotext += palette.colored(palette.c_bad, " BTDT");
			else
				infotext += palette.colored(palette.c_good, " BTDT");

			//Get coverage percentage for all active scanners on the vessel
			SCANtype active = SCANcontroller.controller.activeSensorsOnVessel(v.id);
			if (active != SCANtype.Nothing)
			{
				double cov = SCANUtil.getCoveragePercentage(data, active);
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
		private static string toDMS(double thing, string neg, string pos)
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
			SCANcontroller.controller.BigMap.resetWindowPos(SCANBigMap.defaultRect);
		}

		internal static void resetKSCMapPos()
		{
			SCANcontroller.controller.kscMap.resetWindowPos(SCANkscMap.defaultRect);
		}

		internal static void resetColorMapPos()
		{
			SCANcontroller.controller.colorManager.resetWindowPos(SCANcolorSelection.defaultRect);
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
				lat = SCANUtil.fixLat(map.projectLatitude(vessel.longitude, vessel.latitude));
				lon = SCANUtil.fixLon(map.projectLongitude(vessel.longitude, vessel.latitude));
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
			SCANicon.drawOrbitIcon((int)r.x, (int)r.y, SCANicon.orbitIconForVesselType(vessel.vesselType), col, sz, true);
			if (maprect.width < 360)
				return;
			r.x += 12;
			drawLabel(r, txt, flash, true, true);
		}

		//Handles various map labels; probably should be split up into multiple methods
		internal static void drawMapLabels(Rect maprect, Vessel vessel, SCANmap map, SCANdata data, CelestialBody body, bool showAnom, bool showWaypoints)
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
			if (showAnom)
			{
				foreach (SCANanomaly anomaly in data.Anomalies)
				{
					drawAnomalyLabel(maprect, map, anomaly);
				}
			}
			if (showWaypoints)
			{
				foreach (SCANwaypoint p in data.Waypoints)
				{
					if (!p.LandingTarget)
					{
						if (p.Root != null)
						{
							if (p.Root.ContractState != Contracts.Contract.State.Active)
								continue;
						}
						if (p.Param != null)
						{
							if (p.Param.State != Contracts.ParameterState.Incomplete)
								continue;
						}
					}

					drawWaypointLabel(maprect, map, p, data);
				}
			}
			if (vessel != null)
			{
				if (vessel.mainBody == body)
					drawVesselLabel(maprect, map, 0, vessel);
			}
		}

		//Method to draw anomaly labels on the map
		private static void drawAnomalyLabel(Rect maprect, SCANmap map, SCANanomaly anomaly)
		{
			if (!anomaly.Known)
				return;
			double lon = SCANUtil.fixLon(anomaly.Longitude);
			double lat = SCANUtil.fixLat(anomaly.Latitude);
			if (map != null)
			{
				lat = SCANUtil.fixLat(map.projectLatitude(anomaly.Longitude, anomaly.Latitude));
				lon = SCANUtil.fixLon(map.projectLongitude(anomaly.Longitude, anomaly.Latitude));
				lat = map.scaleLatitude(lat);
				lon = map.scaleLongitude(lon);
				if (lat < 0 || lon < 0 || lat > 180 || lon > 360)
					return;
			}
			lon = lon * maprect.width / 360f;
			lat = maprect.height - lat * maprect.height / 180f;
			string txt = SCANcontroller.controller.anomalyMarker + " " + anomaly.Name;
			if (!anomaly.Detail)
				txt = SCANcontroller.controller.anomalyMarker + " Anomaly";
			Rect r = new Rect(maprect.x + (float)lon, maprect.y + (float)lat, 250f, 25f);
			drawLabel(r, txt, true, true, true);
		}

		private static void drawWaypointLabel(Rect maprect, SCANmap map, SCANwaypoint p, SCANdata data)
		{
			double lon = SCANUtil.fixLon(p.Longitude);
			double lat = SCANUtil.fixLat(p.Latitude);

			if (map != null)
			{
				lat = SCANUtil.fixLat(map.projectLatitude(p.Longitude, p.Latitude));
				lon = SCANUtil.fixLon(map.projectLongitude(p.Longitude, p.Latitude));
				lat = map.scaleLatitude(lat);
				lon = map.scaleLongitude(lon);
				if (lat < 0 || lon < 0 || lat > 180 || lon > 360)
					return;
			}
			lon = lon * maprect.width / 360f;
			lat = maprect.height - lat * maprect.height / 180f;

			Rect r = new Rect(maprect.x + (float)lon, maprect.y + (float)lat, 24, 24);

			r.x -= 12;

			if (!p.LandingTarget)
			{
				r.y -= 24;
				drawMapIcon(r, SCANskins.SCAN_WaypointIcon, true);
			}
			else
			{
				r.x += 1;
				r.y -= 13;
				drawMapIcon(r, SCANcontroller.controller.mechJebTargetSelection ? SCANskins.SCAN_MechJebIcon : SCANskins.SCAN_TargetIcon, true, SCANcontroller.controller.mechJebTargetSelection ? palette.red : palette.xkcd_PukeGreen, true);
			}
		}

		internal static void drawMapIcon(Rect pos, Texture2D tex, bool outline = false, Color c = new Color(), bool flash = false, Rect texPos = new Rect(), bool texCoords = false)
		{
			if (texCoords)
			{
				Color old = GUI.color;
				if (outline)
				{
					GUI.color = palette.black;
					pos.x -= 1;
					GUI.DrawTextureWithTexCoords(pos, tex, texPos);
					pos.x += 2;
					GUI.DrawTextureWithTexCoords(pos, tex, texPos);
					pos.x -= 1;
					pos.y -= 1;
					GUI.DrawTextureWithTexCoords(pos, tex, texPos);
					pos.y += 2;
					GUI.DrawTextureWithTexCoords(pos, tex, texPos);
					pos.y -= 1;
				}
				if (flash)
					GUI.color = c;
				else
					GUI.color = old;

				GUI.DrawTextureWithTexCoords(pos, tex, texPos);
				GUI.color = old;
			}
			else
			{
				Color old = GUI.color;
				if (outline)
				{
					GUI.color = palette.black;
					pos.x -= 1;
					GUI.DrawTexture(pos, tex);
					pos.x += 2;
					GUI.DrawTexture(pos, tex);
					pos.x -= 1;
					pos.y -= 1;
					GUI.DrawTexture(pos, tex);
					pos.y += 2;
					GUI.DrawTexture(pos, tex);
					pos.y -= 1;
				}
				if (flash)
					GUI.color = c;
				else
					GUI.color = old;

				GUI.DrawTexture(pos, tex);
				GUI.color = old;
			}
		}

		internal static void drawMapIconGL(Rect pos, Texture2D tex, Color c, Material iconMat, Color shadow = new Color(), bool outline = false, Rect texPos = new Rect(), bool texCoords = false)
		{
			if (texCoords)
			{
				if (outline)
				{
					iconMat.color = shadow;
					pos.x -= 1;
					Graphics.DrawTexture(pos, tex, texPos, 0, 0, 0, 0, iconMat);
					pos.x += 2;
					Graphics.DrawTexture(pos, tex, texPos, 0, 0, 0, 0, iconMat);
					pos.x -= 1;
					pos.y -= 1;
					Graphics.DrawTexture(pos, tex, texPos, 0, 0, 0, 0, iconMat);
					pos.y += 2;
					Graphics.DrawTexture(pos, tex, texPos, 0, 0, 0, 0, iconMat);
					pos.y -= 1;
				}
				iconMat.color = c;

				Graphics.DrawTexture(pos, tex, texPos, 0, 0, 0, 0, iconMat);
			}
			else
			{
				if (outline)
				{
					iconMat.color = shadow;
					pos.x -= 1;
					Graphics.DrawTexture(pos, tex, 0, 0, 0, 0, iconMat);
					pos.x += 2;
					Graphics.DrawTexture(pos, tex, 0, 0, 0, 0, iconMat);
					pos.x -= 1;
					pos.y -= 1;
					Graphics.DrawTexture(pos, tex, 0, 0, 0, 0, iconMat);
					pos.y += 2;
					Graphics.DrawTexture(pos, tex, 0, 0, 0, 0, iconMat);
					pos.y -= 1;
				}
				iconMat.color = c;

				Graphics.DrawTexture(pos, tex);
			}

		}

		/* FIXME: This may use assumed, shared, static constants with Legend stuff in other SCANsat files */
		private static void drawLegendLabel(Rect r, float val, float min, float max)
		{
			if (val < min || val > max)
				return;
			float scale = r.width * 1f / (max - min);
			float x = r.x + scale * (val - min);
			Rect lr = new Rect(x, r.y + r.height / 4, 15, r.height);
			drawLabel(lr, "|", false, true, true);
			string txt = val.ToString("N0");
			GUIContent c = new GUIContent(txt);
			Vector2 dim = SCANskins.SCAN_whiteReadoutLabel.CalcSize(c);
			lr.y += dim.y * 0.25f;
			lr.x -= dim.x / 2;
			if (lr.x < r.x)
				lr.x = r.x;
			if (lr.x + dim.x > r.x + r.width)
				lr.x = r.x + r.width - dim.x;
			lr.width = dim.x;
			drawLabel(lr, txt, false, true, true);
		}
		
		internal static void drawSliderLabel(Rect r, string min, string max)
		{
			Rect sr = new Rect(r.x, r.y + 7, 10, 20);
			drawLabel(sr, "|", true, true);
			sr.x += (r.width - 8);
			drawLabel(sr, "|", true, false);
			sr.width = 80;
			sr.x -= (r.width + 60);
			sr.y += 12;
			drawLabel(sr, min, true, false);
			sr.x += (r.width + 62);
			drawLabel(sr, max, true, true);
		}

		internal static void drawVerticalSliderLabel(Rect r, string min, string max)
		{
			Rect sr = new Rect(r.x - 15, r.y - 4, 20, 20);
			drawLabel(sr, "_", true, false);
			sr.y += (r.height - 8);
			drawLabel(sr, "_", true, false);
			sr.width = 50;
			sr.x -= 40;
			sr.y = r.y + 2;
			drawLabel(sr, max, true, false);
			sr.y += (r.height - 8);
			drawLabel(sr, min, true, false);
		}

		/* FIXME: This uses assumed, shared, static constants with Legend stuff in other SCANsat files */
		internal static void drawLegend(SCANdata data, SCANmapLegend legend)
		{
			GUILayout.Label("", GUILayout.ExpandWidth(true));
			Rect r = GUILayoutUtility.GetLastRect();
			r.width -= 64;
			GUI.DrawTexture(r, legend.Legend); //SCANmapLegend.getLegend(data.MinHeight, data.MaxHeight, SCANcontroller.controller.colours, data));
			float minLabel = data.TerrainConfig.MinTerrain;
			float maxLabel = data.TerrainConfig.MaxTerrain;
			if (data.TerrainConfig.MinTerrain % 1000 != 0)
				minLabel += 500;
			if (data.TerrainConfig.MaxTerrain % 1000 != 0)
				maxLabel -= 500;
			float range = data.TerrainConfig.MaxTerrain - data.TerrainConfig.MinTerrain;
			float step = 1000f;
			if (range > 10000)
				step = 2000;
			else if (range < 4000)
				step = 500;
			for (float val = minLabel; val < maxLabel; val += step)
			{
				drawLegendLabel(r, val, data.TerrainConfig.MinTerrain, data.TerrainConfig.MaxTerrain);
			}
		}

		#endregion

		#region Overlays

		internal static Color32 lineColor = new Color(1f, 1f, 1f, 1f);
		internal static Color32 blackLineColor = new Color(0f, 0f, 0f, 0.9f);
		private static Material lineMat = JUtil.DrawLineMaterial();

		internal static Dictionary<int, List<List<Vector2d>>> drawGridLine(Rect maprect, SCANmap map)
		{
			var lineDict = new Dictionary<int, List<List<Vector2d>>>();
			var whiteLineList = new List<List<Vector2d>>();
			var blackLineList = new List<List<Vector2d>>();

			switch (map.Projection)
			{
				case MapProjection.Rectangular:
					{
						for (double lon = -150; lon <= 150; lon += 30)
						{
							List<Vector2d> points = new List<Vector2d>();
							List<Vector2d> pointsBlack = new List<Vector2d>();
							points.Add(new Vector2d((int)(map.MapScale * (lon + 180)), 0));
							points.Add(new Vector2d((int)(map.MapScale * (lon + 180)), (int)(map.MapScale * 180)));
							pointsBlack.Add(new Vector2d(points[0].x + 1, points[0].y));
							pointsBlack.Add(new Vector2d(points[1].x + 1, points[1].y));

							whiteLineList.Add(points);
							blackLineList.Add(pointsBlack);
						}
						for (double lat = -60; lat <= 60; lat += 30)
						{
							List<Vector2d> points = new List<Vector2d>();
							List<Vector2d> pointsBlack = new List<Vector2d>();
							points.Add(new Vector2d(0, (int)(map.MapScale * (lat + 90))));
							points.Add(new Vector2d((int)(map.MapScale * 360), (int)(map.MapScale * (lat + 90))));
							pointsBlack.Add(new Vector2d(points[0].x, points[0].y - 1));
							pointsBlack.Add(new Vector2d(points[1].x, points[1].y - 1));

							whiteLineList.Add(points);
							blackLineList.Add(pointsBlack);
						}
							break;
					}
				case MapProjection.KavrayskiyVII:
					{
						for (double lon = -150; lon <= 150; lon += 30)
						{
							List<Vector2d> points = new List<Vector2d>();
							List<Vector2d> pointsBlack = new List<Vector2d>();
							int i = 0;
							for (double lat = -88; lat <= 88; lat += 4)
							{
								points.Add(new Vector2d((int)(map.MapScale * (map.projectLongitude(lon, lat) + 180)), (int)(map.MapScale * (map.projectLatitude(lon, lat) + 90))));
								pointsBlack.Add(new Vector2d(points[i].x + 1, points[i].y));
								i++;
							}

							whiteLineList.Add(points);
							blackLineList.Add(pointsBlack);
						}
						for (double lat = -60; lat <= 60; lat += 30)
						{
							List<Vector2d> points = new List<Vector2d>();
							List<Vector2d> pointsBlack = new List<Vector2d>();
							points.Add(new Vector2d((int)(map.MapScale * (map.projectLongitude(-179, lat) + 180)), (int)(map.MapScale * (lat + 90))));
							points.Add(new Vector2d((int)(map.MapScale * (map.projectLongitude(179, lat) + 180)), (int)(map.MapScale * (lat + 90))));
							pointsBlack.Add(new Vector2d(points[0].x, points[0].y - 1));
							pointsBlack.Add(new Vector2d(points[1].x, points[1].y - 1));

							whiteLineList.Add(points);
							blackLineList.Add(pointsBlack);
						}
						break;
					}
				case MapProjection.Polar:
					{
						for (double lon = -180; lon <= 150; lon += 30)
						{
							List<Vector2d> pointsS = new List<Vector2d>();
							List<Vector2d> pointsBlackS = new List<Vector2d>();
							pointsS.Add(new Vector2d((int)(map.MapScale * (map.projectLongitude(lon, -88) + 180)), (int)(map.MapScale * (map.projectLatitude(lon, -88) + 90))));
							pointsS.Add(new Vector2d((int)(map.MapScale * (map.projectLongitude(lon, -2) + 180)), (int)(map.MapScale * (map.projectLatitude(lon, -2) + 90))));

							whiteLineList.Add(pointsS);

							List<Vector2d> pointsN = new List<Vector2d>();
							List<Vector2d> pointsBlackN = new List<Vector2d>();
							pointsN.Add(new Vector2d((int)(map.MapScale * (map.projectLongitude(lon, 2) + 180)), (int)(map.MapScale * (map.projectLatitude (lon, 2) + 90))));
							pointsN.Add(new Vector2d((int)(map.MapScale * (map.projectLongitude(lon, 88) + 180)), (int)(map.MapScale * (map.projectLatitude(lon, 88) + 90))));

							whiteLineList.Add(pointsN);

							if (lon == -180 || lon == 0)
							{
								pointsBlackS.Add(new Vector2d(pointsS[0].x + 1, pointsS[0].y));
								pointsBlackS.Add(new Vector2d(pointsS[1].x + 1, pointsS[1].y));
								pointsBlackN.Add(new Vector2d(pointsN[0].x + 1, pointsN[0].y));
								pointsBlackN.Add(new Vector2d(pointsN[1].x + 1, pointsN[1].y));
							}
							else if (lon == -90 || lon == 90)
							{
								pointsBlackS.Add(new Vector2d(pointsS[0].x, pointsS[0].y - 1));
								pointsBlackS.Add(new Vector2d(pointsS[1].x, pointsS[1].y - 1));
								pointsBlackN.Add(new Vector2d(pointsN[0].x, pointsN[0].y - 1));
								pointsBlackN.Add(new Vector2d(pointsN[1].x, pointsN[1].y - 1));
							}
							else if (lon == -60 || lon == -30 || lon == 120 || lon == 150)
							{
								pointsBlackS.Add(new Vector2d(pointsS[0].x - 1, pointsS[0].y - 1));
								pointsBlackS.Add(new Vector2d(pointsS[1].x - 1, pointsS[1].y - 1));
								pointsBlackN.Add(new Vector2d(pointsN[0].x + 1, pointsN[0].y - 1));
								pointsBlackN.Add(new Vector2d(pointsN[1].x + 1, pointsN[1].y - 1));
							}
							else
							{
								pointsBlackS.Add(new Vector2d(pointsS[0].x + 1, pointsS[0].y - 1));
								pointsBlackS.Add(new Vector2d(pointsS[1].x + 1, pointsS[1].y - 1));
								pointsBlackN.Add(new Vector2d(pointsN[0].x - 1, pointsN[0].y - 1));
								pointsBlackN.Add(new Vector2d(pointsN[1].x - 1, pointsN[1].y - 1));
							}
							blackLineList.Add(pointsBlackS);
							blackLineList.Add(pointsBlackN);
						}
						for (double lat = -88; lat <= 88; lat += 2)
						{
							if (lat != 0)
							{
								if (lat % 30 == 0 || lat == -88 || lat == 88)
								{
									List<Vector2d> points = new List<Vector2d>();
									List<Vector2d> pointsBlack = new List<Vector2d>();
									for (double lon = -180; lon <= 180; lon += 4)
									{
										points.Add(new Vector2d((int)(map.MapScale * (map.projectLongitude(lon, lat) + 180)), (int)(map.MapScale * (map.projectLatitude(lon, lat) + 90))));
										float offset = 0;
										if (lat == 30) offset = -0.8f;
										else if (lat == -30) offset = 0.8f;
										else if (lat == 60) offset = -0.3f;
										else if (lat == -60) offset = 0.3f;
										if (lat != -88 && lat != 88)
											pointsBlack.Add(new Vector2d((int)(map.MapScale * (map.projectLongitude(lon, lat + offset) + 180)), (int)(map.MapScale * (map.projectLatitude(lon, lat + offset) + 90))));
									}
									whiteLineList.Add(points);
									blackLineList.Add(pointsBlack);
								}
							}
						}
						break;
					}
			}

			lineDict.Add(0, blackLineList);
			lineDict.Add(1, whiteLineList);

			return lineDict;
		}

		internal static void drawGridLines(IList<Vector2d> points, float mapWidth, float left, float top, Color c)
		{
			if (points.Count < 2)
				return;
			GL.Begin(GL.LINES);
			lineMat.SetPass(0);
			GL.Color(c);
			float xStart, yStart;
			xStart = (float)points[0].x;
			yStart = (mapWidth / 2) - (float)points[0].y;
			if (xStart < 0 || yStart < 0 || yStart > (mapWidth / 2) || xStart > mapWidth)
				return;
			xStart += left;
			yStart += top;
			for (int i = 1; i < points.Count; i++)
			{

				float xEnd = (float)points[i].x;
				float yEnd = (mapWidth / 2) - (float)points[i].y;
				if (xEnd < 0 || yEnd < 0 || yEnd > (mapWidth / 2) || xEnd > mapWidth)
					continue;
				xEnd += left;
				yEnd += top;

				drawLine(xStart, yStart, xEnd, yEnd);

				xStart = xEnd;
				yStart = yEnd;
			}
			GL.End();
		}

		private static void drawLine(float xStart, float yStart, float xEnd, float yEnd)
		{
			var start = new Vector2(xStart, yStart);
			var end = new Vector2(xEnd, yEnd);

			GL.Vertex(start);
			GL.Vertex(end);
		}

		//Draw the orbit overlay
		internal static void drawOrbit(Rect maprect, SCANmap map, Vessel vessel, CelestialBody body)
		{
			if (vessel == null) return;
			if (vessel.mainBody != body) return;
			int eqh = 16;

			if (vessel.LandedOrSplashed)
				return;
			bool lite = maprect.width < 400;
			Orbit o = vessel.orbit;
			double startUT = Planetarium.GetUniversalTime();
			double UT = startUT;
			int steps = 80; // increase for neater lines, decrease for better speed indication
			bool ath = false;
			if (vessel.mainBody.atmosphere)
			{
				if (vessel.mainBody.atmosphereDepth >= vessel.altitude)
				{
					ath = true;
				}
			}
			Rect r = new Rect(0, 0, 70f, 50f);
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
						if (vessel.mainBody.atmosphereDepth >= alt)
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
				SCANicon.drawOrbitIcon((int)r.x, (int)r.y, SCANicon.OrbitIcon.Planet, col, 8, false);
			}

			// show apoapsis and periapsis
			if (o.ApA > 0 && mapPosAtT(maprect, map, ref r, vessel, o, o.timeToAp, startUT))
			{
				SCANicon.drawOrbitIcon((int)r.x, (int)r.y, SCANicon.OrbitIcon.Ap, palette.cb_skyBlue, 32, true);
				r.x += 24;
				r.y -= 12;
				if (!lite)
					drawLabel(r, o.ApA.ToString("N0"), true, true, true);
			}
			if (o.PeA > 0 && mapPosAtT(maprect, map, ref r, vessel, o, o.timeToPe, startUT))
			{
				SCANicon.drawOrbitIcon((int)r.x, (int)r.y, SCANicon.OrbitIcon.Pe, palette.cb_skyBlue, 32, true);
				r.x += 24;
				r.y -= 12;
				if (!lite)
					drawLabel(r, o.PeA.ToString("N0"), true, true, true);
			}

			if (lite)
				return;

			// show first maneuver node
			if (vessel.patchedConicSolver != null)
			{
				if (vessel.patchedConicSolver.maneuverNodes.Count > 0)
				{
					ManeuverNode n = vessel.patchedConicSolver.maneuverNodes[0];
					if (n.patch == vessel.orbit && n.nextPatch != null && n.nextPatch.activePatch && n.UT > startUT - o.period && mapPosAtT(maprect, map, ref r, vessel, o, n.UT - startUT, startUT))
					{
						col = palette.cb_reddishPurple;
						if (SCANcontroller.controller.colours != 1)
							col = palette.xkcd_PurplyPink;
						SCANicon.drawOrbitIcon((int)r.x, (int)r.y, SCANicon.OrbitIcon.ManeuverNode, col, 32, true);
						Orbit nuo = n.nextPatch;
						for (int i = 0; i < steps; ++i)
						{
							double T = n.UT - startUT + i * nuo.period / steps;
							if (T + startUT > nuo.EndUT)
								break;
							if (mapPosAtT(maprect, map, ref r, vessel, nuo, T, startUT))
							{
								SCANicon.drawOrbitIcon((int)r.x, (int)r.y, SCANicon.OrbitIcon.Planet, col, 8, false);
							}
						}
						if (nuo.patchEndTransition == Orbit.PatchTransitionType.ESCAPE)
						{
							SCANicon.drawOrbitIcon((int)r.x, (int)r.y, SCANicon.OrbitIcon.Exit, col, 32, true);
						}
						else if (nuo.patchEndTransition == Orbit.PatchTransitionType.ENCOUNTER)
						{
							SCANicon.drawOrbitIcon((int)r.x, (int)r.y, SCANicon.OrbitIcon.Encounter, col, 32, true);
						}
						if (nuo.timeToAp > 0 && n.UT + nuo.timeToAp < nuo.EndUT && mapPosAtT(maprect, map, ref r, vessel, nuo, n.UT - startUT + nuo.timeToAp, startUT))
						{
							SCANicon.drawOrbitIcon((int)r.x, (int)r.y, SCANicon.OrbitIcon.Ap, col, 32, true);
						}
						if (nuo.timeToPe > 0 && n.UT + nuo.timeToPe < nuo.EndUT && mapPosAtT(maprect, map, ref r, vessel, nuo, n.UT - startUT + nuo.timeToPe, startUT))
						{
							SCANicon.drawOrbitIcon((int)r.x, (int)r.y, SCANicon.OrbitIcon.Pe, col, 32, true);
						}
					}
				}
			}

			if (o.PeA < 0)
				return;
			if (map.Projection == MapProjection.Polar)
				return;

			if (SCANBigMap.eq_frame <= 0)
			{
				// predict equatorial crossings for the next 100 loops
				double TAAN = 360f - o.argumentOfPeriapsis;	// true anomaly at ascending node
				double TADN = (TAAN + 180) % 360;			// true anomaly at descending node
				double MAAN = meanForTrue(TAAN, o.eccentricity);
				double MADN = meanForTrue(TADN, o.eccentricity);
				double tAN = (((MAAN - o.meanAnomaly * Mathf.Rad2Deg + 360) % 360) / 360f * o.period + startUT);
				double tDN = (((MADN - o.meanAnomaly * Mathf.Rad2Deg + 360) % 360) / 360f * o.period + startUT);

				if (SCANBigMap.eq_an_map == null || SCANBigMap.eq_dn_map == null || SCANBigMap.eq_an_map.Length != maprect.width)
				{
					SCANBigMap.eq_an_map = new int[(int)maprect.width];
					SCANBigMap.eq_dn_map = new int[(int)maprect.width];
				}
				if (SCANBigMap.eq_map == null || SCANBigMap.eq_map.width != SCANBigMap.eq_an_map.Length)
				{
					SCANBigMap.eq_map = new Texture2D(SCANBigMap.eq_an_map.Length, eqh, TextureFormat.ARGB32, false);
				}
				for (int i = 0; i < SCANBigMap.eq_an_map.Length; ++i)
				{
					SCANBigMap.eq_an_map[i] = 0;
					SCANBigMap.eq_dn_map[i] = 0;
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
					int lonAN = (int)(((map.projectLongitude(loAN, 0) + 180) % 360) * SCANBigMap.eq_an_map.Length / 360f);
					int lonDN = (int)(((map.projectLongitude(loDN, 0) + 180) % 360) * SCANBigMap.eq_dn_map.Length / 360f);
					if (lonAN >= 0 && lonAN < SCANBigMap.eq_an_map.Length)
						SCANBigMap.eq_an_map[lonAN] += 1;
					if (lonDN >= 0 && lonDN < SCANBigMap.eq_dn_map.Length)
						SCANBigMap.eq_dn_map[lonDN] += 1;
				}
				Color[] pix = SCANBigMap.eq_map.GetPixels(0, 0, SCANBigMap.eq_an_map.Length, eqh);
				Color cAN = palette.cb_skyBlue, cDN = palette.cb_orange;
				for (int y = 0; y < eqh; ++y)
				{
					Color lc = palette.clear;
					for (int x = 0; x < SCANBigMap.eq_an_map.Length; ++x)
					{
						Color c = palette.clear;
						float scale = 0;
						if (y < eqh / 2)
						{
							c = cDN;
							scale = SCANBigMap.eq_dn_map[x];
						}
						else
						{
							c = cAN;
							scale = SCANBigMap.eq_an_map[x];
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
									pix[y * SCANBigMap.eq_an_map.Length + x - 1] = palette.black;
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
						pix[y * SCANBigMap.eq_an_map.Length + x] = c;
						lc = c;
					}
				}
				SCANBigMap.eq_map.SetPixels(0, 0, SCANBigMap.eq_an_map.Length, eqh, pix);
				SCANBigMap.eq_map.Apply();
				SCANBigMap.eq_frame = 4;
			}
			else
			{
				SCANBigMap.eq_frame -= 1;
			}

			if (SCANBigMap.eq_map != null)
			{
				r.x = maprect.x;
				r.y = maprect.y + maprect.height / 2 + -SCANBigMap.eq_map.height / 2;
				r.width = SCANBigMap.eq_map.width;
				r.height = SCANBigMap.eq_map.height;
				GUI.DrawTexture(r, SCANBigMap.eq_map);
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

		#region MechJeb Target Overlay

		/*These methods borrowed from MechJeb GLUtils: 
		 * https://github.com/MuMech/MechJeb2/blob/master/MechJeb2/GLUtils.cs
		 * 
		*/
		internal static void drawTargetOverlay(CelestialBody body, double latitude, double longitude, Color c)
		{
			double rotation = 0;
			double radius = 0;
			Vector3d up = body.GetSurfaceNVector(latitude, longitude);
			var height = SCANUtil.getElevation(body, longitude, latitude);
			if (height < body.Radius)
				height = body.Radius;
			Vector3d center = body.position + height * up;

			if (occluded(center, body))
				return;

			Vector3d north = Vector3d.Exclude(up, body.transform.up).normalized;

			if (radius <= 0)
				radius = body.Radius / 15;

			GLTriangleMap(new Vector3d[] { center, center + radius * (QuaternionD.AngleAxis(rotation - 55, up) * north), center + radius * (QuaternionD.AngleAxis(rotation -35, up) * north) }, c);

			GLTriangleMap(new Vector3d[] { center, center + radius * (QuaternionD.AngleAxis(rotation + 55, up) * north), center + radius * (QuaternionD.AngleAxis(rotation + 35, up) * north) }, c);

			GLTriangleMap(new Vector3d[] { center, center + radius * (QuaternionD.AngleAxis(rotation - 145, up) * north), center + radius * (QuaternionD.AngleAxis(rotation - 125, up) * north) }, c);

			GLTriangleMap(new Vector3d[] { center, center + radius * (QuaternionD.AngleAxis(rotation + 145, up) * north), center + radius * (QuaternionD.AngleAxis(rotation + 125, up) * north) }, c);
		}

		private static bool occluded(Vector3d pos, CelestialBody body)
		{
			if (Vector3d.Distance(pos, body.position) < body.Radius - 100)
				return true;

			Vector3d camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);

			if (Vector3d.Angle(camPos - pos, body.position - pos) > 90)
				return false;

			double bodyDistance = Vector3d.Distance(camPos, body.position);
			double separationAngle = Vector3d.Angle(pos - camPos, body.position - camPos);
			double altitude = bodyDistance * Math.Sin(Math.PI / 180 * separationAngle);
			return (altitude < body.Radius);
		}

		private static Material mat;

		private static void GLTriangleMap(Vector3d[] vert, Color c)
		{
			GL.PushMatrix();
			if (mat == null)
				mat = new Material(Shader.Find("Particles/Additive"));
			mat.SetPass(0);
			GL.LoadOrtho();
			GL.Begin(GL.TRIANGLES);
			GL.Color(c);
			GLVertexMap(vert[0]);
			GLVertexMap(vert[1]);
			GLVertexMap(vert[2]);
			GL.End();
			GL.PopMatrix();
		}

		private static void GLVertexMap(Vector3d pos)
		{
			Vector3 screenPoint = PlanetariumCamera.Camera.WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(pos));
			GL.Vertex3(screenPoint.x / Camera.main.pixelWidth, screenPoint.y / Camera.main.pixelHeight, 0);
		}

		#endregion

		#region Planet Overlay Textures

		internal static void drawResourceTexture(int height, ref int step, SCANdata data, SCANresourceGlobal resource)
		{
			Color[] pix;
			float scale = height / 180f;

			if (resource.MapOverlay == null)
			{
				resource.MapOverlay = new Texture2D(height * 2, height, TextureFormat.ARGB32, true);
				pix = resource.MapOverlay.GetPixels();
				for (int i = 0; i < pix.Length; i++)
					pix[i] = palette.clear;
				resource.MapOverlay.SetPixels(pix);
			}
			else if (step >= resource.MapOverlay.height)
			{
				return;
			}

			pix = resource.MapOverlay.GetPixels(0, step, resource.MapOverlay.width, 1);

			for (int i = 0; i < pix.Length; i++)
			{
				double lon = (i / scale);
				double lat = (step / scale) - 90;

				if (lon <= 180)
					lon = 180 - lon;
				else
					lon = (lon - 180) * -1;
				lon -= 90;
				if (lon < -180)
					lon += 360;

				pix[i] = resourceToColor(lon, lat, data, palette.clear, resource, 0.05f);
			}

			resource.MapOverlay.SetPixels(0, step, resource.MapOverlay.width, 1, pix);
			step++;
			if (step % 10 == 0 || step >= height)
				resource.MapOverlay.Apply();
		}

		private static double resourceMapValue(double Lon, double Lat, SCANdata Data, SCANresourceGlobal resource)
		{
			double amount = 0;
			if (SCANUtil.isCovered(Lon, Lat, Data, resource.SType))
			{
				amount = SCANUtil.ResourceOverlay(Lat, Lon, resource.Name, Data.Body);
				amount *= 100;
				if (amount >= resource.CurrentBody.MinValue)
				{
					if (amount > resource.CurrentBody.MaxValue)
						amount = resource.CurrentBody.MaxValue;
				}
				else
					amount = 0;
			}
			else
				amount = -1;
			return amount;
		}

		/* Converts resource amount to pixel color */
		internal static Color resourceToColor(double Lon, double Lat, SCANdata Data, Color BaseColor, SCANresourceGlobal Resource, float Transparency = 0.4f)
		{
			double amount = resourceMapValue(Lon, Lat, Data, Resource);
			if (amount < 0)
				return BaseColor;
			else if (amount == 0)
				return palette.lerp(BaseColor, palette.grey, Transparency);
			else
				return palette.lerp(palette.lerp(Resource.MinColor, Resource.MaxColor, (float)amount / (Resource.CurrentBody.MaxValue - Resource.CurrentBody.MinValue)), BaseColor, Resource.Transparency / 100f);
		}

		#endregion

	}
}
