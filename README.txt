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

A bigger map can be rendered on demand. Rendered maps are automatically
saved to GameData/SCANsat/PluginData. Note that position indicators for
vessels are not visible on exported images.


4. Background Scanning
----------------------

Currently, only partial background scanning is supported. All online
scanners scan at the same time, but only when your active vessel has
one of the parts included in this mod equipped and the mapping
interface open.


5. Time Warp
------------

This mod doesn't interpolate satellite paths during time warp, but
due to the relatively large field of view of each sensor, it's still
possible to acquire data faster by time warping. 


6. Research & Development
-------------------------

The parts are currently all hooked into the Science Tech node.
This is not final. Expect inconvenient changes in the future, in 
particular to the more advanced sensors.





