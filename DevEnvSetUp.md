#Setting up Xamarin Studio to build an agent library using POSH-sharp under Windows.

This tutorial guides you through the set up of POSH-sharp for behaviour library development. Instead of Xamarin Studio( formerly known as  Monodevelop) it is also possible to use Visual Studio C#.

_If you are using another OS steps should be similar and we try to provide more info on that by time._

## Requirements ##
  * [Xamarin Studio](http://monodevelop.com) (tested with v4)
    * you might need additional software such as GTK# which will be linked to from the Xamarin installer page
  * [mono .NET 3.5](http://www.mono-project.com/Main_Page)
  * a git client; I am using [Git} and [http://www.syntevo.com/smartgithg/ SmartGIT](http://git-scm.com/) as a GUI which is a quite good combination


## Set Up the Sources ##
After installation of the above mentioned software or their alternatives we are now setting up our workplace.
  * Open your project archive (a folder where you normally put your projects in) in our case that's ` D:\Projects `
  * create a new folder for your POSH project; how about "POSH"
  * if you are using just the command line GIT client
    * open a command prompt and cd into your POSH folder:
` cd D:\Projects\POSH `
    * download the latest sources using
` git clone https://code.google.com/p/posh-sharp/ `
  * if you are using SmartGIT (recommended)
    * open menu "Project" -> "Clone ..." ![https://googledrive.com/host/0B_0aVieboMdWSWdIdDdfQkUxcDQ/Smartgit2a.png](https://googledrive.com/host/0B_0aVieboMdWSWdIdDdfQkUxcDQ/Smartgit2a.png)
    * put ` https://code.google.com/p/posh-sharp ` into the Remote GIT URL text field ![https://googledrive.com/host/0B_0aVieboMdWSWdIdDdfQkUxcDQ/Smartgit3.jpg](https://googledrive.com/host/0B_0aVieboMdWSWdIdDdfQkUxcDQ/Smartgit3.jpg)
    * press "Next"
    * Select `GIT` as repository type otherwise it won't work properly as we are using GIT ![https://googledrive.com/host/0B_0aVieboMdWSWdIdDdfQkUxcDQ/Smartgit4a.png](https://googledrive.com/host/0B_0aVieboMdWSWdIdDdfQkUxcDQ/Smartgit4a.png)
    * put ` D:\Projects\POSH ` into the Path field ![https://googledrive.com/host/0B_0aVieboMdWSWdIdDdfQkUxcDQ/Smartgit5a.png](https://googledrive.com/host/0B_0aVieboMdWSWdIdDdfQkUxcDQ/Smartgit5a.png)
    * press "Next"
    * enter a clever name for new new project so that you can remember it; I recommend naming it "POSH" or "POSH-sharp" or "sharp POSH stuff" ;)
    * press "FINISH"
  * you should now have downloaded the latest sources for POSH# and it should look like this ![https://googledrive.com/host/0B_0aVieboMdWSWdIdDdfQkUxcDQ/Smartgit6a.png](https://googledrive.com/host/0B_0aVieboMdWSWdIdDdfQkUxcDQ/Smartgit6a.png)

## Set Up the Environment ##
If you have finished downloading/cloning the repository you are now able to set up your IDE.
  * navigate into your newly cloned source folder (from the steps above) and right click the solution file `POSH-sharp.sln` -> select "Open With ..." and choose Xamarin Studio ![https://googledrive.com/host/0B_0aVieboMdWdVRvV0xCOS0zMzA/Xamarin1.png](https://googledrive.com/host/0B_0aVieboMdWdVRvV0xCOS0zMzA/Xamarin1.png)
    * If there is no option for Xamarin Studio you might check if it installed correctly on your machine.
  * Your project view should look like this presenting you the different projects inside the solution. ![https://googledrive.com/host/0B_0aVieboMdWdVRvV0xCOS0zMzA/Xamarin2.png](https://googledrive.com/host/0B_0aVieboMdWdVRvV0xCOS0zMzA/Xamarin2.png)

### !!!Before starting we should check if everything is set correctly!!! ###

  * Open the options menu for the start-up project _**POSH-sharp** (A Start-up project defines the execution point for a solution.)_. To do so click on the small icon right of the bold project. Then press on options. ![https://googledrive.com/host/0B_0aVieboMdWdVRvV0xCOS0zMzA/Xamarin3a.png](https://googledrive.com/host/0B_0aVieboMdWdVRvV0xCOS0zMzA/Xamarin3a.png)
  * You should see the following screen. Now add the run parameters under RUN -> General and put your parameters into the parameter field, eg. `-v -a=PoshBot.dll POSHBot`. ![https://googledrive.com/host/0B_0aVieboMdWdVRvV0xCOS0zMzA/Xamarin4.png](https://googledrive.com/host/0B_0aVieboMdWdVRvV0xCOS0zMzA/Xamarin4.png)
  * The only thing left is enable external assembly debugging. To do so, in the main window under Tools -> Options you will find a group called Project and the Debugger. Check that {{{Debug project code only;}} is not tickt ![https://googledrive.com/host/0B_0aVieboMdWdVRvV0xCOS0zMzA/Xamarin6.png](https://googledrive.com/host/0B_0aVieboMdWdVRvV0xCOS0zMzA/Xamarin6.png)

# Create your own project or try extending POSHBot! #
  * If you want to work with POSHBot try following page page to get going! [POSHBot](POSHBot.md)
# Enjoy Programming! #