## General

### Advanced settings

![Instruction Settings](../../images/sequencer/instructions/instruction_settings.png)  

When clicking on the three dots on the right side of an instruction, an advanced section will appear where advanced settings for an instruction are revealed.

**Number of attempts**  
This drives how many reattempts an instruction should make in case of failure

**On error**  
When all attempts have been unsuccessful this setting will drive how to continue with the sequence.  
- *Continue*: The sequencer will just continue with the next instruction  
- *Skip current instruction set*: The currently running instruction set will be skipped  
- *Abort*: The sequence will be completely stopped  
- *Skip to end of sequence instructions*: Skip any remaining instructions from the start and target area and continue with the instructions in the sequence end area  

**Reset**  
This button will reset the state of the instruction, like progress exposures etc.

**Copy**  
Create an exact copy of the current instruction set and add it below the current instruction

**Move up**  
Moves the instruction one row above. If it is already the first instruction of an instruction set, it will move to the parent instruction set above the current instruction set instead.
If the previous instruction is an instruction set that is not collapsed, the instruction will move to the bottom of that instruction set

**Move down**  
Moves the instruction one row below. If it is already the last instruction of an instruction set, it will move to the parent instruction set below the current instruction set instead.
If the next instruction is an instruction set that is not collapsed, the instruction will move to the top of that instruction set

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
*Requires a camera that has a controllable dew heater*

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

### Close Dome Shutter

### Enable Dome Sync

### Open Dome Shutter

### Park Dome

## Filter Wheel

### Switch Filter

## Flat Device


### Close Flat Panel Cover

### Open Flat Panel Cover

### Set Brightness

### Toggle Light

### Trained Flat Exposure

### Trained Dark Flat Exposure

## Focuser

### Move Focuser

### Move Focuser By Temp.

### Move Focuser Relative

### Run Autofocus

## Guider

### Dither

### Start Guiding

### Stop Guiding

## Rotator

### Rotate By Mechanical Angle

## Safety Monitor

### Wait Until Safe

## Switch

### Set Switch Value

## Telescope

### Find Home

### Park Scope

### Set Tracking

### Slew And Center

### Slew To Alt/Az

### Slew To Ra/Dec

### Slew, Center And Rotate

### Solve And Sync

### Unpark Scope

## Utility

### Annotation

### External Script

### Message Box

### Wait For Altitude

### Wait For Time

### Wait For Time Span

### Wait If Moon Altitude

### Wait If Sun Altitude

### Wait Until Above Horizon