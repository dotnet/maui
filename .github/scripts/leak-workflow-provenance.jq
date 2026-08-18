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
   # inline-code delimiters, indented code, tabs, or surrounding prose cannot
   # become provenance. CommonMark permits at most three leading spaces before
   # a rendered contract line; four spaces or a tab starts an inert code block.
   | scan("(?im)^[ ]{0,3}Fixes\\b:?[ ]*(?:" + $repo_re + "#|#)([0-9]+)\\b[ ]*\\r?$")
   | .[0]];

def leak_issue_reference_numbers($repo_re):
  [(.body // "")
   | leak_without_inert_markdown
   | scan("(?im)^[ ]{0,3}(?:Fixes|Refs)\\b:?[ ]*(?:" + $repo_re + "#|#)([0-9]+)\\b[ ]*\\r?$")
   | .[0]];

def leak_first_exact_fixes_number($repo_re):
  leak_exact_fixes_numbers($repo_re) | .[0] // empty;

def leak_has_exact_fixes($repo_re; $number):
  leak_exact_fixes_numbers($repo_re) | any(. == ($number | tostring));

def leak_has_issue_reference($repo_re; $number):
  leak_issue_reference_numbers($repo_re) | any(. == ($number | tostring));

# A leak-scan marker is itself an HTML comment, so the general inert-markdown
# remover cannot preserve it. Strip fenced code first, then inspect complete
# HTML comments one by one and accept only a dedicated contract line with 0–3
# leading spaces whose entire comment content is the marker. A marker nested
# inside a larger comment, inline in prose, or indented as code is therefore
# inert, as is an unclosed comment; tabs/newlines are not accepted.
def leak_scan_key:
  [(.body // "")
   | leak_without_fenced_markdown
   | scan("(?ms)^[ ]{0,3}(<!--.*?-->)[ ]*\\r?$")
   | .[0]
   | try capture("(?i)^<!-- *leak-scan-key: *(?<key>[^>\\t\\r\\n]+?) *-->$").key catch null
   | select(. != null)]
  | .[0] // null;

# GitHub exposes PullRequest.lastEditedAt and the complete edit history via
# GraphQL. Current PR bodies are safe as merge-time provenance only when the
# latest body edit was no later than the merge. Missing or malformed metadata
# fails closed so a post-merge `Fixes` edit cannot affect later automation.
def leak_merge_provenance_guard:
  . as $pr
  | (try ($pr.mergedAt | fromdateiso8601) catch null) as $merged_at
  | if $merged_at == null then
      {
        verified: false,
        block_provenance: true,
        merged_at: ($pr.mergedAt // null),
        last_edited_at: ($pr.lastEditedAt // null)
      }
    elif $pr.lastEditedAt == null then
      {
        verified: true,
        block_provenance: false,
        merged_at: $pr.mergedAt,
        last_edited_at: null
      }
  else
    (try ($pr.lastEditedAt | fromdateiso8601) catch null) as $last_edited_at
    | if $last_edited_at == null then
        {
          verified: false,
          block_provenance: true,
          merged_at: $pr.mergedAt,
          last_edited_at: $pr.lastEditedAt
        }
      else
        {
          verified: true,
          block_provenance: ($last_edited_at > $merged_at),
          merged_at: $pr.mergedAt,
          last_edited_at: $pr.lastEditedAt
        }
      end
  end;
