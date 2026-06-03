# Contexto del proyecto: Psicologia_del_dolor

Actualizado el 2026-06-02 para reflejar el estado real del repositorio local en `D:\Psicologia_del_dolor`.

Este documento es la referencia base antes de desarrollar nuevas funciones. Debe mantenerse sincronizado cuando cambien escenas, sistemas globales, dialogos, minijuegos, UI, audio o progresion.

## 1. Resumen ejecutivo

`Psicologia_del_dolor` es una experiencia interactiva en Unity sobre psicologia del dolor. El juego combina:

- Menu inicial con fondo propio, botones y cursor funcional.
- Escena 3D principal con exploracion, jugador, puertas narrativas, lobo/mascota, dialogos Yarn y guia contextual.
- Sistema de progreso del capitulo 1 con puertas, objetivos, indicadores visuales, brujula/inventario y recompensas.
- Dialogos seleccionables por mouse y avanzables con Enter, sin depender de Escape.
- Menu de pausa con control de volumen de musica.
- Audio manager global con loop ambiental y alternancia temporal de musica.
- Minijuego 2D tipo Jetpack Joyride enfocado en respiracion consciente.

Unity:

- Version: `6000.4.4f1`.
- Render pipeline: URP `17.4.0`.
- Input: New Input System `1.19.0`.
- Dialogos: Yarn Spinner Unity desde GitHub.
- Navegacion: `com.unity.ai.navigation`.
- UI: UGUI y TextMesh Pro.

## 2. Flujo oficial actual

La entrada recomendada es:

```text
Assets/Scenes/Bootstrap.unity
```

Flujo real:

```text
Bootstrap
  -> Bootstrapper
  -> SceneLoader
  -> Interfaz
  -> MainMenuController
  -> boton JUGAR
  -> SampleScene
  -> exploracion/dialogos/puertas/progreso
  -> MiniGameBridge o SceneLoader
  -> FirstMiniGame
  -> completar respiracion
  -> pantalla final editable del minijuego
  -> SampleScene
```

Escenas en Build Settings:

```text
0. Assets/Scenes/Bootstrap.unity
1. Assets/Scenes/Interfaz.unity
2. Assets/Scenes/FirstMiniGame.unity
3. Assets/Scenes/SampleScene.unity
```

## 3. Escenas

### `Bootstrap.unity`

Escena de arranque. Debe mantenerse como primera escena del build.

Sistemas principales:

- `Bootstrapper`: carga la primera escena configurada.
- `SceneLoader`: singleton global de carga de escenas, con fade/pantalla de carga.
- `GameAudioManager`: se auto-instala antes de cargar escenas.

Estado actual esperado:

- `Bootstrapper` carga `Interfaz`, no `SampleScene` directamente.
- `SceneLoader` persiste con `DontDestroyOnLoad`.

### `Interfaz.unity`

Menu inicial actual del juego.

Assets relacionados:

```text
Assets/Interfaces/FondoInterfaz.png
Assets/Interfaces/BOTON JUGAR.png
Assets/Interfaces/BOTON INSTRUCCIONES.png
Assets/Interfaces/VIDEO INTERFAZ.mp4
```

Sistema:

- `MainMenuController` se auto-instala en la escena.
- Corrige cursor visible/desbloqueado.
- Conecta boton `JUGAR` para cargar `SampleScene`.
- Conecta boton `SALIR`.
- Usa `FondoInterfaz.png` como fondo principal.
- Asegura `EventSystem` si falta.

### `SampleScene.unity`

Escena principal jugable 3D.

Contiene:

- Entorno del capitulo 1.
- Jugador 3D.
- Lobo/mascota.
- Sistema de dialogos Yarn.
- Puertas narrativas.
- Sistema de progreso.
- Inventario simple.
- Guia de proximidad/objetivos.
- Menu de pausa.
- Tutorial contextual.
- Audio ambiental.
- Puente al minijuego.

Sistemas auto-instalables o conectados:

- `Chapter1EnvironmentController`
- `Chapter1ProgressManager`
- `Chapter1GuidanceController`
- `Chapter1InventorySystem`
- `DialogueInputController`
- `DialoguePlayerControlLock`
- `ContextualTutorialController`
- `BrightEnvironmentController`
- `PauseMenuController`
- `GameAudioManager`

### `FirstMiniGame.unity`

Minijuego 2D tipo Jetpack Joyride terapeutico.

Objetivo:

- Regular altura segun fases de respiracion.
- Mantenerse dentro de la zona objetivo.
- Completar ciclos de respiracion.
- Evitar obstaculos.

Sistemas:

