# CLabs.Utility -- Guide

## Overview

CLabs.Utility is the foundation layer for every Core package. It has zero dependencies beyond UnityEngine and Newtonsoft.Json, and provides the shared building blocks that higher-level packages rely on: entity-scoped registries, serializable collections, observable properties, extension methods, Inspector attributes, and general-purpose helpers.

## Registry\<TKey, TValue\>

An abstract dictionary wrapper designed for entity-scoped lookups. Packages subclass it to create strongly typed registries (e.g., `StatRegistry : Registry<StatDefinition, StatInstance>`).

### API

| Member | Description |
|--------|-------------|
| `Register(TKey, TValue)` | Adds an entry and returns an `IDisposable` that removes it on dispose |
| `Get(TKey)` | Returns the value for a key; throws if missing |
| `TryGet(TKey, out TValue)` | Safe lookup returning `bool` |
| `TryGetValue(TKey, out TValue)` | Alias for `TryGet` |
| `Get(IEnumerable<TKey>)` | Batch lookup; silently skips missing keys |
| `Contains(TKey)` | Checks if a key is registered |
| `Values` | All registered values |
| `Keys` | All registered keys |
| `Count` | Number of entries |

### Usage

```csharp
// Define a concrete registry
public sealed class WeaponRegistry : Registry<string, WeaponData> { }

// Register and auto-cleanup
var registry = new WeaponRegistry();
IDisposable handle = registry.Register("sword", swordData);

WeaponData weapon = registry.Get("sword");

// Unregister when done
handle.Dispose();
```

The disposable registration pattern is central to Core's architecture. Bridge mediators collect registration handles into a `DisposableCollection` and dispose them all on teardown.

---

## SerializableDictionary\<TKey, TValue\>

A generic dictionary that Unity can serialize through the Inspector. It implements `ISerializationCallbackReceiver` to sync between parallel `List<TKey>` / `List<TValue>` backing fields and an in-memory `Dictionary<TKey, TValue>`.

### API

| Member | Description |
|--------|-------------|
| `Dictionary` | The underlying `Dictionary<TKey, TValue>` |
| `Add(TKey, TValue)` | Add an entry |
| `Remove(TKey)` | Remove an entry |
| `TryGetValue(TKey, out TValue)` | Safe lookup |
| `ContainsKey(TKey)` | Check for key existence |
| `Clear()` | Remove all entries |
| `this[TKey]` | Indexer for get/set |
| `Count` | Number of entries |
| `Keys` / `Values` | Enumerables over keys and values |

### Usage

```csharp
[Serializable]
public class MyConfig : ScriptableObject {
    [SerializeField] private SerializableDictionary<string, int> m_Scores = new();
    
    public int GetScore(string name) {
        m_Scores.TryGetValue(name, out var score);
        return score;
    }
}
```

A custom property drawer (`SerializableDictionaryDrawer`) renders key-value pairs in the Inspector.

---

## Property\<T\>

A serializable wrapper around a value with a dirty flag for change tracking. The dirty flag is set whenever `Value` is assigned and must be manually consumed via `DirtyFlagConsumed()`.

### API

| Member | Description |
|--------|-------------|
| `Value` | Get or set the wrapped value; setting marks the property as dirty |
| `IsDirty` | `true` if the value has been set since the last consume |
| `DirtyFlagConsumed()` | Resets the dirty flag to `false` |

### Usage

```csharp
var health = new Property<float>(100f);

health.Value = 80f;
Debug.Log(health.IsDirty); // true

// Presenter reads and consumes
if (health.IsDirty) {
    UpdateHealthBar(health.Value);
    health.DirtyFlagConsumed();
}
```

### Typed Properties

Two sealed convenience types are provided:

- **`BoolProperty`** -- `Property<bool>`
- **`Vector2Property`** -- `Property<Vector2>`

---

## Disposable and DisposableCollection

### Disposable

Wraps a single `Action` as `IDisposable`. Used throughout Core for cleanup callbacks, especially from `Registry.Register()`.

