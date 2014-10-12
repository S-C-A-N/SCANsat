#region license
/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 * 
 * SCANdata - encapsulates scanned data for a body
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using palette = SCANsat.SCANpalette;

namespace SCANsat
{
	public class SCANdata
	{
		/* MAP: state */
		public Int32[,] coverage = new Int32[360 , 180];
		internal float[,] heightmap = new float[360 , 180];
		public float[,] kethaneValueMap = new float[360, 180]; //Store kethane cell data in here
		public CelestialBody body;
		public Texture2D map_small = new Texture2D (360 , 180 , TextureFormat.RGB24 , false);
		public bool disabled;

		internal SCANdata(CelestialBody b)
		{
			body = b;
		}

		/* MAP: known types of data */
		public enum SCANtype: int
		{
			Nothing = 0, 		    // no data (MapTraq)
			AltimetryLoRes = 1<<0,  // low resolution altimetry (limited zoom)
			AltimetryHiRes = 1<<1,  // high resolution altimetry (unlimited zoom)
			Altimetry = (1<<2)-1, 	        // both (setting) or either (testing) altimetry
			SCANsat_1 = 1<<2,		// Unused, reserved for future SCANsat scanner
			Biome = 1<<3,		    // biome data
			Anomaly = 1<<4,		    // anomalies (position of anomaly)
			AnomalyDetail = 1<<5,	// anomaly detail (name of anomaly, etc.)
            Kethane = 1<<6,         // Kethane - K-type - Kethane
            Ore = 1<<7,             // Ore - ORS & K-type - EPL & MKS
            Kethane_3 = 1<<8,       // Reserved - K-type
            Kethane_4 = 1<<9,       // Reserved - K-type
            Uranium = 1<<10,        // Uranium - ORS - KSPI
            Thorium = 1<<11,        // Thorium - ORS - KSPI
            Alumina = 1<<12,        // Alumina - ORS - KSPI
            Water = 1<<13,          // Water - ORS - KSPI
            Aquifer = 1<<14,        // Aquifer - ORS & K-type - MKS
            Minerals = 1<<15,       // Minerals - ORS & K-type - MKS
            Substrate = 1<<16,      // Substrate - ORS & K-type - MKS
            KEEZO = 1<<17,          // KEEZO - ORS - Kass Effect
			Karbonite = 1 << 18,    // Karbonite - ORS
            ORS_10 = 1<<19,         // Reserved - ORS

			Everything_SCAN = (1<<6)-1,	// All default SCANsat scanners
            Everything = Int32.MaxValue      // All scanner types
		}

		/* DATA: resources */
        public class SCANResource //The new class to store resource information stored in the respective config nodes
        {
            public SCANResource (string n, string Body, Color full, Color empty, bool sc, double scalar, double mult, double threshold, float max, SCANresourceType t)
            {
                name = n;
				body = Body;
                fullColor = full;
                emptyColor = empty;
                linear = sc;
                ORS_Scalar = scalar;
                ORS_Multiplier = mult;
                ORS_Threshold = threshold;
                maxValue = max;
				resourceType = t;
				type = resourceType.type;
            }

            public string name, body;
            public double ORS_Scalar, ORS_Multiplier, ORS_Threshold;
            public Color fullColor, emptyColor;
            public bool linear;
            public float maxValue;
            public SCANtype type;
			public SCANresourceType resourceType;
        }

		public class SCANresourceType
		{
			public string name;
			public SCANtype type;
			public Color colorFull, colorEmpty;

			internal SCANresourceType (string s, int i, string Full, string Empty)
			{
				name = s;
				type = (SCANtype)i;
				if ((type & SCANtype.Everything_SCAN) != SCANtype.Nothing)
				{
					Debug.LogWarning("[SCANsat] Attempt To Override Default SCANsat Sensors; Resetting Resource Scanner Type To 0");
					type = SCANtype.Nothing;
				}
				colorFull = ConfigNode.ParseColor(Full);
				colorEmpty = ConfigNode.ParseColor(Empty);
			}
		}

