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
8. Clear trained exposures times
9. Trained flats exposure times from the Flats Wizard

Trained exposure table will automatically populate when running the [Flats Wizard](../flatwizard.md) and will report the gains/exposure times for each Filter, remembering the relative flat panel brightness.

!!! tip
    Follow these steps to fully automate the acquisition of flat frames during an imaging session:  
    1. Populate the _Trained Exposure Times_ Table  
    2. Create a new  sequence (or load a pre-defined sequence) in the [advanced sequencer](../../sequencer/advanced/advanced.md) and add a sequential instruction set to it (let's call it "Flats").    
    3. Populate the instruction set with the "Trained Flat exposure" and select it for the specific filter you want to take flats for as well as the amount of flats to be taken.

    When N.I.N.A. runs this instruction it will look up the trained exposure time and the panel brightness to automatically set those and automate the flat taking process

    

 