- `BreathingController`: controla fases Inhale, Hold, Exhale.
- `BreathingTherapyGuide`: calcula si el jugador esta en zona y su `RegulationScore`.
- `ObstacleSpawner`: genera corredores segun respiracion.
- `PlayerJetpack`: controla ascenso/descenso.
- `PlayerCollision`: registra errores por choque.
- `GameManager`: controla estado, intentos y retorno a `SampleScene`.
- `MiniGameFlowController`: escucha victoria y reporta progreso.
- `JetpackPreGameOverlay`: pantalla previa con instrucciones y boton `PLAY`.
- `JetpackTherapyHud`: HUD automatico del minijuego.
- `JetpackCompletionOverlay`: pantalla final al completar los 4 ciclos, con resumen y recompensas.
- `BreathingTargetZoneVisualizer`: franja visual de zona correcta.
- `ObstacleSpawner` compensa el tiempo de viaje del obstaculo para abrir el corredor donde el jugador estara cuando ese obstaculo llegue.
- Las interfaces del minijuego se pueden editar como prefabs en `Assets/Resources/UI`.

Estado importante:

- El minijuego ya no debe fallar al primer choque. `GameManager` maneja intentos, por defecto `3`.
- Tras un golpe, `PlayerCollision` da invulnerabilidad breve y feedback visual.
- `ObstacleSpawner` intenta usar el `BreathingController` expuesto por `MiniGameFlowController` para evitar referencias equivocadas.
- La escena conserva un `BreathingController` duplicado en `Spawner`, pero esta desactivado y no arranca. El controlador activo es el del `GameManager`.
- La calibracion actual busca un patron terapeutico: mantener Espacio para inhalar/subir, pulsar suave para sostener, soltar para exhalar/bajar.
- El minijuego inicia congelado con pantalla previa; el ciclo de respiracion arranca al pulsar `PLAY`.
- Al completar los 4 ciclos, el juego muestra una pantalla de finalizacion antes de volver a `SampleScene`.

## 3.1 Objetos reales y arquitectura de escenas

Esta seccion baja el contexto a objetos concretos de Unity. Sirve para saber que existe en escena y que sistemas lo controlan.

### Objetos en `Bootstrap.unity`

Objeto raiz:

```text
AppRoot
```

Componentes/sistemas:

- `Bootstrapper`
  - `firstScene: Interfaz`
- `SceneLoader`
  - Singleton persistente.
  - Crea pantalla/fade de carga.

Arquitectura:

- `Bootstrap` no contiene gameplay.
- Su unica responsabilidad es crear los servicios iniciales y cargar la primera escena jugable/interfaz.
- `SceneLoader` queda vivo entre escenas por `DontDestroyOnLoad`.

### Objetos en `Interfaz.unity`

Objetos principales detectados:

```text
MainMenu
Root
Canvas
RawImage
Play
Instructions
EventSystem
Camera
```

Componentes relevantes:

- `Canvas`: UI principal del menu.
- `RawImage`: fondo visual, actualmente controlado para usar `FondoInterfaz.png`.
- `Play`: boton de inicio.
- `Instructions`: boton visual de instrucciones.
- `EventSystem`: input/clicks de UI.
- `Camera`: render de la escena de menu.
- `MainMenuController`: se auto-instala si la escena activa es `Interfaz`.

Arquitectura:

- La escena puede funcionar aunque algunos eventos de botones no esten configurados manualmente, porque `MainMenuController` busca los botones por nombre y los conecta.
- El cursor se fuerza visible/desbloqueado en el menu.
- El boton `JUGAR` carga `SampleScene` usando `SceneLoader`.

### Objetos en `SampleScene.unity`

Objetos/sistemas principales detectados:

```text
Root
Camera
Canvas
Line Presenter
Options Presenter
Continue Button
Button Container
Character Name
Last Line
YarnCommands
MinigameBridge
MiniGameStarter
Chapter1_Placeholders
BrujulaDelCompromiso
Estacion4_Preguntas
Progreso_1_Retirada
Progreso_2_Entender
Progreso_3_Compromiso
Progreso_4_Preguntas
Progreso_5_Brujula
RewardLight
GuidanceLight
DoorLight 2
DoorLight 2 (1)
DoorLight 3
Prompt
LockedPrompt
InteractionTrigger
Lobito
Player
```

Tambien hay multiples objetos genericos `GameObject`, luces y elementos de entorno importado. Varios de ellos son placeholders visuales o piezas del mapa.

Componentes/sistemas por grupo:

- Dialogo:
  - `DialogueRunner` desde Yarn Spinner.
  - `Line Presenter`.
  - `Options Presenter`.
  - `Continue Button`.
  - `YarnCommands` con `YarnSceneCommands`.
  - `DialogueInputController` auto-instalado.
  - `DialoguePlayerControlLock`.
