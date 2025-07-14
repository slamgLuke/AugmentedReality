# R.E.P.O. AR

## Version Details

- Unity Version: 6000.0.44f1
- Modules: Android Build Support
- Target: Android
- Project Template: 3D (Built-in Render Pipeline)


## Scripts

El siguiente documento describe el propósito de cada script.

Los códigos se encuentran dentro de la carpeta `Assets/Resources/Scripts`, dentro de estos códigos se encuentran:

`IkFootSolver.cs`: Encargado de la animación procedural del robot.
- `Update()`: Actualiza la posición del pie dado una función seno en el tiempo
- `UpdateFootTarget()`: Calcula la nueva posición del pie utilizando raycasting

`NavMeshHowto.cs`: Encargado de la navegación del robot y de la instanciación de este.
- `HandleTouch()`: Encargado de instanciar el robot si no existe
- `MovePlayer()`: Encargado de mover el robot al punto
- `RotatePlayer()`: Encargado de la rotación del robot

`ObjectSpawner.cs`: Encargado de la instancación de los objetos que empujan al robot.
- `Update()`: Verificar que se tocó la pantalla en el estado correcto para invocar un proyectil y lanzarlo de forma al azar.

`UiManager.cs`: Encargado de la interfaz de usuario:
`Start()`: Añadir los listeners de los eventos
- `ToggleSideMenu()`: Encargado de abrir y cerrar el menú lateral
- `AnimateSideMenu()`: Función de ayuda de la animación
- `SetTouchMode()`: Cambiar a modo touch (posición del robot)
- `SetSpawnMode()`: Cambiar al modo spawn (Instanciación del robot)
- `UpdateModeVisuals()`: Actualizar los visuales del menú lateral
- `HandleResetButton()`: Reiniciar la escena eliminar los actores



`basicBlend.cs`: Encargado del control de la iluminación de la escena con un promedio de la norma euclidiana de la matriz de píxeles de la pantalla.
- `OnCameraFrameReceived()`: Asignar la iluminación de la escena a la norma euclidiana de la imagen del celular.



## Building
(cómo buildear el proyecto)

Pre-requisitos:
- Unity 6000.0.44f1 con todos los módulos de Soporte Android 

1. Clonar el Repositorio
2. Abrir el repositorio en Unity (la primera vez instalará y compilará todas las dependencias, puede demorar)
3. Para Buildear: `File > Build Profiles > Build`

En este último paso se pueden configurar opciones para permitir compatibilidad con más dispositivos, y entre otras cosas dentro de la pestaña `Player Settings`.

Se recomienda ir a `Edit > Project Settings > XR Plug-in Management > Project Validation > Android` y seleccionar Fix All en caso hayan errores o warnings que impidan Buildear el proyecto.
