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

		private static bool ORSXRun = false;

		private delegate double ORSXpixelAbundance(int body, string resourceName, double lat, double lon);

		private static ORSXpixelAbundance _ORSXpixelAbundance;

		internal static Type _ORSXPlanetType;

		internal static double ORSXpixelAbundanceValue(int body, string resourceName, double lat, double lon)
		{
			return _ORSXpixelAbundance(body, resourceName, lat, lon);
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

	}
}
