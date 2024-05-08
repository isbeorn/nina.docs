## Blank screen when using remote access
When you are experience a blank screen of N.I.N.A. during usage of a remote access software it is most likely caused by not having any screen connected at the target machine. As N.I.N.A. is hardware accelerated and Windows will not render anything that is hardware accelerated when no display is connected, the application will be just a blank screen.
To get around this issue, you can disable hardware acceleration in N.I.N.A. by navigating to Options > General > Advanced > Hardware Acceleration and setting it to off. After a restart the application should now show properly when using the remote access software.

## Artifacts and missing graphics

In the past there were reports where the application was not rendering correctly. Follow this guide if your application looks like the window below, where some icons are disappearing and windows are not rendering:  

![Display Issues](../images/troubleshooting/renderissues.png) 

It has been identified that there is an audio driver service called "Nahimic service" responsible for causing side effects on WPF applications. (WPF is the User Interface framework that N.I.N.A. is built on).
Once this service is stopped, the application will render correctly again.  
  
To disable the service:  
- Open the Windows Run menu by holding the keys `⊞ Win` + `r`   
- Enter "services.msc" into the window and hit "Ok"  
- A new window will open showing all available services.   
- Check for a service called "Nahimic Service" and follow the steps in the screenshot below  

![Disable Nahimic Service](../images/troubleshooting/disablenahimic.png) 
