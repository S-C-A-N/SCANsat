/*
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 * 
 * SCANmap - makes maps from data
 *
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
*/

using System;
using UnityEngine;

namespace SCANsat
{
	public class SCANmap
	{
		private static Color[] heightGradient = {
			XKCDColors.ArmyGreen,
			XKCDColors.Yellow,
			XKCDColors.Red,
			XKCDColors.Magenta,
			XKCDColors.White,
			XKCDColors.White
		};

		public static Color heightToColor ( float val , int scheme ) {
			if (scheme == 1 || SCANcontroller.controller.colours == 1) {
				return Color.Lerp (Color.black , Color.white , Mathf.Clamp ((val + 1500f) / 9000f , 0 , 1));
			}
			Color c = Color.black;
			int sealevel = 0;

			if (val <= sealevel) {
				val = (Mathf.Clamp (val , -1500 , sealevel) + 1500) / 1000f;
				c = Color.Lerp (XKCDColors.DarkPurple , XKCDColors.Cerulean , val);
			} else {
				val = (heightGradient.Length - 2) * Mathf.Clamp (val , sealevel , (sealevel + 7500)) / (sealevel + 7500.0f);
				c = Color.Lerp (heightGradient [(int)val] , heightGradient [(int)val + 1] , val - (int)val);
			}
			return c;
		}

		/* MAP: legends */

		public static Texture2D legend;
		private static float legendMin, legendMax;
		private static int legendScheme;
		public static Texture2D getLegend ( float min , float max , int scheme ) {
			if (legend != null && legendMin == min && legendMax == max && legendScheme == scheme)
				return legend;
			legend = new Texture2D (256 , 1 , TextureFormat.RGB24 , false);
			legendMin = min;
			legendMax = max;
			legendScheme = scheme;
			Color[] pix = legend.GetPixels ();
			for (int x=0; x<256; ++x) {
				float val = (x * (max - min)) / 256f + min;
				pix [x] = heightToColor (val , scheme);
			}
			legend.SetPixels (pix);
			legend.Apply ();
			return legend;
		}

		/* MAP: projections */
		public enum MapProjection
		{
			Rectangular = 0,
			KavrayskiyVII = 1,
			Polar = 2,
		}
		private static string [] getProjectionNames () {
			MapProjection[] v = (MapProjection[])Enum.GetValues (typeof(MapProjection));
			string[] r = new string[v.Length];
			for (int i=0; i<v.Length; ++i)
				r [i] = v [i].ToString ();
			return r;
		}
		public static string[] projectionNames = getProjectionNames ();
		public MapProjection projection = MapProjection.Rectangular;

		protected float[,,] big_heightmap;
        protected int mapType;
        

        	public void heightMapArray (float height, int line, int i, int type)
        	{
                if (type == 0)
                {
            		big_heightmap[i, line, SCANcontroller.controller.projection] = height;
                }
        	}


