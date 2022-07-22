The Flat Wizard offers the possibility to automate flat image capture. It takes multiple exposures until it finds an optimal exposure time for the settings specified. There is also [Multi Mode](flatwizard.md#multi-mode). Multi Mode automates flat acquisition for each filter for users with electronic filter wheels.

Flat Wizard takes 3 test exposures and attempts to calculate the optimal exposure time for a flat image by using linear extrapolation. Should that not be sufficient to derive a suitable exposure time, it will continue to take test exposures until it can determine the optimal exposure time or will ask to adjust the parameters when it fails to find one.

![The Flatwizard tab](../images/tabs/flatwizard1.png)

## Settings

### Target Name
When using the image file pattern $$TARGETNAME$$ this field can be used to control how the pattern will be filled for the FLAT and DARKFLAT frames

### Flats to take
The number of flats to take for each filter

### Dark flats to take
The number of dark flats to take for each exposure time

### Binning
Which binning mode should be used for taking the flats

### Gain
Determines the camera gain to be used during flat exposure

### Slew To Zenith
Depending on the selected value inside the combobox next to the button the telescope will slew to zenith with pier side east or west

### Wizard Mode
This mode will switch the operational behavior for the flat wizard for different scenarios

- Dynamic Exposure
    - In this mode the flat wizard will try to find an exposure time (for a fixed panel brightness if a flat panel is connected)
- Dynamic Brightness
    - For a fixed exposure time the flat wizard will try to find a flat panel brightness to match the desired exposure time. This requires a controllable flat panel to be connected.
- Sky Flats
    - A mode when no flat panel is available, but rather the sky at dusk or dawn should be used to take flat exposures. During the runtime of the flat wizard the exposure time will be constantly adjusted for the changing sky brightnes, so the exposures will all have a different exposure time

## Single Mode

### Image Preview
-  The most recent flat image is displayed in this area while determining the optimal flat exposure time. Note that this area will not be updated once the optimal exposure time is determined. This is to speed up the flat acquisition process.

### Flats & Dark Flats to take
-  The number of flats and dark flats the wizard should capture

### Binning
-  Sets camera binning level for the exposures

### Gain
-  Sets the gain of the camera to use for the exposures. The camera and driver needs to support gain control

### Zenith Slew
-  Slews scope to point at zenith on either east or west side

### Single Mode
-  The mode to take flats for a single filter or no filter at all

### Multi Mode
-  The mode to take flats for multiple filters

### Filter
-  If a filter wheel is connected, a filter can be chosen for single mode

### Flat Min Exposure
-  The minimum exposure time Flat Wizard should use

### Flat Max Exposure
-  The maximum exposure time Flat Wizard should use

### Flat Step Size
-  Sets the step size (in seconds) that Flat Wizard should use when comparing test flats while determining the optimal exposure time

### Histogram Mean Target
-  Sets the mean ADU that the flat image histogram should use.
-  A percentage can be specified on the right or with using the slider. The number left of the percentage displays the ADU value of the desired percentage

### Mean Tolerance
-  Determines how large the tolerance of the flat mean from the mean target (12) should be
-  A percentage can be specified on the right or with using the slider. The number left of the percentage displays the ADU range of the desired tolerance percentage based on the mean target (12). A tolerance value of 20-30% should be typical

### Start Flat Wizard
-  This button starts the flat acquisition process using the current settings
-  First, Flat Wizard will calculate the optimal exposure time using test exposures, and then take flat the full course of flats as set in (2). You will be prompted to extinguish any light source prior to taking Dark Flats.

### Calculated Target Exposure Time
-  Once Flat Wizard determines the necessary exposure time, it will use that to take all flats

### Calculated Target Histogram Mean
-  Once Flat Wizard determines the necessary exposure time and resulting ADU, the ADU mean will be displayed here

## Multi Mode

![The Flatwizard multi mode menu](../images/tabs/flatwizard2.png)

In essence, Multi Mode works just like Single Mode, but for multiple filters. The majority of controls are identical to [Single Mode](flatwizard.md#single-mode).

In Multi Mode, Flat Wizard settings are saved on a per-filter basis and do not transfer to Single Mode.

### Filter Toggle
- Enables a specific filter for flat capture

### Filter List
- Displays all available filters by name and can be expanded by clicking the > icon

## Error Handling

![The Flatwizard error window](../images/tabs/flatwizard3.png)

If Flat Wizard cannot determine the necessary exposure time or there is a conflict with the specified settings, it will display this dialog.

### Error message
- Flat Wizard will display what the issue with the current configuration is and what should be done to fix the issue

### Current Exposure Calculations
- Flat Wizard will display current calculated metrics, as the current mean, the maximum bit depth of the camera in ADU and the estimated exposure time
- Use those to adjust the values in (3)

### Flat Wizard Settings
- Specify settings for the current flat capture to get successful flats
- For further detail, see the description of the settings in the [Single Mode](flatwizard.md#single-mode) section

### Reset and Continue
- Pressing this button will lead to Flat Wizard re-starting from the Flat Min Exposure as set in the settings (3)

### Continue
- This button will continue the flat capture with the currently determined exposure time, even if outside the defined threshold

### Cancel Flat Wizard
- This button will abort Flat Wizard's sequence