		/* DATA: coverage */
        public int[] coverage_count = new int[32];
		public void updateCoverage () {
            for (int i=0; i<32; ++i) {
                SCANtype t = (SCANtype)(1 << i);
				int cc = 0;
				for (int x=0; x<360; ++x) {
					for (int y=0; y<180; ++y) {
						if ((coverage [x, y] & (Int32)t) == 0)
							++cc;
					}
				}
                coverage_count [i] = cc;
            }
		}
		public int getCoverage ( SCANtype type ) {
			int uncov = 0;
			if ((type & SCANtype.AltimetryLoRes) != SCANtype.Nothing)
                uncov += coverage_count[0];
            if ((type & SCANtype.AltimetryHiRes) != SCANtype.Nothing)
                uncov += coverage_count [1];
            if ((type & SCANtype.Biome) != SCANtype.Nothing)
                uncov += coverage_count [3];
            if ((type & SCANtype.Anomaly) != SCANtype.Nothing)
                uncov += coverage_count [4];
            if ((type & SCANtype.AnomalyDetail) != SCANtype.Nothing)
                uncov += coverage_count [5];
            if ((type & SCANtype.Kethane) != SCANtype.Nothing)
                uncov += coverage_count [6];
            if ((type & SCANtype.Ore) != SCANtype.Nothing)
                uncov += coverage_count [7];
            if ((type & SCANtype.Kethane_3) != SCANtype.Nothing)
                uncov += coverage_count [8];
            if ((type & SCANtype.Kethane_4) != SCANtype.Nothing)
                uncov += coverage_count [9];
            if ((type & SCANtype.Uranium) != SCANtype.Nothing)
                uncov += coverage_count [10];
            if ((type & SCANtype.Thorium) != SCANtype.Nothing)
                uncov += coverage_count [11];
            if ((type & SCANtype.Alumina) != SCANtype.Nothing)
                uncov += coverage_count [12];
            if ((type & SCANtype.Water) != SCANtype.Nothing)
                uncov += coverage_count [13];
            if ((type & SCANtype.Aquifer) != SCANtype.Nothing)
                uncov += coverage_count [14];
            if ((type & SCANtype.Minerals) != SCANtype.Nothing)
                uncov += coverage_count [15];
            if ((type & SCANtype.Substrate) != SCANtype.Nothing)
                uncov += coverage_count [16];
            if ((type & SCANtype.KEEZO) != SCANtype.Nothing)
                uncov += coverage_count [17];
            if ((type & SCANtype.Karbonite) != SCANtype.Nothing)
                uncov += coverage_count [18];
            if ((type & SCANtype.ORS_10) != SCANtype.Nothing)
                uncov += coverage_count [19];
			return uncov;

		}
		public double getCoveragePercentage ( SCANtype type ) {
			double cov = 0d;
            if (type == SCANtype.Nothing) 
                type = SCANtype.AltimetryLoRes | SCANtype.AltimetryHiRes | SCANtype.Biome | SCANtype.Anomaly;          
            cov = getCoverage (type);
            if (cov <= 0)
			    cov = 100;
			else
				cov = Math.Min (99.9d , 100 - cov * 100d / (360d * 180d * SCANUtil.countBits ((int)type)));
			return cov;
		}

		/* DATA: all hail the red line of scanning */

