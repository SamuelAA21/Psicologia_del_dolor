# Contexto completo del proyecto: Psicologia_del_dolor

Documento generado el 2026-05-26 a partir de una revision del repositorio local.

Este archivo sirve como mapa de contexto antes de pedir cambios nuevos. Resume que contiene el proyecto, para que sirve cada parte, como se usa, como se ejecuta, como se conectan las escenas y donde tocar cuando se quiera modificar una funcionalidad.

## 1. Resumen ejecutivo

`Psicologia_del_dolor` es un proyecto de Unity orientado a una experiencia interactiva sobre psicologia del dolor. La estructura actual mezcla:

- Una escena principal 3D con jugador en primera persona, dialogo con Yarn Spinner, un entorno navegable, UI de dialogo y una mascota/NPC tipo lobo.
- Un flujo narrativo en Yarn que presenta decisiones sobre la relacion con el dolor y termina lanzando un minijuego.
- Un minijuego 2D tipo Jetpack Joyride enfocado en respiracion consciente, donde el jugador debe regular su altura segun fases de respiracion.
- Assets visuales importados: personajes, lobo, escenario 3D, skyboxes, imagenes de UI, sprites del minijuego, TextMesh Pro y muestras de Yarn Spinner.

El proyecto usa Unity 6:

- Version exacta del editor: `6000.4.4f1`.
- Render pipeline: Universal Render Pipeline, URP `17.4.0`.
- Input: New Input System `1.19.0`.
- Dialogos: Yarn Spinner Unity instalado desde GitHub.
- Navegacion: `com.unity.ai.navigation` para NavMesh.
- UI: UGUI y TextMesh Pro.

La entrada esperada del juego es `Assets/Scenes/Bootstrap.unity`, que carga `SampleScene`. Desde `SampleScene`, el dialogo puede lanzar `FirstMiniGame`, y al completar o perder el minijuego se vuelve a `SampleScene`.

## 2. Estado general del repositorio

Raiz del proyecto:

```text
C:\Psicologia_del_dolor
```

Carpetas principales:

```text
Assets/
Packages/
ProjectSettings/
```

Carpetas generadas por Unity que existen localmente pero no deberian tratarse como codigo fuente:

```text
Library/
Temp/
Logs/
UserSettings/
```

La configuracion de `.gitignore` ya excluye esas carpetas generadas, ademas de `*.csproj`, `*.sln`, `*.slnx`, builds, logs y archivos temporales comunes.

En la revision se detectaron cambios locales previos no creados por este documento:

```text
ProjectSettings/EditorBuildSettings.asset
ProjectSettings/Packages/dev.yarnspinner/Assembly-CSharp-generated.ysls.json
ProjectSettings/Packages/dev.yarnspinner/YarnSpinner.Unity.Samples-generated.ysls.json
ProjectSettings/ShaderGraphSettings.asset
```

Esos archivos no fueron modificados para generar este resumen.

## 3. Para que sirve el proyecto

El objetivo funcional actual parece ser una experiencia educativa/terapeutica gamificada:

1. El usuario entra a un espacio 3D.
2. Un avatar o sistema de dialogo le presenta tres puertas o posturas frente al dolor:
   - Retirada.
   - Entender sin cambiar.
   - Compromiso.
3. Segun sus respuestas, avanza por nodos narrativos.
4. El final del dialogo invita a practicar respiracion consciente.
5. Se carga un minijuego 2D donde el movimiento vertical representa inhalar, sostener y exhalar.
6. Al terminar los ciclos de respiracion o chocar con obstaculos, se vuelve a la escena principal.

La idea central no es solo "jugar", sino usar mecanicas interactivas para representar:

- Evitacion frente al dolor.
- Comprension cognitiva.
- Accion comprometida.
- Autorregulacion por respiracion.
- Relacion entre decisiones narrativas y actividades de practica.

## 4. Como abrir el proyecto

Requisitos recomendados:

- Unity Hub.
- Unity Editor `6000.4.4f1` o una version compatible de Unity 6.
- Conexion a internet la primera vez si Unity necesita resolver paquetes desde el Package Manager o desde GitHub.

Pasos:

1. Abrir Unity Hub.
2. Agregar el proyecto desde `C:\Psicologia_del_dolor`.
3. Abrir con Unity `6000.4.4f1`.
4. Esperar a que Unity regenere `Library/` y compile scripts.
5. Abrir la escena `Assets/Scenes/Bootstrap.unity`.
6. Presionar Play.

Tambien se puede abrir directamente `Assets/Scenes/SampleScene.unity` durante desarrollo si se quiere saltar el bootstrap, pero para probar el flujo real conviene iniciar desde `Bootstrap`.

## 5. Como ejecutar el flujo principal

Escena recomendada para Play:

```text
Assets/Scenes/Bootstrap.unity
```

Flujo:

```text
Bootstrap.unity
  -> Bootstrapper.Start()
  -> SceneLoader.Instance.LoadScene("SampleScene")
  -> SampleScene.unity
  -> dialogo Yarn / exploracion 3D
  -> comando Yarn <<iniciar_respiracion MinigameBridge>>
  -> MiniGameBridge.IniciarRespiracion()
  -> FirstMiniGame.unity
  -> BreathingController completa 4 ciclos o PlayerCollision detecta obstaculo
  -> SceneLoader.LoadSceneSafe("SampleScene")
```

Escenas incluidas en Build Settings:

```text
Assets/Scenes/Bootstrap.unity
Assets/Scenes/FirstMiniGame.unity
Assets/Scenes/SampleScene.unity
```

Escena existente pero no incluida en Build Settings:

```text
Assets/Scenes/Interfaz.unity
```

Esto significa que `Interfaz.unity` puede abrirse manualmente en el editor, pero no forma parte del build actual salvo que se agregue a `ProjectSettings/EditorBuildSettings.asset`.

## 6. Dependencias principales

Archivo de paquetes:

```text
Packages/manifest.json
```

Dependencias directas destacadas:

| Paquete | Version / origen | Uso probable |
| --- | --- | --- |
| `com.unity.render-pipelines.universal` | `17.4.0` | Renderizado URP. |
| `com.unity.inputsystem` | `1.19.0` | Movimiento 3D, UI, jetpack y acciones de entrada. |
| `com.unity.ai.navigation` | `2.0.12` | NavMesh para mascota/NPC. |
| `com.unity.ugui` | `2.0.0` | UI de dialogo, botones, canvas. |
| `com.unity.timeline` | `1.8.12` | Disponible, aunque no se ve como eje principal en scripts propios. |
| `com.unity.visualscripting` | `1.9.11` | Disponible, no parece ser el nucleo del flujo actual. |
| `com.unity.test-framework` | `1.6.0` | Framework de tests instalado, pero no hay tests propios detectados. |
| `dev.yarnspinner.unity` | GitHub | Dialogos Yarn Spinner. |

El lockfile muestra ademas un paquete embebido:

