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
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SCANsat.Platform.Palettes;
using SCANsat.Platform.Palettes.ColorBrewer;
using palette = SCANsat.SCAN_UI.SCANpalette;

namespace SCANsat
{
	public class SCANdata
	{
		/* MAP: state */
		private Int32[,] coverage = new Int32[360, 180];
		private float[,] heightmap = new float[360, 180];
		private float[,] kethaneValueMap = new float[360, 180]; //Store kethane cell data in here
		private CelestialBody body;
		private Texture2D map_small = new Texture2D(360, 180, TextureFormat.RGB24, false);

		/* MAP: options */
		private float minHeight, maxHeight;
		private float? clampHeight;
		private string paletteName;
		private int paletteSize;
		private bool paletteReverse, paletteDiscrete, disabled;
		private Palette colorPalette;

		/* MAP: default values */
		private static float?[,] bodyHeightRange = new float?[17, 3]
		{
			{ 0, 1000, null }, { -1500, 6500, 0 }, { -500, 7000, null }, { -500, 5500, null },
			{ 0, 6500, null }, { -2000, 7000, 0 }, { 0, 7500, null }, { 0, 12000, null }, { 0, 1000, null },
			{ -3000, 6000, 0 }, { -500, 7500, null }, { 2000, 21500, null }, { -500, 11000, null },
			{ 1500, 6000, null }, { 500, 5500, null }, { 0, 5500, null }, { -500, 3500, null }
		};
		private static Palette[] paletteDefaults = { PaletteLoader.defaultPalette, PaletteLoader.defaultPalette,
			BrewerPalettes.RdGy(11), BrewerPalettes.Paired(9), BrewerPalettes.PuBuGn(6), BrewerPalettes.BuPu(7),
			BrewerPalettes.BuGn(9), BrewerPalettes.BrBG(8), PaletteLoader.defaultPalette, BrewerPalettes.YlGnBu(8),
			BrewerPalettes.Set1(9), BrewerPalettes.PuOr(7), BrewerPalettes.Set3(8), BrewerPalettes.Accent(7),
			BrewerPalettes.Spectral(8), BrewerPalettes.Pastel1(9), BrewerPalettes.RdYlGn(10) };
		private static bool[] paletteReverseDefaults = { false, false, true, false, false, true, false, false,
			false, true, false, false, false, false, true, false, false };
		private const float defaultMinHeight = -1000f;
		private const float defaultMaxHeight = 8000f;
		private float? defaultClampHeight = null;
		private Palette defaultPalette = PaletteLoader.defaultPalette;

		/* MAP: constructor */
		internal SCANdata(CelestialBody b)
		{
			body = b;
			if (b.flightGlobalsIndex <= 16)
			{
				minHeight = (float)bodyHeightRange[b.flightGlobalsIndex, 0];
				maxHeight = (float)bodyHeightRange[b.flightGlobalsIndex, 1];
				clampHeight = bodyHeightRange[b.flightGlobalsIndex, 2];
				colorPalette = paletteDefaults[b.flightGlobalsIndex];
				paletteName = colorPalette.name;
				paletteSize = colorPalette.size;
				paletteReverse = paletteReverseDefaults[b.flightGlobalsIndex];
			}
			else
			{
				colorPalette = defaultPalette;
				paletteName = colorPalette.name;
				paletteSize = colorPalette.size;
				minHeight = defaultMinHeight;
				maxHeight = defaultMaxHeight;
				if (b.ocean)
					clampHeight = 0;
				else
					clampHeight = defaultClampHeight;
			}
		}

		#region Public accessors
		/* Accessors: body-specific variables */
		public Int32[,] Coverage
		{
			get { return coverage; }
			internal set { coverage = value; }
		}

		public float[,] HeightMap
		{
			get { return heightmap; }
			internal set { heightmap = value; }
		}

		public CelestialBody Body
		{
			get { return body; }
		}

		public Texture2D Map
		{
			get { return map_small; }
		}

		public float[,] KethaneValueMap
		{
			get { return kethaneValueMap; }
			set { kethaneValueMap = value; }
		}

		public float MinHeight
		{
			get { return minHeight; }
			internal set
			{
				if (value < maxHeight)
					minHeight = value;
			}
		}

		public float MaxHeight
		{
			get { return maxHeight; }
			internal set
			{
				if (value > minHeight)
					maxHeight = value;
			}
		}

		public float DefaultMinHeight
		{
			get
			{
				if (body.flightGlobalsIndex < 17)
					return (float)bodyHeightRange[body.flightGlobalsIndex, 0];
				else
					return -1000f;
			}
		}

