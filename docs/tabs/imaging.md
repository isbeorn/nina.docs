The Imaging tab is your imaging cockpit.
Here N.I.N.A. will display a variety of information regarding the captured images and will let you control all the vital parameters of your imaging session.

The Imaging tab is organized in windows that can be arranged dynamically to create your own layout.
Available windows can be activated and deactivated from the top bar.
To arrange a window, simply drag it from the window header and drop it according to the suggested placeholders.

Depending on your connected equipment and the panels you enable, the Imaging workspace can also show additional panels for the Dome, Safety Monitor, Flat Device, and the currently active sequencer.

The top bar is divided in two main sections: **Info** and **Tools**  

![topbarmenu](../images/tabs/Imaging_menu.PNG)  

## Info  
These windows provide important status information about captured images and connected equipment  

### Image ![imageicon](../images/tabs/imaging_imageicon.PNG)
The image panel is the central part of the Imaging tab and is used to display the latest captured images

![image](../images/tabs/imaging_image.png)

1.   Zoom In/out  
2.   Zoom to Fit  
3.   Zoom 100% (1:1 )    
4.   Opens a 3x3 crop mosaic of the current image to check for distortion and tilt
5.   Initiates a plate solving routine for the current image
6.   Toggles crosshair overlay on/off
7.   Toggles automatic display of the displayed image (for autostretch settings refer to [Options](options/imaging.md))
8.   Toggles automatic HFR (Half-Flux-Radius) star detection analysis. HFR is used for [Autofocus](options/autofocus.md) routines. When HFR detection is ON, the average HFR value for each captured image is plotted in the HFR History window
> If _Annotate Image_ is switched ON under [Options->Imaging](options/imaging.md), the calculated HFR values will be displayed on the image  
   ![HFR](../images/tabs/imaging_HFR.PNG)
9.   Activates the Bahtinov Analyzer aid tool for manual focusing with a Bahtinov Mask.

### Camera ![cameraicon](../images/tabs/imaging_cameraicon.PNG)
This panel displays the main camera and sensor properties and cooling status
> Requires a connected camera

1. Camera status details
2. Camera cooling properties
3. Camera warming 
   
![cameratab](../images/tabs/cameratab.png)

### Filter Wheel ![fwicon](../images/tabs/imaging_fwicon.PNG)
When a Filter Wheel is connected, this panel displays the current filter (1) and lets you manually switch filters by selecting them with the drop-down menu (2)

![image](../images/tabs/imaging_filterwheel.PNG)

### Focuser ![focusericon](../images/tabs/imaging_focusericon.PNG)
This panel displays the focuser status and lets you manually move it to the desired position
> Requires a connected focuser

![focuser](../images/tabs/imaging_focuser.png)

1. Focuser current status (Moving or Settling)
2. Focuser current position (for absolute stepper motor focusers)
3. Focuser temperature (if the focuser is equipped with an ambient temperature sensor)
4. Toggles focuser temperature compensation
5. Here you can set the target focuser position for the focuser to move to by clicking on "Move" (6)
6. Moves the focuser to the Target Position defined in (5)
   > It is convenient to set the target position as the position of near-focus for your setup. This position can be determined by using a Bahtinov mask on a bright star (see **Manual Focus Targets**). Once the near-focus position is determined, input the number of steps indicated in "Position" (2) in the Target Position field. You can then instruct the focuser to move to this position at the beginning of each imaging session before starting the Auto-Focusing routine
7. The arrows will move the focuser back and forth by a predefined amount related to the Auto Focus Step Size defined under Options - [Autofocus](options/autofocus.md):
    * Single arrow <  > : half the Auto Focus Step Size
    * Double arrows <<  >> : five times the Auto Focus Step Size


### Rotator ![rotatoricon](../images/tabs/imaging_rotatoricon.PNG)
Here you can control the Rotator
> Requires a connected ASCOM Rotator

![rotator](../images/tabs/imaging_rotator.png)

1. Rotator current status
2. Rotator current position
3. Input the Rotator target position
4. Moves the rotator to the Target Position


### Telescope ![telescopeicon](../images/tabs/imaging_telescopeicon.PNG)
The telescope panel provides all important information about your telescope like tracking status, sidereal time, time to meridian passing and current telescope coordinates.
> Requires a connected ASCOM telescope

![telescope](../images/tabs/imaging_telescope.PNG)

### Guiding ![guideicon](../images/tabs/imaging_guideicon.PNG)
The guider panel shows the current guider state, RMS values, and a live guide graph when the connected guider provides guide telemetry.

![guider](../images/tabs/imaging_guider.png)

1. Select the scale range of the y-axis
2. Select the scale range of the x-axis
3. Select the units for the y-axis:
    * Pixels: guide camera pixels
    * Arcseconds: guide error shown in arcseconds
4. Clears the chart
5. Chart area, this is where the guider graph is visualized

### Sequence ![sequenceicon](../images/tabs/imaging_sequenceicon.PNG)
The sequence panel follows the currently active sequencer and gives you quick access to the running sequence from the Imaging workspace. Depending on your current mode, it can show the legacy/simple sequencer, the advanced sequencer, or a navigation placeholder until a sequencer is active. To learn how to set up a sequence refer to the [Sequence](sequencer.md) section.

![sequence1](../images/tabs/imaging_sequence.png)

