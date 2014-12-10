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
using SCANsat.Platform;
using SCANsat;
using UnityEngine;

using palette = SCANsat.SCAN_UI.SCANpalette;

namespace SCANsat.SCAN_UI
{
	class SCANinstrumentUI: SCAN_MBW
	{
		private bool notMappingToday; //Unused out-of-power bool
		private SCANremoteView anomalyView;
		private SCANdata.SCANtype sensors;
		private SCANdata data;
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
		}

		internal override void OnDestroy()
		{
			GameEvents.onVesselSOIChanged.Remove(soiChange);
		}

		protected override void DrawWindowPre(int id)
		{
			//Grab the active scanners on this vessel
			sensors = SCANcontroller.controller.activeSensorsOnVessel(FlightGlobals.ActiveVessel.id);

			//if (maptraq_frame >= Time.frameCount - 5) //Still not sure what this actually does
			if (true)
			{
				//Check if region below the vessel is scanned
				if (SCANUtil.isCovered(FlightGlobals.ActiveVessel.longitude, FlightGlobals.ActiveVessel.latitude, data, SCANdata.SCANtype.AltimetryLoRes))
				{
					sensors |= SCANdata.SCANtype.Altimetry;
				}
				else if (SCANUtil.isCovered(FlightGlobals.ActiveVessel.longitude, FlightGlobals.ActiveVessel.latitude, data, SCANdata.SCANtype.AltimetryHiRes))
				{
					sensors |= SCANdata.SCANtype.Altimetry;
				}
				if (SCANUtil.isCovered(FlightGlobals.ActiveVessel.longitude, FlightGlobals.ActiveVessel.latitude, data, SCANdata.SCANtype.Biome))
				{
					sensors |= SCANdata.SCANtype.Biome;
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
				if (altInfo(id)) ++parts;			/* show current altitude *FixMe - inaccurate* */
				if (anomalyInfo(id)) ++parts;		/* show nearest anomaly detail - including BTDT view */
				if (parts <= 0) noData(id);			/* nothing to show */
			}
			stopS();
		}

		//Draw the version label in the upper left corner
		private void versionLabel(int id)
		{
			Rect r = new Rect(4, 0, 40, 18);
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
			if ((sensors & SCANdata.SCANtype.Biome) != SCANdata.SCANtype.Nothing)
			{
				GUILayout.Label(string.Format("Biome:  {0}", SCANUtil.getBiomeName(FlightGlobals.ActiveVessel.mainBody, FlightGlobals.ActiveVessel.longitude, FlightGlobals.ActiveVessel.latitude)), SCANskins.SCAN_insColorLabel);
				fillS(-10);
				return true;
			}
			return false;
		}

		//Display the current vessel altitude *Needs to be fixed to display accurate alt*
		private bool altInfo(int id)
		{
			if ((sensors & SCANdata.SCANtype.Altimetry) != SCANdata.SCANtype.Nothing)
			{
				double h = FlightGlobals.ActiveVessel.heightFromTerrain;
				if (h < 0)
					h = FlightGlobals.ActiveVessel.altitude;
				GUILayout.Label(string.Format("Altitude:  {0}", SCANuiUtil.distanceString(h, 100000)), SCANskins.SCAN_insColorLabel);
				fillS(-10);
				return true;
			}
			return false;
		}

		//Display info on the nearest anomaly *Need to separate the BTDT display*
		private bool anomalyInfo(int id)
		{
			if ((sensors & SCANdata.SCANtype.AnomalyDetail) != SCANdata.SCANtype.Nothing)
			{
				SCANdata.SCANanomaly nearest = null;
				double nearest_dist = -1;
				foreach (SCANdata.SCANanomaly a in data.Anomalies)
				{
					if (!a.known)
						continue;
					double d = (a.mod.transform.position - FlightGlobals.ActiveVessel.transform.position).magnitude;
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
					if (nearest.detail)
						txt = nearest.name;
					txt += ":  " + SCANuiUtil.distanceString(nearest_dist, 5000);
					GUILayout.Label(txt, SCANskins.SCAN_insColorLabel);

					if (anomalyView == null)
						anomalyView = new SCANremoteView();
					if (anomalyView != null)
					{
						if (nearest.mod != null)
						{
							if (anomalyView.lookat != nearest.mod.gameObject)
								anomalyView.setup(320, 240, nearest.mod.gameObject);
							Texture t = anomalyView.getTexture();
							if (t != null)
							{
								GUILayout.Label(anomalyView.getTexture());
								anomalyView.drawOverlay(GUILayoutUtility.GetLastRect(), SCANskins.SCAN_anomalyOverlay, nearest.detail);
							}
						}
					}
					return true;
				}
				return false;
			}
			return false;
		}

		private void soiChange (GameEvents.HostedFromToAction<Vessel, CelestialBody> VC)
		{
			data = SCANUtil.getData(VC.to);
			if (data == null)
			{
				data = new SCANdata(VC.to);
				SCANcontroller.controller.addToBodyData(VC.to, data);
			}
		}

	}
}
