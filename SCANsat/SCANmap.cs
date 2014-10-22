#region license
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
#endregion
using System;
using UnityEngine;
using palette = SCANsat.SCAN_UI.SCANpalette;

namespace SCANsat
{
	public class SCANmap
	{
		internal SCANmap(CelestialBody Body)
		{
			body = Body;
		}

		internal SCANmap()
		{

		}

		/* MAP: legends */

		public static Texture2D legend;
		private static float legendMin, legendMax;
		private static int legendScheme;
		public static Texture2D getLegend(float min, float max, int scheme, SCANdata data)
		{
			if (legend != null && legendMin == min && legendMax == max && legendScheme == scheme)
				return legend;
			legend = new Texture2D(256, 1, TextureFormat.RGB24, false);
			legendMin = min;
			legendMax = max;
			legendScheme = scheme;
			Color[] pix = legend.GetPixels();
			for (int x = 0; x < 256; ++x)
			{
				float val = (x * (max - min)) / 256f + min;
				pix[x] = palette.heightToColor(val, scheme, data);
			}
			legend.SetPixels(pix);
			legend.Apply();
			return legend;
		}

		public static Texture2D getLegend(float max, float min, float? clamp, Platform.Palettes.Palette p)
		{
			Texture2D t = new Texture2D(128, 1, TextureFormat.RGB24, false);
			Color[] pix = t.GetPixels();
			for (int x = 0; x < 128; x++ )
			{
				float val = (x * (max - min)) / 128f + min;
				pix[x] = palette.heightToColor(val, 0, max, min, clamp, p);
			}
			t.SetPixels(pix);
			t.Apply();
			return t;
		}

		/* MAP: projections */
		public enum MapProjection
		{
			Rectangular = 0,
			KavrayskiyVII = 1,
			Polar = 2,
		}
		public enum mapType
		{
			Altimetry = 0,
			Slope = 1,
			Biome = 2,
		}

		private static string[] getProjectionNames()
		{
			MapProjection[] v = (MapProjection[])Enum.GetValues(typeof(MapProjection));
			string[] r = new string[v.Length];
			for (int i = 0; i < v.Length; ++i)
				r[i] = v[i].ToString();
			return r;
		}

		private static string[] getMapTypeNames()
		{
			mapType[] v = (mapType[])Enum.GetValues(typeof(mapType));
			string[] r = new string[v.Length];
			for (int i = 0; i < v.Length; i++)
				r[i] = v[i].ToString();
			return r;
		}
		public static string[] projectionNames = getProjectionNames();
		public static string[] mapTypeNames = getMapTypeNames();
		public MapProjection projection = MapProjection.Rectangular;

		/* MAP: Big Map height map caching */
		protected float[, ,] big_heightmap;
		protected CelestialBody big_heightmap_body;
		internal mapType mType;


		public void heightMapArray(float height, int line, int i, mapType type)
		{
			if (type == 0)
			{
				big_heightmap[i, line, SCANcontroller.controller.projection] = height;
			}
		}


