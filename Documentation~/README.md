# CLabs.Utility

Shared foundation utilities used by nearly every Core package -- collections, observable properties, extension methods, attributes, and helper classes.

## What It Provides

### Collections
| Type | Description |
|------|-------------|
| `Registry<TKey, TValue>` | Abstract dictionary wrapper for entity-scoped lookups with disposable registration |
| `SerializableDictionary<TKey, TValue>` | Unity-serializable dictionary that survives Inspector round-trips |

### Properties
| Type | Description |
|------|-------------|
| `Property<T>` | Serializable wrapper with a dirty flag for change tracking |
| `BoolProperty` | Sealed `Property<bool>` convenience type |
| `Vector2Property` | Sealed `Property<Vector2>` convenience type |

### Disposables
| Type | Description |
|------|-------------|
| `Disposable` | Wraps an `Action` as `IDisposable` for cleanup callbacks |
| `DisposableCollection` | Aggregates multiple `IDisposable` instances for batch disposal |

### Extensions
| Type | Description |
|------|-------------|
| `EnumerableExt` | `Complete()` -- eagerly iterates a sequence applying an action |
| `RectTransformExt` | `SetAnchor()`, `SetTopAnchor()`, `SetTopLeftAnchor()` |
| `CameraExtensions` | `AddLayer()`, `RemoveLayer()`, `HasLayer()`, `ExcludeLayer()` on Camera culling masks |

### Attributes
| Type | Description |
|------|-------------|
| `[ReadOnly]` | Makes a serialized field non-editable in the Inspector |
| `[EditorButton]` | Exposes a method as a clickable button in the Inspector |

### Utility Classes
| Type | Description |
|------|-------------|
| `GameObjectUtils` | `ForceComponent<T>()`, `TryDestroyComponent<T>()`, `SetLayer()` |
| `TransformUtils` | `ForceGameObject()`, `ForceComponent<T>()`, `TryDestroyComponent<T>()` |
| `StringUtils` | `ToPropertyName()`, `ToTitleCase()`, `RemoveWhiteSpace()` |
| `IOUtils` | `ForceDirectory()`, `DeleteFile()` -- safe file-system helpers |
| `BinaryConvert` | JSON-to-bytes round-trip via Newtonsoft.Json |
| `Serializer` | JSON-to-bytes round-trip via Unity's JsonUtility |
| `ReflectionUtilities` | `FindImplementors()`, `GetConstructorParams()`, `ConvertType()` |
| `CoroutineUtils` | Chainable coroutine helpers (`DelaySeconds`, `WaitUntil`, `Chain`) |
| `IncrementalAction` | Countdown that fires an action after N calls to `Decrement()` |
| `PlayerLoopInjector` | Inject a custom update callback into Unity's PlayerLoop with disposable cleanup |

## Dependencies

None. This is the foundation layer.

## Assembly

`CLabs.Utility`
