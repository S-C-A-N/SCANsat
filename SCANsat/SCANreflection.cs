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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SCANsat
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	class SCANreflection : MonoBehaviour
	{
		internal static bool ORSXFound;

		private const string ORSXPlanetDataType = "ORSX.ORSX_PlanetaryResourceMapData";
		private const string ORSXPixelAbundanceMethod = "getPixelAbundanceValue";
		private const string ORSXAssemblyName = "ORSX";
		private const string ORSXVersion = "1.0.0.0";

		private static bool ORSXRun = false;

		private delegate double ORSXpixelAbundance(int body, string resourceName, double lat, double lon);

		private static AssemblyLoader.LoadedAssembly ORSXAssembly;

		private static ORSXpixelAbundance _ORSXpixelAbundance;

		internal static Type _ORSXPlanetType;

		internal static double ORSXpixelAbundanceValue(int body, string resourceName, double lat, double lon)
		{
			return _ORSXpixelAbundance(body, resourceName, lat, lon);
		}


		private void Start()
		{
			ORSXFound = ORSXReflectionMethod();
		}

		private static bool ORSXAssemblyLoaded()
		{
			if (ORSXAssembly != null)
				return true;

			AssemblyLoader.LoadedAssembly assembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.assembly.GetName().Name == ORSXAssemblyName);
			if (assembly != null)
			{
				if (assembly.assembly.GetName().Version.ToString() == ORSXVersion)
				{
					SCANUtil.SCANlog("ORSX Assembly Loaded");
					ORSXAssembly = assembly;
					return true;
				}
				else
				{
					Debug.LogWarning(string.Format("[SCANsat] Incompatible ORSX Assembly Detected: Version [{0}]; Delete All Copies of ORSX Other Than {1}", assembly.assembly.GetName().Version, ORSXAssemblyName));
					return false;
				}
			}

			SCANUtil.SCANlog("ORSX Assembly Not Found");
			return false;
		}

		private static bool ORSXReflectionMethod()
		{
			if (ORSXAssemblyLoaded() == false)
				return false;

			if (_ORSXpixelAbundance != null)
				return true;

			if (ORSXRun)
				return false;

			ORSXRun = true;

			try
			{
				Type ORSXType = ORSXAssembly.assembly.GetExportedTypes()
					.SingleOrDefault(t => t.FullName == ORSXPlanetDataType);

				if (ORSXType == null)
				{
					SCANUtil.SCANlog("ORSX Type Not Found");
					return false;
				}

				_ORSXPlanetType = ORSXType;

				MethodInfo ORSMethod = ORSXType.GetMethod(ORSXPixelAbundanceMethod, new Type[] { typeof(int), typeof(string), typeof(double), typeof(double) });

				if (ORSMethod == null)
				{
					SCANUtil.SCANlog("ORSX Method Not Found");
					return false;
				}

				_ORSXpixelAbundance = (ORSXpixelAbundance)Delegate.CreateDelegate(typeof(ORSXpixelAbundance), ORSMethod);

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
