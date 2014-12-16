#region license
/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANversions - logs version numbers for SCANsat and various associated mods
 * 
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace SCANsat
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class SCANversions : MonoBehaviour
	{
		private const string kVersion = "0.9.2";
		private string[] Assemblies = new string[9] { "SCANsatRPM", "SCANsatKethane", "Kethane", "GeodesicGrid", "RasterPropMonitor", "MechJebRPM", "MechJeb2", "ORSX", "Regolith" };

		internal static string SCANsatVersion = "";
		public static bool kethaneLoaded = false;
		internal static bool ORSXFound = false;
		internal static bool RegolithFound = false;

		private List<AssemblyLog> assemblyList = new List<AssemblyLog>();

		private void Start()
		{
			findAssemblies(Assemblies);
		}

		private void findAssemblies(string[] assemblies)
		{
			assemblyList.Add(new AssemblyLog(AssemblyLoader.loadedAssemblies.GetByAssembly(Assembly.GetExecutingAssembly()))); //More reliable method for SCANsat.dll
			foreach (string name in assemblies)
			{ //Search for the relevant plugins among the loaded assemblies
				var assembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.assembly.GetName().Name == name);
				if (assembly != null)
					assemblyList.Add(new AssemblyLog(assembly));
			}
			if (assemblyList.Count > 0)
			{
				SCANsatVersion = assemblyList[0].infoVersion;
				var kAssembly = assemblyList.FirstOrDefault(a => a.name == "Kethane");
				var scankAssembly = assemblyList.FirstOrDefault(a => a.name == "SCANsatKethane");
				if (kAssembly != null && scankAssembly != null)
				{
					if (kAssembly.infoVersion == kVersion)
						kethaneLoaded = true;
				}
				//var ORSXAssembly = assemblyList.FirstOrDefault(a => a.name == "ORSX");
				//if (ORSXAssembly != null)
				//{
				//	ORSXFound = SCANreflection.ORSXReflectionMethod(ORSXAssembly.assemblyLoaded);
				//}
				var RegolithAssembly = assemblyList.FirstOrDefault(a => a.name == "Regolith");
				if (RegolithAssembly != null)
				{
					RegolithFound = SCANreflection.RegolithReflectionMethod(RegolithAssembly.assemblyLoaded);
				}

				debugWriter();
			}
		}

		private void debugWriter()
		{
			foreach (AssemblyLog log in assemblyList)
			{
				print(string.Format("[SCANlogger] Assembly: {0} found; Version: {1}; File Version: {2}; Info Version: {3}; Location: {4}", log.name, log.version, log.fileVersion, log.infoVersion, log.location));
			}
		}

	}

	//A class to gather and store information about assemblies
	internal class AssemblyLog
	{
		internal string name, version, fileVersion, infoVersion, location;
		internal Assembly assemblyLoaded;

		internal AssemblyLog(AssemblyLoader.LoadedAssembly Assembly)
		{
			assemblyLoaded = Assembly.assembly;
			var ainfoV = Attribute.GetCustomAttribute(Assembly.assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
			var afileV = Attribute.GetCustomAttribute(Assembly.assembly, typeof(AssemblyFileVersionAttribute)) as AssemblyFileVersionAttribute;

			switch (afileV == null)
			{
				case true: fileVersion = ""; break;
				default: fileVersion = afileV.Version; break;
			}

			switch (ainfoV == null)
			{
				case true: infoVersion = ""; break;
				default: infoVersion = ainfoV.InformationalVersion; break;
			}

			name = Assembly.assembly.GetName().Name;
			version = Assembly.assembly.GetName().Version.ToString();
			location = Assembly.url.ToString();
		}

	}

}
