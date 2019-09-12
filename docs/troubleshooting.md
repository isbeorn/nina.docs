## General Troubleshooting

Should you encounter any bugs during your usage of N.I.N.A, please report them on the project's [Issue Tracker](//bitbucket.org/Isbeorn/nina/issues?status=new&status=open) or directly to the team on the [Discord chat](//discord.gg/fwpmHU4). If possible, attach the latest log file. It is also helpful to increase the application's logging verbosity to **Debug** or **Trace** under **Options > Log Level**. The logging level of **Trace* includes the most information and may lead to the accumulation of large log files. Therefore, it is not recommended to leave that level specified under normal conditions.

Log files may be found in the `%LOCALAPPDATA%\NINA\Logs\` folder.

## Installation Issues

Often, Anti-Virus software can interfere with the installation of N.I.N.A. and cause either an aborted installation or an incomplete one.
In these cases, it is advisable to disable any AV software temporarily and reattempt the installation.
The likelihood of running into installation issues can vary with the number and types of AV software in use, as well as how strict the AV software is set to operate.
No significant problems have been encountered on Windows 10 when using only Microsoft's built-in Windows Defender suite.

## Application Crashes

In case you encounter a hard crash, Windows will create a crash dump file to investigate the problem in detail. Should you encounter such an issue, please provide this crash dump file.

The crash dump may be found in the `%LOCALAPPDATA%\NINA\CrashDump\` folder.
