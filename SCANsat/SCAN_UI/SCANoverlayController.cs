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
using SCANsat.SCAN_Data;
using SCANsat.SCAN_UI.UI_Framework;
using SCANsat.SCAN_Platform;
using UnityEngine;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat.SCAN_UI
{
	class SCANoverlayController : SCAN_MBW
	{
		internal readonly static Rect defaultRect = new Rect(Screen.width - 280, 200, 175, 100);
		private static Rect sessionRect = defaultRect;
		private CelestialBody body;
		private SCANdata data;
		private SCANresourceGlobal currentResource;
		private List<SCANresourceGlobal> resources;
		private List<PResource.Resource> resourceFractions;
		private bool drawOverlay;
		private bool oldOverlay;
		private bool terrainGenerated;
		private bool mapGenerating;
		private int selection;
		private double degreeOffset;
		private bool enableUI = true;
		private int mapStep, mapStart;

		private int timer;
		private string tooltipText = "";

		private Texture2D mapOverlay;
		private Texture2D biomeOverlay;
		private Texture2D terrainOverlay;
		private Color32[] resourcePixels;
		private Color32[] biomePixels;
		private Color32[] terrainPixels;
		private float[,] abundanceValues;
		private float[,] terrainValues;

		protected override void Awake()
		{
			WindowCaption = "  S.C.A.N. Overlay";
			WindowRect = sessionRect;
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

		protected override void OnDestroy()
		{
			GameEvents.onShowUI.Remove(showUI);
			GameEvents.onHideUI.Remove(hideUI);
		}

		public bool DrawOverlay
		{
			get { return drawOverlay; }
		}

		protected override void DrawWindowPre(int id)
		{

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
					OverlayGenerator.Instance.ClearDisplay();
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
					if (mapGenerating)
						return;

					OverlayGenerator.Instance.ClearDisplay();

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

			if (GUILayout.Button("Biome Map", selection == (resources.Count) ? SCANskins.SCAN_labelLeftActive : SCANskins.SCAN_labelLeft))
			{
				if (mapGenerating)
					return;

				OverlayGenerator.Instance.ClearDisplay();

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

			if (GUILayout.Button("Terrain Map", selection == (resources.Count + 1) ? SCANskins.SCAN_labelLeftActive : SCANskins.SCAN_labelLeft))
			{
				if (mapGenerating)
					return;

				OverlayGenerator.Instance.ClearDisplay();

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
						double elevation = SCANUtil.getElevation(body, coords.longitude, coords.latitude);

						tooltipText += string.Format("\nTerrain: {0}", SCANuiUtil.getMouseOverElevation(coords.longitude, coords.latitude, data, 0));

						tooltipText += string.Format("\nSlope: {0:F1}°", SCANUtil.slope(elevation, body, coords.longitude, coords.latitude, degreeOffset));
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
					if (SCANcontroller.controller.needsNarrowBand)
					{
						bool coverage = false;
						bool scanner = false;

						foreach (Vessel vessel in FlightGlobals.Vessels)
						{
							if (vessel.protoVessel.protoPartSnapshots.Count <= 1)
								continue;

							if (vessel.vesselType == VesselType.Debris || vessel.vesselType == VesselType.Unknown || vessel.vesselType == VesselType.EVA || vessel.vesselType == VesselType.Flag)
								continue;

							if (vessel.mainBody != body)
								continue;

							if (vessel.situation != Vessel.Situations.ORBITING)
								continue;

							if (inc(vessel.orbit.inclination) < Math.Abs(coords.latitude))
							{
								coverage = true;
								continue;
							}

							var scanners = from pref in vessel.protoVessel.protoPartSnapshots
										   where pref.modules.Any(a => a.moduleName == "ModuleResourceScanner")
										   select pref;

							if (scanners.Count() == 0)
								continue;

							foreach (var p in scanners)
							{
								if (p.partInfo == null)
									continue;

								ConfigNode node = p.partInfo.partConfig;

								if (node == null)
									continue;

								var moduleNodes = from nodes in node.GetNodes("MODULE")
												  where nodes.GetValue("name") == "ModuleResourceScanner"
												  select nodes;

								foreach (ConfigNode moduleNode in moduleNodes)
								{
									if (moduleNode == null)
										continue;

									if (moduleNode.GetValue("ScannerType") != "0")
										continue;

									if (moduleNode.GetValue("ResourceName") != currentResource.Name)
										continue;

									if (moduleNode.HasValue("MaxAbundanceAltitude") && !vessel.Landed)
									{
										string alt = moduleNode.GetValue("MaxAbundanceAltitude");
										float f = 0;
										if (!float.TryParse(alt, out f))
											continue;

										if (f < vessel.altitude)
										{
											coverage = true;
											continue;
										}
									}

									coverage = false;
									scanner = true;
									break;
								}
								if (scanner)
									break;
							}
							if (scanner)
								break;
						}

						if (coverage)
							tooltipText += string.Format("\n{0}: No Coverage", currentResource.Name);
						else if (!scanner)
							tooltipText += string.Format("\n{0}: No Scanner", currentResource.Name);
						else
							resourceLabel(ref tooltipText, fuzzy, coords.latitude, coords.longitude);
					}
					else
						resourceLabel(ref tooltipText, fuzzy, coords.latitude, coords.longitude);
				}

				drawToolTipLabel();
			}
		}

		private void resourceLabel(ref string t, bool fuzz, double lat, double lon)
		{
			if (fuzz)
				t += string.Format("\n{0}: {1:P0}", currentResource.Name, SCANUtil.ResourceOverlay(lat, lon, currentResource.Name, body, SCANcontroller.controller.resourceBiomeLock));
			else
				t += string.Format("\n{0}: {1:P2}", currentResource.Name, SCANUtil.ResourceOverlay(lat, lon, currentResource.Name, body, SCANcontroller.controller.resourceBiomeLock));
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

		public void refreshMap(float t, int height, int interp)
		{
			SCANcontroller.controller.overlayTransparency = t;
			SCANcontroller.controller.overlayMapHeight = height;
			SCANcontroller.controller.overlayInterpolation = interp;
			if (drawOverlay)
				refreshMap();
		}

		private void refreshMap()
		{
			if (mapGenerating)
				return;

			if (selection == resources.Count)
				body.SetResourceMap(SCANuiUtil.drawBiomeMap(ref biomeOverlay, ref biomePixels, data, SCANcontroller.controller.overlayTransparency, SCANcontroller.controller.overlayMapHeight * 2));
			else if (selection == resources.Count + 1)
				StartCoroutine(setTerrainMap());
			else if (selection == resources.Count + 2)
				StartCoroutine(setSlopeMap());
			else
				body.SetResourceMap(SCANuiUtil.drawResourceTexture(ref mapOverlay, ref resourcePixels, ref abundanceValues, SCANcontroller.controller.overlayMapHeight, data, currentResource, SCANcontroller.controller.overlayInterpolation, SCANcontroller.controller.overlayTransparency));
		}

		private IEnumerator setTerrainMap()
		{
			if (data.Body.pqsController == null)
				yield return null;

			int timer = 0;

			while (!data.Built && timer < 2000)
			{
				mapGenerating = true;
				if (!data.Building)
				{
					data.ExternalBuilding = true;
					data.generateHeightMap(ref mapStep, ref mapStart, 360);
				}
				timer++;
				yield return null;
			}

			mapGenerating = false;

			if (timer >= 2000)
				yield return null;

			if (!terrainGenerated)
			{
				SCANuiUtil.generateTerrainArray(ref terrainValues, 720, 4, data);
				terrainGenerated = true;
			}

			body.SetResourceMap(SCANuiUtil.drawTerrainMap(ref terrainOverlay, ref terrainPixels, ref terrainValues, data, 720, 4));
		}

		private IEnumerator setSlopeMap()
		{
			if (data.Body.pqsController == null)
				yield return null;

			int timer = 0;

			while (!data.Built && timer < 2000)
			{
				mapGenerating = true;
				if (!data.Building)
				{
					data.ExternalBuilding = true;
					data.generateHeightMap(ref mapStep, ref mapStart, 360);
				}
				timer++;
				yield return null;
			}

			mapGenerating = false;

			if (timer >= 2000)
				yield return null;

			if (!terrainGenerated)
			{
				SCANuiUtil.generateTerrainArray(ref terrainValues, 720, 4, data);
				terrainGenerated = true;
			}

			body.SetResourceMap(SCANuiUtil.drawSlopeMap(ref terrainOverlay, ref terrainPixels, ref terrainValues, data, 720, 4));
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
