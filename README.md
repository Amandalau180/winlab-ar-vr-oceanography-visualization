# WINLAB AR VR Oceanography Visualization



## Getting Started
Uses version 2022.3.31f1  
Requires Meta Quest 3 and Meta XR All-in-One SDK found [here](https://assetstore.unity.com/packages/tools/integration/meta-xr-all-in-one-sdk-269657)

## How To Set Up Project
Using Unity Hub, download Unity version 2022.3.31f1 and the Meta XR All-In-One SDK  
Download Project files from GitLab  
Open Project once in Unity by locating the "Unified Project" folder you downloaded  

## Usage
In addition to the in-game help guide, below is some useful information:
- Plotter GameObject and Plot2 Script
    - You can select the specific dataset you want to visualize under the field Dataset_id
    - You can type in specific variables of interest under the field Vars
    - You can change the sampling rate
        - Lower means more definition, but is also slower
        - Default is 5
    - If you change the pre-loaded terrain for worldmap, the map width, length, and height fields must be updated accordingly
    - Checkbox to download new data or use previously downloaded data
    - The maximum interpolation range deefines the maximum length of connections between points
    - If desired, you can load in new gradients by changing the gradient file name to your own file (must be imported separately)

## Additional Features
