The Dome Tab lets you connect an ASCOM-compatible Dome.

![Dome](../../images/tabs/Equipment-Dome0.png)
N.I.N.A. provides some useful capabilities when connected to a Dome. They include:<p>

1. **Telescope Following**
<br/>When a Telescope is connected, enabling *Dome follows telescope* ensures that the Dome azimuth (where the opening of a Dome is) is centered wherever the Telescope is pointed. This covers several different cases:
      * *Tracking* - As the telescope follows the rotation of the Earth, the Dome stays in sync with it.
      * *External Slew* - If a program other than N.I.N.A. slews the telescope, then the Dome will rotate until the telescope stops and they are lined back up. This movement can be jerky however - some Domes (such as NexDome) don't allow changing the destination azimuth while it is rotating, so N.I.N.A. repeatedly sends slew commands based on wherever the telescope is pointing at the time.
      * *N.I.N.A. Slew* - If N.I.N.A. issues a slew command to the telescope (such as from the Framing Wizard), then the Dome will go directly to the target azimuth that would be in sync with the telescope at its destination.

2. **Find Home Before Park**
<br/>This setting can be found in the [Dome Options](../options/dome.md). Domes with a Home position (such as NexDome) use a sensor to synchronize the physical azimuth with what is in the software. This is conceptually similar to star alignment with a mount. Finding the home position before parking increases the reliability of finding the precise park location, which can be important if that is where batteries recharge.

3. **Wait for Dome Synchronization**
<br/>When the telescope moves, the Dome typically needs extra time to follow it and re-synchronize their azimuths. N.I.N.A. waits until the telescope stops slewing *and* the Dome is synchronized with it before starting operations that take images (such as Plate Solving and Auto Focus).

4. **Manual Shutter Control**
<br/>The Dome shutter can be directly opened and closed.

    !!! note
        There is no manual rotation control. Unfortunately, the ASCOM standard doesn't provide operations that would make this possible without jerky behavior, such as rotating 10 degrees at a time.
