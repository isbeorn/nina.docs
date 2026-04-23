This is the tab where you set up all the parameters related to your dome.

![Dome Settings](../../images/tabs/Options-Dome0.png)

## Dome & mount geometry

### Mount Type
Specify if your mount is an equatorial mount or a fork on a wedge. This will affect the calculation.

### Scope Position +N/-S (mm)
<br/>Measure the North or South offset (in mm) of the center of the mount from the center of the dome. North is true north - the same direction a polar aligned telescope would point. Use a positive number for North, and a negative number for South.

### Scope Position +E/-W (mm)
<br/>Measure the East or West offset (in mm) of the center of the mount from the center of the dome. East/West is relative to true north - the same direction a polar aligned telescope would point in the northern hemisphere. Use a positive number for East, and a negative number for West.

### Scope Position +Up/-Down (mm)
<br/>Measure the height difference (in mm) of the center of the mount axis relative to the base of the dome. For an Alt/Az mount, this is from the center of the Altitude axis, and for an EQ mount this is where the RA and DEC axes intersect. A positive number indicates the center of axis is higher than the base of the dome, and a negative number means it is lower.

### Dome Radius (mm)
<br/>Measure from the center to the rim of the dome, along the base.

### GEM Axis Length (mm)
<br/>If Alt/Az, this should be 0. For an EQ mount, slew RA to +/- 90 degrees, and measure the lateral distance (in mm) from the axis to center of the telescope aperture.

!!! note
    The purpose of this setting is to determine what should point to the center of the Dome aperture. If you have a guide scope, you should add half the length from the OTA to the top of the guide scope. For example, if the guide scope mount is 40mm and the guide scope aperture is 60mm, you should add 70mm to **GEM Axis Length**.

### Lateral axis length (mm)
<br/>If you have a dual saddle with side-by-side OTAs, this setting specifies offset (in mm) of the center of the OTA from the mount axis. Use a positive number for offsets to the right relative to the RA and DEC axis.

### Azimuth Tolerance (degrees)
<br/>The Dome slews if the target azimuth is off by more than this amount. Some dome rotators have a maximum precision, so you should set this either at that precision or greater. For example, NexDome could only support 1 degree of resolution when slewing until mid-2020 when high precision slewing was added.

## Dome Settings

### Synchronization Timeout
<br/>Actions that require an image to be taken (such as Plate Solving and Auto Focusing) depend on the dome being synchronized with the mount. If <i>Dome follows telescope</i> is enabled, imaging operations will wait until the Telescope has stopped slewing **and** the Dome is pointed to the same azimuth (within the configured tolerance). This setting specifies the maximum amount of time, in seconds, to wait for this synchronization to complete.

!!! important
    This value should be no smaller than twice the precision of the Dome rotator. For example, NexDome can only slew to integer granularity, which means its precision is 1 degree. If you own a NexDome, don't set this value smaller than 2 or **Wait for Dome Synchronization** will delay periodically.

### Settle time after slew
Add a settle time here if your dome needs to settle down for a couple of seconds.

### Allow sync while mount slews
<br/>When an external application or the handset slews the mount, dome synchronization will wait for the mount to stop before it rotates the dome. Turning this option on allows dome synchronization to chase the mount. If rotation is fast, you may prefer to turn this on.

### Sync slew dome when scope slews
<br/>When enabled, N.I.N.A. slews the dome to synchronize it with the telescope when the scope slews. This does not continuously maintain dome synchronization while the scope tracks.

### Find Home Before Parking
<br/>This is an innovative reliability feature. Some Domes, such as NexDome, require the Park location to be precise so that a battery powering the shutter motor can recharge. If this setting is enabled, the Dome will find the Home position (if the Dome provides one) before parking and closing the shutter. This resynchronizes the Dome azimuth to increase Park accuracy.

!!! note
    Some Dome vendors also provide manuals to configure many of these same parameters. If you're stuck, try checking some of them out too.

## Shutter Coordination

!!! important
    These settings relate to the safety of your equipment. They govern only N.I.N.A.'s control of the dome or roof while N.I.N.A. is running and connected. Dome or roof safety mechanisms should always have hardware backup and electrical interlocks to prevent undesired shutter or roof movement.

### Close on unsafe conditions
When a [safety monitor](../equipment/safetymonitor.md) is connected, the dome will automatically close immediately when the monitor is reporting unsafe conditions. This will happen independent of any other action in the application like the sequencer.

### Refuse open if a Safety Monitor is not connected
When enabled, N.I.N.A. will refuse to open the shutter or roof if a [Safety Monitor](../equipment/safetymonitor.md) device is not connected.

### Park mount before shutter operation
When enabled, N.I.N.A. attempts to park the mount before opening or closing the shutter.

### Park dome before shutter operation
When enabled, N.I.N.A. sends the dome to its park position before attempting to open or close the shutter.

### Refuse mount unpark if shutter is not open
When enabled, N.I.N.A. rejects attempts to unpark the mount unless the shutter or roof reports an open state.

### Refuse open or close if mount is unparked
When enabled, N.I.N.A. refuses to open or close the shutter or roof if the mount is in an unparked state. This is evaluated after the "Park mount before shutter operation" setting.
