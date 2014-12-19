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
using UnityEngine;

namespace SCANsat
{
	static class SCANreflection
	{
		private const string ORSXPlanetDataType = "ORSX.ORSX_PlanetaryResourceMapData";
		private const string ORSXPixelAbundanceMethod = "getPixelAbundanceValue";
		private const string ORSXAssemblyName = "ORSX";

		private const string RegolithTypeName = "Regolith.Common.RegolithResourceMap";
		private const string RegolithAssemblyName = "Regolith";
		private const string RegolithMethodName = "GetAbundance";

		private static bool ORSXRun = false;

		private static bool RegolithRun = false;

		private delegate double ORSXpixelAbundance(int body, string resourceName, double lat, double lon);

		private delegate float RegolithPosAbundance(double lat, double lon, string resource, int body, int type, double altitude);

		private static ORSXpixelAbundance _ORSXpixelAbundance;

		private static RegolithPosAbundance _RegolithPosAbundance;

		internal static Type _ORSXPlanetType;

		internal static Type _RegolithPlanetType;

		internal static double ORSXpixelAbundanceValue(int body, string resourceName, double lat, double lon)
		{
			return _ORSXpixelAbundance(body, resourceName, lat, lon);
		}

		internal static float RegolithAbundanceValue(double lat, double lon, string resource, int body, int type, double altitude)
		{
			return _RegolithPosAbundance(lat, lon, resource, body, type, altitude);
		}

		internal static bool ORSXReflectionMethod(Assembly ORSXAssembly)
		{
			if (_ORSXpixelAbundance != null)
				return true;

			if (ORSXRun)
				return false;

			ORSXRun = true;

			try
			{
				Type ORSXType = ORSXAssembly.GetExportedTypes()
					.SingleOrDefault(t => t.FullName == ORSXPlanetDataType);

				if (ORSXType == null)
				{
					SCANUtil.SCANlog("ORSX Type Not Found");
					return false;
				}

				_ORSXPlanetType = ORSXType;

				MethodInfo ORSXMethod = ORSXType.GetMethod(ORSXPixelAbundanceMethod, new Type[] { typeof(int), typeof(string), typeof(double), typeof(double) });

				if (ORSXMethod == null)
				{
					SCANUtil.SCANlog("ORSX Method Not Found");
					return false;
				}

				_ORSXpixelAbundance = (ORSXpixelAbundance)Delegate.CreateDelegate(typeof(ORSXpixelAbundance), ORSXMethod);

				SCANUtil.SCANlog("ORSX Reflection Method Assigned");

				return true;
			}
			catch (Exception e)
			{
				Debug.LogWarning("[SCANsat] Exception While Loading ORSX Reflection Method: " + e);
			}

			return false;
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

				_RegolithPlanetType = RegolithType;

				MethodInfo RegolithMethod = RegolithType.GetMethod(RegolithMethodName, new Type[] { typeof(double), typeof(double), typeof(string), typeof(int), typeof(int), typeof(double) });

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

	}
}
