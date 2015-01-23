### [**SCANsat**][top]: Real Scanning, Real Science, Warp Speed!
[![][shield:support-ksp]][KSP:developers]&nbsp;
[![][shield:license-bsd]][SCANsat:rel-license]&nbsp;
[![][shield:license-mit]][SCANsat:dev-license]&nbsp;
[![][shield:license-cc-by-sa]][SCANsat:dev-license]&nbsp;
![scan your planetoid like the big boys do][bigmap-scan-10000x]
> ###### **Example SAR scan of Kerbin at 1000x and then 10,000x warp**

[![][shield:support-rpm]][RPM:release]&nbsp;
[![][shield:support-ket]][Kethane:release]&nbsp;
[![][shield:support-reg]][reg:release]&nbsp;
[![][shield:support-mm]][mm:release]&nbsp;
[![][shield:support-toolbar]][toolbar:release]&nbsp;
[![][shield:support-karbonite]][karbonite:release]&nbsp;
[![][shield:support-usi]][usi:release]&nbsp;
[![][shield:support-epl]][epl:release]&nbsp;
[![][shield:support-ctt]][ctt:release]&nbsp;
[![][shield:support-tm]][techmanager:release]&nbsp;
[![][shield:support-ccfg]][cconfig:release]&nbsp;

**Table of Contents**
------------------------------------------

* [0. People, Facts, and FAQs][0]
  * [a. FAQs][0a]
* [1. Installation and Interoperability][1]
  * [a. Installation][1a]
  * [b. GameData Layout][1b]
  * [c. Other Add-Ons][1c]
* [2. Types of Scans][2]
  * [a. Native SCANsat][2a]
  * [b. Resource Scans][2b]
  * [c. Regolith][2c]
  * [d. Kethane][2d] [![][shield:support-ket]][kethane:release][![][shield:jenkins-ket]][SCANsat:ket-jenkins]
* [3. Basic Usage][3]
  * [a. FAQ: Finding a Good Altitude][3a]
  * [b. Mismatched Scanners][3b]
* [4. Big Map][4]
  * [a. Big Map Options][4a]
* [5. Parts and Sensors Types][5]
  * [a. RADAR][5a]
  * [b. SAR][5b]
  * [c. Multi][5c]
  * [d. BTDT][5d]
  * [e. MapTraq (deprecated)][5e]
* [6. (Career Mode) Research and Development][6]
  * [a. Community Tech Tree Support][6a]
  * [b. Minimum Scan for Science (30%)][6b]
  * [c. Getting Maximum Science][6c]
  * [d. Contracts][6d]
