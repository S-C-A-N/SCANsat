using System;
using UnityEngine;

public class SCANmap {
	private static Color[] heightGradient = {XKCDColors.ArmyGreen, XKCDColors.Yellow, XKCDColors.Red, XKCDColors.Magenta, XKCDColors.White, XKCDColors.White};
	public static Color heightToColor(float val, int scheme) {
		if(scheme == 1 || SCANcontroller.controller.colours == 1) {
			return Color.Lerp(Color.black, Color.white, Mathf.Clamp(val+1500f, 0, 9000)/9000f);
		}
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

	public float mapscale, lon_offset, lat_offset;
	public int mapwidth, mapheight;
	public int mapmode = 0;

	protected int mapstep;
	protected bool mapsaved;
	protected float[] mapline;
	protected CelestialBody body;

	public Texture2D map;

	public void setBody(CelestialBody b) {
		if(body == b) return;
		body = b;
		resetMap();
	}

	public void setSize(int w, int h) {
		if(w == 0) w = 360 * (Screen.width / 360);
		if(w > 360 * 4) w = 360 * 4;
		mapwidth = w;
		mapscale = mapwidth / 360f;
		if(h <= 0) h = (int)(180 * mapscale);
		mapheight = h;
		if(map != null) {
			if(mapwidth != map.width || mapheight != map.height) map = null;
		}
	}

	public void centerAround(float lon, float lat) {
		lon_offset = 180 + lon - (mapwidth / mapscale) / 2;
		lat_offset = 90 + lat - (mapheight / mapscale) / 2;
	}

	public Texture2D getPartialMap() {
		SCANdata data = SCANcontroller.controller.getData(body);
		Color[] pix;
		if(map == null) {
			map = new Texture2D(mapwidth, mapheight, TextureFormat.RGB24, false);
			pix = map.GetPixels();
			for(int i=0; i<pix.Length; ++i) pix[i] = Color.grey;
			map.SetPixels(pix);
		} else if(mapstep >= map.height) {
			if(!mapsaved) {
				// if we just finished rendering a map, save it to our PluginData folder
				string mode = "unknown";
				if(mapmode == 0) mode = "elevation";
				else if(mapmode == 1) mode = "slope";
				else if(mapmode == 2) mode = "biome";
				if(SCANcontroller.controller.colours == 1) mode += "-grey";
				string filename = body.name + "_" + mode + "_" + map.width.ToString() + "x" + map.height.ToString() + ".png";
				KSP.IO.File.WriteAllBytes<SCANdata>(map.EncodeToPNG(), filename, null);
				mapsaved = true;
			}
			return map;
		}
		if(mapstep <= 0) {
			mapstep = 0;
			mapline = new float[map.width];
		}
		pix = map.GetPixels(0, mapstep, map.width, 1);
		float lat = (mapstep * 1.0f / mapscale) - 90f + lat_offset;
		for(int i=0; i<map.width; i++) {
			int scheme = 0;
			float lon = (i * 1.0f / mapscale) - 180f + lon_offset;
			pix[i] = Color.grey;
			if(mapmode == 0) {
				if(!data.isCovered(lon, lat, SCANdata.SCANtype.Altimetry)) continue;
				if(body.pqsController == null) {
					pix[i] = Color.Lerp(Color.black, Color.white, UnityEngine.Random.value);
					continue;
				}
				float val;
				if(data.isCovered(lon, lat, SCANdata.SCANtype.AltimetryHiRes)) {
					// high resolution gets a coloured pixel for the actual position
					val = data.getElevation(lon, lat);
					pix[i] = heightToColor(val, scheme);
				} else {
					// basic altimetry gets forced greyscale with lower resolution
					val = data.getElevation(((int)(lon*5))/5, ((int)(lat*5))/5);
					pix[i] = heightToColor(val, 1);
				}
				/* draw height lines - works, but mostly useless...
				int step = (int)(val / 1000);
				int step_h = step, step_v = step;
				if(i > 0) step_h = (int)(bigline[i - 1] / 1000);
				if(bigstep > 0) step_v = (int)(bigline[i] / 1000);
				if(step != step_h || step != step_v) {
					pix[i] = Color.white;
				}
				*/
				mapline[i] = val;
			} else if(mapmode == 1) {
				if(!data.isCovered(lon, lat, SCANdata.SCANtype.Slope)) continue;
				if(body.pqsController == null) {
					pix[i] = Color.Lerp(Color.black, Color.white, UnityEngine.Random.value);
					continue;
				}
				float val = data.getElevation(lon, lat);
				if(mapstep == 0) {
					pix[i] = Color.grey;
				} else {
					// This doesn't actually calculate the slope per se, but it's faster
					// than asking for yet more elevation data. Please don't use this
					// code to operate nuclear power plants or rockets.
					float v1 = mapline[i];
					if(i > 0) v1 = Math.Max(v1, mapline[i - 1]);
					if(i < mapline.Length - 1) v1 = Math.Max(v1, mapline[i + 1]);
					float v = Mathf.Clamp(Math.Abs(val - v1) / 1000f, 0, 2f);
					if(SCANcontroller.controller.colours == 1) {
						pix[i] = Color.Lerp(Color.black, Color.white, v / 2f);
					} else {
						if(v < 1) {
							pix[i] = Color.Lerp(XKCDColors.PukeGreen, XKCDColors.Lemon, v);
						} else {
							pix[i] = Color.Lerp(XKCDColors.Lemon, XKCDColors.OrangeRed, v-1);
						}
					}
				}
				mapline[i] = val;
			} else if(mapmode == 2) {
				if(!data.isCovered(lon, lat, SCANdata.SCANtype.Biome)) continue;
				if(body.BiomeMap == null || body.BiomeMap.Map == null) {
					pix[i] = Color.Lerp(Color.black, Color.white, UnityEngine.Random.value);
					continue;
				}
				/* // this just basically stretches the actual biome map to fit... it looks horrible
				float u = ((lon + 360 + 180 + 90)) % 360;
				float v = ((lat + 180 + 90)) % 180;
				if(u < 0 || v < 0 || u >= 360 || v >= 180) continue;
				u /= 360f; v /= 180f;
				pix[i] = body.BiomeMap.Map.GetPixelBilinear(u, v);
				*/
				float bio = data.getBiomeIndexFraction(lon, lat);
				Color biome = Color.grey;
				if(SCANcontroller.controller.colours == 1) {
					if((i > 0 && mapline[i - 1] != bio) || (mapstep > 0 && mapline[i] != bio)) {
						biome = Color.white;
					} else {
						biome = Color.Lerp(Color.black, Color.white, bio);
					}
				} else {
					Color elevation = Color.gray;
					if(data.isCovered(lon, lat, SCANdata.SCANtype.Altimetry)) {
						float val = data.getElevation(lon, lat);
						elevation = Color.Lerp(Color.black, Color.white, Mathf.Clamp(val+1500f, 0, 9000)/9000f);
					}
					Color bio1 = XKCDColors.CamoGreen;
					Color bio2 = XKCDColors.Marigold;
					if((i > 0 && mapline[i - 1] != bio) || (mapstep > 0 && mapline[i] != bio)) {
						biome = Color.Lerp(XKCDColors.Puce, elevation, 0.5f);
					} else {
						biome = Color.Lerp(Color.Lerp(bio1, bio2, bio), elevation, 0.5f);
					}
				}

				pix[i] = biome;
				mapline[i] = bio;
			}
		}
		map.SetPixels(0, mapstep, map.width, 1, pix);
		map.Apply();
		mapstep++;
		return map;
	}

	public bool isMapComplete() {
		if(map == null) return false;
		return mapstep >= map.height;
	}

	public void resetMap() {
		mapstep = 0;
		mapsaved = false;
	}

	public void resetMap(int mode) {
		mapmode = mode;
		resetMap();
	}
}
