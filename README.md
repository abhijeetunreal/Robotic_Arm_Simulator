# Project Documentation: Air Mouse Controlled Robotic Arm Simulator

A comprehensive guide to the hardware, firmware, software, and algorithms used in the project.

### Table of Contents
* [1. Project Overview](#1-project-overview)
  * [1.1. Core Concept](#11-core-concept)
  * [1.2. Key Features](#12-key-features)
* [2. Hardware Setup & Components](#2-hardware-setup--components)
  * [2.1. Components List](#21-components-list)
  * [2.2. Wiring Diagram & Instructions](#22-wiring-diagram--instructions)
  * [2.3. Component Roles](#23-component-roles)
* [3. Arduino Firmware Explained](#3-arduino-firmware-explained)
  * [3.1. Primary Function](#31-primary-function)
  * [3.2. Two-Way Communication Protocol](#32-two-way-communication-protocol)
  * [3.3. Code Breakdown](#33-code-breakdown)
* [4. Unity Project Architecture](#4-unity-project-architecture)
  * [4.1. Core Systems Overview](#41-core-systems-overview)
  * [4.2. Scene & Prefab Setup](#42-scene--prefab-setup)
  * [4.3. Script-by-Script Breakdown](#43-script-by-script-breakdown)
* [5. Key Algorithms & Techniques](#5-key-algorithms--techniques)
  * [5.1. Inverse Kinematics: Cyclic Coordinate Descent (CCD)](#51-inverse-kinematics-cyclic-coordinate-descent-ccd)
  * [5.2. Procedural Audio Synthesis for Motor Sounds](#52-procedural-audio-synthesis-for-motor-sounds)
  * [5.3. Raymarching Shaders for UI and 3D Objects](#53-raymarching-shaders-for-ui-and-3d-objects)
  * [5.4. Event-Driven Architecture for Decoupled Logic](#54-event-driven-architecture-for-decoupled-logic)
* [6. User Interface (UI) System](#6-user-interface-ui-system)
  * [6.1. Main Menu & Connection Panel](#61-main-menu--connection-panel)
  * [6.2. Air Mouse & Haptics Control Panel](#62-air-mouse--haptics-control-panel)
  * [6.3. In-Game HUD](#63-in-game-hud)
* [7. Signal Flow: How It All Works Together](#7-signal-flow-how-it-all-works-together)

---

## 1. Project Overview

### 1.1. Core Concept
This project is an interactive simulation of a robotic arm controlled in 3D space by a custom-built, motion-sensing "air mouse." The user physically moves the air mouse, and the on-screen robotic arm mimics the motion to perform tasks in a training game. The system is designed to be highly modular, allowing for easy customization of the arm's structure, controls, and game mechanics.

### 1.2. Key Features
* **Real-time Motion Control**: The arm's target is controlled by the pitch, yaw, and roll of a physical MPU-6050 sensor.
* **Modular Robotic Arm**: The arm is built using an Inverse Kinematics (IK) system that can support any number of joints and bone lengths.
* **Interactive Training Game**: A game loop where the player picks up and assembles parts at target locations.
* **Haptic Feedback**: A vibration motor provides physical feedback for key game events like picking up and placing objects.
* **Procedural Audio**: Joint movement sounds are generated in real-time, creating a dynamic and realistic effect that syncs perfectly with motion speed.
* **Advanced UI Shaders**: Custom raymarching shaders create a dynamic, glowing "holographic" cube and background that react to the air mouse's rotation.
* **Comprehensive UI Control**: In-game menus allow the user to connect, configure all air mouse and haptic feedback parameters, and view a live data stream.

---

## 2. Hardware Setup & Components

### 2.1. Components List
* Arduino Nano (or compatible board)
* MPU-6050 6-Axis Accelerometer & Gyroscope Module
* Vibration Motor Module (with built-in driver)
* Pushbutton (for optional click input)
* Breadboard and Jumper Wires

### 2.2. Wiring Diagram & Instructions
The components are connected to the Arduino Nano as follows. Power is supplied via the USB connection to the computer.



* **MPU-6050 Sensor:**
    * `VCC` → `5V` on Arduino
    * `GND` → `GND` on Arduino
    * `SCL` → `A5` on Arduino (I2C Clock)
    * `SDA` → `A4` on Arduino (I2C Data)
* **Vibration Motor Module:**
    * `GND` → `GND` on Arduino
    * `VCC` → `5V` on Arduino
    * `IN` → Pin `D9` on Arduino (must be a PWM pin, marked with `~`)
* **Pushbutton (Optional):**
    * One leg → `GND` on Arduino
    * Other leg → Pin `D3` on Arduino

### 2.3. Component Roles
* **Arduino Nano**: The "brain" of the physical controller. Its sole purpose is to read raw data from the MPU-6050, listen for commands from Unity, and send all data to the computer over the USB serial port.
* **MPU-6050**: The motion sensor. It contains a 3-axis accelerometer and a 3-axis gyroscope, providing the rotational data (pitch, roll, yaw) that drives the air mouse.
* **Vibration Motor Module**: The haptic feedback device. The module includes a driver transistor, allowing the Arduino to safely turn the motor on and off with a simple digital signal, and control its intensity with PWM.

---

## 3. Arduino Firmware Explained

### 3.1. Primary Function
The Arduino code is designed to be a "dumb" data forwarder. It performs **no calculations or game logic**. This is a deliberate design choice that makes the hardware universal; all complex logic is handled in Unity, allowing for rapid iteration without ever needing to re-upload code to the Arduino.

### 3.2. Two-Way Communication Protocol
Communication happens over the serial port at a **115200 baud rate**.

* **Arduino to Unity (Sensor Data)**: The Arduino constantly sends a single line of comma-separated values (CSV) representing the full sensor state.
    * **Format**: `ax,ay,az,gx,gy,gz,clickState`
    * `ax, ay, az`: Raw accelerometer data.
    * `gx, gy, gz`: Raw gyroscope data (used for pitch, yaw, roll).
    * `clickState`: `1` if the button is pressed, `0` otherwise.

* **Unity to Arduino (Haptic Commands)**: Unity sends simple string commands to the Arduino to control the vibration motor.
    * **Format**: `V,intensity,duration\n`
    * `V`: The command prefix for **V**ibration.
    * `intensity`: A number from `0` (off) to `255` (full power).
    * `duration`: The time in milliseconds the motor should stay on.
    * `\n`: A newline character to signal the end of the command.

### 3.3. Code Breakdown
* **`setup()`**: Initializes serial communication, connects to the MPU-6050 sensor, and configures the motor pin as an output.
* **`loop()`**:
    1.  **Listen for Commands**: It first checks `Serial.available()` to see if a command has arrived from Unity. If so, it reads the string and parses the intensity and duration.
    2.  **Manage Vibration (Non-Blocking)**: The code uses the `millis()` function for vibration control. When a command is received, it turns the motor on and calculates a future timestamp (`vibrationStopTime`). It then checks on every loop if the current time has passed this timestamp. If it has, it turns the motor off. This avoids using `delay()`, which would halt the program and disrupt the sensor data stream.
    3.  **Read and Send Sensor Data**: It reads the latest data from the MPU-6050 and sends it to the computer in the specified CSV format.

---

## 4. Unity Project Architecture

### 4.1. Core Systems Overview
The Unity project is built on several independent, modular systems that communicate with each other.
* **Input System (`AirMouseInput`)**: Handles all communication with the Arduino.
* **Robotic Arm System (`IKController`)**: Manages the arm's physical simulation.
* **Game Logic (`AssemblyGameManager`)**: Controls the training game rules and state.
* **UI System (Various Scripts)**: Manages all menus, displays, and visual feedback.
* **Sound System (Various Scripts)**: Manages procedural joint audio and event-based sound effects.

### 4.2. Scene & Prefab Setup
* **`IK_System`**: An empty GameObject containing the hierarchy of arm joints. The `IKController` script is attached here.
* **`GameManager`**: An empty GameObject with the `AssemblyGameManager` script.
* **`UI Canvas`**: Contains all UI panels, buttons, and text displays.
* **Prefabs**: `AssemblyPart` and `TargetLocation` prefabs are used to spawn the game objects for each round.

### 4.3. Script-by-Script Breakdown
* **`AirMouseInput.cs`**:
    * **Purpose**: Manages the serial port connection on a separate thread to prevent the game from freezing. Reads incoming sensor data, queues outgoing vibration commands, and provides smoothed input values to other scripts.
    * **Key Logic**: Uses `System.IO.Ports.SerialPort` for communication. A `Thread` reads data in a loop. `ConcurrentQueue` safely handles commands sent from the main game thread. `Vector2.SmoothDamp` provides frame-rate independent smoothing for buttery-smooth controls in both the editor and final builds.

* **`IKController.cs`**:
    * **Purpose**: Implements the Inverse Kinematics algorithm.
    * **Key Logic**: Takes a list of joint `Transforms` and a `Target` transform. In `LateUpdate`, it iteratively adjusts each joint's rotation to make the `EndEffector` reach the `Target`. See the Algorithms section for more detail.

* **`TargetController.cs`**:
    * **Purpose**: Moves the IK target in the scene based on user input.
    * **Key Logic**: Has two modes. In `Mouse` mode, it uses screen-to-world raycasting. In `AirMouse` mode, it reads the smoothed data from `AirMouseInput` and translates its position accordingly. It also implements the movement bounding box.

* **`AssemblyGameManager.cs`**:
    * **Purpose**: The "brain" of the training game. Manages game state, spawning, and win conditions.
    * **Key Logic**: Uses a state machine (`AwaitingPickup`, `AssemblingPart`). Spawns parts and locations at random. The "pickup" mechanic is achieved by parenting the assembly part to the player's target. It uses C# `Action` events to announce key moments (`OnPartPickedUp`, etc.) to other scripts in a decoupled way.

* **`JointSoundController.cs`**:
    * **Purpose**: Generates realistic, procedural motor sounds for a single joint.
    * **Key Logic**: Attached to each joint pivot. It calculates angular speed in `Update()`. In `OnAudioFilterRead()`, it generates a sound wave from scratch, mixing a base waveform (like Sawtooth) with percussive clicks and a low-frequency grind. The pitch and volume are directly controlled by the joint's movement speed.

* **UI Controller Scripts (`MainMenuController`, `AirMouseUIController`, etc.)**:
    * **Purpose**: These scripts act as bridges between the UI elements (sliders, buttons) and the public variables of the core system scripts.
    * **Key Logic**: They read initial values from scripts like `AirMouseInput` to populate the UI. They use `onClick` and `onValueChanged` listeners to call functions that update the variables on the target scripts.

---

## 5. Key Algorithms & Techniques

### 5.1. Inverse Kinematics: Cyclic Coordinate Descent (CCD)
The `IKController` uses CCD, an intuitive iterative algorithm.
1.  The loop starts from the joint closest to the end effector and moves backward toward the base.
2.  For each joint, it creates two vectors: `VectorA` (from the current joint to the end effector) and `VectorB` (from the current joint to the target).
3.  It calculates the rotation needed to align `VectorA` with `VectorB` using `Quaternion.FromToRotation`.
4.  It applies this small rotation to the current joint.
5.  This process repeats for all joints. By running this entire cycle several times per frame, the arm quickly converges on a solution and points at the target.

### 5.2. Procedural Audio Synthesis for Motor Sounds
To create a realistic sound that syncs perfectly with movement, the `JointSoundController` generates audio from code instead of playing a recording.
* **`OnAudioFilterRead(float[] data, int channels)`**: This special Unity function runs on a separate audio thread. It gives the script direct access to the audio buffer (`data`) before it's sent to the speakers.
* **Waveform Generation**: The script generates a base tone by calculating the values of a mathematical function (like `Sine` or `Sawtooth`) over time. The `frequency` of this function determines the pitch.
* **Layer Mixing**: A convincing mechanical sound is created by mixing three layers:
    1.  **Whine**: The base `Sawtooth` waveform mixed with a small amount of random noise.
    2.  **Clicks**: A percussive layer made by generating a burst of loud noise that quickly decays, triggered at a frequency proportional to movement speed.
    3.  **Grind**: A low-frequency sine wave that modulates the amplitude of the whine layer, creating a "wobble" effect.

### 5.3. Raymarching Shaders for UI and 3D Objects
The holographic cube and background effects are created using raymarching, a rendering technique different from standard polygons.
* **Signed Distance Field (SDF)**: The core of the shader is a function `D(p)` that, for any point in space `p`, returns the shortest distance to any object in the scene.
* **Raymarching Loop**: The shader casts a ray from the camera. Instead of checking for triangle intersections, it evaluates the SDF to find the largest "safe" step it can take along the ray. It takes this step and repeats the process. This is much more efficient for rendering complex mathematical shapes.
* **Shader Logic**: The shader calculates the cube's SDF, which is rotated based on the air mouse input. When a ray hits the surface, it calculates reflections of a procedural sky and floor to determine the final color. The background-only version uses the same technique but makes the cube itself invisible, showing only the reflections.

### 5.4. Event-Driven Architecture for Decoupled Logic
The `AssemblyGameManager` uses `public static event Action` to announce game events. Other scripts, like `GameLogUI` and `GameSoundEffects`, "subscribe" to these events.
* **Benefit**: This is a powerful design pattern. The `GameManager` doesn't need to know that a UI or sound script exists. It just shouts "A part was picked up!" into the void. Any script interested in that event can listen for it. This makes the code highly modular and easy to expand—you can add new feedback systems without ever touching the `GameManager` code again.

---

## 6. User Interface (UI) System

### 6.1. Main Menu & Connection Panel
* **Functionality**: Allows the user to input the COM port and baud rate. A "Connect" button attempts to initialize the `AirMouseInput` script. A toggle allows the user to bypass this and use the standard mouse.
* **Feedback**: A status text field provides real-time updates ("Connecting...", "Connected!", "Failed"). Upon successful connection, a raw data display panel appears, showing the live data stream from the Arduino.

### 6.2. Air Mouse & Haptics Control Panel
* **Functionality**: This panel, controlled by `AirMouseUIController`, provides sliders, toggles, and dropdowns to configure every public variable on the `AirMouseInput` and `AssemblyGameManager` scripts.
* **Controls**: Sensitivity, roll sensitivity, deadzone, smoothing, axis mapping, axis inversion, and all haptic feedback intensity/duration values can be tweaked in real-time.

### 6.3. In-Game HUD
* **Targeting Line**: A `LineRenderer` object controlled by `TargetingLineUI`. It draws a line between the player's controller and the current objective (either the part to be picked up or the assembly location). A `TextMeshPro` object at the line's midpoint displays the distance.
* **Game Log**: A `TextMeshPro` text element controlled by `GameLogUI`. It listens for game events and displays status messages ("New Round!", "Part Picked Up!") with a smooth fade-out effect.

---

## 7. Signal Flow: How It All Works Together
This is the end-to-end data flow for a single user action:

1.  The user physically moves the air mouse hardware.
2.  The MPU-6050 sensor detects the change in orientation.
3.  The Arduino's `loop()` reads the new sensor values.
4.  The Arduino formats the data into a CSV string and sends it over the USB serial port.
5.  In Unity, the `AirMouseInput` script's dedicated thread is constantly listening. It receives the CSV string.
6.  The thread parses the string into numerical values for pitch and yaw.
7.  In the main game thread, the `Update()` method in `AirMouseInput` reads these latest values and applies `SmoothDamp` to create a smoothed input vector.
8.  The `TargetController` script reads this final, smoothed vector and updates the 3D position of the `Target` GameObject in the scene.
9.  The `IKController`'s `LateUpdate()` method detects that its `Target` has moved. It runs the CCD algorithm to calculate the new rotations for all arm joints to make the end effector follow the target.
10. As the joints rotate, the `JointSoundController` script on each joint detects the angular speed and generates a procedural motor sound with a corresponding pitch and volume.
11. If the `Target` moves close to an `AssemblyPart`, the `AssemblyGameManager` script detects this, parents the part to the target, and fires the `OnPartPickedUp` event.
12. The `GameSoundEffects` and `GameLogUI` scripts hear this event and play the pickup sound and display the "Part Picked Up!" message, respectively.
13. The `AssemblyGameManager` also calls the `SendVibrationCommand` on the `AirMouseInput` script.
14. `AirMouseInput` queues the command. On its next processing cycle, the serial thread sends the `V,200,150\n` command back to the Arduino.
15. The Arduino's `loop()` receives the command, turns on the vibration motor, and sets a timer to turn it off, providing haptic feedback to the user.
