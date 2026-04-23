## Overview

Plate solving is a method used to determine exactly where the telescope is pointing in the sky by comparing the star field in an image to a database of star positions. Upon matching the image's stars, the plate solving application returns the right ascension (RA) and declination (Dec) of the center of the image. N.I.N.A. understands this output, and may be used to synchronize the mount to that coordinate. This gives the mount a very accurate notion of where it is pointing in space, making subsequent slews very precise.

!!! important
	Plate solving requires that your camera and telescope settings are correct. The two important settings are the camera's sensor pixel size and the focal length of the telescope. N.I.N.A. will usually be able to detect the sensor's pixel size, but the user **will** need to inform the software of the effective focal length of the optical path. This includes the focal length of the telescope itself and any focal length-affecting devices such as reducers, telecompressors, or barlows.


## Manual Plate Solving

To manually trigger plate solving of an image, you need to use the Plate Solving tool in the image panel.
Clicking on the play button there will capture a new image and solve it based on the given parameters.
Should the solve attempt fail, N.I.N.A. will prompt if the Blind Solver should be used to solve the image.
The reason for this is that while most plate solving software is quite good, none are infallible.
Where one fails to solve a given image, another might succeed.
Thus, the Blind Solver acts as a backup method.

Before being able to plate solve in N.I.N.A., any setup that the primary and blind solvers require must be done in accordance with their respective instructions.

!!! tip
    In the event that both the Primary and Blind plate solving applications fail to produce a result, please verify that your image is in focus and, if necessary, increase the exposure time or change the filter type to allow more stars to be adequately exposed.  
    Furthermore, the last failure is stored in `%LOCALAPPDATA%\NINA\PlateSolver\Failed` to analyze why it failed.

To apply the plate solving results to your mount, you need to enable the Sync option and, if desired, the Reslew To Target option in the Plate Solving panel.
The former will synchronize your mount's notion of its pointing position to the position that the plate solver has determined it is pointed at.
The latter option will make N.I.N.A. slew your mount to the location where it was supposed to be in the first place.
This allows skipping the star alignment process that is typically done during a mount's start-up process.
For precise centering with a given error margin, use the "Repeat until error <" option.



## Automatic Plate Solving

Plate solving is also utilized in the [Sequencer](../sequencer/overview.md) to center on a given target and in the [Automated Meridian Flip](meridianflip.md) feature to recenter the scope after a flip has occurred.
Here plate solving will be triggered automatically and the parameters are used from the Plate Solving Options.
This is essential for a hands-off operation of N.I.N.A. and it is recommended that the application is set up to have a blind solver, should the preferred primary plate solver not work as expected.

## Important Parameters for Plate Solving

In order for plate solving to work properly, a couple of important parameters have to be set as precisely as possible. When these deviate to some degree from the real-world hardware, plate solving software will have a hard time solving the image or may even fail all the time.

1. *Camera Pixel Size*  
Go to Options->Equipment->Camera section, the pixel size needs to be entered in micrometers.
2. *Telescope Focal Length*  
Go to Options->Equipment->Telescope section and set the focal length of your telescope in millimeters. The value has to be the **effective focal length** considering all correctors and optical elements that could shift the overall focal length

!!! tip
    If you are unsure about the effective focal length, you can use nova.astrometry.net to validate your real focal length. Upload a non binned image of a target that used your current setup. Once the image is solved the "Pixel Scale" is displayed. You can extract the focal length now by using the formula  
    "Focal Length = (Camera Pixel Size / Pixel Scale) * 206.26"

## Plate Solving Software

N.I.N.A. supports the most popular plate solving software suites available, each of which has its own benefits and drawbacks. The following is a list of supported plate solving software and their general characteristics.

