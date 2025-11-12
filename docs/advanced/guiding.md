## Overview
Telescope mounts inherently have imperfections that prevent them from perfectly tracking objects in the sky as the Earth rotates. These issues include:
  * Imperfect polar alignment
  * Mechanical errors in drive gearing
  * Wind and other environmental conditions

One way of countering these issues to improve long exposure quality is to use a 2nd camera, a guide scope or Off-Axis Guider, and a **Guiding Application**. It works by locking onto a guide star and sending small corrections (pulses) to counteract guide star movements. N.I.N.A. supports the following **Guiding Applications**:
  * PHD2
  * MGEN2
  * MetaGuide
  * Mount Dither

## PHD2
PHD2 is the most commonly used guiding application. It is free and open source, and can be found [here](https://openphdguiding.org/).

### PHD2 Settings
In order for N.I.N.A. to communicate with PHD2 and command operations such as dithering and for receiving guiding telemetry, PHD2's internal server must be enabled. To enable PHD2's internal server, go to PHD2's **Tools** menu and ensure that **Enable Server** is selected.

![PHD2 Enable Server](../images/advanced/dithering1.png)

There are numerous tutorials online for setting up PHD2. One nice feature N.I.N.A. provides is that it can automatically start PHD2 and connect to it after you've set it up the first time.

## MetaGuide
MetaGuide takes a different approach to guiding than PHD2 by using *Lucky Imaging*. It is free, and provided by Frank Freestar8n [here](http://www.astrogeeks.com/Bliss/MetaGuide/).

### MetaGuide Setup
MetaGuide setup is more involved than PHD2, so be sure to carefully read the documentation. After you've set it up, you can do the following to connect it to N.I.N.A.

  1. Enable the MetaMonitor server. Using a Broadcast Mask of 127.255.255.255 enables connections only over the local PC, which is what we recommend for security. The Broadcast Port default is 1277, but if this doesn't work for you, try another port. The N.I.N.A. contributor that added this needed to use a different port, so they used 1279.<br/>
![Enable MetaMonitor server](../images/advanced/metaguide_monitor.PNG)
  2. Set IP and Port in Equipment -> Guider Settings. The IP address should be 127.0.0.1 if you're following our recommended Broadcast Mask.<br/>
![Set Guider Settings](../images/advanced/metaguide_equipment_settings.PNG)
  3. Make sure Pixel Size (um), Aperture (mm), and Prime F/# are configured properly based on your guide camera and scope. Pixel Scale (arcseconds per pixel) is based on these settings, and N.I.N.A. uses the Pixel Scale reported by MetaGuide to interact with it properly (such as dithering the correct amount).<br/>
![MetaGuide Scope Setup](../images/advanced/metaguide_scope_setup.PNG)

### MetaGuide Extended Status
N.I.N.A. provides the same tracking accuracy and status information for MetaGuide that it does for PHD2 as well as some additional information specific to MetaGuide.<br/>
![MetaGuide Equipment Status](../images/advanced/metaguide_equipment_status.PNG)

  1. **Intensity** is a value from 0-255 that represents saturation of the selected guide star. You typically want this close to 255, and could reduce exposure duration in MetaGuide if you're fully saturated. The value can drop during guiding when clouds come in, which is where the next setting comes in.
  2. **Minimum Intensity** is the lowest value for **Intensity** that N.I.N.A. will allow guiding to continue. If the **Intensity** falls below this threshold for a few seconds, then guiding will pause until **Intensity** is restored
  3. **Dither Settling Time** is how long N.I.N.A. will wait after triggering a dither to resume imaging. This differs from PHD2 which tells N.I.N.A. when guiding has settled after dithering. You'll likely need to tune this based on how your mount behaves in practice.
