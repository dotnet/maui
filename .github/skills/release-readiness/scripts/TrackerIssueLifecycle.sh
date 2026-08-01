#!/usr/bin/env bash

rr_select_oldest_tracker() {
  printf '%s\n' "$1" | sed '/^[[:space:]]*$/d' | head -n 1
}

rr_edit_landed_after_close() {
  local closed_at="$1"
  local edit_updated_at="$2"
  [ -n "$closed_at" ] && [ -n "$edit_updated_at" ] && [[ "$edit_updated_at" > "$closed_at" ]]
}

rr_has_exact_marker_line() {
  local input_file="$1"
  local marker="$2"

  LC_ALL=C awk -v marker="$marker" '{
    normalized = $0
    sub(/\r$/, "", normalized)
    if (normalized == marker) {
      found = 1
      exit
    }
  }
  END { exit(found ? 0 : 1) }' "$input_file"
}

rr_remove_exact_marker_line() {
  local input_file="$1"
  local output_file="$2"
  local marker="$3"

  LC_ALL=C awk -v marker="$marker" '{
    normalized = $0
    sub(/\r$/, "", normalized)
    if (normalized != marker) {
      print $0
    }
  }' "$input_file" > "$output_file"
}
