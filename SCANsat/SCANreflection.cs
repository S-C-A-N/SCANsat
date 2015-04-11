#region license
/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANreflection - assigns reflection methods at startup
 * 
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FinePrint;
using FinePrint.Contracts.Parameters;
using FinePrint.Utilities;
using UnityEngine;

namespace SCANsat
{
	static class SCANreflection
	{

		private const string RegolithTypeName = "Regolith.Common.RegolithResourceMap";
		private const string RegolithAssemblyName = "Regolith";
		private const string RegolithMethodName = "GetAbundance";

		private static bool RegolithRun = false;
		private static bool FinePrintWaypointRun = false;
		private static bool FinePrintFlightBandRun = false;
		private static bool FinePrintStationaryWaypointRun = false;

		private delegate float RegolithPosAbundance(double lat, double lon, string resource, int body, int type, int altitude, bool biomes);

		private static RegolithPosAbundance _RegolithPosAbundance;

		private static FieldInfo _FinePrintWaypoint;
		private static FieldInfo _FinePrintFlightBand;
		private static FieldInfo _FinePrintStationaryWaypoint;

		internal static float RegolithAbundanceValue(double lat, double lon, string resource, int body, int type, int altitude, bool biomes)
		{
			return _RegolithPosAbundance(lat, lon, resource, body, type, altitude, biomes);
		}

		internal static Waypoint FinePrintWaypointObject(SurveyWaypointParameter p)
		{
			Waypoint w = null;
			try
			{
				w = (Waypoint)_FinePrintWaypoint.GetValue(p);
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error in detecting FinePrint Waypoint object: {0}", e);
			}

			return w;
		}

		internal static Waypoint FinePrintStationaryWaypointObject(StationaryPointParameter p)
		{
			Waypoint w = null;
			try
			{
				w = (Waypoint)_FinePrintStationaryWaypoint.GetValue(p);
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error in detecting FinePrint Stationary Waypoint object: {0}", e);
			}

			return w;
		}

		internal static FlightBand FinePrintFlightBandValue(SurveyWaypointParameter p)
		{
			FlightBand b = FlightBand.NONE;
			try
			{
				b = (FlightBand)_FinePrintFlightBand.GetValue(p);
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error in detecting FinePrint FlightBand object: {0}", e);
			}

			return b;
		}

		internal static bool RegolithReflectionMethod(Assembly RegolithAssembly)
		{
			if (_RegolithPosAbundance != null)
				return true;

			if (RegolithRun)
				return false;

			RegolithRun = true;

			try
			{
				Type RegolithType = RegolithAssembly.GetExportedTypes()
					.SingleOrDefault(t => t.FullName == RegolithTypeName);

				if (RegolithType == null)
				{
					SCANUtil.SCANlog("Regolith Type Not Found");
					return false;
				}

				MethodInfo RegolithMethod = RegolithType.GetMethod(RegolithMethodName, new Type[] { typeof(double), typeof(double), typeof(string), typeof(int), typeof(int), typeof(int), typeof(bool) });

				if (RegolithMethod == null)
				{
					SCANUtil.SCANlog("Regolith Method Not Found");
					return false;
				}

				_RegolithPosAbundance = (RegolithPosAbundance)Delegate.CreateDelegate(typeof(RegolithPosAbundance), RegolithMethod);

				SCANUtil.SCANlog("Regolith Reflection Method Assigned");

				return true;
			}
			catch (Exception e)
			{
				Debug.LogWarning("[SCANsat] Exception While Loading Regolith Reflection Method: " + e);
			}

			return false;
		}

		internal static bool FinePrintWaypointReflection()
		{
			if (_FinePrintWaypoint != null)
				return true;

			if (FinePrintWaypointRun)
				return false;

			FinePrintWaypointRun = true;

			try
			{
				Type sType = typeof(SurveyWaypointParameter);

				var field = sType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				_FinePrintWaypoint = field[0];

				if (_FinePrintWaypoint == null)
				{
					SCANUtil.SCANlog("FinePrint Waypoint Field Not Found");
					return false;
				}

				SCANUtil.SCANlog("FinePrint Waypoint Field Assigned");

				return _FinePrintWaypoint != null;
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error in assigning FinePrint Waypoint method: {0}", e);
			}

			return false;
		}

		internal static bool FinePrintStationaryWaypointReflection()
		{
			if (_FinePrintStationaryWaypoint != null)
				return true;

			if (FinePrintStationaryWaypointRun)
				return false;

			FinePrintStationaryWaypointRun = true;

			try
			{
				Type sType = typeof(StationaryPointParameter);

				var field = sType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				_FinePrintStationaryWaypoint = field[0];

				if (_FinePrintStationaryWaypoint == null)
				{
					SCANUtil.SCANlog("FinePrint Stationary Waypoint Field Not Found");
					return false;
				}

				SCANUtil.SCANlog("FinePrint Stationary Waypoint Field Assigned");

				return _FinePrintWaypoint != null;
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error in assigning FinePrint Stationary Waypoint method: {0}", e);
			}

			return false;
		}

		internal static bool FinePrintFlightBandReflection()
		{
			if (_FinePrintFlightBand != null)
				return true;

			if (FinePrintFlightBandRun)
				return false;

			FinePrintFlightBandRun = true;

			try
			{
				Type sType = typeof(SurveyWaypointParameter);

				var field = sType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				_FinePrintFlightBand = field[3];

				if (_FinePrintFlightBand == null)
				{
					SCANUtil.SCANlog("FinePrint FlightBand Field Not Found");
					return false;
				}

				SCANUtil.SCANlog("FinePrint FlightBand Field Assigned");

				return _FinePrintFlightBand != null;
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error in assigning FinePrint FlightBand method: {0}", e);
			}

			return false;
		}

	}
}
