# XISF

Extensible Image Serialization Format (XISF) is an image file format created by PixInsight. It is a free format open to contributions of anyone interested. For more detailed information about the format itself refer to [pixinsight.com/xisf](https://pixinsight.com/xisf/).

N.I.N.A. is capable of saving images in XISF format. The XISF format offers a variety of header meta information and N.I.N.A. will populate all available information into this header. A detailed list of all available Headers and their conditions is described below.
Many applications can make use of these headers (e.g. PixInsight during processing).

## Standard XISF Headers

- Version: 1.0
- CreationTime: Time when the file is created
- CreatorApplication: N.I.N.A. - Nighttime Imaging 'N' Astronomy

## [Observation Namespace](http://pixinsight.com/doc/docs/XISF-1.0-spec/XISF-1.0-spec.html#__XISF_Core_Elements_:_Image_Core_Element_:_Astronomical_Image_Properties_:_Observation_Namespace__)

### Time
- Start: UTC time at exposure start

### Location
- Elevation: Elevation (currently taken from a connected telescope)
- Latitude: Latitude taken from the astrometry settings
- Longitude: Longitude taken from the astrometry settings

### Center
Requires a telescope to be connected

- RA: Current telescope's right ascension coordinates
- Dec: Current telescope's declination coordinates

### Object
Available when a target is set inside a sequence.

- Name: Name of object
- RA: Right ascension of target
- Dec: Declination of target

### Meterology
Requires a weather data source to be connected

- RelativeHumidity: Relative humidity percentage
- AtmosphericPressure: Air pressure in hPa
- AmbientTemperature: Ambient air temperature in °C
- WindDirection: Wind direction: 0=N, 180=S, 90=E, 270=W
- WindGust: Wind gust in kph
- WindSpeed: Wind speed in kph

## [Instrument Namespace](http://pixinsight.com/doc/docs/XISF-1.0-spec/XISF-1.0-spec.html#__XISF_Core_Elements_:_Image_Core_Element_:_Astronomical_Image_Properties_:_Instrument_Namespace__)
- ExposureTime: Expsure duration in seconds

### Camera
Requires a camera to be connected

- Name: Name of camera
- Gain: Electrons per A/D unit (only available for some cameras)
- XBinning: X binning factor
- YBinning: Y binning factor

### Sensor
Requires a camera to be connected

- Temperature: actual sensor temperature (requires a cooling unit)
- XPixelSize: Pixelsize
- YPixelSize: Pixelsize

### Telescope
Requires a telescope to be connected

- Name: Name of telescope
- FocalLength: Focal length (taken from equipment options)
- Aperture: Focal length / Focal ratio (taken from equipment options)

### Filter
Requires a filterwheel to be connected

- Name: Current active filter

### Focuser
Requires a focuser to be connected

- Position: Current step position



!!! tip 
    Additionally all information that is explained [in the FITS description](fits.md) is stored using the [FITSKeyword Core Element](http://pixinsight.com/doc/docs/XISF-1.0-spec/XISF-1.0-spec.html#__XISF_Core_Elements_:_FITSKeyword_Core_Element__).