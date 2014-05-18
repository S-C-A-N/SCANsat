![May Your Tentacles Orbit in Peace][logo]

High Performance Scanning Sensors
------------------------------------------
#### **SCANsat**: Real Scanning, Real Science, at Warp Speed!
![scan your planetoid like the big boys do][bigmap-scan-10000x]
###### **Example SAR scan of Kerbin at 1000x and then 10,000x warp**


**Table of Contents**:
* [0. Maintainers, Authors, Contributors, and Licenses][0]
* [1. Installation][1]
* [2. Types of Scans][2]
* [3. Basic Usage][3]
  * [a. FAQ: Finding a Good Altitude][3a]
  * [b. Mismatched Scanners][3b]
* [4. Big Map][4]
* [5. Parts and Sensors Types][5]
  * [a. RADAR][5a]
  * [b. SAR][5b]
  * [c. Multi][5c]
  * [d. BTDT][5d]
  * [e. MapTraq][5e]
* [6. (Career Mode) Research and Development][6]
  * [a. Minimum Scan for Science (30%)][6a]
  * [b. Getting Maximum Science][6b]
* [7. Background Scanning][7]
* [8. Time Warp][8]
* [9. Note: Data Sources][9]

**WARNING**:

This add-on is a **work**-in-**progress**.

This means you should expect that it *may not work*, and you should be unsurprised if it *does not progress*.

Disclaimer aside, this add-on is widely used and it usually works just fine.

### 0. Maintainers, Authors, Contributors, and Licenses
------------------------------------------
#### Maintainers
The current maintainer is:
  + [technogeeky][technogeeky] \<<technogeeky@gmail.com>\>

Maintainers are the people who you should complain to if there is something wrong.

Complaints in the form of [GitHub Issues][SCANsat:issues] are given *higher* priority than other complaints;
complaints in the form of [GitHub Pull Requests][SCANsat:pulls] are given the **highest** priority possible.

#### Authors
The current authors include:
  + [technogeeky][technogeeky] \<<technogeeky@gmail.com>\>
  + [DMagic][DMagic] \<<david.grandy@gmail.com>\>

Past authors include:
  + [damny][damny] \<<missing-in-action@nowhere-to-be-found.com>\>

As of May 2014, the vast majority of code is damny's, and technogeeky and DMagic are very slowly encroaching.


#### Contributors

In addition to the authors, the following people have contributed:
  + (Models, Graphics, Textures) [Milkshakefiend][Milkshakefiend]

#### Licenses

For licensing information, please see the [included LICENSE.txt][SCANsat:licenses] file.

[Source Code][SCANsat:source] is available, as some licenses may require.


### 1. Installation
------------------------------------------
  1. Put the SCANsat folder in your KSP installation's GameData folder.
  2. (Optional) Place the SCANsatRPM folder in your KSP installation's GameData folder.

### 2. Types of Scans
------------------------------------------
SCANsat supports several different kinds of scans (as opposed to
scanning modules or parts).

As of May 2014, these include:
  * **RadarLo**: Basic, Low-Resolution RADAR Altimetry (b&w, limited zoom)
  * **RadarHi**: Advanced, High-Resolution RADAR Altimetry (in color, unlimited zoom)
  * **Slope**: Slope Data converted from RADAR data
  * **Biome**: Biome Detection and Classification (in color, unlimited zoom)
  * **Anomaly**: Anomaly Detection and Labeling

Other parts and add-ons are free to include one or more of these kinds of scans. In general,
we would request that similar (same order of magitude) scanning paramters and limitations are used
on custom parts, but this is not a requirement.

### 3. Basic Usage
------------------------------------------

Put scanner part on rocket, aim rocket at sky, launch. If your rocket is not pointing at the sky, you are probably not going to map today, because most sensors only work above 5 km.

You can start scanning by selecting a SCANsat part's context menu, enabling the part. Here, you will find a **small map**.

#### 3a. FAQ: Finding a Good Altitude

Watch the data indicators on the small map to determine how well your scanners are performing.


###### too high
Solid ORANGE means you're too high (and therefore no data is being recorded):
![][small-toohigh]

###### too low
Flashing ORANGE/GREEN means you're too low (and therefore you have a FOV penalty):
![][small-toolow]

###### just right
Solid GREEN means you're in an ideal orbit. Notice the larger swath width on the right:
![][small-justright]

#### 3b. Mismatched Scanners

In these examples, the SAR and Multi sensors are not very well matched. Because the SAR sensors is ideal above 750km, and becuase it has a large field of view penalty if it's down near the ideal for Multi (250km), these sensors probably should not be used on the same scanner.

![][small-mismatch1]

![][small-mismatch2]


###### SAR scanner with a thin swath width due to low altitude
![][small-scan-color]

###### RADAR scanner with an ideal swath width
![][small-scan-bw]

The mapping interface consists of a small-ish map of the planet, as far as it has been scanned in your current game. It scans and updates quickly and shows positions of the active vessel, as well as other scanning vessels in orbit around the same planet. Orbital information is also provided. For a slower but more detailed view, see the **[big map][4]**.

Be sure to remember to pack enough batteries, radioisotope generators, and solar panels. If you forget, you'll run out of electricity, you'll stop recording data, and you'll see useless static:

###### Static! Oh no, adjust the rabbit ears!
![][small-static]

### 4. Big Map
------------------------------------------
![A Big Big Map][bigmap-anim]

A bigger map can be rendered on demand. Rendered maps are automatically
saved to GameData/SCANsat/PluginData. Note that position indicators for
vessels or anomalies are not visible on exported images (but they may be a future release).

You can mouse over the big map to see what sensors have data for the location, as well as terrain elevation, and other details.

