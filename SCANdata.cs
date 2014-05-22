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
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using OpenResourceSystem;

namespace SCANsat
{
	public class SCANdata
	{
		/* MAP: anonymous functions (in place of preprocessor macros */
		// icLON and icLAT: [i]nteger casted, [c]lamped, longitude and latitude
		internal Func<double,int> icLON = (lon) => ((int)(lon + 360 + 180)) % 360;
		internal Func<double,int> icLAT = (lat) => ((int)(lat + 180 + 90 )) % 180;
		Func<int,int,bool> badLonLat = (lon,lat) => (lon < 0 || lat < 0 || lon >= 360 || lat >= 180);

		/* MAP: state */
		internal byte[,] coverage = new byte[360 , 180];
        public byte[,] resourceCoverage = new byte[360, 180]; //Secondary coverage map for resources
		protected float[,] heightmap = new float[360 , 180];
        public float[,] kethanemap = new float[360, 180]; //Store kethane cell data in here
		public CelestialBody body;
		public Texture2D map_small = new Texture2D (360 , 180 , TextureFormat.RGB24 , false);
		public bool disabled;

		/* MAP: known types of data */
		public enum SCANtype
		{
			Nothing = 0, 		// no data
			AltimetryLoRes = 1, // low resolution altimetry (limited zoom)
			AltimetryHiRes = 2, // high resolution altimetry (unlimited zoom)
			Altimetry = 3, 	// both (setting) or either (testing) altimetry
			Kethane = 4,		// Generic Kethane resource sensor
			Biome = 8,		// biome data
			Anomaly = 16,		// anomalies (position of anomaly)
			AnomalyDetail = 32,	// anomaly detail (name of anomaly, etc.)
            ORS = 64,           // Generic ORS scanner
			Everything = 255	// everything
		}

        public enum SCANResourceType //Additional enum needed here to store the many possible resource types
        {
            Nothing = 0,
            Kethane_1 = 1, //Kethane
            Kethane_2 = 2, //Ore - EPL, MKS
            Kethane_3 = 3, //Minerals - MKS
            Kethane_4 = 4, //Water - MKS
            Kethane_5 = 5, //Substrate - MKS
            Kethane_6 = 6,
            Kethane_7 = 7,
            Kethane_8 = 8,
            Kethane_9 = 9,
            Kethane_10 = 10,
            ORS_1 = 21, //Uranium - KSPI
            ORS_2 = 22, //Thorium - KSPI
            ORS_3 = 23, //Alumina - KSPI
            ORS_4 = 24, //Water - KSPI, MKS
            ORS_5 = 25, //Ore - MKS
            ORS_6 = 26, //Minerals - MKS
            ORS_7 = 27, //Substrate - MKS
            ORS_8 = 28,
            ORS_9 = 29,
            ORS_10 = 30
        }

		/* DATA: map passes and coverage (passes >= 1)*/
		public void registerPass ( double lon , double lat , SCANtype type ) {
			int ilon = icLON(lon);
			int ilat = icLAT(lat);
			if (badLonLat(ilon,ilat)) return;
			coverage [ilon, ilat] |= (byte)type;
		}
		public bool isCovered ( double lon , double lat , SCANtype type ) {
			int ilon = icLON(lon);
			int ilat = icLAT(lat);
			if (badLonLat(ilon,ilat)) return false;
			return (coverage [ilon, ilat] & (byte)type) != 0;
		}
		public bool isCoveredByAll ( double lon , double lat , SCANtype type ) {
			int ilon = icLON(lon);
			int ilat = icLAT(lat);
			if (badLonLat(ilon,ilat)) return false;
			return (coverage [ilon, ilat] & (byte)type) == (byte)type;
		}
        public void registerResourcePass (double lon, double lat, SCANResourceType type) { //A few additional methods to handle resource coverage
			int ilon = icLON(lon);
			int ilat = icLAT(lat);
			if (badLonLat(ilon,ilat)) return;
			resourceCoverage [ilon, ilat] |= (byte)type;
        }
        public bool isCoveredResource (double lon, double lat, SCANResourceType type) {
            int ilon = icLON(lon);
            int ilat = icLAT(lat);
            if (badLonLat(ilon, ilat)) return false;
            return (resourceCoverage[ilon, ilat] & (byte)type) != 0;
        }

		/* DATA: elevation (called often, probably) */
		public double getElevation ( double lon , double lat ) {
			if (body.pqsController == null) return 0;	// FIXME: something == null does not imply sealevel.
			/* FIXME: unused */ //int ilon = ((int)(lon + 360 + 180)) % 360;
			/* FIXME: unused */ //int ilat = ((int)(lat + 180 + 90)) % 180;
			double rlon = Mathf.Deg2Rad * lon;
			double rlat = Mathf.Deg2Rad * lat;
			Vector3d rad = new Vector3d (Math.Cos (rlat) * Math.Cos (rlon) , Math.Sin (rlat) , Math.Cos (rlat) * Math.Sin (rlon));
			return Math.Round (body.pqsController.GetSurfaceHeight (rad) - body.pqsController.radius , 1);
		}

