# System Requirements and Device Support

## Minimum System Requirements

The following represents the minimum system resources required to operate N.I.N.A.

 * A dual-core x64 CPU
 * 3GB of RAM
 * Windows 8.1 (64 bit) or later, however Windows 10 is highly recommended
 * 350MB of free disk space without the optional SkyAtlas image data (1.5GB with)
 * [.NET Framework 4.8 Runtime](//dotnet.microsoft.com/download/dotnet-framework/net48) (included with Windows 10 May 2019 Update (build 1903) and later)

!!! info
    Some users have reported successful operation of N.I.N.A. on small, low-power embedded systems such as the Intel Compute Stick and similar platforms. As expected, experiences will vary in such resource-constrained environments. Technically, N.I.N.A. should be able to run on a single core, but this will certainly lead to a *highly* undesirable experience and is certainly not recommended. However, if forced to choose between the two, more RAM is desirable over CPU power.

## Recommended and Optional support software

The functionality of N.I.N.A. shines through when paired with a supporting cast of other applications. Please consider the items in the following lists to access the full breadth of N.I.N.A.'s capabilities.

  * [ASCOM 6.5SP1 Platform](//ascom-standards.org/Downloads/Index.htm) (recommended)
  * [PHD2 Guiding](//openphdguiding.org/downloads/)
  * [Metaguide Guiding](//https://www.smallstarspot.com/metaguide/)
  * Any of several supported [plate solving applications](advanced/platesolving.md) (recommended)
    * [ASTAP](//www.hnsky.org/astap.htm)
    * [All Sky Plate Solver](http://www.astrogb.com/astrogb/All_Sky_Plate_Solver.html)
    * [Local Astrometry.net (ansvr)](//adgsoftware.com/ansvr/)
    * [PlateSolve2](//planewave.com/downloads/software/)
  * Any of several supported planetarium applications (optional)
    * [Cartes du Ciel](//www.ap-i.net/skychart/)
    * [HNSKY](//www.hnsky.org/)
    * [Stellarium](//stellarium.org/)
    * [TheSky X](//www.bisque.com/sc/pages/TheSkyX-Editions.aspx)
  * [SkyAltas image data](https://nighttime-imaging.eu/download/) (optional at the bottom of the download section)


## Supported Devices

### Direct (native) camera support

N.I.N.A. can directly interface with a wide range of popular cameras without the need for an intermediate ASCOM driver. Direct camera control is recommended over accessing the camera through ASCOM for performance reasons and to access additional camera controls that cannot be manipulated through ASCOM.

 * Altair
 * Atik
 * Canon
 * Nikon
 * FLI
 * Omegon
 * QHYCCD
 * RisingCam
 * ToupTek
 * ZWO
 * SVBony
 * SBIG

!!! note
    Certain older Nikon DSLRs require a serial shutter cable for bulb exposures. Please consult your camera's documentation regarding its requirements for long exposure operation using a USB or other remote cable.

The list of supported cameras can change and expand as N.I.N.A. developers gain access to relevant hardware or support is contributed.

### ASCOM Device Support

Astronomy-related equipment often has an [ASCOM](//ascom-standards.org/) driver for it. N.I.N.A. supports accessing the following types of devices through their associated ASCOM drivers, as long as the drivers are fully compliant with the relevant ASCOM frameworks. Cameras that lack direct support in N.I.N.A. but *do* have an ASCOM driver may also be utilized this way. The following ASCOM device classes are supported:

 * Cameras
 * Mounts (aka "Telescopes")
 * Filter Wheels
 * Flat Panels (ASCOM Cover Calibrator) 
 * Focusers
 * Rotators
 * Weather data (ASCOM ObservingConditions)
 * Domes
 * Switches
 * Safety Devices

!!! tip
    Be aware that ASCOM drivers that are provided by their vendor in a 32 bit-only form will **not** be accessible by a 64 bit N.I.N.A. or any other 64 bit ASCOM client application. If this is the case for you, then the 32 bit version of N.I.N.A. must be installed.

!!! info "A note to ASCOM driver developers"
	If it has not already been done, please consider making both 32 and 64 bit varieties of your driver(s) available to your users or customers, and ensure that the driver passes all [ASCOM Conformance](//ascom-standards.org/Developer/Conformance.htm) tests. Please refer to the [relevant documentation](//ascom-standards.org/Developer/DevFor32And64Bits.htm) on ASCOM's website for more information.

### Guiding Applications

N.I.N.A. supports several guiding applications to guide, dither, and monitor tracking accuracy. Telemetry from these applications are also displayed inside of the Imaging tab. The guiding applications N.I.N.A. supports are:

  * [PHD2](https://openphdguiding.org/)
  * [MetaGuide](http://www.astrogeeks.com/Bliss/MetaGuide/)
  * [MGEN2](https://mgen-autoguider.com/en/)
  * [MGEN3](https://mgen-autoguider.com/en/)
