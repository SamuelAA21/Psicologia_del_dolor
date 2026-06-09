# Entrega técnica - Psicología del Dolor

Estado documentado: 2026-06-09  
Rama de trabajo: `CAP-1-V3`  
Commit documentado: `ac1e616`  
Unity: `6000.4.4f1`

Este documento es la guía rápida para continuar el desarrollo. Para el inventario técnico completo consultar también `CONTEXTO_PROYECTO.md`.

## 1. Estado del repositorio

Al momento de esta entrega:

- La rama local es `CAP-1-V3`.
- La rama está sincronizada con `origin/CAP-1-V3`.
- El commit base `ac1e616` estaba limpio y sincronizado antes de agregar esta documentación.
- `CONTEXTO_PROYECTO.md` y `ENTREGA_DESARROLLADOR.md` deben confirmarse y publicarse como parte de la entrega.
- El Capítulo 1 tiene flujo narrativo, dos minijuegos, menú, audio, inventario, guía, pausa y mascota.
- No existe una suite automatizada consolidada. La validación principal sigue siendo manual en Play Mode.

Antes de trabajar:

```powershell
git switch CAP-1-V3
git pull
git status --short
```

Abrir el proyecto usando exactamente Unity `6000.4.4f1`. Una versión distinta puede reserializar assets de URP, perfiles de volumen y Project Settings.

## 2. Cómo ejecutar

Escena oficial de entrada:

```text
Assets/_Game/Scenes/Bootstrap.unity
```

No iniciar una build final desde `SampleScene`, porque se omiten servicios y el menú principal.

Escenas incluidas en Build Settings:

```text
0 Bootstrap.unity
1 Interfaz.unity
2 FirstMiniGame.unity
3 SampleScene.unity
```

Flujo de arranque:

```text
Bootstrap
  -> Interfaz
  -> JUGAR
  -> SampleScene
```

## 3. Flujo jugable actual

### Puerta 1: Negación

Nodo Yarn inicial:

```text
Puerta1
```

Opciones:

- `Quedarse`: abre el minijuego de respiración.
- `Retirarse`: cierra visualmente la fortaleza, devuelve control y permite reintentar.

Si se elige `Quedarse`:

```text
Puerta1
  -> chapter1_start_door1_breathing
  -> FirstMiniGame
  -> completar 4 ciclos
  -> volver a SampleScene
  -> restaurar posición y rotación del jugador
  -> Puerta1_PostRespiracion
```

Después del minijuego:

- Respuesta positiva: desbloquea `Puerta2`.
- Respuesta de salida: mantiene `Puerta1` reintentable.

Protecciones existentes:

- `Chapter1ProgressState` conserva el resultado entre escenas.
- Se guarda la posición/rotación del jugador antes de cargar el minijuego.
- `PlayerController.TeleportTo` restaura correctamente usando `CharacterController`.
- Puerta 1 cambia al nodo `Puerta1_PostRespiracion` después de completar la respiración para evitar bucles.
- `Chapter1ProgressManager` espera a que `DialogueRunner` esté listo antes de continuar Yarn.

### Puerta 2: Entender o aplicar

Nodo Yarn:

```text
Puerta2
```

Opciones:

- `Sí, suena lógico.`
- `Quiero probarlo en mi vida real.`

Ambas ramas registran su variable Yarn correspondiente y abren el minijuego educativo:

```text
chapter1_start_understand_apply
```

El minijuego muestra 6 tarjetas para clasificar entre:

- `LA ENTIENDO`
- `LA APLICO`

Al completar las 6 tarjetas:

- Se marca `UnderstandApplyCompleted`.
- Se desbloquea `Puerta3`.
- Se devuelve el control al jugador.

### Puerta 3 y cierre

`Puerta3` desbloquea `Estacion4`.

`Estacion4` contiene preguntas narrativas y conduce a `Final`.

La Brújula del Compromiso solo se entrega en:

```text
title: Final
```

La brújula no abre ni representa el minijuego de respiración.

