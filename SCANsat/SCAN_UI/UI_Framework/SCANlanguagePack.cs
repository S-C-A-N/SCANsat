using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_Platform;
using System.Text.RegularExpressions;
using System.Reflection;

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
		public string settingsHelpAnomalies = "Select the marker used to display\nanomalies on the map.";
		[Persistent]
		public string settingsHelpScanningToggle = "Toggle all SCANsat scanning.";
		[Persistent]
		public string settingsHelpBackground = "Toggle background scanning on\nselected celestial bodies.";
		[Persistent]
		public string settingsHelpTimeWarp = "Adjust the scanning frequency during TimeWarp.\nHigher settings result in fewer gaps in the maps\nbut may have a performance impact at high TimeWarp.";
		[Persistent]
		public string settingsHelpGroundTracks = "Display a visible indicator of\nscanning activity in map mode.";
		[Persistent]
		public string settingsHelpGroundTracksActive = "The ground track indicator can be limited\nto only be displayed for the active vessel.";
		[Persistent]
		public string settingsHelpStockUIStyle = "Switch between stock KSP-style UI elements and Unity-style elements."; 
		[Persistent]
		public string settingsHelpOverlayTooltips = "Displays tooltips for the current mouse position when a planetary overlay map is activated.\nThese tooltips include The cursor coordinates, terrain height, slope, biome name,\nand resource abundance, depending on scanning coverage.";
		[Persistent]
		public string settingsHelpWindowTooltips = "Display tooltips on some map window buttons.\nThese are primarily used to identify icon buttons.";
		[Persistent]
		public string settingsHelpStockToolbar = "Use the stock toolbar.\nCan be used concurrently with the Blizzy78 Toolbar.";
		[Persistent]
		public string settingsHelpToolbarMenu = "Use a pop-out menu for the stock toolbar to show all available windows.";
		[Persistent]
		public string settingsHelpMechJeb = "The SCANsat zoom map target selection mode can be used to select a MechJeb landing site.";
		[Persistent]
		public string settingsHelpResetWindows = "Reset all window positions and scale.\nUse this in case a window has been dragged completely\noff screen or if any windows are not visible.";
		[Persistent]
		public string settingsHelpResetPlanetData = "Resets all SCANsat data for the current celestial body.\nA confirmation window will open before activating.\nCannot be reversed.";
		[Persistent]
		public string settingsHelpResetAllData = "Resets all SCANsat data for all celestial bodies.\nA confirmation window will open before activating.\nCannot be reversed.";
		[Persistent]
		public string settingsHelpVesselsSensorsPasses = "Information about the currently active SCANsat sensors.\nVessels indicates the number of vessels with active sensors.\nSensors indicates the total number of sensors;\ninstruments with multiple sensor types count each individual sensor.\nPasses indicates the number of sensor updates performed per second.\nThis value is affected by the TimeWarp Resolution setting.";
		[Persistent]
		public string settingsHelpGreyScale = "Use a true grey-scale color spectrum for black-and-white SCANsat maps.\nPixels on the altitude map will interpolate between black and white;\nthe min and max terrain heights for each celestial body define the limits.";
		[Persistent]
		public string settingsHelpExportCSV = "Export a .csv file along with map texture when using the Export button on the big map.\nThe file contains coordinates and the terrain height for each pixel.\nPixels are labeled from left to right and from top to bottom.";
		[Persistent]
		public string settingsHelpSetMapWidth = "Enter an exact value for the SCANsat big map texture width.\nValues are limited to 550 - 8192 pixels wide.\nPress the Set button to apply the value.";
		[Persistent]
		public string settingsHelpWindowScale = "Adjust all SCANsat window scales.";
		[Persistent]
		public string settingsHelpFillPlanet = "Fill in all SCANsat data for the current celestial body.";
		[Persistent]
		public string settingsHelpFillAll = "Fill in all SCANsat data for all celestial bodies.";

		//Resource Settings Window Help Tooltips
		[Persistent]
		public string resourceSettingsHelpBiomeLock = "Circumvents the requirement for stock surface biome scans.\nSCANsat displays will show the full accuracy for resource\nabundance with or without any surface biome scans.";
		[Persistent]
		public string resourceSettingsHelpInstant = "By default, the stock M700 resource scanner's orbital survey will fill in all SCANsat resource maps.\nThis can be disabled, requiring standard SCANsat methods for all resource scanning.\nDisabled automatically when stock resource scanning is disabled.";
		[Persistent]
		public string resourceSettingsHelpNarrowBand = "Numerous SCANsat functions require a Narrow-Band resource scanner on-board\nthe current vessel or in orbit of a celestial body for fully accurate resource abundance data.\nDisable this to circumvent these restrictions.";
		[Persistent]
		public string resourceSettingsHelpDisableStock = "Disables all stock resource scanning functions.\nSCANsat scanning methods will be required for all resource data.\nReplaces several stock resource functions with SCANsat tools.\nThese include The right-click readouts, and the planetary overlay maps.";
		[Persistent]
		public string resourceSettingsHelpResetSCANsatResource = "Resets all SCANsat resource data for the current celestial body.\nOther SCANsat data is not affected.\nA confirmation window will open before activating.\nCannot be reversed.";
		[Persistent]
		public string resourceSettingsHelpResetStockResource = "Resets the stock resource scanning coverage for the current celestial body.\nA reload or scene change may be required for all changes to take effect.\nA confirmation window will open before activating.\nCannot be reversed.";
		[Persistent]
		public string resourceSettingsHelpOverlayInterpolation = "Change the number of resource abundance measurements used in constructing\nthe planetary overlay and big map resource overlay.\nDecrease the value to increase the accuracy of the map.\nLower values will result in slower map generation.";
		[Persistent]
		public string resourceSettingsHelpOverlayHeight = "Change the texture size (map width is 2XHeight) used in constructing\nthe planetary overlay and big map resource overlay.\nIncrease the value to increase the quality and accuracy of the map.\nHigher values will result in slower map generation.";
		[Persistent]
		public string resourceSettingsHelpOverlayBiomeHeight = "Change the texture size (map width is 2XHeight) used in constructing\nthe planetary overlay biome map. Increase the value to increase\nthe quality and accuracy of the map.\nHigher values will result in slower map generation.";
		[Persistent]
		public string resourceSettingsHelpOverlayTransparency = "Create a grey background for planetary overlay resource maps.\nUsed to make clear which sections of the celestial body\nhave been scanned but contain no resources.";
		[Persistent]
		public string resourceSettingsHelpScanThreshold = "A threshold level used to apply the stock resource scan to a celestial body after scanning with SCANsat sensors.\nThis is useful when contracts or other addons require that a stock resource scan be performed.\nSet a value from 0-100 in the text box and click on the Set button.\nAll celestial bodies will be checked immediately;\ncelestial bodies will also be checked upon loading or a scene change.\nA reload may be required for the changes to take effect.";

		//Color Config Window Help Tooltips
		[Persistent]
		public string colorTerrainHelpMin = "Defines the low altitude cutoff for the terrain color palette.\nAnything below this altitude will be shown with the lowest color.";
		[Persistent]
		public string colorTerrainHelpMax = "Defines the high altitude cutoff for the terrain color palette.\nAnything above this altitude will be shown with the highest color.";
		[Persistent]
		public string colorTerrainHelpClampToggle = "Used to define a cutoff between the low and high altitude values.\nThis is particularly useful for ocean planets where it helps to\ndefine a sharp distinction between land and ocean.";
		[Persistent]
		public string colorTerrainHelpClamp = "Defines the clamp altitude cutoff.\nAnything below the cutoff will be represented by the first two colors in the selected color palette.\nAnything above the cutoff will be represented with the remaining colors.";
		[Persistent]
		public string colorTerrainHelpReverse = "Reverses the order of the currently\nselected color palette.";
		[Persistent]
		public string colorTerrainHelpDiscrete = "Draws the map using only the specific colors defined by each palette,\ninstead of smoothly interpolating between them.";
		[Persistent]
		public string colorTerrainHelpPaletteSize = "Adjust the number of colors available\nin the currently selected color palette.";
		[Persistent]
		public string colorBiomeHelpStock = "Use the stock biome color scheme for\nSCANsat biome maps.";
		[Persistent]
		public string colorBiomeHelpWhiteBorder = "Draw a white border between biomes.\nDoes not apply to the planetary overlay biome maps.";
		[Persistent]
		public string colorBiomeHelpTransparency = "Adjust the transparency of biome maps.\nTerrain elevation is shown behind the biome maps.\nSet to 0% to disable terrain drawing.";
		[Persistent]
		public string colorPickerHelpLow = "The top color swatch shows the updated\ncolor selection for the low end of this color spectrum.\nThe bottom color swatch shows the currently active color.";
		[Persistent]
		public string colorPickerHelpHigh = "The top color swatch shows the updated\ncolor selection for the high end of this color spectrum.\nThe bottom color swatch shows the currently active color.";
		[Persistent]
		public string colorResourceHelpMin = "The low cutoff for resource concentration on the selected celestial body.\nResource deposits at this level will be displayed\nusing the low end of the current resource overlay color spectrum.\nResource deposits below this value will not be shown.";
		[Persistent]
		public string colorResourceHelpMax = "The high cutoff for resource concentration on the selected celestial body.\nResource deposits above this value will be displayed\nusing the high end of the current resource overlay color spectrum.";
		[Persistent]
		public string colorResourceHelpTransparency = "Defines the level of transparency for resource overlays.\nIncrease to allow more of the underlying terrain, slope, or biome map to be visible.\nThis also affect the transparency of resource deposits on the planetary overlay resource map.";
		[Persistent]
		public string colorResourceHelpApply = "Applies the current values for the\nselected resource and celestial body only.";
		[Persistent]
		public string colorResourceHelpApplyAll = "Applies the current values for the\nselected resource for all celestial bodies.";
		[Persistent]
		public string colorResourceHelpDefault = "Reverts to the default values for the\nselected resource and celestial body only.";
		[Persistent]
		public string colorResourceHelpDefaultAll = "Reverts to the default values for the\nselected resource for all celestial bodies.";
		[Persistent]
		public string colorHelpSaveToConfig = "Save all color configuration values to\nthe config file found in your SCANsat/Resources folder.\nThese values serve as the defaults for new saves\nand for all Revert To Default buttons.\nValues do not need to be saved to the config file to be applied for this save file.";
		[Persistent]
		public string colorSlopeHelpCutoff = "Adjust the cutoff level between\nthe two selected slope color pairs.";

		//Window toggle buttons
		[Persistent]
		public string mainMapToggle = "Main Map";
		[Persistent]
		public string bigMapToggle = "Big Map";
		[Persistent]
		public string zoomMapToggle = "Zoom Map";
		[Persistent]
		public string overlayToggle = "Planetary Overlay";
		[Persistent]
		public string instrumentsToggle = "Instruments Readout";
		[Persistent]
		public string settingsToggle = "Settings";

		//Main Map tooltips
		[Persistent]
		public string mainMapColor = "Map Color";
		[Persistent]
		public string mainMapType = "Terrain/Biome Toggle";
		[Persistent]
		public string mainMapMinimize = "Show/Hide Vessel Info";
		[Persistent]
		public string mainMapStatus = "Scanner Status Indicators";
		[Persistent]
		public string mainMapPercentage = "Active Scanner Completion Percentage";

		//Big Map tooltips
		[Persistent]
		public string bigMapRefresh = "Map Refresh";
		[Persistent]
		public string bigMapColor = "Map Color";
		[Persistent]
		public string bigMapGrid = "Grid Overlay";
		[Persistent]
		public string bigMapOrbit = "Orbit Overlay";
		[Persistent]
		public string bigMapWaypoints = "Waypoints";
		[Persistent]
		public string bigMapAnomaly = "Anomalies";
		[Persistent]
		public string bigMapFlags = "Flags";
		[Persistent]
		public string bigMapLegend = "Map Legend";
		[Persistent]
		public string bigMapResource = "Resource Overlay";
		[Persistent]
		public string bigMapExport = "Export Map To Disk";

		//Overlay tooltips
		[Persistent]
		public string overlayRefresh = "Map Refresh";

		//Instruments tooltips
		[Persistent]
		public string insNextResource = "Next Resource";
		[Persistent]
		public string insPreviousResource = "Previous Resource";

		//Zoom Map tooltips
		[Persistent]
		public string zoomVesselSync = "Sync Current Vessel";
		[Persistent]
		public string zoomVesselLock = "Lock Position To Vessel";
		[Persistent]
		public string zoomMapRefresh = "Map Refresh";
		[Persistent]
		public string zoomMapWindowState = "Toggle Window Size";
		[Persistent]
		public string zoomMapIn = "Zoom In";
		[Persistent]
		public string zoomMapOut = "Zoom Out";
		[Persistent]
		public string zoomMapLeft = "Shift Left";
		[Persistent]
		public string zoomMapRight = "Shift Right";
		[Persistent]
		public string zoomMapUp = "Shift Up";
		[Persistent]
		public string zoomMapDown = "Shift Down";
		[Persistent]
		public string zoomMapIcons = "Map Icons";

		//Waypoint tooltips
		[Persistent]
		public string waypointToggle = "Waypoint Selector";
		[Persistent]
		public string waypointSet = "Create Waypoint";
		[Persistent]
		public string waypointCancel = "Cancel Waypoint";
		[Persistent]
		public string waypointMechJeb = "Set MechJeb Landing Target";
		[Persistent]
		public string waypointNameRefresh = "Reset Waypoint";

		//Warning labels
		[Persistent]
		public string warningDataResetCurrent = "Erase all map data for {0}?";
		[Persistent]
		public string warningDataResetAll = "Erase map data for all celestial bodies?";
		[Persistent]
		public string warningSCANResourceResetCurrent = "Erase SCANsat resource data for {0}?";
		[Persistent]
		public string warningSCANResourceResetAll = "Erase SCANsat resource data for all celestial bodies?";
		[Persistent]
		public string warningStockResourceResetCurrent = "Erase stock resource data for {0}?";
		[Persistent]
		public string warningStockResourceResetAll = "Erase stock resource data for all celestial bodies?";
		[Persistent]
		public string warningModuleManagerResource = "Warning\nModule Manager is required for all SCANsat resource scanning functions.";
		[Persistent]
		public string warningSaveToConfig = "Overwrite existing config file on disk?";
		
		public override void OnDecodeFromConfigNode()
		{
			Regex openBracket = new Regex(@"\[(?=\d+:?\w?\d?\])");

			Regex closeBraket = new Regex(@"(?<=\{\d+:?\w?\d?)\]");

			Regex newLines = new Regex(@"\\n");

			var stringFields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).Where(a => a.FieldType == typeof(string)).ToList();

			for (int i = stringFields.Count - 1; i >= 0; i--)
			{				
				FieldInfo f = stringFields[i];

				f.SetValue(this, openBracket.Replace((string)f.GetValue(this), "{"));

				f.SetValue(this, closeBraket.Replace((string)f.GetValue(this), "}"));

				f.SetValue(this, newLines.Replace((string)f.GetValue(this), Environment.NewLine));
			}
		}

		public override void OnEncodeToConfigNode()
		{
			Regex openCurlyBracket = new Regex(@"\{(?=\d+:?\w?\d?\})");

			Regex closeCurlyBraket = new Regex(@"(?<=\[\d+:?\w?\d?)\}");

			Regex newLines = new Regex(@"\n");

			var stringFields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).Where(a => a.FieldType == typeof(string)).ToList();

			for (int i = stringFields.Count - 1; i >= 0; i--)
			{
				FieldInfo f = stringFields[i];

				f.SetValue(this, openCurlyBracket.Replace((string)f.GetValue(this), "["));

				f.SetValue(this, closeCurlyBraket.Replace((string)f.GetValue(this), "]"));

				f.SetValue(this, newLines.Replace((string)f.GetValue(this), @"\n"));
			}
		}

		public string GetStringWithName(string title)
		{
			var stringFields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).Where(a => a.FieldType == typeof(string)).ToList();

			for (int i = stringFields.Count - 1; i >= 0; i--)
			{
				FieldInfo f = stringFields[i];

				if (f.Name != title)
					continue;

				return (string)f.GetValue(this);
			}

			return "";
		}

	}
}
