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
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Map;
using SCANsat.SCAN_UI.UI_Framework;
using SCANsat.SCAN_Platform;
using SCANsat.SCAN_Platform.Palettes;
using FinePrint.Utilities;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;
using UnityEngine;

namespace SCANsat.SCAN_UI
{
	class SCANcolorSelection: SCAN_MBW
	{
		private bool dropDown, paletteBox, resourceBox, planetBox, saveWarning;
		private bool oldReverseState, oldDiscreteState;
		private bool controlLock, clampState, oldClampState;
		private Rect ddRect;
		private int paletteIndex;
		private SCANmapLegend currentLegend, previewLegend;
		private int windowMode = 0;

		private SCANterrainConfig currentTerrain;
		private SCANterrainConfig bodyTerrain;
		private float minT, maxT, clampT, pSize;

		private SCANuiSlider minTerrainSlider, maxTerrainSlider, clampTerrainSlider, paletteSizeSlider, resourceMinSlider, resourceMaxSlider, resourceTransSlider, biomeTransSlider, slopeSlider;

		private SCANuiColorPicker slopeColorPickerLow, slopeColorPickerHigh, biomeColorPicker, resourceColorPicker;
		private float bTrans, rTrans, sCutoff;

		private bool stockBiomes = false;
		private bool biomeBorders = true;

		private SCANresourceGlobal currentResource;
		private float lowRCutoff, highRCutoff;

		private List<SCANresourceGlobal> loadedResources;

		private bool fineControlMode, oldFineControl;
		private int bodyIndex;

		private Vector2 scrollR;
		private const string lockID = "colorLockID";
		internal readonly static Rect defaultRect = new Rect(100, 400, 780, 360);
		private static Rect sessionRect = defaultRect;

		//SCAN_MBW objects to sync the color selection fields to the currently displayed map
		private SCANkscMap kscMapObj;
		private SCANBigMap bigMapObj;

		private static SCANmap bigMap;
		private CelestialBody body;

		private string colorTerrainHelpMin = "Defines the low altitude cutoff for the terrain color palette. Anything below this altitude will be shown with the lowest color.";
		private string colorTerrainHelpMax = "Defines the high altitude cutoff for the terrain color palette. Anything above this altitude will be shown with the highest color.";
		private string colorTerrainHelpClampToggle = "Used to define a cutoff between the low and high altitude values. This is particularly useful for ocean planets where it helps to define a sharp distinction between land and ocean.";
		private string colorTerrainHelpClamp = "Defines the clamp altitude cutoff. Anything below the cutoff will be represented by the first two colors in the selected color palette. Anything above the cutoff will be represented with the remaining colors.";
		private string colorTerrainHelpReverse = "Reverses the order of the currently\nselected color palette.";
		private string colorTerrainHelpDiscrete = "Draws the map using only the specific colors defined by each palette, instead of smoothly interpolating between them.";
		private string colorTerrainHelpPaletteSize = "Adjust the number of colors available in the currently selected color palette.";
		private string colorBiomeHelpStock = "Use the stock biome color scheme for\nSCANsat biome maps.";
		private string colorBiomeHelpWhiteBorder = "Draw a white border between biomes. Does not apply to the planetary overlay biome maps.";
		private string colorBiomeHelpTransparency = "Adjust the transparency of biome maps. Terrain elevation is shown behind the biome maps. Set to 0% to disable terrain drawing.";
		private string colorPickerHelpLow = "The top color swatch shows the updated color selection for the low end of this color spectrum. The bottom color swatch shows the currently active color.";
		private string colorPickerHelpHigh = "The top color swatch shows the updated color selection for the high end of this color spectrum. The bottom color swatch shows the currently active color.";
		private string colorPickerHelpValue = "This slider adjusts the Value (in HSV color terms) or Brightness for the currently selected color.";
		private string colorResourceHelpFineControl = "Activates Fine Control Mode where the sliders will only show values within 5% of the current selection.";
		private string colorResourceHelpMin = "The low cutoff for resource concentration on the selected celestial body. Resource deposits at this level will be displayed using the low end of the current resource overlay color spectrum. Resource deposits below this value will not be shown.";
		private string colorResourceHelpMax = "The high cutoff for resource concentration on the selected celestial body. Resource deposits above this value will be shown using the high end of the current resource overlay color spectrum.";
		private string colorResourceHelpTransparency = "Defines the level of transparency for resource overlays. Increase to allow more of the underlying terrain, slope, or biome map to be visible. This also affect the transparency of resource deposits on the planetary overlay resource map.";
		private string colorResourceHelpApply = "Applies the current values for the selected resource and celestial body only.";
		private string colorResourceHelpApplyAll = "Applies the current values for the selected resource for all celestial bodies.";
		private string colorResourceHelpDefault = "Reverts to the default values for the selected resource and celestial body only.";
		private string colorResourceHelpDefaultAll = "Reverts to the default values for the selected resource for all celestial bodies.";
		private string colorHelpSaveToConfig = "Save all color configuration values to the config file found in your SCANsat/Resources folder. These values serve as the defaults for new saves and for all Revert To Default buttons. Values do not need to be saved to the config file to be applied for this save file.";
		private string colorSlopeHelpCutoff = "Adjust the cutoff level between the two selected slope color pairs.";

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Color Management";
			WindowRect = sessionRect;
			WindowStyle = SCANskins.SCAN_window;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(780), GUILayout.Height(360) };
			Visible = false;
			DragEnabled = true;
			TooltipMouseOffset = new Vector2d(-10, -25);
			TooltipMaxWidth = 350;
			TooltipDisplayForSecs = 60;
			ClampToScreenOffset = new RectOffset(-450, -450, -250, -250);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");

