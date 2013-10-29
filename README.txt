
The Scientific Committee on Advanced Navigation brings you the

	S.C.A.N. High Performance Terrain Scanning Sensors Family
	---------------------------------------------------------
	
THIS IS A WORK IN PROGRESS:
Generally, expect that it doesn't work, and doesn't progress.
Install at your own peril.


0. Authors and License
----------------------

Currently only one author:
	- damny <df.ksp@erinye.com>
	
For licensing information, please see the included LICENSE.txt file.
Source code is available here:

	https://github.com/thatfool/SCAN
	
	
1. Installation
---------------

Put the SCANsat folder in your KSP installation's GameData folder.


2. Included Parts
-----------------

SCANsat includes several different terrain scanner modules that
perform the following functions:

  - basic RADAR altimetry
  - high-resolution RADAR altimetry
  - slope detection
  - SAR biome/anomaly detection
  - anomaly identification
  
Also included is a mobile navigation unit that doesn't scan but still 
allows access to the mapping interface. You know, so you can actually
use your maps and see where you are.

EXPECTATION MANAGEMENT DISCLAIMER:
The intention behind these various sensors is that it should be possible
to acquire a useful map of a planet by playing normally. I've also had
to make some compromises to get useful performance. This unfortunately
means realism is currently not a very high priority here...


3. Basic Usage
--------------

Put scanner part on rocket, aim rocket at sky, launch.
If your rocket is not pointing at the sky, you are probably not
going to map today, because most sensors only work above 5 km.

Start scanning via the part's context menu. (The navigation unit
allows opening the mapping interface in the same way.) 

The mapping interface consists of a small-ish map of the planet, as
far as it has been scanned in your current game. It updates sorta
live-ish and shows positions of the active vessel, as well as other
scanning vessels in orbit around the same planet.


4. "Big Map" (for lack of a better name)
------------

A bigger map can be rendered on demand. Rendered maps are automatically
saved to GameData/SCANsat/PluginData. Note that position indicators for
vessels or anomalies are not visible on exported images.

You can mouse over the big map (seriously, I need a better name) to see
what sensors have data for the location, as well as terrain elevation.

Right-clicking on the big map (this name is starting to annoy me) shows
a magnified view around the position where you clicked. Mouse operations
work inside this magnified view just like they work outside, meaning the
data displayed at the bottom window applies to your position inside the
magnified view, and right-clicking inside it will increase magnification.


5. Parts and Sensor Types
-------------------------

Note: Currently, all parts in this mod share the same ugly test run mesh
that I made to figure out how to get meshes into a mod. Sorry about that.

a) SCAN RADAR Altimetry Sensor

This is your basic RADAR altimetry sensor. Regions scanned by this 
sensor are only available in greyscale, displayed elevations are rounded
to the next multiple of 500 m, and zoom is severely restricted. In other
words, it's pretty shoddy, but it has a large field of view and should
get some good coverage even out of the worst possible mapping orbit.

b) SCAN Advanced Altimetry Sensor

The real deal. This altimetry sensor enables colour for regions it scans
(you can still view them in greyscale, if you prefer that), elevations
are reported accurately, zoom is completely unrestricted... the only
downside is that it doesn't cover as much land, which makes it a less
suitable choice for badly chosen orbits. 

c) SCAN Slope Scanner

This scanner scans terrain smoothness... I know it says "slope", and if
you don't look at the code you'll never know it doesn't *actually* show
the slope, but you know. It's just another way to look at the terrain.
Renders can look quite nice but it's not necessarily very useful.

d) SCAN SAR Sensor

This sensor doesn't work like a SAR sensor at all, but it can see things
that you wouldn't see with your ordinary RADAR sensor without an antenna
from the KSC all the way to the Mun. In particular, it can detect biomes
on planets that have biomes. It can also detect anomalies, although it's
not quite advanced enough to identify them.

The biome map is currently not 100% accurate, because the function that
seems to be meant for mods to query the biome at a given location also
prints a debug message to the screen, so I had to write my own, using my
own interpretation of "biome with the closest map color". 

e) SCAN Been There Done That®

This close-range sensor is unique among the sensors included in this
package in that it doesn't work from orbit at all, unless you manage a
very low orbit just a few meters above the ground. It detects anomalies,
and is able to identify them as well, but you need to get pretty close
to one for it to work. 

If that didn't get through: This part is meant to let you mark anomalies
you've visited. Put it on a rover at the KSC and its label will change 
from "Anomaly" to "KSC" on the map, and so on.

f) SCAN MapTraq®

The MapTraq® is a map display unit only, it can't gather data itself.
Unfortunately it still shares the ugly RADAR array mock-up mesh with 
the other parts, so there is no real advance of this part over one that
*can* gather data... at some point in the future, it'll be smaller and
lighter and not cost as much electrical charge to operate, and so on.


6. Background Scanning
----------------------

Currently, only partial background scanning is supported. All online
scanners scan at the same time, but only when your active vessel has
at least one of the parts included in this mod equipped and the mapping
interface is open. 


7. Time Warp
------------

This mod doesn't interpolate satellite paths during time warp, but
due to the relatively large field of view of each sensor, it's still
possible to acquire data faster by time warping. For a typical mapping
orbit around Kerbin, up to 1000x time warp shouldn't skip too much.


8. Research & Development
-------------------------

The parts are currently all hooked into the Science Tech node.
This is not final. Expect inconvenient changes in the future, in 
particular to the more advanced sensors.


9. Note Concerning Data Sources
-------------------------------

All data this mod shows you is pulled from your game as you play. This
includes terrain elevation, biomes, and so on, and also anomalies. I can't
guarantee that all anomalies will be found; in particular, some are so close
to others that they don't show up on their own, and if the devs want to be
sneaky then they can of course be sneaky.
 