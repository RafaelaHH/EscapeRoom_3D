# Casos 06–10: código, merges y evidencias

Guía complementaria para ejecutar los casos en el proyecto Escape Room 3D sin convertir los conflictos en simulaciones.

## Antes de empezar

Estado observado al preparar esta guía: estás en `dev`, sincronizada con `origin/dev`. Los scripts `PlayerController.cs` y `PlayerInteraction.cs` existen en `Feature_PlayerMovement` y `Feature_Interaction`, pero todavía no están en `dev`. Unity está configurado con **Visible Meta Files** y **Force Text**, y el proyecto usa Unity 2022.3.62f2.

1. En VS Code confirma `dev` en la barra inferior y que Source Control no muestre cambios pendientes.
2. Desde Source Control, integra primero `Feature_PlayerMovement` en `dev`; luego integra `Feature_Interaction` en `dev`. Haz Push de `dev` y comprueba que ambos scripts aparecen allí. Esto crea la base común para el Caso 06.
3. No integres ni publiques `dev` hacia `main` durante estos casos.
4. Para cada caso crea dos ramas desde el mismo commit de `dev`: por ejemplo `Caso06_A` y `Caso06_B`. Cada autor trabaja en su propia copia o rama, hace commit y Push. El integrador se cambia a `dev`, integra A y luego B. Si estás en una sola copia, crea ambas ramas antes de integrar A.
5. Después de resolver, guarda los archivos, Stage, crea el commit de merge y haz Push de `dev`. Abre Unity y valida antes de empezar el caso siguiente.

Si prefieres conservar los nombres de la guía, reutiliza `Feature_PlayerMovement` y `Feature_Interaction`, pero actualízalas primero desde `dev` y no las edites en paralelo sin confirmar que parten del mismo commit.

> Si una escena o prefab se combina automáticamente, no fabriques un conflicto dañino. Revisa el resultado y documenta el merge. Para forzar un conflicto controlado en los casos 7 y 8, ambos autores cambian además el mismo campo serializado indicado abajo.

## Caso 06 — PlayerController

En la rama A agrega esta línea al `Update()` después de `Move()`:

```csharp
private void Update()
{
    Move();
    Debug.Log("Movement system active");
}
```

En la rama B edita el mismo `Update()` así:

```csharp
private void Update()
{
    Move();
    if (Input.GetKeyDown(KeyCode.E))
    {
        Debug.Log("Interaction key pressed");
    }
}
```

Haz commits independientes (`feat: log player movement` y `feat: add interaction key feedback`). En `dev`, integra A y después B. En Merge Editor, conserva ambas intenciones; el `Update()` final debe quedar así:

```csharp
private void Update()
{
    Move();
    Debug.Log("Movement system active");
    if (Input.GetKeyDown(KeyCode.E))
    {
        Debug.Log("Interaction key pressed");
    }
}
```

El resto de `PlayerController` conserva el `Move()` existente. En Unity asigna el script a un Capsule/Cube Player, entra a Play y prueba WASD/flechas y E.

**Capturas:** `01` ramas A/B con sus commits; `02` Source Control mostrando `PlayerController.cs` en Merge Changes; `03` Merge Editor mostrando Current, Incoming y Result; `04` código final y commit de merge en `dev`; `05` Play Mode y Console con los mensajes de movimiento/interacción.

## Caso 07 — Conflicto en Player.prefab

1. En `dev`, crea un objeto `Player` (Capsule), agrega `PlayerController` y guárdalo como `Assets/02_Prefabs/Player.prefab`. Haz commit base antes de crear las ramas A/B.
2. En la rama A cambia `Speed` de `5` a `6` en el prefab.
3. En la rama B, partiendo del mismo prefab base, cambia `Speed` de `5` a `4` y agrega un componente `Animator` al mismo prefab. Guarda el prefab y haz commit.
4. Integra A y B en `dev`. Si Git no marca conflicto, revisa si conservó ambos cambios. Si hay conflicto, abre Merge Editor y conserva el componente Animator y un valor de Speed decidido por el equipo (por ejemplo `5`). No aceptes un lado completo sin revisar.

**Capturas:** `01` Player.prefab base en Project/Inspector; `02` los cambios A y B; `03` Merge Changes/Merge Editor o diff del prefab; `04` Inspector final mostrando `PlayerController` y `Animator`; `05` Play Mode con el jugador moviéndose y Console sin errores.

## Caso 08 — Conflicto en MainScene.unity

Añade estos componentes de prueba en `Assets/01_Scripts/Environment/` antes de crear las ramas. Crea los archivos en la ruta indicada y deja que Unity genere sus `.meta`:

`ChestController.cs`

```csharp
using UnityEngine;

public class ChestController : MonoBehaviour
{
    [SerializeField] private bool unlocked;

    public void Unlock()
    {
        unlocked = true;
        Debug.Log("Cofre desbloqueado.");
    }

    public void Interact()
    {
        Debug.Log(unlocked ? "Cofre abierto." : "El cofre está cerrado.");
    }
}
```

