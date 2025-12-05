# JSON Stream Chunker Architecture

## Overview

The JSON Stream Chunker solves a unique problem: converting a stream of **complete JSON objects** (where each represents a progressive state of a growing object) back into **partial JSON chunks** that, when concatenated, produce valid JSON matching the final state.

This is the inverse of what most JSON streaming parsers do. Instead of parsing partial JSON into objects, we're comparing complete objects and emitting the minimal string chunks needed to transform from one state to the next.

## The Problem

An AI model returns complete JSON objects at each step:
```
Line 1: {"name":"Mat"}
Line 2: {"name":"Matthew"}
Line 3: {"name":"Matthew","age":32}
```

We need to output chunks:
```
Chunk 1: {"name":"Mat
Chunk 2: thew
Chunk 3: ","age":32}
```

Concatenated: `{"name":"Matthew","age":32}` ✓

### Key Challenges

1. **Property reordering**: The AI model may reorder properties between lines
2. **Only one thing changes at a time**: Despite reordering, semantically only one value grows per chunk
3. **Strings grow, primitives don't**: Numbers, booleans, null arrive complete
4. **Nested structures**: Objects and arrays can be deeply nested and grow progressively
5. **No lookahead abuse**: We can delay by 1-2 chunks for accuracy, but must stream output

## Core Architecture

### The Stack-Based State Machine

The chunker maintains a **stack** representing the current "open" JSON structure:

```
Stack State Example:
┌─────────────────────────────────────────┐
│ Level 3: Property "name" = "Mat...      │  ← Currently editing (OPEN)
├─────────────────────────────────────────┤
│ Level 2: Array index [0]                │  ← Open (no closing ])
├─────────────────────────────────────────┤
│ Level 1: Property "days"                │  ← Open (no closing })
├─────────────────────────────────────────┤
│ Level 0: Root object                    │  ← Open (no closing })
└─────────────────────────────────────────┘
```

Each stack frame tracks:
- **Type**: Object, Array, or Property
- **Path**: The JSON path to this element (e.g., `days[0].name`)
- **State**: Open (still being written) or Closed (complete)
- **EmittedKeys**: For objects, which properties we've already emitted

### The Diff Engine

When a new JSON chunk arrives, we:

1. **Flatten both objects** into path → value dictionaries
2. **Compare by path** (immune to property reordering)
3. **Categorize changes**:
   - **New paths**: Properties/elements that didn't exist before
   - **Extended values**: Strings that grew longer
   - **Completed values**: Values that stopped changing (detected via next chunk)

### Change Detection

```
Previous: {"title":"Mount"}
Current:  {"title":"Mount Fuji"}

Flattened Previous: { "title" → "Mount" }
Flattened Current:  { "title" → "Mount Fuji" }

Diff: Path "title" extended from "Mount" to "Mount Fuji"
      → Emit: " Fuji" (just the extension)
```

## The Algorithm

### Phase 1: Normalize & Flatten

Convert each JSON object to a dictionary of `path → value`:

```csharp
{
  "title": "Trip",
  "days": [
    { "name": "Day 1" }
  ]
}

Flattens to:
{
  ""                → (object marker)
  "title"           → "Trip"
  "days"            → (array marker)
  "days[0]"         → (object marker)
  "days[0].name"    → "Day 1"
}
```

### Phase 2: Diff Analysis

Compare previous and current flattened dictionaries:

```csharp
class Diff
{
    List<string> NewPaths;           // Paths that didn't exist
    List<string> RemovedPaths;       // Paths that disappeared (rare)
    List<(string, string, string)> ExtendedStrings;  // (path, old, new)
    List<(string, object, object)> ChangedValues;    // Non-string changes
}
```

### Phase 3: Stack Reconciliation

The key insight: **changes happen depth-first**. When something higher in the tree changes, everything below it that was "open" must be closed first.

```
Current Stack (open):
  Root → days → [0] → activities → [2] → "description"
                                          ↑ currently writing

New change at: days[0].activities[3]  (new array element)

Action:
  1. Close "description" string: emit `"`
  2. Close activities[2] object: emit `}`  
  3. Emit comma: `,`
  4. Open activities[3]: emit `{`
  5. Start new property...
```

### Phase 4: Chunk Emission

Based on the diff and stack state, emit minimal chunks:

| Change Type | Stack Action | Emitted Chunk |
|-------------|--------------|---------------|
| String extended | None | Just the extension text |
| New property (same object) | None | `,"propName":` + value start |
| New property (different object) | Pop & close until common ancestor | Closing tokens + new structure |
| Array element added | Pop & close current element | `},` + new element start |
| Value completed | Mark as closed | Closing quote/bracket (deferred) |

## State Tracking

### The ChunkState Class

```csharp
class ChunkState
{
    // The emission stack - what structures are "open"
    Stack<StackFrame> OpenStructures;
    
    // Previous chunk's flattened state for diffing
    Dictionary<string, JsonValue> PreviousState;
    
    // What we've emitted so far (for validation)
    StringBuilder EmittedSoFar;
    
    // Paths we know are "complete" (stopped changing)
    HashSet<string> CompletedPaths;
}

