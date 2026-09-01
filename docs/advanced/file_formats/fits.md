# FITS

Flexible Image Transport System (FITS) is a flexible open file format for images, spectra, photon lists, data cubes etc. The data is stored in N-dimensional arrays or tables. 

N.I.N.A. is capable of saving images in FITS format. The FITS format offers a variety of header meta information and N.I.N.A. will populate all available information into this header. A detailed list of all available Headers and their conditions is described below.
Many applications can make use of these headers (e.g. PixInsight during processing).

## Compression

FITS files can be saved uncompressed or with RICE, PLIO, HCOMPRESS, GZIP1 or GZIP2 compression. Compression is applied through CFITSIO and remains lossless except where the selected algorithm or downstream software imposes its own restrictions. The optional `.fz` suffix can be added to compressed files. Confirm that the processing software you use supports the selected FITS compression before adopting it for an unattended session.

## Standard FITS Headers

- SIMPLE: true
- BITPIX: 16
- NAXIS: 2
- NAXIS1: image width
- NAXIS2: image height
- BZERO: 32768
- EXTEND: true
- SWCREATE: N.I.N.A. <version> <architecture>

## Image Headers

- IMAGETYP: Type of exposure (LIGHT, DARK etc.)
- EXPOSURE: Exposure duration in seconds
- EXPTIME: Exposure duration in seconds
- DATE-LOC: Locale time at exposure start
- DATE-UTC: UTC time at exposure start
- DATE-AVG: Averaged midpoint time (UTC)
- ROWORDER: TOP-DOWN orientation of the image starting point. [Details at free-astro.org](//free-astro.org/index.php?title=Siril:FITS_orientation)

## Observer Headers
Taken from the Astrometry Options

- SITEELEV: Elevation specified in the astrometry settings
- SITELAT: Latitude specified in astrometry options
- SITELONG: Longitude specified in astrometry options
- OBSERVER: Observer name specified in astrometry options
- OBSERVAT: Observatory name specified in astrometry options
- SITENAME: Site name specified in astrometry options

## Target Headers
Available when a target is set inside a sequence.

- OBJECT: Name of object
- OBJCTRA: Right ascension of target
- OBJCTDEC: Declination of target
- OBJCTROT: Planned rotation of imaged object

## Camera Headers
Requires a camera to be connected

- CAMERAID: The camera id provided by the driver
- INSTRUME: Name of camera
- XBINNING: X binning factor
- YBINNING: Y binning factor
- GAIN: Gain
- OFFSET: Offset (if camera can set an offset)
- EGAIN: Electrons per A/D unit (only available for some cameras)
- XPIXSZ: X-Pixel size
- YPIXSZ: Y-Pixel size
- SET-TEMP: temperature set point (requires a cooling unit)
- CCD-TEMP: actual sensor temperature (requires a cooling unit)
- READOUTM: Sensor readout mode
- BAYERPAT: Sensor bayer pattern
- XBAYROFF: Bayer pattern X axis offset
- YBAYROFF: Bayer pattern Y axis offset

## Telescope Headers
Requires a telescope to be connected

- TELESCOP: Name of telescope
- FOCALLEN: Focal length (taken from equipment options)
- FOCRATIO: Focal ratio (taken from equipment options)
- RA: Current telescope's right ascension coordinates
- DEC: Current telescope's declination coordinates
- PIERSIDE: The side of pier reported by the driver


## Filter wheel Headers
Requires a filter wheel to be connected

- FWHEEL: Name of filter wheel
- FILTER: Current active filter

## Focuser Headers
Requires a focuser to be connected

- FOCNAME: Name of focuser
- FOCPOS: Current step position
- FOCUSPOS: Current step position
- FOCUSSZ: Step size
- FOCTEMP: Temperature (requires temperature probe on focuser)
- FOCUSTEM: Temperature (requires temperature probe on focuser)

## Rotator Headers
Requires a rotator to be connected

- ROTNAME: Name of rotator
- ROTATOR: Rotator angle in degrees
- ROTATANG: Rotator angle in degrees
- ROTSTPSZ: Step size

## Weather Data Headers
Requires a weather data source to be connected

- CLOUDCVR: Cloud cover percentage
- DEWPOINT: Dew point in Celsius
- HUMIDITY: Humidity percentage
- PRESSURE: Air pressure in hPa
- SKYBRGHT: Sky brightness in lux
- MPSAS: Sky quality in mags/arcsecs^2
- SKYTEMP: Sky temperature in Celsius
- STARFWHM: Star FWHM
- AMBTEMP: Ambient air temperature in Celsius
- WINDDIR: Wind direction: 0=N, 180=S, 90=E, 270=W
- WINDGUST: Wind gust in kph
- WINDSPD: Wind speed in kph
