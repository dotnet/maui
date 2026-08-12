# Deliberately accept only the workflow contract's literal `Fixes` keyword. Generated
# [leak-fix] PRs are required to use it, and keeping the destructive proof narrower than
# GitHub's full closing-keyword set is fail-closed.
def leak_without_inert_markdown:
  gsub("(?s)<!--.*?-->"; "")
  | gsub("(?ms)^[ \t]*~{3,}[^\\r\\n]*\\r?\\n.*?^[ \t]*~{3,}[ \t]*(?:\\r?\\n|$)"; "")
  | gsub("(?s)`+.*?`+"; "");

def leak_exact_fixes_numbers($repo_re):
  [(.body // "")
   | leak_without_inert_markdown
   | scan("(?i)\\bFixes\\b:?[ \t]*(?:" + $repo_re + "#|#)([0-9]+)\\b")
   | .[0]];

def leak_first_exact_fixes_number($repo_re):
  leak_exact_fixes_numbers($repo_re) | .[0] // empty;

def leak_has_exact_fixes($repo_re; $number):
  leak_exact_fixes_numbers($repo_re) | any(. == ($number | tostring));
