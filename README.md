### [**SCANsat**][top]: Real Scanning, Real Science, Warp Speed!
[![][shield:support-ksp]][KSP:developers]&nbsp;
[![][shield:ckan]][CKAN:org]&nbsp;
[![][shield:license-bsd]][SCANsat:rel-license]&nbsp;
[![][shield:license-mit]][SCANsat:dev-license]&nbsp;
[![][shield:license-cc-by-sa]][SCANsat:dev-license]&nbsp;
> ![scan your planetoid like the big boys do][bigmap-scan-10000x]
> ###### **Example SAR scan of Kerbin at 1000x and then 10,000x warp**

[![][shield:support-rpm]][RPM:release]&nbsp;
[![][shield:support-mm]][mm:release]&nbsp;
[![][shield:support-toolbar]][toolbar:release]&nbsp;
[![][shield:support-karbonite]][karbonite:release]&nbsp;
[![][shield:support-usi]][usi:release]&nbsp;
[![][shield:support-epl]][epl:release]&nbsp;
[![][shield:support-ctt]][ctt:release]&nbsp;
[![][shield:support-ccfg]][cconfig:release]&nbsp;
[![][shield:support-mechjeb]][mechjeb:release]&nbsp;

**Table of Contents**
------------------------------------------

* [0. People, Facts, and FAQs][0]
  * [a. FAQs][0a]
  * [b. Video Overview][0b]
* [1. Installation and Interoperability][1]
  * [a. Installation][1a]
  * [b. GameData Layout][1b]
  * [c. Other Add-Ons][1c]
* [2. Types of Scans][2]
  * [a. Native SCANsat][2a]
  * [b. Resource Scans][2b]
  * [c. Resource Settings][2c]
* [3. Basic Usage][3]
  * [a. FAQ: Finding a Good Altitude][3a]
  * [b. Mismatched Scanners][3b]
* [4. Big Map][4]
  * [a. Big Map Options][4a]
* [5. Zoom Map][5]
  * [a. Target Selection][5a]
  * [b. MechJeb Landing Guidance][5b]
* [6. Instrument Window][6]
* [7. Parts and Sensors Types][7]
  * [a. RADAR][7a]
  * [b. SAR][7b]
  * [c. Multi][7c]
  * [d. BTDT][7d]
  * [e. MapTraq (deprecated)][7e]
* [8. (Career Mode) Research and Development][8]
  * [a. Minimum Scan for Science (30%)][8a]
  * [b. Getting Maximum Science][8b]
  * [c. Contracts][8c]
* [9. Color Management][9]
  * [a. Terrain Colors and Options][9a]
  * [b. Biome Colors and Options][9b]
  * [c. Resource Colors and Options][9c]
* [10 Background Scanning][10]
* [11. Time Warp][11]
* [12. Settings Menu][12]
* [13. Note: Data Sources][13]

**WARNING**:

This add-on is a **work**-in-**progress**.

This means you should expect that it **may not work**, and you should be unsurprised if it **does not progress**.

Disclaimer aside, this add-on is widely used and it **usually** works just fine.

### [:top:][top] 0. People, Facts, and FAQs
------------------------------------------

#### Maintainers 
The current maintainer is:
  + [DMagic][DMagic] \<<david.grandy@gmail.com>\>

Maintainers are the people who you should complain to if there is something wrong.