			removeControlLocks();
		}

		protected override void Start()
		{
			TooltipsEnabled = false;

			if (SCANconfigLoader.languagePack != null)
				loadStrings();

			if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				kscMapObj = (SCANkscMap)SCANcontroller.controller.kscMap;
				if (SCANkscMap.BigMap != null)
					bigMap = SCANkscMap.BigMap;
				if (kscMapObj.Data != null)
					body = kscMapObj.Body;
				if (body == null)
					body = Planetarium.fetch.Home;
			}
			else if (HighLogic.LoadedSceneIsFlight)
			{
				bigMapObj = (SCANBigMap)SCANcontroller.controller.BigMap;
				if (SCANBigMap.BigMap != null)
					bigMap = SCANBigMap.BigMap;
				if (bigMapObj.Data != null)
					body = bigMapObj.Data.Body;
				if (body == null)
					body = FlightGlobals.currentMainBody;
			}

			setBodyTerrain();

			currentTerrain = new SCANterrainConfig(bodyTerrain);

			stockBiomes = SCANcontroller.controller.useStockBiomes;
			biomeBorders = SCANcontroller.controller.biomeBorder;

			minTerrainSlider = new SCANuiSlider(currentTerrain.DefaultMinHeight - SCANconfigLoader.SCANNode.RangeBelowMinHeight, currentTerrain.MaxTerrain - 100, currentTerrain.MinTerrain, "Min: ", "m", colorTerrainHelpMin, -2);
			maxTerrainSlider = new SCANuiSlider(currentTerrain.MinTerrain + 100, currentTerrain.DefaultMaxHeight + SCANconfigLoader.SCANNode.RangeAboveMaxHeight, currentTerrain.MaxTerrain, "Max: ", "m", colorTerrainHelpMax, -2);
			clampTerrainSlider = new SCANuiSlider(currentTerrain.MinTerrain + 10, currentTerrain.MaxTerrain - 10, currentTerrain.ClampTerrain ?? currentTerrain.MinTerrain + 10, "Clamp: ", "m", colorTerrainHelpClamp, -1);
			paletteSizeSlider = new SCANuiSlider(3, 12, currentTerrain.PalSize, "Palette Size: ", "", colorTerrainHelpPaletteSize, 0);

			sCutoff = SCANcontroller.controller.slopeCutoff;
			slopeSlider = new SCANuiSlider(0.2f, 1.8f, sCutoff, "Slope Cutoff: ", "", colorSlopeHelpCutoff, 2, 180, 10);

			slopeColorPickerLow = new SCANuiColorPicker(SCANcontroller.controller.lowSlopeColorOne, SCANcontroller.controller.highSlopeColorOne, colorPickerHelpLow, colorPickerHelpHigh, colorPickerHelpValue, true);
			slopeColorPickerHigh = new SCANuiColorPicker(SCANcontroller.controller.lowSlopeColorTwo, SCANcontroller.controller.highSlopeColorTwo, colorPickerHelpLow, colorPickerHelpHigh, colorPickerHelpValue, true);

			slopeColorPickerLow.updateOldSwatches();
			slopeColorPickerHigh.updateOldSwatches();

			bTrans = SCANcontroller.controller.biomeTransparency;
			biomeTransSlider = new SCANuiSlider(0, 80, bTrans, "Ter. Trans: ", "%", colorBiomeHelpTransparency, 0);

			biomeColorPicker = new SCANuiColorPicker(SCANcontroller.controller.lowBiomeColor, SCANcontroller.controller.highBiomeColor, colorPickerHelpLow, colorPickerHelpHigh, colorPickerHelpValue, true);

			biomeColorPicker.updateOldSwatches();

			if (SCANconfigLoader.GlobalResource)
			{
				loadedResources = SCANcontroller.setLoadedResourceList();
				currentResource = new SCANresourceGlobal(loadedResources[0]);
				currentResource.CurrentBodyConfig(body.name);

				if (currentResource != null)
				{
					resourceMinSlider = new SCANuiSlider(0, currentResource.CurrentBody.MinValue - 0.1f, currentResource.CurrentBody.MinValue, "Min: ", "%", colorResourceHelpMin, 1);
					resourceMaxSlider = new SCANuiSlider(currentResource.CurrentBody.MinValue + 0.1f, 100, currentResource.CurrentBody.MaxValue, "Max: ", "%", colorResourceHelpMax, 1);
					resourceTransSlider = new SCANuiSlider(0, 80, currentResource.Transparency, "Trans: ", "%", colorResourceHelpTransparency, 0);

					resourceColorPicker = new SCANuiColorPicker(currentResource.MinColor, currentResource.MaxColor, colorPickerHelpLow, colorPickerHelpHigh, colorPickerHelpValue, true);
				}
			}

			bodyIndex = body.flightGlobalsIndex;

			if (windowMode > 3 || (windowMode > 2 && !SCANconfigLoader.GlobalResource))
				windowMode = 0;

			setSizeSlider(currentTerrain.ColorPal.kind);
		}

		private void loadStrings()
		{
			colorTerrainHelpMin = SCANconfigLoader.languagePack.colorTerrainHelpMin;
			colorTerrainHelpMax = SCANconfigLoader.languagePack.colorTerrainHelpMax;
			colorTerrainHelpClampToggle = SCANconfigLoader.languagePack.colorTerrainHelpClampToggle;
			colorTerrainHelpClamp = SCANconfigLoader.languagePack.colorTerrainHelpClamp;
			colorTerrainHelpReverse = SCANconfigLoader.languagePack.colorTerrainHelpReverse;
			colorTerrainHelpDiscrete = SCANconfigLoader.languagePack.colorTerrainHelpDiscrete;
			colorTerrainHelpPaletteSize = SCANconfigLoader.languagePack.colorTerrainHelpPaletteSize;
			colorBiomeHelpStock = SCANconfigLoader.languagePack.colorBiomeHelpStock;
			colorBiomeHelpWhiteBorder = SCANconfigLoader.languagePack.colorBiomeHelpWhiteBorder;
			colorBiomeHelpTransparency = SCANconfigLoader.languagePack.colorBiomeHelpTransparency;
			colorPickerHelpLow = SCANconfigLoader.languagePack.colorPickerHelpLow;
			colorPickerHelpHigh = SCANconfigLoader.languagePack.colorPickerHelpHigh;
			colorPickerHelpValue = SCANconfigLoader.languagePack.colorPickerHelpValue;
			colorResourceHelpFineControl = SCANconfigLoader.languagePack.colorResourceHelpFineControl;
			colorResourceHelpMin = SCANconfigLoader.languagePack.colorResourceHelpMin;
			colorResourceHelpMax = SCANconfigLoader.languagePack.colorResourceHelpMax;
			colorResourceHelpTransparency = SCANconfigLoader.languagePack.colorResourceHelpTransparency;
			colorResourceHelpApply = SCANconfigLoader.languagePack.colorResourceHelpApply;
			colorResourceHelpApplyAll = SCANconfigLoader.languagePack.colorResourceHelpApplyAll;
			colorResourceHelpDefault = SCANconfigLoader.languagePack.colorResourceHelpDefault;
			colorResourceHelpDefaultAll = SCANconfigLoader.languagePack.colorResourceHelpDefaultAll;
			colorHelpSaveToConfig = SCANconfigLoader.languagePack.colorHelpSaveToConfig;
			colorSlopeHelpCutoff = SCANconfigLoader.languagePack.colorSlopeHelpCutoff;
		}

		protected override void OnDestroy()
		{
			removeControlLocks();
			TooltipsEnabled = false;
		}

		internal void removeControlLocks()
		{
			InputLockManager.RemoveControlLock(lockID);
			controlLock = false;
		}

		protected override void DrawWindowPre(int id)
		{
			//Some clumsy logic is used here to ensure that the color selection fields always remain in sync with the current map in each scene
			switch (HighLogic.LoadedScene)
			{
				case GameScenes.FLIGHT:
					if (SCANBigMap.BigMap != null)
					{
						bigMap = SCANBigMap.BigMap;
					}

					if (body == null)
					{
						body = FlightGlobals.currentMainBody;
					}
					break;
				case GameScenes.SPACECENTER:
					if (kscMapObj.Visible)
					{
						bigMap = SCANkscMap.BigMap;
					}

					if (body == null)
					{
						body = Planetarium.fetch.Home;
					}

					Vector2 mousePos = Input.mousePosition;
					mousePos.y = Screen.height - mousePos.y;
					if (WindowRect.Contains(mousePos) && !controlLock)
					{
						InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS | ControlTypes.KSC_ALL, lockID);
						controlLock = true;
					}
					else if (!WindowRect.Contains(mousePos) && controlLock)
					{
						removeControlLocks();
					}
					break;
				case GameScenes.TRACKSTATION:
					if (kscMapObj.Visible)
					{
						bigMap = SCANkscMap.BigMap;
					}

					if (body == null)
					{
						body = Planetarium.fetch.Home;
					}

					Vector2 mousePosT = Input.mousePosition;
					mousePosT.y = Screen.height - mousePosT.y;
					if (WindowRect.Contains(mousePosT) && !controlLock)
					{
						InputLockManager.SetControlLock(ControlTypes.TRACKINGSTATION_UI, lockID);
						controlLock = true;
					}
					else if (!WindowRect.Contains(mousePosT) && controlLock)
					{
						removeControlLocks();
					}
					break;
			}

			//This updates all of the fields whenever the palette selection is changed
			if (windowMode == 0 && (currentLegend == null || bodyIndex != body.flightGlobalsIndex))
			{
				setBodyTerrain();

				currentTerrain = new SCANterrainConfig(bodyTerrain);

				bodyIndex = body.flightGlobalsIndex;

				updateUI();
			}

			if (windowMode == 0 && previewLegend == null)
			{
				drawPreviewLegend();
			}

			if (!dropDown)
			{
				paletteBox = false;
				resourceBox = false;
				planetBox = false;
				saveWarning = false;
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
					paletteConfirmation(id);	/* The buttons for default, apply, and cancel */
					stopE();
				}
				else if (windowMode == 1)
				{
					growE();
						fillS(10);
						slopeColorPickerLow.drawColorSelector(WindowRect);
						fillS(40);
						slopeColorPickerHigh.drawColorSelector(WindowRect);
					stopE();
					fillS(100);
					growE();
						slopeOptions(id);
						slopeConfirm(id);
					stopE();
				}
				else if (windowMode == 2)
				{
					growE();
						fillS(10);
						biomeColorPicker.drawColorSelector(WindowRect);
						fillS(80);
						growS();
							biomeOptions(id);
							biomeConfirm(id);
						stopS();
					stopE();
				}
				else if (windowMode == 3 && SCANconfigLoader.GlobalResource)
				{
					growE();
						fillS(10);
						resourceColorPicker.drawColorSelector(WindowRect);
						fillS(120);
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
			if (dropDown && Event.current.type == EventType.mouseDown && !ddRect.Contains(Event.current.mousePosition))
			{
				dropDown = false;
			}

			//These methods update all of the UI elements whenever any of the options are changed
			switch (windowMode)
			{
				case 0:
					if (currentTerrain.PalRev != oldReverseState)
					{
						oldReverseState = currentTerrain.PalRev;
						drawPreviewLegend();
					}

					if (minTerrainSlider.valueChanged() || maxTerrainSlider.valueChanged())
					{
						setTerrainSliders();
					}

					if (currentTerrain.PalDis != oldDiscreteState)
					{
						oldDiscreteState = currentTerrain.PalDis;
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
						currentTerrain.ColorPal = palette.CurrentPalettes.availablePalettes[paletteIndex];
						drawPreviewLegend();
					}
					break;

				case 1:
					slopeColorPickerLow.colorStateChanged();
					slopeColorPickerLow.brightnessChanged();
					slopeColorPickerHigh.colorStateChanged();
					slopeColorPickerHigh.brightnessChanged();
					break;

				case 2:
					biomeColorPicker.colorStateChanged();
					biomeColorPicker.brightnessChanged();
					break;

				case 3:
					if (resourceMinSlider.valueChanged() || resourceMaxSlider.valueChanged())
					{
						setResourceSliders();
					}

					if (bodyIndex != body.flightGlobalsIndex)
					{
						SCANUtil.SCANdebugLog("Trigger Body Change");
						bodyIndex = body.flightGlobalsIndex;

						currentResource.CurrentBodyConfig(body.name);

						lowRCutoff = currentResource.CurrentBody.MinValue;
						highRCutoff = currentResource.CurrentBody.MaxValue;

						oldFineControl = fineControlMode = false;

						setResourceSliders();
					}

					if (oldFineControl != fineControlMode)
					{
						oldFineControl = fineControlMode;
						if (fineControlMode)
						{
							if (lowRCutoff < 5f)
								resourceMinSlider.MinValue = 0f;
							else
								resourceMinSlider.MinValue = lowRCutoff - 5;

							if (lowRCutoff > 95f)
								resourceMinSlider.MaxValue = 100f;
							else if (highRCutoff < lowRCutoff + 5f)
								resourceMinSlider.MaxValue = highRCutoff - 0.1f;
							else
								resourceMinSlider.MaxValue = lowRCutoff + 5f;

							if (highRCutoff < 5f)
								resourceMaxSlider.MinValue = 0f;
							else if (lowRCutoff > highRCutoff - 5f)
								resourceMaxSlider.MinValue = lowRCutoff + 0.1f;
							else
								resourceMaxSlider.MinValue = highRCutoff - 5f;

							if (highRCutoff > 95f)
								resourceMaxSlider.MaxValue = 100f;
							else
								resourceMaxSlider.MaxValue = highRCutoff + 5f;
						}
						else
							setResourceSliders();
					}

					resourceColorPicker.colorStateChanged();
					resourceColorPicker.brightnessChanged();
					break;
				default:
					break;
			}

			sessionRect = WindowRect;
		}

		//Draw the version label in the upper left corner
		private void versionLabel(int id)
		{
			Rect r = new Rect(6, 0, 50, 18);
			GUI.Label(r, SCANmainMenuLoader.SCANsatVersion, SCANskins.SCAN_whiteReadoutLabel);
		}

		//Draw the close button in the upper right corner
		private void closeBox(int id)
		{
			Rect r = new Rect(WindowRect.width - 42, 1, 18, 18);
			if (GUI.Button(r, textWithTT("?", "Show Help Tips"), SCANskins.SCAN_closeButton))
			{
				TooltipsEnabled = !TooltipsEnabled;
			}

			r.x += 22;

			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				removeControlLocks();
				Visible = false;
				TooltipsEnabled = false;
			}
		}

		//Draw the window tab options
		private void windowTabs(int id)
		{
			growE();
				if (GUILayout.Button("Altimetry"))
				{
					windowMode = 0;

					setBodyTerrain();

					currentTerrain = new SCANterrainConfig(bodyTerrain);

					bodyIndex = body.flightGlobalsIndex;

					updateUI();
				}
				if (GUILayout.Button("Slope"))
				{
					windowMode = 1;
				}
				if (GUILayout.Button("Biome"))
				{
					windowMode = 2;
				}
				if (SCANconfigLoader.GlobalResource)
				{
					if (GUILayout.Button("Resources"))
					{
						windowMode = 3;

						fineControlMode = oldFineControl = false;

						currentResource.CurrentBodyConfig(body.name);

						bodyIndex = body.flightGlobalsIndex;

						updateUI();
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
					if (GUILayout.Button("Palette Style:", GUILayout.MaxWidth(120)))
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
					else if (palette.CurrentPalettes.paletteType == Palette.Kind.Fixed)
						j = 11;
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
								currentTerrain.ColorPal = palette.CurrentPalettes.availablePalettes[i];
								paletteIndex = currentTerrain.ColorPal.index;
								updateUI();
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
				growE();
					fillS(20);
					GUILayout.Label("Terrain Options: ", SCANskins.SCAN_headlineSmall, GUILayout.Width(150));

					if (GUILayout.Button(body.name, SCANskins.SCAN_headerButton, GUILayout.Width(170)))
					{
						dropDown = !dropDown;
						planetBox = !planetBox;
					}
				stopE();

				growE();
					fillS(10);
					currentTerrain.MinTerrain = minTerrainSlider.drawSlider(dropDown, ref minT);
				stopE();
				fillS(8);
				growE();
					fillS(10);
					currentTerrain.MaxTerrain = maxTerrainSlider.drawSlider(dropDown, ref maxT);
				stopE();
				fillS(6);
				growE();
					fillS();
						if (dropDown)
							GUILayout.Label("Clamp Terrain", SCANskins.SCAN_settingsToggle, GUILayout.Width(100));
						else
						clampState = GUILayout.Toggle(clampState, textWithTT("Clamp Terrain", colorTerrainHelpClampToggle), SCANskins.SCAN_settingsToggle, GUILayout.Width(100));
					fillS();
				stopE();
				if (clampState)
				{
					growE();
						fillS(10);
						currentTerrain.ClampTerrain = clampTerrainSlider.drawSlider(dropDown, ref clampT);
					stopE();
				}
				fillS(6);
				GUILayout.Label("Palette Options", SCANskins.SCAN_headlineSmall);
				if (palette.CurrentPalettes.paletteType != Palette.Kind.Fixed)
				{
					growE();
						fillS(10);
						currentTerrain.PalSize = (int)paletteSizeSlider.drawSlider(dropDown, ref pSize);
					stopE();
				}

				growE();
					if (dropDown)
					{
						GUILayout.Label(" Reverse Order", SCANskins.SCAN_boldToggle, GUILayout.Width(120));
						fillS(10);
						GUILayout.Label(" Discrete Gradient", SCANskins.SCAN_boldToggle, GUILayout.Width(140));
					}
					else
					{
						currentTerrain.PalRev = GUILayout.Toggle(currentTerrain.PalRev, textWithTT(" Reverse Order", colorTerrainHelpReverse), SCANskins.SCAN_boldToggle, GUILayout.Width(120));
						fillS(10);
						currentTerrain.PalDis = GUILayout.Toggle(currentTerrain.PalDis, textWithTT(" Discrete Gradient", colorTerrainHelpDiscrete), SCANskins.SCAN_boldToggle, GUILayout.Width(140));
					}
				stopE();

			stopS();
		}

		//Two boxes to show the current and new palettes as they appear on the legend
		private void palettePreview(int id)
		{
			growS();
				GUILayout.Label("Current Palette", SCANskins.SCAN_headlineSmall);
				GUILayout.Label("", SCANskins.SCAN_legendTex, GUILayout.Width(180), GUILayout.Height(25));
				Rect r = GUILayoutUtility.GetLastRect();
				GUI.DrawTexture(r, currentLegend.Legend);
			stopS();
			fillS(8);
			growS();
				GUILayout.Label("New Palette", SCANskins.SCAN_headlineSmall);
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
					growE();
						if (!dropDown)
						{
							if (GUILayout.Button("Apply Values", GUILayout.Width(110)))
							{
								if (!clampState)
									currentTerrain.ClampTerrain = null;

								SCANcontroller.updateTerrainConfig(currentTerrain);

								updateUI();

								if (bigMap != null && bigMap.Body == body)
								{
									if (bigMap.MType == mapType.Altimetry && SCANcontroller.controller.colours == 0)
										bigMap.resetMap(SCANcontroller.controller.map_ResourceOverlay);
								}
							}

							fillS(6);

							if (GUILayout.Button("Default Values", GUILayout.Width(110)))
							{
								setBodyTerrain();

								currentTerrain.MinTerrain = bodyTerrain.DefaultMinHeight;
								currentTerrain.MaxTerrain = bodyTerrain.DefaultMaxHeight;
								currentTerrain.ClampTerrain = bodyTerrain.DefaultClampHeight;
								currentTerrain.ColorPal = bodyTerrain.DefaultPalette;
								currentTerrain.PalRev = bodyTerrain.DefaultReverse;
								currentTerrain.PalDis = bodyTerrain.DefaultDiscrete;
								currentTerrain.PalSize = bodyTerrain.DefaultPaletteSize;

								SCANcontroller.updateTerrainConfig(currentTerrain);

								updateUI();

								if (bigMap != null && bigMap.Body == body)
								{
									if (bigMap.MType == mapType.Altimetry && SCANcontroller.controller.colours == 0)
										bigMap.resetMap(SCANcontroller.controller.map_ResourceOverlay);
								}
							}
						}
						else
						{
							GUILayout.Label("Apply Values", SCANskins.SCAN_button, GUILayout.Width(110));
							fillS(6);
							GUILayout.Label("Default Values", SCANskins.SCAN_button, GUILayout.Width(110));
						}
				stopE();
				fillS(8);
				if (!dropDown)
				{
					if (GUILayout.Button(textWithTT("Save Values To Config", colorHelpSaveToConfig), GUILayout.Width(180)))
					{
						dropDown = true;
						saveWarning = true;
					}
				}
				else
					GUILayout.Label("Save Values To Config", SCANskins.SCAN_button, GUILayout.Width(180));
			stopS();
		}

		private void biomeOptions(int id)
		{
			GUILayout.Label("Biome Options", SCANskins.SCAN_headline, GUILayout.Width(300));

			fillS(20);
			stockBiomes = GUILayout.Toggle(stockBiomes, textWithTT("Use Stock Biome Maps", colorBiomeHelpStock), SCANskins.SCAN_toggle, GUILayout.Width(200));
			fillS(8);
			biomeBorders = GUILayout.Toggle(biomeBorders, textWithTT("White Biome Borders", colorBiomeHelpWhiteBorder), SCANskins.SCAN_toggle, GUILayout.Width(190));
			fillS(8);
			growE();
				fillS(10);
				biomeTransSlider.drawSlider(false, ref bTrans);
			stopE();
		}

		private void slopeOptions(int id)
		{
			slopeSlider.drawSlider(false, ref sCutoff);
		}

		private void resourceOptions(int id)
		{
			growE();

			fillS(30);
			GUILayout.Label("Resource Options: ", SCANskins.SCAN_headlineSmall, GUILayout.Width(150));

			if (GUILayout.Button(body.name, SCANskins.SCAN_headerButton, GUILayout.Width(170)))
			{
				dropDown = !dropDown;
				planetBox = !planetBox;
			}

			stopE();

			fillS(10);
			growE();
				fillS(20);
				GUILayout.Label("Resource Selection: ", SCANskins.SCAN_headlineSmall, GUILayout.Width(180));

				if (dropDown)
				{
					GUILayout.Label(currentResource.Name, SCANskins.SCAN_headerButton, GUILayout.Width(140));
				}
				else
				{
					if (GUILayout.Button(currentResource.Name, SCANskins.SCAN_headerButton, GUILayout.Width(140)))
					{
						dropDown = !dropDown;
						resourceBox = !resourceBox;
					}
				}

			stopE();
			fillS(10);
			growE();
				fillS(110);
				if (dropDown)
					GUILayout.Label(" Fine Control Mode", SCANskins.SCAN_boldToggle, GUILayout.Width(140));
				else
					fineControlMode = GUILayout.Toggle(fineControlMode, textWithTT(" Fine Control Mode", colorResourceHelpFineControl), SCANskins.SCAN_boldToggle, GUILayout.Width(140));
			stopE();
			growE();
				fillS(10);
				currentResource.CurrentBody.MinValue = resourceMinSlider.drawSlider(dropDown, ref lowRCutoff);
			stopE();
			fillS(8);
			growE();
				fillS(10);
				currentResource.CurrentBody.MaxValue = resourceMaxSlider.drawSlider(dropDown, ref highRCutoff);
			stopE();
			fillS(8);
			growE();
				fillS(10);
				currentResource.Transparency = resourceTransSlider.drawSlider(dropDown, ref rTrans);
			stopE();
		}

		private void biomeConfirm(int id)
		{
			fillS(10);

			growE();
				if (!dropDown)
				{
					if (GUILayout.Button("Apply Values", GUILayout.Width(110)))
					{
						SCANcontroller.controller.lowBiomeColor = biomeColorPicker.ColorLow;
						SCANcontroller.controller.lowBiomeColor32 = biomeColorPicker.ColorLow;
						SCANcontroller.controller.highBiomeColor = biomeColorPicker.ColorHigh;
						SCANcontroller.controller.highBiomeColor32 = biomeColorPicker.ColorHigh;
						SCANcontroller.controller.useStockBiomes = stockBiomes;
						SCANcontroller.controller.biomeBorder = biomeBorders;
						SCANcontroller.controller.biomeTransparency = bTrans;

						biomeColorPicker.updateOldSwatches();

						if (bigMap != null)
						{
							if (bigMap.MType == mapType.Biome)
								bigMap.resetMap(SCANcontroller.controller.map_ResourceOverlay);
						}
					}

					fillS(8);

					if (GUILayout.Button("Default Values", GUILayout.Width(110)))
					{
						SCANcontroller.controller.lowBiomeColor = SCANconfigLoader.SCANNode.LowBiomeColor;
						SCANcontroller.controller.lowBiomeColor32 = SCANconfigLoader.SCANNode.LowBiomeColor;
						SCANcontroller.controller.highBiomeColor = SCANconfigLoader.SCANNode.HighBiomeColor;
						SCANcontroller.controller.highBiomeColor32 = SCANconfigLoader.SCANNode.HighBiomeColor;
						SCANcontroller.controller.useStockBiomes = SCANconfigLoader.SCANNode.StockBiomeMap;
						SCANcontroller.controller.biomeBorder = SCANconfigLoader.SCANNode.BiomeBorder;
						SCANcontroller.controller.biomeTransparency = SCANconfigLoader.SCANNode.BiomeTransparency;

						stockBiomes = SCANcontroller.controller.useStockBiomes;
						biomeBorders = SCANcontroller.controller.biomeBorder;

						biomeColorPicker = new SCANuiColorPicker(SCANcontroller.controller.lowBiomeColor, SCANcontroller.controller.highBiomeColor, colorPickerHelpLow, colorPickerHelpHigh, colorPickerHelpValue, biomeColorPicker.LowColorChange);

						biomeColorPicker.updateOldSwatches();

						bTrans = SCANcontroller.controller.biomeTransparency;

						if (bigMap != null)
						{
							if (bigMap.MType == mapType.Biome)
								bigMap.resetMap(SCANcontroller.controller.map_ResourceOverlay);
						}
					}
				}
				else
				{
					GUILayout.Label("Apply Values", SCANskins.SCAN_button, GUILayout.Width(110));
					fillS(8);
					GUILayout.Label("Default Values", SCANskins.SCAN_button, GUILayout.Width(110));
				}
			stopE();
			fillS(8);
			if (!dropDown)
			{
				if (GUILayout.Button(textWithTT("Save Values To Config", colorHelpSaveToConfig), GUILayout.Width(180)))
				{
					dropDown = true;
					saveWarning = true;
				}
			}
			else
				GUILayout.Label("Save Values To Config", SCANskins.SCAN_button, GUILayout.Width(180));
		}

		private void slopeConfirm(int id)
		{
			if (!dropDown)
			{
				fillS(20);
				
				if (GUILayout.Button("Apply Values", GUILayout.Width(110)))
				{
					SCANcontroller.controller.lowSlopeColorOne = slopeColorPickerLow.ColorLow;
					SCANcontroller.controller.highSlopeColorOne = slopeColorPickerLow.ColorHigh;
					SCANcontroller.controller.lowSlopeColorTwo = slopeColorPickerHigh.ColorLow;
					SCANcontroller.controller.highSlopeColorTwo = slopeColorPickerHigh.ColorHigh;
					SCANcontroller.controller.lowSlopeColorOne32 = slopeColorPickerLow.ColorLow;
					SCANcontroller.controller.highSlopeColorOne32 = slopeColorPickerLow.ColorHigh;
					SCANcontroller.controller.lowSlopeColorTwo32 = slopeColorPickerHigh.ColorLow;
					SCANcontroller.controller.highSlopeColorTwo32 = slopeColorPickerHigh.ColorHigh;
					SCANcontroller.controller.slopeCutoff = sCutoff;

					slopeColorPickerLow.updateOldSwatches();
					slopeColorPickerHigh.updateOldSwatches();

					if (bigMap != null)
					{
						if (bigMap.MType == mapType.Slope)
							bigMap.resetMap(SCANcontroller.controller.map_ResourceOverlay);
					}

				}

				fillS(8);

				if (GUILayout.Button("Default Values", GUILayout.Width(110)))
				{
					SCANcontroller.controller.lowSlopeColorOne = SCANconfigLoader.SCANNode.BottomLowSlopeColor;
					SCANcontroller.controller.highSlopeColorOne = SCANconfigLoader.SCANNode.BottomHighSlopeColor;
					SCANcontroller.controller.lowSlopeColorTwo = SCANconfigLoader.SCANNode.TopLowSlopeColor;
					SCANcontroller.controller.highSlopeColorTwo = SCANconfigLoader.SCANNode.TopHighSlopeColor;
					SCANcontroller.controller.lowSlopeColorOne32 = SCANconfigLoader.SCANNode.BottomLowSlopeColor;
					SCANcontroller.controller.highSlopeColorOne32 = SCANconfigLoader.SCANNode.BottomHighSlopeColor;
					SCANcontroller.controller.lowSlopeColorTwo32 = SCANconfigLoader.SCANNode.TopLowSlopeColor;
					SCANcontroller.controller.highSlopeColorTwo32 = SCANconfigLoader.SCANNode.TopHighSlopeColor;
					SCANcontroller.controller.slopeCutoff = SCANconfigLoader.SCANNode.SlopeCutoff;

					slopeColorPickerLow = new SCANuiColorPicker(SCANcontroller.controller.lowSlopeColorOne, SCANcontroller.controller.highSlopeColorOne, colorPickerHelpLow, colorPickerHelpHigh, colorPickerHelpValue, slopeColorPickerLow.LowColorChange);
					slopeColorPickerHigh = new SCANuiColorPicker(SCANcontroller.controller.lowSlopeColorTwo, SCANcontroller.controller.highSlopeColorTwo, colorPickerHelpLow, colorPickerHelpHigh, colorPickerHelpValue, slopeColorPickerHigh.LowColorChange);

					sCutoff = SCANcontroller.controller.slopeCutoff;

					slopeColorPickerLow.updateOldSwatches();
					slopeColorPickerHigh.updateOldSwatches();

					if (bigMap != null)
					{
						if (bigMap.MType == mapType.Slope)
							bigMap.resetMap(SCANcontroller.controller.map_ResourceOverlay);
					}
				}

				fillS(20);

				if (GUILayout.Button(textWithTT("Save Values To Config", colorHelpSaveToConfig), GUILayout.Width(180)))
				{
					dropDown = true;
					saveWarning = true;
				}
			}
			else
			{
				fillS(20);
				GUILayout.Label("Apply Values", SCANskins.SCAN_button, GUILayout.Width(110));
				fillS(8);
				GUILayout.Label("Default Values", SCANskins.SCAN_button, GUILayout.Width(110));
				fillS(20);
				GUILayout.Label("Save Values To Config", SCANskins.SCAN_button, GUILayout.Width(180));
			}
		}

		private void resourceConfirm(int id)
		{
			fillS(10);
			growE();
				if (!dropDown)
				{
					if (GUILayout.Button(textWithTT("Apply Values", colorResourceHelpApply), GUILayout.Width(110)))
					{
						currentResource.MinColor = resourceColorPicker.ColorLow;
						currentResource.MaxColor = resourceColorPicker.ColorHigh;

						SCANcontroller.updateSCANresource(currentResource, false);

						updateUI();

						if (bigMap != null && SCANcontroller.controller.map_ResourceOverlay)
							bigMap.resetMap(SCANcontroller.controller.map_ResourceOverlay);
					}

					fillS(6);

					if (GUILayout.Button(textWithTT("Apply To All Planets", colorResourceHelpApplyAll), GUILayout.Width(200)))
					{
						for (int i = 0; i < currentResource.getBodyCount; i++)
						{
							SCANresourceBody r = currentResource.getBodyConfig(i);
							if (r != null)
							{
								r.MinValue = lowRCutoff;
								r.MaxValue = highRCutoff;
							}
						}

						currentResource.MinColor = resourceColorPicker.ColorLow;
						currentResource.MaxColor = resourceColorPicker.ColorHigh;

						SCANcontroller.updateSCANresource(currentResource, true);

						updateUI();

						if (bigMap != null && SCANcontroller.controller.map_ResourceOverlay)
							bigMap.resetMap(SCANcontroller.controller.map_ResourceOverlay);
					}
				}
				else
				{
					GUILayout.Label("Apply Values", SCANskins.SCAN_button, GUILayout.Width(110));
					fillS(6);
					GUILayout.Label("Apply To All Planets", SCANskins.SCAN_button, GUILayout.Width(200));
				}
			stopE();
			fillS(8);
			growE();
				if (!dropDown)
				{
					if (GUILayout.Button(textWithTT("Default Values", colorResourceHelpDefault), GUILayout.Width(110)))
					{
						currentResource.CurrentBody.MinValue = currentResource.CurrentBody.DefaultMinValue;
						currentResource.CurrentBody.MaxValue = currentResource.CurrentBody.DefaultMaxValue;
						currentResource.MinColor = currentResource.DefaultLowColor;
						currentResource.MaxColor = currentResource.DefaultHighColor;
						currentResource.Transparency = currentResource.DefaultTrans;

						SCANcontroller.updateSCANresource(currentResource, false);

						updateUI();

						if (bigMap != null && SCANcontroller.controller.map_ResourceOverlay)
							bigMap.resetMap(SCANcontroller.controller.map_ResourceOverlay);
					}

					fillS(6);

					if (GUILayout.Button(textWithTT("Default Values For All Planets", colorResourceHelpDefaultAll), GUILayout.Width(200)))
					{
						currentResource.MinColor = currentResource.DefaultLowColor;
						currentResource.MaxColor = currentResource.DefaultHighColor;
						currentResource.Transparency = currentResource.DefaultTrans;

						for (int i = 0; i < currentResource.getBodyCount; i++)
						{
							SCANresourceBody r = currentResource.getBodyConfig(i);
							if (r != null)
							{
								r.MinValue = r.DefaultMinValue;
								r.MaxValue = r.DefaultMaxValue;
							}
						}

						SCANcontroller.updateSCANresource(currentResource, true);

						updateUI();

						if (bigMap != null && SCANcontroller.controller.map_ResourceOverlay)
							bigMap.resetMap(SCANcontroller.controller.map_ResourceOverlay);
					}
				}
				else
				{
					GUILayout.Label("Default Values", SCANskins.SCAN_button, GUILayout.Width(110));
					fillS(6);
					GUILayout.Label("Default Values For All Planets", SCANskins.SCAN_button, GUILayout.Width(200));
				}
			stopE();
			fillS(8);
			if (!dropDown)
			{
				if (GUILayout.Button(textWithTT("Save Values To Config", colorHelpSaveToConfig), GUILayout.Width(180)))
				{
					dropDown = true;
					saveWarning = true;
				}
			}
			else
				GUILayout.Label("Save Values To Config", SCANskins.SCAN_button, GUILayout.Width(180));
		}

		//Drop down menu for palette selection
		private void dropDownBox(int id)
		{
			if (dropDown)
			{
				if (paletteBox && windowMode == 0)
				{
					ddRect = new Rect(40, 120, 100, 100);
					GUI.Box(ddRect, "");
					for (int i = 0; i < Palette.kindNames.Length; i++)
					{
						Rect r = new Rect(ddRect.x + 10, ddRect.y + 5 + (i * 23), 80, 22);
						if (GUI.Button(r, Palette.kindNames[i], SCANskins.SCAN_dropDownButton))
						{
							paletteBox = false;
							palette.CurrentPalettes = palette.setCurrentPalettesType((Palette.Kind)i, (int)pSize);
							setSizeSlider((Palette.Kind)i);
						}
					}
				}
				else if (resourceBox && windowMode == 3)
				{
					ddRect = new Rect(WindowRect.width - 240, 112, 160, 140);
					GUI.Box(ddRect, "");
					for (int i = 0; i < loadedResources.Count; i ++)
					{
						scrollR = GUI.BeginScrollView(ddRect, scrollR, new Rect(0, 0, 140, 23 * loadedResources.Count));
						Rect r = new Rect(2, i * 23, 136, 22);
						if (GUI.Button(r, loadedResources[i].Name, currentResource.Name == loadedResources[i].Name ? SCANskins.SCAN_dropDownButtonActive : SCANskins.SCAN_dropDownButton))
						{
							currentResource = new SCANresourceGlobal(loadedResources[i]);
							currentResource.CurrentBodyConfig(body.name);

							fineControlMode = oldFineControl = false;

							updateUI();

							dropDown = false;
							resourceBox = false;
						}
						GUI.EndScrollView();
					}
				}
				else if (planetBox)
				{
					ddRect = new Rect(WindowRect.width - 250, 78, 180, 180);
					GUI.Box(ddRect, "");
					for (int i = 0; i < FlightGlobals.Bodies.Count; i++)
					{
						CelestialBody b = FlightGlobals.Bodies[i];
						scrollR = GUI.BeginScrollView(ddRect, scrollR, new Rect(0, 0, 140, 23 * FlightGlobals.Bodies.Count));
						Rect r = new Rect(2, i * 23, 136, 22);
						if (GUI.Button(r, b.name, body == b ? SCANskins.SCAN_dropDownButtonActive : SCANskins.SCAN_dropDownButton))
						{
							body = b;

							bodyIndex = body.flightGlobalsIndex;

							if (windowMode == 0)
							{
								setBodyTerrain();

								currentTerrain = new SCANterrainConfig(bodyTerrain);
							}
							else if (windowMode == 3)
							{
								currentResource.CurrentBodyConfig(body.name);

								lowRCutoff = currentResource.CurrentBody.MinValue;
								highRCutoff = currentResource.CurrentBody.MaxValue;

								oldFineControl = fineControlMode = false;

								setResourceSliders();
							}

							updateUI();

							dropDown = false;
							planetBox = false;
						}
						GUI.EndScrollView();
					}
				}
				else if (saveWarning)
				{
					ddRect = new Rect(WindowRect.width - 182, WindowRect.height - 92, 180, 90);
					GUI.Box(ddRect, "");
					Rect r = new Rect(ddRect.x + 10, ddRect.y, 160, 60);
					GUI.Label(r, "Overwrite Existing Config File?", SCANskins.SCAN_headlineSmall);

					r.x += 40;
					r.y += 55;
					r.width = 80;
					r.height = 30;

					if (GUI.Button(r, "Confirm", SCANskins.SCAN_buttonWarning))
					{
						dropDown = false;
						saveWarning = false;
						SCANconfigLoader.SCANNode.Save();
					}
				}
				else
					dropDown = false;
			}
		}

		private void updateUI()
		{
			if (windowMode == 0)
			{
				minT = currentTerrain.MinTerrain;
				maxT = currentTerrain.MaxTerrain;
				clampT = currentTerrain.ClampTerrain ?? currentTerrain.MinTerrain + 10f;
				pSize = currentTerrain.PalSize;
				oldReverseState = currentTerrain.PalRev;
				oldDiscreteState = currentTerrain.PalDis;
				oldClampState = clampState = currentTerrain.ClampTerrain != null;
				palette.CurrentPalettes = palette.setCurrentPalettesType(currentTerrain.ColorPal.kind, (int)pSize);
				minTerrainSlider.valueChanged();
				maxTerrainSlider.valueChanged();
				clampTerrainSlider.valueChanged();
				paletteSizeSlider.valueChanged();
				paletteIndex = currentTerrain.ColorPal.index;

				regenPaletteSets();
				setSizeSlider(currentTerrain.ColorPal.kind);
				setTerrainSliders();
				drawCurrentLegend();
			}
			else if (windowMode == 3)
			{
				lowRCutoff = currentResource.CurrentBody.MinValue;
				highRCutoff = currentResource.CurrentBody.MaxValue;
				rTrans = currentResource.Transparency;

				resourceColorPicker = new SCANuiColorPicker(currentResource.MinColor, currentResource.MaxColor, colorPickerHelpLow, colorPickerHelpHigh, colorPickerHelpValue, resourceColorPicker.LowColorChange);

				resourceColorPicker.updateOldSwatches();

				setResourceSliders();
			}
		}

		//Draws the palette swatch for the currently active SCANdata selection
		private void drawCurrentLegend()
		{
			currentLegend = new SCANmapLegend();
			currentLegend.Legend = currentLegend.getLegend(0, bodyTerrain);
		}

		//Draws the palette swatch for the newly adjusted palette
		private void drawPreviewLegend()
		{
			float? clamp = null;
			Color32[] c = currentTerrain.ColorPal.colors;
			if (clampState)
				clamp = clampT;
			if (currentTerrain.PalRev)
				c = currentTerrain.ColorPal.colorsReverse;
			previewLegend = new SCANmapLegend();
			previewLegend.Legend = previewLegend.getLegend(maxT, minT, maxT - minT, clamp, currentTerrain.PalDis, c);
		}

		//Resets the palettes whenever the size slider is adjusted
		private void regenPaletteSets()
		{
			palette.CurrentPalettes = palette.setCurrentPalettesType(palette.CurrentPalettes.paletteType, (int)pSize);
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
			if (pSize > paletteSizeSlider.MaxValue)
				pSize = paletteSizeSlider.MaxValue;
			
		}

		//Dynamically adjust the min and max values on all of the terrain height sliders; avoids impossible values
		private void setTerrainSliders()
		{
			setBodyTerrain();

			minTerrainSlider.MinValue = bodyTerrain.DefaultMinHeight - SCANconfigLoader.SCANNode.RangeBelowMinHeight;
			maxTerrainSlider.MaxValue = bodyTerrain.DefaultMaxHeight + SCANconfigLoader.SCANNode.RangeAboveMaxHeight;
			minTerrainSlider.MaxValue = maxT - 100f;
			maxTerrainSlider.MinValue = minT + 100f;
			clampTerrainSlider.MinValue = minT + 10f;
			clampTerrainSlider.MaxValue = maxT - 10f;
			if (clampT < minT + 10f)
				clampT = minT + 10f;
			else if (clampT > maxT - 10f)
				clampT = maxT - 10f;
		}

		private void setResourceSliders()
		{
			if (fineControlMode)
			{
				if (highRCutoff < lowRCutoff + 5f)
					resourceMinSlider.MaxValue = highRCutoff - 0.1f;

				if (lowRCutoff > highRCutoff - 5f)
					resourceMaxSlider.MinValue = lowRCutoff + 0.1f;
			}
			else
			{
				resourceMinSlider.MinValue = 0f;
				resourceMinSlider.MaxValue = highRCutoff - 0.1f;
				resourceMaxSlider.MinValue = lowRCutoff + 0.1f;
				resourceMaxSlider.MaxValue = 100f;
			}
		}

		private void setBodyTerrain()
		{
			bodyTerrain = SCANcontroller.getTerrainNode(body.name);

			if (bodyTerrain == null)
			{
				float? clamp = null;
				if (body.ocean)
					clamp = 0;

				float newMax;

				try
				{
					newMax = ((float)CelestialUtilities.GetHighestPeak(body)).Mathf_Round(-2);
				}
				catch (Exception e)
				{
					SCANUtil.SCANlog("Error in calculating Max Height for {0}; using default value\n{1}", body.theName, e);
					newMax = SCANconfigLoader.SCANNode.DefaultMaxHeightRange;
				}

				bodyTerrain = new SCANterrainConfig(SCANconfigLoader.SCANNode.DefaultMinHeightRange, newMax, clamp, SCANUtil.paletteLoader(SCANconfigLoader.SCANNode.DefaultPalette, 7), 7, false, false, body);

				SCANcontroller.addToTerrainConfigData(body.name, bodyTerrain);
			}
		}

	}
}
