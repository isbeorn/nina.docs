The Imaging tab is your imaging cockpit.   
Here  N.I.N.A. will display  a variety of information regarding  the captured images and will let you control all the vital parameters of your imaging session.

The Imaging tab is organized in windows that can be arranged dynamically to create your own layout.
Available windows can be activated and deactivated from the top bar.
To arrange a window simply drag it from the window header and drop it according to the suggested placeholders.   

The top bar is divided in two main sections: **Info** and **Tools**  

![topbarmenu](../images/tabs/Imaging_menu.PNG)  

## Info  
These windows provide important status information about captured images and connected equipment  

### A. Image  ![imageicon](../images/tabs/imaging_imageicon.PNG)  
The image panel is the central part of the Imaging tab and is used to display the latest captured images

![image](../images/tabs/imaging_image.png)

1.   Zoom In/out  
2.   Zoom to Fit  
3.   Zoom 100% (1:1 )    
4.   Opens a 3x3 crop mosaic of the current image to check for distortion and tilt
5.   Initiates a platesolving routine for the current image
6.   Toggles crosshair overlay on/off
7.   Toggles automatic display of the displayed image (for autostretch settings refer to [Options](options/imaging.md))
8.   Toggles automatic HFR (Half-FLux-Radius) star detection analysis. HFR is used for [Autofocus](options/equipment.md) routines. When HFR detection is ON, the average HFR value for each captured image are plotted in the HFR History window (M)
> If _Annotate Image_ is switched ON under [Options->Imaging](options/imaging.md), the calculated HFR values will be displayed on the image  
   ![HFR](../images/tabs/imaging_HFR.PNG)
9.   Activates the Bahtinov Analyzer aid tool for manual focusing with a Bahtinov Mask.

### B. Camera ![cameraicon](../images/tabs/imaging_cameraicon.PNG)  
This panel displays the main camera and sensor properties and cooling status
> Requires a connected camera

1. Camera status details
2. Camera cooling properties
3. Camera warming 
   
![cameratab](../images/tabs/cameratab.png)

### C. Filter Wheel ![fwicon](../images/tabs/imaging_fwicon.PNG)  
When a Filter Wheel is connected, this panel displays the current filter (1) and lets you manually switch filters by selecting them with the drop-down menu (2)

![image](../images/tabs/imaging_filterwheel.PNG)

### D. Focuser  ![focusericon](../images/tabs/imaging_focusericon.PNG)  
This panel displays the focuser status and lets you manually move it to the desired position
> Requires a connected focuser

![focuser](../images/tabs/imaging_focuser.png)

1. Focuser current status (Moving or Settling)
2. Focuser current position (for absolute stepper motor focusers)
3. Focuser temperature (if the focuser is equipped with an ambient temperature sensor)
4. Toggles focuser temperature compensation
5. Here you can set the target focuser position for the focuser to move by clicking on "Move" (7)
6. Moves the focuser to the Target Position defined in (6)
   > It is convenient to set the target position as the position of near-focus for your setup. This position can be determined by using a Bahtinov mask on a bright star (see **Manual Focus Targets**). Once the near-focus position is determined, input the number of steps indicated in "Position" (3) in Target Position field. You can then instruct the focuser to move to this position at the beginning of each imaging session before starting the Auto-Focusing routine 
7. The arrows will move the focusers back and forth of a pre-defined amount related to the Auto Focus Step Size defined under Options - [Equipment](options/equipment.md):
    * Single arrow <  > : half the Auto Focus Step Size
    * Double arrows <<  >> : five times the Auto Focus Step Size


### E. Rotator ![roticon](../images/tabs/imaging_rotatoricon.PNG)  
Here you can control the Rotator
> Requires a connected ASCOM Rotator

![rotator](../images/tabs/imaging_rotator.png)

1. Rotator current status
2. Rotator current position
3. Input the Rotator target position
4. Moves the rotator to the Target Position


### F. Telescope ![telescopeicon](../images/tabs/imaging_telescopeicon.PNG)  
The telescope panel provides all important information about your telescope like tracking status, sidereal time, time to meridian passing and current telescope coordinates.
> Requires a connected ASCOM telescope

![telescope](../images/tabs/imaging_telescope.PNG)

### G. Guiding  ![guideicon](../images/tabs/imaging_guideicon.PNG)  
The guider panel replicates the PHD guiding graph in real time. 
> PHD2 must be connected for the Guider to display the guiding trends and pulses (RA and DEC).

![guider](../images/tabs/imaging_guider.png)

