

The Guider Tab lets you connect to autoguiders

![Guider](../../images/tabs/equipment_guider.png)

1. Guider information
2. Guider settings - The available settings depend on the type of guider that is connected
3. Guider Graph showing the corrections and drift of the guider

When a guider is connected, the settings for this specific guider are shown on the top right.

## PHD2 Settings
    ## Guider Settings

1. **PHD2 Path**
    * PHD2 installation path

2. **PHD2 Server URL**
    * You can set the PHD2 server settings here
    > Usually the defaults should work fine. You need to enable PHD2 server in PHD2.

3. **PHD2 Server URL**
    * PHD server port, usually the default 4400 works fine

4. **PHD2 Dither Pixels and Dither RA Only**
    * The amount of guide camera pixels to dither in PHD2. If "Dither RA only" is checked, the dither movements will only be performed in RA. 
    
    !!!tip
    Refer to [Dithering](../../advanced/dithering.md) in Advanced documentation topics for more information about Dithering and how to set the above parameters

5. **PHD2 Settle Pixel Tolerance**
    * The threshold expressed in guide camera pixels that will determine a dither settling completion after a dither move.
    > A dither  will be considered settled if, after the "Minimum Settle Time" and before the "PHD2 Settle Timeout", the guide movements in PHD2  will be below the "PHD2 Settle Pixel Tolerance".

6. **Minimum Settle Time**
    * The minimum time N.I.N.A. should wait after a dithering process until it starts the next capture

7. **PHD2 Settle Timeout**
    * The maximum time N.I.N.A. should wait after a dithering process until it starts the next capture. After this time N.I.N.A. will start a new capture regardless of dithering settling completion.

8. **Direct Guide Duration**
    * Duration of guide when Direct Guide is selected.
  
9. **Guiding Start Retry**
    * If PHD2 fails to restart NINA will send a new start guiding command again until a successful guiding is initiated.
  
10. **Guiding Start Timeout (seconds)**
    * Seconds to wait before sending a new start guiding command to PHD2 (default = 60).