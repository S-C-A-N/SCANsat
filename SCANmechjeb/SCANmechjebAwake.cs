

using System;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace SCANmechjeb
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class SCANmechjebAwake : MonoBehaviour
	{
		private const string SCANsat = "SCANsat";
		private const string SCANsatVersion = "v11rc3";
		private MonoBehaviour SCANsatMechjebInterface;
		private bool loaded = false;

		private void Awake()
		{
			loaded = checkLoaded();

			if (loaded)
				launchSCANmechjebInterface();
		}

		private bool checkLoaded()
		{
			var assembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.assembly.GetName().Name == SCANsat);

			if (assembly == null)
				return false;

			var infoV = Attribute.GetCustomAttribute(assembly.assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;

			if (infoV == null)
				return false;

			if (infoV.InformationalVersion == SCANsatVersion)
				return true;

			return false;
		}

		private void launchSCANmechjebInterface()
		{
			SCANsatMechjebInterface = gameObject.AddComponent<SCANmechjeb>();
			DontDestroyOnLoad(this);
		}

	}
}
