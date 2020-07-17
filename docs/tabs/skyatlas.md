The Object Browser enables searching for various objects in the sky, filter by object type and other various criteria, set them as sequence target or for the framing assistant.

For further information about the object browser refer to the [advanced object browser topic](../advanced/objectbrowser.md).

The UI consists of following elements:

![The SkyAtlas tab](../images/tabs/skyAtlas1.png)

1. **Search Field**
    * Search for the object by various catalog designations
    > Some commonly used names (e.g. “Andromeda”) are not implemented at the moment
    * Can be left empty
    > Press “Enter” to search
        
2. **Filters**
    * Filtering and modifying a search can be done by various object criteria and parameters
        * Object type (For more info about object types refer to the [glossary](../glossary.md))
            * Galaxy, 2 Stars, 1 Star, Cluster with Nebulosity in a Galaxy, Open Cluster, Planetary Nebula, Galaxy cluster, 3 Stars, Globular Cluster, Asterism, Dark Nebula, Bright Nebula, 8 Stars, Nonexistent, Supernova Remnant, Cluster with Nebulosity, Quasar, 4 Stars, Diffuse Nebula in a Galaxy, various forms of clusters in the LMC and SMC
        * Constellation: any constellation in the night sky
        * Coordinates: limits the search to specific right ascension and declination coordinates or a specific subset of those
        * Surface brightness
        * Apparent size
        * Apparent magnitude
        * Minimum altitude: limits the search to objects that are at a minimum specified altitude during a specific timeframe
        * Reference date: The user can specify a reference date for sequence planning
        
3. **Order settings**
    * Ordering of objects can be done by the following criteria:
        * Size, Apparent Magnitude, Constellation, RA, Dec, Surface Brightness and Object Type
    * Display order can be either Descending or Ascending
    * The user can specify the number the items displayed per page
    > Be aware that high numbers can possibly lead to performance issues

4. **Search**
    * Start the search for objects based on the set parameters

5. **Moon phase**
    * Displays the moon phase according to the coordinates of Lat/Long as set in the Settings
    
6. **Object information**
    * Displays various object information, including name, RA, Dec, Type, Constellation, Apparent magnitude, surface brightness and size. If the sky atlas image repository is installed, it also displays an image of the object
    
7. **Object altitude**
    * Shows the object's altitude, what direction it will transit and the darkness phase of the specified day, including the current time
    > Altitude depends on Latitude and Longitude set in the Settings

8. **Set as Sequence**
    * Sets the object as sequence target and uses its name in the sequence tab as well
    
9. **Set for Framing Assistant**
    * Sets the object as target for the framing assistant

10. **Slew**
    * Slews the mount to the specified object