- Progreso capitulo 1:
  - `Chapter1EnvironmentController`.
  - `Chapter1ProgressManager`.
  - `Chapter1ProgressState`.
  - `Chapter1GuidanceController`.
  - `Chapter1InventorySystem`.
  - `RewardVisualController`.
  - `Chapter1RuntimePlaceholderSpawner`.
- Puertas/interaccion:
  - `NarrativeInteractable`.
  - `InteractionTrigger`.
  - `Prompt`.
  - `LockedPrompt`.
  - Luces/indicadores (`GuidanceLight`, `DoorLight`, `RewardLight`).
- Minijuego:
  - `MinigameBridge`
    - `miniGameSceneName: FirstMiniGame`
  - `MiniGameStarter` existe en escena, pero debe revisarse si todavia es necesario.
- Jugador:
  - `PlayerController`.
  - `CharacterController`.
  - Animator del jugador.
- Mascota:
  - `PetFollowController`.
  - `NavMeshAgent`.
  - Animator del lobo.

Arquitectura:

- `SampleScene` es el nucleo del juego 3D.
- La escena depende de Yarn para narrativa, pero la progresion del capitulo se centraliza en scripts `Chapter1`.
- Los comandos Yarn no deberian cambiar escenas directamente salvo mediante `SceneLoader` o puentes como `MiniGameBridge`.
- El inventario, guia y tutorial deben respetar prioridad visual de dialogos.
- Los sistemas auto-instalables se activan por nombre de escena y evitan depender de que el editor tenga todos los objetos manualmente configurados.

### Objetos en `FirstMiniGame.unity`

Objetos principales detectados:

```text
Player
BreathingTherapyGuide
Root
Directional Light
Fondo_1
Fondo_2
Spawner
GameManager
Main Camera
```

Componentes del `Player`:

- `Rigidbody2D`.
- `Player2D`.
- `PlayerJetpack`.
- `PlayerCollision`.
- `PlayerState`.
- Sprites de ascenso/normal/descenso.

Componentes de `BreathingTherapyGuide`:

- Referencia al `BreathingController` del `GameManager`.
- Referencia al transform del jugador.
- Calcula:
  - `IsPlayerInTargetZone`.
  - `RegulationScore`.
  - `CurrentInstruction`.

Componentes del `Spawner`:

- `ObstacleSpawner`
  - `obstaclePrefab`: `Assets/JetpackJoyrideTemplate/Prefab/Obstacle.prefab`
  - `breathingController`: asignado al `BreathingController` del `GameManager`.
  - `playerTransform`: asignado al jugador.
  - `compensateObstacleTravelTime: true`.
  - `spawnInterval: 0.45`.
  - `corridorHeight: 2.8`.
- El `Spawner` conserva otro `BreathingController` desactivado por compatibilidad de escena; no debe usarse como fuente de verdad.

Componentes del `GameManager`:

- `GameManager`
  - `returnSceneName: SampleScene`
  - `maxMistakes: 3` por defecto en script.
- `BreathingController`
  - Secuencia respiratoria serializada.
  - `playOnStart: false`; `JetpackPreGameOverlay` inicia el ciclo al pulsar `PLAY`.
  - Inhala: `4s`, rango alto `1.45..2.45`, instruccion `Manten Espacio: sube lento.`
  - Sosten: `3s`, rango alto `1.45..2.45`, instruccion `Pulsa suave: manten altura.`
  - Exhala: `4s`, rango bajo `-0.55..0.45`, instruccion `Suelta Espacio: baja lento.`
- `MiniGameFlowController`
  - `returnSceneName: SampleScene`.
  - Expone `Controller` para que otros sistemas usen el mismo controlador.

Objetos/sistemas auto-instalados al cargar la escena:

- `JetpackPreGameOverlay`.
- `JetpackTherapyHud`.
- `JetpackCompletionOverlay`.
- `BreathingTargetZoneVisualizer`.

Prefabs editables esperados:

```text
Assets/Resources/UI/JetpackPreGameOverlay.prefab
Assets/Resources/UI/JetpackTherapyHud.prefab
Assets/Resources/UI/JetpackCompletionOverlay.prefab
```

Nombres criticos dentro de `JetpackCompletionOverlay.prefab`:

```text
ContinueButton
Summary
Rewards
```

Estos objetos pueden moverse, redisenarse o cambiar su contenido visual, pero no deben renombrarse porque el script los busca por nombre para conectar el boton y actualizar los textos.

Si no existen, Unity los crea automaticamente mediante:

```text
Assets/Editor/JetpackUiPrefabCreator.cs
```

Tambien pueden regenerarse desde el menu:

```text
Tools/Psicologia del Dolor/Regenerar UI Jetpack editable
```

Arquitectura:

- `BreathingController` es el reloj del minijuego.
- `BreathingController.GetTherapeuticCenterY(offset)` permite predecir la posicion futura de la trayectoria respiratoria.
- `BreathingTherapyGuide` interpreta si el jugador esta cumpliendo la fase actual.
- `ObstacleSpawner` genera obstaculos dejando un corredor alrededor de la zona terapeutica.
- `ObstacleSpawner` usa la velocidad del prefab `MoveLeft` y la distancia al jugador para compensar el retraso entre spawn y llegada.
- `GameManager` decide si se puede jugar, cuantos errores quedan y cuando se pierde.
- `MiniGameFlowController` decide la victoria y reporta el resultado al capitulo 1.
- `JetpackPreGameOverlay` congela el minijuego al entrar, muestra instrucciones y arranca `BreathingController.StartCycle()` al pulsar `PLAY`.
- `JetpackTherapyHud` transforma el estado interno en feedback legible.
- `JetpackCompletionOverlay` congela la escena al terminar, explica la practica completada, muestra objetos/avances conseguidos y solo vuelve a `SampleScene` al pulsar `CONTINUAR`.
- `JetpackPreGameOverlay`, `JetpackTherapyHud` y `JetpackCompletionOverlay` intentan cargar primero sus prefabs desde `Resources/UI`; si no existen, construyen una UI fallback por codigo.
- `BreathingTargetZoneVisualizer` transforma el rango objetivo en una franja visual del mundo.

## 3.2 Arquitectura tecnica por capas

El proyecto actualmente funciona por capas practicas:

```text
Servicios globales
  SceneLoader
  GameAudioManager

Escenas
  Bootstrap
  Interfaz
  SampleScene
  FirstMiniGame

UI global/runtime
  MainMenuController
  PauseMenuController
  ContextualTutorialController
  DialogueInputController

Narrativa
  Yarn Spinner
  IntroLvl.yarn
  YarnSceneCommands
  MiniGameBridge

Capitulo 1
  Chapter1EnvironmentController
  Chapter1ProgressManager
  Chapter1ProgressState
  Chapter1GuidanceController
  Chapter1InventorySystem

Gameplay 3D
  PlayerController
  NarrativeInteractable
  PetFollowController
  CharacterRouteController

Minijuego 2D
  BreathingController
  BreathingTherapyGuide
  PlayerJetpack
  MoveLeft
  ObstacleSpawner
  GameManager
  MiniGameFlowController
  JetpackTherapyHud
  BreathingTargetZoneVisualizer
```

### Flujo de dependencias

```text
Bootstrapper
  -> SceneLoader

MainMenuController
  -> SceneLoader

Yarn / DialogueRunner
  -> YarnSceneCommands
  -> MiniGameBridge
  -> SceneLoader

Chapter1EnvironmentController
  -> Chapter1ProgressManager
  -> Chapter1InventorySystem
  -> Chapter1GuidanceController

FirstMiniGame
  -> BreathingController
  -> BreathingTherapyGuide
  -> ObstacleSpawner
  -> JetpackTherapyHud
  -> BreathingTargetZoneVisualizer

PlayerCollision
  -> GameManager.RegisterMistake()
  -> GameManager.GameOver()
  -> Chapter1ProgressState.ReportBreathingFailed()
  -> SceneLoader.LoadSceneSafe("SampleScene")

BreathingController.SessionCompleted
  -> MiniGameFlowController.OnWin()
  -> Chapter1ProgressState.ReportBreathingCompleted()
  -> SceneLoader.LoadSceneSafe("SampleScene")
```

### Patrones tecnicos usados

- Singletons:
  - `SceneLoader.Instance`
  - `GameAudioManager.Instance`
  - `GameManager.Instance`
  - `Chapter1ProgressManager.Instance`
  - `Chapter1InventorySystem.Instance`
  - `Chapter1EnvironmentController.Instance`
- Auto-instalacion por escena:
  - `MainMenuController`
  - `PauseMenuController`
  - `ContextualTutorialController`
  - `BrightEnvironmentController`
  - `DialogueInputController`
  - `YarnUiKenneySkin`
  - `Chapter1RuntimePlaceholderSpawner`
  - `JetpackTherapyHud`
  - `BreathingTargetZoneVisualizer`
- Eventos:
  - `BreathingController.PhaseChanged`
  - `BreathingController.SessionCompleted`
- Comandos Yarn:
  - `load_scene`
  - `iniciar_respiracion`
  - `chapter1_stage`
  - `chapter1_reward_unlocked`
  - `chapter1_unlock`
  - `chapter1_give_compass`