		protected int scanline = 0;
		protected int scanstep = 0;
		public void drawHeightScanline ( SCANtype type ) {
			Color[] cols_height_map_small = map_small.GetPixels (0 , scanline , 360 , 1);
			for (int ilon=0; ilon<360; ilon+=1) {
				int scheme = 0;
				float val = heightmap [ilon, scanline];
                if (val == 0) { //Some preparation for bigger changes in map caching, automatically calculate elevation for every point on the small map, only display scanned areas
					if (body.pqsController == null) {
						heightmap [ilon, scanline] = 0;
						cols_height_map_small [ilon] = palette.lerp (palette.black , palette.white , UnityEngine.Random.value);
						continue;
					} else {
						// convert to radial vector
						val = (float)SCANUtil.getElevation(body, ilon - 180, scanline - 90);
						//double rlon = Mathf.Deg2Rad * (ilon - 180);
						//double rlat = Mathf.Deg2Rad * (scanline - 90);
						//Vector3d rad = new Vector3d (Math.Cos (rlat) * Math.Cos (rlon) , Math.Sin (rlat) , Math.Cos (rlat) * Math.Sin (rlon));
						//// query terrain controller for elevation at this point
						//val = (float)Math.Round (body.pqsController.GetSurfaceHeight (rad) - body.pqsController.radius , 1);
						if (val == 0)
							val = -0.001f; // this is terrible
						heightmap [ilon, scanline] = val;
					}
				}
				Color c = palette.black;
				if (SCANUtil.isCovered(ilon, scanline, this, SCANtype.Altimetry))
				{ //We check for coverage down here now, after elevation data is collected
					if (SCANUtil.isCovered(ilon, scanline, this, SCANtype.AltimetryHiRes))
						c = palette.heightToColor (val , scheme);
					else
						c = palette.heightToColor (val , 1);
				} else {
					c = palette.grey;
					if (scanline % 30 == 0 && ilon % 3 == 0) {
						c = palette.white;
					} else if (ilon % 30 == 0 && scanline % 3 == 0) {
						c = palette.white;
					}
				}
				if (type != SCANtype.Nothing) {
					if (!SCANUtil.isCoveredByAll(ilon, scanline, this, type))
					{
						c = palette.lerp (c , palette.black , 0.5f);
					}
				}
				cols_height_map_small [ilon] = c;
			}
			map_small.SetPixels (0 , scanline , 360 , 1 , cols_height_map_small);
			scanline = scanline + 1;
			if (scanline >= 180) {
				scanstep += 1;
				scanline = 0;
			}
		}
		public void updateImages ( SCANtype type ) {
			if (palette.small_redline == null) {
				palette.small_redline = new Color[360];
				for (int i=0; i<360; i++)
					palette.small_redline [i] = palette.red;
			}
			drawHeightScanline (type);
			if (scanline < 179) {
				map_small.SetPixels (0 , scanline + 1 , 360 , 1 , palette.small_redline);
			}
			map_small.Apply ();
		}

		/* DATA: anomalies and such */
		public class SCANanomaly
		{
			public SCANanomaly ( string s , double lon , double lat , PQSMod m )
			{
				name = s;
				longitude = lon;
				latitude = lat;
				known = false;
				mod = m;
			}

			public bool known;
			public bool detail;
			public string name;
			public double longitude;
			public double latitude;
			public PQSMod mod;
		}
		SCANanomaly[] anomalies;
		public SCANanomaly[] getAnomalies () {
			if (anomalies == null) {
				PQSCity[] sites = body.GetComponentsInChildren<PQSCity> (true);
				anomalies = new SCANanomaly[sites.Length];
				for (int i=0; i<sites.Length; ++i) {
					anomalies [i] = new SCANanomaly (sites [i].name , body.GetLongitude (sites [i].transform.position) , body.GetLatitude (sites [i].transform.position) , sites [i]);
				}
			}
			for (int i=0; i<anomalies.Length; ++i) {
				anomalies[i].known = SCANUtil.isCovered(anomalies[i].longitude, anomalies[i].latitude, this, SCANtype.Anomaly);
				anomalies[i].detail = SCANUtil.isCovered(anomalies[i].longitude, anomalies[i].latitude, this, SCANtype.AnomalyDetail);
			}
			return anomalies;
		}

        public void fillMap () {
            for (int i = 0; i < 360; i++) {
                for (int j = 0; j < 180; j++) { 
                    coverage[i,j] |= (Int32)SCANtype.Everything;
                }
            }
        }

		/* DATA: reset the map */
		public void reset () {
			coverage = new Int32[360 , 180];
			heightmap = new float[360 , 180];
			resetImages ();
		}
		public void resetImages () {
			// Just draw a simple grid to initialize the image; the map will appear on top of it
			for (int y=0; y<map_small.height; y++) {
				for (int x=0; x<map_small.width; x++) {
					if ((x % 30 == 0 && y % 3 > 0) || (y % 30 == 0 && x % 3 > 0)) {
						map_small.SetPixel (x , y , palette.white);
					} else {
						map_small.SetPixel (x , y , palette.grey);
					}
				}
			}
			map_small.Apply ();
		}


		#region Unused code

		/* MAP: anonymous functions (in place of preprocessor macros */
		// icLON and icLAT: [i]nteger casted, [c]lamped, longitude and latitude
		//internal Func<double,int> icLON = (lon) => ((int)(lon + 360 + 180)) % 360;
		//internal Func<double,int> icLAT = (lat) => ((int)(lat + 180 + 90 )) % 180;
		//Func<int,int,bool> badLonLat = (lon,lat) => (lon < 0 || lat < 0 || lon >= 360 || lat >= 180);

