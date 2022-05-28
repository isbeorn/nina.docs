The Sky Atlas allows you to search for various objects in the sky using N.I.N.A.'s own database of deep sky objects. Filters and sorting options can be applied to search results. Additionally, you may set selected objects as a [Sequence](../tabs/sequencer.md) target, or to send them to the [Framing Assistant](../tabs/framing.md).

For further information about the Sky Atlas, refer to the [Object Browser](../advanced/objectbrowser.md) advanced topic.

The Sky Atlas interface consists of following elements:

![The SkyAtlas tab](../images/tabs/skyAtlas1.png)

### Search Field

Search for the object by name or various catalog designations. Some familiar names (e.g., "Andromeda") are not implemented at the moment

### Reset
Next to the search field there is a small button to reset. This will reset all filters that have been set.

### Filters

Filtering and modifying a search can be done by various object criteria and parameters. Only objects matching the filter criteria will be shown on a search.

#### Observation filter
![The SkyAtlas tab](../images/tabs/SkyAtlas_Altitude_Filter.png)  

To easily find suitable targets for the current night an altitude filter can be used in conjuction with a from and through time and how long the target should be visible.  
Using this filter can give you a great preview of what to expect for the night as you will then only see targets that are visible during the specified time range for at least the specified amount of time.  

### Search result order

Search results can be ordered by one of several criteria: Size, Apparent Magnitude, Constellation, RA, Dec, Surface Brightness and Object Type.  Display order can be either Descending or Ascending, and you can specify the number the items displayed per page.

!!! important
    Be aware that a large number of search results may lead to performance issues

### Search

Press the Search button to initiate the search

### Moon phase and day information

Displays the current moon phase and other day information. The accuracy of this information depends upon the correct latitude and longitude being specified under **Options > General > Astrometry**.

### Object information

Displays various information about objects that are found by a search. The information includes the object name, RA, Dec, Type, Constellation, Apparent magnitude, surface brightness, and size.

If the optional [Sky Atlas Image Repository](../requirements.md#recommended-and-optional-support-software) has been installed and the path to it specified under **Options > General > General > Sky Atlas Image Directory**, a small image of the object is displayed.

### Object altitude

Displays the target object's altitude, the direction point at which it will transit, the darkness phase of the current day, and includes a vertical marker for the current time. The accuracy of the altitude curve requires that the latitude and longitude be set under **Options > General > Astrometry**.

### Set as Sequence

Sets the selected object as a sequence target for the [simple sequencer](../sequencer/simple/simple.md) or the [advanced sequencer](../sequencer/advanced/advanced.md) using a specific template based on the selection

### Set for Framing Assistant

Sets the selected object as the target in the [Framing Assistant](../tabs/framing.md)

### Slew

Slews the mount to the selected object
