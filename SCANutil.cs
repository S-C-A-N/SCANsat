/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 * 
 * SCANutil - various static utilities methods used througout SCANsat
 * 
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using OpenResourceSystem;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using palette = SCANsat.SCANpalette;

namespace SCANsat
{

	public static class SCANUtil
	{
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
			SCANdata data = getData(body);
			return (data.coverage[ilon, ilat] & SCANtype) != 0;
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
			SCANdata data = getData(body);
			return (data.coverage[lon, lat] & SCANtype) != 0;
		}

		internal static bool isCovered(double lon, double lat, SCANdata data, SCANdata.SCANtype type)
		{
			int ilon = icLON(lon);
			int ilat = icLAT(lat);
			if (badLonLat(ilon, ilat)) return false;
			return (data.coverage[ilon, ilat] & (Int32)type) != 0;
		}

		internal static bool isCovered(int lon, int lat, SCANdata data, SCANdata.SCANtype type)
		{
			if (badLonLat(lon, lat)) return false;
			return (data.coverage[lon, lat] & (Int32)type) != 0;
		}

		internal static bool isCoveredByAll (int lon, int lat, SCANdata data, SCANdata.SCANtype type)
		{
			if (badLonLat(lon,lat)) return false;
			return (data.coverage[lon, lat] & (Int32)type) == (Int32)type;
		}

		internal static void registerPass ( double lon, double lat, SCANdata data, SCANdata.SCANtype type ) {
			int ilon = SCANUtil.icLON(lon);
			int ilat = SCANUtil.icLAT(lat);
			if (SCANUtil.badLonLat(ilon, ilat)) return;
			data.coverage [ilon, ilat] |= (Int32)type;
		}

