# Psicología del Dolor

Videojuego educativo desarrollado en Unity para acompañar el aprendizaje de conceptos relacionados con la psicología y la neurociencia del dolor.

El Capítulo 1 combina exploración en tercera persona, narrativa ramificada, decisiones, ejercicios terapéuticos y minijuegos. El propósito no es evaluar al jugador de forma competitiva, sino facilitar la comprensión y aplicación progresiva de los contenidos.

## Estado del proyecto

El Capítulo 1 se encuentra en fase de estabilización e incluye:

- Menú principal y pantalla de carga.
- Exploración en tercera persona.
- Diálogos y decisiones mediante Yarn Spinner.
- Progreso narrativo organizado por puertas.
- Minijuego de respiración controlada.
- Minijuego educativo “La entiendo / La aplico”.
- Inventario, objetivos contextuales y menú de pausa.
- Audio ambiental y control de volumen.
- Mascota con seguimiento mediante NavMesh.
- Herramientas de evaluación para saltar entre etapas durante desarrollo.

La validación actual depende principalmente de pruebas manuales en Play Mode. Antes de una publicación se recomienda completar la automatización de los flujos críticos.

## Requisitos

- Unity `6000.4.4f1`.
- Git.
- Git LFS, si el repositorio remoto administra de esta forma los archivos binarios.
- Plataforma de desarrollo principal: Windows.

Abrir el proyecto con otra versión de Unity puede reserializar escenas, perfiles de URP y archivos de `ProjectSettings`.

## Instalación

```powershell
git clone <URL_DEL_REPOSITORIO>
cd Psicologia_del_dolor
git lfs pull
```

Después:

1. Abrir Unity Hub.
2. Seleccionar **Add project from disk**.
3. Elegir la raíz del repositorio.
4. Abrirlo con Unity `6000.4.4f1`.
5. Esperar a que Unity importe los assets y resuelva los paquetes.

Si Git LFS no está configurado en el remoto, se puede omitir `git lfs pull`.

## Ejecución

La escena oficial de entrada es:

```text
Assets/_Game/Scenes/Bootstrap.unity
```

Abrir esa escena y pulsar **Play**. El recorrido esperado es:

```text
Bootstrap -> Interfaz -> JUGAR -> SampleScene
```

No usar `SampleScene` como punto de entrada para validar una build completa, porque puede omitir la inicialización del menú y de servicios globales.

### Escenas incluidas en la build

| Orden | Escena | Responsabilidad |
| ---: | --- | --- |
| 0 | `Bootstrap` | Inicialización y servicios globales |
| 1 | `Interfaz` | Menú principal |
| 2 | `FirstMiniGame` | Minijuego de respiración |
| 3 | `SampleScene` | Mundo y narrativa del Capítulo 1 |

## Experiencias y decisiones del Capítulo 1

### Decisiones narrativas

Las puertas presentan conversaciones con opciones múltiples mediante Yarn Spinner. Las respuestas permiten explorar distintas actitudes del jugador sin convertir la experiencia en un examen ni aplicar castigos agresivos.

En términos técnicos:

```text
Interacción con la puerta
  -> inicia un nodo de IntroLvl.yarn
  -> Yarn presenta líneas y opciones
  -> la decisión ejecuta un comando Yarn
  -> Chapter1EnvironmentController aplica la consecuencia
  -> Chapter1ProgressState conserva el resultado temporal
```

Las ramas pueden:

- Continuar el recorrido principal.
- Iniciar un minijuego.
- Mantener una puerta bloqueada.
- Ejecutar una salida visual.
- Permitir que el jugador vuelva a intentarlo.

Las decisiones narrativas pertenecen a:

```text
Assets/_Game/Scripts/Dialogue/IntroLvl.yarn
```

Las consecuencias jugables se coordinan principalmente desde:

```text
Assets/_Game/Scripts/Chapter1/Chapter1EnvironmentController.cs
Assets/_Game/Scripts/Chapter1/Chapter1ProgressManager.cs
Assets/_Game/Scripts/Chapter1/Chapter1ProgressState.cs
```

No se deben renombrar nodos, variables, comandos o saltos Yarn existentes sin revisar primero sus referencias en C#.

### Minijuego de respiración

