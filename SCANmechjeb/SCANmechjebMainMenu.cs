

using System;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace SCANmechjeb
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class SCANmechjebMainMenu : MonoBehaviour
	{
		private const string SCANsatName = "SCANsat";
		private const string MechJeb = "MechJeb2";
		private readonly Version MechJebVersion = new Version(2, 4, 2, 0);
		private static bool loaded = false;

		private void Awake()
		{
			loaded = checkLoaded();

			print(loaded ? "[SCANsatMechJeb] SCANsat and MechJeb Assemblies Detected" : "[SCANsatMechJeb] SCANsat or MechJeb Assembly Not Detected; Shutting Down...");
		}

		public static bool Loaded
		{
			get { return loaded; }
		}

		private bool checkLoaded()
		{
			var SCANassembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.assembly.GetName().Name == SCANsatName);

			if (SCANassembly == null)
				return false;

			var infoV = Attribute.GetCustomAttribute(SCANassembly.assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;

			if (infoV == null)
				return false;

			var SCANmechjebAssembly = Assembly.GetExecutingAssembly();

			if (SCANmechjebAssembly == null)
				return false;

			var SMinfoV = Attribute.GetCustomAttribute(SCANmechjebAssembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;

			if (SMinfoV == null)
				return false;

			if (infoV.InformationalVersion != SMinfoV.InformationalVersion)
				return false;

			var MechJebAssembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.assembly.GetName().Name == MechJeb);

			if (MechJebAssembly == null)
				return false;

			var fileV = Attribute.GetCustomAttribute(MechJebAssembly.assembly, typeof(AssemblyFileVersionAttribute)) as AssemblyFileVersionAttribute;

			if (fileV == null)
				return false;

			if (fileV.Version == MechJebVersion.ToString())
			{
				SCANsat.SCANmainMenuLoader.MechJebLoaded = true;
				return true;
			}

			return false;
		}
	}
}