```csharp
var cleanup = new Disposable(() => Debug.Log("Cleaned up"));
cleanup.Dispose(); // prints "Cleaned up"
```

### DisposableCollection

Aggregates multiple `IDisposable` instances and disposes them all in one call. Bridge mediators use this to batch-dispose event subscriptions and registry handles.

```csharp
var collection = new DisposableCollection();
collection.Add(registry.Register("a", dataA));
collection.Add(registry.Register("b", dataB));

// Teardown -- removes both entries
collection.Dispose();
```

---

## Extensions

### EnumerableExt

| Method | Description |
|--------|-------------|
| `Complete<T>(Action<T>)` | Eagerly iterates a sequence, invoking the action on each element |

```csharp
items.Complete(item => item.Initialize());
```

### RectTransformExt

| Method | Description |
|--------|-------------|
| `SetAnchor(Vector2)` | Sets anchorMin, anchorMax, and pivot to the same point while preserving size |
| `SetTopAnchor()` | Shorthand for `SetAnchor(0.5, 1)` |
| `SetTopLeftAnchor()` | Shorthand for `SetAnchor(0, 1)` |

### CameraExtensions

| Method | Description |
|--------|-------------|
| `AddLayer(LayerMask)` | Adds a layer to the camera's culling mask |
| `RemoveLayer(LayerMask)` | Removes a layer from the culling mask |
| `HasLayer(LayerMask)` | Checks if a layer is in the culling mask |
| `ExcludeLayer(LayerMask)` | Alias for `RemoveLayer` |

---

## Attributes

### [ReadOnly]

Marks a serialized field as non-editable in the Inspector. The field is still visible but greyed out. Backed by a custom `PropertyDrawer` in `Attributes/Editor/ReadOnlyDrawer.cs`.

```csharp
[SerializeField, ReadOnly] private int m_Id;
```

### [EditorButton]

Attribute for methods. Exposes a named button in the Inspector that invokes the method when clicked. Backed by a UI Toolkit editor in `Attributes/Editor/EditorButtonUIE.cs`.

```csharp
[EditorButton("Reset Stats", ResetStats)]
public void ResetStats() { ... }
```

---

## Utility Classes

### GameObjectUtils

Extension methods on `GameObject`.

| Method | Description |
|--------|-------------|
| `ForceComponent<T>()` | Returns the existing component or adds one if missing |
| `TryDestroyComponent<T>()` | Destroys the component if it exists; returns whether it was found |
| `SetLayer(int)` | Sets the GameObject's layer |

### TransformUtils

Static helpers and extension methods on `Transform`.

| Method | Description |
|--------|-------------|
| `ForceGameObject(string, bool)` | Finds or creates a root GameObject by name; optionally marks it DontDestroyOnLoad |
| `ForceComponent<T>()` | Extension on Transform -- returns existing or adds a new component |
| `TryDestroyComponent<T>()` | Extension on Transform -- destroys component if present |

```csharp
// Ensure a persistent root exists
var root = TransformUtils.ForceGameObject("Systems", dontDestroyOnLoad: true);
var manager = root.ForceComponent<AudioManager>();
```

### StringUtils

Extension methods on `string`.

| Method | Description |
|--------|-------------|
| `ToPropertyName()` | Converts to TitleCase and removes whitespace (`"my field"` -> `"MyField"`) |
| `ToTitleCase()` | Title-cases the string using current culture |
| `RemoveWhiteSpace()` | Strips all spaces |

### IOUtils

Safe file-system operations.

| Method | Description |
|--------|-------------|
| `ForceDirectory(string)` | Creates the directory if it does not exist |
| `DeleteFile(string)` | Deletes the file if it exists |

### BinaryConvert

JSON-to-bytes round-trip using **Newtonsoft.Json**.

| Method | Description |
|--------|-------------|
| `ToBytes<T>(this T)` | Serializes an object to UTF-8 JSON bytes |
| `ToBytes<T>(this string)` | Encodes a JSON string to UTF-8 bytes |
| `ToJson(this byte[])` | Decodes UTF-8 bytes to a JSON string |
| `ToObject<T>(this byte[])` | Deserializes UTF-8 bytes to an object |

