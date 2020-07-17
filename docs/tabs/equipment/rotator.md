The Rotator Tab lets you connect an ASCOM-compatible rotator. 
A Manual Rotato option is also available

![Rotator](../../images/tabs/equipment_rotator.png)

1. Rotator information 
2. Move the rotator to the selected angle

## Manual Rotator

Mmanual Rotator is a very useful tool for those setup that do not have a motorized rotator but still want to be able to match the faming angle as defined in the [Framing Tab](../framing.md)

To enable manual rotator you must:

1. Define a _Rotator Tolerance_ in [Options->Platesolving](../options/platesolving.md)
2. Connect the manual rotator in the Rotator tab
3. Frame your object in [Framing](../framing.md) and Add as Sequence Target
4. Enable _Center Target_ in [Sequence](../sequence.md)
5. Start the sequence

Once the sequence is started and the mount has finished slewing to the target, N.I.N.A. will perform a platesolving to determine the curent framing coordinates and rotation angle. If the difference between the angle determined by the platesolving and the angle specified in _Sequence ->Rotation_  is above the _Rotator Tolerance_, a pop-up will appear indicating the degrees and direction you need to rotate the camera.
Rotate the camera and close the manual rotator window, a new platesolve will be performed. If the angle is still above the _Rotator Tolerance_ the process will be repeated.

![TargetSettings](../../images/tabs/targetsettings.png)

![ManualRotator](../../images/tabs/manualrotator.PNG)

!!!tip
     If you want to set camera rotation before starting the main imaging sequence you can use a dummy sequence with exposure 1s to kick the manual rotator in