```text
Packages/dev.yarnspinner.unity.samples
```

Ese paquete contiene muestras de Yarn Spinner. Muchas carpetas y assets dentro de `Packages/dev.yarnspinner.unity.samples` son ejemplos externos y no deben confundirse con codigo propio del proyecto.

## 7. Configuracion de proyecto Unity

Archivo:

```text
ProjectSettings/ProjectSettings.asset
```

Datos relevantes:

- `productName`: `Psicologia_del_Dolor`.
- `companyName`: `DefaultCompany`.
- `bundleVersion`: `0.1.0`.
- Resolucion por defecto standalone: `1024x768`.
- Resolucion Web por defecto: `960x600`.
- Input activo: `activeInputHandler: 1`, equivalente al Input System nuevo.
- API compatibility level: `.NET Standard 2.1` / valor Unity `6`.
- Escena por defecto de template: `Assets/Scenes/SampleScene.unity`.

Tags:

```text
ProjectSettings/TagManager.asset
```

Tag personalizado detectado:

```text
Obstacle
```

Este tag es critico para que `PlayerCollision` detecte choques en el minijuego.

Render:

```text
Assets/Settings/PC_RPAsset.asset
Assets/Settings/Mobile_RPAsset.asset
Assets/Settings/PC_Renderer.asset
Assets/Settings/Mobile_Renderer.asset
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
```

El proyecto usa URP con assets separados para PC y mobile.

## 8. Estructura de `Assets`

Resumen de carpetas:

```text
Assets/
  Characters/
  Fantasy Skybox FREE/
  Interfaces/
  JetpackJoyrideTemplate/
  Models/
  Scenes/
  Scripts/
  Settings/
  TextMesh Pro/
  TutorialInfo/
```

Cantidad aproximada de archivos bajo `Assets`:

```text
520 archivos
```

Tipos principales detectados:

| Tipo | Cantidad aproximada | Comentario |
| --- | ---: | --- |
| `.meta` | 283 | Metadatos Unity. |
| `.png` | 81 | Texturas, UI, sprites, skyboxes. |
| `.mat` | 58 | Materiales. |
| `.cs` | 22 | Scripts propios y scripts tutoriales. |
| `.asset` | 17 | Settings, TMP, NavMesh, terrain, perfiles. |
| `.shader` | 14 | Principalmente TextMesh Pro. |
| `.unity` | 6 | Escenas propias y demos de skybox. |
| `.fbx` | 5 | Modelos de personajes/escenario. |
| `.controller` | 4 | Animator Controllers. |
| `.yarn` | 1 | Dialogo narrativo principal. |
| `.yarnproject` | 1 | Proyecto Yarn Spinner. |
| `.inputactions` | 1 | Asset de Input System. |
| `.prefab` | 1 | Obstaculo del minijuego. |
| `.mp4` | 1 | Video para interfaz. |

## 9. Assets visuales y de contenido

### 9.1 Personajes

Carpeta:

```text
Assets/Characters
```

Contenido:

- `Lobito/`: modelo `Wolf.fbx`, texturas y materiales del lobo.
- `MenPlayer/`: modelo `PlayerWalk.fbx`, texturas y `MenTexture.mat`.
- `WomanPlayer/`: modelos de mujer, incluido `MUJER CAMINANDO.fbx` y otro FBX con nombre que incluye `DISEÑO`, mas texturas.

Uso detectado:

- El lobo esta conectado con `PetFollowController` en `SampleScene`.
- El jugador 3D usa `PlayerController` y un Animator Controller propio.

### 9.2 Entorno 3D

Carpeta:

```text
Assets/Models/CAP 1
```

Contenido destacado:

- `MAP1/LVL1.fbx`.
- Materiales como `MaderaV2.mat`, `LVL1 Variant.mat`, `defaultMat.mat`.
- Texturas e imagenes asociadas.
- `MAP1/InputSystem_Actions.inputactions`.
- `MAP1/Readme.asset`.

Uso detectado:

- `SampleScene` contiene muchos cubos/objetos y NavMesh; parece ser el escenario 3D principal.
- `SampleScene` contiene assets de navegacion bakeados bajo `Assets/Scenes/SampleScene/`.

### 9.3 Skyboxes

Carpeta:

```text
Assets/Fantasy Skybox FREE
```

Contenido:

- Cubemaps y panoramas por ambiente: day, night, sunrise, sunset, rainy, snowy.
- Escenas demo:
  - `Demo with terrain.unity`
  - `Demo without terrain.unity`
- Readme y release notes del paquete.

Uso:

- Paquete visual importado para ambientacion. Las escenas demo no estan en Build Settings.

### 9.4 Interfaz

Carpeta:

```text
Assets/Interfaces
```

Contenido:

- `BOTON JUGAR.png`.
- `BOTON INSTRUCCIONES.png`.
- `VIDEO INTERFAZ.mp4`.
- `New Render Texture.renderTexture`.
- `Play.controller`.

Escena relacionada:

```text
Assets/Scenes/Interfaz.unity
```

Esta escena contiene:

- `MainMenu`.
- `Canvas`.
- `Play`.
- `Instructions`.
- `RawImage`.
- `VideoPlayer`.
- `EventSystem`.
- Camara.

Estado importante:

- `Interfaz.unity` existe, pero no esta en Build Settings actualmente.
- No se detecto script propio de menu que cargue escenas. Puede depender de eventos configurados desde UI, o estar incompleta.

### 9.5 Minijuego Jetpack Joyride

Carpeta:

```text
Assets/JetpackJoyrideTemplate
```

Contenido:

- `Fondo.png`.
- Sprites del jugador:
  - `JetpackSheets/Asenso-removebg-preview.png`.
  - `JetpackSheets/Desenso-removebg-preview.png`.
  - `JetpackSheets/Normal-removebg-preview.png`.
- Prefab:
  - `Prefab/Obstacle.prefab`.
- Scripts del minijuego.
- Animator Controller:
  - `Scripts/Player2DController.controller`.

## 10. Escenas propias

### 10.1 `Assets/Scenes/Bootstrap.unity`

Proposito:

- Escena raiz de arranque.
- Mantiene el cargador global de escenas.

Objeto principal:

```text
AppRoot
```

Componentes propios conectados:

- `Bootstrapper`
  - `firstScene: SampleScene`
- `SceneLoader`
  - `mainSceneName: SampleScene`

Comportamiento:

1. `Bootstrapper.Start()` llama a `SceneLoader.Instance.LoadScene(firstScene)`.
2. `SceneLoader` se marca con `DontDestroyOnLoad`.
3. Se carga `SampleScene` en modo `Single`.

Uso recomendado:

- Usar esta escena como primera escena de ejecucion y de build.
- No duplicar otro `SceneLoader` en escenas hijas salvo que se controle explicitamente.

### 10.2 `Assets/Scenes/SampleScene.unity`

Proposito:

- Escena principal jugable 3D.
- Contiene entorno, jugador, dialogo Yarn, UI de dialogo, mascota/NPC y puente al minijuego.

