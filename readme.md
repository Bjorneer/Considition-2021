# C# Starterkit Considition 2021
This is the StarterKit for Considition 2021 which will help you get going as quickly as possible with the competition.

- **The Main Program:** This is where we run the main parts. There is an example solver implemented, GreedySolver, which you can try out of the box.
- **The Game Layer:** A wrapper between the API and the Main Program. Helps you with formatting the input to the API.
- **The API:** A representation of the REST-API that the game is played with. Can be used directly or through the Game Layer.
- **GreedySolver:** This is the out of the box solver you can change, take inspiration from or just replace with your own solver.
- **Scoring:** An explanation of the factors that determine the score.

The competition itself and how the evaluation of the solutions work is described in more in detail on [Considition.com/rules](considition.com/rules).

# Installation and running
Run *Program.cs*

# Main Program
The Main Program is simple. Each run of the program does the following:
- Fetches the desired map
- Creates a solution with the selected solver
    - This is where you can implement your solution. Optimize the solver to maximize your score.
- Submits the score to be validated and if approved, evaluated and posted for the competition.
- Prints the final score, game id to keep track of your best attempts, and a link if you want to see a visualisation of the game.

# Greedy Solver
Greedy Solver is a very simple algorithm which simple focuses on placing the heaviest and largest packages in the back corner of the truck, it completely disregards the order of the packages. Each package is represented by the coordinates of it's 8 corners. You can take a look at the function AddPackage() if you're unsure of how to calculate the coordinates when adding a package.

# Game Layer
The game layer has all the functions you need to play the game.

**The Game**
- **New Game** Gets the properties of the selected map in a *GameResponse*, such as its packages and the vehicle dimensions.
- **Submit Game** Submits your game for validation and evaluation. If solution is valid, it returns a *SubmitResponse* with the scoring of your game.
- **Fetch Games** This is to get access to your past games and see your old results. It returns a ArrayList of *FetchResponses* which contains each game,
  gameId, score, map, and the time it was submitted. If it is specific game one can use the gameId as parameter and a specific game will
  be fetched, in this case the solution with the coordinates of the packages will also be fetched and accessable as a String with a Json format.

# API
The definition of the API and what it returns can be found on https://game.considition.com/swagger/index.html, or on https://Considition.com/rules.
To see the visualization of your solution you can either follow the link in the game response or go to https://visualizer.considition.com and enter your gameId

# Scoring
After submitting a game through **Submit game** a validator will check so that the submission is valid, afterwards the score will be calculated according to the following criteria’s:

- **Order score** = How well the packages are placed, in accordance with their priority
- **Weight score** = How many packages that will get crushed while packaging
- **Efficiency score** = How the high the packaging volume efficiency is
- **Length score** = How far the packages reach in the x-axis
  Through trial and error and within the timeframe stated above, you can attempt to package Budbee-trucks as many times as you want to create the best possible algorithm.
  