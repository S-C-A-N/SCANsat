

using UnityEngine;

namespace SCANmechjeb
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	class SCANmechStarter : MonoBehaviour
	{
		private MonoBehaviour SCANmechjebInt;

		private void Start()
		{
			print("[SCANsatMechJeb] Checking MechJeb/SCANsat load status");
			if (SCANmechjebMainMenu.Loaded)
				SCANmechjebInt = gameObject.AddComponent<SCANmechjeb>();
		}
	}
}
