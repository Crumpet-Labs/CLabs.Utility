# CLabs.Utility

Engine-agnostic foundation utilities used across the CLabs ecosystem: entity identity, colour, disposables, generic registries, resource providers, reflection helpers, and small general-purpose helpers. Pure .NET, no engine references, no CLabs dependencies.

## What it provides

| Type | Purpose |
|------|---------|
| `OwnerId` | Plain `readonly struct` for cross-engine entity identity. Wraps an `int`; implicitly converts to and from `int` / `char`. |
| `Color` | Engine-agnostic RGBA colour struct (floats `0–1`). Factories for RGB / HSV / HSL / CMYK / Hex; conversions back via `ToHex`, `ToHsv`, `ToHsl`, `ToCmyk`; named constants (`White`, `Red`, `Transparent`, …). |
| `Registry<TKey, TValue>` | Abstract dictionary wrapper. `Register()` returns an `IDisposable` that auto-removes the entry on dispose; exposes `Get` / `TryGet` / `Contains` / events `OnRegistered` / `OnUnregistered`. |
| `Disposable` / `DisposableCollection` | Wraps an `Action` as `IDisposable`; aggregates multiple disposables for batch teardown. |
| `IResourceProvider` / `CompositeResourceProvider` / `IDefinition` | Resource-management abstraction: `CanHandle` / `HasResource` / `Consume` / `Grant`. Composite routes calls to the first matching provider. |
| `StringUtils` | `ToPropertyName`, `ToTitleCase`, `RemoveWhiteSpace`. |
| `IOUtils` | `ForceDirectory(string)`, `DeleteFile(string)`. Filesystem helpers that do not throw when the target is absent. |
| `BinaryConvert` | JSON ⇄ bytes round-trip via Newtonsoft.Json. |
| `ReflectionUtilities` | `FindImplementors`, `GetConstructorParams`, `SelectInterfaces`, `ConvertType`. |
| `IRandomSource` | `NextFloat()` / `NextInt(minInclusive, maxExclusive)`, so randomness is injected rather than reached for statically. Ships `SystemRandomSource` (clock-seeded) and `SeededRandomSource` (reproducible). |
| `IncrementalAction` | Countdown trigger. Fires its action once `Decrement()` has been called N times. |
| `EnumerableExt.Complete<T>(Action<T>)` | Eagerly iterates a sequence, invoking the action on each element. |

## Installation

CLabs.Utility is the foundation layer and has no CLabs dependencies. Install it directly.

### .NET projects

Clone the repo (or add as a submodule) and reference the project from your `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="path/to/CLabs.Utility/CLabs.Utility.csproj" />
</ItemGroup>
```

Or, once published, via NuGet:

```bash
dotnet add package CLabs.Utility
```

### Dependencies

- **CLabs**: none. This is the foundation layer.
- **External**: `Newtonsoft.Json` (only required if you use `BinaryConvert`).

## Using it

### Cross-engine entity identity

```csharp
using CLabs.Utility;

OwnerId player = 1;          // implicit from int
OwnerId fromChar = 'A';      // implicit from char
int raw = player;            // implicit back to int
```

### Engine-agnostic colour

```csharp
using CLabs.Utility;

var red    = Color.Red;
var custom = Color.FromHex("#FFB000");
var hsv    = Color.FromHsv(120f, 1f, 1f);
string hex = custom.ToHex();   // "#FFB000FF"
```

### Self-removing registry

```csharp
public sealed class WeaponRegistry : Registry<string, WeaponData> { }

var registry = new WeaponRegistry();
IDisposable handle = registry.Register("sword", swordData);

if (registry.TryGet("sword", out var weapon)) {
    // use it
}

handle.Dispose();   // entry auto-removed
```

### Batched cleanup

```csharp
var collection = new DisposableCollection();
collection.Add(registry.Register("a", dataA));
collection.Add(registry.Register("b", dataB));

collection.Dispose();   // disposes both registrations in one call
```

## Unity users

If you're building a Unity project, install the [CLabs.Unity](https://github.com/Crumpet-Labs/CLabs.Unity) UPM umbrella, which ships Utility together with the rest of the CLabs ecosystem plus the Unity adapters that bridge `Color` / `OwnerId` to their engine-native equivalents. This repo is for plain .NET consumers.

## License

MIT.