		//****Commented code moved to SCANUtil****
		/* DATA: map passes and coverage (passes >= 1)*/
		//public void registerPass ( double lon , double lat , SCANtype type ) {
		//    int ilon = SCANUtil.icLON(lon);
		//    int ilat = SCANUtil.icLAT(lat);
		//    if (SCANUtil.badLonLat(ilon, ilat)) return;
		//    coverage [ilon, ilat] |= (Int32)type;
		//}
		//public bool isCovered ( double lon , double lat , SCANtype type ) {
		//    int ilon = SCANUtil.icLON(lon);
		//    int ilat = SCANUtil.icLAT(lat);
		//    if (SCANUtil.badLonLat(ilon, ilat)) return false;
		//    return (coverage [ilon, ilat] & (Int32)type) != 0;
		//}
		//public bool isCoveredByAll(double lon, double lat, SCANtype type)
		//{
		//    int ilon = SCANUtil.icLON(lon);
		//    int ilat = SCANUtil.icLAT(lat);
		//    if (SCANUtil.badLonLat(ilon, ilat)) return false;
		//    return (coverage[ilon, ilat] & (Int32)type) == (Int32)type;
		//}

		/* DATA: elevation (called often, probably) */
		//public double getElevation ( double lon , double lat ) {
		//    if (body.pqsController == null) return 0;	// FIXME: something == null does not imply sealevel.
		//    /* FIXME: unused */ //int ilon = ((int)(lon + 360 + 180)) % 360;
		//    /* FIXME: unused */ //int ilat = ((int)(lat + 180 + 90)) % 180;
		//    double rlon = Mathf.Deg2Rad * lon;
		//    double rlat = Mathf.Deg2Rad * lat;
		//    Vector3d rad = new Vector3d (Math.Cos (rlat) * Math.Cos (rlon) , Math.Sin (rlat) , Math.Cos (rlat) * Math.Sin (rlon));
		//    return Math.Round (body.pqsController.GetSurfaceHeight (rad) - body.pqsController.radius , 1);
		//}

		/* DATA: Biomes and such */
		//public int getBiomeIndex ( double lon , double lat ) {
		//    if (body.BiomeMap == null)		return -1;
		//    if (body.BiomeMap.Map == null)	return -1;

		//    double u = ((lon + 360 + 180 + 90)) % 360;	// not casted to int, so not the same
		//    double v = ((lat + 180 + 90)) % 180;		// not casted to int, so not the same

		//    if (u < 0 || v < 0 || u >= 360 || v >= 180)
		//        return -1;
		//    CBAttributeMap.MapAttribute att = body.BiomeMap.GetAtt (Mathf.Deg2Rad * lat , Mathf.Deg2Rad * lon);
		//    for (int i = 0; i < body.BiomeMap.Attributes.Length; ++i) {
		//        if (body.BiomeMap.Attributes [i] == att) {
		//            return i;
		//        }
		//    }
		//    return -1;
		//}
		//public double getBiomeIndexFraction ( double lon , double lat ) {
		//    if (body.BiomeMap == null) return 0f;
		//    return SCANUtil.getBiomeIndex(body, lon, lat) * 1.0f / body.BiomeMap.Attributes.Length;
		//}
		//public CBAttributeMap.MapAttribute getBiome ( double lon , double lat ) {
		//    if (body.BiomeMap == null)		return null;
		//    if (body.BiomeMap.Map == null)	return body.BiomeMap.defaultAttribute;
		//    int i = SCANUtil.getBiomeIndex(body, lon, lat);
		//    if (i < 0)					return body.BiomeMap.defaultAttribute;
		//    else 						return body.BiomeMap.Attributes [i];
		//}
		//public string getBiomeName ( double lon , double lat ) {
		//    CBAttributeMap.MapAttribute a = getBiome (lon , lat);
		//    if (a == null)
		//        return "unknown";
		//    return a.name;
		//}

		//public double ORSOverlay(double lon, double lat, int i, string s) //Uses ORS methods to grab the resource amount given a lat and long
		//{
		//    double amount = 0f;
		//    ORSPlanetaryResourcePixel overlayPixel = ORSPlanetaryResourceMapData.getResourceAvailability(i, s, lat, lon);
		//    amount = overlayPixel.getAmount();            
		//    return amount;
		//}

