# AR Shooter Game

An augmented reality shooter game built with Unity and AR Foundation for Android.

## Features

- **Place Objects**: Tap on detected planes to place 3D cubes in AR space
- **Scale Objects**: Select an object and swipe up/down to resize it
- **Shoot Bullets**: Fire physics-based bullets to hit placed objects
- **Score System**: Score increases with each successful hit
- **Image Tracking**: Detects 3 reference images and displays corresponding content
- **Cloud Anchors**: Persistent AR anchors stored in Google Cloud

## Requirements

- Android device with ARCore support
- Android 9.0 (API level 28) or higher
- Google Play Services for AR installed

## How to Play

1. Open the app and select **Game** from the main menu
2. Scan a flat surface until planes are detected
3. Tap on detected planes to place target cubes
4. Select a cube and swipe to scale it
5. Press **Start Game** to begin shooting
6. Press **Shoot** to fire bullets at the cubes
7. Hit as many cubes as possible to increase your score
8. Press **Retry** to restart

## Cloud Anchors Setup

1. From the main menu, press **Initialize Cloud Anchors**
2. Scan a horizontal surface
3. Tap twice to place 2 anchors in the environment
4. Re-enter the game to see the anchors resolved

## Image Tracking

Point the camera at one of the following images:
- **Among Us** → displays front camera feed
- **YouTube Logo** → plays a video
- **Olympic Rings** → displays current score

## Built With

- Unity 6
- AR Foundation 6.4.3
- Google ARCore Extensions
- TextMeshPro
