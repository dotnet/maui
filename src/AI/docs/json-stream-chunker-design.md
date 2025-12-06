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
2. **1-2 chunk delay OK** - For disambiguation when multiple strings appear
3. **Property order varies** - Must track by path, not position
4. **Objects only grow** - Properties never removed, strings only get longer
5. **Only one string changes per chunk** - Confirmed by analysis of all test data

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
    /// Process one complete JSON object and return streaming chunks.
    /// May return 0 chunks (data pending), 1 chunk, or multiple chunks.
    /// </summary>
    /// <param name="completeJson">A complete, valid JSON object representing the current state.</param>
    /// <returns>Zero or more chunks to emit.</returns>
    public IEnumerable<string> Process(string completeJson);
    
    /// <summary>
    /// Finalize processing and return any remaining output.
    /// Call this after all input has been processed.
    /// </summary>
    /// <returns>Final chunks including closing brackets and any pending strings.</returns>
    public IEnumerable<string> Flush();
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
        foreach (var chunk in chunker.Process(completeJson))
        {
            yield return chunk;
        }
    }
    
    foreach (var chunk in chunker.Flush())
    {
        yield return chunk;
    }
}

// Result: Concatenating all yielded chunks produces valid JSON
// equivalent to the final input object
```

### Behavior Contract

1. **Process()** returns chunks that should be emitted immediately
2. **Process()** may return empty enumerable if data is ambiguous (pending resolution)
3. **Flush()** must be called after all input to close any open structures
4. Concatenating all chunks from all Process() + Flush() calls produces valid JSON
5. The output JSON is structurally equivalent to the final input JSON (property order may differ)

---

## Data Analysis Findings

Analysis of all 4 test JSONL files revealed:

| Pattern | Frequency | Notes |
|---------|-----------|-------|
| String grows | ~40-50 per file | Most common change type |
| New string appears | ~39 per file | Often 1 at a time |
| 2 new strings at once | 0-4 per file | Requires pending list |
| Multiple strings change | 0 | Never happens - confirms assumption |
| Empty array `[]` | Occasional | Gets populated in later chunk |
| Empty object `{}` | Occasional | Gets populated in later chunk |
| Non-string primitives | 0 in test data | All values are strings in these examples |

**Key insight**: The "only one string changes per chunk" assumption holds in all test data.

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
If 2+ new strings appear at the same parent level in the same chunk, add ALL to pending and wait for the next chunk to see which one changes.

**Example:**
```
Previous: {"count": 5}
Current:  {"count": 5, "a": "Hello", "b": "World"}

2 new strings appeared at root level (siblings) - which one will grow?
→ Add BOTH to pending
→ Emit nothing for strings yet
→ Wait for next chunk to see which changes
```

---

## State Variables

```
_prevState: Dictionary<string, JsonValue>
  - Flattened path→value dictionary from last chunk
  - Used to detect what changed

_openStringPath: string?
  - Path of the currently open string (no closing quote emitted)
  - At most ONE string can be open at a time
  - null if no string is currently open

_pendingStrings: Dictionary<string, string>
  - Map of path → value for strings we haven't emitted yet
  - Populated when:
    - 2+ new strings appear at the SAME parent level (siblings)
    - OR we already have an open string and encounter a new string at a different level
  - Resolved at start of next chunk by comparing values
  - Note: pending strings may or may not be siblings

_emittedStrings: Dictionary<string, string>
  - Map of path → emitted value for strings we HAVE emitted
  - Used to calculate extension: extension = current[emitted.Length..]

_openStructures: Stack<(string path, bool isArray)>
  - Stack of currently open containers
  - Used to properly close structures when moving to different parts of tree

_emittedPaths: HashSet<string>
  - Tracks which paths have been emitted
  - Used to know when to emit commas (if sibling already emitted, prepend comma)
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

