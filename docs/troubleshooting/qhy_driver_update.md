As of N.I.N.A. 1.11 version 21.3.13.17 of the QHY SDK is included with N.I.N.A.. Starting with this SDK version, you will need to make sure that your QHY USB driver is updated to at least version 21.2.20 or later by installing the latest QHY driver All-in-One pack. This is because the version of QHY SDK that N.I.N.A. ships with and QHY USB driver saw a portion of their Cypress FX3 IO libraries updated and the two must stay in sync.

To help with this requirement, these versions are included in the Equipment > Camera screen. Additionally, N.I.N.A. will emit a warning level notification if the detected driver version is lower than 2.1.20:  
![Outdated Driver](../images/troubleshooting/qhy/outdated_driver.png)  
The benefits of this update include improved USB error recovery. 
*This requirement does not apply to cameras that have USB2 interfaces (CCDs, A-Series, and QHY5II 1.25″ cameras).*  

### In case of USB driver update problems
Some folks have experienced issues updating the USB driver, where it appears that the driver update did not take. There is one thing to try if this is the case:

1. With the camera **disconnected**, right click start and open Device Manager
![Device Manager](../images/troubleshooting/qhy/device_manager.png)  
2. Go to the view menu and select show hidden devices
![Hidden Devices](../images/troubleshooting/qhy/hidden_devices.png)  
3. Expand the AstroImaging Equipment section
![AstroImaging Section](../images/troubleshooting/qhy/astroimaging.png)  
4. Right click **QHY5IIISeries_FW**, select *Properties*, and then the *Driver* tab
5. Press the *Update Driver* button, and select *search automatically for drivers*
6. Also do steps 4 and 5 for the **QHY5IIISeries_IO** device
7. Plug in the camera. Verify that it is on the newer device driver version

After the above steps are done, and barring any deeper issues, the driver and firmware loader for the camera should be updated to the version that was installed. QHY are aware of this issue and are looking into it.