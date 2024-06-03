# Troubleshooting Guide for N.I.N.A. 3.0 ASCOM Driver Issues

## Introduction
With the upgrade of N.I.N.A. from .NET Framework 4.8 to .NET 8 in its 3.0 release, some ASCOM drivers may no longer work correctly. These issues often manifest as errors indicating missing dependencies or incompatibility with the new runtime. Common errors include:

- "ASCOM.DriverException: Could not load file or assembly 'System.Runtime, Version=6.0.0.0 [...]"
- "System.TypeLoadException: Creating an instance of the COM component [...]"
- The driver is not usable by multiple applications at once
- The driver is only available as x86

To resolve these issues, you can use either the `ASCOM Device Hub` or the `Optec ASCOM Server` to wrap the problematic drivers in an isolated environment. This guide outlines the steps to remediate the issues.

## Step-by-Step Remediation

### **Install ASCOM Platform**:  
 
- Ensure you have the latest ASCOM Platform installed. You can download it from the [ASCOM Standards website](https://ascom-standards.org/).

### Using the ASCOM Device Hub

a. **Select ASCOM Device Hub**:   

- Open N.I.N.A
- Navigate to the `Equipment` tab  
- Navigate to the device specific sub-tab  
- Select the `Device Hub` entry  
- If the device hub is not available for this equipment type, fall back to the Optec ASCOM Server described further below  

b. **Configure Devices**:  

- Hit the gears icon to open the driver setup  
- In the ASCOM Device Hub setup, configure your devices as needed.  
- This typically involves selecting the appropriate driver for each device and configuring any necessary settings 

c. **Connect Devices in N.I.N.A.**:   

- Open N.I.N.A  
- Navigate to the `Equipment` tab  
- Navigate to the device specific sub-tab  
- Select the `Device Hub` as the driver for each relevant device.  
- Connect to the devices via N.I.N.A.  

### Using the Optec ASCOM Server  

a. **Download and Install Optec ASCOM Server**:   

- Download the Optec ASCOM Server from the [Optec website](https://optecinc.com/downloads/legacy/optecascomserver/). Follow the installation instructions provided.

b. **Select Optec ASCOM Server**:  

- Open N.I.N.A.  
- Navigate to the `Equipment` tab  
- Navigate to the device specific sub-tab  
- Select the `Optec ASCOM Server` entry     

c. **Configure Devices**:   

- Hit the gears icon to open the driver setup  
- In the Optec ASCOM Server setup, configure your devices as needed  
- This typically involves selecting the appropriate driver for each device and configuring any necessary settings  

d. **Connect Devices in N.I.N.A.**:  

- Open N.I.N.A.  
- Navigate to the `Equipment` tab  
- Navigate to the device specific sub-tab  
- Select the `Optec ASCOM Server` as the driver for each relevant device  
- Connect to the devices via N.I.N.A.  

## Additional Tips

- **Verify Driver Compatibility**: Ensure that you are using the latest drivers available for your devices. Check the manufacturer's website for any updates or compatibility notes regarding .NET 8.
- **Consult ASCOM Community**: If you encounter persistent issues, consider consulting the ASCOM community forums or the N.I.N.A. Discord server for additional support.
