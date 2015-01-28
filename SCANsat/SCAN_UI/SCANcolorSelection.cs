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
		private bool paletteBox, reversePalette, oldReverseState, discretePalette, oldDiscreteState;
		private bool spaceCenterLock, trackingStationLock, clampState, oldClampState;
		private Rect paletteRect;
		private Palette dataPalette;
		private int paletteSizeInt, oldPaletteSizeInt = 6;
		private int paletteIndex;
		private SCANmapLegend currentLegend, previewLegend;
		private float sizeSlider, sizeSliderMin, sizeSliderMax, terrainSliderMinMin, terrainSliderMinMax, terrainSliderMaxMin, terrainSliderMaxMax, clampSliderMin, clampSliderMax;
		private float minHeightF, oldMinHeightF = -500;
		private float maxHeightF, oldMaxHeightF = 8000;
		private float clampHeightF = 0;
		private const string lockID = "colorLockID";
		internal static Rect defaultRect = new Rect(100, 400, 650, 330);

		//SCAN_MBW objects to sync the color selection fields to the currently displayed map
		private SCANkscMap kscMapObj;
		private SCANBigMap bigMapObj;

		private static SCANmap bigMap;
		private SCANdata data;

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
			paletteSizeInt = palette.CurrentPalettes.size;
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
			if (currentLegend == null || data.ColorPalette != dataPalette)
			{
				dataPalette = data.ColorPalette;
				minHeightF = data.MinHeight;
				oldMinHeightF = minHeightF;
				maxHeightF = data.MaxHeight;
				oldMaxHeightF = maxHeightF;
				setTerrainSliders();
				oldPaletteSizeInt = paletteSizeInt = data.PaletteSize;
				setSizeSlider(dataPalette.kind);
				sizeSlider = (float)paletteSizeInt;
				oldReverseState = reversePalette = data.PaletteReverse;
				oldDiscreteState = discretePalette = data.PaletteDiscrete;
				oldClampState = clampState = data.ClampHeight != null;
				if (clampState)
				{
					clampHeightF = (float)data.ClampHeight;
				}
				palette.CurrentPalettes = palette.setCurrentPalettesType(dataPalette.kind);
				palette.CurrentPalette = palette.CurrentPalettes.availablePalettes[0];
				regenPaletteSets();
				drawCurrentLegend();
			}
			if (previewLegend == null)
			{
				drawPreviewLegend();
			}
		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);					/* The standard version number and close button */
			closeBox(id);

			growS();
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
			stopS();

			paletteSelectionBox(id);			/* Draw the drop down menu for the palette selection box */
		}

		protected override void DrawWindowPost(int id)
		{
			if (paletteBox && Event.current.type == EventType.mouseDown && !paletteRect.Contains(Event.current.mousePosition))
				paletteBox = false;

			//These methods update all of the UI elements whenever any of the options are changed
			if (reversePalette != oldReverseState)
			{
				oldReverseState = reversePalette;
				drawPreviewLegend();
			}

			if (oldMinHeightF != minHeightF || oldMaxHeightF != maxHeightF)
			{
				oldMinHeightF = minHeightF;
				oldMaxHeightF = maxHeightF;
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

			if (paletteSizeInt != oldPaletteSizeInt)
			{
				if (paletteSizeInt > 2)
				{
					oldPaletteSizeInt = paletteSizeInt;
					sizeSlider = paletteSizeInt;
					regenPaletteSets();
					palette.CurrentPalette = palette.CurrentPalettes.availablePalettes[paletteIndex];
					drawPreviewLegend();
				}
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

		//Draw the palette selection field
		private void paletteTextures(int id)
		{
			growS();
				GUILayout.Label("Palette Selection", SCANskins.SCAN_headline);
				fillS(12);
				growE();
					if (GUILayout.Button("Palette Style:", SCANskins.SCAN_buttonFixed, GUILayout.MaxWidth(120)))
					{
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
					GUILayout.Label("Min: " + minHeightF + "m", SCANskins.SCAN_whiteReadoutLabel);

					Rect r = GUILayoutUtility.GetLastRect();
					r.x += 110;
					r.width = 130;
					
					minHeightF = GUI.HorizontalSlider(r, minHeightF, terrainSliderMinMin, terrainSliderMinMax).Mathf_Round(-2);

					SCANuiUtil.drawSliderLabel(r, terrainSliderMinMin + "m", terrainSliderMinMax + "m");
				stopE();
				fillS(8);
				growE();
					fillS(10);
					GUILayout.Label("Max: " + maxHeightF + "m", SCANskins.SCAN_whiteReadoutLabel);

					r = GUILayoutUtility.GetLastRect();
					r.x += 110;
					r.width = 130;

					maxHeightF =GUI.HorizontalSlider(r, maxHeightF, terrainSliderMaxMin, terrainSliderMaxMax).Mathf_Round(-2);

					SCANuiUtil.drawSliderLabel(r, terrainSliderMaxMin + "m", terrainSliderMaxMax + "m");
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
							GUILayout.Label("Clamp: " + clampHeightF + "m", SCANskins.SCAN_whiteReadoutLabel);

							r = GUILayoutUtility.GetLastRect();
							r.x += 110;
							r.width = 130;

							clampHeightF = GUI.HorizontalSlider(r, clampHeightF, clampSliderMin, clampSliderMax).Mathf_Round(-1);

							SCANuiUtil.drawSliderLabel(r, clampSliderMin + "m", clampSliderMax +  "m");
						stopE();
					}
				fillS(6);
				GUILayout.Label("Palette Options", SCANskins.SCAN_headlineSmall);
				if (palette.CurrentPalettes.paletteType != Palette.Kind.Fixed)
				{
					growE();
						fillS(10);
						GUILayout.Label("Palette Size: " + paletteSizeInt, SCANskins.SCAN_whiteReadoutLabel);

						r = GUILayoutUtility.GetLastRect();
						r.x += 110;
						r.width = 130;

						paletteSizeInt = Mathf.RoundToInt(GUI.HorizontalSlider(r, sizeSlider, sizeSliderMin, sizeSliderMax));

						SCANuiUtil.drawSliderLabel(r, sizeSliderMin + "  ", " " + sizeSliderMax);
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
					minHeightF = data.MinHeight;
					maxHeightF = data.MaxHeight;
					clampState = data.ClampHeight != null;
					if (clampState)
						clampHeightF = (float)data.ClampHeight;
					else
						clampHeightF = 0;
					dataPalette = palette.CurrentPalette = data.ColorPalette = data.DefaultColorPalette;
					palette.CurrentPalettes = palette.setCurrentPalettesType(dataPalette.kind);
					paletteSizeInt = data.PaletteSize = dataPalette.size;
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
						if (minHeightF < maxHeightF)
						{
							data.MinHeight = minHeightF;
							data.MaxHeight = maxHeightF;
						}
						if (clampState)
						{
							if (clampHeightF > minHeightF && clampHeightF < maxHeightF)
								data.ClampHeight = (float?)clampHeightF;
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

		//Drop down menu for palette selection
		private void paletteSelectionBox(int id)
		{
			if (paletteBox)
			{
				paletteRect = new Rect(40, 90, 100, 100);
				GUI.Box(paletteRect, "", SCANskins.SCAN_dropDownBox);
				for (int i = 0; i < Palette.kindNames.Length; i++)
				{
					Rect r = new Rect(paletteRect.x + 10, paletteRect.y + 5 + (i * 23), 80, 22);
					if (GUI.Button(r, Palette.kindNames[i], SCANskins.SCAN_dropDownButton))
					{
						paletteBox = false;
						palette.CurrentPalettes = palette.setCurrentPalettesType((Palette.Kind)i);
						setSizeSlider((Palette.Kind)i);
					}
				}
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
				clamp = (float?)clampHeightF;
			if (reversePalette)
				c = palette.CurrentPalette.colorsReverse;
			previewLegend = new SCANmapLegend();
			previewLegend.Legend = previewLegend.getLegend(maxHeightF, minHeightF, clamp, discretePalette, c);
			//previewLegend = SCANmapLegend.getLegend(maxHeightF, minHeightF, clamp, discretePalette, c);
		}

		//Resets the palettes whenever the size slider is adjusted
		private void regenPaletteSets()
		{
			palette.DivPaletteSet = palette.generatePaletteSet(paletteSizeInt, Palette.Kind.Diverging);
			palette.QualPaletteSet = palette.generatePaletteSet(paletteSizeInt, Palette.Kind.Qualitative);
			palette.SeqPaletteSet = palette.generatePaletteSet(paletteSizeInt, Palette.Kind.Sequential);
			palette.FixedPaletteSet = palette.generatePaletteSet(0, Palette.Kind.Fixed);
			palette.CurrentPalettes = palette.setCurrentPalettesType(palette.getPaletteType);
		}

		//Change the max range on the palette size slider based on palette type
		private void setSizeSlider(Palette.Kind k)
		{
			switch (k)
			{
				case Palette.Kind.Diverging:
					{
						sizeSliderMin = 3f;
						sizeSliderMax = 11f;
						if (paletteSizeInt > sizeSliderMax)
							paletteSizeInt = (int)sizeSliderMax;
						break;
					}
				case Palette.Kind.Qualitative:
					{
						sizeSliderMin = 3f;
						sizeSliderMax = 12f;
						if (paletteSizeInt > sizeSliderMax)
							paletteSizeInt = (int)sizeSliderMax;
						break;
					}
				case Palette.Kind.Sequential:
					{
						sizeSliderMin = 3f;
						sizeSliderMax = 9f;
						if (paletteSizeInt > sizeSliderMax)
							paletteSizeInt = (int)sizeSliderMax;
						break;
					}
				case Palette.Kind.Fixed:
					{
						break;
					}
			}
			
		}

		//Dynamically adjust the min and max values on all of the terrain height sliders; avoids impossible values
		private void setTerrainSliders()
		{
			terrainSliderMinMin = data.DefaultMinHeight - 10000f;
			terrainSliderMaxMax = data.DefaultMaxHeight + 10000f;
			terrainSliderMinMax = maxHeightF - 100f;
			terrainSliderMaxMin = minHeightF + 100f;
			clampSliderMin = minHeightF + 10f;
			clampSliderMax = maxHeightF - 10f;
			if (clampHeightF < minHeightF + 10f)
				clampHeightF = minHeightF + 10f;
			else if (clampHeightF > maxHeightF - 10f)
				clampHeightF = maxHeightF - 10f;
		}

	}
}