		internal static double getCoveragePercentage(CelestialBody body, SCANdata.SCANtype type )
		{
			SCANdata data = getData(body);
			double cov = 0d;
			if (type == SCANdata.SCANtype.Nothing)
				type = SCANdata.SCANtype.AltimetryLoRes | SCANdata.SCANtype.AltimetryHiRes | SCANdata.SCANtype.Biome | SCANdata.SCANtype.Anomaly;          
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

		public static SCANdata getData(CelestialBody body)
		{
			if (!SCANcontroller.body_data.ContainsKey(body.name)) {
				SCANcontroller.body_data[body.name] = new SCANdata(body);
			}
			SCANdata data = SCANcontroller.body_data[body.name];
			return data;
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

		internal static ScienceData getAvailableScience(Vessel v, SCANdata.SCANtype sensor, bool notZero)
		{
			SCANdata data = getData(v.mainBody);
			ScienceData sd = null;
			ScienceExperiment se = null;
			ScienceSubject su = null;
			bool found = false;
			string id = null;
			double coverage = 0f;

			if(!found && (sensor & SCANdata.SCANtype.AltimetryLoRes) != SCANdata.SCANtype.Nothing) {
				found = true;
				id = "SCANsatAltimetryLoRes";
				coverage = data.getCoveragePercentage(SCANdata.SCANtype.AltimetryLoRes);
			}
			else if(!found && (sensor & SCANdata.SCANtype.AltimetryHiRes) != SCANdata.SCANtype.Nothing) {
				found = true;
				id = "SCANsatAltimetryHiRes";
				coverage = data.getCoveragePercentage(SCANdata.SCANtype.AltimetryHiRes);
			}
			else if(!found && (sensor & SCANdata.SCANtype.Biome) != SCANdata.SCANtype.Nothing) {
				found = true;
				id = "SCANsatBiomeAnomaly";
				coverage = data.getCoveragePercentage(SCANdata.SCANtype.Biome);
			}
			if(!found) return null;
			se = ResearchAndDevelopment.GetExperiment(id);
			if(se == null) return null;

			su = ResearchAndDevelopment.GetExperimentSubject(se, ExperimentSituations.InSpaceHigh, v.mainBody, "surface");
			if(su == null) return null;

			debugLog("Coverage: {0}, Science cap: {1}, Subject value: {2}, Scientific value: {3}, Science: {4}", new object[5] {coverage.ToString("F1"), su.scienceCap.ToString("F1"), su.subjectValue.ToString("F2"), su.scientificValue.ToString("F2"), su.science.ToString("F2")});

			su.scientificValue = 1;

			float science = (float)coverage;
			if(science > 95) science = 100;
			if(science < 30) science = 0;
			science = science / 100f;
			science = Mathf.Max(0, (science * su.scienceCap) - su.science);

			debugLog("Remaining science: {0}, Base value: {1}", new object[2] {science.ToString("F1"), se.baseValue.ToString("F1")});

			science /= Mathf.Max(0.1f, su.scientificValue);
			science /= su.subjectValue;

			debugLog("Resulting science value: {0}", new object[1] {science.ToString("F2")});

			if(notZero && science <= 0) science = 0.00001f;

			sd = new ScienceData(science * su.dataScale, 1f, 0f, su.id, se.experimentTitle + " of " + v.mainBody.theName);
			su.title = sd.title;
			return sd;
		}

		internal static double ORSOverlay(double lon, double lat, int i, string s)
		{
			double amount = 0f;
			ORSPlanetaryResourcePixel overlayPixel = ORSPlanetaryResourceMapData.getResourceAvailability(i, s, lat, lon);
			amount = overlayPixel.getAmount();
			return amount;
		}

		internal static int getBiomeIndex(CelestialBody body, double lon , double lat)
		{
			if (body.BiomeMap == null)		return -1;
			if (body.BiomeMap.Map == null)	return -1;
			double u = fixLon(lon);
			double v = fixLat(lat);

			if (badDLonLat(u, v))
				return -1;
			CBAttributeMap.MapAttribute att = body.BiomeMap.GetAtt (Mathf.Deg2Rad * lat , Mathf.Deg2Rad * lon);
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

		internal static CBAttributeMap.MapAttribute getBiome(CelestialBody body, double lon , double lat)
		{
			if (body.BiomeMap == null) return null;
			if (body.BiomeMap.Map == null) return body.BiomeMap.defaultAttribute;
			int i = getBiomeIndex(body, lon , lat);
			if (i < 0) return body.BiomeMap.defaultAttribute;
			else return body.BiomeMap.Attributes [i];
		}

		internal static string getBiomeName(CelestialBody body, double lon , double lat)
		{
			CBAttributeMap.MapAttribute a = getBiome (body, lon , lat);
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

		internal static void debugLog(string log, params object[] stringObjects)
		{
			log = string.Format(log, stringObjects);
			string finalLog = string.Format("[SCANsat] {0}", log);
			Debug.Log(finalLog);
		}

		//Take the Int32[] coverage and convert it to a single dimension byte array
		private static byte[] ConvertToByte(Int32[,] iArray)
		{
			byte[] bArray = new byte[360 * 180 * 4];
			int k = 0;
			for (int i = 0; i < 360; i++) {
				for (int j = 0; j < 180; j++) {
					byte[] bytes = BitConverter.GetBytes(iArray[i, j]);
					for (int m = 0; m < bytes.Length; m++) {
						bArray[k++] = bytes[m];
					}
				}
			}
			return bArray;
		}

		//Convert byte array from persistent file to usable Int32[]
		private static Int32[,] ConvertToInt(byte[] bArray)
		{
			Int32[,] iArray = new Int32[360, 180];
			int k = 0;
			for (int i = 0; i < 360; i++) {
				for (int j = 0; j < 180; j++) {
					iArray[i, j] = BitConverter.ToInt32(bArray, k);
					k += 4;
				}
			}
			return iArray;
		}

		//One time conversion of single byte[,] to Int32 to recover old scanning data
		private static Int32[,] RecoverToInt(byte[,] bArray)
		{
			Int32[,] iArray = new Int32[360, 180];
			for (int i = 0; i < 360; i++) {
				for (int j = 0; j < 180; j++) {
					iArray[i, j] = (Int32)bArray[i, j];
				}
			}
			return iArray;
		}

		/* DATA: serialization and compression */
		internal static string integerSerialize(SCANdata data)
		{
			byte[] bytes = ConvertToByte(data.coverage);
			MemoryStream mem = new MemoryStream();
			BinaryFormatter binf = new BinaryFormatter();
			binf.Serialize(mem, bytes);
			string blob = Convert.ToBase64String(CLZF2.Compress(mem.ToArray()));
			return blob.Replace("/", "-").Replace("=", "_");
		}

		internal static void integerDeserialize(string blob, bool b, SCANdata data)
		{
			try {
				blob = blob.Replace("-", "/").Replace("_", "=");
				byte[] bytes = Convert.FromBase64String(blob);
				bytes = CLZF2.Decompress(bytes);
				MemoryStream mem = new MemoryStream(bytes, false);
				BinaryFormatter binf = new BinaryFormatter();
				if (b) {
					byte[,] bRecover = new byte[360, 180];
					bRecover = (byte[,])binf.Deserialize(mem);
					data.coverage = RecoverToInt(bRecover);
				}
				else {
					byte[] bArray = (byte[])binf.Deserialize(mem);
					data.coverage = ConvertToInt(bArray);
				}
			}
			catch (Exception e) {
				data.coverage = new Int32[360, 180];
				data.heightmap = new float[360, 180];
				throw e;
			}
			data.resetImages();
		}

	}

	#region MechJeb Extensions

	// This extension is from MechJeb; Used with permission from r4m0n: https://github.com/MuMech/MechJeb2/blob/master/MechJeb2/OrbitExtensions.cs

		public static class OrbitExtensions
		{
				//Returns whether a has an ascending node with b. This can be false
				//if a is hyperbolic and the would-be ascending node is within the opening
				//angle of the hyperbola.
				public static bool AscendingNodeExists(this Orbit a, Orbit b)
				{
						return Math.Abs(JUtil.ClampDegrees180(a.AscendingNodeTrueAnomaly(b))) <= a.MaximumTrueAnomaly();
				}
				//Gives the true anomaly (in a's orbit) at which a crosses its ascending node
				//with b's orbit.
				//The returned value is always between 0 and 360.
				public static double AscendingNodeTrueAnomaly(this Orbit a, Orbit b)
				{
						Vector3d vectorToAN = Vector3d.Cross (a.SwappedOrbitNormal (), b.SwappedOrbitNormal ());
						return a.TrueAnomalyFromVector(vectorToAN);

				}
				//For hyperbolic orbits, the true anomaly only takes on values in the range
				// -M < true anomaly < +M for some M. This function computes M.
				public static double MaximumTrueAnomaly(this Orbit o)
				{
						if (o.eccentricity < 1)
								return 180;
						return 180 / Math.PI * Math.Acos(-1 / o.eccentricity);
				}
				//normalized vector perpendicular to the orbital plane
				//convention: as you look down along the orbit normal, the satellite revolves counterclockwise
				public static Vector3d SwappedOrbitNormal(this Orbit o)
				{
						return -(o.GetOrbitNormal().xzy).normalized;
				}

				//Returns whether a has a descending node with b. This can be false
				//if a is hyperbolic and the would-be descending node is within the opening
				//angle of the hyperbola.
				public static bool DescendingNodeExists(this Orbit a, Orbit b)
				{
						return Math.Abs(JUtil.ClampDegrees180(a.DescendingNodeTrueAnomaly(b))) <= a.MaximumTrueAnomaly();
				}
				//Gives the true anomaly (in a's orbit) at which a crosses its descending node
				//with b's orbit.
				//The returned value is always between 0 and 360.
				public static double DescendingNodeTrueAnomaly(this Orbit a, Orbit b)
				{
						return JUtil.ClampDegrees360(a.AscendingNodeTrueAnomaly(b) + 180);
				}
				//Returns the next time at which a will cross its ascending node with b.
				//For elliptical orbits this is a time between UT and UT + a.period.
				//For hyperbolic orbits this can be any time, including a time in the past if
				//the ascending node is in the past.
				//NOTE: this function will throw an ArgumentException if a is a hyperbolic orbit and the "ascending node"
				//occurs at a true anomaly that a does not actually ever attain
				public static double TimeOfAscendingNode(this Orbit a, Orbit b, double UT)
				{
						return a.TimeOfTrueAnomaly(a.AscendingNodeTrueAnomaly(b), UT);
				}
				//Returns the next time at which a will cross its descending node with b.
				//For elliptical orbits this is a time between UT and UT + a.period.
				//For hyperbolic orbits this can be any time, including a time in the past if
				//the descending node is in the past.
				//NOTE: this function will throw an ArgumentException if a is a hyperbolic orbit and the "descending node"
				//occurs at a true anomaly that a does not actually ever attain
				public static double TimeOfDescendingNode(this Orbit a, Orbit b, double UT)
				{
						return a.TimeOfTrueAnomaly(a.DescendingNodeTrueAnomaly(b), UT);
				}
				//NOTE: this function can throw an ArgumentException, if o is a hyperbolic orbit with an eccentricity
				//large enough that it never attains the given true anomaly
				public static double TimeOfTrueAnomaly(this Orbit o, double trueAnomaly, double UT)
				{
						return o.UTAtMeanAnomaly(o.GetMeanAnomalyAtEccentricAnomaly(o.GetEccentricAnomalyAtTrueAnomaly(trueAnomaly)), UT);
				}
				//The next time at which the orbiting object will reach the given mean anomaly.
				//For elliptical orbits, this will be a time between UT and UT + o.period
				//For hyperbolic orbits, this can be any time, including a time in the past, if
				//the given mean anomaly occurred in the past
				public static double UTAtMeanAnomaly(this Orbit o, double meanAnomaly, double UT)
				{
						double currentMeanAnomaly = o.MeanAnomalyAtUT(UT);
						double meanDifference = meanAnomaly - currentMeanAnomaly;
						if (o.eccentricity < 1)
								meanDifference = JUtil.ClampRadiansTwoPi(meanDifference);
						return UT + meanDifference / o.MeanMotion();
				}
				//Converts an eccentric anomaly into a mean anomaly.
				//For an elliptical orbit, the returned value is between 0 and 2pi
				//For a hyperbolic orbit, the returned value is any number
				public static double GetMeanAnomalyAtEccentricAnomaly(this Orbit o, double E)
				{
						double e = o.eccentricity;
						if (e < 1) { //elliptical orbits
								return JUtil.ClampRadiansTwoPi(E - (e * Math.Sin(E)));
						} //hyperbolic orbits
						return (e * Math.Sinh(E)) - E;
				}
				//Originally by Zool, revised by The_Duck
				//Converts a true anomaly into an eccentric anomaly.
				//For elliptical orbits this returns a value between 0 and 2pi
				//For hyperbolic orbits the returned value can be any number.
				//NOTE: For a hyperbolic orbit, if a true anomaly is requested that does not exist (a true anomaly
				//past the true anomaly of the asymptote) then an ArgumentException is thrown
				public static double GetEccentricAnomalyAtTrueAnomaly(this Orbit o, double trueAnomaly)
				{
						double e = o.eccentricity;
						trueAnomaly = JUtil.ClampDegrees360(trueAnomaly);
						trueAnomaly = trueAnomaly * (Math.PI / 180);

						if (e < 1) { //elliptical orbits
								double cosE = (e + Math.Cos(trueAnomaly)) / (1 + e * Math.Cos(trueAnomaly));
								double sinE = Math.Sqrt(1 - (cosE * cosE));
								if (trueAnomaly > Math.PI)
										sinE *= -1;

								return JUtil.ClampRadiansTwoPi(Math.Atan2(sinE, cosE));
						} else {  //hyperbolic orbits
								double coshE = (e + Math.Cos(trueAnomaly)) / (1 + e * Math.Cos(trueAnomaly));
								if (coshE < 1)
										throw new ArgumentException("OrbitExtensions.GetEccentricAnomalyAtTrueAnomaly: True anomaly of " + trueAnomaly + " radians is not attained by orbit with eccentricity " + o.eccentricity);

								double E = JUtil.Acosh(coshE);
								if (trueAnomaly > Math.PI)
										E *= -1;

								return E;
						}
				}
				//The mean anomaly of the orbit.
				//For elliptical orbits, the value return is always between 0 and 2pi
				//For hyperbolic orbits, the value can be any number.
				public static double MeanAnomalyAtUT(this Orbit o, double UT)
				{
						double ret = o.meanAnomalyAtEpoch + o.MeanMotion() * (UT - o.epoch);
						if (o.eccentricity < 1)
								ret = JUtil.ClampRadiansTwoPi(ret);
						return ret;
				}
				//mean motion is rate of increase of the mean anomaly
				public static double MeanMotion(this Orbit o)
				{
						return Math.Sqrt(o.referenceBody.gravParameter / Math.Abs(Math.Pow(o.semiMajorAxis, 3)));
				}
				//Converts a direction, specified by a Vector3d, into a true anomaly.
				//The vector is projected into the orbital plane and then the true anomaly is
				//computed as the angle this vector makes with the vector pointing to the periapsis.
				//The returned value is always between 0 and 360.
				public static double TrueAnomalyFromVector(this Orbit o, Vector3d vec)
				{
						Vector3d projected = Vector3d.Exclude(o.SwappedOrbitNormal(), vec);
						Vector3d vectorToPe = o.eccVec.xzy;
						double angleFromPe = Math.Abs(Vector3d.Angle(vectorToPe, projected));

						//If the vector points to the infalling part of the orbit then we need to do 360 minus the
						//angle from Pe to get the true anomaly. Test this by taking the the cross product of the
						//orbit normal and vector to the periapsis. This gives a vector that points to center of the 
						//outgoing side of the orbit. If vectorToAN is more than 90 degrees from this vector, it occurs
						//during the infalling part of the orbit.
						if (Math.Abs(Vector3d.Angle(projected, Vector3d.Cross(o.SwappedOrbitNormal(), vectorToPe))) < 90) {
								return angleFromPe;
						}
						return 360 - angleFromPe;
				}
				//distance from the center of the planet
				public static double Radius(this Orbit o, double UT)
				{
						return o.SwappedRelativePositionAtUT(UT).magnitude;
				}
				//position relative to the primary
				public static Vector3d SwappedRelativePositionAtUT(this Orbit o, double UT)
				{
						return o.getRelativePositionAtUT(UT).xzy;
				}
		}

	#endregion

		#region fix Duplicated Code

		// Mihara: Notice that quite a bit of it, at least conceptually, duplicates code that SCANsat already contains elsewhere,
		// and in general needs trimming.

		public static class MapIcons
		{
				public enum OtherIcon
				{
						None,
						PE,
						AP,
						AN,
						DN,
						NODE,
						SHIPATINTERCEPT,
						TGTATINTERCEPT,
						ENTERSOI,
						EXITSOI,
						PLANET,
				}

				public static Rect VesselTypeIcon(VesselType type, OtherIcon icon)
				{
						int x = 0;
						int y = 0;
						const float symbolSpan = 0.2f;
						if (icon != OtherIcon.None) {
								switch (icon) {
								case OtherIcon.AP:
										x = 1;
										y = 4;
										break;
								case OtherIcon.PE:
										x = 0;
										y = 4;
										break;
								case OtherIcon.AN:
										x = 2;
										y = 4;
										break;
								case OtherIcon.DN:
										x = 3;
										y = 4;
										break;
								case OtherIcon.NODE:
										x = 2;
										y = 1;
										break;
								case OtherIcon.SHIPATINTERCEPT:
										x = 0;
										y = 1;
										break;
								case OtherIcon.TGTATINTERCEPT:
										x = 1;
										y = 1;
										break;
								case OtherIcon.ENTERSOI:
										x = 0;
										y = 2;
										break;
								case OtherIcon.EXITSOI:
										x = 1;
										y = 2;
										break;
								case OtherIcon.PLANET:
										// Not sure if it is (2,3) or (3,2) - both are round
										x = 2;
										y = 3;
										break;
								}
						} else {
								switch (type) {
								case VesselType.Base:
										x = 2;
										y = 0;
										break;
								case VesselType.Debris:
										x = 1;
										y = 3;
										break;
								case VesselType.EVA:
										x = 2;
										y = 2;
										break;
								case VesselType.Flag:
										x = 4;
										y = 0;
										break;
								case VesselType.Lander:
										x = 3;
										y = 0;
										break;
								case VesselType.Probe:
										x = 1;
										y = 0;
										break;
								case VesselType.Rover:
										x = 0;
										y = 0;
										break;
								case VesselType.Ship:
										x = 0;
										y = 3;
										break;
								case VesselType.Station:
										x = 3;
										y = 1;
										break;
								case VesselType.Unknown:
										x = 3;
										y = 3;
										break;
								case VesselType.SpaceObject:
										x = 4;
										y = 1;
										break;
								default:
										x = 3;
										y = 2;
										break;
								}
						}
						var result = new Rect();
						result.x = symbolSpan * x;
						result.y = symbolSpan * y;
						result.height = result.width = symbolSpan;
						return result;
				}
		}

		#endregion

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

