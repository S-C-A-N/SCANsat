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
using SCANsat.Platform.Palettes;
using SCANsat.Platform.Logging;
using SCANsat.SCAN_Data;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat.SCAN_Map
{
	public class SCANmap
	{
		internal SCANmap(CelestialBody Body, bool Cache)
		{
			body = Body;
			cache = Cache;
		}

		internal SCANmap()
		{
		}

		/* MAP: Big Map height map caching */
		private float[,] big_heightmap;
		private CelestialBody big_heightmap_body;
		private bool cache;

		private void terrainHeightToArray(double lon, double lat, int ilon, int ilat)
		{
			float alt = 0f;
			alt = (float)SCANUtil.getElevation(body, lon, lat);
			if (alt == 0f)
				alt = -0.001f;
			big_heightmap[ilon, ilat] = alt;
		}

		/* MAP: Projection methods for converting planet coordinates to the rectangular texture */
		internal MapProjection projection = MapProjection.Rectangular;

		internal void setProjection(MapProjection p)
		{
			if (projection == p)
				return;
			projection = p;
			resetMap();
		}

		internal double projectLongitude(double lon, double lat)
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

		internal double projectLatitude(double lon, double lat)
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

		internal double unprojectLongitude(double lon, double lat)
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

		internal double unprojectLatitude(double lon, double lat)
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
		internal double mapscale, lon_offset, lat_offset;
		internal int mapwidth, mapheight;
		internal void setSize(int w, int h)
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

		internal void setWidth(int w)
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
			big_heightmap = new float[mapwidth, mapheight];
			big_heightmap_body = body;
			map = null;
			resetMap();
		}

		internal void centerAround(double lon, double lat)
		{
			lon_offset = 180 + lon - (mapwidth / mapscale) / 2;
			lat_offset = 90 + lat - (mapheight / mapscale) / 2;
		}

		internal double scaleLatitude(double lat)
		{
			lat -= lat_offset;
			lat *= 180f / (mapheight / mapscale);
			return lat;
		}

		internal double scaleLongitude(double lon)
		{
			lon -= lon_offset;
			lon *= 360f / (mapwidth / mapscale);
			return lon;
		}

		private int unScaleLatitude(double lat)
		{
			lat -= lat_offset;
			lat += 90;
			lat *= mapscale;
			int ilat = Mathf.RoundToInt((float)lat);
			if (ilat < 0)
				ilat = 0;
			if (ilat >= mapheight)
				ilat = mapheight - 1;
			return ilat;
		}

		private int unScaleLongitude(double lon)
		{
			lon -= lon_offset;
			lon += 180;
			lon *= mapscale;
			int ilon = Mathf.RoundToInt((float)lon);
			if (ilon < 0)
				ilon = 0;
			if (ilon >= mapwidth)
				ilon = mapwidth - 1;
			return ilon;
		}

		/* MAP: shared state */
		//public int mapmode = 0; // lots of EXTERNAL refs!
		internal mapType mType;
		internal Texture2D map; // refs above: 214,215,216,232, below, and JSISCANsatRPM.
		internal CelestialBody body; // all refs are below
		internal SCANresource resource;
		internal SCANmapLegend mapLegend;

		/* MAP: internal state */
		private int mapstep; // all refs are below
		private bool mapsaved; // all refs are below
		private double[] mapline; // all refs are below

		/* MAP: nearly trivial functions */
		public void setBody(CelestialBody b)
		{
			if (body == b)
				return;
			body = b;
			//SCANcontroller.controller.Resources(b); //Repopulate resource list when changing SOI
			if (SCANcontroller.controller.GlobalResourceOverlay)
				resource = SCANcontroller.controller.ResourceList[SCANcontroller.controller.resourceSelection][b.name];
			resetMap();
		}

		internal bool isMapComplete()
		{
			if (map == null)
				return false;
			return mapstep >= map.height;
		}

		public void resetMap()
		{
			mapstep = 0;
			mapsaved = false;
			if (SCANcontroller.controller.GlobalResourceOverlay)
			{ //Make sure that a resource is initialized if necessary
				if (resource == null && body != null) resource = SCANcontroller.controller.ResourceList[SCANcontroller.controller.resourceSelection][body.name];
				if (SCANcontroller.controller.resourceOverlayType == 1)
					SCANcontroller.controller.KethaneReset = !SCANcontroller.controller.KethaneReset;
			}
		}

		public void resetMap(mapType mode, bool Cache)
		{
			mType = mode;
			cache = Cache;
			resetMap();
		}

		/* MAP: export: PNG file */
		internal void exportPNG()
		{
			string mode;

			switch (mType)
			{
				case mapType.Altimetry: mode = "elevation"; break;
				case mapType.Slope: mode = "slope"; break;
				case mapType.Biome: mode = "biome"; break;
				default: mode = "unknown"; break;
			}
			if (SCANcontroller.controller.map_ResourceOverlay && SCANcontroller.controller.GlobalResourceOverlay && !string.IsNullOrEmpty(SCANcontroller.controller.resourceSelection))
				mode += "-" + SCANcontroller.controller.resourceSelection;
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
		internal Texture2D getPartialMap()
		{
			SCANdata data = SCANUtil.getData(body);
			if (data == null)
				return new Texture2D(1, 1);
			Color[] pix;

			/* init cache if necessary */
			if (body != big_heightmap_body)
			{
				switch (mType)
				{
					case 0:
						{
							for (int x = 0; x < mapwidth; x++)
							{
								for (int y = 0; y < mapwidth / 2; y++)
									big_heightmap[x, y] = 0f;
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
				Color baseColor = palette.grey;
				pix[i] = baseColor;
				int scheme = SCANcontroller.controller.colours;
				float projVal = 0f;
				double lat = (mapstep * 1.0f / mapscale) - 90f + lat_offset;
				double lon = (i * 1.0f / mapscale) - 180f + lon_offset;
				double la = lat, lo = lon;
				lat = unprojectLatitude(lo, la);
				lon = unprojectLongitude(lo, la);

				/* Introduce altimetry check here; Use unprojected lat/long coordinates
				 * All cached altimetry data stored in a single 2D array in rectangular format
				 * Pull altimetry data from cache after unprojection
				 */

				if (body.pqsController != null && cache)
				{
					if (big_heightmap[i, mapstep] == 0f)
					{
						if (SCANUtil.isCovered(lo, la, data, SCANtype.Altimetry))
							terrainHeightToArray(lo, la, i, mapstep);
					}
				}

				if (double.IsNaN(lat) || double.IsNaN(lon) || lat < -90 || lat > 90 || lon < -180 || lon > 180)
				{
					pix[i] = palette.clear;
					continue;
				}

				if (mType == mapType.Altimetry)
				{
					if (body.pqsController == null)
					{
						baseColor = palette.lerp(palette.black, palette.white, UnityEngine.Random.value);
					}
					else if (SCANUtil.isCovered(lon, lat, data, SCANtype.AltimetryHiRes))
					{
						if (cache)
						{
							projVal = big_heightmap[unScaleLongitude(lon), unScaleLatitude(lat)];
							if (projVal == 0f)
								projVal = (float)SCANUtil.getElevation(body, lon, lat);
						}
						else
							projVal = (float)SCANUtil.getElevation(body, lon, lat);
						baseColor = palette.heightToColor(projVal, scheme, data);
					}
					else if (SCANUtil.isCovered(lon, lat, data, SCANtype.AltimetryLoRes))
					{
						// basic altimetry gets forced greyscale with lower resolution
						if (cache)
						{
							projVal = big_heightmap[(unScaleLongitude(lon) * 5) / 5, (unScaleLatitude(lat) * 5) / 5];
							if (projVal == 0f)
								projVal = (float)SCANUtil.getElevation(body, ((int)(lon * 5)) / 5, ((int)(lat * 5)) / 5);
						}
						else
							projVal = (float)SCANUtil.getElevation(body, ((int)(lon * 5)) / 5, ((int)(lat * 5)) / 5);
						baseColor = palette.heightToColor(projVal, 1, data);
					}
					mapline[i] = projVal;

					if (SCANcontroller.controller.map_ResourceOverlay && SCANcontroller.controller.GlobalResourceOverlay)
					{
						if (SCANcontroller.controller.resourceOverlayType == 0 && SCANversions.RegolithFound)
						{
							if (SCANUtil.isCovered(lon, lat, data, resource.Type)) //check our new resource coverage map
							{
								double amount = SCANUtil.RegolithOverlay(lat, lon, resource.Name, body.flightGlobalsIndex); //grab the resource amount for the current pixel
								double scalar = resource.MinValue + ((resource.MaxValue - resource.MinValue) / 5);
								amount *= 100;
								if (amount > scalar)
								{
									if (amount > 100) amount = 100;
									pix[i] = palette.lerp(baseColor, palette.lerp(resource.EmptyColor, resource.FullColor, (float)(amount) / (resource.MaxValue - resource.MinValue)), 0.3f); //vary color by resource amount
								}
								else pix[i] = palette.lerp(baseColor, palette.grey, 0.4f);
							}
							else pix[i] = baseColor;
						}
						else if (SCANcontroller.controller.resourceOverlayType == 1) //Kethane overlay
						{
							if (SCANUtil.isCovered(lon, lat, data, resource.Type))
							{
								int ilon = SCANUtil.icLON(lon);
								int ilat = SCANUtil.icLAT(lat);
								float amount = data.KethaneValueMap[ilon, ilat]; //Fetch Kethane resource values from cached array
								if (amount <= 0) pix[i] = palette.lerp(baseColor, palette.grey, 0.4f);
								else
								{
									pix[i] = palette.lerp(baseColor, palette.lerp(resource.EmptyColor, resource.FullColor, amount / resource.MaxValue), 0.8f);
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

				else if (mType == mapType.Slope)
				{
					if (body.pqsController == null)
					{
						baseColor = palette.lerp(palette.black, palette.white, UnityEngine.Random.value);
					}
					else if (SCANUtil.isCovered(lon, lat, data, SCANtype.Altimetry))
					{
						if (SCANUtil.isCovered(lon, lat, data, SCANtype.AltimetryHiRes))
						{
							if (cache)
							{
								projVal = big_heightmap[unScaleLongitude(lon), unScaleLatitude(lat)];
								if (projVal == 0f)
									projVal = (float)SCANUtil.getElevation(body, lon, lat);
							}
							else
								projVal = (float)SCANUtil.getElevation(body, lon, lat);
						}
						else
						{
							if (cache)
							{
								projVal = big_heightmap[(unScaleLongitude(lon) * 5) / 5, (unScaleLatitude(lat) * 5) / 5];
								if (projVal == 0f)
									projVal = (float)SCANUtil.getElevation(body, ((int)(lon * 5)) / 5, ((int)(lat * 5)) / 5);
							}
							else
								projVal = (float)SCANUtil.getElevation(body, ((int)(lon * 5)) / 5, ((int)(lat * 5)) / 5);
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
							float v = Mathf.Clamp((float)Math.Abs(projVal - v1) / 1000f, 0, 2f);
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
						mapline[i] = projVal;
					}
					if (SCANcontroller.controller.map_ResourceOverlay && SCANcontroller.controller.GlobalResourceOverlay)
					{
						if (SCANcontroller.controller.resourceOverlayType == 0 && SCANversions.RegolithFound)
						{
							if (SCANUtil.isCovered(lon, lat, data, resource.Type)) //check our new resource coverage map
							{
								double amount = SCANUtil.RegolithOverlay(lat, lon, resource.Name, body.flightGlobalsIndex); //grab the resource amount for the current pixel
								double scalar = resource.MinValue + ((resource.MaxValue - resource.MinValue) / 5);
								amount *= 100;
								if (amount > scalar)
								{
									if (amount > 100) amount = 100; //max cutoff value
									pix[i] = palette.lerp(baseColor, palette.lerp(resource.EmptyColor, resource.FullColor, (float)(amount) / (resource.MaxValue - resource.MinValue)), 0.3f); //vary color by resource amount
								}
								else pix[i] = palette.lerp(baseColor, palette.grey, 0.4f);
							}
							else pix[i] = baseColor;
						}
						else if (SCANcontroller.controller.resourceOverlayType == 1)
						{
							if (SCANUtil.isCovered(lon, lat, data, resource.Type))
							{
								int ilon = SCANUtil.icLON(lon);
								int ilat = SCANUtil.icLAT(lat);
								float amount = data.KethaneValueMap[ilon, ilat];
								if (amount <= 0) pix[i] = palette.lerp(baseColor, palette.grey, 0.4f);
								else
								{
									pix[i] = palette.lerp(baseColor, palette.lerp(resource.EmptyColor, resource.FullColor, amount / resource.MaxValue), 0.8f);
								}
							}
							else pix[i] = baseColor;
						}
						else pix[i] = baseColor;
					}
					else pix[i] = baseColor;
				}

				else if (mType == mapType.Biome)
				{
					if (body.BiomeMap == null)
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
					else if (SCANUtil.isCovered(lon, lat, data, SCANtype.Biome))
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
							else if (SCANUtil.isCovered(lon, lat, data, SCANtype.Altimetry))
							{
								if (SCANUtil.isCovered(lon, lat, data, SCANtype.AltimetryHiRes))
								{
									if (cache)
									{
										projVal = big_heightmap[unScaleLongitude(lon), unScaleLatitude(lat)];
										if (projVal == 0f)
											projVal = (float)SCANUtil.getElevation(body, lon, lat);
									}
									else
										projVal = (float)SCANUtil.getElevation(body, lon, lat);
								}
								else
								{
									if (cache)
									{
										projVal = big_heightmap[(unScaleLongitude(lon) * 5) / 5, (unScaleLatitude(lat) * 5) / 5];
										if (projVal == 0f)
											projVal = (float)SCANUtil.getElevation(body, ((int)(lon * 5)) / 5, ((int)(lat * 5)) / 5);
									}
									else
										projVal = (float)SCANUtil.getElevation(body, ((int)(lon * 5)) / 5, ((int)(lat * 5)) / 5);
								}
								elevation = palette.lerp(palette.black, palette.white, Mathf.Clamp(projVal + 1500f, 0, 9000) / 9000f);
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
					if (SCANcontroller.controller.map_ResourceOverlay && SCANcontroller.controller.GlobalResourceOverlay)
					{
						if (SCANcontroller.controller.resourceOverlayType == 0 && SCANversions.RegolithFound)
						{
							if (SCANUtil.isCovered(lon, lat, data, resource.Type)) //check our new resource coverage map
							{
								double amount = SCANUtil.RegolithOverlay(lat, lon, resource.Name, body.flightGlobalsIndex); //grab the resource amount for the current pixel
								double scalar = resource.MinValue + ((resource.MaxValue - resource.MinValue) / 5);
								amount *= 100;
								if (amount > scalar)
								{
									if (amount > 100) amount = 100; //max cutoff value
									pix[i] = palette.lerp(baseColor, palette.lerp(resource.EmptyColor, resource.FullColor, (float)(amount) / (resource.MaxValue - resource.MinValue)), 0.3f); //vary color by resource amount
								}
								else pix[i] = palette.lerp(baseColor, palette.grey, 0.4f);
							}
							else pix[i] = baseColor;
						}
						else if (SCANcontroller.controller.resourceOverlayType == 1)
						{
							if (SCANUtil.isCovered(lon, lat, data, resource.Type))
							{
								int ilon = SCANUtil.icLON(lon);
								int ilat = SCANUtil.icLAT(lat);
								float amount = data.KethaneValueMap[ilon, ilat];
								if (amount <= 0) pix[i] = palette.lerp(baseColor, palette.grey, 0.4f);
								else
								{
									pix[i] = palette.lerp(baseColor, palette.lerp(resource.EmptyColor, resource.FullColor, amount / resource.MaxValue), 0.8f);
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
