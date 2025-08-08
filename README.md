# NC-UI Project


[06.08.2025]

This GitHub contains every file used to develop the software to run the Neural Coupling experiments. The app should allow the experimenter to enter select the patient's profile, start an experiment session, test the necessary sensors, run the MVC separately, select the settings of individual tasks trials and run them one by one before saving them to the patient's folder.

## Language Used

The experiment runs as a **Unity app**, using **scripts** written in **C#**. Notable libraries added to the basic **Unity** and **System** are:
- **LabJack**, to communicate with the LabJack T7 device for data acquisition 
- **XCharts**, for graph representation and visualisation of MVC
- **WaveplusLab**/**WavePlus** to communicate with the data acquisition system for EMG and IMU 

## Structure 

The experiment flow is described in a [Canvas](https://www.canva.com/design/DAGmAEnRNlI/2JzD3tzABZG7PGdzBqqk2g/edit?utm_content=DAGmAEnRNlI&utm_campaign=designshare&utm_medium=link2&utm_source=sharebutton). It is subdivided into Unity scenes: 
- Patient Selection
- Trial Settings
- MVC measurement
- Trial Run

Typically, a **patient** comes to the lab for one **session** made of numerous measures during successive tasks here called "**trials**". Each session normally comprises a single **MVC measurement**, which can be skipped if the MVC value exists from a previous session and further measurement is deemed unnecessary. 

## Test Files and Real Files

This particular GitHub project is a work in progress and as such contains both final and testing files. It contains the final project scenes (in Assets\Scenes\Real Project), optimized for experiment flow and use, and the test scenes (in Assets\Scenes\Testing), optimized to test all parameters and possible situations that the code may have to endure. 

## Code Explained

**AltOpenCloseWindow.cs** 

Seeing how many different UI elements we had to show or hide as a block in the UI, even in the same scenes, I wanted to find an efficient way to animate the UI, that could be intuitive to reproduce or modify if elements were to be changed at any moment.

I wanted to keep the amount of external packages to a minimum and so ended up electing not to use Tweens Animation. 

I found tutorials from youtuber @ChristinaCreatesGames, describing how she animated canvas individually and separated the UI elements by giving them each their own canvas object in the scene. This allows to use a script that activates/deactivates every element in the canvas and slides it around as a whole, attach it to the "window", and call "open" or "close" from any button in the UI, with minimal effort to set up the animation (once the script is set up). 

I reused some of their code and adapted it to the project, removing features I was not interested in such as previsualisation of the animation place, and modifying it to be more intuitive to me at least when manipulating the animation variables from the inspector. 

All this code works solely on Unity library and System. 

To set-up a canvas with this system: 
- Add an image element ("Window" in this project) to the canvas
- Add the Canvas Group component to Window
- Put all the Canvas elements under Window
- Add the AltOpenCloseWindow.cs script to the Canvas

You can now call the "open" or "close" or "toggleOpenClose" methods from any button in the UI by giving the Canvas to the OnClick function.
The opening and closing animations parameters can be modified from the inspector, and the methods can be called using the Context Menu. 

**Overseer**

The Overseer object containing the Overseer.cs script is the vessel for all the information necessary throughout the whole experiment flow. It contains: 
- The patient's information
- The various paths to the patient's files and their syntax definition (name of the data folder for example) which can be modified in the inspector.
- The current Trial Settings (cycles max number, cycle duration, stimulation intensity, etc.)
- The MVC Value
- The LabJack Object

The Overseer object is not destroyed when loading a new scene and allows the relevant information to be communicated between scenes. 

**LabJackObject**

The LabJack device is used both during the MVC Measurement and during the tasks trials to get the torque sensor values. 

To do so in Unity, I created a class LabJackObject, that is an attribute of Overseer as there should only be one LabJack Object connected at all times. The class incorporates methods to connect disconnect and configure the LabJack device, but also to start a ReadLoop. 

Unity updates at around 60Hz normally, but we needed a stable frequency of acquisition for the experiment: The solution adopted here was to build a secondary Thread to run the acquisition on. 
On this Thread, at 100Hz for now, the system asks the LabJack for data at a fixed interval. The LabJack also knows if for some reason one or more intervals have been skipped between the effective updates, and informs us about it. 

To communicate between this thread and the Unity main thread, the data acquired is stocked in a large array in the LabJackObject, and the latest data position is noted as a simple int. This array can then be accessed by the rest of Unity: for example in MVC measurement, the MVC Manager successively calls to check if there is new data since the last call, and adds any new data to the graph visualisation serie in Unity. 

At the time of writing, the ReadLoop runs as long as two bools are true: isRunning and timerIsRunning. 

TimerIsRunning in particular is checked to run a timer inside Unity main thread: in the Overseer Update function, if LJM.timerIsRunning, the LJM.timeReadingSec is updated with deltaTime, and if that time spent reading > maxTimeReadingLoopSec then the timer is stopped with timerIsRunning = false which stops the loop in the secondary thread. 

isRunning is set as true when the readLoop starts, and calling the StopRecording method in Unity puts isRunning to false.  

There may be more efficient ways to obtain these results, but for now controlling the timer based on Unity time is practical, and there has not been any bug on this side of the app.

**XCharts** 

Xcharts is an open source library that allows graph visualisation in Unity. It has proven lightweight and allowed us to visualise torque data in real time at 100Hz of acquisition frequency. 

The way it is used in this project is simple: there is a **chart** object that contains the **series** of data to display in the form of **line graphs**. The MVC Manager gets the data from LabJack and stores it in the series. There has been (many) iterations and tries on how to make the graphs appear fluidly, the documentation of XCharts being just a bit scarce in some areas, and some inherited methods working better than others in my experience. 

For now, the data is added to the serie by a custom method AddTorqueDataPoint that details what constitutes the value of interest.  The X and Y axis are updated by custom methods that decide of the max window size and allow smooth sliding of said window. The Clipping method used is the one furnished by XCharts to not show data outside of the window. 

The **Series** in **MVC** come with a **Markline** object: a line visualising the calculated **MVC Value** of the correspondant measurement data. It should be possible to swap between series to visualise one specific or all the data at once when desired. 
