#region license
/* 
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - Color Selection and Settings Menu
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 *
 */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Map;
using SCANsat.SCAN_UI.UI_Framework;
using SCANsat.SCAN_Platform;
using SCANsat.SCAN_Platform.Palettes;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;
using UnityEngine;

namespace SCANsat.SCAN_UI
{
	class SCANcolorSelection: SCAN_MBW
	{
		private bool dropDown, paletteBox, resourceBox;
		private bool reversePalette, oldReverseState, discretePalette, oldDiscreteState;
		private bool spaceCenterLock, trackingStationLock, clampState, oldClampState;
		private Rect ddRect;
		private Palette dataPalette;
		private int paletteIndex;
		private SCANmapLegend currentLegend, previewLegend;

		private SCANuiSlider minTerrainSlider, maxTerrainSlider, clampTerrainSlider, paletteSizeSlider, resourceMinSlider, resourceMaxSlider, resourceTransSlider, biomeTransSlider;

		private float hueSlider = 50f;

		private int windowMode = 0;
		private SCANresource currentResource;
		private Color resourceColorFull, resourceColorEmpty;
		private bool stockBiomes = false;
		private Color biomeColorLow, biomeColorHigh;
		private const string lockID = "colorLockID";
		internal static Rect defaultRect = new Rect(100, 400, 650, 330);

		//SCAN_MBW objects to sync the color selection fields to the currently displayed map
		private SCANkscMap kscMapObj;
		private SCANBigMap bigMapObj;

		private static SCANmap bigMap;
		private SCANdata data;

		private Texture2D minColorPreview = null;
		private Texture2D minColorOld = null;
		private Texture2D maxColorPreview = null;
		private Texture2D maxColorOld = null;

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Color Management";
			WindowRect = defaultRect;
			WindowStyle = SCANskins.SCAN_window;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(650), GUILayout.Height(300) };
			Visible = false;
			DragEnabled = true;
			ClampToScreenOffset = new RectOffset(-450, -450, -250, -250);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");