		public void setProjection(MapProjection p)
		{
			if (projection == p)
				return;
			projection = p;
			resetMap();
		}
		public double projectLongitude(double lon, double lat)
		{
			lon = (lon + 3600 + 180) % 360 - 180;
			lat = (lat + 1800 + 90) % 180 - 90;
			switch (projection)
			{
				case MapProjection.KavrayskiyVII:
					lon = Mathf.Deg2Rad * lon;
					lat = Mathf.Deg2Rad * lat;
					lon = (3.0f * lon / 2.0f / Math.PI) * Math.Sqrt(Math.PI * Math.PI / 3.0f - lat * lat);
					return Mathf.Rad2Deg * lon;
				case MapProjection.Polar:
					lon = Mathf.Deg2Rad * lon;
					lat = Mathf.Deg2Rad * lat;
					if (lat < 0)
					{
						lon = 1.3 * Math.Cos(lat) * Math.Sin(lon) - Math.PI / 2;
					}
					else
					{
						lon = 1.3 * Math.Cos(lat) * Math.Sin(lon) + Math.PI / 2;
					}
					return Mathf.Rad2Deg * lon;
				default:
					return lon;
			}
		}
		public double projectLatitude(double lon, double lat)
		{
			lon = (lon + 3600 + 180) % 360 - 180;
			lat = (lat + 1800 + 90) % 180 - 90;
			switch (projection)
			{
				case MapProjection.Polar:
					lon = Mathf.Deg2Rad * lon;
					lat = Mathf.Deg2Rad * lat;
					if (lat < 0)
					{
						lat = 1.3 * Math.Cos(lat) * Math.Cos(lon);
					}
					else
					{
						lat = -1.3 * Math.Cos(lat) * Math.Cos(lon);
					}
					return Mathf.Rad2Deg * lat;
				default:
					return lat;
			}
		}
		public double unprojectLongitude(double lon, double lat)
		{
			if (lat > 90)
			{
				lat = 180 - lat;
				lon += 180;
			}
			else if (lat < -90)
			{
				lat = -180 - lat;
				lon += 180;
			}
			lon = (lon + 3600 + 180) % 360 - 180;
			lat = (lat + 1800 + 90) % 180 - 90;
			switch (projection)
			{
				case MapProjection.KavrayskiyVII:
					lon = Mathf.Deg2Rad * lon;
					lat = Mathf.Deg2Rad * lat;
					lon = lon / Math.Sqrt(Mathf.PI * Math.PI / 3.0f - lat * lat) * 2.0f * Math.PI / 3.0f;
					return Mathf.Rad2Deg * lon;
				case MapProjection.Polar:
					lon = Mathf.Deg2Rad * lon;
					lat = Mathf.Deg2Rad * lat;
					double lat0 = Math.PI / 2;
					if (lon < 0)
					{
						lon += Math.PI / 2;
						lat0 = -Math.PI / 2;
					}
					else
					{
						lon -= Math.PI / 2;
					}
					lon /= 1.3;
					lat /= 1.3;
					double p = Math.Sqrt(lon * lon + lat * lat);
					double c = Math.Asin(p);
					lon = Math.Atan2((lon * Math.Sin(c)), (p * Math.Cos(lat0) * Math.Cos(c) - lat * Math.Sin(lat0) * Math.Sin(c)));
					lon = (Mathf.Rad2Deg * lon + 180) % 360 - 180;
					if (lon <= -180)
						lon = -180;
					return lon;
				default:
					return lon;
			}
		}
		public double unprojectLatitude(double lon, double lat)
		{
			if (lat > 90)
			{
				lat = 180 - lat;
				lon += 180;
			}
			else if (lat < -90)
			{
				lat = -180 - lat;
				lon += 180;
			}
			lon = (lon + 3600 + 180) % 360 - 180;
			lat = (lat + 1800 + 90) % 180 - 90;
			switch (projection)
			{
				case MapProjection.Polar:
					lon = Mathf.Deg2Rad * lon;
					lat = Mathf.Deg2Rad * lat;
					double lat0 = Math.PI / 2;
					if (lon < 0)
					{
						lon += Math.PI / 2;
						lat0 = -Math.PI / 2;
					}
					else
					{
						lon -= Math.PI / 2;
					}
					lon /= 1.3;
					lat /= 1.3;
					double p = Math.Sqrt(lon * lon + lat * lat);
					double c = Math.Asin(p);
					lat = Math.Asin(Math.Cos(c) * Math.Sin(lat0) + (lat * Math.Sin(c) * Math.Cos(lat0)) / (p));
					return Mathf.Rad2Deg * lat;
				default:
					return lat;
			}
		}


