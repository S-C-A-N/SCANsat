using System;
using System.Collections.Generic;
using System.Linq;
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
		private bool _inputLock;
		private string _sensorCount = "";
		private Vector2 _position;
		private const string controlLock = "SCANsatSettings";

		private SCAN_Settings uiElement;

		private SCAN_UI_Color colorInterface;

		private static SCAN_UI_Settings instance;

		public static SCAN_UI_Settings Instance
		{
			get { return instance; }
		}

		public SCAN_UI_Settings()
		{
			instance = this;

			colorInterface = new SCAN_UI_Color();
		}

		public int Page
		{
			get
			{
				if (uiElement != null)
					return uiElement.Page;

				return 0;
			}
		}

		public void OnDestroy()
		{
			if (uiElement != null)
			{
				uiElement.gameObject.SetActive(false);
				MonoBehaviour.Destroy(uiElement.gameObject);
			}
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

		public void Open(int page = 0, bool savePosition = false)
		{
			uiElement = GameObject.Instantiate(SCAN_UI_Loader.SettingsPrefab).GetComponent<SCAN_Settings>();

			if (uiElement == null)
				return;

			uiElement.transform.SetParent(UIMasterController.Instance.mainCanvas.transform, false);

			uiElement.setSettings(this, page);

			if (!savePosition)
				_position = new Vector2(0, 100);

			uiElement.SetPosition(_position);

			_isVisible = true;

			if (HighLogic.LoadedSceneIsFlight && SCAN_Settings_Config.Instance.StockToolbar && SCAN_Settings_Config.Instance.ToolbarMenu)
			{
				if (SCANappLauncher.Instance != null && SCANappLauncher.Instance.UIElement != null)
					SCANappLauncher.Instance.UIElement.SetSettingsToggle(true);
			}
		}

		public void Close()
		{
			_isVisible = false;

			if (uiElement == null)
				return;

			uiElement.FadeOut();

			uiElement.FadeOut();

			if (HighLogic.LoadedSceneIsFlight && SCAN_Settings_Config.Instance.StockToolbar && SCAN_Settings_Config.Instance.ToolbarMenu)
			{
				if (SCANappLauncher.Instance != null && SCANappLauncher.Instance.UIElement != null)
					SCANappLauncher.Instance.UIElement.SetSettingsToggle(false);
			}

			if (_inputLock)
				InputLockManager.RemoveControlLock(controlLock);
		}

		public string Version
		{
			get { return SCANmainMenuLoader.SCANsatVersion; }
		}

		public string SensorCount
		{
			get { return _sensorCount; }
		}

		public string DataResetCurrent
		{
			get { return string.Format(SCANconfigLoader.languagePack.warningDataResetCurrent, getTargetBody().theName); }
		}

		public string DataResetAll
		{
			get { return SCANconfigLoader.languagePack.warningDataResetAll; }
		}

		public string SCANResourceResetCurrent
		{
			get { return string.Format(SCANconfigLoader.languagePack.warningSCANResourceResetCurrent, getTargetBody().theName); }
		}

		public string SCANResourceResetAll
		{
			get { return SCANconfigLoader.languagePack.warningSCANResourceResetAll; }
		}

		public string StockResourceResetCurrent
		{
			get { return string.Format(SCANconfigLoader.languagePack.warningStockResourceResetCurrent, getTargetBody().theName); }
		}

		public string StockResourceResetAll
		{
			get { return SCANconfigLoader.languagePack.warningStockResourceResetAll; }
		}

		public string ModuleManagerWarning
		{
			get { return SCANconfigLoader.languagePack.warningModuleManagerResource; }
		}

		public string SaveToConfig
		{
			get { return SCANconfigLoader.languagePack.warningSaveToConfig; }
		}

		public int TimeWarp
		{
			get { return SCAN_Settings_Config.Instance.TimeWarpResolution; }
			set { SCAN_Settings_Config.Instance.TimeWarpResolution = value; }
		}

		public int MapWidth
		{
			get { return SCAN_Settings_Config.Instance.BigMapWidth; }
			set
			{
				SCAN_Settings_Config.Instance.BigMapWidth = value;

				if (SCAN_UI_BigMap.Instance != null && SCAN_UI_BigMap.Instance.IsVisible)
				{
					SCAN_UI_BigMap.Instance.Size = new Vector2(value, value / 2);

					SCAN_UI_BigMap.Instance.SetMapSize();
				}
			}
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
			set
			{
				SCAN_Settings_Config.Instance.UIScale = value;

				uiElement.SetScale(value);

				if (SCAN_UI_BigMap.Instance != null && SCAN_UI_BigMap.Instance.IsVisible)
					SCAN_UI_BigMap.Instance.SetScale(value);

				if (SCAN_UI_MainMap.Instance != null && SCAN_UI_MainMap.Instance.IsVisible)
					SCAN_UI_MainMap.Instance.SetScale(value);

				if (SCAN_UI_Instruments.Instance != null && SCAN_UI_Instruments.Instance.IsVisible)
					SCAN_UI_Instruments.Instance.SetScale(value);

				if (SCAN_UI_Overlay.Instance != null && SCAN_UI_Overlay.Instance.IsVisible)
					SCAN_UI_Overlay.Instance.SetScale(value);
			}
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
			set
			{
				SCAN_Settings_Config.Instance.WindowTooltips = value;

				SCAN_UI_Loader.ToggleTooltips(value);

				if (SCAN_UI_BigMap.Instance != null && SCAN_UI_BigMap.Instance.IsVisible)
					SCAN_UI_BigMap.Instance.ProcessTooltips();

				if (SCAN_UI_MainMap.Instance != null && SCAN_UI_MainMap.Instance.IsVisible)
					SCAN_UI_MainMap.Instance.ProcessTooltips();

				if (SCAN_UI_Instruments.Instance != null && SCAN_UI_Instruments.Instance.IsVisible)
					SCAN_UI_Instruments.Instance.ProcessTooltips();

				if (SCAN_UI_Overlay.Instance != null && SCAN_UI_Overlay.Instance.IsVisible)
					SCAN_UI_Overlay.Instance.ProcessTooltips();

				if (SCAN_Settings_Config.Instance.StockToolbar && SCAN_Settings_Config.Instance.ToolbarMenu
					&& SCANappLauncher.Instance != null && SCANappLauncher.Instance.IsVisible)
					SCANappLauncher.Instance.ProcessTooltips();
			}			
		}

		public bool MapGenSpeed
		{
			get { return SCAN_Settings_Config.Instance.SlowMapGeneration; }
			set { SCAN_Settings_Config.Instance.SlowMapGeneration = value; }
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
			set
			{
				SCAN_Settings_Config.Instance.ToolbarMenu = value;

				if (SCANappLauncher.Instance != null)
					SCANappLauncher.Instance.ToggleToolbarType();
			}
		}

		public bool StockUIStyle
		{
			get { return SCAN_Settings_Config.Instance.StockUIStyle; }
			set
			{
				SCAN_Settings_Config.Instance.StockUIStyle = value;

				SCAN_UI_Loader.ResetUIStyle();

				if (SCAN_UI_BigMap.Instance != null && SCAN_UI_BigMap.Instance.IsVisible)
				{
					SCAN_UI_BigMap.Instance.Close();
					SCAN_UI_BigMap.Instance.Open();
				}

				if (SCAN_UI_MainMap.Instance != null && SCAN_UI_MainMap.Instance.IsVisible)
				{ 
					SCAN_UI_MainMap.Instance.Close();
					SCAN_UI_MainMap.Instance.Open();
				}

				if (SCAN_UI_ZoomMap.Instance != null && SCAN_UI_ZoomMap.Instance.IsVisible)
				{
					SCAN_UI_ZoomMap.Instance.Close();
					SCAN_UI_ZoomMap.Instance.Open(true);
				}

				if (SCAN_UI_Instruments.Instance != null && SCAN_UI_Instruments.Instance.IsVisible)
				{
					SCAN_UI_Instruments.Instance.Close();
					SCAN_UI_Instruments.Instance.Open();
				}

				if (SCAN_UI_Overlay.Instance != null && SCAN_UI_Overlay.Instance.IsVisible)
				{
					SCAN_UI_Overlay.Instance.Close();
					SCAN_UI_Overlay.Instance.Open();
				}

				Close();
				Open(0, true);
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
			get { return SCAN_Settings_Config.Instance.DisableStockResource; }
			set { SCAN_Settings_Config.Instance.DisableStockResource = value; }
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
			get { return SCAN_Settings_Config.Instance.DisableStockResource || !SCAN_Settings_Config.Instance.InstantScan; }
		}

		public bool ShowStockReset
		{
			get { return SCAN_Settings_Config.Instance.DisableStockResource || !SCAN_Settings_Config.Instance.InstantScan; }
		}

		public bool ShowMapFill
		{
			get { return SCAN_Settings_Config.Instance.CheatMapFill; }
		}

		public bool LockInput
		{
			get { return _inputLock; }
			set
			{
				_inputLock = value;

				if (_inputLock)
					InputLockManager.SetControlLock(controlLock);
				else
					InputLockManager.RemoveControlLock(controlLock);
			}
		}

		public bool ModuleManager
		{
			get { return SCANmainMenuLoader.MMLoaded; }
		}

		public Canvas TooltipCanvas
		{
			get { return UIMasterController.Instance.tooltipCanvas; }
		}

		public Vector2 Position
		{
			set { _position = value; }
		}
		
		public IList<string> BackgroundBodies
		{
			get	{ return new List<string>(SCANcontroller.controller.GetAllData.Select(d => d.Body.bodyName)); }
		}

		public ISCAN_Color ColorInterface
		{
			get { return colorInterface; }
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

		public void ResetSCANResourceCurrent()
		{
			CelestialBody thisBody = getTargetBody();

			SCANdata data = SCANUtil.getData(thisBody);

			if (data != null)
				data.resetResources();
		}

		public void ResetSCANResourceAll()
		{
			foreach (SCANdata data in SCANcontroller.controller.GetAllData)
				data.resetResources();
		}

		public void ResetStockResourceCurrent()
		{
			CelestialBody thisBody = getTargetBody();

			var resources = ResourceScenario.Instance.gameSettings.GetPlanetScanInfo();

			resources.RemoveAll(a => a.PlanetId == thisBody.flightGlobalsIndex);
		}

		public void ResetStockResourceAll()
		{
			ResourceScenario.Instance.gameSettings.GetPlanetScanInfo().Clear();
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

		public void ResetWindows()
		{
			if (HighLogic.LoadedSceneIsFlight)
			{
				SCANuiUtil.resetMainMapPos();
				SCANuiUtil.resetBigMapPos();
				SCANuiUtil.resetInstUIPos();
				SCANuiUtil.resetColorMapPos();
				SCANuiUtil.resetOverlayControllerPos();
				SCANuiUtil.resetZoomMapPos();
			}
			else
			{
				SCANuiUtil.resetBigMapPos();
				SCANuiUtil.resetColorMapPos();
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

		public double BodyPercentage(string bodyName)
		{
			SCANdata data = SCANUtil.getData(bodyName);

			if (data == null)
				return 0;

			return SCANUtil.getCoveragePercentage(data, SCANtype.Nothing) / 100;
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
