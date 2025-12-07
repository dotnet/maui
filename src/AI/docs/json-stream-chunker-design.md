# JSON Stream Chunker - Complete Design Document

## Problem Statement

We have an AI model that receives progressive JSON internally but outputs complete, valid JSON objects each time. The property order may vary between chunks. We need to convert these complete JSON objects back into streaming chunks that, when concatenated, produce valid JSON matching the final output.

**Input**: JSONL file where each line is a complete JSON object representing progressive construction  
**Output**: Chunks that when concatenated produce valid JSON structurally equivalent to the final line

### Real-World Example (from serengeti-itinerary-1.jsonl)

```
Line 1:  {"days": [{"subtitle": "Day"}]}
Line 2:  {"days": [{"subtitle": "Day 1: Arrival and Wildlife Safari", "activities": []}]}
Line 3:  {"days": [{"subtitle": "Day 1: Arrival and Wildlife Safari", "activities": [{"title": "", "type": "Sightseeing"}]}]}
Line 4:  {"days": [{"activities": [{"type": "Sightseeing", "description": "Embark", "title": "Morning Game Drive"}], "subtitle": "Day 1: Arrival and Wildlife Safari"}]}
...
```

**Observations from real data:**
1. Property order changes between chunks (subtitle moves around)
2. Strings grow progressively ("Day" → "Day 1: Arrival and Wildlife Safari")
3. Empty strings `""` appear and then grow ("title": "" → "title": "Morning Game Drive")
4. Arrays grow (activities: [] → activities: [{...}])
5. New properties appear (description appears in line 4)
6. Only ONE string changes per chunk (confirmed by data analysis)

## Key Constraints

1. **Must stream output** - Cannot wait until the end to emit
2. **1-2 chunk delay OK** - For disambiguation when multiple growable items appear
3. **Property order varies** - Must track by path, not position
4. **Objects only grow** - Properties never removed, values only get longer/deeper
5. **Only one value changes per chunk** - Confirmed by analysis of all test data

## API Design

### Class Signature

```csharp
namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Converts complete JSON objects (from progressive AI output) back into streaming chunks.
/// </summary>
public class JsonStreamChunker
{
    /// <summary>
    /// Process one complete JSON object and return a streaming chunk.
    /// May return empty string (data pending) or a chunk to emit.
    /// </summary>
    /// <param name="completeJson">A complete, valid JSON object representing the current state.</param>
    /// <returns>A chunk to emit, or empty string if data is pending.</returns>
    public string Process(string completeJson);
    
    /// <summary>
    /// Finalize processing and return any remaining output.
    /// Call this after all input has been processed.
    /// </summary>
    /// <returns>Final chunk including closing brackets and any pending strings.</returns>
    public string Flush();
}
```

### Usage Pattern

```csharp
// Consuming an async stream from an AI model
public async IAsyncEnumerable<string> ConvertToChunks(IAsyncEnumerable<string> completeJsonStream)
{
    var chunker = new JsonStreamChunker();
    
    await foreach (var completeJson in completeJsonStream)
    {
        var chunk = chunker.Process(completeJson);
        if (!string.IsNullOrEmpty(chunk))
        {
            yield return chunk;
        }
    }
    
    var finalChunk = chunker.Flush();
    if (!string.IsNullOrEmpty(finalChunk))
    {
        yield return finalChunk;
    }
}

// Result: Concatenating all yielded chunks produces valid JSON
// equivalent to the final input object
```

### Behavior Contract

1. **Process()** returns a chunk that should be emitted immediately
2. **Process()** may return empty string if data is ambiguous (pending resolution)
3. **Flush()** must be called after all input to close any open structures
4. Concatenating all non-empty chunks from all Process() + Flush() calls produces valid JSON
5. The output JSON is structurally equivalent to the final input JSON (property order may differ)

---

## Data Analysis Findings

Analysis of all 4 test JSONL files revealed:

| Pattern | Frequency | Notes |
|---------|-----------|-------|
| String grows | ~40-50 per file | Most common change type |
| New string appears | ~39 per file | Often 1 at a time |
| 2 new growable items at once | 0-4 per file | Requires pending list |
| Multiple values change | 0 | Never happens - confirms assumption |
| Empty array `[]` | Occasional | Gets populated in later chunk |
| Empty object `{}` | Occasional | Gets populated in later chunk |
| Non-string primitives | 0 in test data | All values are strings in these examples |

**Key insight**: The "only one value changes per chunk" assumption holds in all test data (where values include strings, arrays, and objects).

---

## Core Rules

### Rule 1: Sibling Rule
If a new property appears at the same level as the open string, the open string is complete.
*The AI moved on horizontally.*

**Example:**
```
Previous: {"name": "Mat"}
Current:  {"name": "Matthew", "age": 30}

"age" is a NEW sibling at same level as "name"
→ "name" is COMPLETE (emit extension "thew", then close)
→ Then emit the new sibling
```

### Rule 2: Parent-Level Rule  
If new content appears at a higher level (e.g., new array item in parent), the current container and its open string are complete.
*The AI moved on vertically.*

