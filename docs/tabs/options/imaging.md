The Imaging options tab contains settings for file formats, save directories, Automatic Meridian flips, sequencing and image options.

![The imaging options tab](../../images/tabs/Options-Imaging10.png)

## File Settings

1. **Image Save File Format**
    * The format for every image to be saved as
        * Available formats: TIFF (zip, lzw compressed), FITS, XISF (lz4, lz4hc, zlib compressed)
    * For more information on these file formats see: 
        * [Advanced Topics: File Formats TIFF](../../advanced/file_formats/tiff.md)
        * [Advanced Topics: File Formats FITS](../../advanced/file_formats/fits.md) 
        * [Advanced Topics: File Formats XISF](../../advanced/file_formats/xisf.md)
    * All formats are saved as 16bit
    * If an OSC camera is used, the raw bayered data is saved
    
2. **Compression**
    * Select the compression method (if available)
  
3. **Byte Shuffling**
    * Enable/disable byte shuffling for XSIF compression

4. **Checksum**
    * Select a checksum method for XSIF (optional)
   
5. **Image File Path**
    * The file path where images will be saved
    
6. **Image File Pattern and Preview**
    * The structure of folders and the filename can be defined by the user using the keywords listed in the table below. A preview of the file pattern is also displayed.
    > Fixed text is also possible

    !!! tip
        By using the backslash characters `\\` you can separate your images into various folders and sub folders.
        For example N.I.N.A. will create separate folders on each new day and create sub folders for Lights Darks etc. and then inside these folders putting the actual image files when using a pattern like    
        `$$DATEMINUS12$$\\$$IMAGETYPE$$\\$$EXPOSURENUMBER$$`  
        which will result in    
        `2020-01-01 -> FLAT -> 0001.fits`  
        `2020-01-01 -> LIGHT -> 0001.fits`  
        `2020-01-02 -> LIGHT -> 0001.fits` 

    ## Auto Meridian Flip

7.  **Meridian Flip Enabled**
    * This switch toggles the automatic meridian flip
    * The sequence will check between exposures when to start the flip sequence
    * For usage of the automated meridian flip refer to [Advanced Topics: Automated Meridian Flip](../../advanced/meridianflip.md)
    
8. **Minutes after Meridian**
    * This defines the amount of time in minutes for the flip sequence to wait once the target has passed the meridian
    
9. **Use Telescope Side of Pier**
    * Some telescope mounts can tell N.I.N.A which side of the pier/tripod the telescope is on which is either west or east
    * If this is enabled, this information is taken into account with the flip sequence logic
    > Recommended to have turned off for eqmod users
    
10. **Recenter after flip**
    * When enabled, N.I.N.A. will begin a plate solving sequence after flipping to recenter the target
    > Strongly recommended to have this enabled. Requires a plate solver to be set up.
    
11. **Scope Settle time**
    * After flipping the scope, waits for the specified number of seconds to settle the scope
    > If you observe trailing in your first platesolve attempts after a flip, increase this value
    
12. **Pause before meridian**
    * The amount of time in minutes until meridian at which the imaging sequence will pause and wait for the meridian to pass as specified in (7)
    > I.E. with a value of 5, when a target is 5 minutes away from meridian, the sequence will pause and the flip sequence will wait until it has passed meridian as defined by "Minutes after Meridian". Leave 0 for no pause before meridian.

13. **Auto Focus after Flip**
    * Turns ON/OFF the AF routine after flipping. 
    > Useful for scopes with mirror flop or focus shift after Meridian Flip.
  
    ## Image Options

14. **Autostretch factor and Black Clipping**
    * These are the parameters for the display autostretch
    > N.I.N.A. uses the same MTF as PixInsight
    
    > Default settings should be very good
    
15. **Annotate Images**
    * When this setting and HFR analysis in imaging is enabled, displayed images will exhibit annotated HFR values on detected stars
    > Note that this is only for display in the imaging tab and has no effect on saved data

16. **Debayer Image**
    * When a OSC camera is used, enabling this will debayer the images for display purposes
    > Images are still saved bayered
    
17. **Debayered HFR**
    * If enabled, images will be debayered first before HFR analysis is done
    > This may help with auto focus

18. **Unlinked Stretch**
    * When a OSC camera is used, debayering the image generates 3 channels R,G and B
    * By default the autostretch in imaging is linked and may result in unbalanced colour channels
    * Enabling this will enable debayer image, and should result in balanced colour channels when stretching
    
19. **Star Sensitivity**
    * This changes the sensitivity of star detection used for HFR analysis
    > The default 'normal' should be sufficient

20. **Noise Reduction**
    * This changes the amount of noise reduction performed on the image for star detection and HFR analysis
    
21. **Sharpcap Sensor Analysis Folder**
    * This is use to locate the SharpCap sensor analysis folder used in [Imaging->Exposure Calculator](../imaging.md)


    ## Sequence

22. **Default folder for sequence files**
    * The user can set here the default folder for saving/loading sequences
  
23. **Sequence Template**
    * The user can set a default user defined sequence template here
    > Templates can be made with the 'Save template as xml' button in the sequence tab

24. **Run command when sequence completes**
    * It is possible to select an external command/script to be automatically executed after sequence ends

25. **Park mount when sequence ends**
    * Initiates a park once all sequences are complete
    > Ascom does not support custom park positions and so the driver must support this
    > For example eqmod's custom park position can override the default park position 
    
26. **Warm camera when sequence ends**
    * If the camera driver supports a warming sequence this will be initiated when sequence has ended
    
27. **Close cover when sequence ends**
    * When present, it is possible to automatically close a flat panel cover after sequence ends

    ## Layout

28. **Reset Layout**
    * This will reset the layout of docked windows in the imaging tab