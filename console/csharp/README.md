# WHY | C#

## Overview
This code is designed to work in both **Unity** and **C# Console Applications**, utilizing conditional compilation to select the appropriate logging mechanism.

## Setup & Usage

### Running in Unity
1. **Attach the Why component** to any GameObject in the scene.
2. **Run the scene** and check the Unity Console for output.

### Running as a C# Console Application
1. **Create a new C# Console Project** in your preferred IDE.
2. **Copy `Why.cs`** into your project.
3. **Modify `Program.cs`** (or ensure `WhyProgram.Main();` is called).
4. **Build and Run** the project using:
    ```sh
    dotnet run
    ```
5. **Observe the output** in the console.

## Compilation Instructions

### Unity
- Ensure `Why.cs` is added to the Unity project.
- Unity will automatically compile the script.

### C# Console Application
- Compile using the .NET CLI:
    ```sh
    dotnet build
    ```
- Or use `csc` (if using the .NET Framework):
    ```sh
    csc Why.cs /unsafe
    ```
- Run the compiled executable.