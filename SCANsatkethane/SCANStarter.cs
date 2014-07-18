/*
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANStarter - Helper method to check for the presence or abesence of Kethane
 *
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 *
 */

using System.Linq;
using System.Diagnostics;
using UnityEngine;

namespace SCANsatKethane
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class SCANStarter : MonoBehaviour
    {
        private SCANsatKethane SCANK;
        private string Version = "0.8.7";
		private string Version2 = "0.8.8";

        public void Start() {
            print("[SCAN Kethane] Searching For Kethane Assembly...");
            searching();
        }

        private void searching () {
            var KAssembly = AssemblyLoader.loadedAssemblies.SingleOrDefault(a => a.assembly.GetName().Name == "Kethane");
            if (KAssembly != null) {
				if (FileVersionInfo.GetVersionInfo(KAssembly.assembly.Location).ProductVersion == Version || FileVersionInfo.GetVersionInfo(KAssembly.assembly.Location).ProductVersion == Version2)
				{
                print("[SCAN Kethane] Kethane Assembly Found; Version: " + Version + ", Launching Watcher");
                launcher();
                }
                else 
                    print("[SCAN Kethane] Kethane Assembly Found; Unsupported Version: " + Version + ", Shutting Down");
            } else 
                print("[SCAN Kethane] Kethane Assembly Not Found, Shutting Down");
        }
        
        private void launcher () {
            if (SCANK == null) {
                print("[SCAN Kethane] Starting Watcher");
                SCANK = gameObject.AddComponent<SCANsatKethane>();
            }
        }

    }
}
