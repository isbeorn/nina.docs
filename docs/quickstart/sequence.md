# Frame and run the first sequence

Open the **Framing Assistant**, find the target, select a survey source and set the camera rotation and mosaic dimensions. Use the current profile's focal length, pixel size and sensor dimensions so the framing rectangle matches the rig. Send the framed target to the Advanced Sequencer.

In the generated Deep Sky Object container, add or verify the imaging instructions. A basic session normally includes:

1. connect equipment, cool the camera and unpark the mount in the sequence start area
2. slew and center the target, start guiding and run autofocus before exposures
3. use **Smart Exposure** or **Take Many Exposures** for each filter or exposure group
4. attach autofocus, dither, centering and meridian-flip triggers to the container that covers the exposures
5. stop guiding, park the mount and warm the camera in the sequence end area

Add loop conditions such as **Loop for Iterations**, **Loop Until Time** or **Loop While Above Horizon** to bound the target. Resolve every red validation marker before starting.

Save the sequence, then run it while you are still present. Watch the first slew, center, guide, autofocus, exposure download and file save. Confirm that the expected image appears in the configured directory with the intended metadata.

The sequence can then be reused as a template. Change the target, exposure plan and time or altitude constraints while keeping the tested startup and shutdown containers.

For the full model and reusable templates, continue with the [Advanced Sequencer overview](../sequencer/advanced/advanced.md).
