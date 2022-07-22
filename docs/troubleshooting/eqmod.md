In order for N.I.N.A. to work well with EQMOD some basic settings are recommended.

## Setup Screen
To open the setup for EQMOD, select the EQMOD entry in the equipment page and click on the gears icon.

![EQMOD Equipment](../images/troubleshooting/eqmod/eqmod_equipment.png)

A new window will open with the following screen. 

![EQMOD Setup](../images/troubleshooting/eqmod/eqmod_setup.png)

Inside this screen fill out the information marked in the screenshot above

1. The Port that your mount is connected to according to the Windows Device Manager
2. Your location information
3. Check to allow site writes, which will allow to sync your location from N.I.N.A. to the telescope, in case you have maintained your location in N.I.N.A. already
4. Change the Epoch setting from Unknown to JNOW

## Connection Screen

After these first steps have been filled out correctly you can then proceed to connect to the mount.
Once connected EQMOD will launch a separate window showing the current Telescope status. There you need to further adjust some settings.

![EQMOD Settings](../images/troubleshooting/eqmod/eqmod_settings.png)

1. Expand the Telescope panel to reveal the advanced options
2. Set 'User Interface' to Dialog based, which is required when using Plate Solving in N.I.N.A. to properly sync coordinates to the mount
3. Adjust your limits to match your equipment. The limits should be set in a way to not trigger before the time N.I.N.A. issues a meridian flip (which depends on your flip settings in N.I.N.A.), but rather should be the fail safe when something goes wrong
4. In case you are using some guiding software like PHD2 the recommended PulseGuide Rate should be set to at least 0.5, but more likely 0.9 will work best for both RA and DEC