		public void setProjection ( MapProjection p ) {
			if (projection == p)
				return;
			projection = p;
			resetMap ();
		}
		public double projectLongitude ( double lon , double lat ) {
			lon = (lon + 3600 + 180) % 360 - 180;
			lat = (lat + 1800 + 90) % 180 - 90;
			switch (projection) {
				case MapProjection.KavrayskiyVII:
					lon = Mathf.Deg2Rad * lon;
					lat = Mathf.Deg2Rad * lat;
					lon = (3.0f * lon / 2.0f / Math.PI) * Math.Sqrt (Math.PI * Math.PI / 3.0f - lat * lat);
					return Mathf.Rad2Deg * lon;
				case MapProjection.Polar:
					lon = Mathf.Deg2Rad * lon;
					lat = Mathf.Deg2Rad * lat;
					if (lat < 0) {
						lon = 1.3 * Math.Cos (lat) * Math.Sin (lon) - Math.PI / 2;
				} else {
						lon = 1.3 * Math.Cos (lat) * Math.Sin (lon) + Math.PI / 2;
				}
					return Mathf.Rad2Deg * lon;
				default:
					return lon;
			}
		}
		public double projectLatitude ( double lon , double lat ) {
			lon = (lon + 3600 + 180) % 360 - 180;
			lat = (lat + 1800 + 90) % 180 - 90;
			switch (projection) {
				case MapProjection.Polar:
					lon = Mathf.Deg2Rad * lon;
					lat = Mathf.Deg2Rad * lat;
					if (lat < 0) {
						lat = 1.3 * Math.Cos (lat) * Math.Cos (lon);
				} else {
						lat = -1.3 * Math.Cos (lat) * Math.Cos (lon);
				}
					return Mathf.Rad2Deg * lat;
				default:
					return lat;
			}
		}
		public double unprojectLongitude ( double lon , double lat ) {
			if (lat > 90) {
				lat = 180 - lat;
				lon += 180;
			} else if (lat < -90) {
				lat = -180 - lat;
				lon += 180;
			}
			lon = (lon + 3600 + 180) % 360 - 180;
			lat = (lat + 1800 + 90) % 180 - 90;
			switch (projection) {
				case MapProjection.KavrayskiyVII:
					lon = Mathf.Deg2Rad * lon;
					lat = Mathf.Deg2Rad * lat;
					lon = lon / Math.Sqrt (Mathf.PI * Math.PI / 3.0f - lat * lat) * 2.0f * Math.PI / 3.0f;
					return Mathf.Rad2Deg * lon;
				case MapProjection.Polar:
					lon = Mathf.Deg2Rad * lon;
					lat = Mathf.Deg2Rad * lat;
					double lat0 = Math.PI / 2;
					if (lon < 0) {
						lon += Math.PI / 2;
						lat0 = -Math.PI / 2;
				} else {
						lon -= Math.PI / 2;
				}
					lon /= 1.3;
					lat /= 1.3;
					double p = Math.Sqrt (lon * lon + lat * lat);
					double c = Math.Asin (p);
					lon = Math.Atan2 ((lon * Math.Sin (c)) , (p * Math.Cos (lat0) * Math.Cos (c) - lat * Math.Sin (lat0) * Math.Sin (c)));
					lon = (Mathf.Rad2Deg * lon + 180) % 360 - 180;
					if (lon <= -180)
						lon = -180;
					return lon;
				default:
					return lon;
			}
		}
		public double unprojectLatitude ( double lon , double lat ) {
			if (lat > 90) {
				lat = 180 - lat;
				lon += 180;
			} else if (lat < -90) {
				lat = -180 - lat;
				lon += 180;
			}
			lon = (lon + 3600 + 180) % 360 - 180;
			lat = (lat + 1800 + 90) % 180 - 90;
			switch (projection) {
				case MapProjection.Polar:
					lon = Mathf.Deg2Rad * lon;
					lat = Mathf.Deg2Rad * lat;
					double lat0 = Math.PI / 2;
					if (lon < 0) {
						lon += Math.PI / 2;
						lat0 = -Math.PI / 2;
				} else {
						lon -= Math.PI / 2;
				}
					lon /= 1.3;
					lat /= 1.3;
					double p = Math.Sqrt (lon * lon + lat * lat);
					double c = Math.Asin (p);
					lat = Math.Asin (Math.Cos (c) * Math.Sin (lat0) + (lat * Math.Sin (c) * Math.Cos (lat0)) / (p));
					return Mathf.Rad2Deg * lat;
				default:
					return lat;
			}
		}


		/* MAP: scaling, centering (setting origin), translating, etc */
		public double mapscale, lon_offset, lat_offset;
		public int mapwidth, mapheight;
		public void setSize ( int w , int h ) {
			if (w == 0)
				w = 360 * (Screen.width / 360);
			if (w > 360 * 4)
				w = 360 * 4;
			mapwidth = w;
			mapscale = mapwidth / 360f;
			if (h <= 0)
				h = (int)(180 * mapscale);
			mapheight = h;
			if (map != null) {
				if (mapwidth != map.width || mapheight != map.height)
					map = null;
			}
		}
		public void setWidth ( int w ) {
			if (w == 0) {
				w = 360 * (int)(Screen.width / 360);
				if (w > 360 * 4)
					w = 360 * 4;
			}
			if (w < 360)
				w = 360;
			if (mapwidth == w)
				return;
			mapwidth = w;
			mapscale = mapwidth / 360f;
			mapheight = (int)(w / 2);
			big_heightmap = new float [mapwidth, mapheight, 3];
			map = null;
			resetMap ();
		}
		public void centerAround ( double lon , double lat ) {
			lon_offset = 180 + lon - (mapwidth / mapscale) / 2;
			lat_offset = 90 + lat - (mapheight / mapscale) / 2;
		}
		public double scaleLatitude ( double lat ) {
			lat -= lat_offset;
			lat *= 180f / (mapheight / mapscale);
			return lat;
		}
		public double scaleLongitude ( double lon ) {
			lon -= lon_offset;
			lon *= 360f / (mapwidth / mapscale);
			return lon;
		}

		/* MAP: shared state */
		public int mapmode = 0; // lots of EXTERNAL refs!
		public Texture2D map; // refs above: 214,215,216,232, below, and JSISCANsatRPM.