- Persistencia simple:
  - PlayerPrefs para volumen.
  - PlayerPrefs para tutorial contextual mostrado.
- Carga de recursos:
  - `Resources.Load` para audio, modelos Kenney y algunos assets UI.

## 4. Sistemas globales

### `SceneLoader`

Archivo:

```text
Assets/Scripts/Bootstrap/SceneLoader.cs
```

Responsabilidad:

- Cargar escenas por nombre.
- Mantener singleton global.
- Mostrar fade/pantalla de carga con frases.
- Proveer metodos estaticos seguros.

Metodos clave:

```csharp
LoadScene(string sceneName)
LoadMainScene()
LoadSceneSafe(string sceneName)
LoadMainSceneSafe()
```

Uso recomendado:

- Para cambios de escena desde codigo: `SceneLoader.LoadSceneSafe("NombreEscena")`.
- Desde Yarn: usar comandos registrados.

### `GameAudioManager`

Archivo:

```text
Assets/Scripts/Bootstrap/GameAudioManager.cs
```

Responsabilidad:

- Singleton global de audio.
- Reproducir musica/ambiente por escena.
- Guardar volumen en PlayerPrefs.
- Exponer control de volumen al menu de pausa.

Audio actual:

```text
Assets/Resources/AmbientalLoop.mp3
Assets/Resources/Audio/GameplayMusic.mp3
```

Comportamiento actual:

- En gameplay reproduce `AmbientalLoop`.
- Despues de 60 segundos, reproduce `Audio/GameplayMusic` durante 45 segundos.
- Luego vuelve a `AmbientalLoop` y repite el ciclo.
- El volumen puede subir/bajar desde el menu de pausa.

### `PauseMenuController`

Archivo:

```text
Assets/Scripts/Bootstrap/PauseMenuController.cs
```

Responsabilidad:

- Auto-instalar menu de pausa en escenas jugables.
- Abrir/cerrar con Escape.
- Mostrar cursor al pausar.
- Reanudar juego.
- Volver a menu principal.
- Salir.
- Bajar/subir volumen de musica.

Regla importante:

- Si Yarn esta ejecutando dialogo, Escape no abre pausa. Esto evita conflicto con dialogos.

### `MainMenuController`

Archivo:

```text
Assets/Scripts/Bootstrap/MainMenuController.cs
```

Responsabilidad:

- Controlar `Interfaz`.
- Forzar cursor visible.
- Asegurar que los botones respondan.
- Usar el fondo correcto.
- Cargar `SampleScene` desde `JUGAR`.

### `ContextualTutorialController`

Archivo:

```text
Assets/Scripts/Bootstrap/ContextualTutorialController.cs
```

Responsabilidad:

- Mostrar tutorial contextual al entrar a `SampleScene`.
- Persistir si ya fue mostrado con PlayerPrefs.
- No interferir con dialogos.

Clave PlayerPrefs:

```text
Chapter1_ContextualTutorial_Shown
```

### `BrightEnvironmentController`

Archivo:

```text
Assets/Scripts/Bootstrap/BrightEnvironmentController.cs
```

Responsabilidad:

- Mejorar la iluminacion/cielo de `SampleScene`.
- Crear o ajustar luz direccional.
- Evitar ambiente oscuro/deprimente.

## 5. Dialogos Yarn

Archivos:

```text
Assets/Scripts/Dialogue/IntroLvl.yarn
Assets/Scripts/Dialogue/StoryLvl.yarnproject
```

Sistemas relacionados:

- `YarnSceneCommands.cs`: comando `load_scene`.
- `MiniGameBridge.cs`: comando `iniciar_respiracion`.
- `DialogueInputController.cs`: mouse/Enter para avanzar o seleccionar.
- `DialoguePlayerControlLock.cs`: bloquea control del jugador durante dialogos.
- `YarnUiKenneySkin.cs`: skin visual para UI Yarn.

Reglas actuales de input:

- Enter o Numpad Enter avanza lineas/selecciones.
- Click izquierdo puede avanzar lineas.
- Mouse puede seleccionar opciones.
- Escape queda reservado para pausa solo cuando no hay dialogo activo.

Comandos Yarn utiles:

```yarn
<<load_scene NombreEscena>>
<<iniciar_respiracion MinigameBridge>>
```

## 6. Capitulo 1: puertas, progreso e inventario

Carpeta:

```text
Assets/Scripts/Chapter1
```

Scripts principales:

- `Chapter1EnvironmentController.cs`
- `Chapter1ProgressManager.cs`
- `Chapter1ProgressState.cs`
- `Chapter1GuidanceController.cs`
- `Chapter1InventorySystem.cs`
- `Chapter1RuntimePlaceholderSpawner.cs`
- `Chapter1AmbientMotion.cs`
- `Chapter1Billboard.cs`
- `RewardVisualController.cs`
- `DialoguePlayerControlLock.cs`

