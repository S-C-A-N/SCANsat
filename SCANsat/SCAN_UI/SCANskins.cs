

using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.Platform;
using palette = SCANsat.SCANpalette;
using UnityEngine;


namespace SCANsat.SCAN_UI
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	class SCANskins: MBE
	{
		internal static GUISkin SCAN_skin;

		internal static GUIStyle SCAN_window;
		internal static GUIStyle SCAN_readout;
		internal static GUIStyle SCAN_toggle;
		internal static GUIStyle SCAN_tooltip;
		internal static GUIStyle SCAN_label;

		//Button styles
		internal static GUIStyle SCAN_button;
		internal static GUIStyle SCAN_buttonActive;
		internal static GUIStyle SCAN_buttonFixed;
		internal static GUIStyle SCAN_texButton;
		internal static GUIStyle SCAN_buttonBorderless;
		internal static GUIStyle SCAN_closeButton;

		//Map info readout styles
		internal static GUIStyle SCAN_colorLabel;
		internal static GUIStyle SCAN_whiteLabel;
		internal static GUIStyle SCAN_activeLabel;
		internal static GUIStyle SCAN_inactiveLabel;
		internal static GUIStyle SCAN_shadowLabel;

		//Instrument readout styles
		internal static GUIStyle SCAN_insColorLabel;
		internal static GUIStyle SCAN_insWhiteLabel;
		internal static GUIStyle SCAN_anomalyOverlay;
		internal static GUIStyle SCAN_overlay;

		//Resource overlay styles
		internal static GUIStyle SCAN_resource;
		internal static GUIStyle SCAN_resourceReadout;

		//Settings menu styles
		internal static GUIStyle SCAN_headline;

		//Styles for vessel overlay icons
		internal static GUIStyle SCAN_orbitalLabelOn;
		internal static GUIStyle SCAN_orbitalLabelOff;

		internal static Font dotty;

		protected override void OnGUI_FirstRun()
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
			SCAN_button.normal.textColor = palette.white;
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

			SCAN_buttonBorderless = new GUIStyle(SCAN_button);
			SCAN_buttonBorderless.name = "SCAN_ButtonBorderless";
			SCAN_buttonBorderless.fontSize = 14;
			SCAN_buttonBorderless.margin = new RectOffset(2, 2, 2, 2);
			SCAN_buttonBorderless.padding = new RectOffset(0, 2, 2, 2);
			SCAN_buttonBorderless.normal.background = SCAN_SkinsLibrary.DefUnitySkin.label.normal.background;

			SCAN_closeButton = new GUIStyle(SCAN_buttonBorderless);
			SCAN_closeButton.name = "SCAN_CloseButton";
			SCAN_closeButton.normal.textColor = palette.cb_vermillion;

			//Initialize info readout styles
			SCAN_colorLabel = new GUIStyle(SCAN_label);
			SCAN_colorLabel.name = "SCAN_ColorLabel";

			SCAN_whiteLabel = new GUIStyle(SCAN_label);
			SCAN_whiteLabel.name = "SCAN_WhiteLabel";
			SCAN_whiteLabel.normal.textColor = palette.white;

			SCAN_activeLabel = new GUIStyle(SCAN_label);
			SCAN_activeLabel.name = "SCAN_ActiveLabel";
			SCAN_activeLabel.normal.textColor = palette.xkcd_PukeGreen;

			SCAN_inactiveLabel = new GUIStyle(SCAN_label);
			SCAN_inactiveLabel.name = "SCAN_InactiveLabel";
			SCAN_inactiveLabel.normal.textColor = palette.xkcd_LightGrey;

			SCAN_shadowLabel = new GUIStyle(SCAN_label);
			SCAN_shadowLabel.name = "SCAN_ShadowLabel";
			SCAN_shadowLabel.normal.textColor = palette.black;

			//Initialize instrument styles
			SCAN_insColorLabel = new GUIStyle(SCAN_colorLabel);
			SCAN_insColorLabel.name = "SCAN_InsColorLabel";
			SCAN_insColorLabel.alignment = TextAnchor.MiddleCenter;
			SCAN_insColorLabel.font = dotty;
			SCAN_insColorLabel.fontSize = 40;

			SCAN_insWhiteLabel = new GUIStyle(SCAN_whiteLabel);
			SCAN_insWhiteLabel.name = "SCAN_InsWhiteLabel";
			SCAN_insWhiteLabel.alignment = TextAnchor.MiddleCenter;
			SCAN_insWhiteLabel.font = dotty;
			SCAN_insWhiteLabel.fontSize = 40;

			SCAN_anomalyOverlay = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_anomalyOverlay.name = "SCAN_AnomalyOverlay";
			SCAN_anomalyOverlay.font = dotty;
			SCAN_anomalyOverlay.fontSize = 36;
			SCAN_anomalyOverlay.fontStyle = FontStyle.Bold;
			SCAN_anomalyOverlay.normal.textColor = palette.cb_skyBlue;

			//Initialize settings menu styles
			SCAN_headline = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_headline.name = "SCAN_Headline";
			SCAN_headline.normal.textColor = palette.xkcd_YellowGreen;
			SCAN_headline.alignment = TextAnchor.MiddleCenter;
			SCAN_headline.fontSize = 40;
			SCAN_headline.font = dotty;

			SCAN_readout = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_readout.font = dotty;

			SCAN_overlay = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_overlay.font = dotty;

			SCAN_toggle = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.toggle);
			SCAN_toggle.name = "SCAN_Toggle";

			SCAN_tooltip = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_tooltip.name = "SCAN_Tooltip";

			SCAN_orbitalLabelOn = new GUIStyle(SCAN_label);
			SCAN_orbitalLabelOn.name = "SCAN_OrbitalLabelOn";
			SCAN_orbitalLabelOn.fontSize = 13;
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
			SCAN_SkinsLibrary.AddStyle(SCAN_inactiveLabel, "SCAN_Unity");
			SCAN_SkinsLibrary.AddStyle(SCAN_shadowLabel, "SCAN_Unity");
		}
	}
}
