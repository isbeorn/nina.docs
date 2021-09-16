
## Camera

Control basic functions of a camera. Each instruction requires at least a camera to be connected.

### Cool Camera  
![Cool Camera](../../images/sequencer/instructions/camera_cool.png)  
Cool your camera to the specified temperature and the specified minimum duration. For most cameras the duration can be left at 0 as the driver will handle the cooling duration.
Once the camera has reached the specified temperature the instruction is completed.  
*Requires a camera that is capable of set point cooling*

### Warm Camera  
![Warm Camera](../../images/sequencer/instructions/camera_warm.png)  
Warm your camera to ambient temperature using the specified minimum duration. For most cameras the duration can be left at 0 as the driver will handle the cooling duration.
Once the camera has reached the ambient temperature the cooler is turned off and the instruction is completed.  
*Requires a camera that is capable of set point cooling*

### Dew Heater
![Dew Heater](../../images/sequencer/instructions/camera_dew.png)  
This instruction will turn on or turn off the camera dew heater  
*Requires a camera that has controllable dew heater*

### Set Readout Mode
![Set Readout Mode](../../images/sequencer/instructions/camera_readout.png)  
Set your camera to a specific readout mode. The number indicates the index of the readout mode from the camera dropdown - starting with 0.  
*Requires a camera with settable readout modes*

### Take Exposure
![Take Exposure](../../images/sequencer/instructions/camera_exposure.png)  
This instruction will take an exposure using the specified exposure time, binning, gain and offset.

### Take Many Exposures
![Take Many Exposures](../../images/sequencer/instructions/camera_manyexposures.png)  
Similar to the "Take Exposure" instruction, but with the added ability to specify a number of exposures to complete before proceeding.

### Smart Exposure
![Smart Exposure](../../images/sequencer/instructions/camera_smartexposure.png)  
Similar to the "Take Many Exposure" instruction, but with the added ability to also specify a specific filter and to dither after a specific amount of exposures.  
Keep dither after exposures to 0, to skip the dither completely.  
*Requires a connected filter wheel to switch filters and a connected guider to dither*

!!!note
    A fun fact - the "Take Many Exposures" and "Smart Exposures" are actually instruction sets with static content that is just displayed like a normal instruction and is bundling together most common imaging operations for convenience

## Dome

## Filter Wheel

## Flat Device

## Focuser

## Guider

## Rotator

## Safety Monitor

## Switch

## Telescope

## Utility