Responsabilidades:

- Crear/ordenar elementos faltantes del capitulo.
- Gestionar progreso narrativo.
- Mostrar objetivos/proximidad.
- Mostrar indicadores visuales sobre puertas.
- Manejar inventario simple tipo hotbar.
- Entregar/mostrar brujula cuando corresponde.
- Ocultar UI secundaria durante dialogos si estorba.
- Reportar resultado del minijuego al progreso.

Estado actual:

- Las puertas deben tener indicador visual flotante.
- La guia de proximidad/objetivos se muestra como ayuda de avance.
- El inventario existe y debe estar por debajo de dialogos para no tapar lectura.
- El progreso del minijuego se reporta con:

```csharp
Chapter1ProgressState.ReportBreathingCompleted()
Chapter1ProgressState.ReportBreathingFailed()
```

## 7. Minijuego Jetpack Joyride de respiracion

Carpetas:

```text
Assets/JetpackJoyrideTemplate
Assets/Scripts/MiniGames
Assets/Scripts/MiniGames/JetPackJoyride
```

Archivos principales:

```text
Assets/JetpackJoyrideTemplate/Scripts/BreathingController.cs
Assets/JetpackJoyrideTemplate/Scripts/BreathingTherapyGuide.cs
Assets/JetpackJoyrideTemplate/Scripts/PlayerJetpack.cs
Assets/JetpackJoyrideTemplate/Scripts/PlayerCollision.cs
Assets/JetpackJoyrideTemplate/Scripts/ObstacleSpawner.cs
Assets/JetpackJoyrideTemplate/Scripts/GameManager.cs
Assets/JetpackJoyrideTemplate/Scripts/MoveLeft.cs
Assets/JetpackJoyrideTemplate/Scripts/BackgroundLoop.cs
Assets/Scripts/MiniGames/MiniGameFlowController.cs
Assets/Scripts/MiniGames/MiniGameBridge.cs
Assets/Scripts/MiniGames/JetPackJoyride/JetpackTherapyHud.cs
Assets/Scripts/MiniGames/JetPackJoyride/BreathingTargetZoneVisualizer.cs
```

Mecanica:

```text
Inhale -> subir/controlar altura
Hold   -> sostener estabilidad
Exhale -> bajar/controlar descenso
```

Victoria:

```text
BreathingController completa ciclos
  -> SessionCompleted
  -> MiniGameFlowController.OnWin()
  -> Chapter1ProgressState.ReportBreathingCompleted()
  -> SceneLoader.LoadSceneSafe("SampleScene")
```

Fallo:

```text
PlayerCollision detecta obstaculo
  -> GameManager.RegisterMistake()
  -> si quedan intentos: invulnerabilidad y feedback
  -> si no quedan intentos: GameOver()
  -> Chapter1ProgressState.ReportBreathingFailed()
  -> SceneLoader.LoadSceneSafe("SampleScene")
```

HUD:

- Fase actual.
- Instruccion terapeutica.
- Progreso de fase.
- Ciclo actual.
- Porcentaje de ritmo/regulacion.
- Estado "En zona" o "Sigue la franja".
- Intentos restantes.

Visualizacion:

- `BreathingTargetZoneVisualizer` dibuja una franja horizontal en el mundo.
- Azul si el jugador esta fuera de zona.
- Verde si el jugador esta dentro de zona.

Riesgos tecnicos vigentes:

- Puede haber dos `BreathingController` en la escena.
- El jugador puede tener `Player2D` y `PlayerJetpack` al mismo tiempo. Si ambos escriben velocidad vertical, pueden competir.
- Recomendacion futura: dejar `PlayerJetpack` como controlador oficial y retirar/desactivar `Player2D` si no se usa.

## 8. Jugador 3D

Archivo:

```text
Assets/Scripts/Player/PlayerController.cs
```

Responsabilidad:

- Movimiento en primera persona.
- Input `OnMove` y `OnLook`.
- Gravedad con `CharacterController`.
- Rotacion de camara.
- Animator de caminata.

Notas:

- El cursor se bloquea durante gameplay normal.
- Los sistemas de dialogo y pausa modifican cursor cuando corresponde.

## 9. Lobo, NPC y rutas

Carpeta:

```text
Assets/Scripts/NPC
```

Archivos:

```text
Assets/Scripts/NPC/RouteSharedTypes.cs
Assets/Scripts/NPC/CharacterRouteController.cs
Assets/Scripts/NPC/Wolf/PetFollowController.cs
Assets/Scripts/NPC/Wolf/PetController.controller
```

