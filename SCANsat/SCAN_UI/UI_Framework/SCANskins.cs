#region license
/* 
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - Skins and Styles setup
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
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	class SCANskins: SCAN_MBE
	{
		internal static GUISkin SCAN_skin;

		internal static GUIStyle SCAN_window;
		internal static GUIStyle SCAN_tooltip;
		internal static GUIStyle SCAN_label;

		internal static GUIStyle SCAN_toggle;

		//Button styles
		internal static GUIStyle SCAN_button;
		internal static GUIStyle SCAN_buttonActive;
		internal static GUIStyle SCAN_buttonFixed;
		internal static GUIStyle SCAN_texButton;
		internal static GUIStyle SCAN_buttonBorderless;
		internal static GUIStyle SCAN_closeButton;

		//Map info readout styles
		internal static GUIStyle SCAN_readoutLabel;
		internal static GUIStyle SCAN_whiteReadoutLabel;
		internal static GUIStyle SCAN_activeReadoutLabel;
		internal static GUIStyle SCAN_inactiveReadoutLabel;
		internal static GUIStyle SCAN_shadowReadoutLabel;

		//Instrument readout styles
		internal static GUIStyle SCAN_insColorLabel;
		internal static GUIStyle SCAN_insWhiteLabel;
		internal static GUIStyle SCAN_anomalyOverlay;

		//Settings menu styles
		internal static GUIStyle SCAN_headline;
		internal static GUIStyle SCAN_headlineSmall;
		internal static GUIStyle SCAN_buttonWarning;
		//internal static GUIStyle SCAN_texButton;
		internal static GUIStyle SCAN_legendTex;
		internal static GUIStyle SCAN_textBox;
		internal static GUIStyle SCAN_settingsToggle;

		//Styles for map overlay icons
		internal static GUIStyle SCAN_orbitalLabelOn;
		internal static GUIStyle SCAN_orbitalLabelOff;

		//Drop down menu styles
		internal static GUIStyle SCAN_dropDownButton;
		internal static GUIStyle SCAN_dropDownBox;

		internal static Font dotty;

		//Some UI Textures
		internal static Texture2D SCAN_toggleOn;
		internal static Texture2D SCAN_toggleOnHover;
		//internal static Texture2D SCAN_toggleOff;
		internal static Texture2D SCAN_dropDownTex;

		//Map Icon Textures
		internal static Texture2D SCAN_GridIcon;
		internal static Texture2D SCAN_OrbitIcon;
		internal static Texture2D SCAN_FlagIcon;
		internal static Texture2D SCAN_LegendIcon;
		internal static Texture2D SCAN_ColorWheelIcon;

		protected override void OnGUI_FirstRun()
		{
			initializeTextures();
			initializeSkins();
			initializeColors();
		}

		private static void initializeTextures()
		{
			SCAN_toggleOn = GameDatabase.Instance.GetTexture("SCANsat/Icons/SCAN_Toggle", false);
			SCAN_toggleOnHover = GameDatabase.Instance.GetTexture("SCANsat/Icons/SCAN_Toggle_Hover", false);
			//SCAN_toggleOff = GameDatabase.Instance.GetTexture("SCANsat/Icons/SCAN_Toggle_Off", false);
			SCAN_dropDownTex = GameDatabase.Instance.GetTexture("SCANsat/Icons/DropDownTex", false);
			SCAN_GridIcon = GameDatabase.Instance.GetTexture("SCANsat/Icons/SCAN_Grid_Icon", false);
			SCAN_OrbitIcon = GameDatabase.Instance.GetTexture("SCANsat/Icons/SCAN_Orbit_Icon", false);
			SCAN_FlagIcon = GameDatabase.Instance.GetTexture("SCANsat/Icons/SCAN_Flag_Icon", false);
			SCAN_LegendIcon = GameDatabase.Instance.GetTexture("SCANsat/Icons/SCAN_Legend_Icon", false);
			SCAN_ColorWheelIcon = GameDatabase.Instance.GetTexture("SCANsat/Icons/SCAN_ColorWheel_Icon", false);
		}

		private static void initializeColors()
		{
			palette.DivPaletteSet = palette.generatePaletteSet(7, Palette.Kind.Diverging);
			palette.QualPaletteSet = palette.generatePaletteSet(7, Palette.Kind.Qualitative);
			palette.SeqPaletteSet = palette.generatePaletteSet(7, Palette.Kind.Sequential);
			palette.CurrentPalettes = palette.DivPaletteSet;
			palette.CurrentPalette = PaletteLoader.defaultPalette;
		}

		private static void initializeSkins()
		{
			SCAN_skin = SCAN_SkinsLibrary.CopySkin("Unity");
			SCAN_SkinsLibrary.AddSkin("SCAN_Unity", SCAN_skin);

			ScreenMessages SM = (ScreenMessages)GameObject.FindObjectOfType(typeof(ScreenMessages));
			dotty = SM.textStyles[1].font;

			SCAN_window = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.window);
			SCAN_window.name = "SCAN_Window";

			SCAN_label = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_label.name = "SCAN_Label";
			SCAN_label.normal.textColor = palette.xkcd_PukeGreen;

			//Initialize button styles
			SCAN_button = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.button);
			SCAN_button.name = "SCAN_Button";
			SCAN_button.alignment = TextAnchor.MiddleCenter;
			SCAN_button.active.textColor = palette.xkcd_PukeGreen;

			SCAN_buttonActive = new GUIStyle(SCAN_button);
			SCAN_buttonActive.name = "SCAN_ButtonActive";
			SCAN_buttonActive.normal.textColor = palette.xkcd_PukeGreen;

			SCAN_buttonFixed = new GUIStyle(SCAN_button);
			SCAN_buttonFixed.name = "SCAN_ButtonFixed";
			SCAN_buttonFixed.active.textColor = SCAN_buttonFixed.normal.textColor;

			SCAN_texButton = new GUIStyle(SCAN_button);
			SCAN_texButton.name = "SCAN_TexButton";
			SCAN_texButton.padding = new RectOffset(0, 0, 1, 1);
			SCAN_texButton.normal.background = SCAN_SkinsLibrary.DefUnitySkin.label.normal.background;
			SCAN_texButton.hover.background = SCAN_SkinsLibrary.DefUnitySkin.label.normal.background;
			SCAN_texButton.active.background = SCAN_SkinsLibrary.DefUnitySkin.label.normal.background;

			SCAN_buttonBorderless = new GUIStyle(SCAN_button);
			SCAN_buttonBorderless.name = "SCAN_ButtonBorderless";
			SCAN_buttonBorderless.fontSize = 14;
			SCAN_buttonBorderless.margin = new RectOffset(2, 2, 2, 2);
			SCAN_buttonBorderless.padding = new RectOffset(0, 2, 2, 2);
			SCAN_buttonBorderless.normal.background = SCAN_SkinsLibrary.DefUnitySkin.label.normal.background;

			SCAN_closeButton = new GUIStyle(SCAN_buttonBorderless);
			SCAN_closeButton.name = "SCAN_CloseButton";
			SCAN_closeButton.normal.textColor = palette.cb_vermillion;

			//Initialize drop down menu styles
			SCAN_dropDownBox = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.box);
			SCAN_dropDownBox.name = "SCAN_DropDownBox";
			SCAN_dropDownBox.normal.background = SCAN_dropDownTex;

			SCAN_dropDownButton = new GUIStyle(SCAN_label);
			SCAN_dropDownButton.name = "SCAN_DropDownButton";
			SCAN_dropDownButton.padding = new RectOffset(2, 2, 2, 2);
			SCAN_dropDownButton.normal.textColor = palette.xkcd_PukeGreen;
			SCAN_dropDownButton.hover.textColor = palette.black;
			Texture2D sortBackground = new Texture2D(1, 1);
			sortBackground.SetPixel(1, 1, palette.xkcd_White);
			sortBackground.Apply();
			SCAN_dropDownButton.hover.background = sortBackground;
			SCAN_dropDownButton.alignment = TextAnchor.MiddleLeft;

			//Initialize info readout styles
			SCAN_readoutLabel = new GUIStyle(SCAN_label);
			SCAN_readoutLabel.name = "SCAN_ReadoutLabel";
			SCAN_readoutLabel.fontStyle = FontStyle.Bold;

			SCAN_whiteReadoutLabel = new GUIStyle(SCAN_readoutLabel);
			SCAN_whiteReadoutLabel.name = "SCAN_WhiteLabel";
			SCAN_whiteReadoutLabel.normal.textColor = palette.white;

			SCAN_activeReadoutLabel = new GUIStyle(SCAN_readoutLabel);
			SCAN_activeReadoutLabel.name = "SCAN_ActiveLabel";
			SCAN_activeReadoutLabel.normal.textColor = palette.cb_bluishGreen;

			SCAN_inactiveReadoutLabel = new GUIStyle(SCAN_readoutLabel);
			SCAN_inactiveReadoutLabel.name = "SCAN_InactiveLabel";
			SCAN_inactiveReadoutLabel.normal.textColor = palette.xkcd_LightGrey;

			SCAN_shadowReadoutLabel = new GUIStyle(SCAN_readoutLabel);
			SCAN_shadowReadoutLabel.name = "SCAN_ShadowLabel";
			SCAN_shadowReadoutLabel.normal.textColor = palette.black;

			//Initialize instrument styles
			SCAN_insColorLabel = new GUIStyle(SCAN_label);
			SCAN_insColorLabel.name = "SCAN_InsColorLabel";
			SCAN_insColorLabel.alignment = TextAnchor.MiddleCenter;
			SCAN_insColorLabel.font = dotty;
			SCAN_insColorLabel.fontSize = 36;

			SCAN_insWhiteLabel = new GUIStyle(SCAN_whiteReadoutLabel);
			SCAN_insWhiteLabel.name = "SCAN_InsWhiteLabel";
			SCAN_insWhiteLabel.alignment = TextAnchor.MiddleCenter;
			SCAN_insWhiteLabel.fontStyle = FontStyle.Normal;
			SCAN_insWhiteLabel.font = dotty;
			SCAN_insWhiteLabel.fontSize = 36;

			SCAN_anomalyOverlay = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_anomalyOverlay.name = "SCAN_AnomalyOverlay";
			SCAN_anomalyOverlay.font = dotty;
			SCAN_anomalyOverlay.fontSize = 32;
			SCAN_anomalyOverlay.fontStyle = FontStyle.Bold;
			SCAN_anomalyOverlay.normal.textColor = palette.cb_skyBlue;

			//Initialize settings menu styles
			SCAN_headline = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_headline.name = "SCAN_Headline";
			SCAN_headline.normal.textColor = palette.xkcd_YellowGreen;
			SCAN_headline.alignment = TextAnchor.MiddleCenter;
			SCAN_headline.fontSize = 40;
			SCAN_headline.font = dotty;

			SCAN_headlineSmall = new GUIStyle(SCAN_headline);
			SCAN_headlineSmall.name = "SCAN_HeadlineSmall";
			SCAN_headlineSmall.fontSize = 30;

			SCAN_buttonWarning = new GUIStyle(SCAN_button);
			SCAN_buttonWarning.name = "SCAN_ButtonWarning";
			SCAN_buttonWarning.fontSize = 16;
			SCAN_buttonWarning.fontStyle = FontStyle.Bold;
			SCAN_buttonWarning.normal.textColor = palette.cb_vermillion;

			SCAN_toggle = new GUIStyle(SCAN_SkinsLibrary.DefKSPSkin.toggle);

			//SCAN_toggle = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.toggle);
			SCAN_toggle.name = "SCAN_Toggle";
			//SCAN_toggle.border = new RectOffset(0, 0, 0, 0);
			//SCAN_toggle.alignment = TextAnchor.MiddleCenter;
			//SCAN_toggle.font = dotty;
			//SCAN_toggle.fontSize = 22;
			//SCAN_toggle.fixedHeight = 0;
			//SCAN_toggle.fixedWidth = 0;
			//SCAN_toggle.clipping = TextClipping.Clip;

			//SCANUtil.SCANlog("Fixed Height: {0}", SCAN_toggle.fixedHeight);
			//SCANUtil.SCANlog("Fixed Width: {0}", SCAN_toggle.fixedWidth);
			//SCANUtil.SCANlog("Stretch Height: {0}", SCAN_toggle.stretchHeight);
			//SCANUtil.SCANlog("Stretch Width: {0}", SCAN_toggle.stretchWidth);
			//SCANUtil.SCANlog("Font: {0}", SCAN_toggle.font.name);
			//SCANUtil.SCANlog("Height Depend Width: {0}", SCAN_toggle.isHeightDependantOnWidth);
			//SCANUtil.SCANlog("Content Offset: {0}", SCAN_toggle.contentOffset);

			SCAN_settingsToggle = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.toggle);
			//SCAN_settingsToggle.normal.background = SCAN_toggleOff;
			SCAN_settingsToggle.onNormal.background = SCAN_toggleOn;
			SCAN_settingsToggle.onHover.background = SCAN_toggleOnHover;
			SCAN_settingsToggle.onNormal.background.wrapMode = TextureWrapMode.Clamp;
			SCAN_settingsToggle.onHover.background.wrapMode = TextureWrapMode.Clamp;
			//SCAN_settingsToggle.normal.background.wrapMode = TextureWrapMode.Clamp;
			SCAN_settingsToggle.border = new RectOffset(15, 0, 1, 1);

			SCAN_textBox = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.textField);
			SCAN_textBox.name = "SCAN_TextBox";

			SCAN_legendTex = new GUIStyle(SCAN_label);
			SCAN_legendTex.name = "SCAN_LegendTex";
			SCAN_legendTex.alignment = TextAnchor.MiddleCenter;

			SCAN_tooltip = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_tooltip.name = "SCAN_Tooltip";

			SCAN_orbitalLabelOn = new GUIStyle(SCAN_label);
			SCAN_orbitalLabelOn.name = "SCAN_OrbitalLabelOn";
			SCAN_orbitalLabelOn.fontSize = 13;
			SCAN_orbitalLabelOn.fontStyle = FontStyle.Bold;
			SCAN_orbitalLabelOn.normal.textColor = palette.cb_yellow;

			SCAN_orbitalLabelOff = new GUIStyle(SCAN_orbitalLabelOn);
			SCAN_orbitalLabelOff.name = "SCAN_OrbitalLabelOff";
			SCAN_orbitalLabelOff.normal.textColor = palette.white;

			//Add styles to skin
			SCAN_SkinsLibrary.knownSkins["SCAN_Unity"].window = new GUIStyle(SCAN_window);
			SCAN_SkinsLibrary.knownSkins["SCAN_Unity"].button = new GUIStyle(SCAN_button);
			SCAN_SkinsLibrary.knownSkins["SCAN_Unity"].toggle = new GUIStyle(SCAN_toggle);
			SCAN_SkinsLibrary.knownSkins["SCAN_Unity"].label = new GUIStyle(SCAN_label);

			SCAN_SkinsLibrary.AddStyle(SCAN_window, "SCAN_Unity");
			SCAN_SkinsLibrary.AddStyle(SCAN_button, "SCAN_Unity");
			SCAN_SkinsLibrary.AddStyle(SCAN_toggle, "SCAN_Unity");
			SCAN_SkinsLibrary.AddStyle(SCAN_label, "SCAN_Unity");
			SCAN_SkinsLibrary.AddStyle(SCAN_tooltip, "SCAN_Unity");
		}

	}
}