		/* MAP: scaling, centering (setting origin), translating, etc */
		public double mapscale, lon_offset, lat_offset;
		public int mapwidth, mapheight;
		public void setSize(int w, int h)
		{
			if (w == 0)
				w = 360 * (Screen.width / 360);
			if (w > 360 * 4)
				w = 360 * 4;
			mapwidth = w;
			mapscale = mapwidth / 360f;
			if (h <= 0)
				h = (int)(180 * mapscale);
			mapheight = h;
			if (map != null)
			{
				if (mapwidth != map.width || mapheight != map.height)
					map = null;
			}
		}
		public void setWidth(int w)
		{
			if (w == 0)
			{
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
			/* big map caching */
			big_heightmap = new float[mapwidth, mapheight, 3];
			big_heightmap_body = body;
			map = null;
			resetMap();
		}
		public void centerAround(double lon, double lat)
		{
			lon_offset = 180 + lon - (mapwidth / mapscale) / 2;
			lat_offset = 90 + lat - (mapheight / mapscale) / 2;
		}
		public double scaleLatitude(double lat)
		{
			lat -= lat_offset;
			lat *= 180f / (mapheight / mapscale);
			return lat;
		}
		public double scaleLongitude(double lon)
		{
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
		internal CelestialBody body; // all refs are below
		public SCANdata.SCANResource resource;

		/* MAP: nearly trivial functions */
		public void setBody(CelestialBody b)
		{
			if (body == b)
				return;
			body = b;
			SCANcontroller.controller.Resources(b); //Repopulate resource list when changing SOI
			if (SCANcontroller.controller.globalOverlay)
				resource = SCANcontroller.controller.ResourcesList[SCANcontroller.controller.gridSelection];
			resetMap();
		}
		public bool isMapComplete()
		{
			if (map == null)
				return false;
			return mapstep >= map.height;
		}
		public void resetMap()
		{
			mapstep = 0;
			mapsaved = false;
			if (SCANcontroller.controller.globalOverlay)
			{ //Make sure that a resource is initialized if necessary
				if (resource == null) resource = SCANcontroller.controller.ResourcesList[SCANcontroller.controller.gridSelection];
				if (SCANcontroller.controller.resourceOverlayType == 1)
					SCANcontroller.controller.KethaneReset = !SCANcontroller.controller.KethaneReset;
			}
		}
		public void resetMap(int mode, int maptype)
		{
			mapmode = mode;
			mType = (mapType)maptype;
			resetMap();
		}
		public void setResource(string s)
		{ //Used when a different resource is selected
			if (resource == null) resource = SCANcontroller.controller.ResourcesList[SCANcontroller.controller.gridSelection];
			else if (resource.name == s)
				return;
			resource = SCANcontroller.controller.ResourcesList[SCANcontroller.controller.gridSelection];
			resetMap();
		}

		/* MAP: export: PNG file */
		public void exportPNG()
		{
			string mode;

			switch (mapmode)
			{
				case 0: mode = "elevation"; break;
				case 1: mode = "slope"; break;
				case 2: mode = "biome"; break;
				default: mode = "unknown"; break;
			}

			if (SCANcontroller.controller.colours == 1)
				mode += "-grey";
			string filename = body.name + "_" + mode + "_" + map.width.ToString() + "x" + map.height.ToString();
			if (projection != MapProjection.Rectangular)
				filename += "_" + projection.ToString();
			filename += ".png";
			KSP.IO.File.WriteAllBytes<SCANdata>(map.EncodeToPNG(), filename, null);
			mapsaved = true;
			ScreenMessages.PostScreenMessage("Map saved: " + filename, 5, ScreenMessageStyle.UPPER_CENTER);
		}

		/* MAP: build: map to Texture2D */
		public Texture2D getPartialMap()
		{
			SCANdata data = SCANUtil.getData(body);
			Color[] pix;

			/* init cache if necessary */
			if (body != big_heightmap_body)
			{
				switch (mType)
				{
					case 0:
						{
							for (int x = 0; x < mapwidth; x++)
							{ //Save memory by not unnecessarily making a new cache
								for (int y = 0; y < mapwidth / 2; y++)
								{
									for (int z = 0; z < 3; z++)
									{
										big_heightmap[x, y, z] = 0f;
									}
								}
							}
							big_heightmap_body = body;
							break;
						}
					default: break;
				}
			}

			if (map == null)
			{
				map = new Texture2D(mapwidth, mapheight, TextureFormat.ARGB32, false);
				pix = map.GetPixels();
				for (int i = 0; i < pix.Length; ++i)
					pix[i] = palette.clear;
				map.SetPixels(pix);
			}
			else if (mapstep >= map.height)
			{
				return map;
			}
			if (palette.redline == null || palette.redline.Length != map.width)
			{
				palette.redline = new Color[map.width];
				for (int i = 0; i < palette.redline.Length; ++i)
					palette.redline[i] = palette.red;
			}
			if (mapstep < map.height - 1)
			{
				map.SetPixels(0, mapstep + 1, map.width, 1, palette.redline);
			}
			if (mapstep <= 0)
			{
				mapstep = 0;
				mapline = new double[map.width];
			}
			pix = map.GetPixels(0, mapstep, map.width, 1);
			for (int i = 0; i < map.width; i++)
			{
				Color baseColor = palette.grey; //default pixel color 
				int scheme = 0;
				double lat = (mapstep * 1.0f / mapscale) - 90f + lat_offset;
				double lon = (i * 1.0f / mapscale) - 180f + lon_offset;
				double la = lat, lo = lon;
				lat = unprojectLatitude(lo, la);
				lon = unprojectLongitude(lo, la);
				pix[i] = baseColor;
				if (double.IsNaN(lat) || double.IsNaN(lon) || lat < -90 || lat > 90 || lon < -180 || lon > 180)
				{
					pix[i] = palette.clear;
					continue;
				}
				if (mapmode == 0)
				{
					if (body.pqsController == null)
					{
						baseColor = palette.lerp(palette.black, palette.white, UnityEngine.Random.value);
					}
					else if (SCANUtil.isCovered(lon, lat, data, SCANdata.SCANtype.Altimetry))
					{
						float val = 0f;
						if (mType == 0)
							val = big_heightmap[i, mapstep, SCANcontroller.controller.projection];
						if (val == 0)
						{
							if (SCANUtil.isCovered(lon, lat, data, SCANdata.SCANtype.AltimetryHiRes))
							{
								// high resolution gets a coloured pixel for the actual position
								val = (float)SCANUtil.getElevation(body, lon, lat);
								baseColor = palette.heightToColor(val, scheme, data); //use temporary color to store pixel value
								if (val == 0f) val = -0.001f;
								heightMapArray(val, mapstep, i, mType);
							}
							else
							{
								// basic altimetry gets forced greyscale with lower resolution
								val = (float)SCANUtil.getElevation(body, ((int)(lon * 5)) / 5, ((int)(lat * 5)) / 5);
								baseColor = palette.heightToColor(val, 1, data);
								if (val == 0f) val = -0.001f;
								heightMapArray(val, mapstep, i, mType);
							}
						}
						else if (val != 0)
						{
							if (SCANUtil.isCovered(lon, lat, data, SCANdata.SCANtype.AltimetryHiRes))
							{
								baseColor = palette.heightToColor(val, scheme, data);
							}
							else
							{
								baseColor = palette.heightToColor(val, 1, data);
							}
						}
						mapline[i] = val;
					}
					if (SCANcontroller.controller.map_ResourceOverlay && SCANcontroller.controller.globalOverlay)
					{
						if (SCANcontroller.controller.resourceOverlayType == 0 && SCANversions.ORSXFound)
						{
							if (SCANUtil.isCovered(lon, lat, data, resource.type)) //check our new resource coverage map
							{
								double amount = SCANUtil.ORSOverlay(lon, lat, body.flightGlobalsIndex, resource.name); //grab the resource amount for the current pixel
								double scalar = resource.ORS_Multiplier * resource.ORS_Scalar * resource.ORS_Threshold; //low cutoff value
								if (resource.linear)
								{ //linear resources are measured on 0-100% scale
									amount *= 100;
									if (amount > scalar)
									{
										if (amount > 100) amount = 100;
										pix[i] = palette.lerp(baseColor, palette.lerp(resource.emptyColor, resource.fullColor, (float)(amount) / 100f), 0.3f); //vary color by resource amount
									}
									else pix[i] = palette.lerp(baseColor, palette.grey, 0.4f);
								}
								else
								{ //log_scale resources are measured in ppm
									amount *= 1000000;
									if (amount > scalar)
									{
										if (amount > 25 * scalar) amount = 25 * scalar; //max cutoff value
										pix[i] = palette.lerp(baseColor, palette.lerp(resource.emptyColor, resource.fullColor, (float)(amount) / (float)(25 * scalar)), 0.8f); //vary color by resource amount
									}
									else pix[i] = palette.lerp(baseColor, palette.grey, 0.4f);
								}
							}
							else pix[i] = baseColor;
						}
						else if (SCANcontroller.controller.resourceOverlayType == 1) //Kethane overlay
						{
							if (SCANUtil.isCovered(lon, lat, data, resource.type))
							{
								int ilon = SCANUtil.icLON(lon);
								int ilat = SCANUtil.icLAT(lat);
								float amount = data.KethaneValueMap[ilon, ilat]; //Fetch Kethane resource values from cached array
								if (amount <= 0) pix[i] = palette.lerp(baseColor, palette.grey, 0.4f);
								else
								{
									pix[i] = palette.lerp(baseColor, palette.lerp(resource.emptyColor, resource.fullColor, amount / resource.maxValue), 0.8f);
								}
							}
							else pix[i] = baseColor;
						}
						else pix[i] = baseColor;
					}
					else pix[i] = baseColor;

					/* draw height lines - works, but mostly useless...
				int step = (int)(val / 1000);
				int step_h = step, step_v = step;
				if(i > 0) step_h = (int)(bigline[i - 1] / 1000);
				if(bigstep > 0) step_v = (int)(bigline[i] / 1000);
				if(step != step_h || step != step_v) {
					pix[i] = palette.white;
				}
				*/
					//mapline [i] = val;
				}
				else if (mapmode == 1)
				{
					if (body.pqsController == null)
					{
						baseColor = palette.lerp(palette.black, palette.white, UnityEngine.Random.value);
					}
					else if (SCANUtil.isCovered(lon, lat, data, SCANdata.SCANtype.Altimetry))
					{
						float val = 0f;
						if (mType == 0)
							val = big_heightmap[i, mapstep, SCANcontroller.controller.projection];
						if (val == 0)
						{
							if (SCANUtil.isCovered(lon, lat, data, SCANdata.SCANtype.AltimetryHiRes))
							{
								val = (float)SCANUtil.getElevation(body, lon, lat);
								if (val == 0f) val = -0.001f;
								heightMapArray(val, mapstep, i, mType);
							}
							else
							{
								val = (float)SCANUtil.getElevation(body, ((int)(lon * 5)) / 5, ((int)(lat * 5)) / 5);
								if (val == 0f) val = -0.001f;
								heightMapArray(val, mapstep, i, mType);
							}
						}
						if (mapstep == 0)
						{
							baseColor = palette.grey;
						}
						else
						{
							// This doesn't actually calculate the slope per se, but it's faster
							// than asking for yet more elevation data. Please don't use this
							// code to operate nuclear power plants or rockets.
							double v1 = mapline[i];
							if (i > 0)
								v1 = Math.Max(v1, mapline[i - 1]);
							if (i < mapline.Length - 1)
								v1 = Math.Max(v1, mapline[i + 1]);
							float v = Mathf.Clamp((float)Math.Abs(val - v1) / 1000f, 0, 2f);
							if (SCANcontroller.controller.colours == 1)
							{
								baseColor = palette.lerp(palette.black, palette.white, v / 2f);
							}
							else
							{
								if (v < 1)
								{
									baseColor = palette.lerp(palette.xkcd_PukeGreen, palette.xkcd_Lemon, v);
								}
								else
								{
									baseColor = palette.lerp(palette.xkcd_Lemon, palette.xkcd_OrangeRed, v - 1);
								}
							}
						}
						mapline[i] = val;
					}
					if (SCANcontroller.controller.map_ResourceOverlay && SCANcontroller.controller.globalOverlay)
					{
						if (SCANcontroller.controller.resourceOverlayType == 0 && SCANversions.ORSXFound)
						{
							if (SCANUtil.isCovered(lon, lat, data, resource.type)) //check our new resource coverage map
							{
								double amount = SCANUtil.ORSOverlay(lon, lat, body.flightGlobalsIndex, resource.name); //grab the resource amount for the current pixel
								double scalar = resource.ORS_Multiplier * resource.ORS_Scalar * resource.ORS_Threshold;
								if (resource.linear)
								{
									amount *= 100;
									if (amount > scalar)
									{
										if (amount > 100) amount = 100; //max cutoff value
										pix[i] = palette.lerp(baseColor, palette.lerp(resource.emptyColor, resource.fullColor, (float)(amount) / 100f), 0.3f); //vary color by resource amount
									}
									else pix[i] = palette.lerp(baseColor, palette.grey, 0.4f);
								}
								else
								{
									amount *= 1000000;
									if (amount > scalar)
									{
										if (amount > 25 * scalar) amount = 25 * scalar; //max cutoff value
										pix[i] = palette.lerp(baseColor, palette.lerp(resource.emptyColor, resource.fullColor, (float)(amount) / (float)(25 * scalar)), 0.8f); //vary color by resource amount
									}
									else pix[i] = palette.lerp(baseColor, palette.grey, 0.4f);
								}
							}
							else pix[i] = baseColor;
						}
						else if (SCANcontroller.controller.resourceOverlayType == 1)
						{
							if (SCANUtil.isCovered(lon, lat, data, resource.type))
							{
								int ilon = SCANUtil.icLON(lon);
								int ilat = SCANUtil.icLAT(lat);
								float amount = data.KethaneValueMap[ilon, ilat];
								if (amount <= 0) pix[i] = palette.lerp(baseColor, palette.grey, 0.4f);
								else
								{
									pix[i] = palette.lerp(baseColor, palette.lerp(resource.emptyColor, resource.fullColor, amount / resource.maxValue), 0.8f);
								}
							}
							else pix[i] = baseColor;
						}
						else pix[i] = baseColor;
					}
					else pix[i] = baseColor;
				}
				else if (mapmode == 2)
				{
					if (body.BiomeMap == null || body.BiomeMap.Map == null)
					{
						baseColor = palette.lerp(palette.black, palette.white, UnityEngine.Random.value);
					}
					/* // this just basically stretches the actual biome map to fit... it looks horrible
				float u = ((lon + 360 + 180 + 90)) % 360;
				float v = ((lat + 180 + 90)) % 180;
				if(u < 0 || v < 0 || u >= 360 || v >= 180) continue;
				u /= 360f; v /= 180f;
				pix[i] = body.BiomeMap.Map.GetPixelBilinear(u, v);
				*/
					else if (SCANUtil.isCovered(lon, lat, data, SCANdata.SCANtype.Biome))
					{
						double bio = SCANUtil.getBiomeIndexFraction(body, lon, lat);
						Color biome = palette.grey;
						if (SCANcontroller.controller.colours == 1)
						{
							if ((i > 0 && mapline[i - 1] != bio) || (mapstep > 0 && mapline[i] != bio))
							{
								biome = palette.white;
							}
							else
							{
								biome = palette.lerp(palette.black, palette.white, (float)bio);
							}
						}
						else
						{
							Color elevation = palette.grey;
							if (body.pqsController == null)
							{
								baseColor = palette.lerp(palette.black, palette.white, UnityEngine.Random.value);
							}
							else if (SCANUtil.isCovered(lon, lat, data, SCANdata.SCANtype.Altimetry))
							{
								float val = 0f;
								if (mType == 0)
									val = big_heightmap[i, mapstep, SCANcontroller.controller.projection];
								if (val == 0)
								{
									if (SCANUtil.isCovered(lon, lat, data, SCANdata.SCANtype.AltimetryHiRes))
									{
										val = (float)SCANUtil.getElevation(body, lon, lat);
										if (val == 0f) val = -0.001f;
										heightMapArray(val, mapstep, i, mType);
									}
									else
									{
										val = (float)SCANUtil.getElevation(body, ((int)(lon * 5)) / 5, ((int)(lat * 5)) / 5);
										if (val == 0f) val = -0.001f;
										heightMapArray(val, mapstep, i, mType);
									}
								}
								elevation = palette.lerp(palette.black, palette.white, Mathf.Clamp(val + 1500f, 0, 9000) / 9000f);
							}
							Color bio1 = palette.xkcd_CamoGreen;
							Color bio2 = palette.xkcd_Marigold;
							if ((i > 0 && mapline[i - 1] != bio) || (mapstep > 0 && mapline[i] != bio))
							{
								//biome = palette.lerp(palette.xkcd_Puce, elevation, 0.5f);
								biome = palette.white;
							}
							else
							{
								biome = palette.lerp(palette.lerp(bio1, bio2, (float)bio), elevation, 0.5f);
							}
						}

						baseColor = biome;
						mapline[i] = bio;
					}
					if (SCANcontroller.controller.map_ResourceOverlay && SCANcontroller.controller.globalOverlay)
					{
						if (SCANcontroller.controller.resourceOverlayType == 0 && SCANversions.ORSXFound)
						{
							if (SCANUtil.isCovered(lon, lat, data, resource.type)) //check our new resource coverage map
							{
								double amount = SCANUtil.ORSOverlay(lon, lat, body.flightGlobalsIndex, resource.name); //grab the resource amount for the current pixel
								double scalar = resource.ORS_Multiplier * resource.ORS_Scalar * resource.ORS_Threshold;
								if (resource.linear)
								{
									amount *= 100;
									if (amount > scalar)
									{
										if (amount > 100) amount = 100; //max cutoff value
										pix[i] = palette.lerp(baseColor, palette.lerp(resource.emptyColor, resource.fullColor, (float)(amount) / 100f), 0.3f); //vary color by resource amount
									}
									else pix[i] = palette.lerp(baseColor, palette.grey, 0.4f);
								}
								else
								{
									amount *= 1000000;
									if (amount > scalar)
									{
										if (amount > 25 * scalar) amount = 25 * scalar; //max cutoff value
										pix[i] = palette.lerp(baseColor, palette.lerp(resource.emptyColor, resource.fullColor, (float)(amount) / (float)(25 * scalar)), 0.8f); //vary color by resource amount
									}
									else pix[i] = palette.lerp(baseColor, palette.grey, 0.4f);
								}
							}
							else pix[i] = baseColor;
						}
						else if (SCANcontroller.controller.resourceOverlayType == 1)
						{
							if (SCANUtil.isCovered(lon, lat, data, resource.type))
							{
								int ilon = SCANUtil.icLON(lon);
								int ilat = SCANUtil.icLAT(lat);
								float amount = data.KethaneValueMap[ilon, ilat];
								if (amount <= 0) pix[i] = palette.lerp(baseColor, palette.grey, 0.4f);
								else
								{
									pix[i] = palette.lerp(baseColor, palette.lerp(resource.emptyColor, resource.fullColor, amount / resource.maxValue), 0.8f);
								}
							}
							else pix[i] = baseColor;
						}
						else pix[i] = baseColor;
					}
					else pix[i] = baseColor;
				}
			}
			map.SetPixels(0, mapstep, map.width, 1, pix);
			mapstep++;
			if (mapstep % 10 == 0 || mapstep >= map.height)
				map.Apply();
			return map;
		}

	}
}