1. FIRST CHUNK:
   - Parse and flatten JSON
   - Process all containers depth-first (emit { or [ for each, no closing brackets)
   - For each container (depth-first order), count its direct string children:
     - If 0 strings at this level: continue
     - If 1 string at this level AND no open string yet: emit open, set _openStringPath
     - If 1 string at this level AND already have open string: add to pending
     - If 2+ strings at this level: add ALL to pending
   - Result: at most 1 open string, rest in pending

2. SUBSEQUENT CHUNKS:
   a. Step A - Handle open string (if _openStringPath is set):
      - Check for new siblings at same level
      - Check for new content at parent level
      - If new sibling OR parent-level change:
          → Emit extension (if value changed)
          → Emit closing quote
          → Close containers as needed
          → Set _openStringPath = null
      - Else if value changed:
          → Emit extension
          → Keep open (might still grow)
      - Else (value same):
          → Emit closing quote
          → Set _openStringPath = null

   b. Step B - Resolve pending (if _pendingStrings not empty):
      - Categorize: COMPLETE (unchanged) vs CHANGED
      - Emit all COMPLETE first (with closing ")
      - Emit the CHANGED one as open (if any) - becomes _openStringPath
      - After setting active, check for new siblings → if found, close immediately

   c. Step C - Process new content:
      - For each new path not in previous state:
        - Non-strings → emit complete
        - Objects/arrays → emit opening bracket, push to stack, process children
        - Strings:
          - Group by parent
          - 1 new string at parent AND no open string: emit open, set _openStringPath
          - 1 new string at parent AND have open string: add to pending
          - 2+ new strings at parent: add ALL to pending

3. FINALIZE (after last chunk):
   - Emit any remaining pending strings (all complete with closing ")
   - Close _openStringPath if set (emit closing ")
   - Close all open structures (emit } or ] for each, in reverse order)
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

IF _pendingStrings is not empty:
    │
    ├─► Categorize all pending strings:
    │       For each (path, storedValue) in _pendingStrings:
    │           currentValue = current[path]
    │           IF currentValue == storedValue:
    │               Add to COMPLETE list
    │           ELSE:
    │               Add to CHANGED list (should be exactly 0 or 1)
    │
    ├─► FIRST: Emit all COMPLETE strings (with closing quotes):
    │       For each in COMPLETE:
    │           needsComma = any sibling already in _emittedPaths
    │           Emit: [,]"path":"value"
    │           Add path to _emittedPaths
    │
    ├─► THEN: Emit the CHANGED string (if any) as open:
    │       IF CHANGED has exactly 1:
    │           needsComma = any sibling already in _emittedPaths
    │           Emit: [,]"path":"value (no closing quote)
    │           Set _openStringPath = path
    │           Add path to _emittedPaths
    │           Update _emittedStrings[path] = currentValue
    │       
    │       IF CHANGED has 0:
    │           All were complete, no open string
    │       
    │       IF CHANGED has 2+:
    │           ERROR: Violates "only one string changes per chunk" assumption
    │
    ├─► Clear _pendingStrings
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
    ├─► IF value is non-string (number, bool, null):
    │       needsComma = any sibling already in _emittedPaths
    │       Emit complete: [,]"path":value
    │       Add path to _emittedPaths
    │
    ├─► IF value is object or array:
    │       needsComma = any sibling already in _emittedPaths
    │       Emit opening: [,]{ or [,][
    │       Push to _openStructures
    │       Add path to _emittedPaths
    │       Recursively process children
    │
    └─► IF value is string:
            Group all new strings BY PARENT (siblings only):
            │
            ├─► For each parent with new strings:
            │       Count new strings at this parent
            │       │
            │       ├─► IF 1 new string at this parent:
            │       │       IF _openStringPath is null:
            │       │           needsComma = any sibling in _emittedPaths
            │       │           Emit open: [,]"path":"value
            │       │           Set _openStringPath = path
            │       │           Add to _emittedPaths
            │       │           Update _emittedStrings[path] = value
            │       │       ELSE:
            │       │           We already have an open string at different level
            │       │           Add to _pendingStrings (wait for next chunk)
            │       │
            │       └─► IF 2+ new strings at this parent:
            │               Add ALL to _pendingStrings
            │               (Will resolve in next chunk)
            │               Do NOT emit string values
            │               Do NOT set _openStringPath
```

**Note**: We count new strings per parent level, not globally. If we get:
```json
{"a": {"x": "hello"}, "b": "world"}
```
- `a.x` has 1 new string at parent `a`
- `b` has 1 new string at parent root
- These are NOT siblings, so each is handled separately
- First one encountered becomes _openStringPath
- Second goes to pending (can only have one open string)

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
- Next chunk resolves: whichever changed is active

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

1. **At most ONE string is open** at any time (never two)
2. **Pending strings are NEVER emitted** until resolved in next chunk
3. **Extension = current[emitted.Length..]** - strings only grow, never shrink
4. **Order: Step A (open) → Step B (pending) → Step C (new)** - always this order
5. **2+ new strings at same parent = pending** - never guess which is active
6. **New sibling = complete** - AI moved on horizontally
7. **Parent-level change = complete** - AI moved on vertically
8. **Track by path, not position** - property order doesn't matter
9. **Containers stay open** until sibling/parent change or finalize
10. **Empty containers are valid** - `{}` and `[]` can get populated later
11. **Comma before sibling** - check _emittedPaths to know if comma needed
12. **At most ONE string changes per chunk** - if Step B finds 2+ changed, that's an error

---

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