		public float DefaultMaxHeight
		{
			get
			{
				if (body.flightGlobalsIndex < 17)
					return (float)bodyHeightRange[body.flightGlobalsIndex, 1];
				else
					return 8000f;
			}
		}

		public float? ClampHeight
		{
			get { return clampHeight; }
			internal set
			{
				if (value == null)
					clampHeight = null;
				else if (value > minHeight && value < maxHeight)
					clampHeight = value;
			}
		}

		public float? DefaultClampHeight
		{
			get
			{
				if (body.flightGlobalsIndex < 17)
					return bodyHeightRange[body.flightGlobalsIndex, 2];
				else
					return defaultClampHeight;
			}
		}

		public bool PaletteReverse
		{
			get { return paletteReverse; }
			internal set { paletteReverse = value; }
		}

		public bool PaletteDiscrete
		{
			get { return paletteDiscrete; }
			internal set { paletteDiscrete = value; }
		}

		public string PaletteName
		{
			get { return paletteName; }
			internal set { paletteName = value; }
		}

		public Palette ColorPalette
		{
			get { return colorPalette; }
			internal set { colorPalette = value; }
		}

		public Palette DefaultColorPalette
		{
			get
			{
				if (body.flightGlobalsIndex < 17)
					return paletteDefaults[body.flightGlobalsIndex];
				else
					return paletteDefaults[0];
			}
		}

		public bool DefaultReversePalette
		{
			get
			{
				if (body.flightGlobalsIndex < 17)
					return paletteReverseDefaults[body.flightGlobalsIndex];
				else
					return false;
			}
		}

		public int PaletteSize
		{
			get { return paletteSize; }
			internal set
			{
				if (value >= 3)
					paletteSize = value;
			}
		}

		public bool Disabled
		{
			get { return disabled; }
			internal set { disabled = value; }
		}
		#endregion

		#region SCANtype enum
		/* DATA: known types of data */
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
			Ore = 1 << 7,             // Ore - ORS & K-type - EPL & MKS
			Kethane_3 = 1 << 8,       // Reserved - K-type
			Kethane_4 = 1 << 9,       // Reserved - K-type
			Uranium = 1 << 10,        // Uranium - ORS - KSPI
			Thorium = 1 << 11,        // Thorium - ORS - KSPI
			Alumina = 1 << 12,        // Alumina - ORS - KSPI
			Water = 1 << 13,          // Water - ORS - KSPI
			Aquifer = 1 << 14,        // Aquifer - ORS & K-type - MKS
			Minerals = 1 << 15,       // Minerals - ORS & K-type - MKS
			Substrate = 1 << 16,      // Substrate - ORS & K-type - MKS
			KEEZO = 1 << 17,          // KEEZO - ORS - Kass Effect
			Karbonite = 1 << 18,    // Karbonite - ORS
			ORS_10 = 1 << 19,         // Reserved - ORS

			Everything_SCAN = (1 << 6) - 1,	// All default SCANsat scanners
			Everything = Int32.MaxValue      // All scanner types
		}
		#endregion

		#region Resource classes

		public enum SCANResource_Source
		{
			Kethane = 1,
			ORSX = 2,
			Regolith = 3,
		}

		/* DATA: resources */
		public class SCANResource //The new class to store resource information stored in the respective config nodes
		{
			public SCANResource(string n, string Body, Color full, Color empty, bool sc, double scalar, double mult, double threshold, float max, SCANresourceType t, SCANResource_Source s)
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
				source = s;
			}

			private string name;
			internal string body;
			internal double ORS_Scalar, ORS_Multiplier, ORS_Threshold;
			internal Color fullColor, emptyColor;
			internal bool linear;
			internal float maxValue;
			private SCANtype type;
			internal SCANresourceType resourceType;
			private SCANResource_Source source;

			public string Name
			{
				get { return name; }
			}

			public SCANtype Type
			{
				get { return type; }
			}