class StackFrame
{
    FrameType Type;        // Object, Array, Property
    string Path;           // JSON path
    HashSet<string> EmittedKeys;  // For objects: keys we've written
    int EmittedCount;      // For arrays: elements we've written
    bool NeedsComma;       // Whether next item needs preceding comma
}
```

### Completion Detection

A value is "complete" when:
1. **Next chunk changes a different path** → Previous path is done
2. **Primitive value** → Numbers, booleans, null are always complete immediately
3. **Structure closed** → All children are complete

```
Chunk N:   {"title":"Mount Fuji","days":[]}
Chunk N+1: {"title":"Mount Fuji","days":[{"name":""}]}

Analysis:
- "title" unchanged between N and N+1 → "title" is COMPLETE
- "days" changed (new element) → "days" still OPEN
- "days[0].name" is new → Start emitting it
```

## Handling Property Reordering

The AI model may return:
```
Chunk 1: {"b":"hello","a":1}
Chunk 2: {"a":1,"b":"hello world"}
```

Despite the reordering, our flattened diff approach handles this:

```
Flattened 1: { "a"→1, "b"→"hello" }
Flattened 2: { "a"→1, "b"→"hello world" }

Diff: Only "b" changed (extended)
Emit: " world"
```

The stack tracks what we've **actually emitted**, not what order the source had:
```
Our emission order: {"a":1,"b":"hello world"}
                    ↑ We control this order
```

## Edge Cases

### 1. Empty Values That Get Filled

```
Chunk 1: {"title":""}
Chunk 2: {"title":"Mount"}

This is an extension from "" to "Mount"
Emit: Mount  (no opening quote - it was already emitted with the empty string)
```

### 2. Nested Object Appears

```
Chunk 1: {"days":[]}
Chunk 2: {"days":[{}]}
Chunk 3: {"days":[{"name":""}]}

Emissions:
  Chunk 1: {"days":[
  Chunk 2: {
  Chunk 3: "name":"
```

### 3. Multiple Properties Complete Simultaneously

```
Chunk N:   {"a":"hello","b":"wor"}
Chunk N+1: {"a":"hello","b":"world","c":123}

Analysis:
- "a" unchanged → was already complete
- "b" extended → emit "ld"  
- "c" is new AND complete (number) → emit ","c":123

But wait - we need to close "b" first!
Detection: "c" appearing means "b" stopped growing
Emit: ld","c":123
```

## The 1-Chunk Delay Strategy

To detect completion, we delay closing tokens by one chunk:

```
Processing Chunk N:
1. Compare Chunk N-1 to Chunk N
2. Any paths in N-1 that are UNCHANGED in N → those are complete
3. Close completed structures from N-1
4. Then emit new/extended content from N

This gives us certainty about what's complete without excessive delay.
```

## Output Validation

After processing all chunks, validate:

```csharp
string emitted = chunker.GetEmittedString();
var parsed = JsonSerializer.Deserialize<JsonElement>(emitted);
var final = JsonSerializer.Deserialize<JsonElement>(lastChunk);

// Deep structural equality (ignoring property order)
Assert.IsTrue(JsonDeepEquals(parsed, final));
```

## Implementation Components

### 1. JsonFlattener
Converts JsonElement to Dictionary<string, JsonValue>

### 2. JsonDiffer  
Compares two flattened dictionaries, produces Diff object

### 3. EmissionStack
Manages open structures, handles closing/opening

### 4. ChunkEmitter
Takes Diff + Stack state, produces string chunks

### 5. JsonStreamChunker (Orchestrator)
Coordinates all components, manages the 1-chunk delay

## Example Walkthrough

Input JSONL:
```
{"title":""}
{"title":"Mount"}
{"title":"Mount Fuji"}
{"title":"Mount Fuji","days":[]}
{"title":"Mount Fuji","days":[{}]}
{"title":"Mount Fuji","days":[{"name":"Day 1"}]}
```

Processing:

| Chunk | Previous State | Diff | Stack Before | Emission | Stack After |
|-------|----------------|------|--------------|----------|-------------|
| 1 | (none) | New: title="" | [] | `{"title":"` | [Root, title] |
| 2 | title="" | Extend: title→"Mount" | [Root, title] | `Mount` | [Root, title] |
| 3 | title="Mount" | Extend: title→"Mount Fuji" | [Root, title] | ` Fuji` | [Root, title] |
| 4 | title="Mount Fuji" | New: days=[] | [Root, title] | `","days":[` | [Root, days] |
| 5 | +days=[] | New: days[0]={} | [Root, days] | `{` | [Root, days, [0]] |
| 6 | +days[0]={} | New: days[0].name="Day 1" | [Root, days, [0]] | `"name":"Day 1"}]}`| [] |

Final output: `{"title":"Mount Fuji","days":[{"name":"Day 1"}]}`

## Summary

The JSON Stream Chunker uses:

1. **Flattened path comparison** - Immune to property reordering
2. **Stack-based structure tracking** - Knows what's "open" and needs closing
3. **1-chunk delay for completion detection** - Certainty without excessive buffering
4. **Incremental string emission** - Only emit what changed
5. **Depth-aware closing** - Pop and close structures when moving to different paths

This architecture handles all the complexities of the AI model's behavior while producing correct, streamable JSON chunks.