		/* DATA: Array conversion **** Moved to SCANUtil*/
		////Take the Int32[] coverage and convert it to a single dimension byte array
		//private byte[] ConvertToByte (Int32[,] iArray) {
		//    byte[] bArray = new byte[360 * 180 * 4];
		//    int k = 0;
		//    for (int i = 0; i < 360; i++) {
		//        for (int j = 0; j < 180; j++) {
		//            byte[] bytes = BitConverter.GetBytes(iArray[i,j]);
		//            for (int m = 0; m < bytes.Length; m++) {
		//                bArray[k++] = bytes[m];
		//            }
		//        }
		//    }
		//    return bArray;
		//}

		////Convert byte array from persistent file to usable Int32[]
		//private Int32[,] ConvertToInt (byte[] bArray) {
		//    Int32[,] iArray = new Int32[360, 180];
		//    int k = 0;
		//    for (int i = 0; i < 360; i++) {
		//        for (int j = 0; j < 180; j++) {
		//            iArray[i,j] = BitConverter.ToInt32(bArray, k);
		//            k += 4;
		//        }
		//    }
		//    return iArray;
		//}

		////One time conversion of single byte[,] to Int32 to recover old scanning data
		//private Int32[,] RecoverToInt (byte[,] bArray) {
		//    Int32[,] iArray = new Int32[360, 180];
		//    for (int i = 0; i < 360; i++) {
		//        for (int j = 0; j < 180; j++) {
		//            iArray[i,j] = (Int32)bArray[i,j];
		//        }
		//    }
		//    return iArray;
		//}

		/* DATA: legacy serialization and compression */
		//internal string integerSerialize () {
		//    byte[] bytes = ConvertToByte(coverage);
		//    MemoryStream mem = new MemoryStream ();
		//    BinaryFormatter binf = new BinaryFormatter ();
		//    binf.Serialize (mem , bytes);
		//    string blob = Convert.ToBase64String (CLZF2.Compress (mem.ToArray ()));
		//    return blob.Replace ("/" , "-").Replace ("=" , "_");
		//}

		//public void integerDeserialize ( string blob, bool b ) {
		//    try {
		//        blob = blob.Replace ("-" , "/").Replace ("_" , "=");
		//        byte[] bytes = Convert.FromBase64String (blob);
		//        bytes = CLZF2.Decompress (bytes);
		//        MemoryStream mem = new MemoryStream (bytes , false);
		//        BinaryFormatter binf = new BinaryFormatter ();
		//        if (b) {
		//            byte[,] bRecover = new byte[360, 180];
		//            bRecover = (byte[,])binf.Deserialize (mem);
		//            coverage = RecoverToInt(bRecover);
		//        }
		//        else {
		//            byte[] bArray = (byte[])binf.Deserialize (mem);
		//            coverage = ConvertToInt(bArray);
		//        }
		//    } catch (Exception e) {
		//        coverage = new Int32[360 , 180];
		//        heightmap = new float[360 , 180];
		//        throw e;
		//    }
		//    resetImages ();
		//}

		///* DATA: serialization and compression */
		//public string serialize () {
		//    // convert the byte[,] array into a KSP-savefile-safe variant of Base64
		//    MemoryStream mem = new MemoryStream ();
		//    BinaryFormatter binf = new BinaryFormatter ();
		//    binf.Serialize (mem , backupCoverage);
		//    string blob = Convert.ToBase64String (CLZF2.Compress (mem.ToArray ()));
		//    return blob.Replace ("/" , "-").Replace ("=" , "_");
		//}

		//public void deserialize ( string blob ) {
		//    try {
		//        blob = blob.Replace ("-" , "/").Replace ("_" , "=");
		//        byte[] bytes = Convert.FromBase64String (blob);
		//        bytes = CLZF2.Decompress (bytes);
		//        MemoryStream mem = new MemoryStream (bytes , false);
		//        BinaryFormatter binf = new BinaryFormatter ();
		//        backupCoverage = (byte[,])binf.Deserialize (mem);
		//    } catch (Exception e) {
		//        backupCoverage = new byte[360 , 180];
		//        throw e;
		//    }
		//}

		#endregion
	}
}