Objetos detectados:

- `Camera`.
- `Root`.
- `Canvas`.
- `Line Presenter`.
- `Options Presenter`.
- `Continue Button`.
- `Button Container`.
- `Character Name`.
- `Last Line`.
- `YarnCommands`.
- `MinigameBridge`.
- `MiniGameStarter`.
- `NavMesh Surface`.
- Jugador 3D con `PlayerController`.
- Lobo/mascota con `PetFollowController`.
- Multiples objetos `Cube (...)`, `Plane`, `MeshCollider`, `Background`, etc.

Componentes propios importantes:

`PlayerController`

- `moveSpeed: 5`.
- `gravity: -9.81`.
- `mouseSensitivity: 0.1`.
- `cameraTransform` apunta a la camara.
- `playerAnimator` conectado.

`PetFollowController`

- `playerTarget` apunta al jugador.
- `petAnimator` conectado.
- `stopDistance: 1.5`.
- `followStartDistance: 3`.
- `runDistance: 6`.
- `walkSpeed: 2.5`.
- `runSpeed: 5`.
- `idleBeforeSitTime: 5`.
- Parametros Animator:
  - `Speed`.
  - `Sitting`.
  - `IsWalking`.

`YarnSceneCommands`

- Registra comando Yarn `load_scene`.

`MiniGameBridge`

- `miniGameSceneName: FirstMiniGame`.
- Registra comando Yarn `iniciar_respiracion`.

Otros sistemas:

- `DialogueRunner`, `Line Presenter`, `Options Presenter` y elementos UI de Yarn Spinner aparecen conectados desde paquetes externos.
- `NavMesh Surface` existe para la navegacion de la mascota.
- Hay assets de NavMesh bakeados:
  - `Assets/Scenes/SampleScene/NavMesh-NavMesh Surface.asset`
  - `Assets/Scenes/SampleScene/NavMesh-NavMesh Surface 1.asset`
  - `Assets/Scenes/SampleScene/NavMesh-NavMesh Surface 2.asset`

### 10.3 `Assets/Scenes/FirstMiniGame.unity`

Proposito:

- Minijuego 2D de respiracion consciente.
- El jugador sube al presionar la tecla de impulso y cae cuando suelta.
- Los obstaculos generan corredores verticales alineados con la fase respiratoria.

Objetos detectados:

- `Player`.
- `BreathingTherapyGuide`.
- `Root`.
- `Directional Light`.
- `Fondo_1`.
- `Fondo_2`.
- `Spawner`.
- `GameManager`.
- `Main Camera`.

Componentes propios del jugador:

- `Player2D`
  - `jumpForce: 0.8`.
  - `maxFallSpeed: -1.2`.
  - `thrustKey: Space`.
- `PlayerJetpack`
  - `thrustVelocity: 2`.
  - `maxFallSpeed: -3`.
  - `fallbackKeyboardKey: Space`.
  - `thrustAction` no esta asignado, por lo que cae al fallback de teclado.
- `PlayerCollision`
  - `obstacleTag: Obstacle`.
- `PlayerState`
  - Cambia sprite entre ascenso, transicion y descenso.
  - `tiempoLimite: 1`.

Componentes del sistema:

- `GameManager`
  - `returnSceneName: SampleScene`.
- `MiniGameFlowController`
  - Escucha `BreathingController.SessionCompleted`.
  - Vuelve a `SampleScene`.
- `ObstacleSpawner`
  - `spawnInterval: 0.3`.
  - `useCameraBounds: true`.
  - `spawnFromCameraRightEdge: true`.
  - `obstacleLifetime: 10`.
  - `playAreaYRange: -2 a 3.5`.
  - Usa `Obstacle.prefab`.
- `BreathingController`
  - `playOnStart: true`.
  - `sessionCycleCount: 4`.
  - Secuencia serializada:
    - Inhale: `4s`, rango jugador `1..3`, rango obstaculos `1..3`.
    - Hold: `2s`, rango jugador `-0.5..0.5`, rango obstaculos `-1..1`.
    - Exhale: `4s`, rango jugador `-3..-1`, rango obstaculos `-3..-1`.
- `BreathingTherapyGuide`
  - Referencia al `BreathingController`.
  - Referencia al transform del jugador.
  - Calcula si el jugador esta en zona objetivo y un puntaje de regulacion.
- `BackgroundLoop`
  - En `Fondo_1` y `Fondo_2`.
  - `speed: 2`.
  - `width: 35`.

Prefab de obstaculo:

```text
Assets/JetpackJoyrideTemplate/Prefab/Obstacle.prefab
```

Configuracion:

- Nombre: `Obstacle`.
- Tag: `Obstacle`.
- Script `MoveLeft`.
- `speed: 5`.
- `destroyWhenPastX: -12`.

Notas importantes:

- En la escena hay al menos dos instancias de `BreathingController`: una en `Spawner` y otra en `GameManager`. El `MiniGameFlowController` referencia la del `GameManager`; `BreathingTherapyGuide` tambien referencia esa. Conviene revisar si `ObstacleSpawner` usa la misma o encuentra otra por `FindObjectOfType`.
- El jugador tiene `Player2D` y `PlayerJetpack` al mismo tiempo. Ambos pueden escribir `Rigidbody2D.linearVelocity` si el usuario presiona Space. Esto puede duplicar o competir en el control vertical.

### 10.4 `Assets/Scenes/Interfaz.unity`

Proposito:

- Menu/interfaz inicial o pantalla con video.

Objetos detectados:

- `MainMenu`.
- `Root`.
- `Canvas`.
- `RawImage`.
- `Play`.
- `Instructions`.
- `EventSystem`.
- `Camera`.
- `VideoPlayer`.

Assets relacionados:

- `Assets/Interfaces/VIDEO INTERFAZ.mp4`.
- `Assets/Interfaces/BOTON JUGAR.png`.
- `Assets/Interfaces/BOTON INSTRUCCIONES.png`.
- `Assets/Interfaces/Play.controller`.

Estado:

- No incluida en Build Settings.
- No se detecto script propio de menu o carga de escena.
- Si se quiere que sea pantalla inicial, debe agregarse a Build Settings y conectarse con `SceneLoader` o eventos `Button.onClick`.

### 10.5 `Assets/Scenes/SampleScene.unity` vs `Assets/Scenes/Interfaz.unity`

Actualmente la escena principal real es `SampleScene`, no `Interfaz`.

Si el objetivo final es que el usuario vea primero un menu:

1. Agregar `Interfaz.unity` a Build Settings.
2. Moverla antes de `Bootstrap` o hacer que `Bootstrap` cargue `Interfaz`.
3. Configurar el boton Play para cargar `SampleScene`.
4. Mantener `SceneLoader` vivo si se requiere carga centralizada.

## 11. Scripts propios: responsabilidades y uso

### 11.1 Bootstrap y carga de escenas

Carpeta:

