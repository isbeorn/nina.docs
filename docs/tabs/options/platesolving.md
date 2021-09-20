The Plate Solving tab contains configuration options for each supported plate solver. 
N.I.N.A. currently supports Astrometry.Net, a Local version of astrometry, Platesolve2, ASPS and ASTAP.

For usage of the Plate Solver refer to [Advanced Topics: Plate Solving](../../advanced/platesolving.md)

![The platesolving options tab](../../images/tabs/OptionsPlateSolving10.png)

## Plate Solving

### Plate Solver
* This drop down menu selects the primary platesolver to use
> [ASTAP](https://www.hnsky.org/astap.htm) is recommended

### Blind Solver
* This drop down menu selects the blind solver that is used for initial solves and or backup solving
> The blind solver will be used in the framing assistant and normal platesolving should the primary solver fail.

### Exposure Time
* The default exposure time for plate solving frames

### Filter
* The default filter to be used for platesolving

### Binning
* The default binning to be used for platesolving

### Gain
* The default binning to be used for platesolving
> If empty the current camera Gain will be used

### Pointing Tolerance
* The threshold of acceptable error for re-centering in arcminutes

### Rotation Tolerance
* The threshold of acceptable error in the rotation axis in degrees

### Number of Attempts
* Defines the number of attempts for platesolving
> The value of 1 will not reattempt on a completely failed plate solve

### Delay between attempts
* The delay between plate solving reattempts in minutes, when number of attempts are greater than 1

## Plate Solver Settings

### Plate Solver Settings Selection
* This menu displays the currently supported platesolvers in N.I.N.A.
* Clicking on each entry will display the corresponding solvers' settings to the right (10)

### Platesolver Settings
* Each solver except Astrometry.net will require its install directory to be specified here