Responsabilidad:

- Seguimiento del lobo con NavMesh.
- Estados idle/follow/sit/route.
- Ejecucion de rutas por pasos.
- Integracion con variables Yarn.

Validar al tocar:

- NavMesh bakeado.
- `NavMeshAgent` presente.
- Referencia al jugador.
- Parametros animator: `IsWalking`, `Sitting`, `Speed`.

## 10. Input

Asset:

```text
Assets/Models/CAP 1/MAP1/InputSystem_Actions.inputactions
```

Uso actual:

- `PlayerController`: callbacks `OnMove`, `OnLook`.
- `PlayerJetpack`: puede usar `InputActionReference`, pero tambien tiene fallback con Space.
- UI/Yarn: EventSystem y acciones UI.

Teclas importantes:

- Movimiento 3D: WASD/flechas.
- Interaccion: depende del binding `Interact`.
- Jetpack: Space.
- Dialogo: mouse, Enter, Numpad Enter, Space segun estado.
- Pausa: Escape fuera de dialogo.

## 11. Assets relevantes

### Interfaces

```text
Assets/Interfaces/FondoInterfaz.png
Assets/Interfaces/BOTON JUGAR.png
Assets/Interfaces/BOTON INSTRUCCIONES.png
Assets/Interfaces/VIDEO INTERFAZ.mp4
```

### Audio

```text
Assets/Resources/AmbientalLoop.mp3
Assets/Resources/Audio/GameplayMusic.mp3
```

### Minijuego

```text
Assets/JetpackJoyrideTemplate/Fondo.png
Assets/JetpackJoyrideTemplate/JetpackSheets/Asenso-removebg-preview.png
Assets/JetpackJoyrideTemplate/JetpackSheets/Desenso-removebg-preview.png
Assets/JetpackJoyrideTemplate/JetpackSheets/Normal-removebg-preview.png
Assets/JetpackJoyrideTemplate/Prefab/Obstacle.prefab
```

### UI Kenney

```text
Assets/Resources/KenneyUi
Assets/Resources/KenneyPrototypeKit
```

## 12. Dependencias

Archivo:

```text
Packages/manifest.json
```

Dependencias destacadas:

| Paquete | Version/origen | Uso |
| --- | --- | --- |
| `com.unity.render-pipelines.universal` | `17.4.0` | URP |
| `com.unity.inputsystem` | `1.19.0` | Input |
| `com.unity.ai.navigation` | `2.0.12` | NavMesh |
| `com.unity.ugui` | `2.0.0` | UI runtime |
| `com.unity.2d.sprite` | `1.0.0` | Sprites 2D |
| `com.unity.timeline` | `1.8.12` | Timeline disponible |
| `com.unity.test-framework` | `1.6.0` | Tests, no hay suite propia consolidada |
| `dev.yarnspinner.unity` | GitHub | Dialogos |

## 13. Convenciones del proyecto

- Usar `SceneLoader.LoadSceneSafe` para cargar escenas.
- Mantener `Bootstrap` primero.
- Usar `RuntimeInitializeOnLoadMethod` solo para sistemas globales o auto-instalables muy claros.
- Usar `SerializeField` para parametros configurables desde inspector.
- Evitar UI que tape dialogos. Dialogo debe tener prioridad visual.
- Evitar que Escape avance dialogos. Escape es pausa solo fuera de dialogo.
- Mantener recursos cargados con `Resources.Load` dentro de `Assets/Resources`.
- Para minijuego, preferir feedback visual simple sobre animaciones 3D complejas.

## 14. Riesgos y deuda tecnica

1. `FirstMiniGame` conserva un `BreathingController` duplicado desactivado.
   - El flujo activo usa el controlador del `GameManager`.
   - Ideal: eliminar el componente duplicado desde Unity cuando se valide la escena en editor.

2. El jugador 2D puede tener dos controladores verticales.
   - `Player2D` y `PlayerJetpack` pueden competir.
   - Ideal: elegir uno.

3. `SampleScene` sigue con nombre generico.
   - Ideal: renombrar a `MainWorld`, `Chapter1Scene` o similar cuando el flujo este estable.

4. No hay tests automatizados propios.
   - Recomendado: PlayMode tests para `BreathingController`, `SceneLoader`, `GameAudioManager` y progreso del capitulo.

5. Hay muchos sistemas auto-instalables.
   - Son utiles para avanzar rapido, pero a futuro conviene pasar configuraciones criticas a prefabs/escenas claras.

## 15. Donde tocar segun la peticion