```text
Assets/Scripts/Bootstrap
```

#### `Bootstrapper.cs`

Responsabilidad:

- Arrancar el flujo del juego.
- En `Start`, carga la primera escena configurada.

Campo serializado:

```csharp
[SerializeField] private string firstScene = "SampleScene";
```

Uso:

- Debe vivir en `Bootstrap.unity`.
- Requiere que exista un `SceneLoader.Instance`.

#### `SceneLoader.cs`

Responsabilidad:

- Singleton de carga de escenas.
- Persiste entre escenas con `DontDestroyOnLoad`.
- Evita multiples instancias.
- Permite cargar una escena por nombre.
- Permite fallback estatico si no existe instancia.

Metodos principales:

```csharp
LoadScene(string sceneName)
LoadMainScene()
LoadSceneSafe(string sceneName)
LoadMainSceneSafe()
```

Detalles:

- `LoadScene` corta una rutina de carga anterior si existe.
- `LoadRoutine` usa `SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single)`.
- `currentScene` se guarda internamente pero no se expone.
- `LoadMainSceneSafe` cae a `"SampleScene"` si no existe instancia.

Uso:

- Todo cambio de escena deberia pasar por `SceneLoader.LoadSceneSafe(...)` si se quiere mantener robustez.

#### `YarnSceneComands.cs`

Nombre de archivo:

```text
YarnSceneComands.cs
```

Nota: el nombre tiene `Comands` con una sola `m`, pero la clase se llama `YarnSceneCommands`.

Responsabilidad:

- Exponer un comando Yarn:

```yarn
<<load_scene NombreEscena>>
```

Implementacion:

```csharp
[YarnCommand("load_scene")]
public void LoadScene(string sceneName)
{
    SceneLoader.LoadSceneSafe(sceneName);
}
```

Uso:

- Permite cargar escenas desde archivos `.yarn`.

### 11.2 Jugador 3D

Carpeta:

```text
Assets/Scripts/Player
```

#### `PlayerController.cs`

Responsabilidad:

- Movimiento en primera persona.
- Lectura de `OnMove` y `OnLook` desde `PlayerInput`.
- Rotacion horizontal del jugador.
- Rotacion vertical de la camara.
- Gravedad con `CharacterController`.
- Activacion del bool `IsWalking` en Animator.

Requisitos:

```csharp
[RequireComponent(typeof(CharacterController))]
```

Metodos invocados por Input System:

```csharp
OnMove(InputValue value)
OnLook(InputValue value)
```

Campos:

- `moveSpeed`.
- `gravity`.
- `cameraTransform`.
- `mouseSensitivity`.
- `playerAnimator`.

Comportamiento:

- En `Awake`, bloquea y oculta el cursor.
- En `Update`, procesa rotacion y movimiento.
- El movimiento usa `transform.right` y `transform.forward`.
- La camara se limita entre `-80` y `80` grados en el eje vertical.

Animator relacionado:

```text
Assets/Scripts/Player/PlayerController.controller
```

Parametros:

- `IsWalking` bool.

Estados detectados:

- `Idle`.
- `IsWalking`.

Transiciones:

- `IsWalking == true`: pasa a caminar.
- `IsWalking == false`: vuelve a idle.

### 11.3 Dialogo Yarn

Carpeta:

```text
Assets/Scripts/Dialogue
```

#### `StoryLvl.yarnproject`

Configuracion:

```json
{
  "sourceFiles": ["**/*.yarn"],
  "baseLanguage": "es"
}
```

Uso:

- Proyecto Yarn que compila todos los archivos `.yarn` bajo la carpeta.
- Idioma base: espanol.

#### `IntroLvl.yarn`

Responsabilidad:

- Dialogo narrativo principal.
- Define nodos:
  - `Intro`.
  - `Puerta1`.
  - `Puerta1_A`.
  - `Puerta1_B`.
  - `Puerta2`.
  - `Puerta2_A`.
  - `Puerta2_B`.
  - `Puerta3`.
  - `Puerta3_A`.
  - `Puerta3_B`.
  - `Estacion4`.
  - `Pregunta_A`.
  - `Pregunta_B`.
  - `Final`.

Estructura narrativa:

```text
Intro
  -> Puerta1: retirada
      -> Puerta1_A
      -> Puerta1_B
  -> Puerta2: entender sin cambiar
      -> Puerta2_A
      -> Puerta2_B
  -> Puerta3: compromiso
      -> Puerta3_A
      -> Puerta3_B
      -> Estacion4
          -> Pregunta_A
          -> Pregunta_B
  -> Final
      -> iniciar_respiracion
```

Comando final:

```yarn
<<iniciar_respiracion MinigameBridge>>
```

Ese comando depende de `MiniGameBridge`.

Nota sobre codificacion:

- Al leer desde terminal algunos acentos se ven corruptos, por ejemplo `cobardÃ­a`. Es posible que el archivo este guardado con UTF-8 pero la consola lo haya mostrado con otra codificacion, o que el archivo ya tenga mojibake. Conviene abrirlo en Unity/VS Code y verificar encoding antes de editar texto narrativo.

### 11.4 Puente a minijuego

Carpeta:

```text
Assets/Scripts/MiniGames
```

#### `MiniGameBridge.cs`

Responsabilidad:

- Exponer comando Yarn para iniciar minijuego de respiracion.

Comando:

```csharp
[YarnCommand("iniciar_respiracion")]
public void IniciarRespiracion()
```

Flujo:

1. Busca `DialogueRunner`.
2. Si existe, llama `dialogueRunner.Stop()`.
3. Espera un frame.
4. Carga `FirstMiniGame` usando `SceneLoader.LoadSceneSafe`.

Campo:

```csharp
[SerializeField] private string miniGameSceneName = "FirstMiniGame";
```

Uso:

- Debe estar presente en la escena donde corre el dialogo Yarn.
- En `SampleScene` aparece como objeto `MinigameBridge`.

#### `MiniGameStarter.cs`

Responsabilidad:

- Buscar un `BreathingController` y ejecutar `StartCycle()`.

Uso:

- En `SampleScene` existe un objeto `MiniGameStarter`, pero el script esta pensado para minijuego. Hay que revisar si su presencia en `SampleScene` es intencional.

#### `MiniGameFlowController.cs`

Responsabilidad:

- Escuchar el evento `SessionCompleted` del `BreathingController`.
- Al completar la sesion, volver a `SampleScene`.

Campo:

```csharp
[SerializeField] private string returnSceneName = "SampleScene";
```

Uso:

- En `FirstMiniGame`, esta conectado al `BreathingController` del `GameManager`.

### 11.5 Minijuego 2D: respiracion y jetpack

Carpeta:

```text
Assets/JetpackJoyrideTemplate/Scripts
```

#### `BreathingController.cs`

Responsabilidad:

- Modelar una sesion de respiracion por fases.
- Fases: `Inhale`, `Hold`, `Exhale`.
- Avanzar por temporizador.
- Calcular el centro terapeutico vertical segun fase y progreso.
- Emitir eventos de cambio de fase y sesion completada.

Clases/tipos:

```csharp
public enum BreathPhase { Inhale, Hold, Exhale }
public class BreathingPhaseSettings
public class BreathingController
```

Campos principales:

- `phaseSequence`.
- `playOnStart`.
- `sessionCycleCount`.
- `currentPhaseIndex`.
- `phaseTimer`.
- `completedCycles`.

Eventos:

```csharp
public event Action<BreathingPhaseSettings> PhaseChanged;
public event Action SessionCompleted;
```

Propiedades:

- `CurrentPhase`.
- `CurrentPhaseSettings`.
- `CurrentPhaseProgress`.
- `IsRunning`.
- `CompletedCycles`.
- `SessionCycleCount`.

Metodos publicos:

- `StartCycle()`.
- `StopCycle()`.
- `ResetCycle()`.
- `TryGetCurrentPhaseSettings(out settings)`.
- `GetTherapeuticCenterY()`.

Curvas:

- Inhalar usa `Mathf.Sin(progress * PI * 0.5)`.
- Exhalar usa `1 - Mathf.Cos(progress * PI * 0.5)`.
- Sostener mantiene el centro de entrada.

Uso en minijuego:

- `ObstacleSpawner` usa el centro terapeutico para abrir corredores.
- `BreathingTherapyGuide` usa los rangos para calcular alineacion del jugador.
- `MiniGameFlowController` vuelve a escena principal al completar ciclos.

#### `BreathingTherapyGuide.cs`

Responsabilidad:

- Medir si el jugador esta dentro del rango vertical objetivo de la fase actual.
- Acumular tiempo alineado y tiempo total.
- Exponer `RegulationScore`.
- Exponer `CurrentInstruction`.

Propiedades:

- `IsPlayerInTargetZone`.
- `RegulationScore`.
- `CurrentInstruction`.

Uso:

- Puede alimentar UI futura de puntaje, feedback o instrucciones.
- Actualmente no se detecto UI propia que muestre ese puntaje.

#### `ObstacleSpawner.cs`

Responsabilidad:

- Generar obstaculos a la derecha de la camara.
- Crear bandas de obstaculos arriba y abajo de un corredor libre.
- Ubicar el corredor segun `BreathingController.GetTherapeuticCenterY()`.

Campos importantes:

- `obstaclePrefab`.
- `targetCamera`.
- `spawnInterval`.
- `useCameraBounds`.
- `spawnFromCameraRightEdge`.
- `horizontalSpawnPadding`.
- `obstacleLifetime`.
- `playAreaYRange`.
- `corridorHeight`.
- `obstacleVerticalSpacing`.
- `corridorPadding`.
- `breathingController`.

Logica:

1. Cada `spawnInterval`, si el juego puede correr, llama `Spawn()`.
2. Calcula rango vertical jugable.
3. Calcula centro del corredor.
4. Genera obstaculos desde el minimo hasta el borde inferior del corredor.
5. Genera obstaculos desde el borde superior del corredor hasta el maximo.
6. Destruye cada obstaculo despues de `obstacleLifetime`.

#### `PlayerJetpack.cs`

Responsabilidad:

- Controlar impulso vertical por Input System o fallback a teclado.
- Limitar caida.

Campos:

- `thrustVelocity`.
- `maxFallSpeed`.
- `thrustAction`.
- `fallbackKeyboardKey`.

Uso:

- En escena `FirstMiniGame`, `thrustAction` no esta asignado; usa Space.

#### `Player2D.cs`

Responsabilidad:

- Control 2D simple alternativo.
- Si Space esta presionado, asigna velocidad vertical.
- Limita caida.

Nota:

- En `FirstMiniGame`, este script convive con `PlayerJetpack`. Conviene dejar solo uno si se busca un control mas claro.

#### `PlayerCollision.cs`

Responsabilidad:

- Detectar colision o trigger contra objetos con tag `Obstacle`.
- Llamar `GameManager.Instance.GameOver()`.

Requisitos:

- Los obstaculos deben tener tag `Obstacle`.
- Debe existir `GameManager.Instance`.

#### `GameManager.cs`

Responsabilidad:

- Singleton simple del minijuego.
- Controla estado `isGameOver`.
- Expone `CanPlay`.
- En Game Over vuelve a escena principal.

Metodos:

- `GameOver()`.
- `ResetSession()`.

Campo:

- `returnSceneName = "SampleScene"`.

#### `BackgroundLoop.cs`

Responsabilidad:

- Mover fondos hacia la izquierda.
- Reposicionar un fondo a la derecha del ultimo cuando sale por el borde izquierdo.

Uso:

- `Fondo_1` y `Fondo_2` en `FirstMiniGame`.

#### `MoveLeft.cs`

Responsabilidad:

- Mover objetos a la izquierda y destruirlos al pasar cierto X.

Uso:

- `Obstacle.prefab`.

#### `Animation.cs`

Clase:

```csharp
public class PlayerState : MonoBehaviour
```

Responsabilidad:

- Cambiar sprite del jugador segun `PlayerJetpack.IsThrusting`.
- Usa tres sprites:
  - `volar`.
  - `transicion`.
  - `bajar`.

Nota:

- El archivo se llama `Animation.cs`, pero la clase se llama `PlayerState`. En Unity esto compila si no es `MonoBehaviour` publico? En C#, una clase publica puede estar en un archivo con nombre distinto, pero para scripts Unity de `MonoBehaviour` es buena practica que el archivo y la clase coincidan. Aqui Unity ya lo tiene referenciado, pero podria confundir a futuro.

### 11.6 NPC, mascota y rutas

Carpeta:

```text
Assets/Scripts/NPC
```

#### `RouteSharedTypes.cs`

Responsabilidad:

- Definir tipos compartidos para acciones/rutas.

Tipos:

```csharp
RouteActionType
RouteCondition
RouteAction
RouteStep
```

Acciones disponibles:

- `SetGameObjectActive`.
- `SetAnimatorTrigger`.
- `SetAnimatorBool`.
- `MoveTransformToPoint`.
- `SetYarnVariable`.
- `InvokeUnityEvent`.

Uso:

- Compartido por `CharacterRouteController` y `PetFollowController`.

#### `CharacterRouteController.cs`

Responsabilidad:

- Ejecutar rutas/acciones para personajes.
- Puede dispararse por variable Yarn o manualmente.
- Puede construir una ruta por defecto si no hay pasos configurados.

Campos principales:

- `triggerYarnVariable`.
- `triggerExpectedValue`.
- `autoRunOnStart`.
- `steps`.
- `buildDefaultRouteIfEmpty`.
- `defaultMoveTarget`.
- `defaultAnimator`.
- `defaultAnimatorTrigger`.
- `defaultMoveDuration`.

Flujo:

1. En `Awake`, configura defaults.
2. Busca `InMemoryVariableStorage`.
3. En `Start`, si `autoRunOnStart`, corre ruta.
4. `CheckAndRun()` revisa variable Yarn.
5. Recorre `RouteStep`.
6. Evalua condiciones.
7. Ejecuta acciones en orden.

Acciones:

- Activar/desactivar GameObject.
- Setear trigger/bool de Animator.
- Mover transform a un punto con interpolacion.
- Setear variable booleana de Yarn.
- Invocar `UnityEvent`.

#### `PetFollowController.cs`

Responsabilidad:

- Controlar una mascota/NPC con NavMesh.
- Seguir al jugador.
- Cambiar estados: idle, following, sitting, running route.
- Integrarse con Yarn y rutas por pasos.

Requisitos:

```csharp
[RequireComponent(typeof(NavMeshAgent))]
```

Estados:

```csharp
PetState.Idle
PetState.Following
PetState.Sitting
PetState.RunningRoute
```

Comportamiento de seguimiento:

- Si el jugador esta lejos mas que `followStartDistance`, empieza a seguir.
- Si esta dentro de `stopDistance`, se detiene.
- Si supera `runDistance`, usa `runSpeed`; si no, `walkSpeed`.
- Si pasa demasiado tiempo quieto, pasa a sitting.

Animator:

- `Speed` float.
- `Sitting` bool.
- `IsWalking` bool.
- `greetTrigger` opcional.

Yarn/rutas:

- Similar a `CharacterRouteController`, permite ejecutar `RouteStep`.
- Puede dispararse por variable Yarn.
- Puede construir ruta por defecto con variable `$mascota_activa`.

Controlador Animator relacionado:

```text
Assets/Scripts/NPC/Wolf/PetController.controller
```

Parametros:

- `IsWalking` bool.
- `Sitting` bool.
- `Speed` float.

Estados detectados:

- `Idle`.
- `IsWalking`.

## 12. Input System

Asset:

```text
Assets/Models/CAP 1/MAP1/InputSystem_Actions.inputactions
```

Action maps detectados:

### Player

Acciones:

- `Move`: `Vector2`.
- `Look`: `Vector2`.
- `Attack`: button.
- `Interact`: button, con interaccion `Hold`.
- `Crouch`: button.
- `Jump`: button.
- `Previous`: button.
- `Next`: button.
- `Sprint`: button.

Bindings principales:

- Movimiento:
  - WASD.
  - Flechas.
  - Gamepad left stick.
  - Joystick stick.
  - XR primary 2D axis.
- Mirada:
  - Mouse/pointer delta.
  - Gamepad right stick.
- Jump:
  - Space.
  - Gamepad south button.
  - XR secondary button.
- Sprint:
  - Left Shift.
  - Gamepad left stick press.
  - XR trigger.
- Interact:
  - `E`.
  - Gamepad north/east segun bindings.

### UI

Acciones:

- `Navigate`.
- `Submit`.
- `Cancel`.
- `Point`.
- `Click`.
- `RightClick`.
- `MiddleClick`.
- `ScrollWheel`.
- `TrackedDevicePosition`.
- `TrackedDeviceOrientation`.

Uso:

- `PlayerController` usa los callbacks `OnMove` y `OnLook`.
- `PlayerJetpack` puede usar `InputActionReference`, pero actualmente en escena usa fallback Space.
- UI y Yarn probablemente usan acciones UI desde el EventSystem/InputSystemUIInputModule.

## 13. Dialogo Yarn: como extenderlo

Archivo principal:

```text
Assets/Scripts/Dialogue/IntroLvl.yarn
```

Para agregar una nueva decision:

1. Crear un nuevo nodo:

```yarn
title: NuevoNodo
---
Avatar: Texto...

-> Opcion
    <<jump OtroNodo>>
===
```

2. Agregar un salto desde un nodo existente:

```yarn
-> Ir al nuevo contenido
    <<jump NuevoNodo>>
```

3. Si se quiere activar una escena:

```yarn
<<load_scene NombreEscena>>
```

4. Si se quiere lanzar la respiracion:

```yarn
<<iniciar_respiracion MinigameBridge>>
```

5. Si se quieren usar variables con rutas/NPC:

```yarn
<<set $mascota_activa = true>>
```

Luego el componente correspondiente debe llamar `CheckAndRun()` o estar conectado a eventos/comandos.

## 14. Flujo de minijuego de respiracion

Objetivo de la mecanica:

- Inhalar: el corredor y/o zona objetivo sube.
- Sostener: el jugador debe estabilizarse.
- Exhalar: el corredor baja.
- Completar una secuencia de fases equivale a un ciclo.
- Se requieren `4` ciclos para completar la sesion.

Logica central:

```text
BreathingController.StartCycle()
  -> phase Inhale
  -> PhaseChanged
  -> timer llega a duracion
  -> phase Hold
  -> timer llega a duracion
  -> phase Exhale
  -> timer llega a duracion
  -> completedCycles++
  -> repetir hasta sessionCycleCount
  -> SessionCompleted
  -> MiniGameFlowController.OnWin()
  -> SceneLoader.LoadSceneSafe("SampleScene")
```

Logica de obstaculos:

```text
ObstacleSpawner.Update()
  -> cada spawnInterval
  -> GetCorridorCenterY()
  -> BreathingController.GetTherapeuticCenterY()
  -> genera banda inferior de obstaculos
  -> genera banda superior de obstaculos
```

Logica de derrota:

```text
PlayerCollision.OnCollisionEnter2D / OnTriggerEnter2D
  -> si tag == Obstacle
  -> GameManager.GameOver()
  -> SceneLoader.LoadSceneSafe("SampleScene")
```

## 15. Controladores Animator

### `Assets/Scripts/Player/PlayerController.controller`

Uso:

- Animator del jugador 3D.

Parametro:

- `IsWalking` bool.

Estados:

- `Idle`.
- `IsWalking`.

Script que lo maneja:

- `PlayerController.cs`.

### `Assets/Scripts/NPC/Wolf/PetController.controller`

Uso:

- Animator del lobo/mascota.

Parametros:

- `IsWalking` bool.
- `Sitting` bool.
- `Speed` float.

Script que lo maneja:

- `PetFollowController.cs`.

### `Assets/JetpackJoyrideTemplate/Scripts/Player2DController.controller`

Uso:

- Animator Controller simple con un estado `Idle`.
- No parece ser el nucleo del cambio visual actual, porque `PlayerState` cambia sprites manualmente.

### `Assets/Interfaces/Play.controller`

Uso:

- Animator Controller de UI para boton Play.

Parametros:

- `Normal`.
- `Highlighted`.
- `Pressed`.
- `Selected`.
- `Disabled`.

## 16. Build y escenas

Build Settings actual:

```text
0. Assets/Scenes/Bootstrap.unity
1. Assets/Scenes/FirstMiniGame.unity
2. Assets/Scenes/SampleScene.unity
```

Recomendaciones:

