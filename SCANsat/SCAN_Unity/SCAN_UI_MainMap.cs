﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SCANsat.Unity.Interfaces;
using SCANsat.Unity;
using SCANsat.Unity.Unity;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_UI.UI_Framework;
using KSP.UI;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat.SCAN_Unity
{
	public class SCAN_UI_MainMap : ISCAN_MainMap
	{
		private bool _isVisible;
		
		private Vessel v;
		private SCANdata data;
		private SCANtype sensors;
		private Texture2D map_small;
		private Color32[] cols_height_map_small;
		private Color32[] biomeCache;
		private bool biomeBuilding;
		private double[] biomeIndex = new double[360];
		private int scanline;
		private int scanstep;
		private int updateInterval = 60;
		private int lastUpdate;
		private bool flip;

		private SCAN_MainMap uiElement;
		
		private static SCAN_UI_MainMap instance;

		public static SCAN_UI_MainMap Instance
		{
			get { return instance; }
		}

		public SCAN_UI_MainMap()
		{
			instance = this;

			map_small = new Texture2D(360, 180, TextureFormat.ARGB32, false);
			cols_height_map_small = new Color32[360];
			biomeCache = new Color32[360 * 180];

			if (palette.small_redline == null)
			{
				palette.small_redline = new Color32[360];
				for (int i = 0; i < 360; i++)
					palette.small_redline[i] = palette.Red;
			}

			GameEvents.onVesselSOIChanged.Add(soiChange);
			GameEvents.onVesselChange.Add(vesselChange);
			GameEvents.onVesselWasModified.Add(vesselChange);
		}

		public void OnDestroy()
		{
			if (uiElement != null)
			{
				uiElement.gameObject.SetActive(false);
				MonoBehaviour.Destroy(uiElement.gameObject);
			}

			GameEvents.onVesselSOIChanged.Remove(soiChange);
			GameEvents.onVesselChange.Remove(vesselChange);
			GameEvents.onVesselWasModified.Remove(vesselChange);
		}

		private void soiChange(GameEvents.HostedFromToAction<Vessel, CelestialBody> VC)
		{
			v = FlightGlobals.ActiveVessel;

			data = SCANUtil.getData(v.mainBody);

			if (data == null)
			{
				data = new SCANdata(v.mainBody);
				SCANcontroller.controller.addToBodyData(v.mainBody, data);
			}

			resetImages();

			if (uiElement != null)
				uiElement.RefreshVessels();
		}

		private void vesselChange(Vessel V)
		{
			v = FlightGlobals.ActiveVessel;

			data = SCANUtil.getData(v.mainBody);

			if (data == null)
			{
				data = new SCANdata(v.mainBody);
				SCANcontroller.controller.addToBodyData(v.mainBody, data);
			}

			resetImages();

			if (uiElement != null)
				uiElement.RefreshVessels();
		}

		public void SetScale(float scale)
		{
			if (uiElement != null)
				uiElement.SetScale(scale);
		}

		public void Update()
		{
			if (!_isVisible || data == null)
				return;

			sensors = SCANcontroller.controller.activeSensorsOnVessel(v.id);

			if (!SCANcontroller.controller.mainMapBiome)
				drawPartialMap(sensors);
			else
				drawBiomeMap(sensors);

			lastUpdate++;

			if (uiElement == null)
				return;

			//uiElement.UpdateMapTexture(map_small);

			if (lastUpdate < updateInterval)
				return;

			lastUpdate = 0;
			flip = !flip;

			SCANcontroller.SCANsensor s;

			s = SCANcontroller.controller.getSensorStatus(v, SCANtype.AltimetryLoRes);
			if (s == null)
				uiElement.UpdateLoColor(palette.grey);
			else if (!s.inRange)
				uiElement.UpdateLoColor(palette.c_bad);
			else if (!s.bestRange && flip)
				uiElement.UpdateLoColor(palette.c_bad);
			else
				uiElement.UpdateLoColor(palette.c_good);

			s = SCANcontroller.controller.getSensorStatus(v, SCANtype.AltimetryHiRes);
			if (s == null)
				uiElement.UpdateHiColor(palette.grey);
			else if (!s.inRange)
				uiElement.UpdateHiColor(palette.c_bad);
			else if (!s.bestRange && flip)
				uiElement.UpdateHiColor(palette.c_bad);
			else
				uiElement.UpdateHiColor(palette.c_good);

			s = SCANcontroller.controller.getSensorStatus(v, SCANtype.Biome);
			if (s == null)
				uiElement.UpdateMultiColor(palette.grey);
			else if (!s.inRange)
				uiElement.UpdateMultiColor(palette.c_bad);
			else if (!s.bestRange && flip)
				uiElement.UpdateMultiColor(palette.c_bad);
			else
				uiElement.UpdateMultiColor(palette.c_good);

			if (SCAN_Settings_Config.Instance.DisableStock || !SCAN_Settings_Config.Instance.InstantScan)
			{
				s = SCANcontroller.controller.getSensorStatus(v, SCANtype.FuzzyResources);
				if (s == null)
					uiElement.UpdateM700Color(palette.grey);
				else if (!s.inRange)
					uiElement.UpdateM700Color(palette.c_bad);
				else if (!s.bestRange && flip)
					uiElement.UpdateM700Color(palette.c_bad);
				else
					uiElement.UpdateM700Color(palette.c_good);

				s = SCANcontroller.controller.getSensorStatus(v, SCANtype.Ore);
				if (s == null)
					uiElement.UpdateOreColor(palette.grey);
				else if (!s.inRange)
					uiElement.UpdateOreColor(palette.c_bad);
				else if (!s.bestRange && flip)
					uiElement.UpdateOreColor(palette.c_bad);
				else
					uiElement.UpdateOreColor(palette.c_good);
			}

			if (sensors != SCANtype.Nothing)
				uiElement.UpdatePercentage(string.Format("{0:N1}%", SCANUtil.getCoveragePercentage(data, sensors)));
			else
				uiElement.UpdatePercentage("0%");
		}

		public string Version
		{
			get { return SCANmainMenuLoader.SCANsatVersion; }
		}

		public void Open()
		{
			v = FlightGlobals.ActiveVessel;

			data = SCANUtil.getData(v.mainBody);

			if (data == null)
			{
				data = new SCANdata(v.mainBody);
				SCANcontroller.controller.addToBodyData(v.mainBody, data);
			}			

			uiElement = GameObject.Instantiate(SCAN_UI_Loader.MainMapPrefab).GetComponent<SCAN_MainMap>();

			if (uiElement == null)
				return;

			uiElement.transform.SetParent(UIMasterController.Instance.mainCanvas.transform, false);

			uiElement.setMap(this);

			uiElement.UpdateMapTexture(map_small);

			_isVisible = true;
		}

		public void Close()
		{
			_isVisible = false;

			if (uiElement == null)
				return;

			uiElement.gameObject.SetActive(false);
			MonoBehaviour.Destroy(uiElement.gameObject);
		}

		public bool IsVisible
		{
			get { return _isVisible; }
			set
			{
				_isVisible = value;

				if (!value)
					Close();
			}
		}

		public bool Color
		{
			get { return SCANcontroller.controller.mainMapColor; }
			set
			{
				SCANcontroller.controller.mainMapColor = value;

				resetImages();
			}
		}

		public bool MapType
		{
			get { return SCANcontroller.controller.mainMapBiome; }
			set
			{
				SCANcontroller.controller.mainMapBiome = value;

				SCANUtil.SCANlog("Toggle Biome Map: {0}", SCANcontroller.controller.mainMapBiome);

				resetImages();
			}
		}

		public bool Minimized
		{
			get { return SCANcontroller.controller.mainMapMinimized; }
			set { SCANcontroller.controller.mainMapMinimized = value; }
		}

		public float Scale
		{
			get { return SCAN_Settings_Config.Instance.UIScale; }
		}

		public Vector2 Position
		{
			get { return SCAN_Settings_Config.Instance.MainMapPosition; }
			set { SCAN_Settings_Config.Instance.MainMapPosition = value; }
		}

		public Sprite VesselType(Guid id)
		{
			if (!SCANcontroller.controller.knownVessels.Contains(id))
				return null;

			Vessel sv = SCANcontroller.controller.knownVessels[id].vessel;

			return null;
		}

		public Vector2 VesselPosition(Guid id)
		{
			if (!SCANcontroller.controller.knownVessels.Contains(id))
				return new Vector2();

			Vessel sv = SCANcontroller.controller.knownVessels[id].vessel;

			double lon = SCANUtil.fixLon(sv.longitude);
			double lat = SCANUtil.fixLat(sv.latitude);
			
			lon = lon * ((360 * Scale * GameSettings.UI_SCALE) / 360f);
			lat = (180 * Scale * GameSettings.UI_SCALE) - lat * ((180 * Scale * GameSettings.UI_SCALE) / 180f);

			return new Vector2((float)lon, (float)lat);
		}

		public Dictionary<Guid, MapLabelInfo> VesselInfoList
		{
			get
			{
				Dictionary<Guid, MapLabelInfo> vessels = new Dictionary<Guid, MapLabelInfo>();

				vessels.Add(v.id, new MapLabelInfo()
				{
					label = "",
					image = VesselType(v.id),
					pos = new Vector2()
				});

				for (int i = SCANcontroller.controller.Known_Vessels.Count - 1; i >= 0; i--)
				{
					SCANcontroller.SCANvessel sv = SCANcontroller.controller.Known_Vessels[0];

					if (sv.vessel == v)
						continue;

					if (sv.vessel.mainBody != v.mainBody)
						continue;

					vessels.Add(sv.vessel.id, new MapLabelInfo()
					{
						label = "",
						image = VesselType(sv.vessel.id),
						pos = new Vector2()
					});
				}

				return vessels;
			}
		}

		public void ClampToScreen(RectTransform rect)
		{
			UIMasterController.ClampToScreen(rect, Vector2.zero);
		}

		public void OpenBigMap()
		{
			if (SCAN_UI_BigMap.Instance.IsVisible)
				SCAN_UI_BigMap.Instance.Close();
			else
				SCAN_UI_BigMap.Instance.Open();
		}

		public void OpenZoomMap()
		{
			SCANcontroller.controller.zoomMap.Visible = !SCANcontroller.controller.zoomMap.Visible;
			if (SCANcontroller.controller.zoomMap.Visible && !SCANcontroller.controller.zoomMap.Initialized)
				SCANcontroller.controller.zoomMap.initializeMap();
		}

		public void OpenOverlay()
		{
			if (SCAN_UI_Overlay.Instance.IsVisible)
				SCAN_UI_Overlay.Instance.Close();
			else
				SCAN_UI_Overlay.Instance.Open();
		}

		public void OpenInstruments()
		{
			if (SCAN_UI_Instruments.Instance.IsVisible)
				SCAN_UI_Instruments.Instance.Close();
			else
				SCAN_UI_Instruments.Instance.Open();
		}

		public void OpenSettings()
		{
			if (SCAN_UI_Settings.Instance.IsVisible)
				SCAN_UI_Settings.Instance.Close();
			else
				SCAN_UI_Settings.Instance.Open();
		}

		public string VesselInfo(Guid id)
		{
			if (!SCANcontroller.controller.knownVessels.Contains(id))
				return "";

			Vessel sv = SCANcontroller.controller.knownVessels[id].vessel;

			float lon = (float)SCANUtil.fixLonShift(sv.longitude);
			float lat = (float)SCANUtil.fixLatShift(sv.latitude);

			string units = "";
			if (SCANcontroller.controller.mainMapBiome)
			{
				if (SCANUtil.isCovered(lon, lat, data, SCANtype.Biome))
					units = string.Format("; {0}", SCANUtil.getBiomeName(data.Body, lon, lat));
			}
			else
			{
				if (SCANUtil.isCovered(lon, lat, data, SCANtype.Altimetry))
				{
					if (SCANUtil.isCovered(lon, lat, data, SCANtype.AltimetryHiRes))
					{
						float alt = sv.heightFromTerrain;

						if (alt < 0)
							alt = (float)sv.altitude;

						units = string.Format("; {0}", SCANuiUtil.distanceString(alt, 100000, 100000000));
					}
					else
					{
						float alt = sv.heightFromTerrain;

						if (alt < 0)
							alt = (float)sv.altitude;

						alt = ((int)(alt / 500)) * 500;

						units = string.Format("; {0}", SCANuiUtil.distanceString(alt, 100000, 100000000));
					}
				}
			}

			return string.Format("({0:F1}°,{1:F1}°{2})", lat, lon, units);
		}
		
		private void drawPartialMap(SCANtype type)
		{
			bool pqsController = data.Body.pqsController != null;

			if (data.ControllerBuilding || data.OverlayBuilding)
				return;

			if (!data.Built)
			{
				if (!data.MapBuilding)
				{
					scanline = 0;
					scanstep = 0;
				}

				data.MapBuilding = true;
				data.generateHeightMap(ref scanline, ref scanstep, 360);
				return;
			}

			int scheme = SCANcontroller.controller.colours;

			for (int ilon = 0; ilon < 360; ilon++)
			{
				if (!pqsController)
				{
					cols_height_map_small[ilon] = palette.lerp(palette.black, palette.white, UnityEngine.Random.value);
					continue;
				}

				Color32 c = palette.Grey;
				float val = data.HeightMapValue(data.Body.flightGlobalsIndex, ilon, scanline);
				if (SCANUtil.isCovered(ilon, scanline, data, SCANtype.Altimetry))
				{
					if (SCANUtil.isCovered(ilon, scanline, data, SCANtype.AltimetryHiRes))
						c = palette.heightToColor(val, scheme, data.TerrainConfig);
					else
						c = palette.heightToColor(val, 1, data.TerrainConfig);
				}
				else
				{
					if (scanline % 30 == 0 && ilon % 3 == 0)
					{
						c = palette.White;
					}
					else if (ilon % 30 == 0 && scanline % 3 == 0)
					{
						c = palette.White;
					}
				}

				if (type != SCANtype.Nothing)
				{
					if (!SCANUtil.isCoveredByAll(ilon, scanline, data, type))
					{
						c = palette.lerp(c, palette.Black, 0.5f);
					}
				}

				cols_height_map_small[ilon] = c;
			}

			map_small.SetPixels32(0, scanline, 360, 1, cols_height_map_small);

			if (scanline < 179)
				map_small.SetPixels32(0, scanline + 1, 360, 1, palette.small_redline);

			scanline++;

			if (scanline >= 180)
				scanline = 0;

			map_small.Apply();
		}

		private void drawBiomeMap(SCANtype type)
		{
			bool biomeMap = data.Body.BiomeMap != null;

			if (biomeBuilding)
				buildBiomeCache();

			int scheme = SCANcontroller.controller.colours;

			for (int ilon = 0; ilon < 360; ilon++)
			{
				if (!biomeMap)
				{
					cols_height_map_small[ilon] = palette.lerp(palette.black, palette.white, UnityEngine.Random.value);
					continue;
				}

				Color32 c = biomeCache[scanline * 360 + ilon];
				if (!SCANUtil.isCovered(ilon, scanline, data, SCANtype.Biome))
				{
					c = palette.Grey;
				}

				if (type != SCANtype.Nothing)
				{
					if (!SCANUtil.isCoveredByAll(ilon, scanline, data, type))
					{
						c = palette.lerp(c, palette.Black, 0.5f);
					}
				}

				cols_height_map_small[ilon] = c;
			}

			map_small.SetPixels32(0, scanline, 360, 1, cols_height_map_small);

			if (scanline < 179)
				map_small.SetPixels32(0, scanline + 1, 360, 1, palette.small_redline);

			scanline++;

			if (scanline >= 180)
				scanline = 0;

			map_small.Apply();
		}

		private void buildBiomeCache()
		{
			for (int i = 0; i < 360; i++)
			{
				double index = SCANUtil.getBiomeIndexFraction(data.Body, i - 180, scanline - 90);
				Color32 c = palette.Grey;

				if (SCANcontroller.controller.biomeBorder && ((i > 0 && biomeIndex[i - 1] != index) || (scanline > 0 && biomeIndex[i] != index)))
				{
					c = palette.White;
				}
				else if (SCANcontroller.controller.useStockBiomes)
				{
					c = SCANUtil.getBiome(data.Body, i - 180, scanline - 90).mapColor;
				}
				else
				{
					c = palette.lerp(SCANcontroller.controller.lowBiomeColor32, SCANcontroller.controller.highBiomeColor32, (float)index);
				}

				biomeCache[scanline * 360 + i] = c;

				biomeIndex[i] = index;
			}

			if (scanline >= 179)
				biomeBuilding = false;
		}

		internal void resetImages()
		{
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

			if (SCANcontroller.controller.mainMapBiome)
			{
				biomeBuilding = true;
				scanline = 0;
			}
		}
	}
}