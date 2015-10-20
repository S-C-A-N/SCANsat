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
		public string settingsHelpAnomalies = "Select the marker used to display anomalies on the map";
		[Persistent]
		public string settingsHelpBackground = "Toggle background scanning on some or all celestial bodies";
		[Persistent]
		public string settingsHelpTimeWarp = "Adjust the scanning frequency\nduring timewarp\nHigher settings result in fewer gaps in the maps but may have a performance impact at high timewarp";
		[Persistent]
		public string settingsHelpGroundTracks = "Display a visible indicator of scanning activity in map mode";
		[Persistent]
		public string settingsHelpGroundTracksActive = "The ground track indicator can be limited to only be displayed for the active vessel";
		[Persistent]
		public string settingsHelpOverlayTooltips = "Displays tooltips for the current mouse position when a planetary overlay map is activated\nThese tooltips include\nThe cursor coordinates, terrain height, slope, biome name, and resource abundance, depending on scanning coverage";
		[Persistent]
		public string settingsHelpWindowTooltips = "Display tooltips on some map window buttons\nThese are primarily used to identify icon buttons";
		[Persistent]
		public string settingsHelpStockToolbar = "Use the stock toolbar\nOnly one stock button is available\nCan be used concurrently with the Blizzy78 Toolbar";
		[Persistent]
		public string settingsHelpMechJeb = "The SCANsat zoom map target selection mode can be used to select a MechJeb landing site";
		[Persistent]
		public string settingsHelpResetWindows = "Reset all window positions\nUse this in case a window has been dragged completely off screen or if any windows are not visible";
		[Persistent]
		public string settingsHelpResetPlanetData = "Resets all SCANsat data for the current celestial body\nA confirmation window will open before activating\nCannot be reversed";
		[Persistent]
		public string settingsHelpResetAllData = "Resets all SCANsat data for all celestial bodies\nA confirmation window will open before activating\nCannot be reversed";
		[Persistent]
		public string settingsHelpVesselsSensorsPasses = "Information about the currently active SCANsat sensors\nVessels indicates the number of vessels with active sensors\nSensors indicates the total number of sensors; instruments with multiple sensor types count each individual sensor\nPasses indicates the number of sensor updates performed per second\nThis value is affected by the TimeWarp Resolution setting";

		//Resource Settings Window Help Tooltips
		[Persistent]
		public string resourceSettingsHelpOverlayWindow = "Open the planetary overlay map control window";
		[Persistent]
		public string resourceSettingsHelpBiomeLock = "Circumvents the requirement for stock surface biome scans\nSCANsat displays will show the full accuracy for resource abundance with or without any surface biome scans";
		[Persistent]
		public string resourceSettingsHelpInstant = "By default, the stock M700 resource scanner's orbital survey will fill in all SCANsat resource maps\nthis can be disabled, requiring standard SCANsat methods for all resource scanning\nDisabled automatically when stock resource scanning is disabled";
		[Persistent]
		public string resourceSettingsHelpNarrowBand = "Numerous SCANsat functions require a Narrow-Band resource scanner on-board the current vessel or in orbit of a celestial body for fully accurate resource abundance data\nDisable this to circumvent these restrictions";
		[Persistent]
		public string resourceSettingsHelpDisableStock = "Disables all stock resource scanning functions\nSCANsat scanning methods will be required for all resource data\nReplaces several stock resource functions with SCANsat tools\nThese include\nThe right-click readouts, the high resolution narrow-band scanner map, and the planetary overlay maps";
		[Persistent]
		public string resourceSettingsHelpResetSCANsatResource = "Resets all SCANsat resource data for the current celestial body\nOther SCANsat data is not affected\nA confirmation window will open before activating\nCannot be reversed";
		[Persistent]
		public string resourceSettingsHelpResetStockResource = "Resets the stock resource scanning coverage for the\ncurrent celestial body\nA reload or scene change may be required for all changes to take effect\nA confirmation window will open before activating\nCannot be reversed";
		[Persistent]
		public string resourceSettingsHelpOverlayInterpolation = "Change the number of resource abundance measurements used in constructing the planetary overlay and big map resource overlay\nDecrease the value to increase the accuracy of the map\nLower values will result in slower map generation";
		[Persistent]
		public string resourceSettingsHelpOverlayHeight = "Change the texture size (map width is 2XHeight) used in constructing the planetary overlay and big map resource overlay\nIncrease the value to increase the quality and accuracy of the map\nHigher values will result in slower map generation";
		[Persistent]
		public string resourceSettingsHelpOverlayBiomeHeight = "Change the texture size (map width is 2XHeight) used in constructing the planetary overlay biome map\nIncrease the value to increase the quality and accuracy of the map\nHigher values will result in slower map generation";
		[Persistent]
		public string resourceSettingsHelpOverlayTransparency = "Create a grey background for planetary overlay resource maps\nUsed to make clear which sections of the celestial body have been scanned but contain no resources";

		//Color Config Window Help Tooltips
		[Persistent]
		public string colorTerrainHelpMin = "Defines the low altitude cutoff for the terrain color palette\nAnything below this altitude will be shown with the lowest color";
		[Persistent]
		public string colorTerrainHelpMax = "Defines the high altitude cutoff for the terrain color palette\nAnything above this altitude will be shown with the highest color";
		[Persistent]
		public string colorTerrainHelpClampToggle = "Used to define a cutoff between the low and high altitude values\nThis is particularly useful for ocean planets where it helps to define a sharp distinction between land and ocean";
		[Persistent]
		public string colorTerrainHelpClamp = "Defines the clamp altitude cutoff\nAnything below the cutoff will be represented by the first two colors in the selected color palette\nAnything above the cutoff will be represented with\nthe remaining colors";
		[Persistent]
		public string colorTerrainHelpReverse = "Reverses the order of the currently selected color palette";
		[Persistent]
		public string colorTerrainHelpDiscrete = "Draws the map using only the specific colors defined by each palette, instead of smoothly interpolating between them";
		[Persistent]
		public string colorTerrainHelpPaletteSize = "Adjust the number of colors available in the currently selected color palette";
		[Persistent]
		public string colorBiomeHelpStock = "Use the stock biome color scheme for SCANsat biome maps";
		[Persistent]
		public string colorBiomeHelpWhiteBorder = "Draw a white border between biomes\nDoes not apply to the planetary overlay biome maps";
		[Persistent]
		public string colorBiomeHelpTransparency = "Adjust the transparency\nof biome maps\nTerrain elevation is shown behind the biome maps\nSet to 0% to disable terrain drawing";
		[Persistent]
		public string colorPickerHelpLow = "The top color swatch shows the updated color selection for the low end of this color spectrum\nThe bottom color swatch shows the currently active color";
		[Persistent]
		public string colorPickerHelpHigh = "The top color swatch shows the updated color selection for the high end of this color spectrum\nThe bottom color swatch shows the currently active color";
		[Persistent]
		public string colorPickerHelpValue = "This slider adjusts the Value (in HSV color terms) or Brightness for the currently selected color";
		[Persistent]
		public string colorResourceHelpFineControl = "Activates Fine Control Mode where the sliders will only show values within 5% of the current selection";
		[Persistent]
		public string colorResourceHelpMin = "The low cutoff for resource concentration on the selected celestial body\nResource deposits at this level will be displayed using the low end of the current resource overlay color spectrum\nResource deposits below this value will not be shown";
		[Persistent]
		public string colorResourceHelpMax = "The high cutoff for resource concentration on the selected celestial body\nResource deposits above this value will be shown using the high end of the current resource overlay color spectrum";
		[Persistent]
		public string colorResourceHelpTransparency = "Defines the level of transparency for resource overlays\nIncrease to allow more of the underlying terrain, slope, or biome map to be visible\nThis also affect the transparency of resource deposits on the planetary overlay resource map";
		[Persistent]
		public string colorResourceHelpApply = "Applies the current values for the selected resource and\ncelestial body only";
		[Persistent]
		public string colorResourceHelpApplyAll = "Applies the current values for the selected resource for\nall celestial bodies";
		[Persistent]
		public string colorResourceHelpDefault = "Reverts to the default values for the selected resource and\ncelestial body only";
		[Persistent]
		public string colorResourceHelpDefaultAll = "Reverts to the default values for the selected resource for\nall celestial bodies";
		[Persistent]
		public string colorHelpSaveToConfig = "Save all color configuration values to the config file found in your SCANsat/Resources folder\nThese values serve as the defaults for new saves and for all Revert To Default buttons\nValues do not need to be saved to the config file to be applied\nfor this save file";
	}
}
