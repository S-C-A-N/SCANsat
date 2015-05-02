#region license
/*
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 * 
 * SCANtype - Enum for SCANsat scanner types
 *
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
*/
#endregion

using System;

namespace SCANsat.SCAN_Data
{
	public enum SCANtype : int
	{
		Nothing = 0, 		    // no data (MapTraq)
		AltimetryLoRes = 1 << 0,  // low resolution altimetry (limited zoom)
		AltimetryHiRes = 1 << 1,  // high resolution altimetry (unlimited zoom)
		Altimetry = (1 << 2) - 1, 	        // both (setting) or either (testing) altimetry
		SCANsat_1 = 1 << 2,		// Unused, reserved for future SCANsat scanner
		Biome = 1 << 3,		    // biome data
		Anomaly = 1 << 4,		    // anomalies (position of anomaly)
		AnomalyDetail = 1 << 5,	// anomaly detail (name of anomaly, etc.)
		Kethane = 1 << 6,         // Kethane - K-type - Kethane
		MetallicOre = 1 << 7,             // CRP Ore - Regolith
		Ore = 1 << 8,				//Stock Ore
		Kethane_4 = 1 << 9,       // Reserved - K-type
		Uraninite = 1 << 10,        // Uranium - Regolith - KSPI
		Thorium = 1 << 11,        // Thorium - Regolith - KSPI
		Alumina = 1 << 12,        // Alumina - Regolith - KSPI
		Water = 1 << 13,          // Water - Regolith - KSPI
		Aquifer = 1 << 14,        // Aquifer - Regolith & K-type - MKS
		Minerals = 1 << 15,       // Minerals - Regolith & K-type - MKS
		Substrate = 1 << 16,      // Substrate - Regolith & K-type - MKS
		KEEZO = 1 << 17,          // KEEZO - Regolith - Kass Effect
		Karbonite = 1 << 18,    // Karbonite - Regolith
		Regolith_10 = 1 << 19,         // Reserved - Regolith
		Regolith_11 = 1<< 20,		// Reserved - Regolith

		Everything_SCAN = (1 << 6) - 1,	// All default SCANsat scanners
		AllResources = 2147483584,		// All resource types
		DefinedResources = 392576,		// All defined resource types
		Everything = Int32.MaxValue      // All scanner types
	}
}
