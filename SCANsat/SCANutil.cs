#region license
/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 * 
 * SCANutil - various static utilities methods used througout SCANsat
 * 
 * Several extension methods borrowed from Mechjeb:
 * https://github.com/MuMech/MechJeb2/blob/master/MechJeb2/OrbitExtensions.cs
 * 
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SCANsat.SCAN_Platform;
using SCANsat.SCAN_Platform.Palettes;
using SCANsat.SCAN_Platform.Palettes.ColorBrewer;
using SCANsat.SCAN_Platform.Palettes.FixedColors;
using SCANsat.SCAN_Data;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat
{

	public static class SCANUtil
	{

		#region Public API Methods

		/// <summary>
		/// Determines scanning coverage for a given area with a given scanner type
		/// </summary>
		/// <param name="lon">Clamped double in the -180 - 180 degree range</param>
		/// <param name="lat">Clamped double in the -90 - 90 degree range</param>
		/// <param name="body">Celestial body in question</param>
		/// <param name="SCANtype">SCANtype cast as an integer</param>
		/// <returns></returns>
		public static bool isCovered(double lon, double lat, CelestialBody body, int SCANtype)
		{
			int ilon = icLON(lon);
			int ilat = icLAT(lat);
			if (badLonLat (ilon, ilat)) return false;
			if (SCANcontroller.controller != null)
			{
				SCANdata data = getData(body);
				if (data != null)
					return (data.Coverage[ilon, ilat] & SCANtype) != 0;
				else
					return false;
			}
			else
				return false;
		}

		/// <summary>
		/// Determines scanning coverage for a given area with a given scanner type
		/// </summary>
		/// <param name="lon">Clamped integer in the 0-360 degree range</param>
		/// <param name="lat">Clamped integer in the 0-180 degree range</param>
		/// <param name="body">Celestial body in question</param>
		/// <param name="SCANtype">SCANtype cast as an integer</param>
		/// <returns></returns>
		public static bool isCovered(int lon, int lat, CelestialBody body, int SCANtype)
		{
			if (badLonLat(lon, lat)) return false;
			if (SCANcontroller.controller != null)
			{
				SCANdata data = getData(body);
				if (data != null)
					return (data.Coverage[lon, lat] & SCANtype) != 0;
				else
					return false;
			}
			else
				return false;
		}

		/// <summary>
		/// Public method to return the scanning coverage for a given sensor type on a give body
		/// </summary>
		/// <param name="SCANtype">Integer corresponding to the desired SCANtype</param>
		/// <param name="Body">Desired Celestial Body</param>
		/// <returns>Scanning percentage as a double from 0-100</returns>
		public static double GetCoverage(int SCANtype, CelestialBody Body)
		{
			if (SCANcontroller.controller != null)
			{
				SCANdata data = getData(Body);
				if (data != null)
					return getCoveragePercentage(data, (SCANtype)SCANtype);
				else
					return 0;
			}
			else
				return 0;
		}

		/// <summary>
		/// Given the name of the SCANtype, returns the int value.
		/// </summary>
		/// <param name="SCANname">The name of the SCANtype.</param>
		/// <returns>The int value that can be used in other public methods.</returns>
		public static int GetSCANtype(string SCANname)
		{
			try
			{
				return (int)Enum.Parse(typeof(SCANtype), SCANname);
			}
			catch (ArgumentException e)
			{
				throw new ArgumentException("An invalid SCANtype name was provided.  Valid values are: " +
					string.Join(", ", ((IEnumerable<SCANtype>)Enum.GetValues(typeof(SCANtype))).Select<SCANtype, string>(x => x.ToString()).ToArray()));
			}
		}

		/// <summary>
		/// For a given Celestial Body this returns the SCANdata instance if it exists in the SCANcontroller master dictionary; return is null if the SCANdata does not exist for that body (ie it has never been visited while SCANsat has been active)
		/// </summary>
		/// <param name="body">Celestial Body object</param>
		/// <returns>SCANdata instance for the given Celestial Body; null if none exists</returns>
		public static SCANdata getData(CelestialBody body)
		{
			return getData(body.name);
		}

		/// <summary>
		/// For a given Celestial Body name this returns the SCANdata instance if it exists in the SCANcontroller master dictionary; return is null if the SCANdata does not exist for that body (ie it has never been visited while SCANsat has been active), or if the SCANcontroller Scenario Module has not been loaded.
		/// </summary>
		/// <param name="BodyName">Name of celestial body (do not use TheName string)</param>
		/// <returns>SCANdata instance for the given Celestial Body; null if none exists</returns>
		public static SCANdata getData(string BodyName)
		{
			if (SCANcontroller.controller == null)
				return null;

			return SCANcontroller.controller.getData(BodyName);
		}

		#endregion

		#region Internal Utilities

		internal static bool isCovered(double lon, double lat, SCANdata data, SCANtype type)
		{
			int ilon = icLON(lon);
			int ilat = icLAT(lat);
			if (badLonLat(ilon, ilat)) return false;
			return (data.Coverage[ilon, ilat] & (Int32)type) != 0;
		}

		internal static bool isCovered(int lon, int lat, SCANdata data, SCANtype type)
		{
			if (badLonLat(lon, lat)) return false;
			return (data.Coverage[lon, lat] & (Int32)type) != 0;
		}

		internal static bool isCoveredByAll (int lon, int lat, SCANdata data, SCANtype type)
		{
			if (badLonLat(lon,lat)) return false;
			return (data.Coverage[lon, lat] & (Int32)type) == (Int32)type;
		}

		internal static void registerPass ( double lon, double lat, SCANdata data, SCANtype type ) {
			int ilon = SCANUtil.icLON(lon);
			int ilat = SCANUtil.icLAT(lat);
			if (SCANUtil.badLonLat(ilon, ilat)) return;
			data.Coverage[ilon, ilat] |= (Int32)type;
		}

		internal static double getCoveragePercentage(SCANdata data, SCANtype type )
		{
			if (data == null)
				return 0;
			double cov = 0d;
			if (type == SCANtype.Nothing)
				type = SCANtype.AltimetryLoRes | SCANtype.AltimetryHiRes | SCANtype.Biome | SCANtype.Anomaly;          
			cov = data.getCoverage (type);
			if (cov <= 0)
				cov = 100;
			else
				cov = Math.Min (99.9d , 100 - cov * 100d / (360d * 180d * countBits((int)type)));
			return cov;
		}

		internal static Func<double, int> icLON = (lon) => ((int)(lon + 360 + 180)) % 360;
		internal static Func<double, int> icLAT = (lat) => ((int)(lat + 180 + 90)) % 180;
		internal static Func<int, int, bool> badLonLat = (lon, lat) => (lon < 0 || lat < 0 || lon >= 360 || lat >= 180);
		internal static Func<double, double, bool> badDLonLat = (lon, lat) => (lon < 0 || lat <0 || lon >= 360 || lat >= 180);
		public static Func<double, double, bool> ApproxEq = (a, b) => Math.Abs(a - b) < 0.01;

		internal static double fixLatShift(double lat)
		{
			return (lat + 180 + 90) % 180 - 90;
		}

		internal static double fixLat(double lat)
		{
			return (lat + 180 + 90) % 180;
		}

		internal static double fixLonShift(double lon)
		{
			return (lon + 360 + 180) % 360 - 180;
		}

		internal static double fixLon(double lon)
		{
			return (lon + 360 + 180) % 360;
		}

		internal static double getElevation(CelestialBody body, double lon, double lat)
		{
			if (body.pqsController == null) return 0;
			double rlon = Mathf.Deg2Rad * lon;
			double rlat = Mathf.Deg2Rad * lat;
			Vector3d rad = new Vector3d(Math.Cos(rlat) * Math.Cos(rlon), Math.Sin(rlat), Math.Cos(rlat) * Math.Sin(rlon));
			return Math.Round(body.pqsController.GetSurfaceHeight(rad) - body.pqsController.radius, 1);
		}

		internal static double getElevation(CelestialBody body, int lon, int lat)
		{
			if (body.pqsController == null) return 0;
			double rlon = Mathf.Deg2Rad * lon;
			double rlat = Mathf.Deg2Rad * lat;
			Vector3d rad = new Vector3d(Math.Cos(rlat) * Math.Cos(rlon), Math.Sin(rlat), Math.Cos(rlat) * Math.Sin(rlon));
			return Math.Round(body.pqsController.GetSurfaceHeight(rad) - body.pqsController.radius, 1);
		}

		internal static double getElevation(this CelestialBody body, Vector3d worldPosition)
		{
			if (body.pqsController == null)
				return 0;
			Vector3d pqsRadialVector = QuaternionD.AngleAxis(body.GetLongitude(worldPosition), Vector3d.down) * QuaternionD.AngleAxis(body.GetLatitude(worldPosition), Vector3d.forward) * Vector3d.right;
			double ret = body.pqsController.GetSurfaceHeight(pqsRadialVector) - body.pqsController.radius;
			if (ret < 0)
				ret = 0;
			return ret;
		}

		internal static float ResourceOverlay(double lat, double lon, string name, CelestialBody body)
		{
			float amount = 0f;
			var aRequest = new AbundanceRequest
			{
				Latitude = lat,
				Longitude = lon,
				BodyId = body.flightGlobalsIndex,
				ResourceName = name,
				ResourceType = HarvestTypes.Planetary,
				Altitude = 0,
				CheckForLock = SCANcontroller.controller.resourceBiomeLock,
				BiomeName = getBiomeName(body, lon, lat),
				ExcludeVariance = false,
			};

			amount = ResourceMap.Instance.GetAbundance(aRequest);
			return amount;
		}

		internal static int getBiomeIndex(CelestialBody body, double lon , double lat)
		{
			if (body.BiomeMap == null)		return -1;
			double u = fixLon(lon);
			double v = fixLat(lat);

			if (badDLonLat(u, v))
				return -1;
			CBAttributeMapSO.MapAttribute att = body.BiomeMap.GetAtt (Mathf.Deg2Rad * lat , Mathf.Deg2Rad * lon);
			for (int i = 0; i < body.BiomeMap.Attributes.Length; ++i) {
				if (body.BiomeMap.Attributes [i] == att) {
					return i;
				}
			}
			return -1;
		}

		internal static double getBiomeIndexFraction(CelestialBody body, double lon , double lat)
		{
			if (body.BiomeMap == null) return 0f;
			return getBiomeIndex (body, lon , lat) * 1.0f / body.BiomeMap.Attributes.Length;
		}

		internal static CBAttributeMapSO.MapAttribute getBiome(CelestialBody body, double lon , double lat)
		{
			if (body.BiomeMap == null) return null;
			int i = getBiomeIndex(body, lon , lat);
			return body.BiomeMap.Attributes [i];
		}

		internal static string getBiomeName(CelestialBody body, double lon , double lat)
		{
			CBAttributeMapSO.MapAttribute a = getBiome (body, lon , lat);
			if (a == null)
				return "unknown";
			return a.name;
		}

		internal static int countBits(int i)
		{
			int count;
			for(count=0; i!=0; ++count) i &= (i - 1);
			return count;
		}

		internal static Palette paletteLoader(string name, int size)
		{
			if (name == "Default" || string.IsNullOrEmpty(name))
				return PaletteLoader.defaultPalette;
			else
			{
				try
				{
					if (name == "blackForest" || name == "departure" || name == "northRhine" || name == "mars" || name == "wiki2" || name == "plumbago" || name == "cw1_013" || name == "arctic")
					{
						//Load the fixed size color palette by name through reflection
						var fixedPallete = typeof(FixedColorPalettes);
						var fPaletteMethod = fixedPallete.GetMethod(name);
						var fColorP = fPaletteMethod.Invoke(null, null);
						return (Palette)fColorP;
					}
					else
					{
						//Load the ColorBrewer method by name through reflection
						var brewer = typeof(BrewerPalettes);
						var bPaletteMethod = brewer.GetMethod(name);
						var bColorP = bPaletteMethod.Invoke(null, new object[] { size });
						return (Palette)bColorP;
					}
				}
				catch (Exception e)
				{
					SCANUtil.SCANlog("Error Loading Color Palette; Revert To Default: {0}", e);
					return PaletteLoader.defaultPalette;
				}
			}
		}

		internal static void SCANlog(string log, params object[] stringObjects)
		{
			log = string.Format(log, stringObjects);
			string finalLog = string.Format("[SCANsat] {0}", log);
			Debug.Log(finalLog);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		internal static void SCANdebugLog(string log, params object[] stringObjects)
		{
			SCANlog(log, stringObjects);
		}

		#endregion

	}

		#region JUtil

		public static class JUtil
		{

				private static readonly int ClosestApproachRefinementInterval = 16;

				public static bool OrbitMakesSense(Vessel thatVessel)
				{
						if (thatVessel == null)
								return false;
						if (thatVessel.situation == Vessel.Situations.FLYING ||
								thatVessel.situation == Vessel.Situations.SUB_ORBITAL ||
								thatVessel.situation == Vessel.Situations.ORBITING ||
								thatVessel.situation == Vessel.Situations.ESCAPING ||
								thatVessel.situation == Vessel.Situations.DOCKED) // Not sure about this last one.
								return true;
						return false;
				}
				// Closest Approach algorithms based on Protractor mod
				public static double GetClosestApproach(Orbit vesselOrbit, CelestialBody targetCelestial, out double timeAtClosestApproach)
				{
						Orbit closestorbit = GetClosestOrbit(vesselOrbit, targetCelestial);
						if (closestorbit.referenceBody == targetCelestial) {
								timeAtClosestApproach = closestorbit.StartUT + ((closestorbit.eccentricity < 1.0) ?
										closestorbit.timeToPe :
										-closestorbit.meanAnomaly / (2 * Math.PI / closestorbit.period));
								return closestorbit.PeA;
						}
						if (closestorbit.referenceBody == targetCelestial.referenceBody) {
								return MinTargetDistance(closestorbit, targetCelestial.orbit, closestorbit.StartUT, closestorbit.EndUT, out timeAtClosestApproach) - targetCelestial.Radius;
						}
						return MinTargetDistance(closestorbit, targetCelestial.orbit, Planetarium.GetUniversalTime(), Planetarium.GetUniversalTime() + closestorbit.period, out timeAtClosestApproach) - targetCelestial.Radius;
				}
				public static double GetClosestApproach(Orbit vesselOrbit, CelestialBody targetCelestial, Vector3d srfTarget, out double timeAtClosestApproach)
				{
						Orbit closestorbit = GetClosestOrbit(vesselOrbit, targetCelestial);
						if (closestorbit.referenceBody == targetCelestial) {
								double t0 = Planetarium.GetUniversalTime();
								Func<double,Vector3d> fn = delegate(double t) {
										double angle = targetCelestial.rotates ? (t - t0) * 360.0 / targetCelestial.rotationPeriod : 0;
										return targetCelestial.position + QuaternionD.AngleAxis(angle, Vector3d.down) * srfTarget;
								};
								double d = MinTargetDistance(closestorbit, fn, closestorbit.StartUT, closestorbit.EndUT, out timeAtClosestApproach);
								// When just passed over the target, some look ahead may be needed
								if ((timeAtClosestApproach <= closestorbit.StartUT || timeAtClosestApproach >= closestorbit.EndUT) &&
										closestorbit.eccentricity < 1 && closestorbit.patchEndTransition == Orbit.PatchTransitionType.FINAL) {
										d = MinTargetDistance(closestorbit, fn, closestorbit.EndUT, closestorbit.EndUT + closestorbit.period / 2, out timeAtClosestApproach);
								}
								return d;
						}
						return GetClosestApproach(vesselOrbit, targetCelestial, out timeAtClosestApproach);
				}

				public static double GetClosestApproach(Orbit vesselOrbit, Orbit targetOrbit, out double timeAtClosestApproach)
				{
						Orbit closestorbit = GetClosestOrbit(vesselOrbit, targetOrbit);

						double startTime = Planetarium.GetUniversalTime();
						double endTime;
						if (closestorbit.patchEndTransition != Orbit.PatchTransitionType.FINAL) {
								endTime = closestorbit.EndUT;
						} else {
								endTime = startTime + Math.Max(closestorbit.period, targetOrbit.period);
						}

						return MinTargetDistance(closestorbit, targetOrbit, startTime, endTime, out timeAtClosestApproach);
				}

				// Closest Approach support methods
				private static Orbit GetClosestOrbit(Orbit vesselOrbit, CelestialBody targetCelestial)
				{
						Orbit checkorbit = vesselOrbit;
						int orbitcount = 0;

						while (checkorbit.nextPatch != null && checkorbit.patchEndTransition != Orbit.PatchTransitionType.FINAL && orbitcount < 3) {
								checkorbit = checkorbit.nextPatch;
								orbitcount += 1;
								if (checkorbit.referenceBody == targetCelestial) {
										return checkorbit;
								}

						}
						checkorbit = vesselOrbit;
						orbitcount = 0;

						while (checkorbit.nextPatch != null && checkorbit.patchEndTransition != Orbit.PatchTransitionType.FINAL && orbitcount < 3) {
								checkorbit = checkorbit.nextPatch;
								orbitcount += 1;
								if (checkorbit.referenceBody == targetCelestial.orbit.referenceBody) {
										return checkorbit;
								}
						}

						return vesselOrbit;
				}

				private static Orbit GetClosestOrbit(Orbit vesselOrbit, Orbit targetOrbit)
				{
						Orbit checkorbit = vesselOrbit;
						int orbitcount = 0;

						while (checkorbit.nextPatch != null && checkorbit.patchEndTransition != Orbit.PatchTransitionType.FINAL && orbitcount < 3) {
								checkorbit = checkorbit.nextPatch;
								orbitcount += 1;
								if (checkorbit.referenceBody == targetOrbit.referenceBody) {
										return checkorbit;
								}

						}

						return vesselOrbit;
				}

				private static double MinTargetDistance(Orbit vesselOrbit, Orbit targetOrbit, double startTime, double endTime, out double timeAtClosestApproach)
				{
						return MinTargetDistance(vesselOrbit, t => targetOrbit.getPositionAtUT(t), startTime, endTime, out timeAtClosestApproach);
				}

				private static double MinTargetDistance(Orbit vesselOrbit, Func<double,Vector3d> targetOrbit, double startTime, double endTime, out double timeAtClosestApproach)
				{
						var dist_at_int = new double[ClosestApproachRefinementInterval + 1];
						double step = startTime;
						double dt = (endTime - startTime) / (double)ClosestApproachRefinementInterval;
						for (int i = 0; i <= ClosestApproachRefinementInterval; i++) {
								dist_at_int[i] = (targetOrbit(step) - vesselOrbit.getPositionAtUT(step)).magnitude;
								step += dt;
						}
						double mindist = dist_at_int.Min();
						double maxdist = dist_at_int.Max();
						int minindex = Array.IndexOf(dist_at_int, mindist);
						if ((maxdist - mindist) / maxdist >= 0.00001) {
								// Don't allow negative times.  Clamp the startTime to the current startTime.
								mindist = MinTargetDistance(vesselOrbit, targetOrbit, startTime + (Math.Max(minindex - 1, 0) * dt), startTime + ((minindex + 1) * dt), out timeAtClosestApproach);
						} else {
								timeAtClosestApproach = startTime + minindex * dt;
						}

						return mindist;
				}
				// Some snippets from MechJeb...
				public static double ClampDegrees360(double angle)
				{
						angle = angle % 360.0;
						if (angle < 0)
								return angle + 360.0;
						return angle;
				}
				//keeps angles in the range -180 to 180
				public static double ClampDegrees180(double angle)
				{
						angle = ClampDegrees360(angle);
						if (angle > 180)
								angle -= 360;
						return angle;
				}
				//acosh(x) = log(x + sqrt(x^2 - 1))
				public static double Acosh(double x)
				{
						return Math.Log(x + Math.Sqrt(x * x - 1));
				}
				public static double ClampRadiansTwoPi(double angle)
				{
						angle = angle % (2 * Math.PI);
						if (angle < 0)
								return angle + 2 * Math.PI;
						return angle;
				}

				public static Material DrawLineMaterial()
				{
						var lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
								"SubShader { Pass {" +
								"   BindChannels { Bind \"Color\",color }" +
								"   Blend SrcAlpha OneMinusSrcAlpha" +
								"   ZWrite Off Cull Off Fog { Mode Off }" +
								"} } }");
						lineMaterial.hideFlags = HideFlags.HideAndDontSave;
						lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
						return lineMaterial;
				}

				public static bool IsActiveVessel(Vessel thatVessel)
				{
						return (HighLogic.LoadedSceneIsFlight && thatVessel != null	&& thatVessel.isActiveVessel);
				}
				public static bool IsInIVA()
				{
						return CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA || CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Internal;
				}
		}
	}
		#endregion

