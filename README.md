# Vuforia-Unity-Introduction
Introductory-level sample project for Vuforia in Unity as a reference on how to start developing Augmented Reality (AR) projects using Vuforia on Unity. Also includes codes to allow testing of tracking within the Unity Editor system.

## Modified DefaultTrackableEventHandler script, CustomTrackableEventHandler.cs
- Replace the DefaultTrackableEventHandler Component on an ImageTarget with CustomTrackableEventHandler.
- Allows for assigning of function callback in the Inspector.

## Simulate tracking behaviour in Unity Editor's Play Mode; webcam or image markers not required!
- Attach EditorCamera Component to ARCamera to allow ARCamera to move within the scene. (If for Vuforia for HoloLens, this is not required.)
- Enable Tracking Without Marker on the selected ImageTarget(s) should be enabled.
- Press *key* to toggle between tracking and off-tracking.
- ImageTargets should be placed separated from one another in the scene to allow testing multiple of targets at once.
