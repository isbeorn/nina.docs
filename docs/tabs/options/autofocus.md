# ToDo - This page is under contruction

10. **Use FilterWheel offset**
    * Determines whether the focuser should move per the defined offset when the filter wheel changes filter

11. **Auto Focus Step Size**
    *  the number of focuser steps that the autofocus routine will move by between autofocus points
  
12. **Auto Focus Initial Offset Steps**
    * The number of focus points that will be used on each side of perfect focus by the autofocus routine

13. **Default Auto Focus Exposure Time**
    * The exposure time in seconds that will be used by autofocus, if filter times are not set

14. **AF Method**
    * Method used to detect datapoints for auto focus

15. **AF disable Guiding**
    * Activate to pause guiding during AF routine (recommended when guiding with OAG)

16. **AF Curve Fitting**
    * Fitting that should be used to determine ideal focus position out of the measured data points

17. **Focuser Settle Time**
    * The amount of time, in seconds, that should be awaited after a focuser move before starting a new exposure
  
18. **AF Number of Attempts**
    * The number of attempts the autofocus routine should be retried in case of unsuccessful focusing

19. **AF Number of Frames per Point**
    * The number of frames whose HFR or contrast will be averaged per focus points
  
20. **Use brightest n stars**
    * The number of top brightest stars that the autofocus routine will use - 0 means there is no limit

21. **AF Inner Crop Ratio**
    * Inner ratio that will determine a centered region of interest for autofocus

22.  **AF Outer Crop Ratio**
    * Outer ratio that will determine a centered region of interest for autofocus
  
23. **Backlash Compensation Method**
    * This controls the backlash compensation method used. The method can only be changed when the focuser is not connected!
      * Absolute: 
  When the focuser changes directions, an absolute value will be added to the focuser movement.
  Backlash IN: when the focuser changes from moving outwards to moving inwards the Backlash IN value will be added
  Backlash OUT: when the focuser changes from moving inwards to moving outwards the Backlash OUT value will be added
      * Overshoot:
  This method will compensate for Backlash by overshooting the target position by a large amount and then moving the focuser back to the initially requested position.
  Due to this compensation the last movement of the focuser will always be in the same direction (either always inwards or always outwards)
  
24. **Backlash IN/OUT**
      * The focuser backlash in the IN (decreasing position) and OUT (increasing position) directions, expressed in focuser steps. 
  
        > When Overshoot is chosen, only ONE between Backlash IN and OUT must be set! When setting IN, the amount will be applied on each inward movement, so the final movement will always be outwards. For Backlash OUT, it will be the other way around

25. **Binning**
    * The binning to be used for Autofocus exposures.



    **Changing filter information**

        It is possible to double click within the table to change the name of the filter (used throughout N.I.N.A.), its focuser offset and its Auto Focus Exposure Time directly.

        **Filter offsets**

        Most filters are not exactly par focal, meaning that when changing filters, the ideal focus distance changes slightly. This will cause an imaging system that was in perfect focus with one filter to be slightly out of focus with another filter. This can be a big problem for precise imaging, requiring an additional autofocus run each time the filter is changed.

        To avoid this, it is possible to set filter offsets, which are the amount of focuser steps that the focuser should move by when switching from one filter to another.

        For example, I could run the autofocus routine on each of my filters one after the other (with hopefully very little temperature change in between), with the following results:

      * L filter achieves perfect focus at focuser position 5000
      * R filter achieves perfect focus at focuser position 4990 (10 steps fewer than L filter)
      * G filter achieves perfect focus at focuser position 5030 (30 steps more than L filter)
      * B filter achieves perfect focus at focuser position 5045 (45 steps more than L filter)
      * Ha filter achieves perfect focus at focuser position 4988 (12 steps fewer than L filter)

        If we take the L filter as the reference filter, we can set up all the filter offsets relative to the L filter, as below:

      * L filter offset 0 (reference filter)
      * R filter offset -10 (10 steps fewer than L)
      * G filter offset 30 (30 steps more than L)
      * B filter offset 45 (45 more steps than L)
      * HA filter offset -12 (12 steps fewer than L)

        This is what has been done in the above screenshot.

        Note that for this to work, the *Use FilterWheel Offsets* parameter under the Focuser Options needs to be set to On.

        **Auto Focus Exposure Time**

        The ideal auto-focus time can change per filter, particularly between broadband and narrowband filters (in the above example, the narrowband filter requires an exposure time 5 times longer than the broadband filters). This can easily be set up here.

        Finding a good exposure time for autofocus is further explained in the [Auto-Focus section](../../advanced/autofocus.md)

        **Auto Focus Filter**

        From this screen, it is possible to set (or unset) an autofocus filter, which will be used by the autofocus routine (if the *Use FilterWheel Offsets* setting under *Focuser Settings* is set to On). This can be done by simply selecting a filter in the list, and clicking on the *Set as Default AF Filter* button. The same button can be used to unset the Auto-Focus Filter.