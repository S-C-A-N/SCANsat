using System;
using System.Collections.Generic;
using System.Linq;

using SCANsat.SCAN_Data;
using SCANsat.SCAN_UI.UI_Framework;
using SCANsat.SCAN_Platform;
using UnityEngine;

namespace SCANsat.SCAN_UI
{
	public class SCANGLUtil : SCAN_MBE
	{


		protected override void OnGUIEvery()
		{
			if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
				drawTarget();
		}

		private void drawTarget()
		{
			if (!MapView.MapIsEnabled)
				return;

			CelestialBody b = SCANUtil.getTargetBody(MapView.MapCamera.target);

			if (b == null)
				return;

			SCANdata d = SCANUtil.getData(b.name);

			if (d == null)
				return;

			if (SCANcontroller.controller.groundTracks)
				drawGroundTracks(b);

			if (SCANcontroller.controller.mechJebTargetSelection)
				return;

			SCANwaypoint target = d.Waypoints.FirstOrDefault(a => a.LandingTarget);

			if (target == null)
				return;

			SCANuiUtil.drawTargetOverlay(b, target.Latitude, target.Longitude, XKCDColors.DarkGreen);
		}

		private void drawGroundTracks(CelestialBody body)
		{
			if (SCANcontroller.controller.groundTrackActiveOnly)
			{
				Vessel v = FlightGlobals.ActiveVessel;

				if (v.mainBody != body)
					return;

				if (v.situation == Vessel.Situations.LANDED || v.situation == Vessel.Situations.PRELAUNCH || v.situation == Vessel.Situations.SPLASHED)
					return;

				if (!SCANcontroller.controller.isVesselKnown(v))
					return;

				SCANcontroller.SCANvessel sv = SCANcontroller.controller.knownVessels[v.id];

				if (sv == null)
					return;

				Color col;

				double groundWidth = getFOV(sv, body, out col);

				if (groundWidth < 1)
					return;

				double surfaceScale = (2 * Math.PI * body.Radius) / 360;

				groundWidth *= surfaceScale;

				SCANuiUtil.drawGroundTrackTris(body, sv.vessel, groundWidth, col);
			}
			else
			{
				double surfaceScale = (2 * Math.PI * body.Radius) / 360;

				for (int j = 0; j < SCANcontroller.controller.knownVessels.Count; j++)
				{
					SCANcontroller.SCANvessel sv = SCANcontroller.controller.knownVessels.Values.ElementAt(j);

					if (sv == null)
						continue;

					if (sv.vessel.mainBody != body)
						continue;

					if (sv.vessel.situation == Vessel.Situations.LANDED || sv.vessel.situation == Vessel.Situations.PRELAUNCH || sv.vessel.situation == Vessel.Situations.SPLASHED)
						continue;

					Color col;

					double groundWidth = getFOV(sv, body, out col);

					if (groundWidth < 1)
						continue;

					groundWidth *= surfaceScale;

					SCANuiUtil.drawGroundTrackTris(body, sv.vessel, groundWidth, col);
				}
			}
		}

		private double getFOV(SCANcontroller.SCANvessel v, CelestialBody b, out Color c)
		{
			c = XKCDColors.DarkGreen;
			double maxFOV = 0;
			double alt = v.vessel.altitude;
			double soi_radius = b.sphereOfInfluence - b.Radius;
			double surfscale = Planetarium.fetch.Home.Radius / b.Radius;
			if (surfscale < 1)
				surfscale = 1;
			surfscale = Math.Sqrt(surfscale);

			for (int j = 0; j < v.sensors.Count; j++)
			{
				SCANcontroller.SCANsensor s = v.sensors.Values.ElementAt(j);

				if (alt < s.min_alt)
					continue;
				if (alt > Math.Min(s.max_alt, soi_radius))
					continue;

				double fov = s.fov;
				double ba = Math.Min(s.best_alt, soi_radius);
				if (alt < ba)
				{
					fov = (alt / ba) * fov;
				}

				fov *= surfscale;
				if (fov > 20)
					fov = 20;

				if (fov > maxFOV)
					maxFOV = fov;
			}

			return maxFOV;
		}
	}
}
