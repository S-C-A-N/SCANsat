/* 
 * Scientific Committee on Advanced Navigation S.C.A.N. Satellite
 * SCANdata - encapsulates scanned data for a body
 * 
 * Copyright (c)2013 damny; see LICENSE.txt for licensing details.
 */

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SCANdata
{
	protected byte[,] coverage = new byte[360, 180];
	protected float[,] heightmap = new float[360,180];

	public CelestialBody body;
	public int updateSerial;
	public Texture2D map_small = new Texture2D(360, 180, TextureFormat.RGB24, false);

	public enum SCANtype {
		Nothing = 0,			// no data
		AltimetryLoRes = 1,		// low resolution altimetry (limited zoom)
		AltimetryHiRes = 2,		// high resolution altimetry (unlimited zoom)
		Altimetry = 3,			// both (setting) or either (testing) altimetry
		Slope = 4,				// slope data
		Biome = 8,				// biome data
		Anomaly = 16,			// anomalies (position of anomaly)
		AnomalyDetail = 32,		// anomaly detail (name of anomaly, etc.)
		Everything = 255		// everything
	}

	public void registerPass(double lon, double lat, SCANtype type) {
		// fudging coordinates a bit because KSP may return them unclipped
		int ilon = ((int)(lon + 360 + 180)) % 360;
		int ilat = ((int)(lat + 180 + 90)) % 180;
		if(ilon < 0 || ilat < 0 || ilon >= 360 || ilat >= 180) return;
		coverage[ilon, ilat] |= (byte)type;
		updateSerial += 1;
	}

	public bool isCovered(double lon, double lat, SCANtype type) {
		int ilon = ((int)(lon + 360 + 180)) % 360;
		int ilat = ((int)(lat + 180 + 90)) % 180;
		if(ilon < 0 || ilat < 0 || ilon >= 360 || ilat >= 180) return false;
		return (coverage[ilon, ilat] & (byte)type) != 0;
	}

	public bool isCoveredByAll(double lon, double lat, SCANtype type) {
		int ilon = ((int)(lon + 360 + 180)) % 360;
		int ilat = ((int)(lat + 180 + 90)) % 180;
		if(ilon < 0 || ilat < 0 || ilon >= 360 || ilat >= 180) return false;
		return (coverage[ilon, ilat] & (byte)type) == (byte)type;
	}

	public class SCANanomaly {
		public SCANanomaly(string s, double lon, double lat) {
			name = s;
			longitude = lon;
			latitude = lat;
			known = false;
		}
		public bool known;
		public bool detail;
		public string name;
		public double longitude;
		public double latitude;
	}
	SCANanomaly[] anomalies;
	public SCANanomaly[] getAnomalies() {
		if(anomalies == null) {
			PQSCity[] sites = body.GetComponentsInChildren<PQSCity>(true);
			anomalies = new SCANanomaly[sites.Length];
			for(int i=0; i<sites.Length; ++i) {
				anomalies[i] = new SCANanomaly(sites[i].name, body.GetLongitude(sites[i].transform.position), body.GetLatitude(sites[i].transform.position));
			}
		}
		for(int i=0; i<anomalies.Length; ++i) {
			anomalies[i].known = isCovered(anomalies[i].longitude, anomalies[i].latitude, SCANtype.Anomaly);
			anomalies[i].detail = isCovered(anomalies[i].longitude, anomalies[i].latitude, SCANtype.AnomalyDetail);
		}
		return anomalies;
	}

	public string serialize() {
		// convert the byte[,] array into a KSP-savefile-safe variant of Base64
		MemoryStream mem = new MemoryStream();
		BinaryFormatter binf = new BinaryFormatter();
		binf.Serialize(mem, coverage);
		string blob = Convert.ToBase64String(CLZF2.Compress(mem.ToArray()));
		return blob.Replace("/","-").Replace("=","_");
	}

	public void deserialize(string blob) {
		try {
			blob = blob.Replace("-","/").Replace("_","=");
			byte[] bytes = Convert.FromBase64String(blob);
			bytes = CLZF2.Decompress(bytes);
			MemoryStream mem = new MemoryStream(bytes, false);
			BinaryFormatter binf = new BinaryFormatter();
			coverage = (byte[,])binf.Deserialize(mem);
		} catch(Exception e) {
			coverage = new byte[360, 180];
			heightmap = new float[360,180];
			throw e;
		}
		resetImages();
	}

	public void reset() {
		coverage = new byte[360, 180];
		heightmap = new float[360, 180];
		resetImages();
	}

	public void resetImages() {
		// Just draw a simple grid to initialize the image; the map will appear on top of it
		for(int y=0; y<map_small.height; y++) {
			for(int x=0; x<map_small.width; x++) {
				if((x % 30 == 0 && y % 3 > 0) || (y % 30 == 0 && x % 3 > 0)) {
					map_small.SetPixel(x, y, Color.white);
				} else {
					map_small.SetPixel(x, y, Color.grey);
				}
			}
		}
		map_small.Apply();
	}

	protected Color[] redline;
	public void updateImages(SCANtype type) {
		if(redline == null) {
			redline = new Color[360];
			for(int i=0; i<360; i++) redline[i] = Color.red;
		}
		drawHeightScanline(type);
		if(scanline < 179) {
			map_small.SetPixels(0, scanline + 1, 360, 1, redline);
		}
		map_small.Apply();
	}

	public double getElevation(double lon, double lat) {
		if(body.pqsController == null) return 0;
		int ilon = ((int)(lon + 360 + 180)) % 360;
		int ilat = ((int)(lat + 180 + 90)) % 180;
		double rlon = Mathf.Deg2Rad * lon;
		double rlat = Mathf.Deg2Rad * lat;
		Vector3d rad = new Vector3d(Math.Cos(rlat) * Math.Cos(rlon), Math.Sin(rlat), Math.Cos(rlat) * Math.Sin(rlon));
		return Math.Round(body.pqsController.GetSurfaceHeight(rad) - body.pqsController.radius, 1);
	}

	public int getBiomeIndex(double lon, double lat) {
		// It could be so easy, if this function didn't print debug messages to the screen...
		// return body.BiomeMap.GetAtt(Mathf.Deg2Rad * lat, Mathf.Deg2Rad * lon).name;
		if(body.BiomeMap == null) return -1;
		if(body.BiomeMap.Map == null) return -1;
		double u = ((lon + 360 + 180 + 90)) % 360;
		double v = ((lat + 180 + 90)) % 180;
		if(u < 0 || v < 0 || u >= 360 || v >= 180) return -1;
		u /= 360f; v /= 180f;
		Color c = body.BiomeMap.Map.GetPixelBilinear((float)u, (float)v);
		double maxdiff = 12345;
		int index = -1;
		for(int i=0; i<body.BiomeMap.Attributes.Length; ++i) {
			CBAttributeMap.MapAttribute x = body.BiomeMap.Attributes[i];
			Color d = x.mapColor;
			double diff = ((Vector4)d - (Vector4)c).sqrMagnitude;
			if(diff < maxdiff) {
				index = i;
				maxdiff = diff;
			}
		}
		return index;
	}
	
	public double getBiomeIndexFraction(double lon, double lat) {
		if(body.BiomeMap == null) return 0f;
		return getBiomeIndex(lon, lat) * 1.0f / body.BiomeMap.Attributes.Length;
	}

	public CBAttributeMap.MapAttribute getBiome(double lon, double lat) {
		// It could be so easy, if this function didn't print debug messages to the screen...
		// return body.BiomeMap.GetAtt(Mathf.Deg2Rad * lat, Mathf.Deg2Rad * lon);
		if(body.BiomeMap == null) return null;
		if(body.BiomeMap.Map == null) return body.BiomeMap.defaultAttribute;
		int i = getBiomeIndex(lon, lat);
		if(i < 0) return body.BiomeMap.defaultAttribute;
		return body.BiomeMap.Attributes[i];
	}
	
	public string getBiomeName(double lon, double lat) {
		CBAttributeMap.MapAttribute a = getBiome(lon, lat);
		if(a == null) return "unknown";
		return a.name;
	}

	protected int scanline = 0;
	protected int scanstep = 0;
	public void drawHeightScanline(SCANtype type) {
		Color[] cols_height_map_small = map_small.GetPixels(0, scanline, 360, 1);
		for(int ilon=0; ilon<360; ilon+=1) {
			int scheme = 0;
			float val = heightmap[ilon, scanline];
			if(val == 0 && isCovered(ilon-180, scanline-90, SCANtype.Altimetry)) {
				if(body.pqsController == null) {
					heightmap[ilon, scanline] = 0;
					cols_height_map_small[ilon] = Color.Lerp(Color.black, Color.white, UnityEngine.Random.value);
					continue;
				} else {
					// convert to radial vector
					double rlon = Mathf.Deg2Rad * (ilon - 180);
					double rlat = Mathf.Deg2Rad * (scanline - 90);
					Vector3d rad = new Vector3d(Math.Cos(rlat) * Math.Cos(rlon), Math.Sin(rlat), Math.Cos(rlat) * Math.Sin(rlon));
					// query terrain controller for elevation at this point
					val = (float)Math.Round(body.pqsController.GetSurfaceHeight(rad) - body.pqsController.radius, 1);
					if(val == 0) val = -0.001f; // this is terrible
					heightmap[ilon, scanline] = val;
				}
			}
			Color c = Color.black;
			if(val != 0) {
				if(isCovered(ilon - 180, scanline - 90, SCANtype.AltimetryHiRes)) c = SCANmap.heightToColor(val, scheme);
				else c = SCANmap.heightToColor(val, 1);
			} else {
				c = Color.grey;
				if(scanline % 30 == 0 && ilon % 3 == 0) {
					c = Color.white;
				} else if(ilon % 30 == 0 && scanline % 3 == 0) {
					c = Color.white;
				}
			}
			if(type != SCANtype.Nothing) {
				if(!isCoveredByAll(ilon - 180, scanline - 90, type)) {
					c = Color.Lerp(c, Color.black, 0.5f);
				}
			}
			cols_height_map_small[ilon] = c;
		}
		map_small.SetPixels(0, scanline, 360, 1, cols_height_map_small);
		scanline = scanline + 1;
		if(scanline >= 180) {
			scanstep += 1;
			scanline = 0;
		}
	}


}
