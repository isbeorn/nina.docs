## General Troubleshooting

Should you encounter any bugs during your usage of N.I.N.A., please report them on the project's [Issue Tracker](https://github.com/isbeorn/nina/issues) or directly to the team on the [Discord chat](//discord.gg/fwpmHU4). If possible, attach the latest log file. It is also helpful to increase the application's logging verbosity to **Debug** or **Trace** under **Options > Log Level**. The logging level of **Trace** includes the most information and may lead to the accumulation of large log files. Therefore, it is not recommended to leave that level specified under normal conditions.

Log files may be found in the `%LOCALAPPDATA%\NINA\Logs\` folder.

## Installation Issues

### Installation fails in general

Often, Anti-Virus software can interfere with the installation of N.I.N.A. and cause either an aborted installation or an incomplete one.
In these cases, it is advisable to disable any AV software temporarily and retry the installation.
The likelihood of running into installation issues can vary with the number and types of AV software in use, as well as how strict the AV software is set to operate.
No significant problems have been encountered on Windows 10 when using only Microsoft's built-in Windows Defender suite.

### Error: "The feature you are trying to use is on a network resource that is unavailable"

In case you get this error or are unable to uninstall the application, some of the registry keys got corrupted. Follow the advice on the following page to fix the corrupted keys:
[https://support.microsoft.com/en-us/topic/fix-problems-that-block-programs-from-being-installed-or-removed-cca7d1b6-65a9-3d98-426b-e9f927e1eb4d](https://support.microsoft.com/en-us/topic/fix-problems-that-block-programs-from-being-installed-or-removed-cca7d1b6-65a9-3d98-426b-e9f927e1eb4d)

## Application Crashes

### Crashdump
In case you encounter a hard crash, Windows will create a crash dump file to investigate the problem in detail. Should you encounter such an issue, please provide this crash dump file.

The crash dump may be found in the `%LOCALAPPDATA%\NINA\CrashDump\` folder.

### Event Viewer  
![Event Viewer](../images/troubleshooting/eventviewer.png)
You can check the Windows Event Viewer for root causes of hard application crashes.  
To open event viewer, go to the windows search bar, enter "Event Viewer" and open the app.  

Once inside the app, go to the "Windows Logs -> Application" (1). Then go to "Filter Current Log..." (2) and narrow down the "Event Sources" (3) to only select ".NET Runtime" in the pop up window and click "OK".  
After the filter is applied, you will find all event sources in the list in the middle (4). Look for the message that contains "Application: NINA.exe" in the Detail section (5). This will show the complete stack trace of why the application crashed. This is useful information that can be posted to the contributors who can analyze this further.