		/* MAP: internal state */
		protected int mapstep; // all refs are below
		protected bool mapsaved; // all refs are below
		protected double[] mapline; // all refs are below
		protected CelestialBody body; // all refs are below
		protected Color[] redline; // all refs are below

		/* MAP: nearly trivial functions */
		public void setBody ( CelestialBody b ) {
			if (body == b)
				return;
			body = b;
			resetMap ();
		}
		public bool isMapComplete () {
			if (map == null)
				return false;
			return mapstep >= map.height;
		}
		public void resetMap () {
			mapstep = 0;
			mapsaved = false;
		}
		public void resetMap ( int mode, int maptype ) {
			mapmode = mode;
            mapType = maptype;
			resetMap ();
		}

		/* MAP: export: PNG file */
		public void exportPNG () {
			string mode;

			switch (mapmode) {
				case 0: mode = "elevation"; break;
				case 1: mode = "slope"; break;
				case 2: mode = "biome"; break;
				default: mode = "unknown"; break;
			}

			if (SCANcontroller.controller.colours == 1)
				mode += "-grey";
			string filename = body.name + "_" + mode + "_" + map.width.ToString () + "x" + map.height.ToString ();
			if (projection != MapProjection.Rectangular)
				filename += "_" + projection.ToString ();
			filename += ".png";
			KSP.IO.File.WriteAllBytes<SCANdata> (map.EncodeToPNG () , filename , null);
			mapsaved = true;
			ScreenMessages.PostScreenMessage ("Map saved: " + filename , 5 , ScreenMessageStyle.UPPER_CENTER);
		}

