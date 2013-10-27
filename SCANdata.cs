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
	protected float[,] heightmap = new float[360,180];
	public int updateSerial;
	public Texture2D height_map_small = new Texture2D(360, 180, TextureFormat.RGB24, false);
	public Texture2D height_map_big;
	protected int bigscale = Screen.width/360;

	public void registerPass(CelestialBody body, float lon, float lat) {
		// fudging coordinates a bit because KSP may return them unclipped
		int ilon = ((int)(lon + 360 + 180)) % 360;
		int ilat = ((int)(lat + 180 + 90)) % 180;
		if(ilon < 0 || ilat < 0 || ilon >= 360 || ilat >= 180) return;
		if(heightmap[ilon, ilat] != 0) return;
		// convert to radial vector
		float rlon = Mathf.Deg2Rad * (ilon - 180);
		float rlat = Mathf.Deg2Rad * (ilat - 90);
		Vector3d rad = new Vector3d(Math.Cos(rlat) * Math.Cos(rlon), Math.Sin(rlat), Math.Cos(rlat) * Math.Sin(rlon));
		// query terrain controller for elevation at this point
		float val = (float)Math.Round(body.pqsController.GetSurfaceHeight(rad) - body.pqsController.radius, 1);
		if(val == 0) val = -0.001f; // this is terrible
		heightmap[ilon, ilat] = val;
		updateSerial += 1;
	}

	public string serialize() {
		// convert the float[,] array into a KSP-savefile-safe variant of Base64
		MemoryStream mem = new MemoryStream();
		BinaryFormatter binf = new BinaryFormatter();
		binf.Serialize(mem, heightmap);
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
			heightmap = (float[,])binf.Deserialize(mem);
		} catch(Exception e) {
			heightmap = new float[360,180];
			throw e;
		}
		resetImages();
	}

	public void reset() {
		heightmap = new float[360, 180];
		resetImages();
	}

	public void resetImages() {
		// Just draw a simple grid to initialize the image; the map will appear on top of it
		for(int y=0; y<height_map_small.height; y++) {
			for(int x=0; x<height_map_small.width; x++) {
				if((x % 30 == 0 && y % 3 > 0) || (y % 30 == 0 && x % 3 > 0)) {
					height_map_small.SetPixel(x, y, Color.white);
				} else {
					height_map_small.SetPixel(x, y, Color.grey);
				}
			}
		}
		height_map_small.Apply();
	}

	protected Color[] redline;
	public void updateImages() {
		if(redline == null) {
			redline = new Color[360];
			for(int i=0; i<360; i++) redline[i] = Color.red;
		}
		drawHeightScanline();
		if(scanline < 179) {
			height_map_small.SetPixels(0, scanline + 1, 360, 1, redline);
		}
		height_map_small.Apply();
	}

	protected Color[] heightGradient = {XKCDColors.ArmyGreen, XKCDColors.Yellow, XKCDColors.Red, XKCDColors.Magenta, XKCDColors.White, XKCDColors.White};
	public Color heightToColor(float val) {
		Color c = Color.black;
		if(val <= 0) {
			val = (Mathf.Clamp(val, -1500, 0) + 1500) / 1000f;
			c = Color.Lerp(XKCDColors.DarkPurple, XKCDColors.Cerulean, val);
		} else {
			val = (heightGradient.Length-2) * Mathf.Clamp(val, 0, 7500) / 7500.0f;
			c = Color.Lerp(heightGradient[(int)val], heightGradient[(int)val + 1], val - (int)val);
		}
		return c;
	}

	protected int scanline = 0;
	protected int scanstep = 0;
	public void drawHeightScanline() {
		Color[] cols_height_map_small = height_map_small.GetPixels(0, scanline, 360, 1);
		for(int ilon=0; ilon<360; ilon+=1) {
			float val = heightmap[ilon, scanline];
			Color c = Color.black;
			if(val != 0) {
				c = heightToColor(val);
			} else {
				c = Color.grey;
				if(scanline % 30 == 0 && ilon % 3 == 0) {
					c = Color.white;
				} else if(ilon % 30 == 0 && scanline % 3 == 0) {
					c = Color.white;
				}
			}
			if(c != Color.black) {
				cols_height_map_small[ilon] = c;
			}
		}
		height_map_small.SetPixels(0, scanline, 360, 1, cols_height_map_small);
		scanline = scanline + 1;
		if(scanline >= 180) {
			scanstep += 1;
			scanline = 0;
		}
	}

	protected int bigstep;
	protected bool bigsaved;
	protected float[] bigline;
	public Texture2D getPartialBigMap(CelestialBody body) {
		Color[] pix;
		if(bigscale > 5) bigscale = 5;
		if(bigscale < 1) bigscale = 1;
		if(height_map_big == null) {
			height_map_big = new Texture2D(360 * bigscale, 180 * bigscale, TextureFormat.RGB24, false);
			pix = height_map_big.GetPixels();
			for(int i=0; i<pix.Length; ++i) pix[i] = Color.grey;
			height_map_big.SetPixels(pix);
		} else if(bigstep >= height_map_big.height) {
			if(!bigsaved) {
				// if we just finished rendering a map, save it to our PluginData folder
				string filename = body.name + "_elevation_" + height_map_big.width.ToString() + "x" + height_map_big.height.ToString() + ".png";
				KSP.IO.File.WriteAllBytes<SCANdata>(height_map_big.EncodeToPNG(), filename, null);
				bigsaved = true;
			}
			return height_map_big;
		}
		if(bigstep <= 0) {
			bigstep = 0;
			bigline = new float[height_map_big.width];
		}
		pix = height_map_big.GetPixels(0, bigstep, height_map_big.width, 1);
		float lat = (bigstep * 1.0f / bigscale) - 90f;
		for(int i=0; i<height_map_big.width; i++) {
			float lon = (i * 1.0f / bigscale) - 180f;
			int ilon = ((int)(lon + 360 + 180)) % 360;
			int ilat = ((int)(lat + 180 + 90)) % 180;
			if(heightmap[ilon, ilat] == 0) continue;
			float rlon = Mathf.Deg2Rad * lon;
			float rlat = Mathf.Deg2Rad * lat;
			Vector3d rad = new Vector3d(Math.Cos(rlat) * Math.Cos(rlon), Math.Sin(rlat), Math.Cos(rlat) * Math.Sin(rlon));
			float val = (float)Math.Round(body.pqsController.GetSurfaceHeight(rad) - body.pqsController.radius, 1);
			pix[i] = heightToColor(val);
			/* draw height lines - works, but mostly useless...
			int step = (int)(val / 1000);
			int step_h = step, step_v = step;
			if(i > 0) step_h = (int)(bigline[i - 1] / 1000);
			if(bigstep > 0) step_v = (int)(bigline[i] / 1000);
			if(step != step_h || step != step_v) {
				pix[i] = Color.white;
			}
			*/
			bigline[i] = val;
		}
		height_map_big.SetPixels(0, bigstep, height_map_big.width, 1, pix);
		height_map_big.Apply();
		bigstep++;
		return height_map_big;
	}

	public void resetBigMap() {
		bigstep = 0;
		bigsaved = false;
	}
}
