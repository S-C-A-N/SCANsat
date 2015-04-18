

using UnityEngine;

namespace SCANmechjeb
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	class SCANmechStarter : MonoBehaviour
	{
		private MonoBehaviour SCANmechjebInt;

		private void Start()
		{
			if (SCANmechjebMainMenu.Loaded)
			{
				print("[SCANsatMechJeb] Starting SCANsat - MechJeb Interface...");
				SCANmechjebInt = gameObject.AddComponent<SCANmechjeb>();
			}
		}
	}
}
