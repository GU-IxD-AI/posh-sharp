#developing POSHBOT.

Following instructions and information are based on [Setting up POSH sharp](DevEnvSetUp.md)

## BOD / UT ##

Behavior Oriented Design (BOD) is a way to develop AI systems.  It requires a modular behavior library in any OO language, and a version of POSH action selection for that language. POSH-sharp is a version of POSH that is based on .NET C# using [mono](http://www.mono-project.com).  Unreal Tournament 2004 (UT2004) is a commercial game that you can buy quite cheaply on ebay/amazon, since it's a bit old now.

This web page provides information to get you running in POSH using our Unreal Tournament behavior library.  (Note the screenshot is from a Windows system using UT2004 including the latest patch.)

![https://googledrive.com/host/0B_0aVieboMdWR0FaY0YyNS1lbVk/UTDev1.png](https://googledrive.com/host/0B_0aVieboMdWR0FaY0YyNS1lbVk/UTDev1.png)

### Here's what you need to download: ###

  * First, get a (legal, of course) version of Unreal Tournament.
    * We bought our M$ versions off of amazon for about £4 each (be careful of postage charges though). You should get the Unreal Anthology version which includes UT2004. Do not buy the old version of Unreal Tournament from 1999 which does not include 2004! (at least, not for this!)  Our Gamebots interface won't work.
  * If you use Linux, buy a Windows copy and use [CodeWeavers](http://www.codeweavers.com/), which costs money, or [Wine](http://www.winehq.org/) both should work OK depending on your Linux distribution.
  * On Mac OS X, you can use CodeWeavers CrossOver Mac or [WineBottler](http://winebottler.kronenberg.org/), which uses Wine.
  * Next, you need to install something called !Gamebots, which many AI programmers use (that's why we are into UT).
    * We are using a modified version so make sure you use our GameBots2004 _(link at the bottom of the page)_ or know how to modify them manually.
    * If you are using Windows or Wine copy the content of the GameBots2004 archive into your UT2004 directory `([drive]/Unreal Anthology/UT2004`. It should ask you if you want to merge folders under Windows which is OK as we are only adding files to three specific folders.
    * For CodeWeavers: Start up CodeWeavers, click "manage bottles", then click the button "open C drive in finder"
      * Copy the files from the sub directories in the zip directory (once you've unzipped it) into the directories with the same names under drive\_c/Unreal Anthology/UT2004
    * To make it easier to see what's going on with your bot, you might want to use this simplified level file SimpleMaze.
    * Move the map file to the Maps directory under your UT2004 directory.
  * The UT2004 behaviour library comes with our standard POSH distributions([POSH-sharp](http://code.google.com/p/posh-sharp) and [jyPOSH](https://sourceforge.net/projects/jyposh/)).
  * You may also want to download [Abode-star](http://code.google.com/p/abode-star/) to help edit your POSH plans.


### Here's what you need to do to run things: ###

  * Start UT
    * double click on the game (or run it from the programs menu.)
    * If you have any trouble e.g. with graphics, start in safe mode.
    * Go to Settings -> Display
      * disable Fullscreen mode so that you can start and observe your agents
      * Set the resolution to 640x480; if you want to play yourself you can change this later on but reducing the resolution has some impact on the CPU load
    * Go back to the main screen.
    * To run our agent you will need to start a Server. You can do this by pressing the button "Host Game"
    * You will now see a list of available game types. If the setup was correct you can scroll down a bit to see "Custom Game Types" which all start with GameBots.
      * Select the "GameBots CTF Game Type" (CTF == Capture the Flag)
      * Select a map on which your bot will move.
        * For the time being it would be good to start with a simple map like to one we supplied SimpleMaze. The map name is "CTF-BATH-CW3-SMALL" and should be at the end of the list
      * Start a dedicated Server which allows the bot to connect to the game.
      * The window should disappear and a Console should pop up showing the server log.
      * To close/shutdown the server:
        * Do not just close the Console as it will still be running afterwards.
        * click into the console window and type "exit" followed by pressing enter; the console should disappear from your task list as well. If this does not work you need to shutdown the daemon manually.
          * Under Windows you will find a task icon at the right end of the menu bar (Win7 hidden menu). Right click the icon (a small "U") and click "Exit UnrealServer"

  * **Start your BOD bot**
    * If you want to run the bot without setting up the development environment first download the release version linked in references called !POSHBot-debug-release.
    * If you want to compile the latest version of the bot build a bot first by following the instructions below for building a bot.
    * Open a "Command Prompt"/"Terminal" and navigate into your POSH folder
    * if your POSH-sharp folder is in `D:\Projects\POSH-sharp` use {{cd}}} to navigate into that folder
      * if you use the release version you should find a file called `StartMe.bat` which you can now start if the game is already running
      * if you are using the development sources you can hit the run button or go into `/bin/Debug` on the command prompt and start `StartMe.bat` as well
    * fire up your POSH Engine and start your bot
      * the bot should initialize now and you should see an ongoing stream of log data on the command line

**If there is an error check the support section on this website and if there is no entry matching your problem please report your issue!**

  * Watch what your bot is doing
    * open a second instance of UT2004
    * select Join a Game and select the LAN tab
    * you should see an entry showing the map you selected
    * join this map in spectator mode to observe the bot

  * **Modify your bot:**
    * Now follow the [POSH installation instructions](DevEnvSetUp.md), if you have not already done so.
    * As you probably have observed the bot is not the smartest or best player. If you want to change that you can modify either its action plan, or is behaviour library. The action plan is for prioritizing the different actions the bot can take and to allow easy combination of different primitive actions combining them into a more sophisticated behaviour. Modifying the behaviour library is a bit more complicated but allows you to include new actions or lower level approaches such as path-finding, or even learning.
    * start !Xamarin Studio open the !POSHBot project options menu; if you are not sure how have a look at the previous environment instructions on how to access the options menu ![https://googledrive.com/host/0B_0aVieboMdWdVRvV0xCOS0zMzA/Xamarin5.png](https://googledrive.com/host/0B_0aVieboMdWdVRvV0xCOS0zMzA/Xamarin5.png)
    * check if under Build -> Custom Commands those two commands are set:
      * You can create an after build command by clicking into the drop-down list entry and select after build. The two commands we need are there for copying the `POSHBot.dll` and the needed debug library `POSHBot.pdb`into the library folder of the POSH-sharp executable.
      * command1: After build: `${SolutionDir}\copy_win.bat "${TargetFile}" "${SolutionDir}\execute\library\${TargetName}"`
      * command2: After build: `${SolutionDir}\copy_win.bat "${TargetDir}\${ProjectName}.pdb" "${SolutionDir}\execute\bin\${ProjectConfigName}\library\${ProjectName}.pdb"`
      * the working directory for both is `${SolutionDir`}
    * you can now compile the solution by right clicking on it and say build
    * now you can hit the **Debug** button and the project shoudl start and connect to the game and you can happily debug the whole code
    * the action plan can be modified using Abode-star and is located in `[ProjectDir]/library/plans/` and is named _bodbot2.lap_ (check out the Abode website for further instructions on how to use ABODE)
      * the primitives for the behaviour library are in the POSHBot project and are the C# classes `Combat Navigator Status` `POSHBot Movement`. They communicate with the game through the POSHBot class utilizing the `UTBehaviour` superclass which is the control hub of the bot.
      * you can also add multiple bots to the game by modifying the init file which is called `POSHBot_init.txt` and is in `[ProjectDir]/library/init/`
        * the name in the `[ ]` specifies the action plan to load so if you copy below it you create a new bot with identical parameters

### Enjoy experimenting!!! ###


### References ###

[Abode-star](http://code.google.com/p/abode-star/)

[CodeWeavers](http://www.codeweavers.com/)

[GameBots2004](http://sourceforge.net/projects/jyposh/files/GameBotsUT2004.7z) :  Release 5697

[POSHBot-debug-release](https://docs.google.com/file/d/0B_0aVieboMdWVG1JaWF3emNrQmM/edit?usp=sharing)

[SimpleMaze](http://sourceforge.net/projects/jyposh/files/CTF-Bath-CW3-small.ut2)

[Wine](http://www.winehq.org/)

[WineBottler](http://winebottler.kronenberg.org/)


### People Involved ###

[Swen Gaudl](http://bath.academia.edu/SwenGaudl)