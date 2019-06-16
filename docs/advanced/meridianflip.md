## Overview

Mount operations known as Meridian Flips are important when using a German Equitorial Mount (GEM). The meridian is an imaginary line that divides the sky into east and west halves. It starts at 180 degrees (south) and passes directly overhead to 0 degrees (north). It is static and does not move with the sky. Imaging an object typically begins when it is in the East half of the sky. As the night progresses, the object will approach the meridian, cross it, and then be in the West half of the sky.

When a GEM's RA axis approaches the meridian, with the telescope on the West side of the mount and looking East, a "flip" must be performed to swap the side of the mount that the telescope is on. This is to avoid the mount tracking past the meridian. Otherwise the counterweights will be higher than the telescope (an undesirable situation on some mounts) and the telescope (or some part of it) contacting the pier or tripod legs. Some mounts and equipment configurations are more tolerant than others to these conditions. Some mounts can track for hours after passing the meridian in a counterweight-up condition. Some telescopes are both short enough in length and height that they will not crash into the pier or tripod legs. Every situation is different, so it is up to the user to know when a meridian flip should be commanded.

## Automating Meridian Flips

An Automatic Meridian Flip operation swaps the telescope to the west side of the mount, N.I.N.A. verifies that it is still imaging the desired area of sky through [plate solving]and the imaging session continues. They prevent that your telescope and camera bump into the mount and do major damage to your equipment. N.I.N.A. has built-in functionality for the automated flip, even if your mount does not support it in firmware.

To enable the Automated Meridian Flip you need to enable it in the imaging settings.