Right-clicking on the big map shows a magnified view around the position where you clicked. Mouse operations work inside this magnified view just like they work outside, meaning the data displayed at the bottom window applies to your position inside the magnified view, and right-clicking inside it will increase magnification. This can be useful to find landing spots which won't kill your kerbals.

### 5. Parts and Sensor Types
------------------------------------------

| **Part** | **Scan Type** | **FOV** | Altitude (**Min**) | (**Ideal**) | (**Max**) 
|:--- | --- | --- | --- | --- | --- |
| [RADAR Altimetry Sensor][5a] | **RadarLo** / **Slope**| 5 | 5000 m | 5000 m | 500 km
| [SAR Altimetry Sensor][5b] | **RadarHi** | 2 | 5000 m | 750 km | 800 km 
| [Multispectral Sensor][5c] | **Biome** **ANOM** | 4 | 5000 m | 250 km | 500 km 
| [Been There Done That®][5d] | **Anomaly** | 1 | 0 m | 0 m | 2 km
| [MapTraq®][5e] | **None** | N/A | N/A | N/A | N/A 

#### a. The RADAR Altimetry Sensor
![RADAR][vab-radar]
#### b. The SAR Altimetry Sensor
![SAR][vab-sar]
#### c. The Multispectral Sensor
![Multi][vab-multi]
#### d. Been There Done That
![BTDT][vab-btdt]
#### e. MapTraq
![MapTraq][vab-maptraq]
 


### 6. (Career Mode) Research and Development
------------------------------------------

The **RADAR Altimetry** sensor can be unlocked in **Science Tech**.

The **SAR Altimetry** sensor can be unlocked in **Experimental Science**.

The **Multispectral** sensor can be unlocked in **Advanced Exploration**.

The **BTDT** sensor can be unlocked in **Field Science**.


##### 6a.Minimum Scan for Science
Once you scan at least 30% of a particular map, you can use **Analyze Data** to get delicious science:

![30% is your minimum][science-min]

##### 6b. Getting Maximum Science
Between 30% and 100%, you will get a number of science points proportional to the percentage. Really,
the upper cutoff is 95% in case you didn't scan the whole map.

![Scan 95% to get all science][science-max]

### 7. Background Scanning
------------------------------------------

![Note the background scanning (non-active vessels are scanning)][small-scan]

Unlike some other KSP scanning systems, SCANsat allows scanning with multiple
vessels.  All online scanners scan at the same time, but only when your *active vessel* has
**at least one** of the parts included in this mod equipped and the mapping interface is open. 

### 8. Time Warp
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

### 9. Note Concerning Data Sources
------------------------------------------
All data this mod shows you is pulled from your game as you play. This
includes:
  * terrain elevation
  * biomes
  * anomiles

SCANsat can't guarantee that all anomalies will be found; in particular, some are so close
to others that they don't show up on their own, and if the [developers][KSP:developers] want to be
sneaky then they can of course be sneaky.

------------------------------------------



[technogeeky]: http://forum.kerbalspaceprogram.com/members/110153-technogeeky
[DMagic]: http://forum.kerbalspaceprogram.com/members/59127-DMagic
[damny]: http://forum.kerbalspaceprogram.com/members/80692-damny
[Milkshakefiend]: http://forum.kerbalspaceprogram.com/members/72507-Milkshakefiend

[SCANsat:issues]: https://github.com/S-C-A-N/SCANsat/issues
[SCANsat:pulls]: https://github.com/S-C-A-N/SCANsat/pulls
[SCANsat:source]: https://github.com/S-C-A-N/SCANsat
[SCANsat:licenses]: https://github.com/S-C-A-N/SCANsat/blob/master/LICENSE.txt

[KSP:developers]: https://kerbalspaceprogram.com/index.php

[0]: https://github.com/S-C-A-N/SCANsat#0-maintainers-authors-contributors-and-licenses
[1]: https://github.com/S-C-A-N/SCANsat#1-installation
[2]: https://github.com/S-C-A-N/SCANsat#2-types-of-scans
[3]: https://github.com/S-C-A-N/SCANsat#3-basic-usage
[3a]: https://github.com/S-C-A-N/SCANsat#3a-faq-finding-a-good-altitude
[3b]: https://github.com/S-C-A-N/SCANsat#3b-mismatched-scanners
[4]: https://github.com/S-C-A-N/SCANsat#4-big-map
[5]: https://github.com/S-C-A-N/SCANsat#5-parts-and-sensor-types
[5a]: https://github.com/S-C-A-N/SCANsat#a-the-radar-altimetry-sensor
[5b]: https://github.com/S-C-A-N/SCANsat#b-the-sar-altimetry-sensor
[5c]: https://github.com/S-C-A-N/SCANsat#c-the-multispectral-sensor
[5d]: https://github.com/S-C-A-N/SCANsat#d-been-there-done-that
[5e]: https://github.com/S-C-A-N/SCANsat#e-maptraq
[6]: https://github.com/S-C-A-N/SCANsat#6-career-mode-research-and-development
[6a]: https://github.com/S-C-A-N/SCANsat#6aminimum-scan-for-science
[6b]: https://github.com/S-C-A-N/SCANsat#6b-getting-maximum-science
[7]: https://github.com/S-C-A-N/SCANsat#7-background-scanning
[8]: https://github.com/S-C-A-N/SCANsat#8-time-warp
[9]: https://github.com/S-C-A-N/SCANsat#9-note-concerning-data-sources


[logo]: http://i.imgur.com/GArPFFB.png

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

[bigmap-scan-10000x]: http://i.imgur.com/VEPL3oN.gif
[bigmap-scan-100x]: http://i.imgur.com/bcht47p.gif
[bigmap-anim]: http://i.imgur.com/kxyl8xR.gif
