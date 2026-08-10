def leak_exact_fixes_numbers($repo_re):
  [(.body // "")
   | scan("(?i)\\bFixes\\b:?[ \t]*(?:" + $repo_re + "#|[^0-9A-Za-z_/]#)([0-9]+)\\b")
   | .[0]];

def leak_first_exact_fixes_number($repo_re):
  leak_exact_fixes_numbers($repo_re) | .[0] // empty;

def leak_has_exact_fixes($repo_re; $number):
  leak_exact_fixes_numbers($repo_re) | any(. == ($number | tostring));
