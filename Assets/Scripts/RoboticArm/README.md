# Robotic Arm System

A complete Unity robotic arm system with inverse kinematics and air mouse control.

## Features

- **Inverse Kinematics**: Realistic arm movement using CCD (Cyclic Coordinate Descent) algorithm
- **Air Mouse Control**: Control the arm using the existing AirMouseInput system
- **End Effector**: Cube at the end of the arm with roll capability
- **Constraint System**: Joint angle limits and movement boundaries
- **Visual Feedback**: Color-coded end effector and debug visualization
- **Audio Feedback**: Optional sound effects for movement and rotation

## Components

### Core Scripts

1. **InverseKinematics.cs**: Handles the IK calculations for realistic arm movement
2. **RoboticArmController.cs**: Main controller that manages arm behavior and movement limits
3. **EndEffectorController.cs**: Controls the cube at the end of the arm with visual feedback
4. **RoboticArmInputHandler.cs**: Integrates air mouse input with arm controls
5. **RoboticArmSetup.cs**: Helper script for setting up the arm structure
6. **RoboticArmSceneSetup.cs**: Creates a complete scene with ground, lighting, and camera

## Quick Start

### Method 1: Use Scene Setup (Recommended)

1. Create a new scene
2. Add an empty GameObject to the scene
3. Add the `RoboticArmSceneSetup` component
4. Click "Create Complete Scene" in the inspector
5. The system will automatically create everything needed

### Method 2: Manual Setup

1. Create a new scene
2. Create the arm structure:
   - Base joint (stationary)
   - Shoulder joint
   - Elbow joint
   - Wrist joint
   - End effector (cube)
3. Add the `RoboticArmSetup` component to the base joint
4. Assign the joints and end effector
5. Click "Setup Arm" in the inspector

## Input Controls

### Air Mouse Input
- **X-axis**: Left/right movement
- **Y-axis**: Up/down movement
- **Roll**: End effector rotation

### Keyboard Fallback (for testing)
- **WASD**: Movement
- **Mouse Scroll**: Roll control

## Configuration

### RoboticArmController
- `moveSpeed`: How fast the arm moves
- `rotationSpeed`: How fast the arm rotates
- `maxReach`: Maximum reach distance from base
- `minHeight`/`maxHeight`: Vertical movement limits

### InverseKinematics
- `tolerance`: How close to get to target (lower = more precise)
- `maxIterations`: Maximum IK iterations (higher = more accurate)
- `damping`: Stability factor (0.1-0.9 recommended)

### RoboticArmInputHandler
- `inputSensitivity`: How much air mouse input affects movement
- `rollSensitivity`: How much roll affects end effector rotation
- `deadZone`: Minimum input to register movement

## Customization

### Adding More Joints
1. Create additional joint GameObjects
2. Add them to the `armJoints` array in `RoboticArmController`
3. Update the `InverseKinematics` joints array
4. Set appropriate joint constraints

### Changing End Effector
1. Replace the cube with any GameObject
2. Ensure it has a `Renderer` component for visual feedback
3. Add the `EndEffectorController` component

### Modifying Movement Limits
1. Adjust `maxReach`, `minHeight`, and `maxHeight` in `RoboticArmController`
2. Set joint angle constraints in `InverseKinematics`
3. Modify the `ClampTargetPosition()` method for custom limits

## Debug Features

- **Visual Debugging**: Gizmos show arm structure, target position, and movement limits
- **OnGUI Debug**: Real-time display of arm status and input values
- **Console Logging**: Detailed setup and error information

## Troubleshooting

### Arm Not Moving
- Check if `AirMouseInput` is connected and working
- Verify `RoboticArmInputHandler` has all references assigned
- Ensure `enablePositionControl` is enabled

### IK Not Working
- Verify all joints are assigned in the `InverseKinematics` component
- Check that the target (end effector) is assigned
- Increase `maxIterations` if arm doesn't reach target

### End Effector Not Rotating
- Check if `enableRollControl` is enabled in `RoboticArmInputHandler`
- Verify `EndEffectorController` is attached to the end effector
- Ensure `rollSensitivity` is set appropriately

## Performance Tips

- Lower `maxIterations` for better performance (at cost of accuracy)
- Increase `tolerance` for faster convergence
- Use `damping` values between 0.1-0.5 for stability
- Disable debug features in builds

## Future Enhancements

- Collision detection and avoidance
- Path planning and trajectory optimization
- Multiple arm coordination
- Haptic feedback integration
- Advanced constraint systems
- Machine learning-based control

## Dependencies

- Unity 2021.3 or later
- Existing `AirMouseInput` system
- Standard Unity components (Transform, Renderer, AudioSource)

## License

This system is part of the Projekt1 Unity project and follows the same licensing terms.
