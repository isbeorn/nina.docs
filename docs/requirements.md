# System Requirements and Device Support

## Minimum System Requirements

The following represents the minimum system resources required to operate N.I.N.A.

 * A dual core x64 CPU
 * 2GB of RAM
 * Windows 7 (64 bit) or later, however Windows 10 is highly recommended
 * 150MB of free disk space without the optional SkyAtlas image data (1.5GB with)
 * [.NET Framework 4.7.2 Runtime](//dotnet.microsoft.com/download/dotnet-framework/net472) (included with Windows 10 April 2018 Update (build 1803) and later)

!!! info
    Some users have reported successful operation of N.I.N.A. on small, low-power embedded systems such as the Intel Compute Stick and similar platforms. As expected, experiences will vary in such resource-constrained environments. Technically, N.I.N.A. should be able to run on a single core, but this will certainly lead to a *highly* undesirable experience and is certainly not recommended. However, if forced to choose between the two, more RAM is desirable over CPU power.

## Recommended and Optional support software

The functionality of N.I.N.A. shines through when paired with a supporting cast of other applications. Please consider the items in the following lists in order to access the full breadth of N.I.N.A.'s capabilities.

  * [ASCOM 6.4 framework](//ascom-standards.org/Downloads/Index.htm) (recommended)
  * [PHD2 Guiding](//openphdguiding.org/downloads/) (recommended)
  * Any of several supported [plate solving](advanced/platesolving.md) applications (recommended)
    * [All Sky Plate Solver](//www.astrogb.com/astrogb/All_Sky_Plate_Solver.html)
    * [ASTAP](//www.hnsky.org/astap.htm)
    * [Local Astrometry.net (ansvr)](//adgsoftware.com/ansvr/)
    * [PlateSolve2](//planewave.com/downloads/software/)
  * Any of several supported planetarium applications (optional)
    * [Cartes du Ciel](//www.ap-i.net/skychart/)
    * [HNSKY](//www.hnsky.org/)
    * [Stellarium](//stellarium.org/)
    * [TheSky X](//www.bisque.com/sc/pages/TheSkyX-Editions.aspx)
  * [SkyAltas image data](//nighttime-imaging.eu/downloads/SkyAtlasImageRepository/SkyAtlasImageRepository.zip) (optional)


## Supported Devices

### Direct (native) camera support

N.I.N.A. can directly interface with a wide range of popular cameras without the need for an intermediate ASCOM driver. Direct camera access is preferred over accesing the camera through ASCOM for performance reasons and to access additional camera controls that cannot be manipulated through ASCOM.

 * Altair
 * Atik
 * Canon
 * Nikon
 * QHYCCD
 * ToupTek
 * ZWO

!!! note
    Certain older Nikon DSLRs require a serial shutter cable for bulb exposures. Please consult your camera's documentation regarding its requirements for long exposure operation using a USB or other remote cable.

The list of supported cameras can change and expand as N.I.N.A. developers gain access to relevant hardware or support is contributed in some fashion.

### ASCOM Device Support

Astronomy-related equipment often has an [ASCOM](//ascom-standards.org/) driver for it. N.I.N.A. supports accessing the following types of devices through their associated ASCOM drivers, as long as the drivers are fully compliant with the relevant ASCOM frameworks. Cameras which lack direct support in N.I.N.A. but *do* have an ASCOM driver may also be utilized this way. The following ASCOM device classes are suppported:

 * Cameras
 * Mounts (aka "Telescopes")
 * Filter Wheels
 * Focusers
 * Rotators
 * Weather data (ASCOM ObservingConditions)

!!! tip
    Be aware that ASCOM drivers that are provided by their vendor in a 32 bit-only form will **not** be accessible by a 64 bit N.I.N.A. or any other 64 bit ASCOM client application. If this is the case for you, then the 32 bit version of N.I.N.A. must be installed.

!!! info "A note to ASCOM driver developers"
	If it has not already been done, please consider making both 32 and 64 bit varieties of your driver(s) available to your users or customers, and ensure that the driver passes all [ASCOM Conformance](//ascom-standards.org/Developer/Conformance.htm) tests. Please refer to the [relevant documentation](//ascom-standards.org/Developer/DevFor32And64Bits.htm) on ASCOM's website for more information.

### Guiding Applications

N.I.N.A. supports interfacing directly with PHD2 to know the status of guiding and to effect actions such as dithering. Telemetry from PHD2 also can be displayed inside the Imaging tab. If desired, N.I.N.A. can automatically launch PHD2. Currently, PHD2 is the only external guiding application that N.I.N.A. supports in such a way.