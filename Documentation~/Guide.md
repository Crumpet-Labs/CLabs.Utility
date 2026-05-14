# CLabs.Utility — Guide

## Overview

CLabs.Utility is the engine-agnostic foundation layer for every Core package. It has zero package dependencies (one external: Newtonsoft.Json for `BinaryConvert`). It provides the shared building blocks that higher-level packages rely on: entity identity, colour, disposables, registries, pub/sub, resource providers, reflection helpers, and general-purpose helpers.

---

## OwnerId

A plain-C# readonly struct for cross-engine entity identity. Wraps a single `int`. Converts implicitly from `int` and `char`.

```csharp
OwnerId id = 42;
int raw = id;          // implicit back to int
OwnerId fromChar = 'A';
```

---

## Color

Engine-agnostic RGBA colour (floats in `[0, 1]`). Create via factory methods; convert to engine-native types through platform adapters (e.g. `ToUnityColor()` in `CLabs.Utility.Unity`).

### Factories

| Method | Description |
|--------|-------------|
| `FromRgb(r, g, b, a)` | From float components |
| `FromRgb255(r, g, b, a)` | From 0–255 byte components |
| `FromHex(string)` | Parses `#RGB`, `#RGBA`, `#RRGGBB`, `#RRGGBBAA` |
| `FromHsv(h, s, v, a)` | Hue in degrees `[0, 360)`, S/V in `[0, 1]` |
| `FromHsl(h, s, l, a)` | Hue in degrees `[0, 360)`, S/L in `[0, 1]` |
| `FromCmyk(c, m, y, k, a)` | CMYK components in `[0, 1]` |

### Properties

| Property | Description |
|----------|-------------|
| `R`, `G`, `B`, `A` | Float components in `[0, 1]` |
| `R8`, `G8`, `B8`, `A8` | Byte components `[0, 255]` |

### Conversions out

| Method | Description |
|--------|-------------|
| `ToHex()` | `"#RRGGBBAA"` |
| `ToHexRgb()` | `"#RRGGBB"` (alpha dropped) |
| `ToHsv()` | Returns `(H, S, V)` tuple |
| `ToHsl()` | Returns `(H, S, L)` tuple |
| `ToCmyk()` | Returns `(C, M, Y, K)` tuple |

### Named constants

`White`, `Black`, `Transparent`, `Red`, `Green`, `Blue`, `Yellow`, `Cyan`, `Magenta`, `Grey`.

---

## Disposable and DisposableCollection

### Disposable

Wraps a single `Action` as `IDisposable`. Used throughout Core for cleanup callbacks, especially from `Registry.Register()`.

```csharp
IDisposable cleanup = new Disposable(() => Console.WriteLine("Cleaned up"));
cleanup.Dispose();
```

### DisposableCollection

Aggregates multiple `IDisposable` instances and disposes them all in one call. Bridge mediators use this to batch-dispose event subscriptions and registry handles.

```csharp
var collection = new DisposableCollection();
collection.Add(registry.Register("a", dataA));
collection.Add(registry.Register("b", dataB));

// Teardown — removes both entries
collection.Dispose();
```

---

## Registry\<TKey, TValue\>

An abstract dictionary wrapper. Packages subclass it to create strongly-typed registries.

### API

| Member | Description |
|--------|-------------|
| `Register(TKey, TValue)` | Adds an entry; returns an `IDisposable` that removes it on dispose |
| `Get(TKey)` | Returns the value; throws if missing |
| `TryGet(TKey, out TValue)` | Safe lookup returning `bool` |
| `TryGetValue(TKey, out TValue)` | Alias for `TryGet` |
| `Get(IEnumerable<TKey>)` | Batch lookup; silently skips missing keys |
| `Contains(TKey)` | Checks if a key is registered |
| `Values` / `Keys` / `Count` | Enumeration and count |
| `OnRegistered` / `OnUnregistered` | Events fired on registration/removal |

### Usage

