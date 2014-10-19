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
using SCANsat.Platform;
using SCANsat.Platform.Palettes;
using palette = SCANsat.SCAN_UI.SCANpalette;
using UnityEngine;

namespace SCANsat.SCAN_UI
{
	class SCANcolorSelection: SCAN_MBW
	{
		private bool paletteBox, reversePalette, discretePalette, clampTerrain;
		private Rect paletteRect;
		private Palettes currentPalettes;
		private string paletteSize = "5";
		private Texture2D currentLegend, previewLegend;
		private string lowRange = "-500";
		private string highRange = "8000";
		private string clampLevel = "0";
		private int lowRangeInt = -500;
		private int highRangeInt = 8000;
		private int clampLevelInt = 0;
		internal static Rect defaultRect = new Rect(100, 300, 600, 300);

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Color Management";
			WindowRect = defaultRect;
			WindowStyle = SCANskins.SCAN_window;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(600), GUILayout.Height(300) };
			Visible = false;
			DragEnabled = true;
			ClampToScreenOffset = new RectOffset(-400, -400, -250, -250);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");
		}

		internal override void Start()
		{
			currentPalettes = SCANpalette.CurrentPalettes;
		}

		internal override void OnDestroy()
		{
			
		}

		protected override void DrawWindowPre(int id)
		{
			currentLegend = currentPalettes.paletteSwatch[0];
			previewLegend = currentPalettes.paletteSwatch[1];
		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);
			closeBox(id);

			growS();

			growE();
				paletteTextures(id);
				paletteOptions(id);
			stopE();
			fillS(16);
			growE();
				palettePreview(id);
				fillS(20);
				paletteConfirmation(id);
			stopE();

			stopS();

			paletteSelectionBox(id);
		}

		protected override void DrawWindowPost(int id)
		{
			if (paletteBox && Event.current.type == EventType.mouseDown && !paletteRect.Contains(Event.current.mousePosition))
			{
				paletteBox = false;
			}
		}

		//Draw the version label in the upper left corner
		private void versionLabel(int id)
		{
			Rect r = new Rect(6, 0, 40, 18);
			GUI.Label(r, SCANversions.SCANsatVersion, SCANskins.SCAN_whiteReadoutLabel);
		}

		//Draw the close button in the upper right corner
		private void closeBox(int id)
		{
			Rect r = new Rect(WindowRect.width - 20, 0, 18, 18);
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				Visible = false;
			}
		}

		private void paletteTextures(int id)
		{
			growS();
				GUILayout.Label("Palette Selection", SCANskins.SCAN_headline);
				fillS(8);
				growE();
					if (GUILayout.Button("Palette Style:", SCANskins.SCAN_buttonFixed, GUILayout.MaxWidth(120)))
					{
						paletteBox = !paletteBox;
					}
					fillS(10);
					GUILayout.Label(SCANpalette.getPaletteType, SCANskins.SCAN_whiteReadoutLabel);
				stopE();
				growE();
					int j = 9;
					if (currentPalettes.paletteType == Palette.Kind.Qualitative)
						j = 8;
					else if (currentPalettes.paletteType == Palette.Kind.Sequential)
						j = 17;
					else if (currentPalettes.paletteType == Palette.Kind.Invertable || currentPalettes.paletteType == Palette.Kind.Unknown)
						j = 0;
					for (int i = 0; i < j; i++)
					{
						if (i % 3 == 0)
						{
							stopE();
							fillS(8);
							growE();
						}
						Texture2D t = currentPalettes.paletteSwatch[i];
						if (paletteBox)
						{
							GUILayout.Label("", GUILayout.Width(110), GUILayout.Height(25));
						}
						else
						{
							if (GUILayout.Button("", SCANskins.SCAN_texButton, GUILayout.Width(110), GUILayout.Height(25)))
							{
								SCANpalette.CurrentPalette = currentPalettes.availablePalettes[i];
							}
						}
						Rect r = GUILayoutUtility.GetLastRect();
						r.width -= 10;
						GUI.DrawTexture(r, t);
					}
				stopE();
			stopS();
		}

		private void paletteOptions(int id)
		{
			growS();
				GUILayout.Label("Palette Options", SCANskins.SCAN_headline);
				fillS(8);
				GUILayout.Label("Terrain Height Range", SCANskins.SCAN_headlineSmall);
				growE();
					fillS();
					GUILayout.Label("Min:", SCANskins.SCAN_whiteReadoutLabel, GUILayout.Width(40));
					lowRange = GUILayout.TextField(lowRange, 5, SCANskins.SCAN_whiteReadoutLabel, GUILayout.Width(40));
					fillS(10);
					GUILayout.Label("Max:", SCANskins.SCAN_whiteReadoutLabel, GUILayout.Width(40));
					highRange = GUILayout.TextField(highRange, 5, SCANskins.SCAN_whiteReadoutLabel, GUILayout.Width(40));
					fillS();
				stopE();
				growE();
					fillS();
					clampTerrain = GUILayout.Toggle(clampTerrain, "Clamp Terrain", GUILayout.Width(100));
					if (clampTerrain)
					{
						fillS(5);
						clampLevel = GUILayout.TextField(clampLevel, 5, SCANskins.SCAN_whiteReadoutLabel, GUILayout.Width(40));
					}
					fillS();
				stopE();
				GUILayout.Label("Palette Size", SCANskins.SCAN_headlineSmall);

				growE();
					reversePalette = GUILayout.Toggle(reversePalette, "Reverse Order");
					fillS(10);
					discretePalette = GUILayout.Toggle(discretePalette, "Discrete Gradient");
				stopE();
			stopS();
		}

		private void palettePreview(int id)
		{
			growS();
				GUILayout.Label("Current Palette", SCANskins.SCAN_headlineSmall);
				fillS(8);
				GUILayout.Label("", SCANskins.SCAN_legendTex, GUILayout.Width(180), GUILayout.Height(25));
				Rect r = GUILayoutUtility.GetLastRect();
				GUI.DrawTexture(r, currentLegend);
			stopS();
			fillS(8);
			growS();
				GUILayout.Label("New Palette", SCANskins.SCAN_headlineSmall);
				fillS(8);
				GUILayout.Label("", SCANskins.SCAN_legendTex, GUILayout.Width(180), GUILayout.Height(25));
				r = GUILayoutUtility.GetLastRect();
				GUI.DrawTexture(r, previewLegend);
			stopS();
		}

		private void paletteConfirmation(int id)
		{
			growS();
			fillS(16);
			if (GUILayout.Button("Apply", GUILayout.Width(80)))
			{

			}
			fillS(8);
			if (GUILayout.Button("Cancel", GUILayout.Width(80)))
			{

			}
			stopS();
		}

		//Drop down menu for palette selection
		private void paletteSelectionBox(int id)
		{
			if (paletteBox)
			{
				paletteRect = new Rect(40, 90, 100, 80);
				GUI.Box(paletteRect, "", SCANskins.SCAN_dropDownBox);
				for (int i = 0; i < Palette.kindNames.Length; i++)
				{
					Rect r = new Rect(paletteRect.x + 10, paletteRect.y + 5 + (i * 23), 80, 22);
					if (GUI.Button(r, Palette.kindNames[i], SCANskins.SCAN_dropDownButton))
					{
						paletteBox = false;
						currentPalettes = palette.CurrentPalettes = palette.generatePaletteSet(int.Parse(paletteSize), (Palette.Kind)i);
					}
				}
			}
		}

	}
}
