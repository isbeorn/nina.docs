Conditions are special instructions that can be put into instructions sets into the loop condition area to loop instruction sets for as long as these conditions are fullfilled. When at least one condition inside an instruction set is not fullfilled anymore, the rest of the instruction set is skipped and not looped anymore.    

Conditions can be identified by the highlighted icon next to them in the sequencer sidebar.  
![Loop Conditions](../../images/sequencer/conditions/conditions.png)  

### Loop For Iterations
![Loop For Iterations](../../images/sequencer/conditions/loopforiterations.png)  
Loop the instruction set for the specified amount of iterations.

### Loop For Time Span
![Loop For Time Span](../../images/sequencer/conditions/loopfortimespan.png)  
Loop the instruction set for the specified amount of seconds.

### Loop Until Altitude Sets Below
![Loop Until Altitude Sets Below](../../images/sequencer/conditions/loopuntilaltitude2.png)  
For a given target coordinates the condition will loop until the altitude sets below the specified amount.
When this condition is part of a "Deep Sky Object Sequence" the coordinates will be inherited by this set and no coordinates need to be entered  
![Loop Until Altitude Sets Below](../../images/sequencer/conditions/loopuntilaltitude.png)  

### Loop Until Time
![Loop Until Time](../../images/sequencer/conditions/loopuntiltime.png)  
Loop an instruction set until a specific point in time. The time can either be set manually or automatically determined based on criteria and an offset specified in minutes.  
**Time**: Manually entered time  
**Sunset**:  The time when the sun gets below 6° of the horizon  
**Nautical Dusk**: The time when the sun gets below 12° of the horizon  
**Astronomical Dusk**: The time when the sun gets below 18° of the horizon  
**Astronomical Dawn**: The time when the sun gets above 18° of the horizon  
**Nautical Dawn**: The time when the sun gets above 12° of the horizon  
**Sunrise**: The time when the sun gets above 6° of the horizon  
**Meridian**: When a target is set this will be the time the target will cross the meridian  

### Loop While Altitude Above Horizon
![Loop While Altitude Above Horizon](../../images/sequencer/conditions/loopwhilehorizon2.png)  
This will loop the instruction set for as long as the specified target is above the horizon. When a [custom horizon](../../tabs/options/general.md) is set, the custom horizon will be considered as the altitude to be above. When no custom horizon is set, 0° of altitude will be considered. Furthermore an altitude offset can be specified.  
When this condition is part of a "Deep Sky Object Sequence" the coordinates will be inherited by this set and no coordinates need to be entered  
![Loop While Altitude Above Horizon](../../images/sequencer/conditions/loopwhilehorizon.png)  

### Loop While Safe
![Loop While Safe](../../images/sequencer/conditions/loopwhilesafe.png)  
Loop for as long as the safety monitor is reporting safe conditions. When the state of the safety monitor switches to unsafe, the currently running instruction will be cancelled and the rest of the instruction set will be skipped.  
It is recommended to use this condition in conjunction with another condition, to not run in an endless loop when the safety monitor is reporting safe conditions for the whole time.  
*Requires a safety monitor device to be connected*

### Loop While Unsafe
![Loop While Unsafe](../../images/sequencer/conditions/loopwhileunsafe.png)  
Loop for as long as the safety monitor is reporting unsafe conditions. When the state of the safety monitor switches to safe, the currently running instruction will be cancelled and the rest of the instruction set will be skipped.  
It is recommended to use this condition in conjunction with another condition, to not run in an endless loop when the safety monitor is reporting unsafe conditions for the whole time.  
*Requires a safety monitor device to be connected*
### Moon Altitude
![Moon Altitude](../../images/sequencer/conditions/moonaltitude.png)  
Loop while the moon altitude is above or below the specified amount of degrees

### Moon Illumination
![Moon Illumination](../../images/sequencer/conditions/moonillumination.png)  
Loop while the moon illunination is above or below the specified amount of percentage

### Sun Altitude
![Sun Altitude](../../images/sequencer/conditions/sunaltitude.png)  
Loop while the sun altitude is above or below the specified amount of degrees