## Overview

Focusers can suffer from backlash when they reverse directions, which is the number of focuser steps that "slip by" before the focuser physically moves again during a change in direction.

A typical effect of backlash on the AF curve is shown in the image below. In the first part of the curve (from right to left), the HFR remains constant due to the focuser drawtube not moving while the focuser is only compensating for backlash. In this case the curve shows approximately 150 steps of backlash.

![focuserbacklash](../images/advanced/focuserbacklash.png)

A good AF curve should not display any signs of backlash and look similar to the one below:

![good AF](../images/advanced/goodAF.PNG)

Several methods can be used to measure the focuser backlash, and most of them involve a mechanical measurement of the focuser drawtube movements.
For example, by using a dial gauge to measure focuser drawtube movements, backlash is determined by changing focuser movement direction and measuring how many steps are necessary before the drawtube moves again.  
![dialgauge](../images/advanced/dialgauge.PNG)

If the drawtube has a marked scale that can also be used in the same fashion as above. 
Another method involves running a standard AF routine and determining the number of backlash steps as shown in the example above.


N.I.N.A. offers two [backlash compensation methods](autofocus.md): __Absolute__ and __Overshoot__.

When __Absolute__ backlash compensation is used, N.I.N.A. will add a fixed amount of steps (as specified in [Focuser Advanced options](autofocus.md)) when the focuser changes directions. This requires a good backlash measurement and is mostly effective with focusers with small backlash relative to the _Auto Focus Step Size_.

With __Overshoot__, N.I.N.A. compensates for backlash by overshooting the target position by a large amount and then moving the focuser back to the initially requested position. This method is much more forgiving than Absolute and is recommended for focusers with large backlash or when the backlash measurement is not very accurate.
For __Overshoot__, once the user has determined a rough backlash value, this can be increased by an extra 50% and input as IN or OUT compensation. Since this method is very forgiving, a trial-and-error procedure is also possible, by using progressively larger compensation values until the AF routine behaves properly and no sign of backlash is shown in the AF curve.
  
!!! tip 
    Overshoot can be very useful for SCT users to avoid mirror flop. In fact, when setting the Backlash Compensation to _IN_, the last focuser movement will always be outwards.
