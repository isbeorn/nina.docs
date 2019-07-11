# FITS

Flexible Image Transport System (FITS) is a flexible open file format for images, spectra, photon lists, data cubes etc. The data is stored in N-dimensional arrays or tables. 

N.I.N.A. is capable of saving images in FITS format. The FITS format offers a variety of header meta information and N.I.N.A. will populate all available information into this header. A detailed list of all available Headers and their conditions is described below.
Many applications can make use of these headers (e.g. PixInsight during processing).

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
- EXPOSURE: Expsure duration in seconds
- DATE-LOC: Locale time at exposure start
- DATE-UTC: UTC time at exposure start

## Observer Headers
Taken from the Astrometry Options

- SITEELEV: Elevation (currently taken from a connected telescope)
- SITELAT: Latitude 
- SITELONG: Longitude

## Target Headers
Available when a target is set inside a sequence.

- OBJECT: Name of object
- OBJCTRA: Right ascension of target
- OBJCTDEC: Declination of target

## Camera Headers
Requires a camera to be connected

- INSTRUME: Name of camera
- XBINNING: X binning factor
- YBINNING: Y binning factor
- GAIN: Gain
- OFFSET: Offset (if camera can set an offset)
- EGAIN: Electrons per A/D unit (only available for some cameras)
- XPIXSZ: Pixelsize
- SET-TEMP: tempterature set point (requires a cooling unit)
- CCD-TEMP: actual sensor temperature (requires a cooling unit)

## Telescope Headers
Requires a telescope to be connected

- TELESCOP: Name of telescope
- FOCALLEN: Focal length (taken from equipment options)
- FOCRATIO: Focal ratio (taken from equipment options)
- RA: Current telescope's right ascension coordinates
- DEC: Current telescope's declination coordinates


## Filterwheel Headers
Requires a filterwheel to be connected

- FWHEEL: Name of filterwheel
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
- ROTATOR: Current position
- ROTATANG: Current position
- ROTSTPSZ: Step size

## Weather Data Headers
Requires a weather data source to be connected

- CLOUDCVR: Cloud cover percantage
- DEWPOINT: Dew point in °C
- HUMIDITY: Humidity percentage
- PRESSURE: Air pressure in hPa
- SKYBRGHT: Sky brightness in lux
- MPSAS: Sky quality in mags/arcsecs²
- SKYTEMP: Sky temperature in °C
- STARFWHM: Star FWHM
- AMBTEMP: Ambient air temperature in °C
- WINDDIR: Wind direction: 0=N, 180=S, 90=E, 270=W
- WINDGUST: Wind gust in kph
- WINDSPD: Wind speed in kph