| Necesidad | Archivos principales |
| --- | --- |
| Menu inicial | `Assets/Scenes/Interfaz.unity`, `Assets/Scripts/Bootstrap/MainMenuController.cs`, `Assets/Interfaces` |
| Carga/pantalla de carga | `Assets/Scripts/Bootstrap/SceneLoader.cs` |
| Pausa | `Assets/Scripts/Bootstrap/PauseMenuController.cs` |
| Audio | `Assets/Scripts/Bootstrap/GameAudioManager.cs`, `Assets/Resources` |
| Tutorial contextual | `Assets/Scripts/Bootstrap/ContextualTutorialController.cs` |
| Cielo/ambiente | `Assets/Scripts/Bootstrap/BrightEnvironmentController.cs`, `SampleScene` |
| Dialogos | `Assets/Scripts/Dialogue/IntroLvl.yarn`, `DialogueInputController.cs` |
| Puertas/progreso capitulo 1 | `Assets/Scripts/Chapter1` |
| Inventario | `Assets/Scripts/Chapter1/Chapter1InventorySystem.cs` |
| Objetivos/proximidad | `Assets/Scripts/Chapter1/Chapter1GuidanceController.cs` |
| Jugador 3D | `Assets/Scripts/Player/PlayerController.cs` |
| Lobo/NPC | `Assets/Scripts/NPC/Wolf/PetFollowController.cs` |
| Minijuego respiracion | `Assets/Scenes/FirstMiniGame.unity`, `Assets/JetpackJoyrideTemplate/Scripts`, `Assets/Scripts/MiniGames` |
| UI editable minijuego | `Assets/Resources/UI/JetpackPreGameOverlay.prefab`, `Assets/Resources/UI/JetpackTherapyHud.prefab`, `Assets/Resources/UI/JetpackCompletionOverlay.prefab` |
| HUD minijuego | `Assets/Resources/UI/JetpackTherapyHud.prefab`, `Assets/Scripts/MiniGames/JetPackJoyride/JetpackTherapyHud.cs` |
| Finalizacion minijuego | `Assets/Resources/UI/JetpackCompletionOverlay.prefab`, `Assets/Scripts/MiniGames/JetPackJoyride/JetpackCompletionOverlay.cs` |
| Zona objetivo minijuego | `Assets/Scripts/MiniGames/JetPackJoyride/BreathingTargetZoneVisualizer.cs` |

## 16. Comandos utiles

Listar scripts:

```powershell
rg --files Assets -g "*.cs" -g "!**/*.meta"
```

Buscar comandos Yarn:

```powershell
rg "YarnCommand" Assets/Scripts
```

Buscar referencias a escenas:

```powershell
rg "Bootstrap|Interfaz|SampleScene|FirstMiniGame" Assets ProjectSettings
```

Ver escenas del build:

```powershell
Get-Content ProjectSettings/EditorBuildSettings.asset
```

Ver cambios locales:

```powershell
git -c safe.directory=D:/Psicologia_del_dolor status --short
```

## 17. Roadmap tecnico recomendado

Prioridad alta:

1. Probar flujo completo desde `Bootstrap`: menu, jugar, dialogo, minijuego, retorno.
2. Limpiar `FirstMiniGame`: un solo `BreathingController`, un solo controlador vertical.
3. Ajustar dificultad del minijuego: velocidad, densidad de obstaculos, duracion de ciclos.
4. Revisar visualmente que HUD, inventario, dialogos y pausa no se tapen.

Prioridad media:

1. Convertir sistemas auto-instalables importantes en prefabs configurables.
2. Agregar sonidos de UI, golpe y victoria.
3. Mejorar feedback de puertas con efectos simples, luces y particulas.
4. Agregar pantalla de resultado del minijuego antes de volver a `SampleScene`.

Prioridad futura:

1. Renombrar escenas y scripts genericos.
2. Agregar PlayMode tests.
3. Documentar intencion terapeutica por mecanica.
4. Preparar build final con resolucion, calidad, icono y controles consistentes.

## 18. Resumen mental

Si el cambio es de menu, empieza en `MainMenuController`.

Si el cambio es de escena/carga, empieza en `SceneLoader`.

Si el cambio es de audio o volumen, empieza en `GameAudioManager` y `PauseMenuController`.

Si el cambio es de dialogos, empieza en `IntroLvl.yarn` y `DialogueInputController`.

Si el cambio es de puertas, objetivos o brujula, empieza en `Assets/Scripts/Chapter1`.

Si el cambio es del minijuego, empieza en `FirstMiniGame`, `BreathingController`, `ObstacleSpawner`, `GameManager`, `PlayerCollision`, `JetpackTherapyHud` y `BreathingTargetZoneVisualizer`.

Este documento queda como mapa actualizado del proyecto y debe revisarse cada vez que se agregue una funcionalidad grande.
