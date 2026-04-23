Loop conditions will drive the behavior of an instruction set. Without a condition, an instruction set will just process each sequence item inside once and is finished. This behavior will be changed, when loop conditions are attached. When an instruction set has a loop condition attached, it will process its items and loop itself again as long as the attached loop conditions are fulfilled. Once at least one of these loop conditions is not fulfilled anymore (e.g. a condition to loop until a specific time and the time has passed) the current instruction will be finished and afterwards the rest of the instructions inside this set will be skipped as well as the instruction set marked as finished. Conditions will be evaluated constantly in the background, so things like time based conditions or safety monitor conditions will interrupt ongoing instructions once they are no longer fulfilled.    

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
Loops an instruction set until a specific local time or astronomy-based event. The source can be a manually entered time, a sun event, or the current target's meridian crossing. For calculated sources, the time fields are populated automatically and can be shifted earlier or later by setting an offset in minutes.

This condition remains true while there is enough time left to run the next instruction. If the selected time has already passed for the current observing day, or if the next instruction's estimated duration would exceed the selected time, the condition becomes false and the instruction set stops.

* **Time**: Manually entered local time in `hh:mm:ss`
* **Sunset**: The time when the sun goes below 0 degrees altitude
* **Civil Dusk**: The time when the sun goes below -6 degrees altitude
* **Nautical Dusk**: The time when the sun goes below -12 degrees altitude
* **Astronomical Dusk**: The time when the sun goes below -18 degrees altitude
* **Astronomical Dawn**: The time when the sun rises above -18 degrees altitude
* **Nautical Dawn**: The time when the sun rises above -12 degrees altitude
* **Civil Dawn**: The time when the sun rises above -6 degrees altitude
* **Sunrise**: The time when the sun rises above 0 degrees altitude
* **Meridian**: The time when the current target crosses the meridian. If no target coordinates are available, this resolves to the current time.

| Time source         | Rollover time |
|---------------------|---------------|
| Time                | Sunrise, or noon if sunrise is unavailable |
| Sunset              | Sunrise, or noon if sunrise is unavailable |
| Civil Dusk          | Sunrise, or noon if sunrise is unavailable |
| Nautical Dusk       | Sunrise, or noon if sunrise is unavailable |
| Astronomical Dusk   | Sunrise, or noon if sunrise is unavailable |
| Astronomical Dawn   | Sunset, or noon if sunset is unavailable |
| Nautical Dawn       | Sunset, or noon if sunset is unavailable |
| Civil Dawn          | Sunset, or noon if sunset is unavailable |
| Sunrise             | Sunset, or noon if sunset is unavailable |
| Meridian            | Meridian + 12 hours |

!!! note
    `Loop Until Time` has no date field, so N.I.N.A. uses the rollover time to decide whether the selected time belongs to the current observing day or the next one. The rollover time shown in the condition is the value currently being used.

    Examples, assuming sunrise is at 09:00:

    * Current time: 18:00 | Loop until time: 19:00 -> loops for one hour
    * Current time: 20:00 | Loop until time: 19:00 -> condition is false because 19:00 has already passed
    * Current time: 18:00 | Loop until time: 02:00 -> loops for eight hours
    * Current time: 02:00 | Loop until time: 03:00 -> loops for one hour
    * Current time: 04:00 | Loop until time: 03:00 -> condition is false because 03:00 has already passed
    * Current time: 08:00 | Loop until time: 18:00 -> condition is false because the 09:00 rollover has not happened yet, so 18:00 still belongs to the previous observing day

!!! note
    If a calculated source such as sunset, astronomical dusk, or astronomical dawn is unavailable for the current location and date, N.I.N.A. marks the condition as invalid instead of using the current time.



### Loop While Altitude Above Horizon
![Loop While Altitude Above Horizon](../../images/sequencer/conditions/loopwhilehorizon2.png)  
This will loop the instruction set for as long as the specified target is above the horizon. When a [custom horizon](../../tabs/options/general.md) is set, the custom horizon will be considered as the altitude to be above of. When no custom horizon is set, 0° of altitude will be considered. Furthermore an altitude offset can be specified.  
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

### Loop While (NINA 3.3)
![Sun Altitude](../../images/sequencer/conditions/loopwhile.png)  
Loop while the Expression is True

### Moon Altitude
![Moon Altitude](../../images/sequencer/conditions/moonaltitude.png)  
Loop for as long as the moon matches the specified parameters.

### Moon Illumination
![Moon Illumination](../../images/sequencer/conditions/moonillumination.png)  
Loop for as long as the sun matches the specified parameters.

### Sun Altitude
![Sun Altitude](../../images/sequencer/conditions/sunaltitude.png)  
Loop while the sun altitude is above or below the specified amount of degrees


