The Flat Wizard offers the possibility to automate flat image capture. It takes multiple exposures until it finds a optimal exposure time for the settings specified. There is also a [Multi Mode](flatwizard.md#multi-mode) that automates flat acquistion for each filter for users with electronic filter wheels.

Usually the flat wizard will take 3 exposures and try to calculate the optimal exposure time via linear extrapolation. Should this still not be enough it will continue refining the value.

![The Flatwizard tab](../images/tabs/flatwizard1.png)

## Single Mode

1. **Image Preview**
    * Displays the latest captured flat while determining the flat exposure time
    > Note this will not change anymore once the exposure time is determined and the flats are downloading to speed up the process.

2. **Flats & Dark flats to Take**
    * The amount of flats and dark flats the wizard should capture
    
3. **Binning**
    * Sets binning for the exposures

4. **Gain**
    * Sets the gain of the camera to use for the exposures
    > The camera and driver needs to support gain control
    
5. **Zenith Slew**
    * Slews scope to point at zenith on either east or west side

6. **Single Mode**
    * The mode to take flats for a single filter or no filter at all

7. **Multi Mode**
    * The mode to take flats for multiple filters
    
8. **Filter**
    * If a filter wheel is connected, a filter can be chosen for single mode

9. **Flat Min Exposure**
    * The minimum exposure time for the flats the flat wizard should use and start with
    > To use the slider, drag and hold it. This allows for precise small movements as well as large movements (it accelerates over time)
    
10. **Flat Max Exposure**
    * The maximum exposure time for the flats the flat wizard should use
    > Uses the same style slider used for (9)
    
11. **Flat Step Size**
    * This sets the step size in seconds that the flat wizard should use between two flats to determine the necessary exposure time

12. **Histogram Mean Target**
    * Sets the mean ADU that the flat image histogram should have.
    * A percentage can be specified on the right or with using the slider. The number left of the percentage displays the ADU value of the desired percentage

13. **Mean Tolerance**
    * Determines how large the tolerance of the flat mean from the mean target (12) should be
    * A percentage can be specified on the right or with using the slider. The number left of the percentage displays the ADU range of the desired tolerance percentage based on the mean target (12)
    > A tolerance value of 20-30% should typically be fine

14. **Start Flat Wizard**
    * This button will start the flat wizard with the settings as set above
    * First it will calculate the necessary exposure time and then take flat exposures as set in (2)

15. **Calculated Target Exposure Time**
    * Once the flat wizard determines the necessary exposure time, it will use that exposure time to take flats

16. **Calculated Target Histogram Mean**
    * Once the flat wizard determines the necessary exposure time and resulting ADU, it will display the ADU mean here
    
## Multi Mode

![The Flatwizard multi mode menu](../images/tabs/flatwizard2.png)

In essence the Multi Mode works the same as the Single Mode but for multiple filters. Majority of the controls are identical to [Single Mode](flatwizard.md#single-mode).

Flat Wizard settings are saved per filter in multi mode and do not transfer to single mode.

1. **Filter Toggle**
    * This toggle will enable this specific filter for flat capture

2. **Filter List**
    * Displays all of available filters by name and can be expanded by clicking on the > icon

## Error Handling

![The Flatwizard error window](../images/tabs/flatwizard3.png)

If the flat wizard cannot determine the necessary exposure time or it conflicts with the spcecified settings it will display this dialog.

1. **Error message**
    * The flat wizard will display what the issue with the current configuration is and what should be done to fix the issue

2. **Current Exposure Calculations**
    * Here the flat wizard will display current calculated metrics, as the current mean, the maximum bit depth of the camera in ADU and the estimated exposure time
    * Use those to adjust the values in (3)

3. **Flat Wizard Settings**
    * Specify settings for the current flat capture to get successful flats
    * For further detail, see the description of the settings in the [Single Mode](flatwizard.md#single-mode) section

4. **Reset and Continue**
    * Pressing this button will lead to the flat wizard re-starting from the Flat Min Exposure as set in the settings (3)
    
5. **Continue**
    * This button will continue the flat capture with the new settings (3)

6. **Cancel Flat Wizard**
    * This button will completely abort the flat wizard sequence