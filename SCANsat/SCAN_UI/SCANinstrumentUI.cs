#region license
/* 
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - Instruments window object
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 *
 */
#endregion

using System;
using System.Linq;
using FinePrint;
using FinePrint.Utilities;
using SCANsat.SCAN_Platform;
using SCANsat;
using SCANsat.SCAN_UI.UI_Framework;
using SCANsat.SCAN_Data;
using UnityEngine;

using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat.SCAN_UI
{
	class SCANinstrumentUI: SCAN_MBW
	{
		private bool notMappingToday; //Unused out-of-power bool
		private SCANremoteView anomalyView;
		private SCANtype sensors;
		private SCANdata data;
		private Vessel v;
		private double degreeOffset;
		private double vlat, vlon;
		private float lastUpdate = 0f;
		private float updateInterval = 0.2f;
		private double slopeAVG;
		internal readonly static Rect defaultRect = new Rect(30, 600, 260, 60);
		private static Rect sessionRect = defaultRect;

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Instruments";
			WindowRect = sessionRect;
			WindowStyle = SCANskins.SCAN_window;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(260), GUILayout.Height(60) };
			Visible = false;
			DragEnabled = true;
			ClampToScreenOffset = new RectOffset(-200, -200, -40, -40);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		protected override void Start()
		{
			GameEvents.onVesselSOIChanged.Add(soiChange);
			data = SCANUtil.getData(FlightGlobals.currentMainBody);
			if (data == null)
			{
				data = new SCANdata(FlightGlobals.currentMainBody);
				SCANcontroller.controller.addToBodyData(FlightGlobals.currentMainBody, data);
			}
			planetConstants(FlightGlobals.currentMainBody);
		}

		protected override void OnDestroy()
		{
			GameEvents.onVesselSOIChanged.Remove(soiChange);
		}

		protected override void DrawWindowPre(int id)
		{
			v = FlightGlobals.ActiveVessel;

			vlat = SCANUtil.fixLatShift(v.latitude);
			vlon = SCANUtil.fixLonShift(v.longitude);

			//Grab the active scanners on this vessel
			sensors = SCANcontroller.controller.activeSensorsOnVessel(v.id);

			//if (maptraq_frame >= Time.frameCount - 5) //Still not sure what this actually does
			if (true)
			{
				//Check if region below the vessel is scanned
				if (SCANUtil.isCovered(vlon, vlat, data, SCANtype.AltimetryLoRes))
				{
					sensors |= SCANtype.Altimetry;
				}
				else if (SCANUtil.isCovered(vlon, vlat, data, SCANtype.AltimetryHiRes))
				{
					sensors |= SCANtype.Altimetry;
				}
				if (SCANUtil.isCovered(vlon, vlat, data, SCANtype.Biome))
				{
					sensors |= SCANtype.Biome;
				}
			}
		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);				/* Standard version label and close button */
			closeBox(id);

			growS();
			if (notMappingToday) noData(id);		/* to be shown when power is out *FixMe - non-functional */
			else
			{
				locationInfo(id);					/* always-on indicator for current lat/long */
				altInfo(id);						/* show current altitude and slope */
				biomeInfo(id);						/* show current biome info */
				anomalyInfo(id);					/* show nearest anomaly detail - including BTDT view */
				//if (parts <= 0) noData(id);		/* nothing to show */
			}
			stopS();
		}

		protected override void DrawWindowPost(int id)
		{
			sessionRect = WindowRect;
		}

		//Draw the version label in the upper left corner
		private void versionLabel(int id)
		{
			Rect r = new Rect(4, 0, 50, 18);
			GUI.Label(r, SCANmainMenuLoader.SCANsatVersion, SCANskins.SCAN_whiteReadoutLabel);
		}

		//Draw the close button in the upper right corner
		private void closeBox(int id)
		{
			Rect r = new Rect(WindowRect.width - 20, 1, 18, 18);
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				Visible = false;
			}
		}

		private void noData(int id)
		{
			GUILayout.Label("NO DATA", SCANskins.SCAN_insWhiteLabel);
		}

		//Displays current vessel location info
		private void locationInfo(int id)
		{
			GUILayout.Label(string.Format("Lat: {0:F2}°, Lon: {1:F2}°", vlat, vlon), SCANskins.SCAN_insColorLabel);
			fillS(-10);

			if (WaypointManager.Instance() != null)
			{
				foreach (SCANwaypoint p in data.Waypoints)
				{
					if (p.LandingTarget)
						continue;

					if (p.Band == FlightBand.NONE)
						continue;

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

					double range = waypointRange(p);

					if (WaypointManager.Instance().Distance(vlat, vlon, v.altitude, p.Latitude, p.Longitude, v.altitude, data.Body) <= range)
					{
						GUILayout.Label(string.Format("Waypoint: {0}", p.Name), SCANskins.SCAN_insColorLabel);
						fillS(-10);
						break;
					}
				}
			}
		}

		//Display current biome info
		private void biomeInfo(int id)
		{
			if ((sensors & SCANtype.Biome) != SCANtype.Nothing && v.mainBody.BiomeMap != null)
			{
				GUILayout.Label(string.Format("Biome: {0}", SCANUtil.getBiomeName(v.mainBody, vlon, vlat)), SCANskins.SCAN_insColorLabel);
				fillS(-10);
			}
		}

		//Display the current vessel altitude
		private void altInfo(int id)
		{
			if ((sensors & SCANtype.Altimetry) != SCANtype.Nothing)
			{
				double h = v.altitude;
				double pqs = 0;
				if (v.mainBody.pqsController != null)
				{
					pqs = v.PQSAltitude();
					if (pqs > 0 || !v.mainBody.ocean)
						h -= pqs;
				}
				if (h < 0)
					h = v.altitude;

				if (v.situation == Vessel.Situations.LANDED || v.situation == Vessel.Situations.SPLASHED || v.situation == Vessel.Situations.PRELAUNCH)
					GUILayout.Label(string.Format("Terrain: {0:N1}m", pqs), SCANskins.SCAN_insColorLabel);
				else
					GUILayout.Label(string.Format("Altitude: {0}", SCANuiUtil.distanceString(h, 100000)), SCANskins.SCAN_insColorLabel);
				fillS(-10);

				//Calculate slope less frequently; the rapidly changing value makes it difficult to read otherwise
				if (v.mainBody.pqsController != null)
				{
					float deltaTime = 1f;
					if (Time.deltaTime != 0)
						deltaTime = TimeWarp.deltaTime / Time.deltaTime;
					if (deltaTime > 5)
						deltaTime = 5;
					if (((Time.time * deltaTime) - lastUpdate) > updateInterval)
					{
						lastUpdate = Time.time;

						/* Slope is calculated using a nine point grid centered 5m around the vessel location
						 * The rise between the vessel location's elevation and each point on the grid is calculated, converted to slope in degrees, and averaged;
						 * Note: Averageing is not the most accurate method
						 */

						double latOffset = degreeOffset * Math.Cos(Mathf.Deg2Rad * vlat);
						double[] e = new double[9];
						double[] s = new double[8];
						e[0] = pqs;
						e[1] = SCANUtil.getElevation(v.mainBody, vlon + latOffset, vlat);
						e[2] = SCANUtil.getElevation(v.mainBody, vlon - latOffset, vlat);
						e[3] = SCANUtil.getElevation(v.mainBody, vlon, vlat + degreeOffset);
						e[4] = SCANUtil.getElevation(v.mainBody, vlon, vlat - degreeOffset);
						e[5] = SCANUtil.getElevation(v.mainBody, vlon + latOffset, vlat + degreeOffset);
						e[6] = SCANUtil.getElevation(v.mainBody, vlon + latOffset, vlat - degreeOffset);
						e[7] = SCANUtil.getElevation(v.mainBody, vlon - latOffset, vlat + degreeOffset);
						e[8] = SCANUtil.getElevation(v.mainBody, vlon - latOffset, vlat - degreeOffset);

						/* Calculate rise for each point on the grid
						 * The distance is 5m for adjacent points and 7.071m for the points on the corners
						 * Rise is converted to slope; i.e. a 5m elevation change over a 5m distance is a rise of 1
						 * Converted to slope using the inverse tangent this gives a slope of 45°
						 * */
						for (int i = 1; i <= 4; i++)
						{
							s[i - 1] = Math.Atan((Math.Abs(e[i] - e[0])) / 5) * Mathf.Rad2Deg;
						}
						for (int i = 5; i <= 8; i++)
						{
							s[i - 1] = Math.Atan((Math.Abs(e[i] - e[0])) / 7.071) * Mathf.Rad2Deg;
						}

						slopeAVG = s.Sum() / 8;

					}

					GUILayout.Label(string.Format("Slope: {0:F2}°", slopeAVG), SCANskins.SCAN_insColorLabel);
					fillS(-10);
				}
			}
		}

		//Display info on the nearest anomaly *Need to separate the BTDT display*
		private void anomalyInfo(int id)
		{
			if ((sensors & SCANtype.AnomalyDetail) != SCANtype.Nothing)
			{
				SCANanomaly nearest = null;
				double nearest_dist = -1;
				foreach (SCANanomaly a in data.Anomalies)
				{
					if (!a.Known)
						continue;
					double d = (a.Mod.transform.position - v.transform.position).magnitude;
					if (d < nearest_dist || nearest_dist < 0)
					{
						if (d < 50000)
						{
							nearest = a;
							nearest_dist = d;
						}
					}
				}
				if (nearest != null)
				{
					string txt = "Anomaly";
					if (nearest.Detail)
						txt = nearest.Name;
					txt += ":  " + SCANuiUtil.distanceString(nearest_dist, 5000);
					GUILayout.Label(txt, SCANskins.SCAN_insColorLabel);

					if (anomalyView == null)
						anomalyView = new SCANremoteView();
					if (anomalyView != null)
					{
						if (nearest.Mod != null)
						{
							if (anomalyView.lookat != nearest.Mod.gameObject)
								anomalyView.setup(320, 240, nearest.Mod.gameObject);
							Texture t = anomalyView.getTexture();
							if (t != null)
							{
								GUILayout.Label(anomalyView.getTexture());
								anomalyView.drawOverlay(GUILayoutUtility.GetLastRect(), SCANskins.SCAN_anomalyOverlay, nearest.Detail);
							}
						}
					}
				}
			}
		}

		private double waypointRange(SCANwaypoint p)
		{
			double min = ContractDefs.Survey.MinimumTriggerRange;
			double max = ContractDefs.Survey.MaximumTriggerRange;

			switch (p.Band)
			{
				case FlightBand.GROUND:
					return min;
				case FlightBand.LOW:
					return (min + max) / 2;
				case FlightBand.HIGH:
					return max;
				default:
					return max;
			}
		}

		private void planetConstants (CelestialBody b)
		{
			double circum = b.Radius * 2 * Math.PI;
			double eqDistancePerDegree = circum / 360;
			degreeOffset = 5 / eqDistancePerDegree;
		}

		private void soiChange (GameEvents.HostedFromToAction<Vessel, CelestialBody> VC)
		{
			data = SCANUtil.getData(VC.to);
			if (data == null)
			{
				data = new SCANdata(VC.to);
				SCANcontroller.controller.addToBodyData(VC.to, data);
			}
			planetConstants(VC.to);
		}

	}
}