- Mantener `Bootstrap` primero si se quiere que `SceneLoader` exista desde el inicio.
- Si `Interfaz` sera menu inicial, decidir si:
  - `Bootstrap` carga `Interfaz`, y el menu carga `SampleScene`.
  - O `Interfaz` es la primera escena y contiene/invoca `SceneLoader`.
- Todo nombre usado en `LoadSceneSafe` debe estar incluido en Build Settings para builds finales.

## 17. Convenciones y patrones actuales

Patrones positivos existentes:

- Uso de `SerializeField` para configurar desde inspector.
- Uso de `RequireComponent` en controladores que necesitan componentes Unity.
- Carga centralizada de escenas con `SceneLoader`.
- Uso de comandos Yarn para desacoplar dialogo y escenas.
- Eventos C# (`SessionCompleted`) para separar respiracion y flujo de escena.
- Configuraciones de fases de respiracion serializables.
- Sistema generico de rutas con `RouteStep`, `RouteAction` y `RouteCondition`.

Convenciones de nombres:

- Escenas usan nombres en ingles: `Bootstrap`, `SampleScene`, `FirstMiniGame`, `Interfaz`.
- Scripts mezclan ingles y espanol:
  - `IniciarRespiracion`.
  - `YarnSceneComands`.
  - `PetFollowController`.
  - `MiniGameFlowController`.
- Variables Yarn usan espanol:
  - `$tema_siguiente_desbloqueado`.
  - `$mascota_activa`.

## 18. Puntos importantes antes de pedir cambios

### 18.1 Si quieres cambiar narrativa

Tocar principalmente:

```text
Assets/Scripts/Dialogue/IntroLvl.yarn
```

Validar:

- Encoding del archivo.
- Que los nodos tengan `===`.
- Que los comandos Yarn existan como `[YarnCommand]`.
- Que las escenas referenciadas esten en Build Settings.

### 18.2 Si quieres cambiar movimiento del jugador 3D

Tocar:

```text
Assets/Scripts/Player/PlayerController.cs
Assets/Models/CAP 1/MAP1/InputSystem_Actions.inputactions
Assets/Scripts/Player/PlayerController.controller
```

Validar:

- `PlayerInput` debe apuntar al asset de input correcto.
- El Action Map por defecto debe ser `Player`.
- Los callbacks deben llamarse `OnMove`, `OnLook`, etc. segun Input System.

### 18.3 Si quieres cambiar el lobo/mascota

Tocar:

```text
Assets/Scripts/NPC/Wolf/PetFollowController.cs
Assets/Scripts/NPC/Wolf/PetController.controller
Assets/Characters/Lobito
```

Validar:

- NavMesh bakeado.
- `NavMeshAgent` en el objeto.
- Referencia a jugador.
- Parametros Animator exactos: `IsWalking`, `Sitting`, `Speed`.

### 18.4 Si quieres cambiar rutas o eventos de NPC

Tocar:

```text
Assets/Scripts/NPC/RouteSharedTypes.cs
Assets/Scripts/NPC/CharacterRouteController.cs
Assets/Scripts/NPC/Wolf/PetFollowController.cs
```

Validar:

- Variables Yarn con prefijo `$`.
- `InMemoryVariableStorage` en escena.
- Llamadas a `CheckAndRun()` desde eventos/comandos.

### 18.5 Si quieres cambiar el minijuego

Tocar:

```text
Assets/JetpackJoyrideTemplate/Scripts/BreathingController.cs
Assets/JetpackJoyrideTemplate/Scripts/ObstacleSpawner.cs
Assets/JetpackJoyrideTemplate/Scripts/PlayerJetpack.cs
Assets/JetpackJoyrideTemplate/Scripts/PlayerCollision.cs
Assets/JetpackJoyrideTemplate/Scripts/GameManager.cs
Assets/Scenes/FirstMiniGame.unity
```

Validar:

- Que exista solo un controlador de respiracion activo o que las referencias sean explicitas.
- Que el obstaculo tenga tag `Obstacle`.
- Que `FirstMiniGame` y `SampleScene` esten en Build Settings.
- Que no haya dos scripts compitiendo por la velocidad vertical del jugador.

### 18.6 Si quieres agregar menu inicial

Tocar:

```text
Assets/Scenes/Interfaz.unity
Assets/Interfaces
ProjectSettings/EditorBuildSettings.asset
Assets/Scripts/Bootstrap/Bootstrapper.cs
```

Decidir:

- Si `Bootstrap` carga `Interfaz`.
- Si `Interfaz` carga `SampleScene`.
- Si `SceneLoader` vive en `Bootstrap` o en menu.

## 19. Riesgos y observaciones detectadas

Estos puntos no son necesariamente errores confirmados, pero conviene revisarlos antes de ampliar el proyecto.

1. `FirstMiniGame` tiene `Player2D` y `PlayerJetpack` en el mismo objeto.
   - Ambos modifican la velocidad vertical del `Rigidbody2D`.
   - Recomendacion: elegir uno como controlador oficial.

2. `FirstMiniGame` parece tener dos `BreathingController`.
   - Uno en `Spawner`.
   - Uno en `GameManager`.
   - `MiniGameFlowController` referencia el del `GameManager`.
   - Recomendacion: usar una sola instancia o asignar referencias explicitas para evitar que `FindObjectOfType` tome la incorrecta.

3. `MiniGameStarter` aparece en `SampleScene`.
   - Ese script busca `BreathingController` y llama `StartCycle()`.
   - En `SampleScene` no parece corresponder al flujo principal 3D.
   - Recomendacion: confirmar si esta ahi por accidente.

4. `YarnSceneComands.cs` tiene typo en el nombre de archivo.
   - Clase: `YarnSceneCommands`.
   - Archivo: `YarnSceneComands.cs`.
   - Recomendacion: renombrar si no rompe referencias Unity.

5. `Animation.cs` contiene clase `PlayerState`.
   - Recomendacion: renombrar archivo a `PlayerState.cs` para claridad.

6. `IntroLvl.yarn` puede tener problemas de codificacion.
   - En terminal se observan caracteres mal renderizados.
   - Recomendacion: abrir en VS Code, confirmar UTF-8 y corregir mojibake si existe.

7. `Interfaz.unity` no esta en Build Settings.
   - Si debe ser menu real, el build no la usara actualmente.

8. No se detectaron tests propios.
   - El paquete `com.unity.test-framework` esta instalado.
   - Recomendacion: agregar PlayMode tests para `SceneLoader`, `BreathingController` y minijuego si el proyecto crecera.

9. `Packages/dev.yarnspinner.unity.samples` esta embebido y tiene muchisimos assets.
   - Puede ser util como referencia, pero aumenta peso y ruido.
   - Recomendacion: mantener solo si se necesitan muestras o assets compartidos.

10. La escena principal se llama `SampleScene`.
    - Para un proyecto final convendria renombrar a algo semantico como `MainWorld`, `NivelDolor01` o `EscenaPrincipal`.

