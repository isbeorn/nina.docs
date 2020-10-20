
![Overview](../images/sequencer/Sequencer_Overview.png)
![Drag & Drop](../images/sequencer/Sequencer_DragDrop.png)
![Issues](../images/sequencer/Sequencer_Issues.png)

## Instructions
An instruction is a single command that the application will execute. Each instruction has a different purpose and can control various types of equipment, set parameters or are utility functions to automate the imaging process.  
A complete list of available instructions can be found on the right side of the sequencer page and each instruction will have a small description as well as a tooltip of its purpose.
Instructions can be added to the sequencer and the specific parameters can be set there. The sequencer will then go through each instruction and process them.
From the available list, instructions can be dragged over from the right side to the left side and dropped into the sequencer.  
![Instructions](../images/sequencer/Sequencer_Instructions.png)

Furthermore instructions can also be directly added into the sequence by clicking on the + button on the top.  
![Add Instructions](../images/sequencer/Sequencer_AddInstruction.png)

Once an instruction is part of the sequencer, it will show the specific options for each instruction to customize the behavior. For example an item can be set to cool down the camera to a specific temperature, another item set to switch to a specific filter etc.
![Instructions in Detail](../images/sequencer/Sequencer_InstructionsDetails.png)

## Instruction Sets
Instruction sets are groups of instructions. Each set will process its content, based on the parameters inside the set. Their behavior can be further controlled by loop conditions and triggers, which is described further below.
Instruction sets can be added to the sequencer in the same way as instructions. Furthermore it is possible to nest instruction sets inside of each other.
![Add Instructions Set](../images/sequencer/Sequencer_AddInstructionSet.png)
### Sequential Instruction Set
This instruction set will process the instructions one after the other, from top to bottom.
![Sequential Instruction Set](../images/sequencer/Sequencer_SequentialInstructions.png)
### Parallel Instruction Set
All instructions inside this special instruction set will be processed in parallel. As everything will run in parallel there are no conditions or triggers available for this set.
![Parallel Instruction Set](../images/sequencer/Sequencer_ParallelInstructions.png)
### Deep Sky Object Set
This special set of instructions behaves similar to a sequential instruction set. The main difference here is, that a specific target can be specified with coordinates and rotation and then all instructions that are dependent on coordinates or rotation will pick up these coordinates automatically, so a user does not need to enter these coordinates multiple times.
![Deep Sky Object Set](../images/sequencer/Sequencer_DSOSet.png)

## Loop Conditions
Loop conditions will drive the behavior of an instruction set. Without a condition, an instruction set will just process each sequence item inside once and is finished. This behavior will be changed, when loop conditions are attached. When an instruction set has a loop condition attached, it will process its items and loop itself again as long as the attached loop conditions are fullfilled. Once at least one of these loop condition is not fullfilled anymore (e.g. a condition to loop until a specific time and the time has passed) the current instruction will be finished and afterwards the rest of the instructions will be skipped and the instruction set will be finished.
These conditions can be dragged and dropped into the loop condition section inside an instruction set.
![Loop Conditions](../images/sequencer/Sequencer_LoopConditions.png)
Loop conditions can also be directly attached to an instruction set by clicking the + icon next to the loop conditions section inside an instruction set.  
![Add Loop Conditions](../images/sequencer/Sequencer_AddLoopCondition.png)

As instruction sets can be nested, the loop conditions are also evaluated for the current instruction set and all loop conditions that are in a parent instruction set.  
Let's take a look at the below example to give an example. The top level instruction set has a condition attached to loop until 12:24:09h. Then there are two further instruction sets inside that should loop 2 times and 3 times respectively.
![Nested Conditions](../images/sequencer/Sequencer_NestedConditions.png)
The following will happen in this case:  
- The first instruction set will loop 2 times. After each instruction the parent condition will be checked too, that the remaining time is still sufficient to continue.  
- Afterwards the second instruction set will loop 3 times. After each instruction the parent condition will be checked too, that the remaining time is still sufficient to continue.  
- Once both instruction sets are finished, the whole set will be reset again, as the "Loop Until Time" condition is still valid  
- This behavior will repeat until the Time is up. Once this happens all items inside this whole set will be skipped  


## Triggers
![Triggers](../images/sequencer/Sequencer_Triggers.png)
![Add Triggers](../images/sequencer/Sequencer_AddTrigger.png)
![Nested Triggers](../images/sequencer/Sequencer_NestedTriggers.png)

## Templates
![Templates](../images/sequencer/Sequencer_Templates.png)
![User Templates](../images/sequencer/Sequencer_UserTemplate.png)
![Save As Template](../images/sequencer/Sequencer_SaveAsTemplate.png)
![Save As Template Drag Drop](../images/sequencer/Sequencer_SaveAsTemplateDragDrop.png)