Se activa como consecuencia de la decisión de quedarse en la Puerta 1. El jugador completa cuatro ciclos siguiendo las fases de inhalar, sostener y exhalar.

Objetivos:

- Practicar un ritmo respiratorio controlado.
- Mantener una trayectoria predecible y terapéutica.
- Evitar presión competitiva o castigos innecesarios.

El minijuego se ejecuta en `FirstMiniGame`. Al finalizar, el sistema regresa a `SampleScene`, restaura la posición previa del jugador y continúa el diálogo pendiente. La brújula no se entrega al iniciar esta actividad; forma parte de la recompensa posterior del capítulo.

### Minijuego “La entiendo / La aplico”

Este es el juego de clasificación asociado a la Puerta 2. Su objetivo pedagógico es distinguir entre:

- **La entiendo:** el jugador reconoce o comprende una idea de forma teórica.
- **La aplico:** el jugador lleva esa idea a una conducta o decisión de la vida real.

El jugador recibe seis tarjetas con situaciones relacionadas con la educación en neurociencia del dolor. Cada tarjeta debe clasificarse en una de las dos categorías mediante botones o arrastre con el mouse.

Ejemplos:

| Situación | Categoría |
| --- | --- |
| “Comprendí que dolor no siempre significa daño.” | La entiendo |
| “Probé una actividad que había evitado por temor.” | La aplico |

Funcionamiento:

- No existe cronómetro ni pérdida de puntos.
- Una respuesta correcta avanza a la siguiente tarjeta.
- Una respuesta incorrecta muestra orientación y permite reintentar inmediatamente.
- El progreso se presenta de `1/6` a `6/6`.
- Al completar las tarjetas se marca el minijuego como terminado y continúa el flujo principal.

La interfaz se crea como un overlay sobre `SampleScene`; no carga una escena independiente.

Código principal:

```text
Assets/_Game/Scripts/MiniGames/UnderstandApplyMiniGame.cs
Assets/_Game/Scripts/MiniGames/UnderstandApplyDeck.cs
Assets/_Game/Scripts/MiniGames/UnderstandApplyCard.cs
Assets/_Game/Scripts/MiniGames/UnderstandApplyCategory.cs
Assets/_Game/Scripts/MiniGames/UnderstandApplyDraggableCard.cs
```

Contenido editable de las tarjetas:

```text
Assets/Resources/MiniGames/UnderstandApply/Chapter1UnderstandApplyDeck.asset
```

Las preguntas no deben agregarse directamente a la lógica de `UnderstandApplyMiniGame`. Se deben editar o crear mediante un `UnderstandApplyDeck` para conservar el diseño basado en datos y permitir reutilizar la mecánica en capítulos futuros.

## Controles

| Acción | Control |
| --- | --- |
| Movimiento | `WASD` o flechas |
| Cámara | Mouse |
| Interacción | Acción `Interact` indicada en pantalla |
| Avanzar diálogo | `Enter` o clic |
| Elegir opciones | Mouse o teclado según la selección activa |
| Pausa | `Escape`, fuera de un diálogo |
| Respiración: inhalar/subir | Mantener `Espacio` |
| Respiración: exhalar/bajar | Soltar `Espacio` |

Los bindings definitivos pertenecen al Input System de Unity. Si cambia un binding, se deben actualizar también los prompts visibles.

## Tecnologías principales

- Unity 6 y C#.
- Universal Render Pipeline `17.4.0`.
- Unity Input System `1.19.0`.
- AI Navigation `2.0.12`.
- Yarn Spinner para narrativa y opciones.
- uGUI para interfaces.
- ScriptableObjects para contenido reutilizable de minijuegos.

Las versiones completas están declaradas en:

```text
ProjectSettings/ProjectVersion.txt
Packages/manifest.json
```

## Arquitectura

El proyecto utiliza un enfoque iterativo basado en **vertical slices** y una arquitectura de **monolito modular compuesto por escenas**.

```text
Presentación
  Canvas, HUD, menús y Yarn
        |
Aplicación y orquestación
  progreso, puertas y puentes con minijuegos
        |
Gameplay y estado
  jugador, interacción, reglas y datos
        |
Infraestructura Unity
  escenas, audio, Input System, NavMesh y Resources
```

Regla principal de integración narrativa:

> Yarn decide la consecuencia narrativa y C# ejecuta la consecuencia jugable.

