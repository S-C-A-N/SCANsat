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
using System.Collections.Generic;
using System.Linq;
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
		private float lastUpdate = 0f;
		private float updateInterval = 0.2f;
		private double slopeAVG;
		internal static Rect defaultRect = new Rect(30, 600, 260, 60);

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Instruments";
			WindowRect = defaultRect;
			WindowStyle = SCANskins.SCAN_window;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(260), GUILayout.Height(60) };
			Visible = false;
			DragEnabled = true;
			ClampToScreenOffset = new RectOffset(-200, -200, -40, -40);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		internal override void Start()
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

		internal override void OnDestroy()
		{
			GameEvents.onVesselSOIChanged.Remove(soiChange);
		}

		protected override void DrawWindowPre(int id)
		{
			v = FlightGlobals.ActiveVessel;

			//Grab the active scanners on this vessel
			sensors = SCANcontroller.controller.activeSensorsOnVessel(v.id);

			//if (maptraq_frame >= Time.frameCount - 5) //Still not sure what this actually does
			if (true)
			{
				//Check if region below the vessel is scanned
				if (SCANUtil.isCovered(v.longitude, v.latitude, data, SCANtype.AltimetryLoRes))
				{
					sensors |= SCANtype.Altimetry;
				}
				else if (SCANUtil.isCovered(v.longitude, v.latitude, data, SCANtype.AltimetryHiRes))
				{
					sensors |= SCANtype.Altimetry;
				}
				if (SCANUtil.isCovered(v.longitude, v.latitude, data, SCANtype.Biome))
				{
					sensors |= SCANtype.Biome;
				}
			}
		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);				/* Standard version label and close button */
			closeBox(id);

			int parts = 0;

			growS();
			if (notMappingToday) noData(id);		/* to be shown when power is out *FixMe - non-functional */
			else
			{
				if (biomeInfo(id)) ++parts;			/* show current biome info */
				if (altInfo(id)) ++parts;			/* show current altitude and slope */
				if (anomalyInfo(id)) ++parts;		/* show nearest anomaly detail - including BTDT view */
				if (parts <= 0) noData(id);			/* nothing to show */
			}
			stopS();
		}

		//Draw the version label in the upper left corner
		private void versionLabel(int id)
		{
			Rect r = new Rect(4, 0, 50, 18);
			GUI.Label(r, SCANversions.SCANsatVersion, SCANskins.SCAN_whiteReadoutLabel);
		}

		//Draw the close button in the upper right corner
		private void closeBox(int id)
		{
			Rect r = new Rect(WindowRect.width - 20, 0, 18, 18);
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				Visible = false;
			}
		}

		private void noData(int id)
		{
			GUILayout.Label("NO DATA", SCANskins.SCAN_insWhiteLabel);
		}

		//Display current biome info
		private bool biomeInfo(int id)
		{
			if ((sensors & SCANtype.Biome) != SCANtype.Nothing && v.mainBody.BiomeMap != null)
			{
				GUILayout.Label(string.Format("Biome:  {0}", SCANUtil.getBiomeName(v.mainBody, v.longitude, v.latitude)), SCANskins.SCAN_insColorLabel);
				fillS(-10);
				return true;
			}
			return false;
		}

		//Display the current vessel altitude
		private bool altInfo(int id)
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

				GUILayout.Label(string.Format("Altitude:  {0}", SCANuiUtil.distanceString(h, 100000)), SCANskins.SCAN_insColorLabel);
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

						double lon = v.longitude;
						double lat = v.latitude;
						double latOffset = degreeOffset * Math.Cos(Mathf.Deg2Rad * lat);
						double[] e = new double[9];
						double[] s = new double[8];
						e[0] = pqs;
						e[1] = SCANUtil.getElevation(v.mainBody, lon + latOffset, lat);
						e[2] = SCANUtil.getElevation(v.mainBody, lon - latOffset, lat);
						e[3] = SCANUtil.getElevation(v.mainBody, lon, lat + latOffset);
						e[4] = SCANUtil.getElevation(v.mainBody, lon, lat - latOffset);
						e[5] = SCANUtil.getElevation(v.mainBody, lon + latOffset, lat + latOffset);
						e[6] = SCANUtil.getElevation(v.mainBody, lon + latOffset, lat - latOffset);
						e[7] = SCANUtil.getElevation(v.mainBody, lon - latOffset, lat + latOffset);
						e[8] = SCANUtil.getElevation(v.mainBody, lon - latOffset, lat - latOffset);

						for (int i = 1; i <= 4; i++)
						{
							s[i - 1] = Math.Atan((Math.Abs(e[i] - e[0])) / 5) * Mathf.Rad2Deg;
						}
						for (int i = 5; i <= 8; i++)
						{
							s[i - 1] = Math.Atan((Math.Abs(e[i] - e[0])) / 7.071) * Mathf.Rad2Deg;
						}

							//SCANUtil.SCANdebugLog("Elevations: {0:F3};{1:F3};{2:F3};{3:F3};{4:F3} ; Long: {5:F6};{6:F6};{7:F6};{8:F6};{9:F6} ; Lat: {10:F6};{11:F6};{12:F6};{13:F6};{14:F6} ; Slope: {15:F1};{16:F1};{17:F1};{18:F1}", e[0], e[1], e[2], e[3], e[4], lon, lon + latOffset, lon - latOffset, lon, lon, lat, lat, lat, lat + latOffset, lat - latOffset, s[0], s[1], s[2], s[3]);

						slopeAVG = s.Sum() / 8;

					}

					GUILayout.Label(string.Format("Slope: {0:F2}°", slopeAVG), SCANskins.SCAN_insColorLabel);
					fillS(-10);
				}

				return true;
			}
			return false;
		}

		//Display info on the nearest anomaly *Need to separate the BTDT display*
		private bool anomalyInfo(int id)
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
					return true;
				}
				return false;
			}
			return false;
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
