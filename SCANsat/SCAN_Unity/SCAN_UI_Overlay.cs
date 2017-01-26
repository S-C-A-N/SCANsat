using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using UnityEngine;
using SCANsat.Unity.Interfaces;
using SCANsat.Unity.Unity;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_UI.UI_Framework;
using KSP.UI;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat.SCAN_Unity
{
	public class SCAN_UI_Overlay : ISCAN_Overlay
	{
		private bool _isVisible;
		private bool _overlayOn;

		private CelestialBody body;
		private SCANdata data;
		private SCANresourceGlobal currentResource;
		private List<SCANresourceGlobal> resources;

		private bool mapGenerating;
		private double degreeOffset;
		private bool enableUI = true;
		private int mapStep, mapStart;
		private bool bodyBiome, bodyPQS;

		private int timer;
		//These are read/written on multiple threads; we use volatile to ensure that cached values are not used when reading the value
		private volatile bool threadRunning, threadFinished;
		private volatile bool terrainGenerated;

		private string tooltipText = "";

		private Texture2D mapOverlay;
		private Color32[] resourcePixels;
		private Color32[] biomePixels;
		private Color32[] terrainPixels;
		private float[,] abundanceValues;
		private float[,] terrainValues;

		private SCAN_Overlay uiElement;

		private static SCAN_UI_Overlay instance;

		public static SCAN_UI_Overlay Instance
		{
			get { return instance; }
		}

		public SCAN_UI_Overlay()
		{
			instance = this;

			resources = SCANcontroller.setLoadedResourceList();

			setBody(HighLogic.LoadedSceneIsFlight ? FlightGlobals.currentMainBody : Planetarium.fetch.Home);

			GameEvents.onGameSceneSwitchRequested.Add(switchScene);
		}

		public void OnDestroy()
		{
			if (uiElement != null)
			{
				uiElement.gameObject.SetActive(false);
				MonoBehaviour.Destroy(uiElement.gameObject);
			}

			GameEvents.onGameSceneSwitchRequested.Remove(switchScene);
		}

		private void switchScene(GameEvents.FromToAction<GameScenes, GameScenes> FT)
		{
			removeOverlay();
		}

		public void SetScale(float scale)
		{
			if (uiElement != null)
				uiElement.SetScale(scale);
		}

		public void Update()
		{
			if ((MapView.MapIsEnabled && HighLogic.LoadedSceneIsFlight && FlightGlobals.ready) || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				CelestialBody mapBody = SCANUtil.getTargetBody(MapView.MapCamera.target);

				if (mapBody == null)
					return;

				if (mapBody != body)
					setBody(mapBody);
			}
			else if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ready)
			{
				if (body != FlightGlobals.currentMainBody)
					setBody(FlightGlobals.currentMainBody);
			}
		}

		public void Open()
		{
			uiElement = GameObject.Instantiate(SCAN_UI_Loader.OverlayPrefab).GetComponent<SCAN_Overlay>();

			if (uiElement == null)
				return;

			uiElement.transform.SetParent(UIMasterController.Instance.mainCanvas.transform, false);

			uiElement.SetOverlay(this);

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

		public string CurrentResource
		{
			get { return currentResource == null ? "" : currentResource.Name; }
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

		public bool Tooltips
		{
			get;
			set;
		}

		public bool DrawOverlay
		{
			get { return _overlayOn; }
			set
			{
				if (value)
				{
					if (!_overlayOn)
						refreshMap(SCANcontroller.controller.overlaySelection);
				}
				else
					removeOverlay();
			}
		}

		public bool DrawBiome
		{
			get { return SCANcontroller.controller.overlaySelection == 0; }
			set
			{
				if (!value)
				{
					if (_overlayOn && SCANcontroller.controller.overlaySelection == 0)
						removeOverlay();

					return;
				}

				SCANcontroller.controller.overlaySelection = 0;

				refreshMap(0);
			}
		}

		public bool DrawTerrain
		{
			get { return SCANcontroller.controller.overlaySelection == 1; }
			set
			{
				if (!value)
				{
					if (_overlayOn && SCANcontroller.controller.overlaySelection == 1)
						removeOverlay();

					return;
				}

				SCANcontroller.controller.overlaySelection = 1;

				refreshMap(1);
			}
		}

		public bool DrawResource
		{
			get { return SCANcontroller.controller.overlaySelection == 2; }
		}

		public float Scale
		{
			get { return SCAN_Settings_Config.Instance.UIScale; }
		}

		public IList<string> Resources
		{
			get { return new List<string>(resources.Select(r => r.Name)); }
		}

		public Vector2 Position
		{
			get { return SCAN_Settings_Config.Instance.OverlayPosition; }
			set { SCAN_Settings_Config.Instance.OverlayPosition = value; }
		}

		public void ClampToScreen(RectTransform rect)
		{
			UIMasterController.ClampToScreen(rect, Vector2.zero);
		}

		public void SetResource(string resource, bool isOn)
		{
			if (!isOn)
			{ 
				if(_overlayOn && SCANcontroller.controller.overlaySelection == 2 && currentResource != null && currentResource.Name == resource)
					removeOverlay();

				return;
			}

			SCANcontroller.controller.overlaySelection = 2;

			if (currentResource.Name != resource)
			{
				for (int i = resources.Count - 1; i >= 0; i--)
				{
					SCANresourceGlobal r = resources[i];

					if (r.Name != resource)
						continue;

					currentResource = r;
					break;
				}
			}

			if (currentResource == null)
				return;

			SCANcontroller.controller.overlayResource = resource;

			refreshMap(2);
		}

		public void Refresh()
		{
			_overlayOn = true;

			refreshMap(SCANcontroller.controller.overlaySelection, false);
		}

		public void OpenSettings()
		{
			if (SCAN_UI_Settings.Instance.IsVisible)
			{
				if (SCAN_UI_Settings.Instance.Page == 2)
					SCAN_UI_Settings.Instance.Close();
				else
				{
					SCAN_UI_Settings.Instance.Close();
					SCAN_UI_Settings.Instance.Open(2, true);
				}
			}
			else
				SCAN_UI_Settings.Instance.Open(2);
		}

		private void setBody(CelestialBody B)
		{
			body = B;

			data = SCANUtil.getData(body);
			if (data == null)
			{
				data = new SCANdata(body);
				SCANcontroller.controller.addToBodyData(body, data);
			}

			if (currentResource == null)
			{
				if (resources.Count > 0)
				{
					for (int i = resources.Count - 1; i >= 0; i--)
					{
						SCANresourceGlobal r = resources[i];

						if (r.Name != SCANcontroller.controller.overlayResource)
							continue;

						currentResource = r;
						break;
					}

					if (currentResource == null)
						currentResource = resources[0];

					currentResource.CurrentBodyConfig(body.name);
				}
			}
			else
			{
				currentResource.CurrentBodyConfig(body.name);
			}

			bodyBiome = body.BiomeMap != null;
			bodyPQS = body.pqsController != null;

			terrainGenerated = false;

			if (_overlayOn)
				refreshMap(SCANcontroller.controller.overlaySelection);

			double circum = body.Radius * 2 * Math.PI;
			double eqDistancePerDegree = circum / 360;
			degreeOffset = 5 / eqDistancePerDegree;
		}

		private void removeOverlay()
		{
			_overlayOn = false;

			OverlayGenerator.Instance.ClearDisplay();

			if (mapOverlay != null)
				MonoBehaviour.Destroy(mapOverlay);

			mapOverlay = null;
		}

		public void refreshMap(float t, int height, int interp, int biomeHeight)
		{
			if (_overlayOn)
				refreshMap(SCANcontroller.controller.overlaySelection);
		}

		private void refreshMap(int i, bool remove = true)
		{
			if (remove)
				removeOverlay();

			if (mapGenerating)
				return;
			if (threadRunning)
				return;

			_overlayOn = true;

			switch(i)
			{
				case 0:
					body.SetResourceMap(SCANuiUtil.drawBiomeMap(ref mapOverlay, ref biomePixels, data, SCAN_Settings_Config.Instance.CoverageTransparency, SCAN_Settings_Config.Instance.BiomeMapHeight));
					break;
				case 1:
					SCANcontroller.controller.StartCoroutine(setTerrainMap());
					break;
				case 2:
					SCANcontroller.controller.StartCoroutine(setOverlayMap());
					break;
				default:
					break;
			}
		}

		private IEnumerator setOverlayMap()
		{
			int timer = 0;

			mapGenerating = true;

			SCANuiUtil.generateOverlayResourceValues(ref abundanceValues, SCAN_Settings_Config.Instance.ResourceMapHeight, data, currentResource, SCAN_Settings_Config.Instance.Interpolation);

			SCANdata copy = new SCANdata(data);
			SCANresourceGlobal resourceCopy = new SCANresourceGlobal(currentResource);
			resourceCopy.CurrentBodyConfig(body.name);

			Thread t = new Thread(() => resourceThreadRun(SCAN_Settings_Config.Instance.ResourceMapHeight, SCAN_Settings_Config.Instance.Interpolation, SCAN_Settings_Config.Instance.CoverageTransparency, new System.Random(ResourceScenario.Instance.gameSettings.Seed), copy, resourceCopy));
			threadRunning = true;
			threadFinished = false;
			t.Start();

			while (threadRunning && timer < 1000)
			{
				timer++;
				yield return null;
			}

			mapGenerating = false;
			copy = null;
			resourceCopy = null;

			if (timer >= 1000)
			{
				Debug.LogError("[SCANsat] Something went wrong when drawing the SCANsat resource map overlay...");
				t.Abort();
				threadRunning = false;
				yield break;
			}

			if (!threadFinished)
			{
				Debug.LogError("[SCANsat] Something went wrong when drawing the SCANsat resource map overlay...");
				yield break;
			}

			if (mapOverlay == null || mapOverlay.height != SCAN_Settings_Config.Instance.ResourceMapHeight)
				mapOverlay = new Texture2D(SCAN_Settings_Config.Instance.ResourceMapHeight * 2, SCAN_Settings_Config.Instance.ResourceMapHeight, TextureFormat.ARGB32, true);

			mapOverlay.SetPixels32(resourcePixels);
			mapOverlay.Apply();

			body.SetResourceMap(mapOverlay);
		}

		private void resourceThreadRun(int height, int step, float transparent, System.Random r, SCANdata copyData, SCANresourceGlobal copyResource)
		{
			try
			{
				SCANuiUtil.generateOverlayResourcePixels(ref resourcePixels, ref abundanceValues, height, copyData, copyResource, r, step, transparent);
				threadFinished = true;
			}
			catch
			{
				threadFinished = false;
			}
			finally
			{
				threadRunning = false;
			}
		}

		private IEnumerator setTerrainMap()
		{
			if (data.Body.pqsController == null)
				yield break;

			int timer = 0;

			while (!data.Built && timer < 2000)
			{
				mapGenerating = true;
				if (!data.ControllerBuilding && !data.MapBuilding)
				{
					if (!data.OverlayBuilding)
					{
						mapStep = 0;
						mapStart = 0;
					}

					data.OverlayBuilding = true;
					data.generateHeightMap(ref mapStep, ref mapStart, 360);
				}
				timer++;
				yield return null;
			}

			if (timer >= 2000)
			{
				mapGenerating = false;
				yield break;
			}

			timer = 0;

			SCANdata copy = new SCANdata(data);
			int index = data.Body.flightGlobalsIndex;

			Thread t = new Thread(() => terrainThreadRun(copy, index));
			threadFinished = false;
			threadRunning = true;
			t.Start();

			while (threadRunning && timer < 1000)
			{
				timer++;
				yield return null;
			}

			mapGenerating = false;
			copy = null;

			if (timer >= 1000)
			{
				Debug.LogError("[SCANsat] Something went wrong when drawing the SCANsat terrain map overlay...");
				t.Abort();
				threadRunning = false;
				yield break;
			}

			if (!threadFinished)
			{
				Debug.LogError("[SCANsat] Something went wrong when drawing the SCANsat terrain map overlay...");
				yield break;
			}

			if (mapOverlay == null)
				mapOverlay = new Texture2D(1440, 720, TextureFormat.ARGB32, true);

			mapOverlay.SetPixels32(terrainPixels);
			mapOverlay.Apply();

			body.SetResourceMap(mapOverlay);
		}

		private void terrainThreadRun(SCANdata copyData, int i)
		{
			try
			{
				if (!terrainGenerated)
				{
					SCANuiUtil.generateTerrainArray(ref terrainValues, 720, 4, copyData, i);
					terrainGenerated = true;
				}

				SCANuiUtil.drawTerrainMap(ref terrainPixels, ref terrainValues, copyData, 720, 4);

				threadFinished = true;
			}
			catch
			{
				threadFinished = false;
			}
			finally
			{
				threadRunning = false;
			}
		}

		public void ResetPosition()
		{
			SCAN_Settings_Config.Instance.OverlayPosition = new Vector2(600, -200);

			if (uiElement != null)
				uiElement.SetPosition(SCAN_Settings_Config.Instance.OverlayPosition);
		}

	}
}
