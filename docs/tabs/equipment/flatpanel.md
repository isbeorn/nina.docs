The Focuser Tab lets you connect and control one of the following flat panels:

* Alnitak Flip-Flat
* Allpro Spike-a-flat
* Artesky flat box
* Pegasus Astro Flat Master

![Flats](../../images/tabs/equipment_flats.png)


1. Flat panel information
2. Toggle light on/off
3. Brightness slider to control flat panel brightness
4. Open/Close flat panel cover if present
5. Opens the Flat Cover when taking darks or dark flats
6. Closes the flat panel cover when the sequence/s end
7. Uses trained flats exposures in (9) when taking flats
8. Clearc trained exposures times
9. Trained flats exposure times from the Flats Wizard

Trained exposure table will automatically populate when running the [Flats Wizard](../flatwizard.md) and will report the gains/exposure times for each Filter, remebering the relative flat panel brightness.

> Follow these steps to fully automate the acquisition of flat frames at the end of an imaging sequence:
> 
 >1. Populate the _Trained Exposure Times_ Table
 >3. Create a new  sequence (or load a pre-defined sequence) in [Sequence](../sequence.md) tab at the end of the imaging sequences (let's call it "Flats").  
 >4. Populate the "Flats" sequence with the flats frames you want to take, set Type = FLATS and leave exposure Time to 0.

 > When N.I:N.A. detects the image type FLATS it will automatically close the flat panel and use the Trained Exposure Times to regulate the panel brightness and exposure time

 ![FLATS1](/images/tabs/equipment_flats1.PNG)

 