#region license
/*  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANoverlayController - Window to control the planetary overlay maps
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 DMagic
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_UI.UI_Framework;
using SCANsat.SCAN_Platform;
using UnityEngine;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat.SCAN_UI
{
	class SCANoverlayController : SCAN_MBW
	{
		internal static Rect defaultRect = new Rect(Screen.width - 320, 200, 175, 100);
		private static Rect sessionRect = defaultRect;
		private CelestialBody body;
		private SCANdata data;
		private SCANresourceGlobal currentResource;
		private List<SCANresourceGlobal> resources;
		//private List<PResource.Resource> resourceFractions;
		private bool drawOverlay;
		private bool oldOverlay;

		private bool mapGenerating;
		private int selection;
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

		protected override void Awake()
		{
			base.Awake();

			WindowCaption = "  S.C.A.N. Overlay";
			WindowRect = sessionRect;
			if ((WindowRect.x * SCANcontroller.controller.windowScale) > (Screen.width - 100))
				WindowRect.x /= SCANcontroller.controller.windowScale;
			WindowStyle = SCANskins.SCAN_window;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(175), GUILayout.Height(100) };
			Visible = false;
			DragEnabled = true;
			ClampToScreenOffset = new RectOffset(-120, -120, -100, -100);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		protected override void Start()
		{
			GameEvents.onShowUI.Add(showUI);
			GameEvents.onHideUI.Add(hideUI);
			GameEvents.onGameSceneSwitchRequested.Add(switchScene);

			resources = SCANcontroller.setLoadedResourceList();

			setBody(HighLogic.LoadedSceneIsFlight ? FlightGlobals.currentMainBody : Planetarium.fetch.Home);
		}

		protected override void Update()
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

		private void switchScene(GameEvents.FromToAction<GameScenes, GameScenes> FT)
		{
			removeOverlay();
		}

		protected override void OnDestroy()
		{
			GameEvents.onShowUI.Remove(showUI);
			GameEvents.onHideUI.Remove(hideUI);
			GameEvents.onGameSceneSwitchRequested.Remove(switchScene);

			resourcePixels = null;
			biomePixels = null;
			terrainPixels = null;
			abundanceValues = null;
			terrainValues = null;
		}

		public bool DrawOverlay
		{
			get { return drawOverlay; }
		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);				/* Standard version label and close button */
			closeBox(id);

			drawResourceList(id);
			overlayToggle(id);
			overlayOptions(id);
			resourceSettings(id);
		}

		protected override void DrawWindowPost(int id)
		{
			if (oldOverlay != drawOverlay)
			{
				oldOverlay = drawOverlay;
				if (oldOverlay)
					refreshMap();
				else
					removeOverlay();
			}

			sessionRect = WindowRect;
		}

		protected override void OnGUIEvery()
		{
			if (enableUI)
				mouseOverToolTip();
		}

		//Draw the version label in the upper left corner
		private void versionLabel(int id)
		{
			Rect r = new Rect(4, 0, 50, 18);
			GUI.Label(r, SCANmainMenuLoader.SCANsatVersion, SCANskins.SCAN_whiteReadoutLabel);
		}

		//Draw the close button in the upper right corner
		private void closeBox(int id)
		{
			Rect r = new Rect(WindowRect.width - 20, 1, 18, 18);
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				Visible = false;
			}
		}

		private void drawResourceList(int id)
		{
			for (int i = 0; i < resources.Count; i++)
			{
				SCANresourceGlobal r = resources[i];

				if (r == null)
					continue;

				if (GUILayout.Button(r.Name, selection == i ? SCANskins.SCAN_labelLeftActive : SCANskins.SCAN_labelLeft))
				{
					if (mapGenerating || threadRunning)
						return;

					removeOverlay();

					if (selection != i)
					{
						selection = i;
						currentResource = r;
						currentResource.CurrentBodyConfig(body.name);
						oldOverlay = drawOverlay = true;
						refreshMap();
						return;
					}

					if (drawOverlay)
					{
						oldOverlay = drawOverlay = false;
					}
					else
					{
						oldOverlay = drawOverlay = true;
						refreshMap();
					}
				}
			}

			if (bodyBiome)
			{
				if (GUILayout.Button("Biome Map", selection == (resources.Count) ? SCANskins.SCAN_labelLeftActive : SCANskins.SCAN_labelLeft))
				{
					if (mapGenerating || threadRunning)
						return;

					removeOverlay();

					if (selection != resources.Count)
					{
						selection = resources.Count;
						oldOverlay = drawOverlay = true;
						refreshMap();
						return;
					}

					if (drawOverlay)
					{
						oldOverlay = drawOverlay = false;
					}
					else
					{
						oldOverlay = drawOverlay = true;
						refreshMap();
					}
				}
			}

			if (bodyPQS)
			{
				if (GUILayout.Button("Terrain Map", selection == (resources.Count + 1) ? SCANskins.SCAN_labelLeftActive : SCANskins.SCAN_labelLeft))
				{
					if (mapGenerating || threadRunning)
						return;

					removeOverlay();

					if (selection != resources.Count + 1)
					{
						selection = resources.Count + 1;
						oldOverlay = drawOverlay = true;
						refreshMap();
						return;
					}

					if (drawOverlay)
					{
						oldOverlay = drawOverlay = false;
					}
					else
					{
						oldOverlay = drawOverlay = true;
						refreshMap();
					}
				}
			}

			//if (GUILayout.Button("Slope Map", selection == (resources.Count + 2) ? SCANskins.SCAN_labelLeftActive : SCANskins.SCAN_labelLeft))
			//{
			//	if (mapGenerating)
			//		return;

			//	OverlayGenerator.Instance.ClearDisplay();

			//	if (selection != resources.Count + 2)
			//	{
			//		selection = resources.Count + 2;
			//		oldOverlay = drawOverlay = true;
			//		refreshMap();
			//		return;
			//	}

			//	if (drawOverlay)
			//	{
			//		oldOverlay = drawOverlay = false;
			//	}
			//	else
			//	{
			//		oldOverlay = drawOverlay = true;
			//		refreshMap();
			//	}
			//}
		}

		private void overlayToggle(int id)
		{
			drawOverlay = GUILayout.Toggle(drawOverlay, "Draw Overlay", SCANskins.SCAN_settingsToggle);
		}

		private void overlayOptions(int id)
		{
			if (!drawOverlay)
				return;

			if (mapGenerating)
				return;

			if (GUILayout.Button("Refresh"))
				refreshMap();

			if (selection >= resources.Count)
				return;
		}

		private void resourceSettings(int id)
		{
			fillS();

			SCANcontroller.controller.planetaryOverlayTooltips = GUILayout.Toggle(SCANcontroller.controller.planetaryOverlayTooltips, "Tooltips", SCANskins.SCAN_settingsToggle);

			if (GUILayout.Button("Resource Settings"))
			{
				SCANcontroller.controller.resourceSettings.Visible = !SCANcontroller.controller.resourceSettings.Visible;
			}

			//if (GUILayout.Button("Biome Summary"))
			//{
			//	foreach (ResourceCache.AbundanceSummary a in ResourceCache.Instance.AbundanceCache)
			//	{
			//		if (a.ResourceName == "Ore" && a.HarvestType == HarvestTypes.Planetary)
			//			SCANUtil.SCANlog("{0}: For {1} on Body {2} of scanner type {3}: Abundance = {4:P3}", a.ResourceName, a.BiomeName, a.BodyId, a.HarvestType, a.Abundance);
			//	}
			//}
		}

		private void removeOverlay()
		{
			OverlayGenerator.Instance.ClearDisplay();

			if (mapOverlay != null)
				Destroy(mapOverlay);

			mapOverlay = null;
		}

		private void mouseOverToolTip()
		{
			if (!drawOverlay)
				return;

			if (SCANcontroller.controller == null)
				return;

			if (!SCANcontroller.controller.planetaryOverlayTooltips)
				return;

			if ((MapView.MapIsEnabled && HighLogic.LoadedSceneIsFlight && FlightGlobals.ready) || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				if (SCANUtil.MouseIsOverWindow())
					return;

				SCANUtil.SCANCoordinates coords = SCANUtil.GetMouseCoordinates(body);

				if (coords == null)
					return;

				if (timer < 5)
				{
					timer++;
					drawToolTipLabel();
					return;
				}

				timer = 0;
				tooltipText = "";

				tooltipText += coords.ToDMS();

				if (body.pqsController != null)
				{
					if (SCANUtil.isCovered(coords.longitude, coords.latitude, data, SCANtype.Altimetry))
					{
						tooltipText += string.Format("\nTerrain: {0}", SCANuiUtil.getMouseOverElevation(coords.longitude, coords.latitude, data, 0));

						tooltipText += string.Format("\nSlope: {0:F1}°", SCANUtil.slope(SCANUtil.getElevation(body, coords.longitude, coords.latitude), body, coords.longitude, coords.latitude, degreeOffset));
					}
				}

				if (body.BiomeMap != null)
				{
					if (SCANUtil.isCovered(coords.longitude, coords.latitude, data, SCANtype.Biome))
						tooltipText += string.Format("\nBiome: {0}", SCANUtil.getBiomeName(body, coords.longitude, coords.latitude));
				}

				bool resources = false;
				bool fuzzy = false;

				if (SCANUtil.isCovered(coords.longitude, coords.latitude, data, currentResource.SType))
				{
					resources = true;
				}
				else if (SCANUtil.isCovered(coords.longitude, coords.latitude, data, SCANtype.FuzzyResources))
				{
					resources = true;
					fuzzy = true;
				}

				if (resources)
				{
					tooltipText += "\n" + SCANuiUtil.getResourceAbundance(body, coords.latitude, coords.longitude, fuzzy, currentResource);
				}

				drawToolTipLabel();
			}
		}

		private void drawToolTipLabel()
		{
			Vector2 size = SCANskins.SCAN_readoutLabelCenter.CalcSize(new GUIContent(tooltipText));

			float sizeX = size.x;
			if (sizeX < 160)
				sizeX = 160;
			else if (sizeX < 190)
				sizeX = 190;

			Rect r = new Rect(Event.current.mousePosition.x - (sizeX / 2), Event.current.mousePosition.y - (size.y + 16), sizeX + 10, size.y + 8);

			GUI.Box(r, "");

			r.x += 5;
			r.y += 4;
			r.width -= 10;
			r.height -= 8;

			SCANuiUtil.drawLabel(r, tooltipText, SCANskins.SCAN_readoutLabelCenter, true, SCANskins.SCAN_shadowReadoutLabelCenter);
		}

		public void refreshMap(float t, int height, int interp, int biomeHeight)
		{
			SCANcontroller.controller.overlayTransparency = t;
			SCANcontroller.controller.overlayMapHeight = height;
			SCANcontroller.controller.overlayInterpolation = interp;
			SCANcontroller.controller.overlayBiomeHeight = biomeHeight;
			if (drawOverlay)
				refreshMap();
		}

		private void refreshMap()
		{
			if (mapGenerating)
				return;
			if (threadRunning)
				return;

			if (selection == resources.Count)
				body.SetResourceMap(SCANuiUtil.drawBiomeMap(ref mapOverlay, ref biomePixels, data, SCANcontroller.controller.overlayTransparency, SCANcontroller.controller.overlayBiomeHeight));
			else if (selection == resources.Count + 1)
				StartCoroutine(setTerrainMap());
			else if (selection == resources.Count + 2)
				StartCoroutine(setSlopeMap());
			else
				StartCoroutine(setOverlayMap());
		}

		private IEnumerator setOverlayMap()
		{
			int timer = 0;

			mapGenerating = true;

			SCANuiUtil.generateOverlayResourceValues(ref abundanceValues, SCANcontroller.controller.overlayMapHeight, data, currentResource, SCANcontroller.controller.overlayInterpolation);

			SCANdata copy = new SCANdata(data);
			SCANresourceGlobal resourceCopy = new SCANresourceGlobal(currentResource);
			resourceCopy.CurrentBodyConfig(body.name);

			Thread t = new Thread(() => resourceThreadRun(SCANcontroller.controller.overlayMapHeight, SCANcontroller.controller.overlayInterpolation, SCANcontroller.controller.overlayTransparency, new System.Random(ResourceScenario.Instance.gameSettings.Seed), copy, resourceCopy));
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

			if (mapOverlay == null || mapOverlay.height != SCANcontroller.controller.overlayMapHeight)
				mapOverlay = new Texture2D(SCANcontroller.controller.overlayMapHeight * 2, SCANcontroller.controller.overlayMapHeight, TextureFormat.ARGB32, true);

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

			Thread t = new Thread( () => terrainThreadRun(copy, index));
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

		private IEnumerator setSlopeMap()
		{
			if (data.Body.pqsController == null)
				yield break;

			int timer = 0;

			while (!data.Built && timer < 2000)
			{
				mapGenerating = true;
				if (!data.ControllerBuilding && !data.MapBuilding)
				{
					data.OverlayBuilding = true;
					data.generateHeightMap(ref mapStep, ref mapStart, 360);
				}
				timer++;
				yield return null;
			}

			mapGenerating = false;

			if (timer >= 2000)
				yield break;

			if (!terrainGenerated)
			{
				SCANuiUtil.generateTerrainArray(ref terrainValues, 720, 4, data, data.Body.flightGlobalsIndex);
				terrainGenerated = true;
			}

			body.SetResourceMap(SCANuiUtil.drawSlopeMap(ref mapOverlay, ref terrainPixels, ref terrainValues, data, 720, 4));
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

			if (drawOverlay)
				refreshMap();

			double circum = body.Radius * 2 * Math.PI;
			double eqDistancePerDegree = circum / 360;
			degreeOffset = 5 / eqDistancePerDegree;

			//resourceFractions = ResourceMap.Instance.GetResourceItemList(HarvestTypes.Planetary, body);
			//if (resources.Count > 0)
			//{
			//	currentResource = resources[0];
			//	currentResource.CurrentBodyConfig(body.name);

			//	//foreach (SCANresourceGlobal r in resources)
			//	//{
			//	//	SCANresourceBody b = r.getBodyConfig(body.name, false);
			//	//	if (b != null)
			//	//	{
			//	//		b.Fraction = resourceFractions.FirstOrDefault(a => a.resourceName == r.Name).fraction;
			//	//	}
			//	//}
			//}
		}

		private double inc(double d)
		{
			if (d > 90)
				d = 180 - d;

			return d;
		}

		private void showUI()
		{
			enableUI = true;
		}

		private void hideUI()
		{
			enableUI = false;
		}
	}
}