`DoorController.cs`

```csharp
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public void Interact()
    {
        Debug.Log("Puerta abierta.");
    }
}
```

1. Usa `MainScene` como base común. En rama A agrega un Cube llamado `Chest`, con su Box Collider y `ChestController`.
2. En rama B agrega un Cube llamado `Door`, con Box Collider y `DoorController`.
3. Para asegurar un conflicto textual controlado, cambia también el mismo campo `Intensity` de `Directional Light`: A usa `0.8`, B usa `1.2`. Guarda la escena en ambas ramas.
4. Integra A y B desde `dev`. En la resolución conserva Chest y Door una sola vez y elige manualmente una intensidad razonable, por ejemplo `1.0`.

Los cubos sirven como prototipos; todavía no se requiere animación ni abrirlos físicamente. Comprueba ambos objetos en Hierarchy y sus componentes en Inspector.

**Capturas:** `01` escena base y ramas; `02` Source Control mostrando el conflicto de `MainScene.unity`; `03` Merge Editor/diff; `04` Hierarchy con Chest y Door, Inspector y luz final; `05` Play Mode y Console.

## Caso 09 — Archivo eliminado vs. modificado

Antes de crear las ramas, crea y commitea en `dev` `Assets/01_Scripts/Environment/OldDoorController.cs`:

```csharp
using UnityEngine;

public class OldDoorController : MonoBehaviour
{
    public bool isOpen;

    public void Open()
    {
        isOpen = true;
        Debug.Log("Puerta antigua abierta.");
    }
}
```

Desde ese mismo commit:

- Rama A elimina `OldDoorController.cs` y su `.meta`, commit `refactor: remove old door controller`.
- Rama B modifica el mensaje del método y agrega `Debug.Log("Controlador antiguo actualizado.");`, commit `fix: update old door controller`.

Integra A y luego B en `dev`. La decisión de este ejercicio será **mantener la eliminación**, porque `DoorController` de Caso 08 lo reemplaza. En el conflicto conserva la eliminación del `.cs` y del `.meta`; después busca `OldDoorController` en todo el proyecto y confirma que no quedan referencias. Si el equipo decide conservar el script, documenta por qué y valida que no haya dos controladores para la misma puerta.

**Capturas:** `01` archivo base; `02` commit de eliminación y commit de modificación; `03` conflicto Delete/Modify en Source Control; `04` resolución más búsqueda global sin referencias; `05` Door en Unity y Console sin Missing Script.

## Caso 10 — Add/Add de PuzzleManager

Confirma primero que `PuzzleManager.cs` no exista en el commit base de `dev`. Desde el mismo commit crea dos ramas.

Rama A, `Assets/01_Scripts/Puzzle/PuzzleManager.cs`:

```csharp
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private string correctCode = "1234";

    public bool ValidateCode(string code)
    {
        return code == correctCode;
    }
}
```

Rama B, mismo nombre y ruta:

```csharp
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private string secretCode = "1234";
    [SerializeField] private ChestController chest;

    public bool ValidateCode(string input)
    {
        if (input != secretCode)
        {
            Debug.Log("Código incorrecto.");
            return false;
        }

        if (chest != null)
        {
            chest.Unlock();
        }

        Debug.Log("Código correcto.");
        return true;
    }
}
```

Integra ambas ramas en `dev`. El resultado integrado debe ser una sola clase coherente; usa esta versión:

```csharp
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private string correctCode = "1234";
    [SerializeField] private ChestController chest;

    public bool ValidateCode(string code)
    {
        bool valid = code == correctCode;
        if (valid)
        {
            Debug.Log("Código correcto.");
            if (chest != null)
            {
                chest.Unlock();
            }
        }
        else
        {
            Debug.Log("Código incorrecto.");
        }

        return valid;
    }
}
```

Resuelve también `PuzzleManager.cs.meta` si aparece como Add/Add: deja un único `.meta` y verifica que Unity compile. Añade el componente a un GameObject `PuzzleManager` y asígnale el `Chest` en el Inspector. Para probarlo temporalmente, llama a `ValidateCode("1234")` y `ValidateCode("0000")` desde un botón/UI o un método de prueba; el código correcto desbloquea el cofre.

**Capturas:** `01` base donde se ve que el archivo no existe; `02` ambas implementaciones y commits; `03` Add/Add en Merge Changes/Merge Editor; `04` script final y único `.meta`; `05` Inspector con `Chest` asignado y Console mostrando correcto/incorrecto y cofre desbloqueado.

## Capturas y cierre de cada caso

Toma las capturas directamente en VS Code/Unity con contexto visible: rama `dev` en la barra inferior, nombre del repositorio, archivo en conflicto y operación. No recortes la imagen a solo “Merge successful”. Al terminar cada caso, comprueba que `dev` quedó limpio, haz Push, abre Unity y valida antes de continuar. Guarda cada grupo en una carpeta, por ejemplo `Evidencias/Caso06/`, y registra en la bitácora qué conflicto ocurrió, qué intención conservaste y cómo probaste el resultado.