			InputLockManager.RemoveControlLock(lockID);
		}

		internal override void Start()
		{
			setSizeSlider(palette.CurrentPalette.kind);

			if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				kscMapObj = (SCANkscMap)SCANcontroller.controller.kscMap;
				if (SCANkscMap.BigMap != null)
					bigMap = SCANkscMap.BigMap;
				if (kscMapObj.Data != null)
					data = kscMapObj.Data;
			}
			else if (HighLogic.LoadedSceneIsFlight)
			{
				bigMapObj = (SCANBigMap)SCANcontroller.controller.BigMap;
				if (SCANBigMap.BigMap != null)
					bigMap = SCANBigMap.BigMap;
				if (bigMapObj.Data != null)
					data = bigMapObj.Data;
			}

			minTerrainSlider = new SCANuiSlider(data.DefaultMinHeight - 10000, data.MaxHeight - 100, data.MinHeight, "Min: ", "m");
			maxTerrainSlider = new SCANuiSlider(data.MinHeight + 100, data.DefaultMaxHeight + 10000, data.MaxHeight, "Max: ", "m");
			clampTerrainSlider = new SCANuiSlider(data.MinHeight + 10, data.MaxHeight - 10, data.ClampHeight ?? data.MinHeight + 10, "Clamp: ", "m");
			paletteSizeSlider = new SCANuiSlider(3, 12, data.PaletteSize, "Palette Size: ", "");

			biomeTransSlider = new SCANuiSlider(0, 100, SCANcontroller.controller.biomeTransparency, "Terrain Trans: ", "%");

			if (SCANcontroller.controller.GlobalResourceOverlay)
			{
				currentResource = SCANcontroller.controller.ResourceList[data.Body.name].ElementAt(0).Value;
				resourceMinSlider = new SCANuiSlider(0, 10, currentResource.MinValue, "Min: ", "%");
				resourceMaxSlider = new SCANuiSlider(1, 100, currentResource.MaxValue, "Max: ", "%");
				resourceTransSlider = new SCANuiSlider(0, 100, currentResource.Transparency, "Trans: ", "%");
			}

			if (windowMode > 3 || (windowMode > 2 && !SCANcontroller.controller.GlobalResourceOverlay))
				windowMode = 0;
		}

		internal override void OnDestroy()
		{
			InputLockManager.RemoveControlLock(lockID);
		}

		protected override void DrawWindowPre(int id)
		{
			//Some clumsy logic is used here to ensure that the color selection fields always remain in sync with the current map in each scene
			if (HighLogic.LoadedSceneIsFlight)
			{
				if (data == null)
				{
					data = SCANUtil.getData(FlightGlobals.currentMainBody);
					if (data == null)
					{
						data = new SCANdata(FlightGlobals.currentMainBody);
						SCANcontroller.controller.addToBodyData(FlightGlobals.currentMainBody, data);
					}
				}
				if (bigMapObj.Visible && SCANBigMap.BigMap != null)
				{
					data = bigMapObj.Data;
					bigMap = SCANBigMap.BigMap;
				}
				else if (data.Body != FlightGlobals.currentMainBody)
				{
					data = SCANUtil.getData(FlightGlobals.currentMainBody);
					if (data == null)
					{
						data = new SCANdata(FlightGlobals.currentMainBody);
						SCANcontroller.controller.addToBodyData(FlightGlobals.currentMainBody, data);
					}
				}
				if (bigMap == null)
				{
					if (SCANBigMap.BigMap != null)
					{
						bigMap = SCANBigMap.BigMap;
					}
				}
			}

			//Lock space center click through - Sync SCANdata
			else if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				if (data == null)
				{
					data = SCANUtil.getData(Planetarium.fetch.Home);
					if (data == null)
					{
						data = new SCANdata(Planetarium.fetch.Home);
						SCANcontroller.controller.addToBodyData(Planetarium.fetch.Home, data);
					}
				}
				if (kscMapObj.Visible)
				{
					data = kscMapObj.Data;
					bigMap = SCANkscMap.BigMap;
				}
				else if (data.Body != Planetarium.fetch.Home)
				{
					data = SCANUtil.getData(Planetarium.fetch.Home);
					if (data == null)
					{
						data = new SCANdata(Planetarium.fetch.Home);
						SCANcontroller.controller.addToBodyData(Planetarium.fetch.Home, data);
					}
				}
				if (bigMap == null)
				{
					if (SCANkscMap.BigMap != null)
					{
						bigMap = SCANkscMap.BigMap;
					}
				}
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && !spaceCenterLock)
				{
					InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS | ControlTypes.KSC_ALL, lockID);
					spaceCenterLock = true;
				}
				else if (!WindowRect.Contains(mousePos) && spaceCenterLock)
				{
					InputLockManager.RemoveControlLock(lockID);
					spaceCenterLock = false;
				}
			}

			//Lock tracking scene click through - Sync SCANdata
			else if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				if (data == null)
				{
					data = SCANUtil.getData(Planetarium.fetch.Home);
					if (data == null)
					{
						data = new SCANdata(Planetarium.fetch.Home);
						SCANcontroller.controller.addToBodyData(Planetarium.fetch.Home, data);
					}
				}
				if (kscMapObj.Visible)
				{
					data = kscMapObj.Data;
					bigMap = SCANkscMap.BigMap;
				}
				else if (data.Body != Planetarium.fetch.Home)
				{
					data = SCANUtil.getData(Planetarium.fetch.Home);
					if (data == null)
					{
						data = new SCANdata(Planetarium.fetch.Home);
						SCANcontroller.controller.addToBodyData(Planetarium.fetch.Home, data);
					}
				}
				if (bigMap == null)
				{
					if (SCANkscMap.BigMap != null)
					{
						bigMap = SCANkscMap.BigMap;
					}
				}
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && !trackingStationLock)
				{
					InputLockManager.SetControlLock(ControlTypes.TRACKINGSTATION_UI, lockID);
					trackingStationLock = true;
				}
				else if (!WindowRect.Contains(mousePos) && trackingStationLock)
				{
					InputLockManager.RemoveControlLock(lockID);
					trackingStationLock = false;
				}
			}

			//This updates all of the fields whenever the palette selection is changed; very ugly...
			if (windowMode == 0 && (currentLegend == null || data.ColorPalette != dataPalette))
			{
				dataPalette = data.ColorPalette;
				minTerrainSlider.CurrentValue = data.MinHeight;
				maxTerrainSlider.CurrentValue = data.MaxHeight;
				minTerrainSlider.valueChanged();
				maxTerrainSlider.valueChanged();
				setTerrainSliders();
				paletteSizeSlider.CurrentValue = data.PaletteSize;
				paletteSizeSlider.valueChanged();
				setSizeSlider(dataPalette.kind);
				oldReverseState = reversePalette = data.PaletteReverse;
				oldDiscreteState = discretePalette = data.PaletteDiscrete;
				oldClampState = clampState = data.ClampHeight != null;
				clampTerrainSlider.CurrentValue = data.ClampHeight ?? minTerrainSlider.CurrentValue + 10;
				palette.CurrentPalettes = palette.setCurrentPalettesType(dataPalette.kind);
				palette.CurrentPalette = palette.CurrentPalettes.availablePalettes[0];
				regenPaletteSets();
				drawCurrentLegend();
			}

			if (windowMode == 0 && previewLegend == null)
			{
				drawPreviewLegend();
			}

			if (!dropDown)
			{
				paletteBox = false;
				resourceBox = false;
			}
		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);					/* The standard version number and close button */
			closeBox(id);

			growS();
				windowTabs(id);					/* Draws the window selection tabs across the top */
				if (windowMode == 0)
				{
					growE();
					paletteTextures(id);		/* Draws the palette selection button and preview swatches */
					paletteOptions(id);			/* All of the terrain and palette options */
					stopE();
					fillS(8);
					growE();
					palettePreview(id);			/* Draws the two preview palette legends */
					fillS(10);
					paletteConfirmation(id);	/* The buttons for default, apply, and cancel */
					stopE();
				}
				else if (windowMode == 1)
				{
					growE();
						colorWheel(id);
					stopE();
				}
				else if (windowMode == 2)
				{
					growE();
						colorWheel(id);
						growS();
							biomeOptions(id);
							biomeConfirm(id);
						stopS();
					stopE();
				}
				else if (windowMode == 3 && SCANcontroller.controller.GlobalResourceOverlay)
				{
					growE();
						colorWheel(id);
						growS();
							resourceOptions(id);
							resourceConfirm(id);
						stopS();
					stopE();
				}
				else
					windowMode = 0;
			stopS();

			dropDownBox(id);				/* Draw the drop down menu for the palette selection box */
		}

		protected override void DrawWindowPost(int id)
		{
			if (paletteBox && Event.current.type == EventType.mouseDown && !ddRect.Contains(Event.current.mousePosition))
			{
				dropDown = false;
				paletteBox = false;
			}

			//These methods update all of the UI elements whenever any of the options are changed
			if (reversePalette != oldReverseState)
			{
				oldReverseState = reversePalette;
				drawPreviewLegend();
			}

			if (minTerrainSlider.valueChanged() || maxTerrainSlider.valueChanged())
			{
				setTerrainSliders();
			}

			if (discretePalette != oldDiscreteState)
			{
				oldDiscreteState = discretePalette;
				drawPreviewLegend();
			}

			if (clampState != oldClampState)
			{
				oldClampState = clampState;
				drawPreviewLegend();
			}

			if (paletteSizeSlider.valueChanged())
			{
				regenPaletteSets();
				palette.CurrentPalette = palette.CurrentPalettes.availablePalettes[paletteIndex];
				drawPreviewLegend();
			}
		}

		//Draw the version label in the upper left corner
		private void versionLabel(int id)
		{
			Rect r = new Rect(6, 0, 50, 18);
			GUI.Label(r, SCANversions.SCANsatVersion, SCANskins.SCAN_whiteReadoutLabel);
		}

		//Draw the close button in the upper right corner
		private void closeBox(int id)
		{
			Rect r = new Rect(WindowRect.width - 20, 0, 18, 18);
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				InputLockManager.RemoveControlLock(lockID);
				spaceCenterLock = false;
				trackingStationLock = false;
				Visible = false;
			}
		}

		//Draw the window tab options
		private void windowTabs(int id)
		{
			growE();
				if (GUILayout.Button("Altimetry"))
				{
					windowMode = 0;
				}
				if (GUILayout.Button("Slope"))
				{
					windowMode = 1;
				}
				if (GUILayout.Button("Biome"))
				{
					windowMode = 2;
				}
				if (SCANcontroller.controller.GlobalResourceOverlay)
				{
					if (GUILayout.Button("Resources"))
					{
						windowMode = 3;
					}
				}
			stopE();
		}

		//Draw the palette selection field
		private void paletteTextures(int id)
		{
			growS();
				GUILayout.Label("Palette Selection", SCANskins.SCAN_headline);
				fillS(12);
				growE();
					if (GUILayout.Button("Palette Style:", SCANskins.SCAN_buttonFixed, GUILayout.MaxWidth(120)))
					{
						dropDown = !dropDown;
						paletteBox = !paletteBox;
					}
					fillS(10);
					GUILayout.Label(palette.getPaletteTypeName, SCANskins.SCAN_whiteReadoutLabel);
				stopE();
				growE();
					// This integer stores the amount of palettes of each type
					int j = 9;
					if (palette.CurrentPalettes.paletteType == Palette.Kind.Sequential)
						j = 12;
					else if (palette.CurrentPalettes.paletteType == Palette.Kind.Qualitative)
						j = 8;
					else if (palette.CurrentPalettes.paletteType == Palette.Kind.Invertable || palette.CurrentPalettes.paletteType == Palette.Kind.Unknown)
						j = 0;
					for (int i = 0; i < j; i++)
					{
						if (i % 3 == 0)
						{
							stopE();
							fillS(9);
							growE();
						}
						Texture2D t = palette.CurrentPalettes.paletteSwatch[i];
						if (paletteBox)
						{
							GUILayout.Label("", GUILayout.Width(110), GUILayout.Height(25));
						}
						else
						{
							if (GUILayout.Button("", SCANskins.SCAN_texButton, GUILayout.Width(110), GUILayout.Height(25)))
							{
								palette.CurrentPalette = palette.CurrentPalettes.availablePalettes[i];
								paletteIndex = palette.CurrentPalette.index;
								drawPreviewLegend();
							}
						}
						Rect r = GUILayoutUtility.GetLastRect();
						r.width -= 10;
						GUI.DrawTexture(r, t);
					}
				stopE();
			stopS();
		}

		//Main palette option settings
		private void paletteOptions(int id)
		{
			growS();
				fillS(4);
				GUILayout.Label("Terrain Options: " + data.Body.name, SCANskins.SCAN_headlineSmall);

				growE();
					fillS(10);
					minTerrainSlider.drawSlider();
				stopE();
				fillS(8);
				growE();
					fillS(10);
					maxTerrainSlider.drawSlider();
				stopE();
				fillS(6);
				growE();
					fillS();
					clampState = GUILayout.Toggle(clampState, "Clamp Terrain", SCANskins.SCAN_settingsToggle, GUILayout.Width(100));
					fillS();
				stopE();
				if (clampState)
					{
						growE();
							fillS(10);
							clampTerrainSlider.drawSlider();
						stopE();
					}
				fillS(6);
				GUILayout.Label("Palette Options", SCANskins.SCAN_headlineSmall);
				if (palette.CurrentPalettes.paletteType != Palette.Kind.Fixed)
				{
					growE();
						fillS(10);
						paletteSizeSlider.drawSlider();
					stopE();
				}

				growE();
					reversePalette = GUILayout.Toggle(reversePalette, "Reverse Order", SCANskins.SCAN_settingsToggle);
					fillS(10);
					discretePalette = GUILayout.Toggle(discretePalette, "Discrete Gradient", SCANskins.SCAN_settingsToggle);
				stopE();

			stopS();
		}

		//Two boxes to show the current and new palettes as they appear on the legend
		private void palettePreview(int id)
		{
			growS();
				GUILayout.Label("Current Palette", SCANskins.SCAN_headlineSmall);
				fillS(8);
				GUILayout.Label("", SCANskins.SCAN_legendTex, GUILayout.Width(180), GUILayout.Height(25));
				Rect r = GUILayoutUtility.GetLastRect();
				GUI.DrawTexture(r, currentLegend.Legend);
			stopS();
			fillS(8);
			growS();
				GUILayout.Label("New Palette", SCANskins.SCAN_headlineSmall);
				fillS(8);
				GUILayout.Label("", SCANskins.SCAN_legendTex, GUILayout.Width(180), GUILayout.Height(25));
				r = GUILayoutUtility.GetLastRect();
				GUI.DrawTexture(r, previewLegend.Legend);
			stopS();
		}

		//Buttons to apply the new palette or cancel and return to the original
		private void paletteConfirmation(int id)
		{
			growS();
				fillS(6);
				if (GUILayout.Button("Default Settings", GUILayout.Width(135)))
				{
					//Lots of fields to update for switching palettes; again, very clumsy
					data.MinHeight = data.DefaultMinHeight;
					data.MaxHeight = data.DefaultMaxHeight;
					data.ClampHeight = data.DefaultClampHeight;
					minTerrainSlider.CurrentValue = data.MinHeight;
					maxTerrainSlider.CurrentValue = data.MaxHeight;
					clampState = data.ClampHeight != null;
					clampTerrainSlider.CurrentValue = data.ClampHeight ?? data.MinHeight + 10;
					dataPalette = palette.CurrentPalette = data.ColorPalette = data.DefaultColorPalette;
					palette.CurrentPalettes = palette.setCurrentPalettesType(dataPalette.kind);
					paletteSizeSlider.CurrentValue = data.PaletteSize = dataPalette.size;
					reversePalette = data.PaletteReverse = data.DefaultReversePalette;
					discretePalette = data.PaletteDiscrete = false;
					setSizeSlider(dataPalette.kind);
					setTerrainSliders();
					drawCurrentLegend();
				}
				fillS(6);
				growE();
					if (GUILayout.Button("Apply", GUILayout.Width(60)))
					{
						if (minTerrainSlider.CurrentValue < maxTerrainSlider.CurrentValue)
						{
							data.MinHeight = minTerrainSlider.CurrentValue;
							data.MaxHeight = maxTerrainSlider.CurrentValue;
						}
						if (clampState)
						{
							if (clampTerrainSlider.CurrentValue > minTerrainSlider.CurrentValue && clampTerrainSlider.CurrentValue < maxTerrainSlider.CurrentValue)
								data.ClampHeight = (float?)clampTerrainSlider.CurrentValue;
						}
						else
							data.ClampHeight = null;

						data.ColorPalette = palette.CurrentPalette;
						data.PaletteName = palette.CurrentPalette.name;
						data.PaletteSize = palette.CurrentPalette.size;
						data.PaletteDiscrete = discretePalette;
						data.PaletteReverse = reversePalette;
						dataPalette = data.ColorPalette;
						drawCurrentLegend();
						if (bigMap != null)
							bigMap.resetMap();
					}
					fillS(10);
					if (GUILayout.Button("Cancel", GUILayout.Width(60)))
					{
						palette.CurrentPalette = data.ColorPalette;
						InputLockManager.RemoveControlLock(lockID);
						spaceCenterLock = false;
						trackingStationLock = false;
						Visible = false;
					}
				stopE();
			stopS();
		}

		private void colorWheel(int id)
		{
			GUILayout.Label("Color Selection", SCANskins.SCAN_headline);

			Rect r = new Rect(20, 20, 256, 256);
			GUI.DrawTexture(r, SCANskins.SCAN_BigColorWheel);

			r.x += 320;
			GUI.DrawTexture(r, SCANskins.SCAN_BigColorWheel);

			r.x -= 240;
			r.y += 300;
			r.width = 60;
			r.height = 30;
			GUI.DrawTexture(r, minColorPreview);

			r.y += 32;
			GUI.DrawTexture(r, minColorOld);

			r.x += 256;
			r.y -= 32;
			GUI.DrawTexture(r, maxColorPreview);

			r.y += 32;
			GUI.DrawTexture(r, maxColorOld);

			r.x = 600;
			r.y = 20;
			r.width = 30;
			r.height = 200;

			hueSlider = GUI.VerticalSlider(r, hueSlider, 100, 0, SCANskins.SCAN_vertSlider, SCANskins.SCAN_sliderThumb);
		}

		private void biomeOptions(int id)
		{
			GUILayout.Label("Biome Options", SCANskins.SCAN_headline);

			stockBiomes = GUILayout.Toggle(stockBiomes, "Use Stock Biome Maps", SCANskins.SCAN_toggle);
			fillS(8);
			growE();
				fillS(10);
				biomeTransSlider.drawSlider();
			stopE();
		}

		private void resourceOptions(int id)
		{
			GUILayout.Label("Resource Options: " + data.Body.name, SCANskins.SCAN_headline);

			growE();
				if (GUILayout.Button("Resource Selection", SCANskins.SCAN_buttonFixed))
				{
					dropDown = !dropDown;
					resourceBox = !resourceBox;
				}
				fillS(10);
				GUILayout.Label(currentResource.Name, SCANskins.SCAN_whiteReadoutLabel);
			stopE();
			fillS(8);
			growE();
				fillS(10);
				resourceMinSlider.drawSlider();
			stopE();
			fillS(8);
			growE();
				fillS(10);
				resourceMaxSlider.drawSlider();
			stopE();
			fillS(8);
			growE();
				fillS(10);
				resourceTransSlider.drawSlider();
			stopE();
		}

		private void biomeConfirm(int id)
		{
			if (GUILayout.Button("Default Settings", GUILayout.Width(135)))
			{
				SCANcontroller.controller.LowBiomeColor = new Color();
				SCANcontroller.controller.HighBiomeColor = new Color();
				SCANcontroller.controller.useStockBiomes = stockBiomes;
				SCANcontroller.controller.biomeTransparency = 40f;
			}
			fillS(6);
			growE();
			if (GUILayout.Button("Apply", GUILayout.Width(60)))
			{
				SCANcontroller.controller.LowBiomeColor = biomeColorLow;
				SCANcontroller.controller.HighBiomeColor = biomeColorHigh;
				SCANcontroller.controller.useStockBiomes = true;
				SCANcontroller.controller.biomeTransparency = biomeTransSlider.CurrentValue;
			}
			fillS(10);
			if (GUILayout.Button("Cancel", GUILayout.Width(60)))
			{
				InputLockManager.RemoveControlLock(lockID);
				spaceCenterLock = false;
				trackingStationLock = false;
				Visible = false;
			}
			stopE();
		}

		private void resourceConfirm(int id)
		{
			if (GUILayout.Button("Save Global Values", GUILayout.Width(160)))
			{
				var allResourceList = SCANcontroller.controller.ResourceList.Values
					.SelectMany(a => a)
					.Where(a => a.Key == currentResource.Name)
					.Select(b => b.Value).ToList();
				foreach (SCANresource r in allResourceList)
				{
					r.MinValue = resourceMinSlider.CurrentValue;
					r.MaxValue = resourceMaxSlider.CurrentValue;
					r.Transparency = resourceTransSlider.CurrentValue;
					r.FullColor = resourceColorFull;
					r.EmptyColor = resourceColorEmpty;
				}
			}
			fillS(6);
			if (GUILayout.Button("Revert All Values To Default", GUILayout.Width(200)))
			{
				var allResourceList = SCANcontroller.controller.ResourceList.Values
					.SelectMany(a => a)
					.Where(a => a.Key == currentResource.Name)
					.Select(b => b.Value).ToList();
				foreach (SCANresource r in allResourceList)
				{
					r.MinValue = r.DefaultMinValue;
					r.MaxValue = r.DefaultMaxValue;
					r.Transparency = 0.4f;
					r.FullColor = r.ResourceType.ColorFull;
					r.EmptyColor = r.ResourceType.ColorEmpty;
				}
			}
			fillS(6);
			if (GUILayout.Button("Default Settings", GUILayout.Width(135)))
			{
				currentResource.MinValue = currentResource.DefaultMinValue;
				currentResource.MaxValue = currentResource.DefaultMaxValue;
				currentResource.Transparency = 0.4f;
				currentResource.FullColor = currentResource.ResourceType.ColorFull;
				currentResource.EmptyColor = currentResource.ResourceType.ColorEmpty;
			}
			fillS(6);
			growE();
			if (GUILayout.Button("Apply", GUILayout.Width(60)))
			{
				currentResource.MinValue = resourceMinSlider.CurrentValue;
				currentResource.MaxValue = resourceMaxSlider.CurrentValue;
				currentResource.Transparency = resourceTransSlider.CurrentValue;
				currentResource.FullColor = resourceColorFull;
				currentResource.EmptyColor = resourceColorEmpty;
			}
			fillS(10);
			if (GUILayout.Button("Cancel", GUILayout.Width(60)))
			{
				InputLockManager.RemoveControlLock(lockID);
				spaceCenterLock = false;
				trackingStationLock = false;
				Visible = false;
			}
			stopE();
		}

		//Drop down menu for palette selection
		private void dropDownBox(int id)
		{
			if (dropDown)
			{
				if (paletteBox && windowMode == 0)
				{
					ddRect = new Rect(40, 90, 100, 100);
					GUI.Box(ddRect, "", SCANskins.SCAN_dropDownBox);
					for (int i = 0; i < Palette.kindNames.Length; i++)
					{
						Rect r = new Rect(ddRect.x + 10, ddRect.y + 5 + (i * 23), 80, 22);
						if (GUI.Button(r, Palette.kindNames[i], SCANskins.SCAN_dropDownButton))
						{
							paletteBox = false;
							palette.CurrentPalettes = palette.setCurrentPalettesType((Palette.Kind)i);
							setSizeSlider((Palette.Kind)i);
						}
					}
				}
				else if (resourceBox && windowMode == 3)
				{
					ddRect = new Rect(40, 100, 100, 100);
					GUI.Box(ddRect, "", SCANskins.SCAN_dropDownBox);
					for (int i = 0; i < SCANcontroller.ResourceTypes.Count; i ++)
					{
						Rect r = new Rect(ddRect.x + 10, ddRect.y + 5 + (i * 23), 80, 22);
						if (GUI.Button(r, SCANcontroller.ResourceTypes.ElementAt(i).Value.Name, SCANskins.SCAN_dropDownButton))
						{
							currentResource = SCANcontroller.controller.ResourceList[data.Body.name][SCANcontroller.ResourceTypes.ElementAt(i).Value.Name];
							resourceMinSlider.CurrentValue = currentResource.MinValue;
							resourceMaxSlider.CurrentValue = currentResource.MaxValue;
							resourceTransSlider.CurrentValue = currentResource.Transparency;
						}
					}
				}
				else
					dropDown = false;
			}
		}

		//Draws the palette swatch for the currently active SCANdata selection
		private void drawCurrentLegend()
		{
			currentLegend = new SCANmapLegend();
			currentLegend.Legend = currentLegend.getLegend(0, data);
			//currentLegend = SCANmapLegend.getLegend(0, data);
		}

		//Draws the palette swatch for the newly adjusted palette
		private void drawPreviewLegend()
		{
			float? clamp = null;
			Color32[] c = palette.CurrentPalette.colors;
			if (clampState)
				clamp = (float?)clampTerrainSlider.CurrentValue;
			if (reversePalette)
				c = palette.CurrentPalette.colorsReverse;
			previewLegend = new SCANmapLegend();
			previewLegend.Legend = previewLegend.getLegend(maxTerrainSlider.CurrentValue, minTerrainSlider.CurrentValue, clamp, discretePalette, c);
		}

		//Resets the palettes whenever the size slider is adjusted
		private void regenPaletteSets()
		{
			palette.DivPaletteSet = palette.generatePaletteSet((int)paletteSizeSlider.CurrentValue, Palette.Kind.Diverging);
			palette.QualPaletteSet = palette.generatePaletteSet((int)paletteSizeSlider.CurrentValue, Palette.Kind.Qualitative);
			palette.SeqPaletteSet = palette.generatePaletteSet((int)paletteSizeSlider.CurrentValue, Palette.Kind.Sequential);
			palette.FixedPaletteSet = palette.generatePaletteSet(0, Palette.Kind.Fixed);
			palette.CurrentPalettes = palette.setCurrentPalettesType(palette.getPaletteType);
		}

		//Change the max range on the palette size slider based on palette type
		private void setSizeSlider(Palette.Kind k)
		{
			int max = 11;
			switch (k)
			{
				case Palette.Kind.Diverging:
					{
						max = 11;
						break;
					}
				case Palette.Kind.Qualitative:
					{
						max = 12;
						break;
					}
				case Palette.Kind.Sequential:
					{
						max = 9;
						break;
					}
				case Palette.Kind.Fixed:
					{
						break;
					}
			}

			paletteSizeSlider.MaxValue = max;
			if (paletteSizeSlider.CurrentValue > paletteSizeSlider.MaxValue)
				paletteSizeSlider.CurrentValue = paletteSizeSlider.MaxValue;
			
		}

		//Dynamically adjust the min and max values on all of the terrain height sliders; avoids impossible values
		private void setTerrainSliders()
		{
			minTerrainSlider.MinValue = data.DefaultMinHeight - 10000f;
			maxTerrainSlider.MaxValue = data.DefaultMaxHeight + 10000f;
			minTerrainSlider.MaxValue = maxTerrainSlider.CurrentValue- 100f;
			maxTerrainSlider.MinValue = minTerrainSlider.CurrentValue + 100f;
			clampTerrainSlider.MinValue = minTerrainSlider.CurrentValue + 10f;
			clampTerrainSlider.MaxValue = maxTerrainSlider.MinValue - 10f;
			if (clampTerrainSlider.CurrentValue < minTerrainSlider.CurrentValue + 10f)
				clampTerrainSlider.CurrentValue = minTerrainSlider.CurrentValue + 10f;
			else if (clampTerrainSlider.CurrentValue > maxTerrainSlider.CurrentValue - 10f)
				clampTerrainSlider.CurrentValue = maxTerrainSlider.CurrentValue - 10f;
		}

	}
}