**Example:**
```
Previous: {"days": [{"title": "Day 1"}]}
Current:  {"days": [{"title": "Day 1"}, {"title": "Day 2"}]}

days[1] appeared at parent level (days array)
→ days[0] and everything inside it is COMPLETE
→ Close days[0], then emit new array item
```

### Rule 3: Unchanged Rule
If the open string's value is unchanged from the previous chunk, it is complete.

**Example:**
```
Previous: {"name": "Matthew"}
Current:  {"name": "Matthew", "age": 30}

"name" value unchanged
→ "name" is COMPLETE
(Note: Sibling rule would also apply here)
```

### Rule 4: Pending Rule
If 2+ new **growable** items appear at the same parent level in the same chunk, add them to pending and wait for the next chunk to see which one changes.

**Growable types:**
- Strings: Can grow (characters appended)
- Arrays: Can grow (items added)
- Objects: Can grow (properties added)

Numbers, bools, and null are NOT growable - they are always complete.

**Example 1 - Multiple strings:**
```
Previous: {"count": 5}
Current:  {"count": 5, "a": "Hello", "b": "World"}

2 new strings appeared at root level (siblings) - which one will grow?
→ Add BOTH to pending
→ Emit nothing for strings yet
→ Wait for next chunk to see which changes
```

**Example 2 - String and array at same level:**
```
Previous: {"days": [{}]}
Current:  {"days": [{"subtitle": "", "activities": []}]}

1 new string (subtitle) + 1 new array (activities) at days[0] level
Total: 2 growable items at same level - which one will grow?
→ Add subtitle (string) to _pendingStrings
→ Add activities (array) to _pendingContainers
→ Emit NOTHING for either yet
→ Wait for next chunk to see which changes:
  - If subtitle value changes → subtitle is active, activities was complete
  - If activities gets children → activities is active, subtitle was complete
  - If both unchanged → both were complete
```

---

## State Variables

```
_prevState: Dictionary<string, JsonValue>?
  - Flattened path→value dictionary from last chunk (null on first call)
  - Used to detect what changed
  - JsonValue is a record struct: (JsonValueKind Kind, string? StringValue, string? RawValue)
  - Stores the Kind, StringValue (for strings), and RawValue (raw JSON for non-strings)
  - IMPORTANT: Empty containers are stored as entries with JsonValueKind.Array or JsonValueKind.Object
    (so we know they exist even with no children)

_openStringPath: string?
  - Path of the currently open string (no closing quote emitted)
  - At most ONE string can be open at a time
  - null if no string is currently open

_pendingStrings: Dictionary<string, string>
  - Map of path → value for strings we haven't emitted yet
  - Populated when:
    - 2+ new growable items (strings, arrays, objects) appear at the SAME parent level
    - OR we already have an open value and encounter a new growable item
  - Resolved at start of next chunk by comparing values
  - Note: pending items may or may not be siblings (different nesting levels also go to pending)

_pendingContainers: Dictionary<string, bool>
  - Map of path → isArray for containers we haven't emitted yet
  - Populated when 2+ new growable items appear at the same parent level
  - Resolved at start of next chunk by checking if container grew (got children)
  - Detection: count paths starting with container path in prev vs curr

_emittedStrings: Dictionary<string, string>
  - Map of path → emitted value for strings we HAVE emitted
  - Used to calculate extension: extension = current[emitted.Length..]

_openStructures: Stack<(string path, bool isArray)>
  - Stack of currently open containers
  - Used to properly close structures when moving to different parts of tree
  - IMPORTANT: When emitting at a different level, close structures down to target path

_emittedPaths: HashSet<string>
  - Tracks which paths have been emitted
  - Used to know when to emit commas (if sibling already emitted, prepend comma)
  - IMPORTANT: Do NOT skip processing of existing array items just because path is in _emittedPaths
```

### Comma Rules

- **First property in an object**: No leading comma → `"prop":"value`
- **Subsequent properties**: Leading comma → `,"prop":"value`
- **First item in an array**: No leading comma → `{` or `"value`
- **Subsequent items**: Leading comma → `,{` or `,"value`

Check `_emittedPaths` to see if any sibling was already emitted at the same parent level.
If yes, prepend comma. If no, don't.

## Path Notation

We flatten JSON into paths:
- Object properties: `parent.child`
- Array items: `parent[0]`, `parent[1]`
- Nested: `days[0].activities[1].title`

**Example:**
```json
{"days": [{"subtitle": "Day", "activities": [{"title": "Game"}]}]}
```
Flattens to:
```
days[0].subtitle = "Day"
days[0].activities[0].title = "Game"
```

**Parent path calculation:**
- `days[0].subtitle` → parent is `days[0]`
- `days[0].activities[0].title` → parent is `days[0].activities[0]`
- `days[0]` → parent is `days`

---

## Algorithm Overview