		/* MAP: build: map to Texture2D */
		public Texture2D getPartialMap () {
			SCANdata data = SCANcontroller.controller.getData (body);
			Color[] pix;
			if (map == null) {
				map = new Texture2D (mapwidth , mapheight , TextureFormat.ARGB32 , false);
				pix = map.GetPixels ();
				for (int i=0; i<pix.Length; ++i)
					pix [i] = Color.clear;
				map.SetPixels (pix);
			} else if (mapstep >= map.height) {
				return map;
			}
			if (redline == null || redline.Length != map.width) {
				redline = new Color[map.width];
				for (int i=0; i<redline.Length; ++i)
					redline [i] = Color.red;
			}
			if (mapstep < map.height - 1) {
				map.SetPixels (0 , mapstep + 1 , map.width , 1 , redline);
			}
			if (mapstep <= 0) {
				mapstep = 0;
				mapline = new double[map.width];
			}
			pix = map.GetPixels (0 , mapstep , map.width , 1);
			for (int i=0; i<map.width; i++) {
				int scheme = 0;
				double lat = (mapstep * 1.0f / mapscale) - 90f + lat_offset;
				double lon = (i * 1.0f / mapscale) - 180f + lon_offset;
				double la = lat, lo = lon;
				lat = unprojectLatitude (lo , la);
				lon = unprojectLongitude (lo , la);
				pix [i] = Color.grey;
				if (double.IsNaN (lat) || double.IsNaN (lon) || lat < -90 || lat > 90 || lon < -180 || lon > 180) {
					pix [i] = Color.clear;
					continue;
				}
				if (mapmode == 0) {
					if (!data.isCovered (lon , lat , SCANdata.SCANtype.Altimetry))
						continue;
					if (body.pqsController == null) {
						pix [i] = Color.Lerp (Color.black , Color.white , UnityEngine.Random.value);
                        //big_heightmap[i, mapstep, SCANcontroller.controller.projection] = 0;
						continue;
					}
                    float val = 0f;
                    if (mapType == 0)
                        val = big_heightmap[i, mapstep, SCANcontroller.controller.projection];
                    if (val == 0)
                    {
                        if (data.isCovered(lon, lat, SCANdata.SCANtype.AltimetryHiRes))
                        {
                            // high resolution gets a coloured pixel for the actual position
                            val = (float)data.getElevation(lon, lat);
                            pix[i] = heightToColor(val, scheme);
                            heightMapArray(val, mapstep, i, mapType);
                        }
                        else
                        {
                            // basic altimetry gets forced greyscale with lower resolution
                            val = (float)data.getElevation(((int)(lon * 5)) / 5, ((int)(lat * 5)) / 5);
                            pix[i] = heightToColor(val, 1);
                            heightMapArray(val, mapstep, i, mapType);
                        }
                    }
                    else if (val != 0)
                    {
                        if (data.isCovered(lon, lat, SCANdata.SCANtype.AltimetryHiRes))
                        {
                            pix[i] = heightToColor(val, scheme);
                        }
                        else
                        {
                            pix[i] = heightToColor(val, 1);
                        }
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
					mapline [i] = val;
				} else if (mapmode == 1) {
					if (!data.isCovered (lon , lat , SCANdata.SCANtype.Altimetry))
						continue;
					if (body.pqsController == null) {
						pix [i] = Color.Lerp (Color.black , Color.white , UnityEngine.Random.value);
						continue;
					}
                    float val = 0f;
                    if (mapType == 0)
                        val = big_heightmap[i, mapstep, SCANcontroller.controller.projection];
					if (val == 0)
                    {
                        if (data.isCovered (lon , lat , SCANdata.SCANtype.AltimetryHiRes)) {
						    val = (float)data.getElevation (lon , lat);
                            heightMapArray(val, mapstep, i, mapType);
					    } else {
						    val = (float)data.getElevation (((int)(lon * 5)) / 5 , ((int)(lat * 5)) / 5);
                            heightMapArray(val, mapstep, i, mapType);
					    }
                    }
					if (mapstep == 0) {
						pix [i] = Color.grey;
					} else {
						// This doesn't actually calculate the slope per se, but it's faster
						// than asking for yet more elevation data. Please don't use this
						// code to operate nuclear power plants or rockets.
						double v1 = mapline [i];
						if (i > 0)
							v1 = Math.Max (v1 , mapline [i - 1]);
						if (i < mapline.Length - 1)
							v1 = Math.Max (v1 , mapline [i + 1]);
						float v = Mathf.Clamp ((float)Math.Abs (val - v1) / 1000f , 0 , 2f);
						if (SCANcontroller.controller.colours == 1) {
							pix [i] = Color.Lerp (Color.black , Color.white , v / 2f);
						} else {
							if (v < 1) {
								pix [i] = Color.Lerp (XKCDColors.PukeGreen , XKCDColors.Lemon , v);
							} else {
								pix [i] = Color.Lerp (XKCDColors.Lemon , XKCDColors.OrangeRed , v - 1);
							}
						}
					}
					mapline [i] = val;
				} else if (mapmode == 2) {
					if (!data.isCovered (lon , lat , SCANdata.SCANtype.Biome))
						continue;
					if (body.BiomeMap == null || body.BiomeMap.Map == null) {
						pix [i] = Color.Lerp (Color.black , Color.white , UnityEngine.Random.value);
						continue;
					}
					/* // this just basically stretches the actual biome map to fit... it looks horrible
				float u = ((lon + 360 + 180 + 90)) % 360;
				float v = ((lat + 180 + 90)) % 180;
				if(u < 0 || v < 0 || u >= 360 || v >= 180) continue;
				u /= 360f; v /= 180f;
				pix[i] = body.BiomeMap.Map.GetPixelBilinear(u, v);
				*/
					double bio = data.getBiomeIndexFraction (lon , lat);
					Color biome = Color.grey;
					if (SCANcontroller.controller.colours == 1) {
						if ((i > 0 && mapline [i - 1] != bio) || (mapstep > 0 && mapline [i] != bio)) {
							biome = Color.white;
						} else {
							biome = Color.Lerp (Color.black , Color.white , (float)bio);
						}
					} else {
						Color elevation = Color.gray;
						if (data.isCovered (lon , lat , SCANdata.SCANtype.Altimetry)) {
                            float val = 0f;
                            if (mapType == 0)
                                val = big_heightmap[i, mapstep, SCANcontroller.controller.projection];
							if (val == 0) {
                                if (data.isCovered(lon, lat, SCANdata.SCANtype.AltimetryHiRes))
                                {
                                    val = (float)data.getElevation(lon, lat);
                                    heightMapArray(val, mapstep, i, mapType);
                                }
                                else
                                {
                                    val = (float)data.getElevation(((int)(lon * 5)) / 5, ((int)(lat * 5)) / 5);
                                    heightMapArray(val, mapstep, i, mapType);
                                }
                            }
							elevation = Color.Lerp (Color.black , Color.white , Mathf.Clamp (val + 1500f , 0 , 9000) / 9000f);
						}
						Color bio1 = XKCDColors.CamoGreen;
						Color bio2 = XKCDColors.Marigold;
						if ((i > 0 && mapline [i - 1] != bio) || (mapstep > 0 && mapline [i] != bio)) {
							//biome = Color.Lerp(XKCDColors.Puce, elevation, 0.5f);
							biome = Color.white;
						} else {
							biome = Color.Lerp (Color.Lerp (bio1 , bio2 , (float)bio) , elevation , 0.5f);
						}
					}

					pix [i] = biome;
					mapline [i] = bio;
				}
			}
			map.SetPixels (0 , mapstep , map.width , 1 , pix);
			mapstep++;
			if (mapstep % 10 == 0 || mapstep >= map.height)
				map.Apply ();
			return map;
		}

	}
}
