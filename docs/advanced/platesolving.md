## Overview

Plate solving is a method used to determine exactly where the telescope is pointing in the sky by comparing the star field in an image to a database of star positions. Upon matching the image's stars, the plate solving application returns the right ascension (RA) and declination (Dec) of the center of the image. N.I.N.A. understands this output, and may be used to sychronise the mount to that coordinate. This gives the mount a very accurate notion of where it is pointing in space, making subsequent slews very precise.

!!! important
	Plate solving requires that your camera and telescope settings are correct. The two important settings are the camera's sensor pixel size and the focal length of the telescope. N.I.N.A. will usually be able to detect the sensor's pixel size, but the user **will** need to inform the software of the effective focal length of the optical path. This includes the focal length of the telescope itself and any focal length-affecting devices such as reducers, telecompressors, or barlows.


## General Plate Solving

To plate solve an image you need to capture an image and use the Plate solve current image button in the image panel. This will start the procedure of plate solving your image with the Plate Solver as set in the plate solving settings. Should the solve attempt fail, N.I.N.A. then use the Blind Solver to solve the image. The reason for this is that while most plate solving software is quite good, none are infallible. Where one fails to solve a given image, another might succeed. Thus, the Blind Solver acts as a back up method.

Before being able to plate solve N.I.N.A., any setup that the primary and blind solvers require must be done in accordance with their respective instructions.

!!! tip
    In the event that both the Primary and Blind plate solving applications fail to produce a result, please verify that your image is in focus and, if necessary, increase the exposure time or change the filter type to allow more stars to be adequately exposed.

To apply the plate solving results to your mount, you need to enable the Sync option and, if desired, tje Reslew To Target option in the Plate Solving panel. The former will synchronize your mount's notion of its pointing position to the position that the plate solver has determined is it pointed. The latter option will make N.I.N.A. slew your mount to the location where it was supposed to be in the first place. This cam allow this skipping of the star alignment process that is typically done during a mount's start-up process.

Plate solving is also utilized in the [Automated Meridian Flip](meridianflip.md) feature to re-center the image after a meridian flip has been performed. This is essential for a hands-off operation of N.I.N.A. It is recommended that all plate solvers be set up to have a backup Blind Solver should the preferred Primary plate solver not work as expected.

## Plate Solving Software

N.I.N.A. supports the most popular plate solving software suites available, each of which has its own benefits and drawbacks. The following is a list of support plate solving software and their general characteristics.

### ASTAP
Author: Han Kleijn  
URL: [www.hnsky.org/astap.htm](//www.hnsky.org/astap.htm)

The ASTAP (Astrometric STAcking Program) astrometric solver and FITS file viewer is a powerful standalone plate solve application. It is downloaded and installed in two parts - the application itself, and its star database.

**Benefits**

 * Fast
 * Reliable
 * Capable of fast blind solves even when the mount is far off the expected position
 * Does not require an Internet connection

**Drawbacks**

 * None known

**Recommendation**

This solver is recommended as both Primary and Blind Solver.

---

### Astrometry.Net (online)
Author: Astrometry.Net Project  
URL: [astrometry.net](//astrometry.net/)

N.I.N.A. can upload the image to the API servers of Astrometry.Net for them to plate solve it. This requires the user to register for an account on Astrometry.Net and generate an API key that is then entered into the plate solve settings for Astrometry.Net.

**Benefits**

 * Reliable when the mount's location is unknown or far off the expected position
 * Does not need to know the camera's sensor pixel size or telescopes focal length

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

The local Astrometry.Net plate solver needs to be installed separately. It requires download of index files which can be installed through N.I.N.A., as well as the required index files that you need for your combination of focal length and pixel size. See plate solving settings.

**Benefits**

 * Reasonably fast when the mount's location is unknown or far off the expected position
 * Fast when the mounts location is close
 * Does not require an Internet connection

**Drawbacks**

 * The downloading and installation of the correct and numerous index files is crucial for performance
 * Can mistake hot pixels for stars (a possible issue with DSLRs and other noisy sensors)
 * Most installer bundles for Windows deliver rather outdated versions

**Recommendation**

 * Primary Solver: Not recommended
 * Blind Solver: Not recommended 

---

### PlateSolve2
Author: PlaneWave Instruments  
URL: [planewave.com/downloads/software/](//planewave.com/downloads/software/)

PlateSolve2 is a standalone executable which can be downloaded from Planewave's PlateSolve2 downloads page. It requires the download of at least one catalogue of stars so it can properly work. You need to start the executable once standalone and set the catalog location of the catalog that you want to use. Both the APM or UCAC3 catalogues will work fine, but it is recommended to download both of them should you encounter issues with either one of them.

**Benefits**

 * Very fast when the mount's location is close to expected position
 * Does not require an Internet connection

**Drawbacks**

 * Slow plate solves when the mount's location is far off the expected position and the focal length of the telescope is long

**Recommendation**

 * Primary Solver: Recommended
 * Blind Solver: use as a Blind Solver is not technically possible