		/* DATA: Biomes and such */
		public int getBiomeIndex ( double lon , double lat ) {
			if (body.BiomeMap == null)		return -1;
			if (body.BiomeMap.Map == null)	return -1;

			double u = ((lon + 360 + 180 + 90)) % 360;	// not casted to int, so not the same
			double v = ((lat + 180 + 90)) % 180;		// not casted to int, so not the same

			if (u < 0 || v < 0 || u >= 360 || v >= 180)
				return -1;
			CBAttributeMap.MapAttribute att = body.BiomeMap.GetAtt (Mathf.Deg2Rad * lat , Mathf.Deg2Rad * lon);
			for (int i = 0; i < body.BiomeMap.Attributes.Length; ++i) {
				if (body.BiomeMap.Attributes [i] == att) {
					return i;
				}
			}
			return -1;
		}
		public double getBiomeIndexFraction ( double lon , double lat ) {
			if (body.BiomeMap == null) return 0f;
			return getBiomeIndex (lon , lat) * 1.0f / body.BiomeMap.Attributes.Length;
		}
		public CBAttributeMap.MapAttribute getBiome ( double lon , double lat ) {
			if (body.BiomeMap == null)		return null;
			if (body.BiomeMap.Map == null)	return body.BiomeMap.defaultAttribute;
			int i = getBiomeIndex (lon , lat);
			if (i < 0)					return body.BiomeMap.defaultAttribute;
			else 						return body.BiomeMap.Attributes [i];
		}
		public string getBiomeName ( double lon , double lat ) {
			CBAttributeMap.MapAttribute a = getBiome (lon , lat);
			if (a == null)
				return "unknown";
			return a.name;
		}

        /* DATA: resources */ //May as well put this in with the other data generating methods instead of in SCANmap
        public double ORSOverlay(double lon, double lat, int i, string s) //Uses ORS methods to grab the resource amount given a lat and long
        {
            double amount = 0f;
            ORSPlanetaryResourcePixel overlayPixel = ORSPlanetaryResourceMapData.getResourceAvailability(i, s, lat, lon);
            amount = overlayPixel.getAmount() * 1000000; //values in ppm
            return amount;
        }

        public void KethaneOverlay() //Needs to be filled in
        {
        }

		/* DATA: coverage */
		public int[] coverage_count = new int[8];
		public void updateCoverage () {
			for (int i=0; i<6; ++i) {
				SCANtype t = (SCANtype)(1 << i);
				int cc = 0;
				for (int x=0; x<360; ++x) {
					for (int y=0; y<180; ++y) {
						if ((coverage [x, y] & (byte)t) == 0)
							++cc;
					}
				}
				coverage_count [i] = cc;
			}
		}
		public int getCoverage ( SCANtype type ) {
			int uncov = 0;
			if ((type & SCANtype.AltimetryLoRes) != SCANtype.Nothing)
				uncov += coverage_count [0];
			if ((type & SCANtype.AltimetryHiRes) != SCANtype.Nothing)
				uncov += coverage_count [1];
            if ((type & SCANtype.Kethane) != SCANtype.Nothing)
                uncov += coverage_count [2];
			if ((type & SCANtype.Biome) != SCANtype.Nothing)
				uncov += coverage_count [3];
			if ((type & SCANtype.Anomaly) != SCANtype.Nothing)
				uncov += coverage_count [4];
			if ((type & SCANtype.AnomalyDetail) != SCANtype.Nothing)
				uncov += coverage_count [5];
            if ((type & SCANtype.ORS) != SCANtype.Nothing)
                uncov += coverage_count [6];
			return uncov;

		}
		public double getCoveragePercentage ( SCANtype type ) {
			if (type == SCANtype.Nothing) {
				type = SCANtype.AltimetryLoRes | SCANtype.AltimetryHiRes | SCANtype.Biome | SCANtype.Anomaly;
			}
			double cov = getCoverage (type);
			if (cov <= 0) {
				cov = 100;
			} else {
				cov = Math.Min (99.9d , 100 - cov * 100d / (360d * 180d * SCANcontroller.countBits ((int)type)));
			}
			return cov;
		}

