# C# Considition 2021
Rules can be found at https://considition.com/rules

## Installation and running
* Configure desired generator and solver
* Run *Program.cs*

# Solvers
## InnerPlacerSolver
Fairly greedy solver. Simple explaination: Sorts by orderclass. Foreach orderclass takes heavy packages and finds the first location it can fit with the lowest Z (prioritize floor) and then lowest X. Then take medium/light and place in the ceiling of the truck maximizing distance to back of the truck but not further than previous package. If not possible then minimize X. I.e. if the max x5 E package is 100 then D packages will start looking at locations where their x5 <= 100.
Together this takes ALL scoring criteria into account.
This solution with some random rotations would probably be top 3 but to improve we now have a MAX_X that we can use. So we loop over each placed package in reverse order staring with the A that is furthest from the front (worst scoring A package). Then we look in the front to see if we can find a better space. Continue this for all packages. Then we do this in reverse starting with E. By doing this back and forth alot of times with a bit of randomization to placement and rotation we can improve ordescore by up to 50%. At the same time try pushing in the entire stack in increasing the packing efficency by 5%.

## FullInnerPlacerSolver / InnerPlacerHeavyReverseSolver
Same logic as InnerPlacerSolver but reduces importance of heavy neagtive effects. For FullInnerPlacerSolver this is achieved by not prioritizing floor but lowest possible X value before Z. For InnerPlacerHeavyReverse this is achived by starting placing A, B, C from front but reduce the length it can stretch out and E and D from inside. This minimizes effect on light/medium placement (still done from ceiling) but still has a kindof good assesment of OrderScore (this could for sure be improved).

# Generators
Different generators for generating the enviroment and scorer for both the live maps and for custom maps with different distributions for size, weight and orderclass

# Visualization
Contains python written 3D visualizer using pyplet aswell as logic for saving solution to csv file that pandas can read.
