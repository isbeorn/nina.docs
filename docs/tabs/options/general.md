The General Settings tab contains application-wide settings that do not belong to a single device or workflow.

![The general settings tab](../../images/tabs/OptionsGeneralv10.png)

## Profiles

The profile list shows the profiles available on your system, including their active state, name, description, and ID.

### Profile Buttons

Use the buttons below the list to:

* add a new profile
* duplicate the selected profile
* delete the selected profile
* load the selected profile

!!! important
    A different profile cannot be loaded while equipment is connected.

## General

### Name, Description, Language and Font

* The active profile name and description can be edited here.
* The language selector changes most labels, tooltips, and buttons throughout the application.
* The font selectors let you choose both the font family and the font face used by the user interface.

> If you want to contribute to localization, check out the [localization guide](../../contributing/localization.md).

### Sky Survey Cache Directory

This directory is used for downloaded survey images, such as images fetched while using the Framing Assistant.

### Profile Chooser On Startup

This option controls whether N.I.N.A. opens with the profile chooser or reuses the saved profile selection automatically.

### Advanced Settings

The **Advanced Settings** expander contains the following application-wide options:

#### Auto Update Source

Selects which update channel N.I.N.A. checks. Available channels are **Nightly**, **Beta**, and **Release**.
* Currently there are 3 version branches, Nightly, Beta and Release

    > Nightly offers the latest developments including bugfixes and feature additions. These builds are not as well tested as the beta or released versions but should be stable for imaging runs

    > Beta versions are considered feature complete and are typically very stable. They will undergo a series of testing and bug fixing with incremental updates before becoming a release version

    > Release offers the most reliability and stability for your imaging runs 

#### Single Imaging Layout

Use this when you want the Imaging tab layout to be shared across profiles instead of keeping separate Imaging layouts per profile.

#### Logging

* Sets the application log level
* The recommended log level for general usage is `Info`
* Opens the log folder directly from the button beside the selector

Available log levels are `Error`, `Warning`, `Info`, `Debug`, and `Trace`.

#### Notification Work Area and Notification Corner

These settings control where N.I.N.A. shows toast notifications on the screen.

#### Device Polling Interval

Sets the device polling interval in seconds.

#### Save Queue Size

Controls the size of the image save queue.

#### Hardware Acceleration

Turns hardware-accelerated rendering on or off for the application.

!!! note
    Changing **Single Imaging Layout**, **Save Queue Size**, or **Hardware Acceleration** requires restarting N.I.N.A.

## Color Schemes

### Current UI Color Scheme

* Selects the primary color scheme used by the application
* Lets you switch to one of the built-in themes
* Lets you copy the current scheme into a custom scheme and then edit the individual colors

### Alternative UI Color Scheme

This is a second full color scheme that you can configure separately from the current one.

### Color Scheme Toggle

The eye button toggles between the current and alternative color scheme.

## Astrometry Settings

This section contains your observing site information:

* latitude
* longitude
* elevation
* observer name
* observatory name
* site name

### GNSS Button

Loads coordinates from the currently selected GNSS source.

> GNSS source configuration is done in [Options > Equipment](equipment.md).

### Planetarium Button

Loads coordinates from the currently selected planetarium application.

> Planetarium software selection and connection details are configured in [Options > Equipment](equipment.md).

### Custom Horizon

Use a custom horizon file when you want local obstructions to be reflected in altitude-based displays and sequence logic.

The file format is a list of azimuth/altitude pairs. Gaps between azimuth values are interpolated. At least two azimuth/altitude pairs are required.

Example:

```
# Az Alt
0 14
5 69
55 77
90 70
105 35
115 10
120 24
135 25
145 30
205 20
230 23
235 14
240 14
265 33
285 33
350 20
360 14
```

Once a horizon file is configured, it is shown in altitude charts across the application.

![Altitude chart with horizon](../../images/tabs/altitudechartwithhorizon.png)

### World Map

The world map provides a quick visual check of the currently configured observing location.

## Plugin Repositories

At the bottom of the page you can manage the list of plugin repositories used by the **Available** plugins tab.

* Use the **+** button to add an additional repository URL.
* Use the trash button to remove a repository you no longer want to query.

!!! note
    The main N.I.N.A. plugin repository is always kept in the list and cannot be removed from this view.
