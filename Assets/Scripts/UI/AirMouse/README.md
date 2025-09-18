# AirMouse UI System

This modular UI system provides a clean interface for configuring AirMouse input settings without modifying the original `AirMouseInput` script.

## Components

### 1. AirMouseUIManager
The main controller that manages the UI system:
- Handles panel visibility toggle
- Manages apply/reset functionality
- Provides events for external integration
- Links UI panel with AirMouseInput script

### 2. AirMouseUIPanel
The UI panel containing all the settings controls:
- Connection settings (Port name, Baud rate)
- Axis orientation settings (Horizontal/Vertical/Roll axis mapping)
- Flight control settings (Sensitivity, Deadzone, Smoothing)
- Real-time status display
- Input value monitoring

### 3. AirMouseUIPrefabSetup
Helper script for programmatically creating the UI:
- Creates complete UI structure
- Sets up all necessary components
- Links components together
- Can be used to generate UI at runtime

### 4. AirMouseUIExample
Example implementation showing integration:
- Demonstrates how to use the UI system
- Shows event handling
- Provides keyboard shortcuts
- Includes auto-setup functionality

## Usage

### Method 1: Manual Setup
1. Create a Canvas in your scene
2. Add the `AirMouseUIManager` component to a GameObject
3. Assign the `AirMouseInput` reference
4. Create UI elements manually and assign them to the manager

### Method 2: Automatic Setup
1. Add the `AirMouseUIExample` component to any GameObject
2. Enable "Create UI On Start" in the inspector
3. The system will automatically create the complete UI structure

### Method 3: Programmatic Creation
1. Use the `AirMouseUIPrefabSetup` script
2. Call `CreateAirMouseUIPrefab()` method
3. The complete UI will be generated programmatically

## Features

- **Modular Design**: Clean separation of concerns
- **Non-Invasive**: Doesn't modify the original AirMouseInput script
- **Real-time Updates**: Live input value display
- **Event System**: Customizable callbacks for settings changes
- **Keyboard Shortcuts**: Toggle UI with F1 key (configurable)
- **Auto-setup**: Can create UI automatically at runtime
- **Status Monitoring**: Connection status and input value display

## Integration

The UI system integrates seamlessly with the existing `AirMouseInput` script:

```csharp
// Get reference to UI manager
AirMouseUIManager uiManager = FindObjectOfType<AirMouseUIManager>();

// Show/hide the settings panel
uiManager.ShowPanel();
uiManager.HidePanel();

// Apply settings
uiManager.ApplySettings();

// Listen to events
uiManager.OnSettingsApplied += OnSettingsApplied;
uiManager.OnSettingsReset += OnSettingsReset;
```

## Customization

The UI system is designed to be easily customizable:
- Modify colors and styling in the prefab setup
- Add new settings by extending the panel
- Customize layout and positioning
- Add additional validation or constraints

## Requirements

- Unity 2021.3 or later
- TextMeshPro package
- AirMouseInput script (existing)
- UI Canvas in scene

## Notes

- The UI system maintains all original functionality of AirMouseInput
- Settings are applied in real-time when using the UI
- The system is designed to be lightweight and performant
- All UI elements are created programmatically for maximum flexibility
