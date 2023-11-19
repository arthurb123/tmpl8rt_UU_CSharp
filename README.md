# C# Template for ADVGR2023
This project is an implementation of the [originally provided template](https://github.com/jbikker/tmpl8rt_UU) for the Advanced Graphics
course in 2023/2024 in C# using .NET Core.

## How to use
Since this project uses .NET Core and OpenGL it should be able to be run on all platforms (Windows, Mac, Linux).
1. Load the project using either Visual Studio Code or Visual Studio.
2. If using Visual Studio Code: Create a debug launch profile using the "Run and Debug" panel.
3. If using Visual Studio: Simply select either the Debug or Release profile and run the application, keep in mind that the Release profile will give significantly better performance especially if you deny Visual Studio from attaching the profiler.
4. Feast your eyes (if you can handle the subpar framerate).

## Why
I mainly use Linux and while the original project was implemented in C++, it heavily relies on Windows specific calls that were quite hard to translate to Linux. I figured that rewriting the project from scratch in C# using .NET Core would take less time and be a fun challenge, so here it is. While I had to make some changes, I tried to stay as close to the original implementation as possible.

## Words of caution
Because this project is written in C#, it will definitely perform worse than the original implementation. I tried to optimize the project here and there but in the end you can only get so far with doing raytracing on the CPU in C#. There are probably still many improvements possible, but for now this will be it. Running the project in Release mode without any external interference (e.g the Visual Studio profiler), I managed to get ~50 FPS compared to the ~180 FPS I get when running the original C++ project with an AMD Ryzen 5600X processor.
