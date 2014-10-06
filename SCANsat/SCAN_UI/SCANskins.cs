

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
		internal static GUIStyle SCAN_headline;
		internal static GUIStyle SCAN_readout;
		internal static GUIStyle SCAN_button;
		internal static GUIStyle SCAN_texButton;
		internal static GUIStyle SCAN_toggle;
		internal static GUIStyle SCAN_overlay;
		internal static GUIStyle SCAN_tooltip;

		//Styles for map information labels
		internal static GUIStyle SCAN_label;
		internal static GUIStyle SCAN_closeLabel;
		internal static GUIStyle SCAN_inactiveLabel;
		internal static GUIStyle SCAN_shadowLabel;

		//Styles for vessel overlays
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

			SCAN_headline = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_headline.normal.textColor = palette.xkcd_YellowGreen;
			SCAN_headline.alignment = TextAnchor.MiddleCenter;
			SCAN_headline.fontSize = 40;
			SCAN_headline.font = dotty;

			SCAN_label = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_label.name = "SCAN_Label";
			SCAN_label.normal.textColor = palette.xkcd_PukeGreen;

			SCAN_inactiveLabel = new GUIStyle(SCAN_label);
			SCAN_inactiveLabel.name = "SCAN_InactiveLabel";
			SCAN_inactiveLabel.normal.textColor = palette.white;

			SCAN_shadowLabel = new GUIStyle(SCAN_label);
			SCAN_shadowLabel.name = "SCAN_ShadowLabel";
			SCAN_shadowLabel.normal.textColor = palette.black;

			SCAN_readout = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_readout.font = dotty;

			SCAN_overlay = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.label);
			SCAN_overlay.font = dotty;

			SCAN_button = new GUIStyle(SCAN_SkinsLibrary.DefUnitySkin.button);
			SCAN_button.name = "SCAN_Button";

			SCAN_texButton = new GUIStyle(SCAN_button);
			SCAN_texButton.name = "SCAN_TexButton";
			SCAN_texButton.alignment = TextAnchor.MiddleCenter;
			SCAN_texButton.fontSize = 14;
			SCAN_texButton.padding = new RectOffset(1, 1, 1, 1);
			SCAN_texButton.normal.background = SCAN_SkinsLibrary.DefUnitySkin.label.normal.background;

			SCAN_closeLabel = new GUIStyle(SCAN_texButton);
			SCAN_closeLabel.name = "SCAN_CloseLabel";
			SCAN_closeLabel.normal.textColor = palette.cb_vermillion;

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
