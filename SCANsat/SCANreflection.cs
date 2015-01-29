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

		private const string RegolithTypeName = "Regolith.Common.RegolithResourceMap";
		private const string RegolithAssemblyName = "Regolith";
		private const string RegolithMethodName = "GetAbundance";

		private static bool RegolithRun = false;

		private delegate float RegolithPosAbundance(double lat, double lon, string resource, int body, int type, int altitude, bool biomes);

		private static RegolithPosAbundance _RegolithPosAbundance;

		internal static float RegolithAbundanceValue(double lat, double lon, string resource, int body, int type, int altitude, bool biomes)
		{
			return _RegolithPosAbundance(lat, lon, resource, body, type, altitude, biomes);
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

	}
}