```csharp
public sealed class WeaponRegistry : Registry<string, WeaponData> { }

var registry = new WeaponRegistry();
IDisposable handle = registry.Register("sword", swordData);

WeaponData weapon = registry.Get("sword");

handle.Dispose(); // auto-removes
```

---

## Pub/Sub (EventService)

A lightweight in-process event bus. Messages are structs keyed by a `(TKey, Type)` pair. Subscriptions return an `IDisposable` that auto-unsubscribes.

### Types

| Type | Role |
|------|------|
| `EventService<TKey>` | Concrete bus; implements `IEventService<TKey>` |
| `EventPublisher<TKey>` | Publish-only facade |
| `EventSubscriber<TKey>` | Subscribe-only facade |
| `PubSubFactory<TKey>` | Creates matched publisher/subscriber pairs |
| `EventReceiver<TKey, TData>` | Binds a key to an `EventMessage<TData>` handler |

### Usage

```csharp
var service = new EventService<string>();
var factory = new PubSubFactory<string>(service);

IEventSubscriber<string> subscriber = factory.CreateSubscriber();
IEventPublisher<string> publisher = factory.CreatePublisher();

IDisposable sub = subscriber.Subscribe(new IEventReceiver<string>[] {
    new EventReceiver<string, DamageEvent>(
        ("player", typeof(DamageEvent)),
        (in DamageEvent e) => Console.WriteLine($"Damage: {e.Amount}")
    )
});

publisher.Publish("player", new DamageEvent { Amount = 10 });

sub.Dispose(); // unsubscribes
```

---

## Resource Providers

A small resource-management abstraction for consuming and granting countable resources.

### IResourceProvider

| Method | Description |
|--------|-------------|
| `CanHandle(IDefinition)` | Returns `true` if this provider manages the given resource |
| `HasResource(IDefinition, int)` | Checks whether sufficient quantity is available |
| `Consume(IDefinition, int)` | Deducts the quantity |
| `Grant(IDefinition, int)` | Adds the quantity |

### CompositeResourceProvider

Routes each call to the first `IResourceProvider` in its list that `CanHandle` the resource.

```csharp
var composite = new CompositeResourceProvider(goldProvider, gemProvider);
composite.Consume(goldDef, 10);
```

### IDefinition

Marker interface for resource/item definition types. Requires a `Name` property. Typically implemented by ScriptableObjects in adapters.

---

## Extensions

### EnumerableExt

| Method | Description |
|--------|-------------|
| `Complete<T>(Action<T>)` | Eagerly iterates a sequence, invoking the action on each element |

```csharp
items.Complete(item => item.Initialize());
```

---

## Utility Classes

### StringUtils

Extension methods on `string`.

| Method | Description |
|--------|-------------|
| `ToPropertyName()` | TitleCase + remove whitespace (`"my field"` → `"MyField"`) |
| `ToTitleCase()` | Title-cases using current culture |
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
| `ToObject<T>(this byte[])` | Deserializes UTF-8 bytes to `T` |

### ReflectionUtilities

Reflection helpers for type discovery.

| Method | Description |
|--------|-------------|
| `FindImplementors(this Type)` | Scans all loaded assemblies for types assignable to the given type |
| `GetConstructorParams(this Type)` | Returns parameter types of the first constructor |
| `SelectInterfaces(this IEnumerable<Type>)` | Filters to interface types only |
| `ConvertType(string)` | Maps CLR full type names to C# keyword aliases (`System.Int32` → `int`) |

### IncrementalAction

A countdown trigger. Initialize with a count and an action; each call to `Decrement()` reduces the counter. When it reaches zero the action fires. Useful for waiting on multiple async completions.

```csharp
var barrier = new IncrementalAction(3, () => Console.WriteLine("All loaded"));
barrier.Decrement();
barrier.Decrement();
barrier.Decrement(); // prints "All loaded"
```

---

## Dependencies

None. CLabs.Utility is the foundation layer with no Core package dependencies.

External: `Newtonsoft.Json` (for `BinaryConvert`).

## Assembly

`CLabs.Utility`