* [7. Color Management][7]
* [8. Instrument Window][8]
* [9. Background Scanning][9]
* [10. Time Warp][10]
* [11. Settings Menu][11]
* [12. Note: Data Sources][12]

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
  
  + (Science results text) [Olympic1][Olympic1]

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
    * **Yes!** [**This link**][6a] tells you which nodes unlock which parts in the tech tree.
  * [Contracts] Does SCANsat offer contracts to complete?
    * **Yes/No.** Contracts are currently only supported through [**third-party addons**][6d].
  * Can you add <some feature or change> to SCANsat?
    * **Probably!** First, check the issues page to see if it's already been requested. If not, add a new issue. Even better, attempt to add the feature yourself and submit a pull request. We'll catch the bugs for you!



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
[![Support for Kethane][kethane:logo]][kethane:release] | [![Support for MKS][usi:logo]][usi:release]
[**Regolith**][reg:release] | [![Support for ALCOR][alcor:logo]][alcor:release]
[**RasterPropMonitor**][rpm:release]  | [![Support for Karbonite][karbonite:logo]][karbonite:release]
[**Blizzy78's Toolbar**][toolbar:release] | [![Support for Community Tech Tree][ctt:logo]][ctt:release]
[**ModuleManager**][mm:release] | [**Extraplanetary Launchpads**][epl:release]



* **SCANsat**
  * [x] [**v8.0**][SCANsat:rel-thread] SCANsat Release **version: v8.0**
  * [x] [**v9.0**][SCANsat:dev-thread] SCANsat Dev **version: v9.0**

 **MM**, **RPM**, **Toolbar**, and **Resource Addons** are all **soft** dependencies. This means your experience with SCANsat will be enhanced if you are using these mods, but they are not necessary.

**SCANsat** is built against the following mods:
  * [x] [![][shield:support-mm]][mm:release]
  * [x] [![][shield:support-rpm]][rpm:release]
  * [x] [![][shield:support-alcor]][alcor:release]
  * [x] [![][shield:support-toolbar]][toolbar:release] 

**SCANsat** also supports resource scanning with the following mods:
  * [x] [![][shield:support-reg]][reg:release]
  * [x] via (Regolith) <- [![][shield:support-usi]][usi:release]
  * [x] via (Regolith) <- [![][shield:support-karbonite]][karbonite:release]
  * [x] [![][shield:support-ket]][kethane:release]
  * [x] via (Kethane) <- [![][shield:support-epl]][epl:release]


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

![][small-newMap1]

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

**SCANsat** will scan planetoids for resources, assuming you have the relevant mods installed. All support for resource scanning is handled through one of two plugins. **Regolith** supports many resources, and **Kethane** supports a few. 

Resource scans are initiated in the same way as any other scan. In this case they must use custom scanner parts included with the resource addon.
> ![][resource-scanner]

Resource scanning proceeds the same way as standard SCANsat scanning instruments do. The grey scale color option generally works best when viewing resource overlays.
> ![][resource-bigmap]

Each of the two resource systems can be enabled through the **SCANsat** Big Map:
> ![][resource-walkthrough-v2]

##### [:top:][top] 2c. Regolith
[![][shield:jenkins-orsx]][SCANsat:orsx-jenkins]

**Regolith** support is built internal to SCANsat. If you have a **Regolith** DLL loaded anywhere SCANsat will only target the newest version.

With any **Regolith**-using mod installed, you can select their resources in the drop down menu from the Big Map or KSC Map, and enable their overlay with the resource icon.
For instance the **Karbonite** mod's resources can be viewed:
> ![][resource-orsx-karbonite-v2]

##### [:top:][top] 2d. Kethane
[![][shield:jenkins-ket]][SCANsat:ket-jenkins]

**Kethane** support is built using an included extra DLL file. 

This file will only be loaded if you have a Kethane installed in its usual location. If Kethane is not installed, this DLL will simply unloaded from memory. **Kethane** also checks to see if you have multiple versions of it installed, and warns you.

Once it is installed correctly, you will be able to enable Kethane resources in the settings menu:
> ![][resource-kethane]

Both of the two resource systems will work in IVA, too:
> ![][resource-iva]

### [:top:][top] 3. Basic Usage
------------------------------------------

Put scanner part on rocket, aim rocket at sky, launch. If your rocket is not pointing at the sky, you are probably not going to map today, because most sensors only work above 5 km.

You can start scanning by selecting a SCANsat part's context menu, enabling the part. Here, you will find a **small map**.

#### [:top:][top] 3a. FAQ: Finding a Good Altitude

Watch the data indicators on the small map to determine how well your scanners are performing. The right-click context menus also contain indicators for the proper scanning altitude.


###### too high
Solid ORANGE means you're too high (and therefore no data is being recorded):
![][small-toohigh-v10]

###### too low
Flashing ORANGE/GREEN means you're too low (and therefore you have a FOV penalty):
![][small-toolow-v10]

###### just right
Solid GREEN means you're in an ideal orbit. Notice the larger swath width on the right:
![][small-justright-v10]

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
![][small-static]

### [:top:][top] 4. Big Map
------------------------------------------

![A Big Big Map][bigmap-anim-v2]

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

### [:top:][top] 5. Parts and Sensor Types
------------------------------------------

| **Part** | **Scan Type** | **FOV** | Altitude (**Min**) | (**Ideal**) | (**Max**) 
|:--- | --- | --- | --- | --- | --- |
| [RADAR Altimetry Sensor][5a] | **RadarLo** / **Slope**| 5 | 5000 m | 5000 m | 500 km
| [SAR Altimetry Sensor][5b] | **RadarHi** | 2 | 5000 m | 750 km | 800 km 
| [Multispectral Sensor][5c] | **Biome** **ANOM** | 4 | 5000 m | 250 km | 500 km 
| [Been There Done That®][5d] | **Anomaly** | 1 | 0 m | 0 m | 2 km
| [MapTraq® (deprecated)][5e] | **None** | N/A | N/A | N/A | N/A 

#### [:top:][top] 5a. The RADAR Altimetry Sensor
![RADAR][vab-radar]
#### [:top:][top] 5b. The SAR Altimetry Sensor
![SAR][vab-sar]
#### [:top:][top] 5c. The Multispectral Sensor
![Multi][vab-multi]
#### [:top:][top] 5d. Been There Done That
![BTDT][vab-btdt]
#### [:top:][top] 5e. MapTraq (deprecated)
![MapTraq][vab-maptraq]
 


### [:top:][top] 6. (Career Mode) Research and Development
------------------------------------------

The **RADAR Altimetry** sensor can be unlocked in **Science Tech**.

The **SAR Altimetry** sensor can be unlocked in **Experimental Science**.

The **Multispectral** sensor can be unlocked in **Advanced Exploration**.

The **BTDT** sensor can be unlocked in **Field Science**.

##### [:top:][top] 6a. Community Tech Tree Support
When the [Community Tech Tree][ctt:release] and [TechManager][techmanager:release] addons are installed SCANsat parts will default to different tech tree nodes.

The **RADAR Altimetry** sensor can be unlocked in **Orbital Surveys**.

The **SAR Altimetry** sensor can be unlocked in **Specialized Science Tech**.

The **Multispectral** sensor can be unlocked in **Advanced Surveys**.

The **BTDT** sensor can be unlocked in **Field Science**.

##### [:top:][top] 6b. Minimum Scan for Science
Once you scan at least 30% of a particular map, you can use **Analyze Data** to get delicious science:

![30% is your minimum][science-min]

##### [:top:][top] 6c. Getting Maximum Science
Between 30% and 100%, you will get a number of science points proportional to the percentage. Really,
the upper cutoff is 95% in case you didn't scan the whole map.

![Scan 95% to get all science][science-max]

##### [:top:][top] 6d. Contract Support
Career mode contracts are supported through third party addons.

* [Contract Configurator Forum Thread][cconfig:release]
* [SCANsat Contract Pack][ccfgSCANsat:release]

### [:top:][top] 7. Color Management
------------------------------------------

![][color-window]

SCANsat provides multiple options for map color configurations and terrain level changes.

The color management window can be accessed from the big or small map with the color palette icon, or from the toolbar menu.

On the left are the various color palettes available; there are four different styles that can be selected from the drop down menu. 
Palettes can be customized by changing the number of colors in the palette, reversing the order, or making the palette use discrete
color transitions, rather than the smooth gradient used by default.

Changes to the color palette are reflected after selecting **Apply**, the big and small maps will automatically refresh using the newly selected color palette.
Note that only the altimetry map is affected by color palette selection.
![][color-palette-switch]

There are several terrain height options available as well.
* The **Min** and **Max** height sliders can be used to set the lower and upper cutoff values for the terrain height-to-color algorithm.
* The **Clamp** option can be used to set a cutoff below which only the first two colors in the selected
palette will be used for the terrain height-to-color algorithm. This is especially useful on planets where there is an ocean, as it makes the transition
from ocean to solid terrain more pronounced.
![][color-clamp-terrain]

All stock KSP planets have default color palette and terrain height values appropriate for the planet's terrain. Standard default values are used
for any addon planets.

### [:top:][top] 8. Instrument Window
------------------------------------------

![][instruments-small]

The instruments window provides a readout of several types of data based on current scanning coverage.

* **Biome** Shows the biome that the current vessel is in or over
* **Altitude** Shows the vessel's current altitude above the terrain
* **Slope** Shows a highly localized slope based on a 3X3 grid centered 5m around the vessel
* **Anomaly** Shows the nearest anomaly and its distance from the vessel
* **BTDT Anomaly** Shows detailed information and a crude image about the nearest anomaly; scroll the mouse wheel when positioned over the anomaly window to switch between different structures if more than one is found

![][instruments-btdt]

### [:top:][top] 9. Background Scanning
------------------------------------------

![Note the background scanning (non-active vessels are scanning)][small-scan]

Unlike some other KSP scanning systems, SCANsat allows scanning with multiple
vessels.  All online scanners scan at the same time during any scene where time progresses; no active SCANsat
parts are necessary. 

### [:top:][top] 10. Time Warp
------------------------------------------

SCANsat does not interpolate satellite paths during time warp; nevertheless, due to the relatively large field of view
of each sensor, it's still possible to acquire data faster by time warping. The maximum recommended time warp speed
is currently **10,000x**. Scanning at this warp factor should allow identical scanning performance 
(in terms of [swath width](http://en.wikipedia.org/wiki/Swath_width)) as scanning at *1x*.

As an example of speed, here is a BigMap rendering of a scan at **100x**:
![this is pretty peaceful][bigmap-scan-100x]

And this is a BigMap rendering of the same orbit, but later in the scan. 
It starts at **1000x** and then speeds up to **10,000x**:
![this makes my OCD happy][bigmap-scan-10000x]

Notice that the only gaps in coverage are those at the poles (ie, the selected inclination was not high enough to capture the poles).

### [:top:][top] 11. Settings Menu
------------------------------------------

![][settings-window]

The settings menu has a various general options
* The marker used for anomalies can be specified
* Background scanning can be controlled for each planet
* Background scanning resolution can be lowered for better performance (watch for short pauses when several scanners are active at very high timewarp; reducing the scanning resolution can help with this)
* Toggles control the availability of the stock app launcher button and the tooltips for various icons on other windows
* If the windows are ever dragged off screen there is an option to reset all windows to their default positions
* Data can be deleted for any or all planets; a confirmation box will appear when these options are selected

### [:top:][top] 12. Note Concerning Data Sources
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






[technogeeky]: http://forum.kerbalspaceprogram.com/members/110153-technogeeky
[DMagic]: http://forum.kerbalspaceprogram.com/members/59127-DMagic
[damny]: http://forum.kerbalspaceprogram.com/members/80692-damny
[Milkshakefiend]: http://forum.kerbalspaceprogram.com/members/72507-Milkshakefiend
[Olympic1]: http://forum.kerbalspaceprogram.com/members/81815

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
[bigmap-anim-v2]: http://i.imgur.com/lwyVBAN.gif

[resource-kethane]: http://i.imgur.com/naJIsvB.gif
[resource-kethane2]: http://i.imgur.com/AT2b4G7.jpg?1
[resource-orsx]: http://i.imgur.com/wzhhPRS.png?2
[resource-orsx-karbonite]: http://i.imgur.com/Sge2OGH.png?1
[resource-iva]: http://i.imgur.com/iRo4kSA.png
[resource-walkthrough]: http://i.imgur.com/HJLK1yi.gif
[resource-walkthrough-v2]: http://i.imgur.com/gU9E8PM.gif
[resource-orsx-v2]: http://i.imgur.com/ERSFwCX.png
[resource-orsx-karbonite-v2]: http://i.imgur.com/qVlHzSN.png
[resource-scanner]: http://i.imgur.com/7Q7a7aD.gif
[resource-bigmap]: http://i.imgur.com/aFu2U3F.gif

[color-window]: http://i.imgur.com/XM2ynyZ.png
[color-palette-switch]: http://i.imgur.com/0XdMGSy.gif
[color-clamp-terrain]: http://i.imgur.com/8dgFLGj.gif

[instruments-small]: http://i.imgur.com/DwkwCI5.gif
[instruments-btdt]: http://i.imgur.com/tybbDap.gif

[settings-window]: http://i.imgur.com/ogQbeso.png

[top]: #table-of-contents
[0]: #top-0-people-facts-and-faqs
[0a]: #top-0a-faqs
[1]: #top-1-installation-and-interoperability
[1a]: #top-1a-installation
[1b]: #top-1b-gamedata-layout
[1c]: #top-1c-other-add-ons
[2]: #top-2-types-of-scans
[2a]: #top-2a-scansat-scans
[2b]: #top-2b-resource-scans
[2c]: #top-2c-regolith
[2d]: #top-2d-kethane
[3]: #top-3-basic-usage
[3a]: #top-3a-faq-finding-a-good-altitude
[3b]: #top-3b-mismatched-scanners
[4]: #top-4-big-map
[4a]: #top-4a-big-map-options
[5]: #top-5-parts-and-sensor-types
[5a]: #top-5a-the-radar-altimetry-sensor
[5b]: #top-5b-the-sar-altimetry-sensor
[5c]: #top-5c-the-multispectral-sensor
[5d]: #top-5d-been-there-done-that
[5e]: #top-5e-maptraq-deprecated
[6]: #top-6-career-mode-research-and-development
[6a]: #top-6a-community-tech-tree-support
[6b]: #top-6b-minimum-scan-for-science
[6c]: #top-6c-getting-maximum-science
[6d]: #top-6d-contract-support
[7]: #top-7-color-management
[8]: #top-8-instrument-window
[9]: #top-9-background-scanning
[10]: #top-10-time-warp
[11]: #top-11-settings-menu
[12]: #top-12-note-concerning-data-sources

[shield:license-bsd]: http://img.shields.io/:license-bsd-blue.svg
[shield:license-mit]: http://img.shields.io/:license-mit-a31f34.svg
[shield:license-cc-by-sa]: http://img.shields.io/badge/license-CC%20BY--SA-green.svg
 
[shield:jenkins-dev]: http://img.shields.io/jenkins/s/https/ksp.sarbian.com/jenkins/SCANsat-dev.svg
[shield:jenkins-rel]: http://img.shields.io/jenkins/s/https/ksp.sarbian.com/jenkins/SCANsat-release.svg
[shield:jenkins-ket]: http://img.shields.io/jenkins/s/https/ksp.sarbian.com/jenkins/SCANsat-kethane.svg
[shield:jenkins-orsx]: http://img.shields.io/jenkins/s/https/ksp.sarbian.com/jenkins/SCANsat-openresourcesystem.svg
[shield:support-ksp]: http://img.shields.io/badge/for%20KSP-v0.90-bad455.svg
[shield:support-rpm]: http://img.shields.io/badge/works%20with%20RPM-v0.18.3-a31f34.svg
[shield:support-ket]: http://img.shields.io/badge/works%20with%20Kethane-v0.9.2-brightgreen.svg
[shield:support-orsx]: http://img.shields.io/badge/works%20with%20ORSX-v0.1.2-000000.svg
[shield:support-mm]: http://img.shields.io/badge/works%20with%20MM-v2.5.9-40b7c0.svg
[shield:support-toolbar]: http://img.shields.io/badge/works%20with%20Blizzy's%20Toolbar-1.7.8-7c69c0.svg
[shield:support-alcor]: http://img.shields.io/badge/works%20with%20ALCOR-0.9-299bc7.svg
[shield:support-kspi]: http://img.shields.io/badge/works%20with%20Interstellar-0.13-a62374.svg
[shield:support-usi]:http://img.shields.io/badge/works%20with%20USI-0.22.3-34c566.svg
[shield:support-karbonite]: http://img.shields.io/badge/works%20with%20Karbonite-0.5.1-ff8c00.svg
[shield:support-epl]: http://img.shields.io/badge/works%20with%20EPL-4.2.3-ff8c00.svg
[shield:support-ctt]: http://img.shields.io/badge/works%20with%20CTT-1.1-blue.svg
[shield:support-tm]: http://img.shields.io/badge/works%20with%20TechManager-1.5-lightgrey.svg
[shield:support-reg]: https://img.shields.io/badge/works%20with%20Regolith-1.1-000000.svg
[shield:support-ccfg]: https://img.shields.io/badge/works%20with%20Contract%20Configurator-6.0-yellowgreen.svg

[shield:gittip-tg-img]: http://img.shields.io/gittip/technogeeky.png
[shield:gittip-tg]: https://www.gittip.com/technogeeky/
[shield:github-issues]: http://img.shields.io/github/issues/technogeeky/SCANsat.svg

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

[techmanager:release]: http://forum.kerbalspaceprogram.com/threads/98293

[kspi:release]: http://forum.kerbalspaceprogram.com/threads/43839

[orsx:release]: http://forum.kerbalspaceprogram.com/threads/91998

[toolbar:release]: http://forum.kerbalspaceprogram.com/threads/60863

[rpm:release]: http://forum.kerbalspaceprogram.com/threads/57603

[reg:release]: http://forum.kerbalspaceprogram.com/threads/100162

[cconfig:release]: http://forum.kerbalspaceprogram.com/threads/101604

[ccfgSCANsat:release]: http://forum.kerbalspaceprogram.com/threads/108097
