# CLabs.Utility

Engine-agnostic foundation utilities used by every Core package — disposables, registries, pub/sub, colour, identity, resource providers, reflection helpers, and general-purpose helpers.

## What It Provides

### Identity
| Type | Description |
|------|-------------|
| `OwnerId` | Plain-C# readonly struct for cross-engine entity identity. Implicitly converts to/from `int` and `char`. |

### Colour
| Type | Description |
|------|-------------|
| `Color` | Engine-agnostic RGBA colour struct (floats 0–1). Factory methods: `FromRgb`, `FromRgb255`, `FromHex`, `FromHsv`, `FromHsl`, `FromCmyk`. Conversions: `ToHex`, `ToHexRgb`, `ToHsv`, `ToHsl`, `ToCmyk`. Named constants: `White`, `Black`, `Red`, `Green`, `Blue`, `Yellow`, `Cyan`, `Magenta`, `Grey`, `Transparent`. |

### Disposables
| Type | Description |
|------|-------------|
| `Disposable` | Wraps an `Action` as `IDisposable` for cleanup callbacks |
| `DisposableCollection` | Aggregates multiple `IDisposable` instances for batch disposal |

### Registries
| Type | Description |
|------|-------------|
| `Registry<TKey, TValue>` | Abstract dictionary wrapper. `Register()` returns an `IDisposable` that auto-removes the entry on dispose. Exposes `Get`, `TryGet`, `TryGetValue`, `Contains`, `Values`, `Keys`, `Count`, and events `OnRegistered`/`OnUnregistered`. |

### Pub/Sub (Event Bus)
| Type | Description |
|------|-------------|
| `EventService<TKey>` | Concrete event bus — publishes and subscribes to struct messages keyed by `(TKey, Type)` |
| `IEventService<TKey>` | Interface for the event bus |
| `EventPublisher<TKey>` / `IEventPublisher<TKey>` | Publish-only view over an `IEventService` |
| `EventSubscriber<TKey>` / `IEventSubscriber<TKey>` | Subscribe-only view over an `IEventService` |
| `PubSubFactory<TKey>` / `IPubSubFactory<TKey>` | Creates matched publisher/subscriber pairs from a shared service |
| `EventReceiver<TKey, TData>` / `IEventReceiver<TKey>` | Subscription handle carrying a key and delegate |
| `EventMessage<T>` | `delegate void EventMessage<T>(in T message)` — the message handler signature |

### Resource Providers
| Type | Description |
|------|-------------|
| `IResourceProvider` | Interface: `CanHandle`, `HasResource`, `Consume`, `Grant` |
| `IDefinition` | Marker interface for definition types (`Name` string) |
| `CompositeResourceProvider` | Routes calls to the first provider that can handle a given resource |

### Extensions
| Type | Description |
|------|-------------|
| `EnumerableExt` | `Complete<T>(Action<T>)` — eagerly iterates a sequence applying an action to each element |

### Utility Classes
| Type | Description |
|------|-------------|
| `StringUtils` | `ToPropertyName()`, `ToTitleCase()`, `RemoveWhiteSpace()` — string extension methods |
| `IOUtils` | `ForceDirectory(string)`, `DeleteFile(string)` — safe file-system helpers |
| `BinaryConvert` | JSON-to-bytes round-trip via Newtonsoft.Json: `ToBytes<T>()`, `ToJson()`, `ToObject<T>()` |
| `ReflectionUtilities` | `FindImplementors(Type)`, `GetConstructorParams(Type)`, `SelectInterfaces()`, `ConvertType(string)` — CLR type-discovery helpers |
| `IncrementalAction` | Countdown trigger: fires an `Action` after N calls to `Decrement()` |

## Dependencies

None. This is the foundation layer.

External: `Newtonsoft.Json` (for `BinaryConvert`).

## Assembly

`CLabs.Utility`
