---
applyTo:
  - "**/PublicAPI.Unshipped.txt"
  - "src/Core/src/**/*.cs"
  - "src/Controls/src/**/*.cs"
  - "src/Essentials/src/**/*.cs"
---
# Public API Surface Design

> **Activation guard**: only apply this guidance when the diff actually changes
> public API surface — `public`/`protected` types or members, interface
> definitions, builder/extension methods on public types, `[Obsolete]` markers,
> or `PublicAPI.*.txt` entries. The broad `.cs` globs exist so this guidance
> loads at the moment of API design (writing `public class Foo` in `Button.cs`),
> not just when the analyzer-generated `Unshipped.txt` is updated afterward.
> If the diff only changes internals or implementation details, ignore.

## API Addition Rules
- New public APIs must have clear, demonstrated use cases — no speculative additions
- API naming must follow .NET design guidelines and be consistent with existing MAUI patterns
- Interfaces belong at `IView`/`IElement` level when behavior is cross-cutting — do not duplicate per-control

## PublicAPI.Unshipped.txt
- Entries must exactly match the actual API shape (namespace, type, member signature)

> For PublicAPI.Unshipped.txt file management workflow (never disable analyzers, `dotnet format analyzers`, revert-then-add pattern), see `copilot-instructions.md` § PublicAPI.Unshipped.txt File Management.

## Obsolescence and Removal
- Deprecated APIs must go through `[Obsolete("message")]` with migration guidance before removal
- Include the replacement API or pattern in the obsolete message
- Breaking changes require explicit design justification documented in the PR

## Visibility Decisions
- Default to `internal` — make `public` only when external consumers need access
- `protected` members in unsealed types are part of the public API surface — treat with same rigor
- Use `[EditorBrowsable(EditorBrowsableState.Never)]` for APIs that must be public for technical reasons but should not appear in IntelliSense
