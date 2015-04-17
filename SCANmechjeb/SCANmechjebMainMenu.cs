

using System;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace SCANmechjeb
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class SCANmechjebMainMenu : MonoBehaviour
	{
		private const string SCANsat = "SCANsat";
		private const string MechJeb = "MechJeb2";
		private const string SCANsatVersion = "v11rc3";
		private readonly Version MechJebVersion = new Version(2, 4, 2, 0);
		private static bool loaded = false;

		private void Awake()
		{
			print("[SCANsatMechJeb] Checking For Assemblies");
			loaded = checkLoaded();
		}

		public static bool Loaded
		{
			get { return loaded; }
		}

		private bool checkLoaded()
		{
			var SCANassembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.assembly.GetName().Name == SCANsat);

			if (SCANassembly == null)
				return false;

			var infoV = Attribute.GetCustomAttribute(SCANassembly.assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;

			if (infoV == null)
				return false;

			if (infoV.InformationalVersion != SCANsatVersion)
				return false;

			print("[SCANsatMechJeb] SCANsat Assembly Loaded");

			var MechJebAssembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.assembly.GetName().Name == MechJeb);

			if (MechJebAssembly == null)
				return false;

			var fileV = Attribute.GetCustomAttribute(MechJebAssembly.assembly, typeof(AssemblyFileVersionAttribute)) as AssemblyFileVersionAttribute;

			if (fileV == null)
				return false;

			if (fileV.Version == MechJebVersion.ToString())
			{
				print("[SCANsatMechJeb] MechJeb Assembly Loaded");
				return true;
			}

			return false;
		}
	}
}