```
For each chunk:

1. FIRST CHUNK (special path - no previous state):
   - Parse and flatten JSON
   - Emit root structure opening "{"
   - Process all containers depth-first via EmitStructure()
   - For each container, count its direct GROWABLE children (strings, arrays, objects):
     - If 0 growable values at this level: emit all non-growables (numbers, bools, null), continue to nested containers
     - If 1 growable value at this level:
       - If string: emit property open (no closing quote), set _openStringPath
       - If container (array/object): emit opening bracket, push to _openStructures, recurse into children
     - If 2+ growable values at this level:
       - Add strings to _pendingStrings
       - Add containers to _pendingContainers
       - Emit NOTHING for these until next chunk resolves which is active
   - Result: at most 1 open string, rest in pending or fully emitted

2. SUBSEQUENT CHUNKS (compare with previous state):
   a. Step A - Handle open string (if _openStringPath is set):
      - Check for new siblings at same level
      - Check for new content at parent level
      - If new sibling OR parent-level change:
          → Emit extension (if value changed)
          → Emit closing quote
          → Set _openStringPath = null
      - Else if value changed:
          → Emit extension
          → Keep open (might still grow)
      - Else (value same):
          → Emit closing quote
          → Set _openStringPath = null

   b. Step B - Resolve pending (if _pendingStrings or _pendingContainers not empty):
      - For strings: Categorize as COMPLETE (unchanged) vs CHANGED
      - For containers: Categorize as COMPLETE (no new children) vs CHANGED (got children)
        - Detection: count paths with prefix in prev vs curr state
      - Emit all COMPLETE items first (sorted by path for consistency):
        - Strings: emit with closing quote
        - Containers: emit opening AND closing brackets AND all content (they're complete)
      - Emit the CHANGED one as open (if any):
        - String: becomes _openStringPath
        - Container: emit opening bracket, push to _openStructures, recurse into children
      - After setting active string, check for new siblings → if found, close immediately

   c. Step C - Process new content:
      - For objects: iterate properties, categorize as existing vs new
        - Existing non-strings: recurse into them
        - New properties: categorize by growable type
      - For arrays: iterate items, compare index to previous count
        - Existing items: recurse into non-strings
        - New items: emit via EmitNewArrayItem
      - For new growable items:
        - Count growables at same parent level
        - If 1 growable AND no open value: emit open, set as active
        - If 1 growable AND have open value: add to pending
        - If 2+ growable: add ALL to pending, emit nothing

3. FINALIZE (after last chunk):
   - Emit any remaining pending items (all complete - sorted by path)
   - Close _openStringPath if set (emit closing ")
   - Close all open structures (emit } or ] for each, in reverse order from stack)
```

---

## Detailed Step A: Handle Open String

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         STEP A: HANDLE OPEN STRING                           │
└─────────────────────────────────────────────────────────────────────────────┘

IF _openStringPath is set:
    │
    ├─► Get current value at _openStringPath
    │   Get emitted value from _emittedStrings[_openStringPath]
    │
    ├─► Check for NEW siblings at same level
    │   (properties in current that weren't in previous, at same parent path)
    │
    ├─► Check for NEW content at PARENT level
    │   (e.g., new array item in parent array)
    │
    ├─► IF NEW SIBLING or PARENT-LEVEL CHANGE:
    │   │
    │   │   String is COMPLETE (AI moved on)
    │   │
    │   ├─► IF value changed:
    │   │       extension = current[emitted.Length..]
    │   │       Emit: extension + closing quote "
    │   │
    │   └─► ELSE:
    │           Emit: closing quote "
    │
    │   Close any containers as needed:
    │     Pop from _openStructures until we reach the level where new content will be emitted
    │     Emit } or ] for each popped container
    │   Set _openStringPath = null
    │
    └─► ELSE (no new siblings, no parent changes):
        │
        ├─► IF value CHANGED:
        │       extension = current[emitted.Length..]
        │       Emit: extension (no closing quote)
        │       Update _emittedStrings[path] = current
        │       Keep _openStringPath set (might still grow)
        │
        └─► ELSE (value SAME):
                Emit: closing quote "
                Set _openStringPath = null
```

**Real example - Sibling Rule (Line 2):**
```
Previous: {"days": [{"subtitle": "Day"}]}
Current:  {"days": [{"subtitle": "Day 1: Arrival and Wildlife Safari", "activities": []}]}

_openStringPath = "days[0].subtitle"
emitted = "Day"
current = "Day 1: Arrival and Wildlife Safari"

Check siblings at days[0]:
  Previous had: subtitle
  Current has: subtitle, activities
  → "activities" is NEW SIBLING!

