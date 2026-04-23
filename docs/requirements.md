# System Requirements and Device Support

## Minimum System Requirements

The following represents the minimum system resources required to operate N.I.N.A.

 * A dual-core x64 CPU
 * 3GB of RAM
 * Windows 10 (64 bit) or later
 * 500MB of free disk space without the optional SkyAtlas image data or framing cache data (3GB with)

!!! info
    Some users have reported successful operation of N.I.N.A. on small, low-power embedded systems such as the Intel Compute Stick and similar platforms. As expected, experiences will vary in such resource-constrained environments. Technically, N.I.N.A. should be able to run on a single core, but this will certainly lead to a *highly* undesirable experience and is certainly not recommended. However, if forced to choose between the two, more RAM is desirable over CPU power.

## Recommended and Optional support software

The functionality of N.I.N.A. shines through when paired with a supporting cast of other applications. Please consider the items in the following lists to access the full breadth of N.I.N.A.'s capabilities.

  * [ASCOM Platform](//ascom-standards.org/Downloads/Index.htm) (recommended)
  * [PHD2 Guiding](//openphdguiding.org/downloads/)
  * [Metaguide Guiding](//https://www.smallstarspot.com/metaguide/)
  * Any of several supported [plate solving applications](advanced/platesolving.md) (recommended)
    * [ASTAP](//www.hnsky.org/astap.htm)
    * [All Sky Plate Solver](http://www.astrogb.com/astrogb/All_Sky_Plate_Solver.html)
    * [Local Astrometry.net (ansvr)](//adgsoftware.com/ansvr/)
    * [PlateSolve2](//planewave.com/downloads/software/)
    * [PlateSolve3](//planewave.com/downloads/software/)
    * [TheSkyX ImageLink](//www.bisque.com/)
    * [PinPoint](//diffractionlimited.com/)
  * Any of several supported planetarium applications (optional)
    * [Cartes du Ciel](//www.ap-i.net/skychart/)
    * [HNSKY](//www.hnsky.org/)
    * [Stellarium](//stellarium.org/)
    * [TheSky X](//www.bisque.com/sc/pages/TheSkyX-Editions.aspx)
    * [C2A](//www.astrosurf.com/c2a/english/download.htm)
    * [SkyTechX](//www.skytechx.eu/)
  * [SkyAltas image data](https://nighttime-imaging.eu/download/) (optional at the bottom of the download section)


## Supported Devices

### Direct (native) camera support

N.I.N.A. can directly interface with a wide range of popular cameras without the need for an intermediate ASCOM driver. Direct camera control is recommended over accessing the camera through ASCOM for performance reasons and to access additional camera controls that cannot be manipulated through ASCOM.

 * [Altair](//www.altairastro.com/)
 * [Atik](//www.atik-cameras.com/)
 * [AstcamPan](//microscope-cameras.com/)
 * [Canon](//global.canon/)
 * [FLI](//www.flicamera.com/)
 * [MallinCam](//www.mallincam.net/)
 * [Nikon](//www.nikon.com)
 * [PlayerOne](//player-one-astronomy.com/)
 * [OGMA](//getogma.com/)
 * [Omegon](//www.omegon.eu)
 * [QHYCCD](//www.qhyccd.com)
 * [RisingCam](//risingcam.aliexpress.com/store/1918400)
 * [ToupTek](//www.touptek-astro.com)
 * [SBIG](//diffractionlimited.com)
 * [SVBony](//www.svbony.com)
 * [ZWO](//www.zwoastro.com)

!!! note
    Certain older Nikon DSLRs require a serial shutter cable for bulb exposures. Please consult your camera's documentation regarding its requirements for long exposure operation using a USB or other remote cable.

The list of supported cameras can change and expand as N.I.N.A. developers gain access to relevant hardware or support is contributed.

### [ASCOM Standard](https://ascom-standards.org/)

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
    Be aware that ASCOM drivers that are provided by their vendor in a 32-bit-only form will **not** be directly accessible by a 64-bit N.I.N.A. or any other 64-bit ASCOM client application. If this is the case for you, try to connect via the ASCOM Device Hub instead.

!!! info "A note to ASCOM driver developers"
	If it has not already been done, please consider making both 32 and 64 bit varieties of your driver(s) available to your users or customers, and ensure that the driver passes all [ASCOM Conformance](//ascom-standards.org/Developer/Conformance.htm) tests. Please refer to the [relevant documentation](//ascom-standards.org/Developer/DevFor32And64Bits.htm) on ASCOM's website for more information.

### [ASCOM Alpaca](//www.ascom-alpaca.org/)

Similar to the ASCOM support N.I.N.A. also offers ASCOM Alpaca device discovery and direct connection to ASCOM Alpaca devices through the network. All types of devices that are mentioned in the ASCOM Standard section are also supported via the ASCOM Alpaca protocol.

### Additional Device Support

While N.I.N.A. offers extensive support for a wide range of devices directly and through ASCOM drivers, there are [plugins](tabs/plugins/available.md) available that can directly or indirectly integrate other devices, providing even more flexibility and options.

* [AstronSCIENTIFIC Rotarion System](//www.astronscientific.com/tech-info/)
    * Although Rotarion is not directly integrated into the application, it can be fully controlled using the available ASCOM drivers as well as the [Connector Plugin](tabs/plugins/available.md) to switch between different profiles that are set up for the different instruments that are attached to the Rotarion system

### Guiding Applications

N.I.N.A. supports several guiding applications to guide, dither, and monitor tracking accuracy. Telemetry from these applications are also displayed inside of the Imaging tab. The guiding applications N.I.N.A. supports are:

  * [PHD2](//openphdguiding.org/)
  * [MetaGuide](//www.astrogeeks.com/Bliss/MetaGuide/)
  * [MGEN2](//mgen-autoguider.com/en/)
  * [MGEN3](//mgen-autoguider.com/en/)
  * [SkyGuard](//www.innovationsforesight.com/)