El proyecto no implementa Clean Architecture, ECS, MVC ni arquitectura hexagonal de forma estricta. Las decisiones, patrones utilizados y deuda técnica están descritos en `ENTREGA_DESARROLLADOR.md`.

## Estructura principal

```text
Assets/_Game/
├── Art/          Modelos, personajes, entornos y materiales propios
├── Editor/       Herramientas disponibles únicamente en el editor
├── MiniGames/    Assets y componentes específicos de minijuegos
├── Resources/    Recursos cargados durante ejecución
├── Scenes/       Escenas oficiales y datos NavMesh
├── Scripts/      Código organizado por responsabilidad
└── UI/           Recursos visuales de interfaz
```

Dentro de `Scripts/`:

```text
Animation/    Puentes y control de animaciones
Bootstrap/    Arranque, escenas, audio, menú, pausa y servicios
Chapter1/     Progreso y orquestación del capítulo
Dialogue/     Yarn, entrada y presentación de diálogos
Interaction/  Interactuables del mundo
MiniGames/    Flujo y lógica reutilizable de minijuegos
NPC/          Rutas, mascota y seguimiento
Player/       Movimiento y control del jugador
```

## Flujo de desarrollo

El trabajo se organiza por recorridos jugables completos:

```text
Objetivo pedagógico
  -> flujo del jugador
  -> implementación
  -> prueba desde Bootstrap
  -> ajuste de experiencia
  -> documentación
```

Antes de confirmar un cambio:

1. Comprobar que Unity compila sin errores.
2. Probar las ramas de éxito, abandono y reintento afectadas.
3. Verificar movimiento, cursor, pausa, audio y retorno entre escenas.
4. Revisar que no se hayan reserializado assets ajenos.
5. Ejecutar `git status` y revisar el diff.
6. Actualizar la documentación cuando cambien contratos o flujos.

## Documentación

| Documento | Propósito |
| --- | --- |
| [ENTREGA_DESARROLLADOR.md](ENTREGA_DESARROLLADOR.md) | Guía de incorporación, metodología, arquitectura, pruebas y diagnóstico |
| [CONTEXTO_PROYECTO.md](CONTEXTO_PROYECTO.md) | Inventario técnico detallado y contexto histórico de implementación |
| [MANUAL_FUNCIONES.md](MANUAL_FUNCIONES.md) | Referencia de funciones públicas, comandos Yarn, eventos y ejemplos de uso |

El README funciona como punto de entrada. Los detalles de funcionamiento y mantenimiento deben consultarse en esos documentos.

## Problemas frecuentes

### La partida no inicia correctamente

Confirmar que la ejecución comienza en `Bootstrap` y que las cuatro escenas oficiales están habilitadas en Build Settings.

### Una opción de diálogo no aparece

Revisar la estructura y sangría del nodo en `IntroLvl.yarn`, además del espacio disponible en la UI de opciones.

### El jugador queda bloqueado después de un minijuego

Revisar el resultado pendiente en `Chapter1ProgressState` y su consumo en `Chapter1ProgressManager`.

### La mascota no sigue el terreno

Revisar el `NavMeshSurface`, los datos horneados de `SampleScene`, el tipo de agente y la posición inicial de la mascota.

### Unity muestra cambios inesperados en configuración

Confirmar que se abrió con Unity `6000.4.4f1`. No confirmar cambios de `ProjectSettings`, URP o perfiles de volumen sin entender su origen.

## Contribución

- Mantener los cambios limitados a una responsabilidad.
- Conservar nombres de nodos, variables y comandos Yarn cuando formen parte de un flujo existente.
- Preferir referencias serializadas para dependencias estables.
- No duplicar sistemas que ya se crean mediante `RuntimeInitializeOnLoadMethod`.
- Evitar introducir una abstracción nueva si no resuelve un problema repetido.
- Documentar nuevos comandos Yarn, escenas, estados y contratos de retorno.

## Licencia y uso

El repositorio no incluye actualmente una licencia general en su raíz. Hasta que el propietario defina una, el código y los recursos deben considerarse de uso restringido.

Los assets de terceros conservan sus licencias originales. Revisar los archivos de licencia incluidos junto a cada paquete antes de redistribuir o publicar el juego.