Action:
  extension = "Day 1: Arrival and Wildlife Safari"["Day".Length..] = " 1: Arrival and Wildlife Safari"
  Emit: 1: Arrival and Wildlife Safari"
  Set _openStringPath = null
  Then emit new sibling: ,"activities":[

Output: 1: Arrival and Wildlife Safari","activities":[
```

**Real example - Parent-Level Rule (Line 6):**
```
Previous: {"days": [{"activities": [{"description": "Embark on a thrilling..."}]}]}
Current:  {"days": [{"activities": [{"description": "...full text..."}, {"type": ""}]}]}

_openStringPath = "days[0].activities[0].description"
Parent of description = days[0].activities[0]
Parent's parent = days[0].activities (array)

Check: days[0].activities[1] is NEW
→ New content at parent level!

Action:
  Emit extension for description
  Close description: "
  Close activities[0] object: }
  Emit new array item: ,{
  Handle strings in new item...
```

---

## Detailed Step B: Resolve Pending

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         STEP B: RESOLVE PENDING                              │
└─────────────────────────────────────────────────────────────────────────────┘

IF _pendingStrings OR _pendingContainers is not empty:
    │
    ├─► RESOLVE PENDING STRINGS:
    │   │
    │   ├─► Categorize all pending strings:
    │   │       For each (path, storedValue) in _pendingStrings:
    │   │           currentValue = current[path]
    │   │           IF currentValue == storedValue:
    │   │               Add to COMPLETE_STRINGS list
    │   │           ELSE:
    │   │               Add to CHANGED_STRINGS list (should be exactly 0 or 1)
    │   │
    │   ├─► FIRST: Emit all COMPLETE strings (with closing quotes):
    │   │       For each in COMPLETE_STRINGS:
    │   │           needsComma = any sibling already in _emittedPaths
    │   │           Emit: [,]"path":"value"
    │   │           Add path to _emittedPaths
    │   │
    │   └─► Clear _pendingStrings
    │
    ├─► RESOLVE PENDING CONTAINERS:
    │   │
    │   ├─► Categorize all pending containers:
    │   │       For each (path, isArray) in _pendingContainers:
    │   │           previousChildCount = count of previous paths starting with this container
    │   │           currentChildCount = count of current paths starting with this container
    │   │           IF currentChildCount == previousChildCount:
    │   │               Add to COMPLETE_CONTAINERS list (container didn't grow)
    │   │           ELSE:
    │   │               Add to CHANGED_CONTAINERS list (container got children)
    │   │
    │   ├─► Emit all COMPLETE containers (with opening AND closing brackets):
    │   │       For each in COMPLETE_CONTAINERS:
    │   │           needsComma = any sibling already in _emittedPaths
    │   │           IF isArray: Emit: [,]"path":[]
    │   │           ELSE: Emit: [,]"path":{}
    │   │           Add path to _emittedPaths
    │   │
    │   └─► Clear _pendingContainers
    │
    ├─► EMIT THE CHANGED ITEM (at most 1 across strings and containers):
    │       IF CHANGED_STRINGS has exactly 1:
    │           needsComma = any sibling already in _emittedPaths
    │           Emit: [,]"path":"value (no closing quote)
    │           Set _openStringPath = path
    │           Add path to _emittedPaths
    │           Update _emittedStrings[path] = currentValue
    │       
    │       IF CHANGED_CONTAINERS has exactly 1:
    │           needsComma = any sibling already in _emittedPaths
    │           IF isArray: Emit: [,]"path":[
    │           ELSE: Emit: [,]"path":{
    │           Push to _openStructures
    │           Add path to _emittedPaths
    │           → Recursively process children of this container
    │       
    │       IF total CHANGED has 2+:
    │           This should never happen per "only one value changes per chunk" invariant
    │           Log warning and treat all as complete
    │
    └─► IF _openStringPath was just set:
            Check for new siblings at same level (in current, not in previous)
            IF new sibling exists:
                → Close immediately (Sibling Rule)
                Emit: "
                Set _openStringPath = null
```

**Real example (Line 4):**
```
_pendingStrings = {
  "days[0].activities[0].title": "",
  "days[0].activities[0].type": "Sightseeing"
}

Current values:
  title = "Morning Game Drive"  (was "")
  type = "Sightseeing"          (unchanged)

Categorize:
  COMPLETE = [type]
  CHANGED = [title]

Emit COMPLETE first:
  type: no siblings emitted yet → no comma
    Emit: "type":"Sightseeing"
    Add to _emittedPaths

Emit CHANGED:
  title: type already emitted (sibling) → needs comma
    Emit: ,"title":"Morning Game Drive
    Set _openStringPath = "days[0].activities[0].title"
    Add to _emittedPaths

Check for new siblings of title:
  "description" is NEW at same level!
  → Sibling Rule: close title immediately
    Emit: "
  → Then handle description in Step C

Output: "type":"Sightseeing","title":"Morning Game Drive"
```

---

## Detailed Step C: Process New Content

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         STEP C: PROCESS NEW CONTENT                          │
└─────────────────────────────────────────────────────────────────────────────┘

For each new path in current that wasn't in previous:
    │
    ├─► IF value is NON-GROWABLE (number, bool, null):
    │       needsComma = any sibling already in _emittedPaths
    │       Emit complete: [,]"path":value
    │       Add path to _emittedPaths
    │
    └─► IF value is GROWABLE (string, object, array):
            Group all new GROWABLE items BY PARENT (siblings only):
            │
            ├─► For each parent with new growable items:
            │       Count new growable items at this parent
            │       │
            │       ├─► IF 1 new growable item at this parent:
            │       │       IF no open value (_openStringPath is null AND no pending):
            │       │           │
            │       │           ├─► IF string:
            │       │           │       needsComma = any sibling in _emittedPaths
            │       │           │       Emit open: [,]"path":"value  (no closing quote)
            │       │           │       Set _openStringPath = path
            │       │           │       Add to _emittedPaths
            │       │           │       Update _emittedStrings[path] = value
            │       │           │
            │       │           └─► IF object or array:
            │       │                   needsComma = any sibling in _emittedPaths
            │       │                   Emit opening: [,]"path":{ or [,]"path":[
            │       │                   Push to _openStructures
            │       │                   Add path to _emittedPaths
            │       │                   Recursively process children
            │       │       
            │       │       ELSE (already have open value):
            │       │           IF string: Add to _pendingStrings (wait for next chunk)
            │       │           IF container: Add to _pendingContainers (wait for next chunk)
            │       │
            │       └─► IF 2+ new growable items at this parent:
            │               For each growable item:
            │                   IF string: Add to _pendingStrings
            │                   IF container: Add to _pendingContainers
            │               Do NOT emit values for these
            │               (Will resolve in next chunk to see which one changes)
```

**Note**: We count new growable items per parent level, not globally. If we get:
```json
{"a": {"x": "hello"}, "b": "world"}
```
- `a.x` has 1 new growable item at parent `a`
- `b` has 1 new growable item at parent root
- These are NOT siblings, so each is handled separately
- First one encountered becomes the open value
- Second goes to pending (can only have one open value at a time)

**Example - 1 new string (Line 7):**
```
Previous: days[0].activities[1] = {type: ""}
Current:  days[0].activities[1] = {type: "FoodAndDining", title: "Lunch"}

After Step A closes type (sibling rule):
  _openStringPath = null
  _emittedPaths contains "days[0].activities[1].type"

Step C - New content:
  "days[0].activities[1].title" is new
  Parent = "days[0].activities[1]"
  1 new string at this parent
  "type" already in _emittedPaths (sibling) → needs comma

Action:
  Emit: ,"title":"Lunch
  Set _openStringPath = "days[0].activities[1].title"
  Add to _emittedPaths

Output: ,"title":"Lunch
```

**Example - 2+ new strings (Line 3):**
```
Previous: days[0].activities = []
Current:  days[0].activities = [{title: "", type: "Sightseeing"}]

Step C - New content:
  days[0].activities[0] is new object
  Parent of this object = days[0].activities
  No siblings emitted → no comma for object

  Emit: {
  Push to _openStructures
  Add "days[0].activities[0]" to _emittedPaths

  Inside it: title and type (2 strings!)
  Parent = days[0].activities[0]
  2 new strings at this parent → pending

  Add both to pending:
    _pendingStrings = {
      "days[0].activities[0].title": "",
      "days[0].activities[0].type": "Sightseeing"
    }
  Do NOT emit string values

Output: {
```

---

## Complete Real-World Walkthrough

### Line 1
```json
{"days": [{"subtitle": "Day"}]}
```

**First chunk processing:**
- Parse and flatten:
  - Root object
  - `days` = array
  - `days[0]` = object
  - `days[0].subtitle` = "Day"
- Process containers:
  - Root: emit `{`, push to _openStructures
  - `days`: emit `"days":[`, push to _openStructures
  - `days[0]`: emit `{`, push to _openStructures
- Process strings at `days[0]`:
  - 1 string (`subtitle`), no open string yet
  - Emit open: `"subtitle":"Day` (no closing quote)
  - Set _openStringPath = "days[0].subtitle"

```
Output: {"days":[{"subtitle":"Day
                              ↑ no closing quote
State:
  _openStringPath = "days[0].subtitle"
  _emittedStrings = {"days[0].subtitle": "Day"}
  _emittedPaths = {"days", "days[0]", "days[0].subtitle"}
  _openStructures = [root, days, days[0]]
```

### Line 2
```json
{"days": [{"subtitle": "Day 1: Arrival and Wildlife Safari", "activities": []}]}
```

**Step A - Handle open string:**
- _openStringPath = "days[0].subtitle"
- emitted = "Day"
- current = "Day 1: Arrival and Wildlife Safari"
- Check siblings at days[0]: `activities` is NEW!
- Sibling Rule → subtitle is COMPLETE

```
extension = " 1: Arrival and Wildlife Safari"
Emit: 1: Arrival and Wildlife Safari"
_openStringPath = null
```

**Step C - New content:**
- `days[0].activities` is new (empty array)
- Emit: ,"activities":[
- Push to _openStructures

```
Output for Line 2: 1: Arrival and Wildlife Safari","activities":[

State:
  _openStringPath = null
  _openStructures = [root, days, days[0], days[0].activities]
```

### Line 3
```json
{"days": [{"subtitle": "Day 1: Arrival and Wildlife Safari", "activities": [{"title": "", "type": "Sightseeing"}]}]}
```

**Step A - No open string** (was closed in Line 2)

**Step C - New content:**
- `days[0].activities[0]` is new (object)
- No siblings in activities array yet → no comma
- Emit: `{`
- Push to _openStructures
- Add to _emittedPaths
- Inside: title="" and type="Sightseeing" (2 strings at same parent!)
- Pending Rule → add both to pending, don't emit string values

```
Emit: {
_pendingStrings = {
  "days[0].activities[0].title": "",
  "days[0].activities[0].type": "Sightseeing"
}

Output for Line 3: {

State:
  _openStructures = [..., days[0].activities[0]]
  _emittedPaths += {"days[0].activities[0]"}
  _pendingStrings = 2 entries
```

### Line 4
```json
{"days": [{"activities": [{"type": "Sightseeing", "description": "Embark", "title": "Morning Game Drive"}], "subtitle": "Day 1: Arrival and Wildlife Safari"}]}
```

**Step A - No open string**

**Step B - Resolve pending:**
- type: "Sightseeing" → "Sightseeing" = UNCHANGED → complete
- title: "" → "Morning Game Drive" = CHANGED → active

```
Emit for type: "type":"Sightseeing"
Emit for title: ,"title":"Morning Game Drive
_openStringPath = "days[0].activities[0].title"
```

Check for siblings of title:
- `description` is NEW at same level!
- Sibling Rule → title is COMPLETE

```
Emit: "
_openStringPath = null
```

**Step C - New content:**
- `description` = "Embark" is new
- 1 new string

```
Emit: ,"description":"Embark
_openStringPath = "days[0].activities[0].description"

Output for Line 4: "type":"Sightseeing","title":"Morning Game Drive","description":"Embark

State:
  _openStringPath = "days[0].activities[0].description"
  _emittedStrings[...description] = "Embark"
```

### Line 5
```json
{"days": [{"activities": [{"description": "Embark on a thrilling morning game drive to witness the Great Migration in all its glory.", "title": "Morning Game Drive", "type": "Sightseeing"}], "subtitle": "Day 1: Arrival and Wildlife Safari"}]}
```

**Step A - Handle open string:**
- _openStringPath = "days[0].activities[0].description"
- emitted = "Embark"
- current = "Embark on a thrilling..."
- Check siblings: none new
- Check parent level: nothing new
- Value CHANGED → emit extension, keep open

```
extension = " on a thrilling..."
Emit:  on a thrilling...
_openStringPath still set

Output for Line 5:  on a thrilling...
```

### Line 6
```json
{"days": [{"subtitle": "Day 1: Arrival and Wildlife Safari", "activities": [{"description": "Embark on a thrilling morning game drive to witness the Great Migration in all its glory.", "type": "Sightseeing", "title": "Morning Game Drive"}, {"type": ""}]}]}
```

**Step A - Handle open string:**
- _openStringPath = "days[0].activities[0].description"
- emitted = "Embark on a thrilling..."
- current = "...full text..."
- Check siblings at activities[0]: none new
- Check parent level: `days[0].activities[1]` is NEW!
- Parent-Level Rule → activities[0] is COMPLETE

```
extension = " morning game drive to witness the Great Migration in all its glory."
Emit extension: ...
Close description: "
Close activities[0] object: }
_openStringPath = null
```

**Step C - New content:**
- `days[0].activities[1]` is new object
- Inside: type="" (1 string)

```
Emit: ,{
Emit: "type":"
_openStringPath = "days[0].activities[1].type"
_emittedStrings[...type] = ""

Output for Line 6:  morning game drive to witness the Great Migration in all its glory."},{"type":"
```

### Line 7
```json
{"days": [{"subtitle": "Day 1: Arrival and Wildlife Safari", "activities": [{"title": "Morning Game Drive", "description": "...", "type": "Sightseeing"}, {"type": "FoodAndDining", "title": "Lunch"}]}]}
```

**Step A - Handle open string:**
- _openStringPath = "days[0].activities[1].type"
- emitted = ""
- current = "FoodAndDining"
- Check siblings: `title` is NEW!
- Sibling Rule → type is COMPLETE

```
extension = "FoodAndDining"
Emit: FoodAndDining"
_openStringPath = null
```

**Step C - New content:**
- `title` = "Lunch" is new
- 1 new string

```
Emit: ,"title":"Lunch
_openStringPath = "days[0].activities[1].title"

Output for Line 7: FoodAndDining","title":"Lunch
```

### Line 8
```json
{"days": [{"activities": [{"type": "Sightseeing", "description": "...", "title": "Morning Game Drive"}, {"description": "Enjoy", "title": "Lunch at Restaurant 1", "type": "FoodAndDining"}], "subtitle": "Day 1: Arrival and Wildlife Safari"}]}
```

**Step A - Handle open string:**
- _openStringPath = "days[0].activities[1].title"
- emitted = "Lunch"
- current = "Lunch at Restaurant 1"
- Check siblings: `description` is NEW!
- Sibling Rule → title is COMPLETE

```
extension = " at Restaurant 1"
Emit:  at Restaurant 1"
_openStringPath = null
```

**Step C - New content:**
- `description` = "Enjoy" is new
- 1 new string

```
Emit: ,"description":"Enjoy
_openStringPath = "days[0].activities[1].description"

Output for Line 8:  at Restaurant 1","description":"Enjoy
```

---

## Edge Cases

### Empty string grows
```
Line 3: "title": ""
Line 4: "title": "Morning Game Drive"
```
- Empty string is just a string with length 0
- Extension = "Morning Game Drive"[0..] = "Morning Game Drive"
- No special handling needed - treat `""` like any other string

### Empty containers get populated
```
Line 2: "activities": []
Line 3: "activities": [{"title": ""}]
```
- Empty array `[]` → emit `[` only, push to _openStructures
- Do NOT emit closing `]`
- When items appear → emit them normally
- Closing brackets emitted when moving to sibling/parent or at finalize

**Note on flattening**: Empty containers ARE stored in the flattened state dictionary
with their JsonValueKind (Array or Object). This allows us to:
1. Know the container exists even with no children
2. Detect when children are added (container grew)

### Nested container changes propagate up

Containers (arrays and objects) can have nested children that grow. When determining if a container is "changing", we must check ALL descendants, not just direct children.

**The Rule**: A container is "still active/growing" if ANY descendant path changes.

**Example - Nested array with growing string:**
```
Previous: {"items": [{"name": "Jo"}]}
Current:  {"items": [{"name": "John"}]}
```
- `items` is an array containing an object
- The object contains a string `name` that grew ("Jo" → "John")
- Even though `items[0]` exists in both, the string inside changed
- Therefore `items` (the array) is still "active" - don't close it

**Example - Deep nesting:**
```
Previous: {"root": {"level1": {"level2": {"value": "He"}}}}
Current:  {"root": {"level1": {"level2": {"value": "Hello"}}}}
```
- `value` is the actual string that changed
- But `level2`, `level1`, and `root` are ALL still active because a descendant changed
- None of these containers should be closed

**Detection Algorithm:**
When checking if a container at path P is "complete" vs "still active":
1. Find all current paths that START WITH P (all descendants)
2. Find all previous paths that START WITH P
3. If ANY descendant value changed → container is still active
4. Only if ALL descendants are unchanged → container is complete

**Why this matters for pending:**
When we have pending containers (e.g., a string and an array both appeared):
```
Previous: {"days": [{}]}
Current:  {"days": [{"subtitle": "", "activities": []}]}
```
- `subtitle` (string) and `activities` (array) are both pending
- Next chunk:
  ```
  Current:  {"days": [{"subtitle": "Day 1", "activities": []}]}
  ```
- `subtitle` changed ("" → "Day 1") → subtitle is the active one
- `activities` has no new children → activities is complete
- BUT if instead:
  ```
  Current:  {"days": [{"subtitle": "", "activities": [{"type": ""}]}]}
  ```
- `subtitle` unchanged ("" → "")
- `activities` now has children → activities is the active one
- The array "grew" even though `activities` itself didn't change - its DESCENDANTS did

### Array grows by new item
```
Line 5: activities has 1 item
Line 6: activities has 2 items
```
- Detect new array index (activities[1])
- This triggers Parent-Level Rule for activities[0]
- Close activities[0], emit `,{`, process new item

### Deep nesting
- Paths handle arbitrary depth: `days[0].activities[2].details.notes[0]`
- Stack of _openStructures handles closing in correct order
- Parent detection walks up the path segments

### New strings at different nesting levels
```
Previous: {"count": 5}
Current:  {"count": 5, "a": {"x": "hello"}, "b": "world"}
```
- `a.x` is at parent `a` (1 string at this level)
- `b` is at parent root (1 string at this level)
- These are NOT siblings - different parents
- We can only have ONE open string at a time
- First encountered (e.g., `a.x`) becomes _openStringPath
- Second (`b`) goes to pending even though it's alone at its level
- Next chunk resolves: whichever changed is active (per invariant, at most one changes)

### All pending strings complete (none changed)
```
Line N: Two strings added to pending
Line N+1: Both strings have same values (unchanged)
```
- Both are COMPLETE
- Emit both with closing quotes
- No _openStringPath is set
- Continue to Step C for new content

---

## Key Decision Points

### When to close the open string?

1. **Sibling Rule**: New property at same level
2. **Parent Rule**: New content at higher level
3. **Unchanged Rule**: Value same as previous chunk
4. **Finalize**: End of stream

### When to use pending?

- First chunk has 2+ strings
- 2+ new strings appear in same chunk
- NEVER emit pending strings immediately - always wait for next chunk

### How to calculate "same level"?

Two paths are at the same level (siblings) if they have the same parent:
- `days[0].title` and `days[0].subtitle` → same parent `days[0]` ✓
- `days[0].title` and `days[1].title` → different parents ✗
- `days[0].activities[0].title` and `days[0].activities[0].type` → same parent ✓

### How to detect parent-level changes?

For open string at path P, check if any new content appeared at:
- P's parent path
- P's grandparent path
- etc.

Example: If open string is at `days[0].activities[0].description`:
- Parent = `days[0].activities[0]`
- Grandparent = `days[0].activities`
- If `days[0].activities[1]` appears → parent-level change!

### How to handle empty containers?

- Emit `[` or `{` only (no closing bracket)
- Add to _openStructures
- Close when moving to sibling/parent or at finalize

---

## Key Invariants

1. **At most ONE growable value is open** at any time (never two)
2. **Pending items (strings and containers) are NEVER emitted** until resolved in next chunk
3. **Extension = current[emitted.Length..]** - strings only grow, never shrink
4. **Order: Step A (open) → Step B (pending) → Step C (new)** - always this order
5. **2+ new growable items at same parent = pending** - never guess which is active
6. **New sibling = complete** - AI moved on horizontally
7. **Parent-level change = complete** - AI moved on vertically
8. **Track by path, not position** - property order doesn't matter
9. **Containers stay open** until sibling/parent change or finalize
10. **Empty containers are valid** - `{}` and `[]` can get populated later
11. **Comma before sibling** - check _emittedPaths to know if comma needed
12. **At most ONE value changes per chunk** - if Step B finds 2+ changed (strings or containers), that's an error
13. **Growable types: strings, arrays, objects** - all can grow and need pending logic
14. **Non-growable types: numbers, bools, null** - always complete immediately
15. **Nested changes propagate up** - if ANY descendant of a container changes, the container is still active
16. **Complete containers emit all content** - when a pending container is resolved as complete, emit its full content (opening bracket, all children recursively, closing bracket)
17. **Structure closing is level-aware** - use `CloseStructuresDownTo` to close structures when emitting at a different tree level

---

## Structure Management

### CloseStructuresDownTo Algorithm

When we need to emit content at a different level in the JSON tree, we must first close any open structures that are not ancestors of the target path.

```
CloseStructuresDownTo(targetPath):
    while _openStructures is not empty:
        (topPath, isArray) = peek top of stack
        
        // Check if targetPath is at or inside topPath
        isPrefix = targetPath.startsWith(topPath) AND
                   (lengths equal OR next char is '.' or '[')
        
        if topPath is root ("") OR isPrefix:
            break  // Don't close - we're inside this structure
        
        pop from stack
        emit ']' if isArray else '}'
```

**Example:**
```
_openStructures = [("", false), ("days", true), ("days[0]", false), ("days[0].activities", true)]

We want to emit at "days[1]" (new array item in days)

1. Check "days[0].activities" - "days[1]" doesn't start with this → close with ']'
2. Check "days[0]" - "days[1]" doesn't start with this → close with '}'  
3. Check "days" - "days[1]" DOES start with "days" → stop

Result: emitted "]}" and stack is now [("", false), ("days", true)]
```

### When Structure Closing Happens

1. **Before emitting a new property** - `EmitNewProperty` calls `CloseStructuresDownTo(parentPath)`
2. **Before emitting a pending string** - `EmitPendingString` calls `CloseStructuresDownTo(parentPath)`
3. **Before emitting a new array item** - `EmitNewArrayItem` calls `CloseStructuresDownTo(arrayPath)`

This ensures we're always at the correct nesting level before emitting new content.

---

## First Chunk vs Subsequent Chunk

The implementation handles the first chunk differently from subsequent chunks:

### First Chunk (ProcessFirstChunk)
- No previous state to compare against
- Uses `EmitStructure` to recursively process the JSON tree
- Counts growables at each level to decide open vs pending
- Emits structure (brackets) and handles string ambiguity

### Subsequent Chunks (ProcessSubsequentChunk)
- Has previous state for comparison
- Follows the Step A → Step B → Step C order
- Uses `ProcessNewContent` to find and emit new properties/items
- Can detect siblings and parent-level changes

## Critical Rules Checklist

### NEVER Do These Things

1. **NEVER defer all output to the end** - This defeats the entire purpose of streaming
2. **NEVER accumulate all chunks and process at the end** - Each chunk must produce output
3. **NEVER rely on property order** - Use path-based comparison, not positional
4. **NEVER close a string prematurely** - Only close when CERTAIN it won't grow
5. **NEVER re-serialize to find diff** - Compare objects structurally, emit directly

### ALWAYS Do These Things

1. **ALWAYS use intermediate deserialization** - Parse each chunk to dictionary for comparison
2. **ALWAYS compare by path** - Flatten objects to `path → value` dictionaries
3. **ALWAYS track what's "open"** - Use stack to know what needs closing tokens
4. **ALWAYS emit extension BEFORE closing** - When closing a string that grew, emit extension first
5. **ALWAYS validate concatenation** - Concatenate all chunks, parse as JSON, compare to final

### Order of Operations

When processing a chunk where a new property appears (signaling completion):

1. **FIRST**: Emit any remaining string extension (the part that grew since last chunk)
2. **THEN**: Emit the closing quote
3. **THEN**: Emit new content (comma, new property, etc.)

This prevents truncation of the final string extension.

---

## Why Naive Approaches Fail

### The String Diff Problem

A naive approach of "serialize with open strings, diff, emit diff" fails because:

1. **Line 2**: We serialize with `subtitle` open (no closing quote):
   `...Safari,"activities":[`  (note: no quote after Safari)

2. **Line 3**: We serialize with `type` open (different string):
   `...Safari","activities":[{"title":"","type":"Sightseeing` (quote after Safari)

3. When we diff these, they diverge at position 55 where one has `,` and the other has `"`.
   The diff produces content that DUPLICATES properties!

### The Root Cause

The **closing quote for a string is NOT at the end of our emitted output**. It's embedded in the middle, before subsequent content. When we change which string is "open", the quote position moves, causing serializations to diverge unexpectedly.

### The Solution

Track what we've **ACTUALLY EMITTED** separately from any serialization:

1. Emit content immediately, but track that a string is "open" (withheld quote)
2. When a new property appears (signaling previous string is complete):
   - Emit the closing quote FIRST
   - THEN emit the new content
3. The emitted stream is what we actually output, not a re-serialization

---

## Files

- **Implementation**: `src/AI/src/Essentials.AI/JsonStreamChunker.cs`
- **Tests**: `src/AI/tests/Essentials.AI.UnitTests/JsonStreamChunkerTests.cs`
- **Test data**: `src/AI/tests/Essentials.AI.UnitTests/TestData/ObjectStreams/*.jsonl`
