This is the tab where you set up all the parameters recmd-k vlated to your equipment.  

![Equipment Settings](../../images/tabs/equipmentSettings1.png)

## Camera

1. **Pixel Size**
    * The Pixel Size of your camera sensor in micrometers. This field will be automatically populated by the camera, if it provides the information.
    > This field together with the values in  “Telescope”  is used for Platesolving operations

2. **Camera read noise in e**
    * The read noise in electrons of your camera device
    > Read noise charts for different Gain values are usually provided by camera manufacturers. Consult your camera manual/website or contact your vendor.   
    For DSLR refer to sensorgen.info or ccdparameters script in PixInsight

3. **BIAS mean (native)**
    * •	The mean value in ADU of your BIAS (shortest exposure) at your cameras native bit depth (4)
    > Select the appropriate RAW bit depth for DSLR.  
    For ZWO and QHY cameras select 16bit since they are rescaled by the by the camera drivers.  
    For Aitik and Touptek cameras select the native ADC bit depth.  
    For other CCD/CMOS cameras ask to your camera manufacturer.

4. **BIAS mean (native)**
    * The mean value in ADU of your BIAS (shortest exposure) at your cameras native bit depth (4)
    > This value changes for different Gain/Offset.   
     To evaluate the mean ADU you can follow these simple steps: 
     > - set the same Gain/Offset that will be used durign your imaging session  
     > - go to [Imaging](../../tabs/imaging.md) tab, under "Imaging" window  set an exposure of 0.001s
     > - take an exposure  
     > - the Mean value will be displayed in the "Statistics" window   
     ![settingswindow](../../images/tabs/settingswindow.png)

5. **Full Well Capacity in e**
    * The Full Well Capacity in electrons 
    > Full well charts for different Gain values are usually provided by camera manufacturers. Consult your camera manual/website or contact your vendor.  

6.	**Download to Data ratio**
      * The ratio the optimal exposure calculator will utilize to calculate the maximum adequately possible exposures. Default will work in most cases.

7. **Bulb Mode**
   * Allows you to change the bulb mode of the camera. Native will work in most cases.
	> RS232 and Mount is available as well and might be necessary for older Nikon cameras
	> For usage of RS232 and Monunt shutter refer to Usage: [Using RS232 or Mount for bulb shutter](https://nighttime-imaging.eu/docs/documentation/tabs/settings/#using-rs232-or-mount-for-bulb-shutter)

8. **Raw Converter**
    * Only for DSLR: select the RAW converter, optriona are DCRaw and FreeImage
    > DCRaw will utilize DCRaw and stretch your images to 16bit, applying the cameras specific color bias profile.  
    FreeImage will deliver the frame exactly as your camera provided it and can be slightly faster for image download on slower machines.

!!! note
    both raw converters will deliver you the raw frame of your DSLR. but they might vary in color. Saving the raw frame without adding the camera specific profile with FreeImage can deliver more faint and less colorful raw images than you are used to.

## Telescope
9. **Telescope**
* This section lets you enter the parameters relative to your telescope that will be used for [Platesolving](../../advanced/platesolving.md).  
> If you change telescope, remember to update these settings or to switch profile under [Options/General](../../tabs/options/general.md).

## Focuser

10. **Use FilterWheel offset**
    * Determines whether the focuser should move per the defined offset when the filter wheel changes filter

11. **Auto Focus Step Size**
    *  the number of focuser steps that the autofocus routine will move by between autofocus points
  
12. **Auto Focus Initial Offset Steps**
    * The number of focus points that will be used on each side of perfect focus by the autofocus routine

13. **Default Auto Focus Exposure Time**
    * The exposure time in seconds that will be used by autofocus, if filter times are not set

14. **Focuser Settle Time**
    * The amount of time, in seconds, that should be awaited after a focuser move before starting a new exposure

15. **AF Number of Attempts**
    * The number of attempts the autofocus routine should be retried in case of unsuccessful focusing

16. **AF Number of Frames per Point**
    * The number of frames whose HFR or contrast will be averaged per focus points

17. **AF Crop Ratio**
    * Ratio  that will determine a centered region of interest for autofocus
  
18. **Use Brightest n Stars**
    * The number of top brightest stars that the autofocus routine will use - 0 means there is no limit

19. **Backlash IN/OUT**
    * The focuser backlash in the IN (decreasing position) and OUT (increasing position) directions, expressed in focuser steps. A tool described in the Focuser Backlash Measurement Section is available to measure it [Imaging](../imaging.md) tab Focuser window.
  
20. **Binning**
    * The binning to be used for Autofocus exposures. 

## Weather
  This section is used to enter the OpenWeatherMap API Key to retrieve real time weather data in [Imaging](../imaging.md) tab Weather window.

  21. **Fahrenheit Temperatures**
    * Switch ON to use Fahrenheit temperature scale

22. **Imperial Units**
    * Switch ON to use Imperial Units

23. **OpenWeatherMap API Key**
    * Input your personal OpenWeather API key.
    > You can get your OpenWeather free API key [here](https://openweathermap.org/api)

## Filterwheel

24. **Filterwheel**
    * If a Filter Wheel is connected in [Equipment](../equipment.md) this window lists the available filters and names.
    > Filters can be added manually  (25) or imported from ASCOM filter wheel drivers (26)

## Guider Settings
This section is used to connect N.I.N.A. with PHD2 and define Dithering parameters

27. **PHD2 Path**
    * PHD2 installation path

28. **PHD2 Server URL and Port**
    * You can set the PHD2 server settings here
    > Usually the defaults should work fine. You need to enable PHD2 server in PHD2.

29. **PHD2 Dither Pixels and Dither RA Only**
    * The amount of guide camera pixels to dither in PHD2. If "Dither RA only" is checked, the dither movements will only be performed in RA. 

30. **PHD2 Settle Pixel Tolerance**
    * The threshold expressed in guide camera pixels that will determine a dither settling completion after a dither move.
    > A dither  will be considered settled if, after the "Minimum Settle Time" and before the "PHD2 Settle Timeout", the guide movements in PHD2  will be below the "PHD2 Settle Pixel Tolerance".

31. **Minimum Settle Time**
    * The minimum time N.I.N.A. should wait after a dithering process until it starts the next capture

32. **PHD2 Settle Timeout**
    * The maximum time N.I.N.A. should wait after a dithering process until it starts the next capture. After this time N.I.N.A. will start a new capture regardless of dithering settling completion.

33. **Direct Guide Duration**
    * Duration of guide when Direct Guide is selected
  
!!!tip
    Refer to [Dithering](../../advanced/dithering.md) in Advanced documentation topics for more information about Dithering and how to set the above parameters
