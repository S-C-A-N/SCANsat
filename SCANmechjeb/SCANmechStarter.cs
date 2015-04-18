

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
				SCANmechjebInt = gameObject.AddComponent<SCANmechjeb>();
		}
	}
}