Complaints in various forms are prioritized as follows:

  1. [Pull Requests][SCANsat:pulls] are given the **highest** priority possible. ~ 24 hour response
  2. [Issues][SCANsat:issues] are given *higher* priority than other complaints. ~ 2 day response
  3. [E-Mails][SCANsat:email] will be answered as soon as possible (it's a forwarded list) ~ 3 day response
  4. [Forum Posts][SCANsat:rel-thread] are given a medium priority. ~ 1 week response
  5. [Forum Private Messages](http://forum.kerbalspaceprogram.com/private.php) are given a low priority. We might forget!
  6. [Reddit Posts and PMs][KSP:reddit] are the lowest priority. We often lurk and don't login!

If you submit a well-reasoned pull request, you may even trigger a new release!

#### Authors
The current authors include:
  + [technogeeky][technogeeky] \<<technogeeky@gmail.com>\>
  + [DMagic][DMagic] \<<david.grandy@gmail.com>\>

Past authors include:
  + [damny][damny] \<<missing-in-action@nowhere-to-be-found.com>\>

As of August 2014, the vast majority of code is damny's and DMagic's; and technogeeky and is slowly helping out here and there.

#### Contributors

In addition to the authors, the following people have contributed:
  + (Models, Graphics, Textures) [Milkshakefiend][Milkshakefiend]
  
  + (Science results text) [madsailor][madsailor]

#### Licenses

For licensing information, please see the [included LICENSE.txt][SCANsat:rel-license] file.

[Source Code][SCANsat:rel-source] is available, as some licenses may require.

### [:top:][top] 0a. FAQs

  * What does SCANsat do?
    * It allows you to scan planetary bodies for terrain, biome, and resource information and generate various kinds of maps.
  * How does SCANsat affect gameplay?
    * It allows you to see surface details from orbit from an interactive, zoom-able map. This will help you plan your missions (for example, landing near a divider between two or three biomes) and provide critical information you need to attempt a safe landing (for instance, the slope map will help you avoid treacherous hills)
  * Will this version break my existing scans from older versions of SCANsat?
    * **No!** This version is completely backwards compatible, and you current scanning state (which is stored in persistent.sfs) will be safe and sound. Nevertheless, you should make a backup copy of your game before upgading any mod.
  * Do I need to attach a part to my vessel to use SCANsat?
    * **No, but...**. You can view existing maps from any vessel, but you need to attach a scanner to add new data to the maps.
  * [Career Mode] Does SCANsat give us science points?
    * **Yes!** For each type of map, if you scan at least 30% of the surface, you can yse Data for partial science points; up until the maximum value at 95% map coverage.
  * [Career Mode] Is it integrated into the tech tree?
    * **Yes!** **[This link][8]** tells you which nodes unlock which parts in the tech tree.
  * [Contracts] Does SCANsat offer contracts to complete?
    * **Yes/No.** Contracts are currently only supported through [**third-party addons**][6d].
  * Can you add <some feature or change> to SCANsat?
    * **Probably!** First, check the issues page to see if it's already been requested. If not, add a new issue. Even better, attempt to add the feature yourself and submit a pull request. We'll catch the bugs for you!

### [:top:][top] 0b. Video Overview

##### SCANsat overview and review by: [TinyPirate][tinypirate]
   * Watch this quick video on the features and functions of SCANsat
> [![][tinypirate-video-screen]][tinypirate-video]



### [:top:][top] 1. Installation and Interoperability 
------------------------------------------

#### [:top:][top] 1a. Installation 

  1. Download the latest [SCANsat package][SCANsat:rel-dist-github] from the releases section of this GitHub Repo
  2. Extract the included package and put the SCANsat folder in your KSP installation's GameData folder.

#### [:top:][top] 1b. GameData Layout 

![][SCANsat:gamedata]

#### [:top:][top] 1c. Other Add-Ons 

S.C.A.N. is proud to collaborate with other KSP mods and modding teams. Following is a table of all of the mods, add-ons, or software that we interoperate with.

**Built Using** | **Supported By**
:---: | :---:
[**MechJeb**][mechjeb:release] | [![Support for MKS][usi:logo]][usi:release]
[**Blizzy78's Toolbar**][toolbar:release] | [![Support for Karbonite][karbonite:logo]][karbonite:release]
[**ModuleManager**][mm:release]  | [![Support for Community Tech Tree][ctt:logo]][ctt:release]
[**RasterPropMonitor**][rpm:release]  | [![Support for ALCOR][alcor:logo]][alcor:release]

* **SCANsat**
  * [x] [**v12.0**][SCANsat:rel-thread] SCANsat Release **version: v12.0**
  * [x] [**v13.0**][SCANsat:dev-thread] SCANsat Dev **version: v13.0**

 **MM**, **RPM**, **MechJeb**, and **Toolbar** are all **soft** dependencies. This means your experience with SCANsat will be enhanced if you are using these mods, but they are not necessary.

**SCANsat** is built against the following mods:
  * [x] [![][shield:support-mm]][mm:release]
  * [x] [![][shield:support-rpm]][rpm:release]
  * [x] [![][shield:support-alcor]][alcor:release]
  * [x] [![][shield:support-toolbar]][toolbar:release] 
  * [x] [![][shield:support-mechjeb]][mechjeb:release]


### [:top:][top] 2. Types of Scans 
------------------------------------------

SCANsat supports several different kinds of scans (as opposed to
scanning modules or parts).

  * **RadarLo**: Basic, Low-Resolution RADAR Altimetry (b&w, limited zoom)
  * **RadarHi**: Advanced, High-Resolution RADAR Altimetry (in color, unlimited zoom)
  * **Slope**: Slope Data converted from RADAR data
  * **Biome**: Biome Detection and Classification (in color, unlimited zoom)
  * **Anomaly**: Anomaly Detection and Labeling
  * **Resource**: Scan for chemical or mineral resource on the surface.

Other parts and add-ons are free to include one or more of these kinds of scans. In general,
we would request that similar (same order of magnitude) scanning parameters and limitations are used
on custom parts, but this is not a requirement.

#### [:top:][top] 2a. SCANsat Scans

Without any resource scanning mods installed, **SCANsat** can scan for a few basic types of data. All of these (non-resource) scans are shown as indicators on the Small Map.

> ![][small-newMap1]

Data Type | Scan Type | Scan Indicator
:--- | :--- | :---:
Altimetry | **RADAR** | **LO**
Altimetry | **SAR** | **HI**
Biome | **Biome** | **MULTI**
Anomaly | **Anomaly** | **MULTI**
Anomaly | **Been There, Done That(tm)** | **BTDT**

* The **Slope** map is generated from either **HI** or **LO** data.
* The **Biome** scan only works for bodies that have biomes. For vanilla KSP, this means all planets except the sun and Jool.
* **Anomalies** are things the builds of KSC, hidden easter eggs, etc...
* The **Anomaly** data scans for anomalies from orbit, while
* **BTDT** shows a camera view of an anomaly once you are near it
* The **Biome** and **Anomaly** scans are combined into the multi-spectral scanner; indicated by **MULTI**

#### [:top:][top] 2b. Resource Scans

**SCANsat** will scan celestial bodies for resources using the new stock resource system.

With default resource scanning options enabled the SCANsat resource map will automatically update as soon as a **stock resource scan** is completed.
> ![][resource-instant]

Resource scans are initiated in the same way as any other scan. In this case they use the stock **Orbital Survey Scanner**.
> ![][resource-scanner]

Resource scanning proceeds the same way as standard SCANsat scanning instruments do. The grey scale color option generally works best when viewing resource overlays.
> ![][resource-bigmap]

The resource system can be enabled through the **SCANsat** Big Map:
> ![][resource-walkthrough]

Zoom map resource overlays require that a vessel with a narrow-band scanner be present in orbit and at an inclination high enough to cover the area in the zoom map.
> ![][resource-zoom-map-covered]

If a vessel with a narrow-band scanner is not present, or its inclination is not high enough, the zoom map will not display the resource overlay.
> ![][resource-zoom-map-uncovered]

Resource overlays will work in IVA, too:
> ![][resource-iva]

#### [:top:][top] 2c. Resource Setting

A number of options are available in the **Settings Menu** for SCANsat resource scanning.

> ![][resource-settings]

* **Instant Resource Scan**
   * When this option is active all resources will be fully scanned for a given planet using the stock **Orbital Survey Scanner** instrument.
   * When disabled the SCANsat resource overlays will need to be generated using the method described above.
* **Resource Biome Lock**
   * With this option active biomes will need to be scanned from the surface to obtain accurate resource abundance reading on SCANsat maps.
   * When disabled all resource abundance values will be fully accurate, with no need for ground surveys.
* **Zoom Requires Narrow Band Scanner**
   * With this active the zoom map will only display resource overlays when a suitable **Narrow-Band Scanner** is in orbit around the planet, and its orbit covers the region showed in the zoom map.
   * When disabled the zoom map will display resource overlays regardless of **Narrow-Band Scanner** coverage.
* **Reset Resource Coverage**
   * This button will erase all resource scanning data for the current planet.
   * Regular SCANsat data will not be affected.
   * A confirmation window will appear upon clicking the button.
   
### [:top:][top] 3. Basic Usage
------------------------------------------

Put scanner part on rocket, aim rocket at sky, launch. If your rocket is not pointing at the sky, you are probably not going to map today, because most sensors only work above 5 km.

You can start scanning by selecting a SCANsat part's context menu, enabling the part. Here, you will find a **small map**.

#### [:top:][top] 3a. FAQ: Finding a Good Altitude

Watch the data indicators on the small map to determine how well your scanners are performing. The right-click context menus also contain indicators for the proper scanning altitude.


###### too high
Solid ORANGE means you're too high (and therefore no data is being recorded):
> ![][small-toohigh-v10]

###### too low
Flashing ORANGE/GREEN means you're too low (and therefore you have a FOV penalty):
> ![][small-toolow-v10]

###### just right
Solid GREEN means you're in an ideal orbit. Notice the larger swath width on the right:
> ![][small-justright-v10]

#### [:top:][top] 3b. Mismatched Scanners

In these examples, the SAR and Multi sensors are not very well matched. Because the SAR sensors is ideal above 750km, and becuase it has a large field of view penalty if it's down near the ideal for Multi (250km), these sensors probably should not be used on the same scanner.

BIO and ANOM are ideal, but HI is not! | HI is ideal, but BIO and ANOM are off!
---|---
![][small-mismatch1] | ![][small-mismatch2]

SAR (HI) has thin swaths due to low alt. | Multi and RADAR have similar ideal swaths
--- | ---
![][small-scan-color] | ![][small-scan-bw]

The mapping interface consists of a small-ish map of the planet, as far as it has been scanned in your current game. It scans and updates quickly and shows positions of the active vessel, as well as other scanning vessels in orbit around the same planet. Orbital information is also provided. For a slower but more detailed view, see the **[big map][4]**.

Note that the indicators flash blue when the gray-scale color option is selected on the big map.

Be sure to remember to pack enough batteries, radioisotope generators, and solar panels. If you forget, you'll run out of electricity, you'll stop recording data, and you'll see useless static:

###### Static! Oh no, adjust the rabbit ears!
> ![][small-static]

### [:top:][top] 4. Big Map
------------------------------------------

> ![A Big Big Map][bigmap-anim-v2]

A bigger map can be rendered on demand. Rendered maps are automatically
saved to GameData/SCANsat/PluginData. Note that position indicators for
vessels or anomalies are not visible on exported images (but they may be a future release).

You can mouse over the big map to see what sensors have data for the location, as well as terrain elevation, and other details.

Right-clicking on the big map shows a magnified view around the position where you clicked. Mouse operations work inside this magnified view just like they work outside, meaning the data displayed at the bottom window applies to your position inside the magnified view, and right-clicking inside it will increase magnification. This can be useful to find landing spots which won't kill your kerbals.

#### [:top:][top] 4a. Big Map Options

There are four drop-down menus along the top of the big map. These control, from left to right: 

* The map projection type - Rectangular, KavrayskiyVII:, or Polar
* The map type - Altimetry, Slope, or Biome
* The resource to overlay on the map
* The planet to display

The icon in the center of the upper row regenerates the map.

The toggle icons along the left side of the map control the various overlays and the color mode.

The four buttons in the bottom-left open and close the other SCANsat windows.

The camera icon in the lower-right exports a copy of the map.

The re-size icon in the lower-right corner can be dragged to re-size the map.

### [:top:][top] 5. Zoom Map
------------------------------------------

> ![][bigmap-zoom-open]

A separate, small map can be opened from the big map by right-clicking somewhere within the big map. This new window will be centered on the mouse cursor's location and zoomed in by a factor of 10. Icons on the zoom map can be used to zoom in or out, to a minimum of 2X zoom.

The zoom scale and map center can be controlled by clicking within the zoom map.
* Left-click to zoom out and re-center the map at the mouse cursor.
* Right-click to zoom in and re-center the map at the mouse cursor.
* Middle-click or Modifier Key (usually Alt on Windows) + Right-click will re-center the map without changing the scale.

The vessel orbit overlay, waypoint icons, and anomaly locations can be toggle on and off independently of the big map settings.

The **zoom map** also features mouse-over information for the cursor location similar to that shown on the big map.

Different map types, resource overlays and polar projections are all applied to the **zoom map** as well.
> ![][zoommap-in]

#### [:top:][top] 5a. Target Selection
> ![][zoommap-scansat-landing]

The **zoom map** features an option to select and display a target site for each planet. Toggle **Target Selection Mode** by clicking on the target icon in the upper left, then select a sight 
in the zoom map window. The icon will be displayed, along with standard, FinePrint waypoints, in the zoom window and the big map. 

While in map view the target site will be overlayed on the planet's surface; shown as a matching, four-arrow green icon.

To clear an existing target, activate **Target Selection Mode** by clicking the target icon, then click somewhere inside of the zoom map window, but outside of the map itself.
> ![][zoommap-clear-target]

#### [:top:][top] 5b. MechJeb Landing Guidance
> ![][zoommap-mechjeb-settings]

If MechJeb is installed and an additional option is available in the settings menu to activate **MechJeb Landing Guidance Mode**

> ![][zoommap-mechjeb-landing]

The **zoom map** can be used in the same way described above to select a landing site for **MechJeb's Landing Guidance** module. The current vessel must have a MechJeb core
and the MechJeb Landing Guidance module must be unlocked in the R&D Center. 

Landing sites selected through MechJeb will automatically show up as a waypoint on SCANsat maps.


### [:top:][top] 6. Instrument Window
------------------------------------------

> ![][instruments-small]

The instruments window provides a readout of several types of data based on current scanning coverage.

* **Location** Shows the vessel's current coordinates; not dependent on scanning coverage
* **Waypoint** Shows if the vessel is inside of a current FinePrint waypoint; not dependent on scanning coverage
* **Altitude** Shows the vessel's current altitude above the terrain; shows the current terrain altitude when landed
* **Slope** Shows a highly localized slope based on a 3X3 grid centered 5m around the vessel
* **Biome** Shows the biome that the current vessel is in or over
* **Anomaly** Shows the nearest anomaly and its distance from the vessel
* **BTDT Anomaly** Shows detailed information and a crude image about the nearest anomaly; scroll the mouse wheel when positioned over the anomaly window to switch between different structures if more than one is found

> ![][instruments-btdt]

### [:top:][top] 7. Parts and Sensor Types
------------------------------------------

| **Part** | **Scan Type** | **FOV** | Altitude (**Min**) | (**Ideal**) | (**Max**) 
|:--- | --- | --- | --- | --- | --- |
| [RADAR Altimetry Sensor][5a] | **RadarLo** / **Slope**| 5 | 5000 m | 5000 m | 500 km
| [SAR Altimetry Sensor][5b] | **RadarHi** | 2 | 5000 m | 750 km | 800 km 
| [Multispectral Sensor][5c] | **Biome** **ANOM** | 4 | 5000 m | 250 km | 500 km 
| [Been There Done That®][5d] | **Anomaly** | 1 | 0 m | 0 m | 2 km
| [MapTraq® (deprecated)][5e] | **None** | N/A | N/A | N/A | N/A 

#### [:top:][top] 7a. The RADAR Altimetry Sensor
> ![RADAR][vab-radar]
#### [:top:][top] 7b. The SAR Altimetry Sensor
> ![SAR][vab-sar]
#### [:top:][top] 7c. The Multispectral Sensor
> ![Multi][vab-multi]
#### [:top:][top] 7d. Been There Done That
> ![BTDT][vab-btdt]
#### [:top:][top] 7e. MapTraq (deprecated)
> ![MapTraq][vab-maptraq] 


### [:top:][top] 8. (Career Mode) Research and Development
------------------------------------------

The **RADAR Altimetry** sensor can be unlocked in **Basic Science**.

The **SAR Altimetry** sensor can be unlocked in **Advanced Science Tech**.

The **Multispectral** sensor can be unlocked in **Advanced Exploration**.

The **BTDT** sensor can be unlocked in **Field Science**.

##### [:top:][top] 8a. Minimum Scan for Science
Once you scan at least 30% of a particular map, you can use **Analyze Data** to get delicious science:

> ![30% is your minimum][science-min]

##### [:top:][top] 8b. Getting Maximum Science
Between 30% and 100%, you will get a number of science points proportional to the percentage. Really,
the upper cutoff is 95% in case you didn't scan the whole map.

> ![Scan 95% to get all science][science-max]

##### [:top:][top] 8c. Contract Support
Career mode contracts are supported through third party addons.

* [Contract Configurator Forum Thread][cconfig:release]
* [SCANsat Contract Pack][ccfgSCANsat:release]

### [:top:][top] 9. Color Management
------------------------------------------

> ![][color-window]

SCANsat provides multiple options for map color configurations and terrain level changes.

The color management window can be accessed from the big or small map with the color palette icon, or from the toolbar menu.

##### [:top:][top] 9a. Terrain Colors and Options
On the left are the various color palettes available; there are four different styles that can be selected from the drop down menu. 
Palettes can be customized by changing the number of colors in the palette, reversing the order, or making the palette use discrete
color transitions, rather than the smooth gradient used by default.

Changes to the color palette are reflected after selecting **Apply**, the big and small maps will automatically refresh using the newly selected color palette.
Note that only the altimetry map is affected by color palette selection.
> ![][color-palette-switch]

There are several terrain height options available as well.
* The **Min** and **Max** height sliders can be used to set the lower and upper cutoff values for the terrain height-to-color algorithm.
* The **Clamp** option can be used to set a cutoff below which only the first two colors in the selected
palette will be used for the terrain height-to-color algorithm. This is especially useful on planets where there is an ocean, as it makes the transition
from ocean to solid terrain more pronounced.

> ![][color-clamp-terrain]

All stock KSP planets have default color palette and terrain height values appropriate for the planet's terrain. Standard default values are used
for any addon planets.

##### [:top:][top] 9b. Biome Colors and Options
Biome map colors and options can be controlled in the **Biome** tab of the window.
* The end-point colors can be selected using the HSV color-picker; the value slider controls the brightness of the color.
* Terrain transparency is controlled with a slider.
* Stock style biome maps can be used in place of SCANsat's custom colors

> ![][color-biome]

Biomes can also be displayed using the stock color maps.

> ![][color-biome-stock]

##### [:top:][top] 9c. Resource Colors and Options
Resource overlays can also be adjusted, using the **Resource** tab.
* Resource colors are selected in the same manner as biome colors.
* Upper and lower resource cutoff values can be adjusted with the sliders; use fine control mode for small adjustments.
* Each resource can be adjusted separately and the values can be applied to the current planet or all planets.
* Most planets share the same resource value settings; it is easiest to set values for all planets then set the values individually where needed (ie water has a higher value on Kerbin than elsewhere).

> ![][color-resource]

### [:top:][top] 10. Background Scanning
------------------------------------------

![Note the background scanning (non-active vessels are scanning)][small-scan]

Unlike some other KSP scanning systems, SCANsat allows scanning with multiple
vessels.  All online scanners scan at the same time during any scene where time progresses; no active SCANsat
parts are necessary. 

### [:top:][top] 11. Time Warp
------------------------------------------

SCANsat does not interpolate satellite paths during time warp; nevertheless, due to the relatively large field of view
of each sensor, it's still possible to acquire data faster by time warping. The maximum recommended time warp speed
is currently **10,000x**. Scanning at this warp factor should allow identical scanning performance 
(in terms of [swath width](http://en.wikipedia.org/wiki/Swath_width)) as scanning at *1x*.

As an example of speed, here is a BigMap rendering of a scan at **100x**:
> ![this is pretty peaceful][bigmap-scan-100x]

And this is a BigMap rendering of the same orbit, but later in the scan. 
It starts at **1000x** and then speeds up to **10,000x**:
> ![this makes my OCD happy][bigmap-scan-10000x]

Notice that the only gaps in coverage are those at the poles (ie, the selected inclination was not high enough to capture the poles).

### [:top:][top] 12. Settings Menu
------------------------------------------

> ![][settings-window]

The settings menu has a various general options
* The marker used for **Anomalies** can be specified
* **Background scanning** can be controlled for each planet
* **Background scanning** resolution can be lowered for better performance (watch for short pauses when several scanners are active at very high timewarp; reducing the scanning resolution can help with this)
* See the **[Resource Settings][2c]** section for information about resource options
* Toggles control the availability of the **Stock App Launcher** button, and the **Tooltips** for various icons on other windows
* If MechJeb is installed an additional option is available to toggle the MechJeb Landing Guidance interface
* If the windows are ever dragged off screen there is an option to **Reset All Windows** to their default positions
* **Scanning Data** can be deleted for any or all planets; a confirmation box will appear when these options are selected
* The numbers under the **Time Warp Resolution** indicate the following
    * **Vessels:** The number of vessels with any active SCANsat sensors present
	* **Sensors:** The total number of SCANsat sensors on all vessels; note that all combination sensors are separated into their invidual components, i.e. the Multi-Spectral scanner consists of two sensors, Biomes and Anomalies.
	* **Passes:** The number of scanning passes recorded per second, this number can easily be in the tens of thousands at high time warp with multiple vessels and sensors active.

### [:top:][top] 13. Note Concerning Data Sources
------------------------------------------

All data this mod shows you is pulled from your game as you play. This
includes:
  * terrain elevation
  * biomes
  * anomalies

SCANsat can't guarantee that all anomalies will be found; in particular, some are so close
to others that they don't show up on their own, and if the [developers][KSP:developers] want to be
sneaky then they can of course be sneaky.

------------------------------------------






[technogeeky]: http://forum.kerbalspaceprogram.com/members/110153
[DMagic]: http://forum.kerbalspaceprogram.com/members/59127
[damny]: http://forum.kerbalspaceprogram.com/members/80692
[Milkshakefiend]: http://forum.kerbalspaceprogram.com/members/72507
[Olympic1]: http://forum.kerbalspaceprogram.com/members/81815
[madsailor]: http://forum.kerbalspaceprogram.com/members/123944
[tinypirate]: http://forum.kerbalspaceprogram.com/members/79868

[KSP:developers]: https://kerbalspaceprogram.com/index.php
[KSP:reddit]: http://www.reddit.com/r/KerbalSpaceProgram

[vab-radar-thumb]: http://i.imgur.com/PrRIcYvs.png 
[vab-sar-thumb]: http://i.imgur.com/4aTTVfWs.png
[vab-multi-thumb]: http://i.imgur.com/byIYXP9s.png
[vab-maptraq-thumb]: http://i.imgur.com/Skrqc8Cs.png
[vab-btdt-thumb]:  http://i.imgur.com/zUmj6USs.png

[vab-radar]: http://i.imgur.com/PrRIcYv.png
[vab-sar]: http://i.imgur.com/4aTTVfW.png
[vab-multi]: http://i.imgur.com/byIYXP9.png
[vab-maptraq]: http://i.imgur.com/Skrqc8C.png
[vab-btdt]:  http://i.imgur.com/zUmj6US.png

[science-min]: http://i.imgur.com/kEj4fz0.gif
[science-max]: http://i.imgur.com/eMtIL5H.gif

[small-scan]: http://i.imgur.com/uVP6Ujs.gif
[small-scan-bw]: http://i.imgur.com/0AbDwKL.gif
[small-scan-color]:  http://i.imgur.com/dlRckBl.gif
[small-static]: http://i.imgur.com/oPN2qIR.gif
[small-nodata]: http://i.imgur.com/0ArIcqj.png

[small-toolow]: https://i.imgur.com/fTDLvw0.gif
[small-toohigh]: https://i.imgur.com/a8YKkXH.gif
[small-justright]: https://i.imgur.com/Oft4xXP.gif
[small-mismatch1]: https://i.imgur.com/fNztoUN.gif
[small-mismatch2]: https://i.imgur.com/aQtTGvV.gif
[small-newMap1]: http://i.imgur.com/mCnphuZ.gif
[small-toolow-v10]: http://i.imgur.com/USsvSSs.gif
[small-toohigh-v10]: http://i.imgur.com/i7rGDIj.gif
[small-justright-v10]: http://i.imgur.com/y7mHvEF.gif

[bigmap-scan-10000x]: http://i.imgur.com/VEPL3oN.gif
[bigmap-scan-100x]: http://i.imgur.com/bcht47p.gif
[bigmap-anim]: http://i.imgur.com/kxyl8xR.gif
[bigmap-anim-v2]: http://i.imgur.com/wUMToq6.gif
[bigmap-zoom-open]: http://i.imgur.com/7egRRTU.gif

[zoommap-in]: http://i.imgur.com/tTCYDfP.gif
[zoommap-scansat-landing]: http://i.imgur.com/ILqRfne.gif
[zoommap-mechjeb-landing]: http://i.imgur.com/nE0BlA8.gif
[zoommap-mechjeb-settings]: http://i.imgur.com/xOQ7ooj.png
[zoommap-clear-target]: http://i.imgur.com/YffxdNs.gif

[resource-iva]: http://i.imgur.com/iRo4kSA.png
[resource-walkthrough]: http://i.imgur.com/KS4FTh0.gif
[resource-scanner]: http://i.imgur.com/mY0fFjr.gif
[resource-bigmap]: http://i.imgur.com/JYKG6f5.gif
[resource-settings]: http://i.imgur.com/sgMklCu.png
[resource-zoom-map-covered]: http://i.imgur.com/7YuYMGW.png
[resource-zoom-map-uncovered]: http://i.imgur.com/cJ9JtdW.png
[resource-instant]: http://i.imgur.com/mfIMBEP.gif

[color-window]: http://i.imgur.com/RQVjq6g.png
[color-palette-switch]: http://i.imgur.com/0XdMGSy.gif
[color-clamp-terrain]: http://i.imgur.com/8dgFLGj.gif
[color-biome]: http://i.imgur.com/NdA1DVY.gif
[color-resource]: http://i.imgur.com/9NR8gvP.gif
[color-biome-stock]: http://i.imgur.com/T14sFzl.png

[instruments-small]: http://i.imgur.com/sZ2MXiK.gif
[instruments-btdt]: http://i.imgur.com/tybbDap.gif

[settings-window]: http://i.imgur.com/MYIfE05.png

[tinypirate-video-screen]: http://img.youtube.com/vi/UY7eBuReSYU/0.jpg
[tinypirate-video]: https://www.youtube.com/watch?v=UY7eBuReSYU

[top]: #table-of-contents
[0]: #top-0-people-facts-and-faqs
[0a]: #top-0a-faqs
[0b]: #top-0b-video-overview
[1]: #top-1-installation-and-interoperability
[1a]: #top-1a-installation
[1b]: #top-1b-gamedata-layout
[1c]: #top-1c-other-add-ons
[2]: #top-2-types-of-scans
[2a]: #top-2a-scansat-scans
[2b]: #top-2b-resource-scans
[2c]: #top-2c-resource-setting
[3]: #top-3-basic-usage
[3a]: #top-3a-faq-finding-a-good-altitude
[3b]: #top-3b-mismatched-scanners
[4]: #top-4-big-map
[4a]: #top-4a-big-map-options
[5]: #top-5-zoom-map
[5a]: #top-5a-target-selection
[5b]: #top-5b-mechJeb-landing-guidance
[6]: #top-6-instrument-window
[7]: #top-7-parts-and-sensor-types
[7a]: #top-7a-the-radar-altimetry-sensor
[7b]: #top-7b-the-sar-altimetry-sensor
[7c]: #top-7c-the-multispectral-sensor
[7d]: #top-7d-been-there-done-that
[7e]: #top-7e-maptraq-deprecated
[8]: #top-8-career-mode-research-and-development
[8a]: #top-8a-minimum-scan-for-science
[8b]: #top-8b-getting-maximum-science
[8c]: #top-8c-contract-support
[9]: #top-9-color-management
[9a]: #top-9a-terrain-colors-and-options
[9b]: #top-9b-biome-colors-and-options
[9c]: #top-9c-resource-colors-and-options
[10]: #top-10-background-scanning
[11]: #top-11-time-warp
[12]: #top-12-settings-menu
[13]: #top-13-note-concerning-data-sources

[shield:license-bsd]: http://img.shields.io/:license-bsd-blue.svg
[shield:license-mit]: http://img.shields.io/:license-mit-a31f34.svg
[shield:license-cc-by-sa]: http://img.shields.io/badge/license-CC%20BY--SA-green.svg
 
[shield:jenkins-dev]: http://img.shields.io/jenkins/s/https/ksp.sarbian.com/jenkins/SCANsat-dev.svg
[shield:jenkins-rel]: http://img.shields.io/jenkins/s/https/ksp.sarbian.com/jenkins/SCANsat-release.svg
[shield:support-ksp]: http://img.shields.io/badge/for%20KSP-v1.0.2-bad455.svg
[shield:support-rpm]: http://img.shields.io/badge/works%20with%20RPM-v0.18.3-a31f34.svg
[shield:support-mm]: http://img.shields.io/badge/works%20with%20MM-v2.6.2-40b7c0.svg
[shield:support-toolbar]: http://img.shields.io/badge/works%20with%20Blizzy's%20Toolbar-1.7.9-7c69c0.svg
[shield:support-alcor]: http://img.shields.io/badge/works%20with%20ALCOR-0.9-299bc7.svg
[shield:support-kspi]: http://img.shields.io/badge/works%20with%20Interstellar-0.13-a62374.svg
[shield:support-usi]:http://img.shields.io/badge/works%20with%20USI-0.30-34c566.svg
[shield:support-karbonite]: http://img.shields.io/badge/works%20with%20Karbonite-0.6-ff8c00.svg
[shield:support-epl]: http://img.shields.io/badge/works%20with%20EPL-4.2.3-ff8c00.svg
[shield:support-ctt]: http://img.shields.io/badge/works%20with%20CTT-1.1-blue.svg
[shield:support-ccfg]: https://img.shields.io/badge/works%20with%20Contract%20Configurator-1.0.2-yellowgreen.svg
[shield:ckan]: https://img.shields.io/badge/CKAN-Indexed-brightgreen.svg
[shield:support-mechjeb]: http://img.shields.io/badge/works%20with%20MechJeb-2.5-lightgrey.svg

[shield:gittip-tg-img]: http://img.shields.io/gittip/technogeeky.png
[shield:gittip-tg]: https://www.gittip.com/technogeeky/
[shield:github-issues]: http://img.shields.io/github/issues/technogeeky/SCANsat.svg

[CKAN:org]: http://ksp-ckan.org/

[SCANsat:organization]: https://github.com/S-C-A-N
[SCANsat:logo]: http://i.imgur.com/GArPFFB.png
[SCANsat:logo-square]: http://i.imgur.com/GArPFFB.png?1
[SCANsat:issues]: https://github.com/S-C-A-N/SCANsat/issues
[SCANsat:pulls]: https://github.com/S-C-A-N/SCANsat/pulls
[SCANsat:imgur-albums]: https://scansat.imgur.com
[SCANsat:best-orbits-table]: https://www.example.com
[SCANsat:email]: mailto:SCANscansat@gmail.com
[SCANsat:gamedata]: http://i.imgur.com/cS1Lu5w.jpg

[SCANsat:dev-readme]: https://github.com/S-C-A-N/SCANsat/tree/dev/#table-of-contents
[SCANsat:rel-readme]: https://github.com/S-C-A-N/SCANsat/#table-of-contents

[SCANsat:rel-thread]: http://forum.kerbalspaceprogram.com/threads/80369
[SCANsat:dev-thread]: http://forum.kerbalspaceprogram.com/threads/96859

[SCANsat:dev-source]: https://github.com/S-C-A-N/SCANsat/tree/dev
[SCANsat:rel-source]: https://github.com/S-C-A-N/SCANsat

[SCANsat:dev-jenkins]: https://ksp.sarbian.com/jenkins/job/SCANsat-dev/
[SCANsat:rel-jenkins]: https://ksp.sarbian.com/jenkins/job/SCANsat-release/
[SCANsat:ket-jenkins]: https://ksp.sarbian.com/jenkins/job/SCANsat-Kethane/
[SCANsat:orsx-jenkins]: https://ksp.sarbian.com/jenkins/job/SCANsat-OpenResourceSystem/

[SCANsat:dev-license]: https://github.com/S-C-A-N/SCANsat/blob/dev/SCANsat/LICENSE.txt
[SCANsat:rel-license]: https://github.com/S-C-A-N/SCANsat/blob/release/SCANsat/LICENSE.txt

[SCANsat:rel-dist-curseforge]: http://kerbal.curseforge.com/ksp-mods/www.example.com-SCANsat
[SCANsat:rel-dist-curseforge-zip]: http://kerbal.curseforge.com/ksp-mods/www.example.com-SCANsat/files/latest

[SCANsat:rel-dist-github]: https://github.com/S-C-A-N/SCANsat/releases
[SCANsat:rel-dist-github-zip]: https://github.com/S-C-A-N/SCANsat/releases/download/www.example.com/SCANsat.zip

[SCANsat:rel-dist-kerbalstuff]: http://beta.kerbalstuff.com/mod/www.example.com/SCANsat
[SCANsat:rel-dist-kerbalstuff-zip]: http://beta.kerbalstuff.com/mod/www.example.com/SCANsat/download/www.example.com

[SCANsat:dev-dist-curseforge]: https://www.example.com
[SCANsat:dev-dist-curseforge-zip]: https://www.example.com

[SCANsat:dev-dist-github]: https://github.com/S-C-A-N/SCANsat/releases/tag/www.example.com
[SCANsat:dev-dist-github-zip]: https://github.com/S-C-A-N/SCANsat/releases/download/www.example.com/SCANsat.zip

[SCANsat:dev-dist-kerbalstuff]: http://beta.kerbalstuff.com/mod/www.example.com/SCANsat
[SCANsat:dev-dist-kerbalstuff-zip]: http://beta.kerbalstuff.com/mod/www.example.com/SCANsat/download/www.example.com

[IAT]: http://forum.kerbalspaceprogram.com/threads/75854
[IAT:kerbin-system]: http://forum.kerbalspaceprogram.com/threads/75854?p=#1
[IAT:inner-systems]: http://forum.kerbalspaceprogram.com/threads/75854?p=#2
[IAT:duna-dres]: http://forum.kerbalspaceprogram.com/threads/75854?p=#3
[IAT:jool-system]: http://forum.kerbalspaceprogram.com/threads/75854?p=#4
[IAT:eeloo]: http://forum.kerbalspaceprogram.com/threads/75854?p=#7
[IAT:earth-system]: http://forum.kerbalspaceprogram.com/threads/75854?p=#9

[karbonite:release]: http://forum.kerbalspaceprogram.com/threads/89401
[karbonite:logo]: http://i.imgur.com/PkewuRD.png

[kethane:release]: http://forum.kerbalspaceprogram.com/threads/23979
[kethane:logo]: http://i.imgur.com/u952LjP.png?1

[usi:release]: http://forum.kerbalspaceprogram.com/threads/79588
[usi:logo]: http://i.imgur.com/aimhLzU.png

[alcor:release]: http://forum.kerbalspaceprogram.com/threads/54925
[alcor:logo]: http://i.imgur.com/7eJ3IFC.jpg

[ctt:logo]: http://i.imgur.com/li2tNgE.png

[mm:release]: http://forum.kerbalspaceprogram.com/threads/55219

[epl:release]: http://forum.kerbalspaceprogram.com/threads/59545

[ctt:release]: http://forum.kerbalspaceprogram.com/threads/100385

[kspi:release]: http://forum.kerbalspaceprogram.com/threads/43839

[toolbar:release]: http://forum.kerbalspaceprogram.com/threads/60863

[rpm:release]: http://forum.kerbalspaceprogram.com/threads/57603

[cconfig:release]: http://forum.kerbalspaceprogram.com/threads/101604

[ccfgSCANsat:release]: http://forum.kerbalspaceprogram.com/threads/108097

[mechjeb:release]: http://forum.kerbalspaceprogram.com/threads/12384
