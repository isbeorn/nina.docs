Triggers are instructions that should only happen when certain events occur. These triggers can be attached to an instruction set. When attached, they will get evaluated after each instruction inside the set, similar to how loop conditions are evaluated. When the defined event occurs, the trigger will execute its instruction. After the trigger has finished its execution, the sequence will continue where it left off.
Triggers can be identified by the highlighted lightning icon next to them in the sequencer sidebar.  
![Triggers](../../images/sequencer/trigger/trigger.png)  

## Dome
Trigger actions for a dome. Each trigger in this category requires at least a dome to be connected.

### Synchronize Dome
![Synchronize Dome](../../images/sequencer/trigger/syncdome.png)  
N.I.N.A. has the capability to automatically synchronize the dome with the telescope pointing direction. However, in some scenarios, for example with a heavy dome, the vibrations of movement can affect imaging quality. Therefore this trigger exists to only synchronize the dome between instructions, instead of having it constantly adjusted. This will also prevent dome movement during exposures.
*Requires dome following to be disabled*

## Focuser
Trigger actions for a focuser. Each trigger in this category requires at least a focuser to be connected.

### AF After # Exposures
![AF After # Exposures](../../images/sequencer/trigger/afafterexposures.png)  
A trigger to simply run an autofocus after a set amount of exposures. As the amount of exposures is an arbitrary metric, this trigger is not recommended.

### AF After Filter Change
![AF After Filter Change](../../images/sequencer/trigger/afafterfilter.png)  
When the filter wheel changes its filters and no filter offsets have been calculated to automatically adjust the focuser position for the change of focus due to the filter shift, this trigger can help to mitigate the problem by running an autofocus run when a filter changes during imaging.

### AF After HFR Increase
![AF After HFR Increase](../../images/sequencer/trigger/afafterhfr.png)  
This trigger will monitor the history of images taken during the sequence. It will take all exposures up until the last autofocus (or all if no autofocus has happened yet) and filter them for the currently active filter. The first point after the autofocus will be taken as a reference point as well as the last n points, where n is the specified sample size. Out of the last n points, the trend will be determined and compared to the reference point. If the trend is above the specified percentage, an autofocus will be triggered.
This is a decent generic trigger if you don't know about the temperature sensitivity of your equipment, but it requires that your autofocus results are consistent and that the seeing is not bad.

### AF After Temperature Change
![AF After Temperature Change](../../images/sequencer/trigger/afaftertemp.png)  
When the temperature rises or falls, most equipment will slightly shift its focus. When you roughly know at which temperature your equipment shifts focus enough to be out of the critical focus zone, this trigger can help you automatically run the autofocus routine when a specified amount of temperature drift has happened.
*Requires a focuser with a temperature probe*

### AF After Time
![AF After Time](../../images/sequencer/trigger/afaftertime.png)  
A trigger to simply run an autofocus after a set amount of time. As the amount of time is an arbitrary metric, this trigger is not recommended.

## Guider
Trigger actions for a guider. Each trigger in this category requires at least a guider to be connected.

### Dither After Exposures
![Dither After Exposures](../../images/sequencer/trigger/ditherafterexposures.png)  
Using this trigger will initiate a dither operation after the set amount of exposures. For more information about dithering, visit the [dedicated page](../../advanced/dithering.md) about it.

### Restore Guiding
![Restore Guiding](../../images/sequencer/trigger/restoreguiding.png)  
This trigger will start guiding each time after an instruction inside its context. When guiding is already started, no action will be taken. Using this trigger makes sure that the guiding software reacquires a guide star after some failures, like clouds.  
This trigger is best used in combination with the "Center After Drift" trigger to guard against interruption from clouds and thus drifting off target.

## Safety Monitor
Trigger actions for a [safety monitor](../../tabs/equipment/safetymonitor.md). Each trigger in this category requires at least a safety monitor to be connected.

### Trigger On Unsafe
![Trigger On Unsafe](../../images/sequencer/trigger/triggeronunsafe.png)

Runs configured instructions when the safety monitor reports unsafe conditions or disconnects after it has been connected. The trigger first runs the instructions in **Before Waiting For Safety**, then pauses with [Wait Until Safe](instructions.md#wait-until-safe), and finally runs **After Waiting For Safety** once the monitor reports safe conditions again.
If the safety monitor disconnects after N.I.N.A. has already seen a valid connection, that disconnect is treated like an unsafe state and the trigger will run. A monitor that has never been connected yet will not trigger this behavior just because it is currently disconnected.
If this trigger fires while N.I.N.A. is working on a Deep Sky Object target, instructions such as slew or center will automatically use that target's coordinates. That also applies when the trigger itself is placed higher up, for example in **Global Triggers**.

## Telescope
Trigger actions for a telescope. Each trigger in this category requires at least a telescope to be connected.

### Center After Drift
![Center After Drift](../../images/sequencer/trigger/centerafterdrift.png)  
After the set amount of exposures, this trigger will plate solve the saved image in the background. When the distance of the solved coordinates is above the specified amount of arcminutes compared to the current target coordinates, this trigger will initiate a recenter operation.
*Requires a plate solver to be set up and the trigger needs to be inside a deep sky object sequence to have a target reference*

### Meridian Flip
![Meridian Flip](../../images/sequencer/trigger/meridianflip.png)  
When the telescope passes the meridian according to the meridian flip settings in the [options](../../tabs/options/imaging.md), this trigger will initiate the meridian flip.  
More information on the settings and how the flip works is available on the [meridian flip page](../../advanced/meridianflip.md).

## Utility
Trigger actions that let you reuse existing trigger timing with your own instruction set.

### Custom Trigger
![Custom Trigger](../../images/sequencer/trigger/customtrigger.png)

Use an existing trigger as the trigger source, then add your own instructions to run when that source would normally fire. This is useful when a built-in trigger has the timing you need, but you want custom actions instead of the built-in behavior.
If this trigger fires while N.I.N.A. is working on a Deep Sky Object target, instructions such as slew or center will automatically use that target's coordinates. That also applies when the trigger itself is placed higher up, for example in **Global Triggers**.
Not every trigger source is guaranteed to be a good fit for **Custom Trigger**. Some triggers may depend on their original built-in behavior or special runtime handling, so if a wrapped trigger does not behave as expected it may need to be used directly instead.
*Requires a trigger source and at least one instruction*