			public SCANResource_Source Source
			{
				get { return source; }
			}
		}

		public class SCANresourceType
		{
			internal string name;
			internal SCANtype type;
			internal Color colorFull, colorEmpty;

			public SCANresourceType(string s, int i, string Full, string Empty)
			{
				name = s;
				type = (SCANtype)i;
				if ((type & SCANtype.Everything_SCAN) != SCANtype.Nothing)
				{
					Debug.LogWarning("[SCANsat] Attempt To Override Default SCANsat Sensors; Resetting Resource Scanner Type To 0");
					type = SCANtype.Nothing;
				}
				try
				{
					colorFull = ConfigNode.ParseColor(Full);
				}
				catch (Exception e)
				{
					SCANUtil.SCANlog("Color Format Incorrect; Reverting To Default Full Resource Color: {0}", e);
					colorFull = palette.cb_reddishPurple;
				}
				try
				{
					colorEmpty = ConfigNode.ParseColor(Empty);
				}
				catch (Exception e)
				{
					SCANUtil.SCANlog("Color Format Incorrect; Reverting To Default Empty Resource Color: {0}", e);
					colorEmpty = palette.magenta;
				}
			}
		}
		#endregion

		#region Anomalies
		/* DATA: anomalies and such */
		internal class SCANanomaly
		{
			internal SCANanomaly(string s, double lon, double lat, PQSMod m)
			{
				name = s;
				longitude = lon;
				latitude = lat;
				known = false;
				mod = m;
			}

			internal bool known;
			internal bool detail;
			internal string name;
			internal double longitude;
			internal double latitude;
			internal PQSMod mod;
		}

		private SCANanomaly[] anomalies;

		internal SCANanomaly[] Anomalies
		{
			get
			{
				if (anomalies == null)
				{
					PQSCity[] sites = body.GetComponentsInChildren<PQSCity>(true);
					anomalies = new SCANanomaly[sites.Length];
					for (int i = 0; i < sites.Length; ++i)
					{
						anomalies[i] = new SCANanomaly(sites[i].name, body.GetLongitude(sites[i].transform.position), body.GetLatitude(sites[i].transform.position), sites[i]);
					}
				}
				for (int i = 0; i < anomalies.Length; ++i)
				{
					anomalies[i].known = SCANUtil.isCovered(anomalies[i].longitude, anomalies[i].latitude, this, SCANtype.Anomaly);
					anomalies[i].detail = SCANUtil.isCovered(anomalies[i].longitude, anomalies[i].latitude, this, SCANtype.AnomalyDetail);
				}
				return anomalies;
			}
		}
		#endregion

		#region Scanning coverage
		/* DATA: coverage */
		internal int[] coverage_count = new int[32];
		internal void updateCoverage()
		{
			for (int i = 0; i < 32; ++i)
			{
				SCANtype t = (SCANtype)(1 << i);
				int cc = 0;
				for (int x = 0; x < 360; ++x)
				{
					for (int y = 0; y < 180; ++y)
					{
						if ((coverage[x, y] & (Int32)t) == 0)
							++cc;
					}
				}
				coverage_count[i] = cc;
			}
		}
		internal int getCoverage(SCANtype type)
		{
			int uncov = 0;
			if ((type & SCANtype.AltimetryLoRes) != SCANtype.Nothing)
				uncov += coverage_count[0];
			if ((type & SCANtype.AltimetryHiRes) != SCANtype.Nothing)
				uncov += coverage_count[1];
			if ((type & SCANtype.Biome) != SCANtype.Nothing)
				uncov += coverage_count[3];
			if ((type & SCANtype.Anomaly) != SCANtype.Nothing)
				uncov += coverage_count[4];
			if ((type & SCANtype.AnomalyDetail) != SCANtype.Nothing)
				uncov += coverage_count[5];
			if ((type & SCANtype.Kethane) != SCANtype.Nothing)
				uncov += coverage_count[6];
			if ((type & SCANtype.Ore) != SCANtype.Nothing)
				uncov += coverage_count[7];
			if ((type & SCANtype.Kethane_3) != SCANtype.Nothing)
				uncov += coverage_count[8];
			if ((type & SCANtype.Kethane_4) != SCANtype.Nothing)
				uncov += coverage_count[9];
			if ((type & SCANtype.Uranium) != SCANtype.Nothing)
				uncov += coverage_count[10];
			if ((type & SCANtype.Thorium) != SCANtype.Nothing)
				uncov += coverage_count[11];
			if ((type & SCANtype.Alumina) != SCANtype.Nothing)
				uncov += coverage_count[12];
			if ((type & SCANtype.Water) != SCANtype.Nothing)
				uncov += coverage_count[13];
			if ((type & SCANtype.Aquifer) != SCANtype.Nothing)
				uncov += coverage_count[14];
			if ((type & SCANtype.Minerals) != SCANtype.Nothing)
				uncov += coverage_count[15];
			if ((type & SCANtype.Substrate) != SCANtype.Nothing)
				uncov += coverage_count[16];
			if ((type & SCANtype.KEEZO) != SCANtype.Nothing)
				uncov += coverage_count[17];
			if ((type & SCANtype.Karbonite) != SCANtype.Nothing)
				uncov += coverage_count[18];
			if ((type & SCANtype.ORS_10) != SCANtype.Nothing)
				uncov += coverage_count[19];
			return uncov;

		}
		internal double getCoveragePercentage(SCANtype type)
		{
			double cov = 0d;
			if (type == SCANtype.Nothing)
				type = SCANtype.AltimetryLoRes | SCANtype.AltimetryHiRes | SCANtype.Biome | SCANtype.Anomaly;
			cov = getCoverage(type);
			if (cov <= 0)
				cov = 100;
			else
				cov = Math.Min(99.9d, 100 - cov * 100d / (360d * 180d * SCANUtil.countBits((int)type)));
			return cov;
		}
		#endregion

		#region Map Texture
		/* DATA: all hail the red line of scanning */
		private int scanline = 0;
		private int scanstep = 0;
		//Draws the actual map texture
		internal void drawHeightScanline(SCANtype type)
		{
			Color[] cols_height_map_small = map_small.GetPixels(0, scanline, 360, 1);
			for (int ilon = 0; ilon < 360; ilon += 1)
			{
				int scheme = SCANcontroller.controller.colours;
				float val = heightmap[ilon, scanline];
				if (val == 0)
				{ //Some preparation for bigger changes in map caching, automatically calculate elevation for every point on the small map, only display scanned areas
					if (body.pqsController == null)
					{
						heightmap[ilon, scanline] = 0;
						cols_height_map_small[ilon] = palette.lerp(palette.black, palette.white, UnityEngine.Random.value);
						continue;
					}
					else
					{
						// convert to radial vector
						val = (float)SCANUtil.getElevation(body, ilon - 180, scanline - 90);
						if (val == 0)
							val = -0.001f; // this is terrible
						heightmap[ilon, scanline] = val;
					}
				}
				Color c = palette.black;
				if (SCANUtil.isCovered(ilon, scanline, this, SCANtype.Altimetry))
				{ //We check for coverage down here now, after elevation data is collected
					if (SCANUtil.isCovered(ilon, scanline, this, SCANtype.AltimetryHiRes))
						c = palette.heightToColor(val, scheme, this);
					else
						c = palette.heightToColor(val, 1, this);
				}
				else
				{
					c = palette.grey;
					if (scanline % 30 == 0 && ilon % 3 == 0)
					{
						c = palette.white;
					}
					else if (ilon % 30 == 0 && scanline % 3 == 0)
					{
						c = palette.white;
					}
				}
				if (type != SCANtype.Nothing)
				{
					if (!SCANUtil.isCoveredByAll(ilon, scanline, this, type))
					{
						c = palette.lerp(c, palette.black, 0.5f);
					}
				}
				cols_height_map_small[ilon] = c;
			}
			map_small.SetPixels(0, scanline, 360, 1, cols_height_map_small);
			scanline = scanline + 1;
			if (scanline >= 180)
			{
				scanstep += 1;
				scanline = 0;
			}
		}

		//Updates the red scanning line
		internal void updateImages(SCANtype type)
		{
			if (palette.small_redline == null)
			{
				palette.small_redline = new Color[360];
				for (int i = 0; i < 360; i++)
					palette.small_redline[i] = palette.red;
			}
			drawHeightScanline(type);
			if (scanline < 179)
			{
				map_small.SetPixels(0, scanline + 1, 360, 1, palette.small_redline);
			}
			map_small.Apply();
		}
		#endregion

		#region Map Utilities
		/* DATA: debug option to fill in the map */
		internal void fillMap()
		{
			for (int i = 0; i < 360; i++)
			{
				for (int j = 0; j < 180; j++)
				{
					coverage[i, j] |= (Int32)SCANtype.Everything;
				}
			}
		}

		/* DATA: reset the map */
		internal void reset()
		{
			coverage = new Int32[360, 180];
			heightmap = new float[360, 180];
			resetImages();
		}
		internal void resetImages()
		{
			// Just draw a simple grid to initialize the image; the map will appear on top of it
			for (int y = 0; y < map_small.height; y++)
			{
				for (int x = 0; x < map_small.width; x++)
				{
					if ((x % 30 == 0 && y % 3 > 0) || (y % 30 == 0 && x % 3 > 0))
					{
						map_small.SetPixel(x, y, palette.white);
					}
					else
					{
						map_small.SetPixel(x, y, palette.grey);
					}
				}
			}
			map_small.Apply();
		}
		#endregion

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