1. Select the scale range of y-axis
2. Select the scale range of x-axis
3. Select the units for y-axis:
    * Pixels: guide camera pixels
    * Arcseconds: units in arcseconds (this is calculated by PHD2 based on your guide camera pixel size and guide scope focal length)
4. Clears the chart
5. Chart area, this is where the PHD2 graph will be visualized

### H. Sequence ![sequenceicon](../images/tabs/imaging_sequenceicon.PNG)  
Sequence panel lets you start/stop imaging sequences and provides information on the sequence run in a compressed format. To learn how to set up a sequence refer to the [Sequence](sequencer.md) section.

![sequence1](../images/tabs/imaging_sequence.png)

### I. Switches ![switchesicon](../images/tabs/imaging_switchesicon.PNG)  
This panel will let you control the active switches
> Requires connected switches

![switches](../images/tabs/imaging_switches.png)

1. Available switches and status
2. Manually select switch
3. Toggle active switch ON/OFF

### J. Weather  ![weathericon](../images/tabs/imaging_weathericon.PNG)  
Weather and temperature information from OpenWeatherMap
> OpenWeatherMap API key must be set under Options [Equipment](options/equipment.md)

![weather](../images/tabs/imaging_weather.png)

### K. Statistics  ![statsicon](../images/tabs/imaging_statsicon.PNG)  
In this panel  all the important information about the last captured image are reported

![statistics](../images/tabs/imaging_statistics.png)

1. Basic statistics relative to the last captured image:
    * Width and Height, in pixels
    * Mean, Standard Deviation, Median and MAD values in ADU
    * Minimum and Maximum ADU values in the image
    * Number of detected stars and mean HFR
        > Stars and HFR will only be displayed if Automatic HFR is active
    * Bit Depth as reported by the image header  

2. Image histogram of the last captured image

### M. HFR History  ![HFRicon](../images/tabs/imaging_HFRicon.PNG)  
When automatic HFR (Half-Flux-Radius) star detection is ON, this panel will diplay the history of HFR values and number of stars used to evaluate the HFR for each exposure.
The chart is limited to displaying a moving window of the last 100 exposures. 


![HFRHistory](../images/tabs/HFR2.png)

1. Yellow line: number of stars used for HFR evaluation
2. Green line: mean HFR
3. Triangle marks: AF runs
   
## Tools 

### N. Imaging ![image1icon](../images/tabs/imaging_imagingicon.PNG)  
The imaging panel allows you to take a single exposure or live view when supported by the camera

![image1](../images/tabs/imaging_image1.png)

1. Capture exposure time in seconds
2. Filter to be used for the capture (if a Filter Wheel is connected)
3. Camera Binning
4. Toggles ON/OFF image looping. This is particularly useful for manual focus with a Bahtinov mask
5. Toggles ON/OFF the saving on HD of the current capture
6. When supported by the camera, this will activate the Live View mode
7. Takes the exposure

### O. Image History ![imagehistory](../images/tabs/imaging_historyicon.PNG)  
The Image History panel shows a list of thumbnalis of the current sequence captured images with basic statistics: Mean value in ADU, average HFR, Filter used, duration and capture time.
> By double-clicking on any of the thumbnails the relative image will be opened in the Image panel (A)

![history](../images/tabs/imaging_history.png)

### P. Plate Solving ![platesolvingicon](../images/tabs/imaging_platesolveicon.PNG)  
Platesolving is a very important step in the imaging process, for further information on the Plate Solving process refer to [Platesolving](../advanced/platesolving.md) in the advanced topics. This panel lets you perform a manual platesolving and keeps the history of all platesolving sessions.
> Prerequisites for platesolving to work are:
> * An external Platesolver is  defined in Options [Platesolving](options/platesolving.md)
> * Telescope focal length is defined in Options [Equipment](options/equipment.md)
> * Camera pixel size is defined in Options [Equipment](options/equipment.md)
> * The image to be plate solved has been captured with the specified focal length and pixel size

![platesolve](../images/tabs/imaging_platesolve.png)

1. Plate solving results
2. Toggles ON/OFF syncing the telescope mount with the plate solved coordinates 
3. Toggles ON/OFF re-slewing and re-centering the mount to the plate solved coordinates if the plate solved position is not matching with the expected one
4. Error threshold for (4)
5. Exposure that will be used to capture the image for plate solving
6. Filter that will be used to capture the image for plate solving
7. Captures  an image for plate solving
8. History of plate solving sessions

### Q. Polar alignment ![polaricon](../images/tabs/imaging_polaricon.PNG)    
The polar alignment panel gives you two ways to determine how off your polar alignment is and to improve it. One is plate solved polar alignment and the other is DARV slew.

![polaralign](../images/tabs/imaging_polaralign.png)

