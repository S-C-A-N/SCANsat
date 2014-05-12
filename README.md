The Scientific Committee on Advanced Navigation is proud to bring you:

S.C.A.N. High Performance Scanning Sensors
------------------------------------------

**Table of Contents**:

* [0. Authors, Maintainers, Contributors, and Licenses][0]
* [1. Installation][1]
* [2. Types of Scans][2]
* [3. Basic Usage][3]
* [4. Big Map][4]
* [5. Parts and Sensors Types][5]
* [6. (Career Mode) Research and Development][6]
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

The mapping interface consists of a small-ish map of the planet, as far as it has been scanned in your current game. It scans and updates quickly and shows positions of the active vessel, as well as other scanning vessels in orbit around the same planet. Orbital information is also provided. For a slower but more detailed view, see the **[big map][4]**.

### 4. Big Map
------------------------------------------
A bigger map can be rendered on demand. Rendered maps are automatically
saved to GameData/SCANsat/PluginData. Note that position indicators for
vessels or anomalies are not visible on exported images (but they may be a future release).

You can mouse over the big map to see what sensors have data for the location, as well as terrain elevation, and other details.

Right-clicking on the big map shows a magnified view around the position where you clicked. Mouse operations work inside this magnified view just like they work outside, meaning the data displayed at the bottom window applies to your position inside the magnified view, and right-clicking inside it will increase magnification. This can be useful to find landing spots which won't kill your kerbals.

### 5. Parts and Sensor Types
------------------------------------------

| **Part** | **Scan Type** | **FOV** | Altitude (**Min**) | (**Ideal**) | (**Max**) |
| --- | --- | --- | --- | --- | --- |
| RADAR Altimetry Sensor | **RadarLo** / **Slope**| 5 | 5000 m | 5000 m | 500 km |
| SAR Altimetry Sensor | **RadarHi** | 2 | 5000 m | 750 km | 800 km |
| Multispectral Sensor | **Biome** | 4 | 5000 m | 250 km | 500 km |
| Been There Done That® | **Anomaly** | 1 | 0 m | 0 m | 2 km |



### 6. (Career Mode) Research and Development
------------------------------------------

The **RADAR Altimetry** sensor can be unlocked in **Science Tech**.

The **SAR Altimetry** sensor can be unlocked in **Experimental Science**.

The **Multispectral** sensor can be unlocked in **Advanced Exploration**.

The **BTDT** sensor can be unlocked in **Field Science**.

### 7. Background Scanning
------------------------------------------
Unlike some other KSP scanning systems, SCANsat allows scanning with multiple
vessels.  All online scanners scan at the same time, but only when your *active vessel* has
**at least one** of the parts included in this mod equipped and the mapping interface is open. 

### 8. Time Warp
------------------------------------------
SCANsat does not interpolate satellite paths during time warp; nevertheless, due to the relatively large field of view
of each sensor, it's still possible to acquire data faster by time warping. The maximum recommended time warp speed
is currently **1000x**. Scanning at this warp factor should allow identical scanning performance 
(in terms of [swath width](http://en.wikipedia.org/wiki/Swath_width)) as scanning at *1x*.

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

[SCANsat:issues]: https://github.com/technogeeky/SCANsat/issues
[SCANsat:pulls]: https://github.com/technogeeky/SCANsat/pulls
[SCANsat:source]: https://github.com/technogeeky/SCANsat
[SCANsat:licenses]: https://github.com/technogeeky/SCANsat/blob/master/LICENSE.txt

[KSP:developers]: https://kerbalspaceprogram.com/index.php

[0]: https://github.com/technogeeky/SCANsat#0-authors-maintainers-contributors-and-licenses
[1]: https://github.com/technogeeky/SCANsat#1-installation
[2]: https://github.com/technogeeky/SCANsat#2-types-of-scans
[3]: https://github.com/technogeeky/SCANsat#3-basic-usage
[4]: https://github.com/technogeeky/SCANsat#4-big-map
[5]: https://github.com/technogeeky/SCANsat#5-parts-and-sensor-types
[6]: https://github.com/technogeeky/SCANsat#6-career-mode-research-and-development
[7]: https://github.com/technogeeky/SCANsat#7-background-scanning
[8]: https://github.com/technogeeky/SCANsat#8-time-warp
[9]: https://github.com/technogeeky/SCANsat#9-note-concerning-data-sources