## 4. Minijuego de respiración

Escena:

```text
Assets/_Game/Scenes/FirstMiniGame.unity
```

Objetivo:

- Inhalar: mantener Espacio y subir.
- Sostener: pulsar/soltar suavemente para mantener altura.
- Exhalar: soltar Espacio y bajar.
- Completar 4 ciclos.

Controles internos:

- `PAUSA / REANUDAR`
- `REINICIAR`
- `SALIR`
- `Escape`: pausa solo este minijuego.

Scripts principales:

```text
Assets/_Game/MiniGames/JetpackJoyride/Scripts/BreathingController.cs
Assets/_Game/MiniGames/JetpackJoyride/Scripts/BreathingTherapyGuide.cs
Assets/_Game/MiniGames/JetpackJoyride/Scripts/ObstacleSpawner.cs
Assets/_Game/MiniGames/JetpackJoyride/Scripts/PlayerJetpack.cs
Assets/_Game/MiniGames/JetpackJoyride/Scripts/GameManager.cs
Assets/_Game/Scripts/MiniGames/MiniGameFlowController.cs
Assets/_Game/Scripts/MiniGames/JetPackJoyride/JetpackPreGameOverlay.cs
Assets/_Game/Scripts/MiniGames/JetPackJoyride/JetpackTherapyHud.cs
Assets/_Game/Scripts/MiniGames/JetPackJoyride/JetpackCompletionOverlay.cs
Assets/_Game/Scripts/MiniGames/JetPackJoyride/JetpackRuntimeControls.cs
```

## 5. Minijuego Entender o Aplicar

No usa una escena separada. Se crea como overlay Canvas en `SampleScene`.

Código:

```text
Assets/_Game/Scripts/MiniGames/UnderstandApplyCategory.cs
Assets/_Game/Scripts/MiniGames/UnderstandApplyCard.cs
Assets/_Game/Scripts/MiniGames/UnderstandApplyDeck.cs
Assets/_Game/Scripts/MiniGames/UnderstandApplyMiniGame.cs
Assets/_Game/Scripts/MiniGames/UnderstandApplyDraggableCard.cs
```

Datos editables:

```text
Assets/Resources/MiniGames/UnderstandApply/Chapter1UnderstandApplyDeck.asset
```

El contenido de las tarjetas no está hardcodeado en el runner. Para cambiar o agregar tarjetas, editar el `UnderstandApplyDeck` desde el Inspector.

Categorías:

```csharp
UnderstandApplyCategory.Understand
UnderstandApplyCategory.Apply
```

Interacción:

- Click sobre una categoría.
- Arrastrar la tarjeta a una categoría.
- Respuesta incorrecta no penaliza y permite reintentar.

## 6. Yarn Spinner

Archivos principales:

```text
Assets/_Game/Scripts/Dialogue/IntroLvl.yarn
Assets/_Game/Scripts/Dialogue/StoryLvl.yarnproject
```

Comandos del capítulo:

```text
chapter1_stage
chapter1_reward_unlocked
chapter1_unlock
chapter1_give_compass
chapter1_retreat_exit
chapter1_start_door1_breathing
chapter1_start_understand_apply
```

Implementación de comandos:

```text
Assets/_Game/Scripts/Chapter1/Chapter1EnvironmentController.cs
```

Metadata generada:

```text
ProjectSettings/Packages/dev.yarnspinner/Assembly-CSharp-generated.ysls.json
```

No usar el JSON generado como fuente de verdad. Unity/Yarn Spinner puede regenerarlo. La fuente de verdad son los atributos `[YarnCommand]` en C# y los comandos usados en `.yarn`.

Nota sobre codificación:

- Los archivos están en UTF-8.
- PowerShell puede mostrar `ClÃ­nico` aunque el contenido real esté correcto.
- Confirmar problemas de codificación desde Unity, VS Code en UTF-8 o una lectura UTF-8 real antes de reescribir textos.

## 7. Progreso y estado

Scripts:

```text
Assets/_Game/Scripts/Chapter1/Chapter1EnvironmentController.cs
Assets/_Game/Scripts/Chapter1/Chapter1ProgressManager.cs
Assets/_Game/Scripts/Chapter1/Chapter1ProgressState.cs
```

Responsabilidades:

- `Chapter1EnvironmentController`: puertas, bloqueos, objetivos, comandos Yarn y lanzamiento de minijuegos.
- `Chapter1ProgressManager`: consume resultados de minijuegos y continúa el flujo al volver.
- `Chapter1ProgressState`: estado estático temporal entre escenas.

Importante:

- `Chapter1ProgressState` no es un sistema de guardado permanente.
- El estado se pierde al cerrar la aplicación o reiniciar el dominio.
- Si se requiere guardar partidas, crear un sistema separado y serializable.

## 8. Herramientas de evaluación

En Editor o Development Build, el menú de pausa muestra `EVALUAR`.

Botones disponibles:

- `PUERTA 1`
- `PUERTA 2`
- `PUERTA 3`
- `PREGUNTAS`
- `FINAL`

Archivo:

```text
Assets/_Game/Scripts/Bootstrap/PauseMenuController.cs
```

Este panel está protegido por:

```csharp
#if UNITY_EDITOR || DEVELOPMENT_BUILD
```

No debe aparecer en una build Release.

## 9. Mascota y NavMesh

La escena contiene un `NavMeshSurface`.

Estado esperado del lobo:

```text
useNavMeshForFollow: true
directFollowFallback: false
```

Archivos:

```text
Assets/_Game/Scripts/NPC/Wolf/PetFollowController.cs
Assets/_Game/Scenes/SampleScene.unity
```

Si el lobo no sigue, atraviesa terreno o muestra errores:

1. Abrir `SampleScene`.
2. Localizar el objeto con `NavMeshSurface`.
3. Confirmar que la geometría caminable está incluida.
4. Volver a hornear el NavMesh.
5. Confirmar que lobo y jugador están sobre áreas conectadas.

No activar `directFollowFallback` como solución final: ese modo puede atravesar geometría.

## 10. UI y prioridad visual

Reglas actuales:

- Diálogos deben quedar por encima del inventario y HUD.
- `Escape` no abre pausa durante un diálogo.
- El panel de opciones Yarn fue ampliado para mostrar múltiples opciones.
- El minijuego de respiración excluye el menú global de pausa y usa sus propios controles.

Archivos:

```text
Assets/_Game/Scripts/Dialogue/YarnUiKenneySkin.cs
Assets/_Game/Scripts/Dialogue/DialogueInputController.cs
Assets/_Game/Scripts/Chapter1/Chapter1InventorySystem.cs
Assets/_Game/Scripts/Bootstrap/PauseMenuController.cs
```

## 11. Pruebas manuales obligatorias

Ejecutar desde `Bootstrap.unity`.

### Prueba A: arranque

1. Aparece `Interfaz`.
2. Cursor visible.
3. `JUGAR` abre `SampleScene`.
4. Tutorial contextual visible.
5. WASD, cámara e interacción funcionan.

### Prueba B: Puerta 1 y respiración

1. Interactuar con Puerta 1.
2. Confirmar opciones `Quedarse` y `Retirarse`.
3. Elegir `Quedarse`.
4. Confirmar pantalla previa y botón Play.
5. Probar pausa, reinicio y salida.
6. Completar 4 ciclos.
7. Pulsar Continuar.
8. Confirmar retorno a la misma posición.
9. Confirmar diálogo `Puerta1_PostRespiracion`.
10. Elegir respuesta positiva y comprobar que Puerta 2 se activa.

### Prueba C: Puerta 2

1. Confirmar dos opciones visibles.
2. Elegir cualquiera.
3. Confirmar aparición del overlay `Entender o Aplicar`.
4. Clasificar las 6 tarjetas.
5. Confirmar feedback sin castigo.
6. Confirmar texto final clínico.
7. Pulsar Continuar.
8. Confirmar que Puerta 3 queda activa.

