## Sequence Templates

When creating a new sequence item via Framing or when just adding a new target from anywhere in the application the default values of a Sequence will most likely not be satisfactory for a user.
Therefore N.I.N.A. offers the possibility to create a sequence template and set this template inside the settings.

### Exporting a sequence Template

To create a sequence template you just need to setup a standard sequence to your liking. In the example below I have set up a sequence for broadband. Once all sequence settings are done, just click on the save button to store the sequence xml on your hard drive. Once a template is set, each new Sequence you create will be pre-populated with this template. As you currently can only have one template for one profile, you can have two copies of your profile with different templates and take one profile for narrowband and one for broadband as an example.

![Broadband](../../images/advanced/sequencing/Broadband.png)

This exported sequence can then be set as a template inside **Options->Imaging->Sequence**

![Template](../../images/advanced/sequencing/Template.png)

!!! tip
    The best use for these templates are sequences created out of the Framing Assistant. For example when you have the broadband sequence set, that were mentioned earlier, you can just set up the mosaic, click "Replace Sequence" and the following sequence is created based on the template

![Mosaic](../../images/advanced/sequencing/Mosaic.png)

## Sequence Auto-Focus

When having a motorized focuser, the sequence offers a variety of options to ensure that the focus is as good as possible during the sequence run. Let's look into each option and explain their usage in more depth.

![AutoFocus](../../images/advanced/sequencing/AutoFocus.png)

*On start* - This one is rather obvious. Most of the time you want to have this on in your first target to ensure that the initial focus position is good.  

*On filter change* - This option is useful when you don't know or don't have your filter offsets set and your filters aren't parfocal and thus having a different focus position. This is a good option when you only have a few filter changes during the course of the night. Otherwise it is beneficial to measure and set filter offsets instead.  

*After elapsed time* - Triggering an autofocus after an elapsed time is almost always just a guesstimate and should be avoided. A better metric is the option to focus after an HFR increase.  

*After temperature change* - This will use the focuser temperature probe as reference and refocuses every time the temperature drifts by the specified amount in reference to the last initiated auto focus. When you know that your equipment will shift focus after a certain amount of temperature change, this is a good option.  

*After HFR increase* - This method will only trigger when the measured HFR trend is going up by a certain percentage. Having just one sub with worse focus won't necessarily trigger this, as it could just be one sub with bad guiding or worse sky conditions. In general this is a good way to ensure best Auto Focus during the run and can be used almost always. A visual representation of the HFR history can be seen in the imaging tab which is used to determine the baseline.

![HFRHistory](../../images/tabs/HFR2.png)