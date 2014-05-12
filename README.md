The Scientific Committee on Advanced Navigation is proud to bring you:

S.C.A.N. High Performance Scanning Sensors
==========================================

0. Authors, Maintainers, Contributors, and Licenses
-------------------------------------------------------------------------------

###Authors
The current authors include:
  + [technogeeky][technogeeky] <technogeeky@gmail.com>
  + [DMagic][DMagic] <david.grandy@gmail.com>

Past authors include:
  + [damny][damny] <missing-in-action@nowhere-to-be-found.com>

As of May 2014, the vast majority of code is damny's, and technogeeky and DMagic are very slowly encroaching.

### Maintainers
The current maintainer is:
  + [technogeeky][technogeeky] <technogeeky@gmail.com>

Maintainers are the people who you should complain to if there is something wrong.

Complaints in the form of [GitHub Issues][SCANsat:issues] are given **higher** priority than other complaints;
complaints in the form of [GitHub Pull Requests][SCANsat:pulls] are given the **highest** priority possible.


### Contributors

In addition to the authors, the following people have contributed:
  + (Models, Graphics, Textures) [Milkshakefiend][Milkshakefiend]

### Licenses

For licensing information, please see the [included LICENSE.txt][SCANsat:licenses] file.

[Source Code][SCANsat:source] is available, as some licenses may require.


1. Installation
-------------------------------------------------------------------------------

  1. Put the SCANsat folder in your KSP installation's GameData folder.
  2. (Optional) Place the SCANsatRPM folder in your KSP installation's GameData folder.

2. Types of Scans
-------------------------------------------------------------------------------
3. Basic Usage
-------------------------------------------------------------------------------
4. Big Map
-------------------------------------------------------------------------------
5. Parts and Sensor Types
-------------------------------------------------------------------------------
6. (Career Mode) Research and Development
-------------------------------------------------------------------------------
7. Background Scanning
-------------------------------------------------------------------------------
Unlike some other KSP scanning systems, SCANsat allows scanning with multiple
vessels.  All online scanners scan at the same time, but only when your *active vessel* has
**at least one** of the parts included in this mod equipped and the mapping interface is open. 
8. Time Warp
-------------------------------------------------------------------------------
SCANsat does not interpolate satellite paths during time warp; nevertheless, due to the relatively large field of view
of each sensor, it's still possible to acquire data faster by time warping. The maximum recommended time warp speed
is currently **1000x**. Scanning at this warp factor should allow identical scanning performance 
(in terms of [swath width](http://en.wikipedia.org/wiki/Swath_width)) as scanning at *1x*.
9. Note Concerning Data Sources
-------------------------------------------------------------------------------

All data this mod shows you is pulled from your game as you play. This
includes:
  * terrain elevation
  * biomes
  * anomiles

I can't guarantee that all anomalies will be found; in particular, some are so close
to others that they don't show up on their own, and if the [developers][KSP:developers] want to be
sneaky then they can of course be sneaky.
[technogeeky]: http://forum.kerbalspaceprogram.com/members/110153-technogeeky
[DMagic]: http://forum.kerbalspaceprogram.com/members/59127-DMagic
[damny]: http://forum.kerbalspaceprogram.com/members/80692-damny
[Milkshakefiend]: http://forum.kerbalspaceprogram.com/members/72507-Milkshakefiend

[SCANsat:issues]: https://github.com/technogeeky/SCANsat/issues
[SCANsat:pulls]: https://github.com/technogeeky/SCANsat/pulls
[SCANsat:source]: https://github.com/technogeeky/SCANsat
[SCANsat:licenses]: https://github.com/technogeeky/SCANsat/blob/master/LICENSE.txt

[KSP:developers]: https://kerbalspaceprogram.com/index.php