### Switches ![switchesicon](../images/tabs/imaging_switchesicon.PNG)
This panel will let you control the active switches
> Requires connected switches

![switches](../images/tabs/imaging_switches.png)

1. Available switches and status
2. Manually select switch
3. Toggle active switch ON/OFF

### Weather ![weathericon](../images/tabs/imaging_weathericon.PNG)
The weather panel shows the values reported by the connected weather source. Only the values provided by that source are shown.
> Some weather sources require additional setup under [Equipment > Weather](equipment/weather.md)

![weather](../images/tabs/imaging_weather.png)

### Statistics ![statsicon](../images/tabs/imaging_statsicon.PNG)
In this panel, all the important information about the last captured image is reported

![statistics](../images/tabs/imaging_statistics.png)

1. Basic statistics relative to the last captured image:
    * Width and Height, in pixels
    * Mean, Standard Deviation, Median and MAD values in ADU
    * Minimum and Maximum ADU values in the image
    * Number of detected stars and mean HFR
        > Stars and HFR will only be displayed if Automatic HFR is active
    * Bit Depth as reported by the image header  

2. Image histogram of the last captured image

### Additional Equipment Panels
Depending on your connected hardware and selected layout, the Imaging tab can also show these additional panels:

* **Dome**: reports connected state, park/home status, slewing state, following mode, shutter status, azimuth, and altitude
* **Safety Monitor**: reports whether the selected safety monitor is connected and currently safe
* **Flat Device**: reports light state, brightness, and cover state, and can optionally expose manual controls for the light, brightness, and cover

### HFR History ![HFRicon](../images/tabs/imaging_HFRicon.PNG)
When automatic HFR (Half-Flux-Radius) star detection is ON, this panel will display the history of the captured images using two configurable plotted values and autofocus markers.

![HFRHistory](../images/tabs/HFR2.png)

By default the HFR History shows HFR and star count throughout the current session. When the panel settings are open, you can:

1. Green line: Left y-axis
2. Yellow line: right y-axis
3. Triangle marks: AF runs
4. Choose the left and right plotted values
5. Filter the graph by filter
6. Include or exclude snapshots
7. Switch star measurements between pixels and arcseconds
8. Clear the current history
9. Save the current history as a CSV file

Hovering a plotted image point shows the recorded image properties for that exposure. Hovering an autofocus marker shows the recorded autofocus details for that run.
   
## Tools 

### Imaging ![image1icon](../images/tabs/imaging_imagingicon.PNG)
The imaging panel allows you to take a single exposure or live view when supported by the camera

![image1](../images/tabs/imaging_image1.png)

1. Capture exposure time in seconds
2. Filter to be used for the capture (if a Filter Wheel is connected)
3. Camera Binning
4. Toggles ON/OFF image looping. This is particularly useful for manual focus with a Bahtinov mask
5. Toggles ON/OFF saving the current capture to disk
6. When supported by the camera, this will activate the Live View mode
7. Takes the exposure

### Image History ![imagehistory](../images/tabs/imaging_historyicon.PNG)
The Image History panel shows the most recent saved images as thumbnails. The list keeps up to 50 entries and can show basic image details such as mean value in ADU, average HFR, filter, duration, and capture time.
> Clicking a thumbnail opens that image in the Image panel

Hovering a thumbnail shows the grading button so you can mark or clear that image as bad.

![history](../images/tabs/imaging_history.png)

### Plate Solving ![platesolvingicon](../images/tabs/imaging_platesolveicon.PNG)
Plate solving is a very important step in the imaging process. For further information on the Plate Solving process, refer to [Plate Solving](../advanced/platesolving.md) in the advanced topics. This panel lets you perform manual plate solving and keeps the history of all plate solving sessions.
> Prerequisites for plate solving to work are:
> * An external plate solver is defined in Options [Plate Solving](options/platesolving.md)
> * Telescope focal length is defined in Options [Equipment](options/equipment.md)
> * Camera pixel size is defined in Options [Equipment](options/equipment.md)
> * The image to be plate solved has been captured with the specified focal length and pixel size

![platesolve](../images/tabs/imaging_platesolve.png)

1. Plate solving results
2. Toggles ON/OFF syncing the telescope mount with the plate solved coordinates 
3. Toggles ON/OFF re-slewing and re-centering the mount to the plate solved coordinates if the plate solved position does not match the expected one
4. Error threshold for (3)
5. Exposure that will be used to capture the image for plate solving
6. Filter that will be used to capture the image for plate solving
7. Captures an image for plate solving
8. History of plate solving sessions

### Auto Focus ![AFicon](../images/tabs/imaging_aficon.PNG)
This panel lets you manually trigger an Auto Focus routine based on the Auto Focus parameters set in Options [Autofocus](options/autofocus.md).

![AF](../images/tabs/AF10.png)

1. Autofocus curve 
2. Last Auto Focus run parameters
3. Starts Auto Focus routine

### Manual Focus Targets ![MFicon](../images/tabs/imaging_mftargetsicon.PNG)
When you have to manual focus your scope this tab lets you conveniently choose among the current visible brighter stars according to your location and time.

![MFtargets](../images/tabs/imaging_mftargets.png)

1. List of stars to choose from
2. Selected star properties
3. Slews telescope to the selected star