## 20. Donde estan las piezas criticas

| Necesidad | Archivo/carpeta |
| --- | --- |
| Arranque del juego | `Assets/Scenes/Bootstrap.unity` |
| Carga de escenas | `Assets/Scripts/Bootstrap/SceneLoader.cs` |
| Escena 3D principal | `Assets/Scenes/SampleScene.unity` |
| Movimiento jugador 3D | `Assets/Scripts/Player/PlayerController.cs` |
| Animator jugador | `Assets/Scripts/Player/PlayerController.controller` |
| Dialogo principal | `Assets/Scripts/Dialogue/IntroLvl.yarn` |
| Proyecto Yarn | `Assets/Scripts/Dialogue/StoryLvl.yarnproject` |
| Comando Yarn para cargar escena | `Assets/Scripts/Bootstrap/YarnSceneComands.cs` |
| Comando Yarn para minijuego | `Assets/Scripts/MiniGames/MiniGameBridge.cs` |
| Minijuego | `Assets/Scenes/FirstMiniGame.unity` |
| Respiracion | `Assets/JetpackJoyrideTemplate/Scripts/BreathingController.cs` |
| Spawner de obstaculos | `Assets/JetpackJoyrideTemplate/Scripts/ObstacleSpawner.cs` |
| Control jetpack | `Assets/JetpackJoyrideTemplate/Scripts/PlayerJetpack.cs` |
| Colisiones minijuego | `Assets/JetpackJoyrideTemplate/Scripts/PlayerCollision.cs` |
| GameManager minijuego | `Assets/JetpackJoyrideTemplate/Scripts/GameManager.cs` |
| Mascota/lobo | `Assets/Scripts/NPC/Wolf/PetFollowController.cs` |
| Tipos de rutas | `Assets/Scripts/NPC/RouteSharedTypes.cs` |
| Rutas de personajes | `Assets/Scripts/NPC/CharacterRouteController.cs` |
| Menu/interfaz | `Assets/Scenes/Interfaz.unity`, `Assets/Interfaces` |
| Config paquetes | `Packages/manifest.json` |
| Version Unity | `ProjectSettings/ProjectVersion.txt` |
| Build Settings | `ProjectSettings/EditorBuildSettings.asset` |

## 21. Comandos utiles para inspeccion

Listar archivos del proyecto sin carpetas generadas:

```powershell
rg --files -g '!bin' -g '!obj' -g '!node_modules' -g '!dist' -g '!build' -g '!coverage'
```

Listar scripts propios:

```powershell
rg --files Assets -g '*.cs' -g '!**/*.meta'
```

Buscar comandos Yarn registrados:

```powershell
rg 'YarnCommand' Assets/Scripts
```

Buscar referencias a una escena:

```powershell
rg 'SampleScene|FirstMiniGame|Bootstrap|Interfaz' Assets ProjectSettings
```

Buscar scripts con logs o advertencias:

```powershell
rg 'Debug.LogWarning|Debug.LogError|TODO|FIXME' Assets/Scripts Assets/JetpackJoyrideTemplate/Scripts
```

Ver escenas incluidas en build:

```powershell
Get-Content ProjectSettings/EditorBuildSettings.asset
```

## 22. Recomendacion de roadmap tecnico

Orden sugerido para estabilizar el proyecto antes de crecer:

1. Definir el flujo inicial oficial:
   - `Bootstrap -> SampleScene`, o
   - `Bootstrap -> Interfaz -> SampleScene`.

2. Limpiar el minijuego:
   - Elegir `PlayerJetpack` o `Player2D`.
   - Dejar una sola instancia de `BreathingController`.
   - Conectar UI a `BreathingTherapyGuide.CurrentInstruction` y `RegulationScore` si se quiere feedback terapeutico.

3. Revisar encoding de Yarn:
   - Corregir acentos si estan rotos.
   - Mantener todo en UTF-8.

4. Consolidar nombres:
   - `SampleScene` a nombre final.
   - `YarnSceneComands.cs` a `YarnSceneCommands.cs`.
   - `Animation.cs` a `PlayerState.cs`.

5. Documentar decisiones de diseno terapeutico:
   - Que representa cada puerta.
   - Que objetivo tiene cada fase de respiracion.
   - Que feedback debe recibir el usuario.

6. Agregar tests minimos:
   - `BreathingController` avanza fases y dispara `SessionCompleted`.
   - `SceneLoader` ignora nombres vacios.
   - `ObstacleSpawner` no spawnea si falta prefab.

## 23. Glosario rapido

- `Bootstrap`: escena inicial que crea el cargador global.
- `SceneLoader`: singleton para cambiar escenas.
- `SampleScene`: escena principal 3D actual.
- `FirstMiniGame`: escena de respiracion tipo jetpack.
- `Interfaz`: escena de menu/interfaz, actualmente fuera del build.
- `Yarn`: formato de dialogo narrativo usado por Yarn Spinner.
- `YarnCommand`: metodo C# invocable desde Yarn.
- `DialogueRunner`: componente de Yarn Spinner que ejecuta dialogos.
- `InMemoryVariableStorage`: almacenamiento de variables Yarn.
- `NavMeshAgent`: componente de Unity para navegacion automatica.
- `RouteStep`: paso de una ruta de NPC.
- `RouteAction`: accion ejecutable dentro de una ruta.
- `BreathingController`: sistema de fases de respiracion.
- `ObstacleSpawner`: generador de obstaculos del minijuego.
- `RegulationScore`: proporcion de tiempo en que el jugador estuvo en zona objetivo.

## 24. Resumen mental para futuras peticiones

Si la peticion es de narrativa, casi seguro empieza en:

```text
Assets/Scripts/Dialogue/IntroLvl.yarn
```

Si la peticion es de flujo entre escenas, empieza en:

```text
Assets/Scripts/Bootstrap/SceneLoader.cs
Assets/Scripts/MiniGames/MiniGameBridge.cs
ProjectSettings/EditorBuildSettings.asset
```

Si la peticion es de jugabilidad 3D, empieza en:

```text
Assets/Scenes/SampleScene.unity
Assets/Scripts/Player/PlayerController.cs
Assets/Scripts/NPC/Wolf/PetFollowController.cs
```

Si la peticion es del minijuego de respiracion, empieza en:

```text
Assets/Scenes/FirstMiniGame.unity
Assets/JetpackJoyrideTemplate/Scripts/BreathingController.cs
Assets/JetpackJoyrideTemplate/Scripts/ObstacleSpawner.cs
Assets/JetpackJoyrideTemplate/Scripts/PlayerJetpack.cs
```

Si la peticion es de menu inicial, empieza en:

```text
Assets/Scenes/Interfaz.unity
Assets/Interfaces
Assets/Scripts/Bootstrap/Bootstrapper.cs
```

Este documento debe actualizarse cuando cambien escenas, flujo de carga, scripts base, nombres de escena, comandos Yarn o arquitectura del minijuego.
