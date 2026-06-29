# AR Shooter Game

An Augmented Reality application for Android, developed in Unity 6 with AR Foundation and ARCore Extensions.  
The project implements an AR game featuring Cloud Anchors, Geospatial API, Image Tracking, AR Light Estimation, and Depth API.

**Test Device:** Nokia 8 / Android 9  

---

## Technologies

| Technology | Version |
|---|---|
| Unity | 6 (6000.4.0b11) |
| AR Foundation | 6.4.3 |
| ARCore Extensions | 1.54.0 |
| Target SDK | Android 9 (API 28) |

---

## Scenes

| Scene | Description |
|---|---|
| `MyARGame` | Main menu — Game, Initialize Anchors, Geospatial |
| `SampleScene` | Main game + Cloud Anchors |
| `GeospatialScene` | Geospatial API — displays a 3D model at a real-world GPS location |

---

## Features

### AR Game
- Place target cubes on AR planes using touch
- Scale objects with swipe gestures
- Shoot bullets with physics (Rigidbody)
- Score system with floating text animation
- Game state management (Start / End / Restart)

### Cloud Anchors
- **Hosting:** Place 2 anchors (RobotKyle + GlossyObject) and upload to Google Cloud
- **Resolving:** Automatic resolve on app start — waits up to 30 seconds
- **PlayerPrefs Fallback:** If Cloud resolve fails, objects are placed using locally stored coordinates

### Geospatial API
- GPS tracking via AREarthManager
- Distance calculation using the Haversine formula
- Spawns Kyle at a target location in Thessaloniki when the user is within 100m
- **Geospatial Anchor Fallback:** If the device does not support Geospatial Anchors, the model is placed 2m in front of the camera

### Image Tracking
- **front_camera:** Detected image → displays live front camera feed
- **score:** Detected image → displays current score panel
- **video:** Detected image → plays an mp4 video
- Timer-based hide (1.5s delay) to handle ARCore tracking flicker

### AR Light Estimation
- Automatically updates the Directional Light based on real-world lighting conditions
- Adjusts brightness, color temperature, and light direction
- Custom HLSL ShadowReceiver shader for casting shadows on AR planes

### Depth API (Occlusion)
- AROcclusionManager for occluding AR objects behind real-world surfaces
- Graceful fallback for devices without Depth API support

---

## Scripts

| Script | Description |
|---|---|
| `CloudAnchorManager.cs` | Cloud Anchor hosting/resolving, PlayerPrefs fallback |
| `GeospatialManager.cs` | GPS tracking, Haversine distance, Geospatial Anchor placement |
| `ImageTrackingManager.cs` | Image tracking, WebCamTexture, VideoPlayer |
| `ARLightEstimation.cs` | AR Light Estimation, Directional Light updates |
| `PlaceObject.cs` | Place objects on AR planes |
| `ScaleObject.cs` | Scale objects with swipe |
| `BulletShooter.cs` | Shoot bullets |
| `BulletCollision.cs` | Collision detection, destroy targets |
| `FloatingText.cs` | Floating UI text animation |
| `ScoreManager.cs` | Score management |
| `GameStateManager.cs` | Game state management |
| `MenuManager.cs` | Scene navigation |
| `SceneLoader.cs` | Load menu scene |