### ASTAP - *recommended*
Author: Han Kleijn  
URL: [www.hnsky.org/astap.htm](//www.hnsky.org/astap.htm)

The ASTAP (Astrometric STAcking Program) astrometric solver and FITS file viewer is a powerful standalone plate solve application. It is downloaded and installed in two parts - the application itself, and its star database.

**Benefits**

 * Fast
 * Reliable
 * Capable of fast blind solves even when the mount is far off the expected position
 * Does not require an Internet connection

**Drawbacks**

 * Requires precise parameter setup

**Recommendation**

 * Primary Solver: Recommended
 * Blind Solver: Recommended

---

### Astrometry.Net (online)
Author: Astrometry.Net Project  
URL: [astrometry.net](//astrometry.net/)

N.I.N.A. can upload the image to the API servers of Astrometry.Net for them to plate solve it. This requires the user to register for an account on Astrometry.Net and generate an API key that is then entered into the plate solve settings for Astrometry.Net.

**Benefits**

 * Reliable when the mount's location is unknown or far off the expected position
 * Does not need to know the camera's sensor pixel size or telescope's focal length

**Drawbacks**

 * Requires an Internet connection
 * Slow

**Recommendation**

 * Primary Solver: Not recommended
 * Blind Solver: Recommended when an Internet connection is available

---

### Local Astrometry.Net (ansvr)
Author: Andy Glasso  
URL: [adgsoftware.com/ansvr/](//adgsoftware.com/ansvr/)

The local Astrometry.Net plate solver needs to be installed separately.
It requires downloading index files, which can be installed through N.I.N.A., for your combination of focal length and pixel size. See plate solving settings.

**Benefits**

 * Reasonably fast when the mount's location is unknown or far off the expected position
 * Fast when the mount's location is close
 * Does not require an Internet connection

**Drawbacks**

 * The downloading and installation of the correct and numerous index files is crucial for performance
 * Can mistake hot pixels for stars (a possible issue with DSLRs and other noisy sensors)
 * Most installer bundles for Windows deliver rather outdated versions

**Recommendation**

 * Primary Solver: Not recommended
 * Blind Solver: Not recommended 

---

### All Sky Plate Solver
Author: Giovanni Benintende  
URL: [astrogb.com/astrogb/All_Sky_Plate_Solver.html](http://www.astrogb.com/astrogb/All_Sky_Plate_Solver.html)

This application is basically a wrapper for the local astrometry.net client. 

**Benefits**

* Much easier to set up than the local astrometry.net client
* Does not require an Internet connection

**Drawbacks**

 * Same as local astrometry.net client, except for the setup part

**Recommendation**

 * Primary Solver: Not recommended
 * Blind Solver: Not recommended 

---

### PlateSolve3.80
Author: PlaneWave Instruments (Dave Rowe)
URL: [PlateSolve 3.80](https://drive.google.com/drive/folders/1JCgzmSJGBXOtfYrqpG5Jvp_Dr_gvEOgt)

N.I.N.A. was kindly selected to include the improved PlateSolve3.80 in its arsenal of plate solvers, courtesy of Dave Rowe of PlaneWave Instruments.
PlateSolve3 is a standalone executable. It handles longer focal lengths and small FOVs, and solves quickly with few stars (<10).

PlateSolve3.80's catalogs are included in the zip file. They must be installed before plate solving.
Run PlateSolve3.80, then go to the File menu and click on "Configure Directory...". For the UCAC4 catalog, select the Kepler directory, and for the GaiaDR2 catalog, select the UD Catalog directory. Close the application.

**Benefits**

* Reliable with long focal lengths and small FOVs (tested up to 24000mm)
* Very fast blind solving
* Few stars required; stars can be moderately elongated/distorted
* Does not require an Internet connection

**Drawbacks**

* The regional settings of Windows must be set to use a point as the decimal symbol

**Recommendation**

* Primary Solver: Recommended
* Blind Solver: Recommended

---

### TheSkyX ImageLink
Author: Software Bisque
URL: [bisque.com](//www.bisque.com/)

TheSkyX ImageLink can be used as a primary plate solver when TheSkyX is installed and reachable through its TCP server.

**Benefits**

* Useful for users already running TheSkyX
* Can integrate with an existing TheSkyX workflow

**Drawbacks**

* Requires TheSkyX to be installed, licensed, and configured
* Not available as a blind solver

**Recommendation**

* Primary Solver: Recommended for TheSkyX users
* Blind Solver: Not available

---

### PinPoint
Author: Diffraction Limited
URL: [diffractionlimited.com](//diffractionlimited.com/)

PinPoint can be used as a primary or blind plate solver when its astrometric engine and required catalogs are installed.

**Benefits**

* Can perform local catalog-based solving
* Can fall back to an AllSky API solve when configured

**Drawbacks**

* Requires PinPoint and its 64-bit component to be installed
* Requires catalog and optional AllSky API settings to be configured

**Recommendation**

* Primary Solver: Recommended for PinPoint users
* Blind Solver: Recommended for PinPoint users

---

### PlateSolve2
Author: PlaneWave Instruments  
URL: [planewave.com/downloads/software/](//planewave.com/downloads/software/)

PlateSolve2 is a standalone executable which can be downloaded from PlaneWave's PlateSolve2 downloads page.
It requires the download of at least one catalog of stars so it can work properly.
You need to start the executable once standalone and set the catalog location of the catalog that you want to use. Both the APM and UCAC3 catalogs will work fine, but it is recommended to download both of them should you encounter issues with either one of them.

**Benefits**

* Very fast when the mount's location is close to the expected position
* Does not require an Internet connection

**Drawbacks**

* Slow plate solves when the mount's location is far off the expected position and the focal length of the telescope is long
* The regional settings of Windows need to be set to use a point as the decimal symbol

**Recommendation**

* Primary Solver: Recommended
* Blind Solver: cannot be used as a Blind Solver
