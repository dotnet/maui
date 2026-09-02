# Handler property-update batching

Handler property-update batching is an experimental, internal optimization that reduces repeated property-mapper work without changing the XAML or handler API surface.

## Behavior

Two independently gated modes are available:

- Explicit batches coalesce property updates between `VisualElement.BatchBegin()` and `BatchCommit()`.
- Automatic batches map the first top-level update immediately, then coalesce later top-level updates until the queued dispatcher callback runs.

Both modes require the `Microsoft.Maui.Experimental.HandlerUpdateBatching` `AppContext` switch. Automatic batching additionally requires `Microsoft.Maui.Experimental.HandlerUpdateBatching.AutoDispatch`.

The switches must be set before the first `VisualElement` type initialization because their values are cached in static fields.

## Ordering and barriers

Only top-level property notifications are deferred. An `UpdateValue` requested by a property mapper runs synchronously so dependencies such as formatting, container creation, clipping, shadows, and borders retain their existing order.

Pending properties are distinct and retain relative last-occurrence order. For example, `A, B, A` flushes as `B, A`.

Handlers reuse an indexed pending-property list, its lookup dictionary, and the dispatcher callback after their first automatic batch. This keeps enqueue and move-to-last operations O(1) without allocating linked-list nodes or callback closures for each batch.

Accessing `PlatformView` or `ContainerView`, invoking any command -- including measure invalidation -- committing an explicit batch, changing the virtual view, and disconnecting the handler are barriers. Platform-view access does not flush while a mapper is executing.

Measure invalidation is not coalesced: `InvalidateMeasure` is never itself queued and is always dispatched synchronously. However, like every other command, any pending property updates queued earlier in the same batch are flushed immediately beforehand, so the platform view reflects current values before a layout pass is requested. Disconnecting a handler is the one exception: pending updates are discarded, not flushed, since applying them to a handler that is being torn down would be wasted (or unsafe) work.

At flush time, the handler:

1. verifies that platform mappers can still run;
2. resolves the current mapper action, including chained mapper customization;
3. invokes the mapper with reentrant dependency updates kept synchronous.

When neither explicit nor automatic batching is active, `UpdateValue`, command invocation, and initial mapper application use the existing direct mapper path. Platform-view access reduces to a zero pending-count check. The disabled path does not allocate or enter mapper-depth tracking.

## Rollout

The intended rollout is:

1. validate explicit batching first;
2. keep automatic dispatcher batching behind its separate switch until device correctness, feature-disabled overhead, sparse latency, and end-to-render latency are measured;
3. enable broader use only after iOS, Android, and Mac Catalyst lifecycle and rendering validation.

Benchmark results must compare the base branch, new code with batching disabled, explicit batching, and automatic batching. Timed mapper callbacks must not perform logging, string formatting, or shared metric aggregation. Mapper-call reduction alone is not an end-to-end startup or rendering result.
