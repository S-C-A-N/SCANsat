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
		Kethane = 1 << 6,         // Kethane
		MetallicOre = 1 << 7,             // CRP Ore
		Ore = 1 << 8,				//Stock Ore
		Helium3 = 1 << 9,       // Helium 3
		Uraninite = 1 << 10,        // Uranium - CRP
		Thorium = 1 << 11,        // Thorium - CRP
		Alumina = 1 << 12,        // Alumina - CRP
		Water = 1 << 13,          // Water - CRP
		Aquifer = 1 << 14,        // Aquifer - CRP
		Minerals = 1 << 15,       // Minerals - CRP
		Substrate = 1 << 16,      // Substrate - CRP
		MetalOre = 1 << 17,          // Metal Ore - EPL
		Karbonite = 1 << 18,    // Karbonite - CRP
		FuzzyResources = 1 << 19,         // Low Detail Resource
		Hydrates = 1 << 20,		// Hydrates - CRP
		Gypsum = 1 << 21,		// Gypsum - CRP
		ExoticMinerals = 1 << 22, // Exotic Minerals - CRP
		Dirt = 1 << 23,			// Dirt - CRP
		RareMetals = 1 << 24,	// Rare Metals - CRP
		CRP_Reserved = 1 << 25,

		Everything_SCAN = (1 << 6) - 1,	// All default SCANsat scanners
		AllResources = 2147483584,		// All resource types
		DefinedResources = 66584448,		// All defined resource types
		MKSResources = 107648,			// All standard MKS/USI resources
		Everything = Int32.MaxValue      // All scanner types
	}
}
