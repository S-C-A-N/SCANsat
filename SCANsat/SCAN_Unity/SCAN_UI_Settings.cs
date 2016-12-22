using System;
using System.Collections.Generic;
using UnityEngine;
using SCANsat.Unity.Interfaces;
using SCANsat.Unity.Unity;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Toolbar;
using SCANsat.SCAN_UI.UI_Framework;
using KSP.UI;

namespace SCANsat.SCAN_Unity
{
	public class SCAN_UI_Settings : ISCAN_Settings
	{
		private bool _isVisible;
		private string _sensorCount = "";

		private SCAN_Settings uiElement;

		private static SCAN_UI_Settings instance;

		public static SCAN_UI_Settings Instance
		{
			get { return instance; }
		}

		public SCAN_UI_Settings()
		{
			instance = this;
		}

		public void OnDestroy()
		{

		}

		public void Update()
		{
			if (!_isVisible)
				return;

			_sensorCount = string.Format("Vessels: {0} Sensors: {1} Passes: {2}"
				, SCANcontroller.controller.ActiveVessels
				, SCANcontroller.controller.ActiveSensors
				, SCANcontroller.controller.ActualPasses);
		}

		public void Open(int page = 0)
		{
			uiElement = GameObject.Instantiate(SCAN_UI_Loader.SettingsPrefab).GetComponent<SCAN_Settings>();

			if (uiElement == null)
				return;

			uiElement.transform.SetParent(UIMasterController.Instance.mainCanvas.transform, false);

			uiElement.setSettings(this, page);

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

		public string Version
		{
			get { return SCANmainMenuLoader.SCANsatVersion; }
		}

		public string SensorCount
		{
			get { return _sensorCount; }
		}

		public int TimeWarp
		{
			get { return SCAN_Settings_Config.Instance.TimeWarpResolution; }
			set { SCAN_Settings_Config.Instance.TimeWarpResolution = value; }
		}

		public int MapWidth
		{
			get { return SCAN_Settings_Config.Instance.BigMapWidth; }
			set { SCAN_Settings_Config.Instance.BigMapWidth = value; }
		}

		public int Interpolation
		{
			get { return SCAN_Settings_Config.Instance.Interpolation; }
			set { SCAN_Settings_Config.Instance.Interpolation = value; }
		}

		public int MapHeight
		{
			get { return SCAN_Settings_Config.Instance.ResourceMapHeight; }
			set { SCAN_Settings_Config.Instance.ResourceMapHeight = value; }
		}

		public int BiomeMapHeight
		{
			get { return SCAN_Settings_Config.Instance.BiomeMapHeight; }
			set { SCAN_Settings_Config.Instance.BiomeMapHeight = value; }
		}

		public float Transparency
		{
			get { return SCAN_Settings_Config.Instance.CoverageTransparency; }
			set { SCAN_Settings_Config.Instance.CoverageTransparency = value; }
		}

		public float StockThresholdValue
		{
			get { return SCAN_Settings_Config.Instance.StockTreshold; }
			set { SCAN_Settings_Config.Instance.StockTreshold = value; }
		}

		public float UIScale
		{
			get { return SCAN_Settings_Config.Instance.UIScale; }
			set { SCAN_Settings_Config.Instance.UIScale = value; }
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

		public bool BackgroundScanning
		{
			get { return SCAN_Settings_Config.Instance.BackgroundScanning; }
			set { SCAN_Settings_Config.Instance.BackgroundScanning = value; }
		}

		public bool GroundTracks
		{
			get { return SCAN_Settings_Config.Instance.ShowGroundTracks; }
			set { SCAN_Settings_Config.Instance.ShowGroundTracks = value; }
		}

		public bool ActiveGround
		{
			get { return SCAN_Settings_Config.Instance.GroundTracksActiveOnly; }
			set { SCAN_Settings_Config.Instance.GroundTracksActiveOnly = value; }
		}

		public bool OverlayTooltips
		{
			get { return SCAN_Settings_Config.Instance.OverlayTooltips; }
			set { SCAN_Settings_Config.Instance.OverlayTooltips = value; }
		}

		public bool WindowTooltips
		{
			get { return SCAN_Settings_Config.Instance.WindowTooltips; }
			set { SCAN_Settings_Config.Instance.WindowTooltips = value; }			
		}

		public bool StockToolbar
		{
			get { return SCAN_Settings_Config.Instance.StockToolbar; }
			set
			{
				SCAN_Settings_Config.Instance.StockToolbar = value;

				if (value)
					SCANcontroller.controller.appLauncher = SCANcontroller.controller.gameObject.AddComponent<SCANappLauncher>();
				else
				{
					MonoBehaviour.Destroy(SCANcontroller.controller.appLauncher);
					SCANcontroller.controller.appLauncher = null;
				}
			}
		}

		public bool ToolbarMenu
		{
			get { return SCAN_Settings_Config.Instance.ToolbarMenu; }
			set { SCAN_Settings_Config.Instance.ToolbarMenu = value; }
		}

		public bool StockUIStyle
		{
			get { return SCAN_Settings_Config.Instance.StockUIStyle; }
			set
			{
				SCAN_Settings_Config.Instance.StockUIStyle = value;

				SCAN_UI_Loader.ResetUIStyle();
			}
		}

		public bool BiomeLock
		{
			get { return SCAN_Settings_Config.Instance.BiomeLock; }
			set { SCAN_Settings_Config.Instance.BiomeLock = value; }
		}

		public bool NarrowBand
		{
			get { return SCAN_Settings_Config.Instance.RequireNarrowBand; }
			set { SCAN_Settings_Config.Instance.RequireNarrowBand = value; }
		}

		public bool InstantScan
		{
			get { return SCAN_Settings_Config.Instance.InstantScan; }
			set { SCAN_Settings_Config.Instance.InstantScan = value; }
		}

		public bool DisableStock
		{
			get { return SCAN_Settings_Config.Instance.DisableStock; }
			set { SCAN_Settings_Config.Instance.DisableStock = value; }
		}

		public bool StockThreshold
		{
			get { return SCAN_Settings_Config.Instance.UseStockTreshold; }
			set { SCAN_Settings_Config.Instance.UseStockTreshold = value; }
		}

		public bool GreyScale
		{
			get { return SCAN_Settings_Config.Instance.TrueGreyScale; }
			set { SCAN_Settings_Config.Instance.TrueGreyScale = value; }
		}

		public bool ExportCSV
		{
			get { return SCAN_Settings_Config.Instance.ExportCSV; }
			set { SCAN_Settings_Config.Instance.ExportCSV = value; }
		}

		public bool ShowSCANsatReset
		{
			get { return SCAN_Settings_Config.Instance.DisableStock || !SCAN_Settings_Config.Instance.InstantScan; }
		}

		public bool ShowStockReset
		{
			get { return SCAN_Settings_Config.Instance.DisableStock || !SCAN_Settings_Config.Instance.InstantScan; }
		}

		public bool ShowMapFill
		{
			get { return SCAN_Settings_Config.Instance.CheatMapFill; }
		}

		public void ClampToScreen(RectTransform rect)
		{
			UIMasterController.ClampToScreen(rect, Vector2.zero);
		}

		public void ResetCurrent()
		{
			CelestialBody thisBody = getTargetBody();

			SCANdata data = SCANUtil.getData(thisBody);

			if (data != null)
				data.reset();
		}

		public void ResetAll()
		{
			foreach (SCANdata data in SCANcontroller.controller.GetAllData)
				data.reset();
		}

		public void ResetSCANResource()
		{
			CelestialBody thisBody = getTargetBody();

			SCANdata data = SCANUtil.getData(thisBody);

			if (data != null)
				data.resetResources();
		}

		public void ResetStockResource()
		{
			CelestialBody thisBody = getTargetBody();

			var resources = ResourceScenario.Instance.gameSettings.GetPlanetScanInfo();

			resources.RemoveAll(a => a.PlanetId == thisBody.flightGlobalsIndex);
		}

		public void FillCurrent()
		{
			CelestialBody thisBody = getTargetBody();

			SCANdata data = SCANUtil.getData(thisBody);
			if (data == null)
			{
				data = new SCANdata(thisBody);
				SCANcontroller.controller.addToBodyData(thisBody, data);
			}
			data.fillMap();
		}

		public void FillAll()
		{
			foreach (CelestialBody b in FlightGlobals.Bodies)
			{
				SCANdata data = SCANUtil.getData(b);
				if (data == null)
				{
					data = new SCANdata(b);
					SCANcontroller.controller.addToBodyData(b, data);
				}
				data.fillMap();
			}
		}

		public void OpenColor()
		{

		}

		public void ResetWindows()
		{
			if (HighLogic.LoadedSceneIsFlight)
			{
				SCANuiUtil.resetMainMapPos();
				SCANuiUtil.resetBigMapPos();
				SCANuiUtil.resetInstUIPos();
				SCANuiUtil.resetColorMapPos();
				SCANuiUtil.resetResourceSettingPos();
				SCANuiUtil.resetOverlayControllerPos();
				SCANuiUtil.resetZoomMapPos();
			}
			else
			{
				SCANuiUtil.resetKSCMapPos();
				SCANuiUtil.resetColorMapPos();
				SCANuiUtil.resetSettingsUIPos();
				SCANuiUtil.resetZoomMapPos();
				if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
					SCANuiUtil.resetOverlayControllerPos();
			}
		}

		public void ToggleBody(string name)
		{
			SCANdata data = SCANUtil.getData(name);

			if (data != null)
				data.Disabled = !data.Disabled;
		}

		public string BodyPercentage(string bodyName)
		{
			SCANdata data = SCANUtil.getData(bodyName);

			if (data == null)
				return "";

			return string.Format("{0} ({1:N1}%)", data.Body.bodyName, SCANUtil.getCoveragePercentage(data, SCANtype.Nothing));
		}

		private CelestialBody getTargetBody()
		{
			switch (HighLogic.LoadedScene)
			{
				case GameScenes.FLIGHT:
					return FlightGlobals.currentMainBody;
				case GameScenes.SPACECENTER:
					return Planetarium.fetch.Home;
				case GameScenes.TRACKSTATION:
					return SCANUtil.getTargetBody(MapView.MapCamera.target);
				default:
					return null;
			}
		}

	}
}