### Serializer

JSON-to-bytes round-trip using **Unity's JsonUtility**. Classes must be marked `[Serializable]`.

| Method | Description |
|--------|-------------|
| `Serialize<T>(this T)` | Serializes to UTF-8 bytes via `JsonUtility.ToJson` |
| `Deserialize<T>(this byte[])` | Deserializes from UTF-8 bytes via `JsonUtility.FromJson` |

### ReflectionUtilities

Reflection helpers for type discovery.

| Method | Description |
|--------|-------------|
| `FindImplementors(this Type)` | Scans all loaded assemblies for types assignable to the given type |
| `GetConstructorParams(this Type)` | Returns parameter types of the first constructor |
| `SelectInterfaces(this IEnumerable<Type>)` | Filters to interface types only |
| `ConvertType(string)` | Maps CLR full type names to C# keyword aliases (`System.Int32` -> `int`) |

### CoroutineUtils

Chainable coroutine building blocks for `StartCoroutine`.

| Method | Description |
|--------|-------------|
| `Chain(this IEnumerator[])` | Chains an array of coroutines into a single sequence |
| `ExecuteCoroutines(this MonoBehaviour, params IEnumerator[])` | Starts multiple coroutines in parallel |
| `DelaySeconds(Action, float)` | Waits, then invokes an action |
| `WaitUntil(Func<bool>)` | Yields until predicate is true |
| `WaitWhile(Func<bool>)` | Yields while predicate is true |
| `WaitForSeconds(float)` | Yields for a duration (scaled time) |
| `WaitForSecondsRealtime(float)` | Yields for a duration (unscaled time) |
| `WaitForUpdate()` | Yields one frame |
| `WaitForFixedUpdate()` | Yields until next FixedUpdate |
| `WaitForEndOfFrame()` | Yields until end of frame |
| `Do(Action)` | Executes an action, then yields one frame |

```csharp
StartCoroutine(new IEnumerator[] {
    CoroutineUtils.Do(() => Debug.Log("Start")),
    CoroutineUtils.WaitForSeconds(2f),
    CoroutineUtils.Do(() => Debug.Log("Done"))
}.Chain());
```

### IncrementalAction

A countdown trigger. Initialize with a count; each call to `Decrement()` reduces the counter. When it reaches zero the action fires. Useful for waiting on multiple async completions.

```csharp
var barrier = new IncrementalAction(3, () => Debug.Log("All loaded"));
// Called from three separate callbacks:
barrier.Decrement();
barrier.Decrement();
barrier.Decrement(); // prints "All loaded"
```

### PlayerLoopInjector

Injects a custom update callback into Unity's native PlayerLoop under the `Update` phase. Returns an `IDisposable` that cleanly removes the subsystem on dispose.

```csharp
struct MyCustomUpdate { }

IDisposable sub = PlayerLoopInjector.InjectUpdate<MyCustomUpdate>(() => {
    // Runs every frame without a MonoBehaviour
});

// Stop the update
sub.Dispose();
```

The type parameter (`MyCustomUpdate`) acts as a unique tag so the injector can find and remove the correct subsystem later.

---

## Editor Utilities

The `Editor/` folder contains Inspector tooling (behind an Editor asmdef):

- **AssetUtilities** -- asset lookup helpers
- **ScriptableObjectList** -- editor window listing ScriptableObjects
- **UIToolkitUtils** -- UI Toolkit helper methods
- **SerializableDictionaryDrawer** -- custom property drawer for `SerializableDictionary`
- **ReadOnlyDrawer** -- property drawer for `[ReadOnly]`
- **EditorButtonUIE** -- UI Toolkit inspector for `[EditorButton]`
- **PropertyDrawer_Bool / PropertyDrawer_Vector2** -- drawers for typed properties

---

## Dependencies

None. CLabs.Utility is the foundation layer with no Core package dependencies.

External: `UnityEngine`, `Newtonsoft.Json` (for `BinaryConvert`).