1. Polar scope: shows position of polaris when looking through the mount’s polar scope
    > Will not work for southern hemisphere. Uses latitude and longitude from settings.
2. Defines the exposure time that should be used for a plate solved polar alignment measurement
3. Filter for the alignment
4. Camera binning
5. Measurement location for altitude where the telescope is pointing at. Can be either east or west
6. Measure Altitude Error: will start a platesolve of current position, then slews half a degree along RA axis, takes another platesolve and compares the result of both to measure the amount of error
    > Telescope should point east or west at 0 declination
7. Meridian Offset and Declination for telescope
    > The values entered here will be saved as settings for using next time
8. Slews mount to specified meridian offset and declination
9. Will start a platesolve of current position, then slews half a degree along RA axis, takes another platesolve and compares the result of both to measure the amount of error
    > Telescope should point south near meridian at 0 declination
10. Same as Step (7) except for azimuth
    > The values entered here will be saved as settings for using next time
11. Same as step (8)
12. Duration and Rate in which the scope should move for a DARV Slew
13. Initiates a DARV Slew by slewing half the specified Duration in one RA direction and then back while taking an exposure.


### R. Auto Focus ![AFicon](../images/tabs/imaging_aficon.PNG)  
This panel lets you manually trigger an Auto Focus routine based on the Auto Focus parameters set in Options [Equipment](options/equipment.md).

![AF](../images/tabs/AF10.png)

1. Autofocus curve 
2. Last Auto Focus run parameters
3. Starts Auto Focus routine

### S. Manual Focus Targets ![MFicon](../images/tabs/imaging_mftargetsicon.PNG)  
When you have to manual focus your scope this tab lets you conveniently choose among the current visible brighter stars according to your location and time.

![MFtargets](../images/tabs/imaging_mftargets.png)

1. List of stars to choose from
2. Selected star properties
3. Slews telescope to the selected star

### T. Exposure Calculator ![ExpCalcIcon](../images/tabs/imaging_expcalcicon.PNG)  

This tool will suggest a recommended exposure time based on the read noise from the camera sensor and the average skyglow.
If SharpCap is installed and a Sensor Analysis is available for the current camera, RN and FW are derived from the sensor analysis.

![ExpCalc](../images/tabs/expcalc10.png)

1. Exposure time. This is only used to measure the average skyglow when clicking on (8)
2. Filter: this menu lets you choose the filter for the calculation
    > Filters affect the  wavelength bandpass of incident light and therefore the average skyglow. The analysis should be repeated for each filter to determine an optimal exposures set.
3. Gain: select the gain for the exposure analysis. 
    > Camera parameters vary significantly with gain values, the analysis should be repeated for the different gain values used for imaging. A guideline to determine the optimal gain values for your imaging conditions can be found [here](https://www.youtube.com/watch?v=ub1HjvlCJ5Y&list=PLhIb8N-jSR_rNKxCFGzbd87TfmyQS4U4X&index=14)
4. The drop-down menu lets you select available sensor analysis files from SharpCap
   > SharpCap must be installed and you must first perform a Sensor Analysis in SharpCap following the instructions [here](http://docs.sharpcap.co.uk/3.2/19_SensorAnalysis.htm). Sensor analysis files are saved in %APPDATA%\Roaming\SharpCap\SensorCharacteristics
5. Full Well Capacity in electrons: if known this value can be entered manually for the specified gain or retrieved automatically from the Sharpcap Sensor Analysis
6. Read Noise in electrons: if known this value can be entered manually for the specified gain or retrieved automatically from the Sharpcap Sensor Analysis
7. BIAS median value (in 16bit): median ADU value of a bias frame (scald to 16bit), can be entered manually or calculated automatically by covering the scope and clicking on the "Calculate Bias" button
8. Click here to perform the exposure for the analysis
9. The recommended exposure times are displayed in this section

Recommended exposure time is calculated according to the following [formula](https://forums.sharpcap.co.uk/viewtopic.php?t=456):

`Recommended Exposure Time = 10 * read noise squared / light pollution rate`

where light pollution rate is defined as:

`(median ADU of a subframe - median of the bias) * electrons per ADU / the length of the exposure`

Further details on the theory for the optimal exposure calculation can be found [here](https://www.youtube.com/watch?v=3RH93UvP358)

 >  The analysis will use whatever gain is specified and linear interpolate between the values calculated by sharpcap. for example, if you have read noise in the analysis for gains 100 and 150 but specify 125, the tool will set the read noise exactly between the two.

 > Remember to cool down your camera to the desired temperature before using the tool, high Dark Current values may affect the results.