### Prueba D: final

1. Completar Puerta 3.
2. Completar preguntas de Estación 4.
3. Llegar a `Final`.
4. Confirmar entrega de la Brújula.
5. Confirmar que no vuelve a abrirse el minijuego de respiración.

### Prueba E: mascota

1. Caminar por curvas y desniveles.
2. Confirmar que el lobo sigue el NavMesh.
3. Confirmar que no atraviesa decoraciones ni terreno.

## 12. Riesgos conocidos

1. No hay pruebas automatizadas de flujo completo.
2. `Chapter1ProgressState` es memoria temporal, no guardado persistente.
3. `FirstMiniGame` históricamente ha tenido más de un `BreathingController`; validar antes de modificar referencias.
4. El jugador 2D puede conservar `Player2D` y `PlayerJetpack`; evitar que ambos controlen la velocidad vertical.
5. Varias interfaces se construyen por código en runtime. Para trabajo visual intensivo conviene migrarlas gradualmente a prefabs editables.
6. Cambiar de versión de Unity puede generar cambios automáticos en:
   - `Assets/Settings`
   - `ProjectSettings/GraphicsSettings.asset`
   - `ProjectSettings/ProjectSettings.asset`
   - assets URP y Volume Profiles.
7. No confirmar cambios de render/settings sin revisar el diff y probar visualmente.

## 13. Convenciones para continuar

- Mantener assets propios dentro de `Assets/_Game`, salvo recursos existentes que actualmente viven en `Assets/Resources`.
- Usar `GameSceneNames` en vez de strings nuevos para escenas.
- Usar `SceneLoader.LoadSceneSafe` para cambios de escena.
- No editar manualmente archivos `.meta` salvo necesidad justificada.
- No renombrar nodos Yarn, comandos o nombres de objetos buscados por runtime sin actualizar todas sus referencias.
- Mantener los textos Yarn en UTF-8.
- Hacer commits separados para:
  - narrativa,
  - gameplay,
  - UI,
  - render/settings,
  - assets pesados.

## 14. Primeras tareas recomendadas

1. Ejecutar todas las pruebas manuales de la sección 11.
2. Revisar Console y corregir cualquier error de importación/compilación.
3. Crear una build Development y validar el flujo completo.
4. Crear PlayMode tests para progreso de Puerta 1 y Puerta 2.
5. Convertir el overlay `Entender o Aplicar` a prefab editable si el equipo de arte necesita modificarlo frecuentemente.
6. Definir guardado persistente antes de comenzar el Capítulo 2.

## 15. Checklist para transferir el repositorio

Antes de entregar acceso:

1. Confirmar que esta documentación esté incluida en un commit.
2. Confirmar que `CAP-1-V3` esté publicada en el remoto.
3. Informar al nuevo desarrollador que `CAP-1-V3` es la rama de continuidad y `main` está atrasada respecto al Capítulo 1 actual.
4. Dar acceso al repositorio y a cualquier almacenamiento externo usado para assets pesados.
5. No transferir carpetas generadas:
   - `Library`
   - `Temp`
   - `Logs`
   - `Obj`
   - `UserSettings`
6. Sí transferir:
   - `Assets`
   - `Packages`
   - `ProjectSettings`
   - archivos `.meta`
   - `CONTEXTO_PROYECTO.md`
   - `ENTREGA_DESARROLLADOR.md`
7. Confirmar que el nuevo desarrollador puede instalar/descargar el paquete Yarn Spinner declarado por URL Git.
8. Abrir `Bootstrap.unity`, esperar la importación completa y comprobar que Console no tenga errores rojos.
9. No aceptar automáticamente cambios masivos en URP/Project Settings producidos por otra versión de Unity.

Comandos finales sugeridos:

```powershell
git status --short
git add CONTEXTO_PROYECTO.md ENTREGA_DESARROLLADOR.md
git commit -m "Documentar entrega tecnica del Capitulo 1"
git push origin CAP-1-V3
```
