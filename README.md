# TFG

## Descripción del Proyecto

Este repositorio, creado para el desarrollo del Trabajo de Fin de Grado, es contenedor del proyecto desarrollado en Unity para la implementación de la simulación del robot Baxter y el control con el dispositivo háptico Touch X del mismo. Adicionalmente incluimos como respaldo de los archivos en el entorno local y como prueba de su funcionamiento las gráficas e imágenes que se incluyen en la memoria que documenta este trabajo.

## Enlaces de interés

Listamos en esta sección algunos enlaces relevantes en el desarrollo de este proyecto que han servido de ayuda en el desarollo o que están estrachamente relacionados.

### Enlaces Internos

- [**Código Desarrollado:**](https://github.com/XDavid1999/TFG/tree/main/Assets/Scripts) En esta carpeta encontramos los principales scripts utilizados para el [control de Baxter](https://github.com/XDavid1999/TFG/blob/main/Assets/Scripts/mapBaxterArticulations.cs) y la integración del [feedback háptico](https://github.com/XDavid1999/TFG/blob/main/Assets/Scripts/SensablePlugin.cs) e importación de funciones para el control de Touch X.
- [**Proyecto Látex de la Memoria:**](https://github.com/XDavid1999/TFG/tree/main/Memoria) Dentro de este encontraremos el archivo pdf compilado, aunque podremos generarlo con el proyecto completo.
- **Mediciones Tomadas:** [Momento de Fuerza](https://github.com/XDavid1999/TFG/tree/main/Mediciones%20Momento) al mover objetos y [Torque en Colisión](https://github.com/XDavid1999/TFG/tree/main/Medidas_Colision) al chocar contra objetos simulados.

### Enlaces Externos

- [**Baxter SDK:**](https://github.com/RethinkRobotics/baxter) Para el control de Baxter.
- [**URDF-Importer:**](https://github.com/Unity-Technologies/URDF-Importer) Para la importación y simulación de Baxter.
- [**ROS-TCP-Connector:**](https://github.com/Unity-Technologies/ROS-TCP-Connector) Para la conexión entre Unity y ROS y poder comandar así a Baxter. 
- [**Plugin de 3DSystems:**](https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-5-haptic-plugin-for-geomagic-openhaptics-3-3-hlapi-hdapi-34393) Para el control y demostración de las capacidades del dispositivo háptico en Unity.
