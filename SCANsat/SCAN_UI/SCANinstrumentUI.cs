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
using FinePrint;
using FinePrint.Utilities;
using SCANsat.SCAN_Platform;
using SCANsat;
using SCANsat.SCAN_PartModules;
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
		private List<SCANresourceDisplay> resourceScanners = new List<SCANresourceDisplay>();
		private List<SCANresourceGlobal> resources = new List<SCANresourceGlobal>();
		private int currentResource;
		private double degreeOffset;
		private double vlat, vlon;
		private float lastUpdate = 0f;
		private float updateInterval = 0.2f;
		private double slopeAVG;
		internal readonly static Rect defaultRect = new Rect(30, 600, 260, 60);
		private static Rect sessionRect = defaultRect;

		private string infoLabel = "";
		SCANanomaly nearest = null;
		private string slopeString = "";

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
			GameEvents.onVesselChange.Add(vesselChange);
			GameEvents.onVesselWasModified.Add(vesselChange);
			data = SCANUtil.getData(FlightGlobals.currentMainBody);
			if (data == null)
			{
				data = new SCANdata(FlightGlobals.currentMainBody);
				SCANcontroller.controller.addToBodyData(FlightGlobals.currentMainBody, data);
			}
			planetConstants(FlightGlobals.currentMainBody);

			v = FlightGlobals.ActiveVessel;
			resources = SCANcontroller.setLoadedResourceList();
			resetResourceList();
		}

		protected override void OnDestroy()
		{
			GameEvents.onVesselSOIChanged.Remove(soiChange);
			GameEvents.onVesselChange.Remove(vesselChange);
			GameEvents.onVesselWasModified.Remove(vesselChange);
		}

		protected override void DrawWindowPre(int id)
		{
			v = FlightGlobals.ActiveVessel;

			vlat = SCANUtil.fixLatShift(v.latitude);
			vlon = SCANUtil.fixLonShift(v.longitude);

			//Grab the active scanners on this vessel
			sensors = SCANcontroller.controller.activeSensorsOnVessel(v.id);

			if (true)
			{
				infoLabel = "";
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

				foreach (SCANresourceGlobal s in resources)
				{
					if (SCANUtil.isCovered(vlon, vlat, data, s.SType))
						sensors |= s.SType;
				}

				if (SCANUtil.isCovered(vlon, vlat, data, SCANtype.FuzzyResources))
					sensors |= SCANtype.FuzzyResources;
			}
		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);				/* Standard version label and close button */
			closeBox(id);

			growS();
				locationInfo(id);					/* always-on indicator for current lat/long */
				altInfo(id);						/* show current altitude and slope */
				biomeInfo(id);						/* show current biome info */
				resourceInfo(id);					/* show current resource abundance */
				drawInfoLabel(id);					/* method to actually draw the label */
				drawResourceButtons(id);			/* draw the toggle buttons to change resources */
				anomalyInfo(id);					/* show nearest anomaly detail */
				drawBTDTInfo(id);					/* draws the BTDT anomaly viewer */
				//if (parts <= 0) noData(id);		/* nothing to show */
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
			infoLabel = "No Data";
		}

		//Displays current vessel location info
		private void locationInfo(int id)
		{
			infoLabel += string.Format("Lat: {0:F2}°, Lon: {1:F2}°", vlat, vlon);

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
						infoLabel += string.Format("\nWaypoint: {0}", p.Name);
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
				infoLabel += string.Format("\nBiome: {0}", SCANUtil.getBiomeName(v.mainBody, vlon, vlat));
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
					infoLabel += string.Format("\nTerrain: {0:N1}m", pqs);
				else
					infoLabel += string.Format("\nAltitude: {0}", SCANuiUtil.distanceString(h, 100000));

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

						slopeAVG = SCANUtil.slope(pqs, v.mainBody, vlon, vlat, degreeOffset);
						slopeString = string.Format("\nSlope: {0:F2}°", slopeAVG);
					}

					infoLabel += slopeString;
				}
			}
		}

		//Display resource abundace info
		private void resourceInfo(int id)
		{
			if (SCANcontroller.controller.needsNarrowBand)
			{
				bool tooHigh = false;
				bool scanner = false;

				foreach (SCANresourceDisplay s in resourceScanners)
				{
					if (s == null)
						continue;

					if (s.ResourceName != resources[currentResource].Name)
						continue;

					if (ResourceUtilities.GetAltitude(v) > s.MaxAbundanceAltitude && !v.Landed)
					{
						tooHigh = true;
						continue;
					}

					scanner = true;
					tooHigh = false;
					break;
				}

				if (tooHigh)
				{
					infoLabel += string.Format("\n{0}: Too High", resources[currentResource].Name);
				}
				else if (!scanner)
				{
					infoLabel += string.Format("\n{0}: No Scanner", resources[currentResource].Name);
				}
				else
				{
					resourceLabel(resources[currentResource]);
				}
			}
			else
			{
				resourceLabel(resources[currentResource]);
			}
		}

		private void resourceLabel(SCANresourceGlobal r)
		{
			if ((sensors & r.SType) != SCANtype.Nothing)
			{
				infoLabel += string.Format("\n{0}: {1:P2}", r.Name, SCANUtil.ResourceOverlay(vlat, vlon, r.Name, v.mainBody, SCANcontroller.controller.resourceBiomeLock));
			}
			else if ((sensors & SCANtype.FuzzyResources) != SCANtype.Nothing)
			{
				infoLabel += string.Format("\n{0}: {1:P0}", r.Name, SCANUtil.ResourceOverlay(vlat, vlon, r.Name, v.mainBody, SCANcontroller.controller.resourceBiomeLock));
			}
			else
			{
				infoLabel += string.Format("\n{0}: No Data", r.Name);
			}
		}

		private void drawInfoLabel(int id)
		{
			GUILayout.Label(infoLabel, SCANskins.SCAN_insColorLabel);
		}

		private void drawResourceButtons(int id)
		{
			if (resources.Count > 1)
			{
				Rect r = GUILayoutUtility.GetLastRect();

				r.x = 8;
				r.y = r.yMax - 30;
				r.width = 18;
				r.height = 28;

				if (GUI.Button(r, "<"))
				{
					currentResource -= 1;
					if (currentResource < 0)
						currentResource = resources.Count - 1;
				}

				r.x = WindowRect.width - 24;

				if (GUI.Button(r, ">"))
				{
					currentResource += 1;
					if (currentResource >= resources.Count)
						currentResource = 0;
				}
			}
		}

		//Display info on the nearest anomaly *Need to separate the BTDT display*
		private void anomalyInfo(int id)
		{
			if ((sensors & SCANtype.AnomalyDetail) != SCANtype.Nothing)
			{
				nearest = null;
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
					string txt = "";
					if (nearest.Detail)
						txt = nearest.Name;
					else
						txt += "Anomaly";

					fillS(-10);
					GUILayout.Label(txt, SCANskins.SCAN_insColorLabel);
				}
			}
			else
				nearest = null;
		}

		private void drawBTDTInfo(int id)
		{
			if (nearest == null)
				return;

			if (!nearest.Detail)
				return;

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

		private void vesselChange(Vessel V)
		{
			resetResourceList();
		}

		public void resetResourceList()
		{
			resourceScanners = new List<SCANresourceDisplay>();

			if (v == null)
				return;

			foreach (SCANresourceDisplay s in v.FindPartModulesImplementing<SCANresourceDisplay>())
			{
				if (s == null)
					continue;

				if (resourceScanners.Contains(s))
					continue;

				resourceScanners.Add(s);
			}
		}

	}
}
