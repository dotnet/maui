# Deliberately accept only the workflow contract's literal `Fixes` keyword. Generated
# [leak-fix] PRs are required to use it, and keeping the destructive proof narrower than
# GitHub's full closing-keyword set is fail-closed.
def leak_without_fenced_markdown:
  reduce ((gsub("\\r\\n"; "\n") | split("\n"))[]) as $line
    ({ fence: null, visible: [] };
     if .fence == null then
       ([$line | try capture("^[ ]{0,3}(?<fence>`{3,})[^`]*$") catch null] | .[0] // null) as $backticks
       | ([$line | try capture("^[ ]{0,3}(?<fence>~{3,}).*$") catch null] | .[0] // null) as $tildes
       | if $backticks != null then
           .fence = { character: "`", length: ($backticks.fence | length) }
         elif $tildes != null then
           .fence = { character: "~", length: ($tildes.fence | length) }
         else
           .visible += [$line]
         end
     else
       . as $state
       | if ($line | test(
           "^[ ]{0,3}" + $state.fence.character + "{" +
           ($state.fence.length | tostring) + ",}[ \t]*$"))
         then .fence = null
         else .
         end
     end)
  | .visible
  | join("\n");

def leak_without_inert_markdown:
  # Fences are recognized only at line boundaries and close only with the same
  # character at least as long as the opener. If no valid closer appears, the
  # state machine consumes through EOF and emits none of the remaining lines.
  leak_without_fenced_markdown
  # Likewise, an unclosed HTML comment consumes the remainder of the body.
  | gsub("(?s)<!--.*?(?:-->|\\z)"; "");

def leak_exact_fixes_numbers($repo_re):
  [(.body // "")
   | leak_without_inert_markdown
   # Provenance is a dedicated contract line. Anchoring the full line means
   # inline-code delimiters or surrounding prose can never authorize closure.
   | scan("(?im)^[ \t]*Fixes\\b:?[ \t]*(?:" + $repo_re + "#|#)([0-9]+)\\b[ \t]*\\r?$")
   | .[0]];

def leak_issue_reference_numbers($repo_re):
  [(.body // "")
   | leak_without_inert_markdown
   | scan("(?im)^[ \t]*(?:Fixes|Refs)\\b:?[ \t]*(?:" + $repo_re + "#|#)([0-9]+)\\b[ \t]*\\r?$")
   | .[0]];

def leak_first_exact_fixes_number($repo_re):
  leak_exact_fixes_numbers($repo_re) | .[0] // empty;

def leak_has_exact_fixes($repo_re; $number):
  leak_exact_fixes_numbers($repo_re) | any(. == ($number | tostring));

def leak_has_issue_reference($repo_re; $number):
  leak_issue_reference_numbers($repo_re) | any(. == ($number | tostring));

# Keep the marker grammar identical to the workflow runtime: literal spaces are
# accepted around the key, while tabs/newlines are not.
def leak_scan_key:
  [(.body // "")
   | try capture("(?i)<!-- *leak-scan-key: *(?<key>[^>]+?) *-->").key catch null]
  | .[0] // null;

# Input is the `gh api --paginate --slurp` result: an array of page arrays.
# Invalid shapes and missing merge timestamps block closure (fail closed).
def leak_reopen_guard($fix_merged_at):
  if (type != "array") or any(.[]; type != "array") or
     (($fix_merged_at | type) != "string") or ($fix_merged_at == "") then
    { verified: false, block_close: true, reopened_at: null }
  else
    (add // []) as $events
    | ([$events[]
        | select(.event == "reopened")
        | .created_at
        | select(type == "string" and . != "")]
       | max // null) as $reopened_at
    | {
        verified: true,
        block_close: ($reopened_at != null and $reopened_at > $fix_merged_at),
        reopened_at: $reopened_at
      }
  end;