		/* DATA: all hail the red line of scanning */
		protected Color[] redline;
		protected int scanline = 0;
		protected int scanstep = 0;
		public void drawHeightScanline ( SCANtype type ) {
			Color[] cols_height_map_small = map_small.GetPixels (0 , scanline , 360 , 1);
			for (int ilon=0; ilon<360; ilon+=1) {
				int scheme = 0;
				float val = heightmap [ilon, scanline];
				if (val == 0 && isCovered (ilon - 180 , scanline - 90 , SCANtype.Altimetry)) {
					if (body.pqsController == null) {
						heightmap [ilon, scanline] = 0;
						cols_height_map_small [ilon] = Color.Lerp (Color.black , Color.white , UnityEngine.Random.value);
						continue;
					} else {
						// convert to radial vector
						double rlon = Mathf.Deg2Rad * (ilon - 180);
						double rlat = Mathf.Deg2Rad * (scanline - 90);
						Vector3d rad = new Vector3d (Math.Cos (rlat) * Math.Cos (rlon) , Math.Sin (rlat) , Math.Cos (rlat) * Math.Sin (rlon));
						// query terrain controller for elevation at this point
						val = (float)Math.Round (body.pqsController.GetSurfaceHeight (rad) - body.pqsController.radius , 1);
						if (val == 0)
							val = -0.001f; // this is terrible
						heightmap [ilon, scanline] = val;
					}
				}
				Color c = Color.black;
				if (val != 0) {
					if (isCovered (ilon - 180 , scanline - 90 , SCANtype.AltimetryHiRes))
						c = SCANmap.heightToColor (val , scheme);
					else
						c = SCANmap.heightToColor (val , 1);
				} else {
					c = Color.grey;
					if (scanline % 30 == 0 && ilon % 3 == 0) {
						c = Color.white;
					} else if (ilon % 30 == 0 && scanline % 3 == 0) {
						c = Color.white;
					}
				}
				if (type != SCANtype.Nothing) {
					if (!isCoveredByAll (ilon - 180 , scanline - 90 , type)) {
						c = Color.Lerp (c , Color.black , 0.5f);
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
			if (redline == null) {
				redline = new Color[360];
				for (int i=0; i<360; i++)
					redline [i] = Color.red;
			}
			drawHeightScanline (type);
			if (scanline < 179) {
				map_small.SetPixels (0 , scanline + 1 , 360 , 1 , redline);
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
				anomalies [i].known = isCovered (anomalies [i].longitude , anomalies [i].latitude , SCANtype.Anomaly);
				anomalies [i].detail = isCovered (anomalies [i].longitude , anomalies [i].latitude , SCANtype.AnomalyDetail);
			}
			return anomalies;
		}

		/* DATA: serialization and compression */
		public string serialize (int i) {
			// convert the byte[,] array into a KSP-savefile-safe variant of Base64
			MemoryStream mem = new MemoryStream ();
			BinaryFormatter binf = new BinaryFormatter ();
			if (i == 0) binf.Serialize (mem , coverage); //Modified to handle saving both the regural coverage map and the resource coverage map, same for deserialize
            else if (i == 1) binf.Serialize (mem , resourceCoverage); 
			string blob = Convert.ToBase64String (CLZF2.Compress (mem.ToArray ()));
			return blob.Replace ("/" , "-").Replace ("=" , "_");
		}
		public void deserialize ( string blob, int i ) {
			try {
				blob = blob.Replace ("-" , "/").Replace ("_" , "=");
				byte[] bytes = Convert.FromBase64String (blob);
				bytes = CLZF2.Decompress (bytes);
				MemoryStream mem = new MemoryStream (bytes , false);
				BinaryFormatter binf = new BinaryFormatter ();
				if (i == 0) coverage = (byte[,])binf.Deserialize (mem);
                else if (i == 1) resourceCoverage = (byte[,])binf.Deserialize (mem);
			} catch (Exception e) {
				if (i ==0) {
                    coverage = new byte[360 , 180];
                    heightmap = new float[360 , 180];
                }
				else if (i ==1) resourceCoverage = new byte[360, 180];
				throw e;
			}
			resetImages ();
		}

		/* DATA: reset the map */
		public void reset () {
			coverage = new byte[360 , 180];
			heightmap = new float[360 , 180];
			resetImages ();
		}
        public void resetResource () {
            resourceCoverage = new byte[360, 180];
        }
		public void resetImages () {
			// Just draw a simple grid to initialize the image; the map will appear on top of it
			for (int y=0; y<map_small.height; y++) {
				for (int x=0; x<map_small.width; x++) {
					if ((x % 30 == 0 && y % 3 > 0) || (y % 30 == 0 && x % 3 > 0)) {
						map_small.SetPixel (x , y , Color.white);
					} else {
						map_small.SetPixel (x , y , Color.grey);
					}
				}
			}
			map_small.Apply ();
		}
	}
}
