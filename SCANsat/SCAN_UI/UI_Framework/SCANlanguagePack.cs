using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_Platform;

namespace SCANsat.SCAN_UI.UI_Framework
{
	public class SCANlanguagePack : SCAN_ConfigNodeStorage
	{
		[Persistent]
		public bool activePack = true;
		[Persistent]
		public string language = "English (USA)";

		//Settings Window Help Tooltips
		[Persistent]
		public string settingsHelpAnomalies = "";
		[Persistent]
		public string settingsHelpBackground = "";
		[Persistent]
		public string settingsHelpTimeWarp = "";
		[Persistent]
		public string settingsHelpGroundTracks = "";
		[Persistent]
		public string settingsHelpGroundTracksActive = "";
		[Persistent]
		public string settingsHelpOverlayTooltips = "";
		[Persistent]
		public string settingsHelpWindowTooltips = "";
		[Persistent]
		public string settingsHelpStockToolbar = "";
		[Persistent]
		public string settingsHelpMechJeb = "";
		[Persistent]
		public string settingsHelpResetWindows = "";
		[Persistent]
		public string settingsHelpResetPlanetData = "";
		[Persistent]
		public string settingsHelpResetAllData = "";
		[Persistent]
		public string settingsHelpVesselsSensorsPasses = "";

		//Resource Settings Window Help Tooltips
		[Persistent]
		public string resourceSettingsHelpOverlayWindow = "";
		[Persistent]
		public string resourceSettingsHelpBiomeLock = "";
		[Persistent]
		public string resourceSettingsHelpInstant = "";
		[Persistent]
		public string resourceSettingsHelpNarrowBand = "";
		[Persistent]
		public string resourceSettingsHelpDisableStock = "";
		[Persistent]
		public string resourceSettingsHelpResetSCANsatResource = "";
		[Persistent]
		public string resourceSettingsHelpResetStockResource = "";
		[Persistent]
		public string resourceSettingsHelpOverlayInterpolation = "";
		[Persistent]
		public string resourceSettingsHelpOverlayHeight = "";
		[Persistent]
		public string resourceSettingsHelpOverlayBiomeHeight = "";
		[Persistent]
		public string resourceSettingsHelpOverlayTransparency = "";

	}
}
