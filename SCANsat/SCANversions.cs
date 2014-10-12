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
	internal class SCANversions : MonoBehaviour
	{
		private string[] Assemblies = new string[7] { "SCANsatRPM", "SCANsatKethane", "Kethane", "RasterPropMonitor", "MechJebRPM", "MechJeb2", "ORSX" };
		internal static string SCANsatVersion = "";
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
				AssemblyLoader.LoadedAssembly assembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.assembly.GetName().Name == name);
				if (assembly != null)
					assemblyList.Add(new AssemblyLog(assembly));
			}
			if (assemblyList.Count > 0)
			{
				SCANsatVersion = assemblyList[0].infoVersion;
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

		internal AssemblyLog(AssemblyLoader.LoadedAssembly assembly)
		{
			var ainfoV = Attribute.GetCustomAttribute(assembly.assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
			var afileV = Attribute.GetCustomAttribute(assembly.assembly, typeof(AssemblyFileVersionAttribute)) as AssemblyFileVersionAttribute;

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

			name = assembly.assembly.GetName().Name;
			version = assembly.assembly.GetName().Version.ToString();
			location = assembly.url.ToString();
		}

	}

}
