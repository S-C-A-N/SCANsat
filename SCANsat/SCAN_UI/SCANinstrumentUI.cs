

using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.Platform;
using SCANsat;
using UnityEngine;

using palette = SCANsat.SCANpalette;

namespace SCANsat.SCAN_UI
{
	class SCANinstrumentUI: MBW
	{
		private bool notMappingToday;
		private RemoteView anomalyView;
		private SCANdata.SCANtype sensors;
		private SCANdata data;

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Instruments";
			WindowRect = new Rect(20, 455, 200, 60);
			WindowStyle = SCANskins.SCAN_window;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(200), GUILayout.Height(60) };
			Visible = true;
			DragEnabled = true;

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		internal override void OnDestroy()
		{
			
		}

		protected override void DrawWindowPre(int id)
		{
			sensors = SCANcontroller.controller.activeSensorsOnVessel(FlightGlobals.ActiveVessel.id);
			data = SCANUtil.getData(FlightGlobals.currentMainBody);

			//if (maptraq_frame >= Time.frameCount - 5)
			if (true)
			{
				if (SCANUtil.isCovered(FlightGlobals.ActiveVessel.longitude, FlightGlobals.ActiveVessel.latitude, data, SCANdata.SCANtype.AltimetryHiRes))
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
			versionLabel(id);
			closeBox(id);

			int parts = 0;

			growS();
			if (notMappingToday)
			{
				GUILayout.Label("NO DATA", SCANskins.SCAN_insWhiteLabel);
			}
			else
			{
				if ((sensors & SCANdata.SCANtype.Biome) != SCANdata.SCANtype.Nothing)
				{
					biomeInfo(id);
					++parts;
				}
				if ((sensors & SCANdata.SCANtype.AltimetryHiRes) != SCANdata.SCANtype.Nothing)
				{
					altInfo(id);
					++parts;
				}
				if ((sensors & SCANdata.SCANtype.AnomalyDetail) != SCANdata.SCANtype.Nothing)
				{
					anomalyInfo(id);
					++parts;
				}
				if (parts <= 0)
				{
					GUILayout.Label("NO DATA", SCANskins.SCAN_insWhiteLabel);
				}
			}
			stopS();
		}

		private void versionLabel(int id)
		{
			Rect r = new Rect(4, 0, 40, 18);
			GUI.Label(r, SCANversions.SCANsatVersion, SCANskins.SCAN_whiteLabel);
		}

		private void closeBox(int id)
		{
			Rect r = new Rect(WindowRect.width - 20, 0, 18, 18);
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				Visible = false;
				SCANUtil.SCANlog("Close Info Box");
			}
		}

		private void biomeInfo(int id)
		{
			GUILayout.Label(SCANUtil.getBiomeName(FlightGlobals.ActiveVessel.mainBody, FlightGlobals.ActiveVessel.longitude, FlightGlobals.ActiveVessel.latitude), SCANskins.SCAN_insColorLabel);
		}

		private void altInfo(int id)
		{
			double h = FlightGlobals.ActiveVessel.heightFromTerrain;
			if (h < 0)
				h = FlightGlobals.ActiveVessel.altitude;
			GUILayout.Label(h.ToString("N2") + "m", SCANskins.SCAN_insColorLabel);
		}

		private void anomalyInfo(int id)
		{
			SCANdata.SCANanomaly nearest = null;
			double nearest_dist = -1;
			foreach (SCANdata.SCANanomaly a in data.getAnomalies())
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
				txt += " " + SCANuiUtil.distanceString(nearest_dist);
				GUILayout.Label(txt, SCANskins.SCAN_insColorLabel);
				if (anomalyView == null)
					anomalyView = new RemoteView();
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
			}
		}

	}
}
