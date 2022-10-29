# Final Degree Project

## Descriprion
The main purpose of this final degree project is the investigation and testing of the remote control of collaborative robots in a dynamic and inaccessible environment for human beings. In this context, we will focus on the use of devices with high haptic capabilities for their integration into teleoperation tasks, making the performer’s experience simpler and more immersive. 

Thus, the memoire of this work describes, as well as the environment in which it has been thought to apply this technology as a solution, as the associated costs, the way in which the project has been developed and the devices that have been used. Through the chapters in which the report is divided, the technologies used for each specific purpose and the justification for the choice of each of them will also be described, explaining their role within the work carried out.

Likewise, the methodology which the project has been carried out and, in detail, each of the interfaces that we have dealt with to coordinate the activity of the devices will be discussed. We will also clarify the problems that have arisen during the implementation of the project and the solutions adopted, adding the reasoning that has been followed for their implementation. 

On the other hand, we will make an analysis of the requirements that were intended to be covered, to later expose the degree of achievement of the proposed objectives with the tests carried out and the results obtained. 

Finally, we will present a reflection of the carried out work, discussing the solution obtained with a review of the changes in the objectives and priorities of the study, concluding with ideas and comments for future work within this line of research.

## Devices

Touch X                             |Baxter Front             |  Baxter Top          |    Baxter Side                 :         
:-------------------------:|:-------------------------:|:-------------------------:|:-------------------------:
![image](https://user-images.githubusercontent.com/56881598/198829552-c071ef37-c935-4801-860b-52bb7d68876a.png) | ![image](https://user-images.githubusercontent.com/56881598/198829228-b9ec0ea3-6f48-4775-a28c-3a3ef7430a9d.png) | ![image](https://user-images.githubusercontent.com/56881598/198829269-c79f07e5-6ecc-4a23-a7cc-b1ae4947fe26.png) | ![image](https://user-images.githubusercontent.com/56881598/198829276-d0c6cde3-c0c2-498b-bd75-a9cfd17c1c1d.png)



## Links of interest

We list in this section some relevant links in the development of this project that have helped in the development or that are closely related.

### Internal Links

- [**Developed Code:**](https://github.com/XDavid1999/TFG/tree/main/Assets/Scripts) In this folder we find the main scripts used for the [Baxter control](https:/ /github.com/XDavid1999/TFG/blob/main/Assets/Scripts/mapBaxterArticulations.cs) and [haptic feedback] integration(https://github.com/XDavid1999/TFG/blob/main/Assets/Scripts/ SensablePlugin.cs) and import functions for Touch X control.
- [**Memory Latex Project:**](https://github.com/XDavid1999/TFG/tree/main/Memoria) Inside this we will find the compiled pdf file, although we can generate it with the complete project.
- **Measurements Taken:** [Moment of Force](https://github.com/XDavid1999/TFG/tree/main/Measurements%20Moment) when moving objects and [Torque on Collision](https://github. com/XDavid1999/TFG/tree/main/Medidas_Collision) when colliding with simulated objects.

### External links

- [**Baxter SDK:**](https://github.com/RethinkRobotics/baxter) For Baxter control.
- [**URDF-Importer:**](https://github.com/Unity-Technologies/URDF-Importer) For Baxter import and simulation.
- [**ROS-TCP-Connector:**](https://github.com/Unity-Technologies/ROS-TCP-Connector) For the connection between Unity and ROS and thus be able to command Baxter.
- [**3DSystems Plugin:**](https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-5-haptic-plugin-for-geomagic-openhaptics-3-3-hlapi-hdapi-34393) For control and demonstration of haptic device capabilities in Unity.
