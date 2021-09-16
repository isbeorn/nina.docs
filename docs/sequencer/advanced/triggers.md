Triggers are instructions that should only happen when certain events occur. These triggers can be attached to an instruction set. When attached, they will get evaluated after each instruction inside the set, similar like loop conditions are evaluated. When the defined event occurred for the trigger to fire, the trigger will execute its instruction.  
Triggers can be identified by the highlighted lightning icon next to them in the sequencer sidebar.  
![Triggers](../../images/sequencer/trigger/trigger.png)  

## Dome
Trigger actions for a dome. Each trigger in this category requires at least a dome to be connected.

### Synchronize Dome
*Requires dome following to be disabled*

## Focuser
Trigger actions for a focuser. Each trigger in this category requires at least a focuser to be connected.

### AF After # Exposures

### AF After Filter Change

### AF After HFR Increase

### AF After Temperature Change

### AF After Time

## Guider
Trigger actions for a guider. Each trigger in this category requires at least a guider to be connected.

### Dither After Exposures
![Restore Guiding](../../images/sequencer/trigger/ditherafterexposures.png)  
Using this trigger will initiate a dither operation after the set amount of exposures. For more information about dithering, visit the [dedicated page](../../advanced/dithering.md) about it.

### Restore Guiding
![Restore Guiding](../../images/sequencer/trigger/restoreguiding.png)  
This trigger will start guiding each time after an instruction inside its context. When guiding is already started, no action will be taken. Using this trigger makes sure that the guiding software reacquires a guide star after some failures, like clouds.  
This trigger is best used in combination with the "Center After Drift" trigger to guard against interruption from clouds and thus drifting off target.

## Telescope
Trigger actions for a telescope. Each trigger in this category requires at least a telescope to be connected.

### Center After Drift
![Center After Drift](../../images/sequencer/trigger/centerafterdrift.png)  
After the set amount of exposures, this trigger will plate solve the saved image in the background. When the distance of the solved coordinates are above the specified amount of arcminutes compared to the current target coordinates, this trigger will initiate a recenter operation.  
*Requires a plate solver to be set up and the trigger needs to be inside a deep sky object sequence to have a target reference*

### Meridian Flip
When the telescope passes the meridian according to the meridian flip settings in the [options](../../tabs/options/imaging.md), this trigger will initiate the meridian flip.  
More information on the settings and how the flip works is available on the [meridian flip page](../../advanced/meridianflip.md)
