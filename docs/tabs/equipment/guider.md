The Guider tab lets you connect to supported guiders and configure the settings used for guiding and dithering.

![Guider](../../images/tabs/equipment_guider.png)

The header contains the usual guider controls for connecting, disconnecting, refreshing the device list, and opening the setup dialog when one is available.

## Overview

The left side of the page shows guider status and graph settings:

* connected state
* current guider state
* guider pixel scale
* guider dither distance
* main camera pixel scale
* equivalent main camera dither distance
* guide graph correction visibility
* RA and Dec guide graph colors

The right side contains the guider-specific settings page for the selected guider. Dedicated settings pages are available for:

* PHD2
* MGEN
* MetaGuide
* Direct Guider
* SkyGuard

The graph at the bottom shows live guide drift, optional correction bars, and dither markers.

## PHD2 Setup
![Guider](../../images/tabs/guider_phdsetup.png)

### PHD2 Path
PHD2 installation path. This is used to start PHD2 when it is not already running.

### PHD2 Server URL
You can set the PHD2 server settings here.
> Usually the defaults should work fine. You also need to enable PHD2 server in PHD2.

### PHD2 Server Port
PHD server port. Usually the default 4400 works fine. If you are using multiple instances of PHD2, each instance will add 1 to the port number. So instance 2 will run on port 4401, instance 3 on 4402, etc.

### PHD2 Instance Number
Use this when you run more than one PHD2 instance on the same machine.

## PHD2 Settings
![Guider](../../images/tabs/guider_phdsettings.png)

### Dither Pixels and Dither RA Only
The amount of guide camera pixels to dither in PHD2. If "Dither RA only" is checked, the dither movements will only be performed in RA. 
    
!!!tip
    Refer to [Dithering](../../advanced/dithering.md) in Advanced documentation topics for more information about Dithering and how to set the above parameters
  

### Settle Pixel Tolerance
The threshold expressed in guide camera pixels that will determine a dither settling completion after a dither move.

!!!tip
    A dither will be considered settled if, after the duration of "Minimum Settle Time" and before the "PHD2 Settle Timeout", the guide movements in PHD2 will be below the "PHD2 Settle Pixel Tolerance".

### Minimum Settle Time
The minimum time the settling should wait after a dithering process until the process is complete.

### Settle Timeout
The maximum time N.I.N.A. should wait during a settling process until it starts the next action.
### Guiding Start Retry
If PHD2 fails to restart guiding, N.I.N.A. will send a new start guiding command again until guiding is successfully initiated.
  
### Guiding Start Timeout (seconds)
Seconds to wait before sending a new start guiding command to PHD2.

### ROI percentage to find guide star
A region of interest expressed in a percentage of the full frame with the center of the frame as reference. If you want to prevent guide star selection near the edges of the frame, decrease this percentage.

### PHD2 profile
Select from the list of available PHD2 profiles to switch to.

## Other Guider-Specific Pages

If you select a guider other than PHD2, the settings panel changes to match that guider. For example, MetaGuide, MGEN, Direct Guider, and SkyGuard each expose their own dedicated controls when selected.
