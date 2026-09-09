#!/usr/bin/env bash

set -euo pipefail

: "${GITHUB_REPOSITORY:?GITHUB_REPOSITORY must be set}"
: "${TRACKER_KEY:?TRACKER_KEY must be set}"
: "${ISSUE_TITLE:?ISSUE_TITLE must be set}"
: "${BODY_FILE:?BODY_FILE must be set}"
: "${RECENT_COMMIT_COUNT:?RECENT_COMMIT_COUNT must be set}"
: "${MODE:?MODE must be set}"

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/TrackerIssueLifecycle.sh"

# Find any open issue whose body carries the canonical marker for this tracker.
# This is the idempotent join key — Get-ReleaseReadiness / Get-PreviewReadiness
# both embed `<!-- release-readiness-tracker: $TRACKER_KEY -->`.
MARKER="<!-- release-readiness-tracker: ${TRACKER_KEY} -->"
# The generated report is authoritative for lifecycle state. Detector
# matrix values can become stale if a tag or branch commit lands between
# detection and report generation; the body markers reflect the exact
# refs the report actually surveyed.
HOTFIX_MARKER=$(LC_ALL=C grep -m1 -E '^<!-- release-readiness-hotfix: [^>]+ -->$' "$BODY_FILE" || true)
SHIPPED_MARKER=$(LC_ALL=C grep -m1 -E '^<!-- release-readiness-shipped: [^>]+ -->$' "$BODY_FILE" || true)
REPORT_HOTFIX_IN_PROGRESS=false
[ -n "$HOTFIX_MARKER" ] && REPORT_HOTFIX_IN_PROGRESS=true
CREATE_GENERATION=false
GENERATION_MARKER=""
if [ "$MODE" = "shipped" ]; then
  GENERATION_MARKER="$SHIPPED_MARKER"
  [ "$REPORT_HOTFIX_IN_PROGRESS" = "true" ] && GENERATION_MARKER="$HOTFIX_MARKER"
  if [ -z "$GENERATION_MARKER" ]; then
    echo "::warning::Shipped report for ${TRACKER_KEY} has no lifecycle generation marker; refusing to create or infer closure state."
    exit 0
  fi
fi

EXISTING=$(gh issue list \
  --repo "${GITHUB_REPOSITORY}" \
  --state open \
  --label area-infrastructure \
  --search "in:body \"${MARKER}\"" \
  --json number,title,createdAt,body,labels \
  --limit 100 | jq -r \
    --arg tracker "$MARKER" '
      [.[] | select(
        ([.labels[].name] | index("area-infrastructure") != null) and
        (((.body // "") | gsub("\r"; "") | split("\n") | index($tracker)) != null)
      )] | sort_by(.createdAt) | .[].number
    ')

# Do not silently orphan or duplicate a tracker whose ownership label
# was removed. Exact marker text proves a candidate exists, but without
# the durable label the workflow must not edit or adopt it automatically.
if [ -z "$EXISTING" ]; then
  UNOWNED_EXISTING=$(gh issue list \
    --repo "${GITHUB_REPOSITORY}" \
    --state open \
    --search "in:body \"${MARKER}\"" \
    --json number,createdAt,body,labels \
    --limit 100 | jq -r \
      --arg tracker "$MARKER" '
        [.[] | select(
          ([.labels[].name] | index("area-infrastructure") == null) and
          (((.body // "") | gsub("\r"; "") | split("\n") | index($tracker)) != null)
        )] | sort_by(.createdAt) | .[].number
      ')
  if [ -n "$UNOWNED_EXISTING" ]; then
    echo "::warning::Open tracker marker ${MARKER} exists on unlabeled issue(s): $(echo "$UNOWNED_EXISTING" | tr '\n' ' '). Restore the area-infrastructure label before automation resumes; refusing to create a duplicate."
    exit 0
  fi
fi

# Each exact shipped-tag or hotfix-tip generation is create-once. A
# later hotfix commit intentionally has a new version@commit marker.
# This distinguishes an intentionally closed generation from one that was
# never tracked (for example, a hotfix tag published before the first
# scheduled updater run). Search the exact generated marker directly so
# the result is not diluted by a generic-marker --limit window.
if [ "$MODE" = "shipped" ]; then
  EXACT_OPEN=$(gh issue list \
    --repo "${GITHUB_REPOSITORY}" \
    --state open \
    --label area-infrastructure \
    --search "in:body \"${GENERATION_MARKER}\"" \
    --json number,createdAt,body,labels \
    --limit 100 | jq -r \
      --arg tracker "$MARKER" \
      --arg generation "$GENERATION_MARKER" '
        [.[] | select(
          ([.labels[].name] | index("area-infrastructure") != null) and
          (((.body // "") | gsub("\r"; "") | split("\n")) as $lines |
            ($lines | index($tracker) != null) and
            ($lines | index($generation) != null))
        )] | sort_by(.createdAt) | .[].number
      ')

  if [ -n "$EXACT_OPEN" ]; then
    # The exact open proves this generation was tracked, but the oldest
    # generic tracker remains canonical. It may carry Release Captain
    # Notes that must not be discarded when duplicate cleanup runs.
    echo "Current generation is already open as issue(s): $(echo "$EXACT_OPEN" | tr '\n' ' '); preserving oldest tracker #$(rr_select_oldest_tracker "$EXISTING") as canonical."
  else
    # Narrow server-side to tracker-labeled issues, then enforce exact
    # marker lines locally. Legacy/current trackers may be human-created
    # and later adopted by this workflow, so author identity is not a
    # durable ownership signal.
    CLOSED_GENERATION=$(gh issue list \
      --repo "${GITHUB_REPOSITORY}" \
      --state closed \
      --label area-infrastructure \
      --search "in:body \"${GENERATION_MARKER}\"" \
      --json number,closedAt,body,labels \
      --limit 100 | jq -r \
        --arg tracker "$MARKER" \
        --arg generation "$GENERATION_MARKER" '
          [.[] | select(
            ([.labels[].name] | index("area-infrastructure") != null) and
            (((.body // "") | gsub("\r"; "") | split("\n")) as $lines |
              ($lines | index($tracker) != null) and
              ($lines | index($generation) != null))
          )] | sort_by(.closedAt) | last | .number // empty
        ')
    if [ -n "$CLOSED_GENERATION" ]; then
      # A prior best-effort duplicate close may have failed. Reconcile
      # stale generic opens before honoring the exact closed generation.
      for stale in $EXISTING; do
        echo "Closing stale tracker issue #$stale because exact generation ${GENERATION_MARKER} is already closed as #${CLOSED_GENERATION}"
        gh issue close "$stale" \
          --repo "${GITHUB_REPOSITORY}" \
          --reason "completed" \
          --comment "Closing stale tracker: exact release-readiness generation was intentionally closed as #${CLOSED_GENERATION}." || true
      done
      echo "Tracker generation ${GENERATION_MARKER} was intentionally closed as #${CLOSED_GENERATION} — not recreating."
      exit 0
    fi
    # Keep one open tracker stable across commits so Release Captain
    # Notes and subscriptions survive. A new issue is needed only when
    # there is no open tracker and this exact generation was never closed.
    [ -z "$EXISTING" ] && CREATE_GENERATION=true
  fi
fi

# Activity gate: when there is no recent activity AND no open tracker issue,
# skip new-issue creation. A newly observed shipped/hotfix generation
# is exempt even when its commits fall outside the activity window.
if [ "$RECENT_COMMIT_COUNT" -eq 0 ] && [ "$CREATE_GENERATION" != "true" ] && [ -z "$EXISTING" ]; then
  echo "Skipping ${TRACKER_KEY}: no recent commits and no open tracker issue."
  exit 0
fi

if [ -n "$EXISTING" ]; then
  # Reuse the OLDEST open tracker issue (first in chronological order).
  # Close any duplicates created by past misfires before refreshing the canonical one.
  CANONICAL=$(rr_select_oldest_tracker "$EXISTING")
  DUPLICATES=$(echo "$EXISTING" | tail -n +2 || true)

  for dup in $DUPLICATES; do
    echo "Closing duplicate tracker issue #$dup"
    gh issue close "$dup" \
      --repo "${GITHUB_REPOSITORY}" \
      --reason "not planned" \
      --comment "Closing as duplicate of #${CANONICAL} — there should be exactly one open tracker per release branch." || true
  done

  echo "Refreshing tracker issue #${CANONICAL} for ${TRACKER_KEY}"

  # Preserve the human-editable "Release Captain Notes" block and avoid
  # churning the issue when nothing material changed. Both engines emit
  # <!-- release-readiness:human-notes:begin/end --> markers; the SR engine
  # additionally embeds <!-- release-readiness-hash: sha=... -->.
  #
  # Concurrency note: the per-tracker `concurrency` group on this job
  # (release-readiness-tracker-<canonicalKey>, cancel-in-progress:false) is
  # the PRIMARY guard against two runs racing this splice+edit for the same
  # tracker — it serializes same-tracker writers across every event kind. The
  # read-modify-write below (re-read live body → splice notes → edit) is the
  # defense-in-depth second layer: even if serialization were ever bypassed,
  # the fetch is taken IMMEDIATELY before the edit and every ambiguous state
  # skips the edit (see the guards below) rather than risk clobbering notes.
  CUR_BODY_FILE="$(mktemp)"
  # Capture the live issue body. A transient fetch failure must NOT lead
  # to an overwrite: an empty CUR_BODY_FILE would skip the notes splice
  # AND zero out OLD_HASH, falling through to `gh issue edit` and wiping
  # the human-authored Release Captain Notes. Guard the exit status and
  # skip the whole refresh instead (a missing refresh self-heals next run;
  # lost notes do not).
  CUR_FETCH_OK=1
  CUR_META=$(gh issue view "$CANONICAL" \
    --repo "${GITHUB_REPOSITORY}" \
    --json body,updatedAt \
    --jq '[.updatedAt, ((.body // "") | @base64)] | @tsv' 2>/dev/null) || CUR_FETCH_OK=0
  if [ "$CUR_FETCH_OK" -eq 1 ]; then
    IFS=$'\t' read -r CUR_UPDATED_AT CUR_BODY_B64 <<< "$CUR_META"
    printf '%s' "$CUR_BODY_B64" | base64 -d > "$CUR_BODY_FILE" || CUR_FETCH_OK=0
  fi

  if [ "$CUR_FETCH_OK" -ne 1 ]; then
    echo "::warning::Could not read live body of issue #${CANONICAL}; skipping refresh to protect Release Captain Notes."
  else
    # Detect the human-notes block using the SAME anchored full-line
    # markers the awk splice relies on. A substring (unanchored) guard
    # desyncs from the awk and silently wipes the Release Captain Notes:
    #   * a note that merely MENTIONS the end token makes the count 2, the
    #     -eq 1 guard fails, the splice is skipped, and the edit overwrites
    #     the notes; and
    #   * a marker line carrying trailing text passes a substring guard but
    #     the anchored awk matches nothing, splicing in an EMPTY block.
    # The anchors tolerate the CRLF bodies GitHub returns (\r is ASCII
    # whitespace in every locale). LC_ALL=C is MANDATORY on every grep and
    # awk here: GNU grep in the runner's UTF-8 locale treats Unicode spaces
    # (e.g. U+00A0 NO-BREAK SPACE, easily pasted from a web editor) as
    # [[:space:]], but mawk (the runner default) does not — so a UTF-8
    # grep guard could PASS while the awk extracts nothing, splicing an
    # EMPTY block over real notes. Forcing C locale makes grep and awk
    # agree on ASCII-only [[:space:]], so a weird space fails the guard and
    # freezes the issue (safe) instead of destroying the notes.
    NOTES_BEGIN_RE='^[[:space:]]*<!-- release-readiness:human-notes:begin -->[[:space:]]*$'
    NOTES_END_RE='^[[:space:]]*<!-- release-readiness:human-notes:end -->[[:space:]]*$'
    CUR_HAS_CLEAN_NOTES=0
    if [ "$(LC_ALL=C grep -cE "$NOTES_BEGIN_RE" "$CUR_BODY_FILE")" -eq 1 ] \
       && [ "$(LC_ALL=C grep -cE "$NOTES_END_RE" "$CUR_BODY_FILE")" -eq 1 ]; then
      CUR_HAS_CLEAN_NOTES=1
    fi
    # Does the FRESH body carry exactly one clean begin+end pair? If a
    # truncated/markerless fresh body would be used to overwrite an issue
    # that HAS real notes, those notes are lost — so we require this too.
    BODY_HAS_CLEAN_NOTES=0
    if [ "$(LC_ALL=C grep -cE "$NOTES_BEGIN_RE" "$BODY_FILE")" -eq 1 ] \
       && [ "$(LC_ALL=C grep -cE "$NOTES_END_RE" "$BODY_FILE")" -eq 1 ]; then
      BODY_HAS_CLEAN_NOTES=1
    fi

    SKIP_EDIT=0
    # 1) Splice any human-authored notes from the live issue into the fresh
    #    body, replacing the freshly generated placeholder block. Require a
    #    COMPLETE, single begin+end marker pair in BOTH bodies — an
    #    unterminated or duplicated block would otherwise capture the entire
    #    stale report to EOF and re-inject it, growing the body every run.
    if [ "$CUR_HAS_CLEAN_NOTES" -eq 1 ] && [ "$BODY_HAS_CLEAN_NOTES" -eq 1 ]; then
      MERGED_BODY_FILE="$(mktemp)"
      # Markers are matched as ANCHORED FULL LINES so a note that merely
      # mentions the marker text cannot prematurely terminate capture.
      # LC_ALL=C keeps awk's [[:space:]] ASCII-only, matching the grep guard.
      LC_ALL=C awk '
        /^[[:space:]]*<!-- release-readiness:human-notes:begin -->[[:space:]]*$/ {
          if (FNR==NR) { cap=1; next } else { print; printf "%s", notes; skip=1; next }
        }
        /^[[:space:]]*<!-- release-readiness:human-notes:end -->[[:space:]]*$/ {
          if (FNR==NR) { cap=0; next } else { print; skip=0; next }
        }
        FNR==NR { if (cap) { notes = notes $0 "\n" } ; next }
        { if (!skip) print }
      ' "$CUR_BODY_FILE" "$BODY_FILE" > "$MERGED_BODY_FILE"
      mv "$MERGED_BODY_FILE" "$BODY_FILE"
      echo "Preserved existing Release Captain Notes block."
    elif [ "$CUR_HAS_CLEAN_NOTES" -eq 1 ] && [ "$BODY_HAS_CLEAN_NOTES" -ne 1 ]; then
      # The live issue HAS clean notes but the freshly generated body does
      # NOT carry a clean begin+end pair (e.g. truncated below the cap, or
      # markers otherwise missing). Splicing is impossible and overwriting
      # would wipe the live notes, so skip the edit entirely. Self-heals on
      # the next run once the fresh body regains its markers.
      echo "::warning::Fresh report for #${CANONICAL} lacks clean notes markers (truncated?); skipping edit to protect existing Release Captain Notes."
      SKIP_EDIT=1
    elif [ "$CUR_HAS_CLEAN_NOTES" -ne 1 ] \
         && LC_ALL=C grep -q 'release-readiness:human-notes:' "$CUR_BODY_FILE"; then
      # The live body carries notes markers that don't resolve to a single
      # clean begin+end pair (corrupted, duplicated, or text on the marker
      # line). We can't splice safely and overwriting would wipe the notes,
      # so skip the edit entirely — self-heals once the markers are a clean
      # pair again (a stale refresh recovers; destroyed captain notes do not).
      echo "::warning::Issue #${CANONICAL} has malformed Release Captain Notes markers; skipping edit to protect them."
      SKIP_EDIT=1
    fi

    # 1b) Final body-size guard. The awk splice above injects the LIVE
    #     notes block (which a captain may have grown to many KB) into the
    #     freshly capped body. The engines cap the FRESH body, reserving
    #     room only for the small notes PLACEHOLDER — they never see the
    #     live-notes size — so a busy report plus large notes can push the
    #     merged body past GitHub's 65,536-byte issue-body limit, which
    #     makes `gh issue edit` 422 and fail the run under set -e. Skip the
    #     edit instead (notes stay safe; the report just stays stale this
    #     run) and self-heal once the report or the notes shrink. `wc -c`
    #     counts bytes — matching the engines' byte-based cap — and is
    #     conservative against GitHub's character limit.
    if [ "$SKIP_EDIT" -ne 1 ]; then
      MERGED_SIZE=$(wc -c < "$BODY_FILE")
      if [ "$MERGED_SIZE" -gt 65536 ]; then
        echo "::warning::Body for #${CANONICAL} is ${MERGED_SIZE} bytes (> GitHub's 65536-byte limit) after splicing live notes; skipping edit to avoid a failed gh issue edit. Self-heals once the report or notes shrink."
        SKIP_EDIT=1
      fi
    fi

    # 2) Idempotent no-op: if the semantic hash is unchanged, skip the edit
    #    so scheduled re-runs don't spam watchers. The engine emits its hash
    #    at the very TOP of the body, ABOVE the human-notes block, so scope
    #    extraction to the pre-notes region with `sed '/begin/q'`. Anchoring
    #    the grep to the full HTML-comment form is not enough on its own: the
    #    `<!-- release-readiness-hash: sha=... -->` line is exactly what a
    #    captain copies from a prior raw-markdown run and may paste INTO their
    #    notes; the splice then carries it into the fresh body. On Preview
    #    trackers (which emit NO hash and must refresh every run) that pasted
    #    line would make OLD_HASH==NEW_HASH and FREEZE the issue. Scoping to
    #    above the notes block makes any hash inside the notes invisible to the
    #    compare, regardless of paste form. The anchored grep keeps the match
    #    precise and drops a trailing CRLF \r from the captured hash.
    if [ "$SKIP_EDIT" -ne 1 ]; then
      OLD_HASH=$(sed '/<!-- release-readiness:human-notes:begin -->/q' "$CUR_BODY_FILE" | grep -oE '<!-- release-readiness-hash: sha=[0-9a-f]+ -->' | head -n1 | sed 's/.*sha=//; s/ -->//') || true
      NEW_HASH=$(sed '/<!-- release-readiness:human-notes:begin -->/q' "$BODY_FILE"     | grep -oE '<!-- release-readiness-hash: sha=[0-9a-f]+ -->' | head -n1 | sed 's/.*sha=//; s/ -->//') || true
      if [ -n "$NEW_HASH" ] && [ "$OLD_HASH" = "$NEW_HASH" ]; then
        echo "Semantic hash unchanged (${NEW_HASH}) — skipping issue edit (no-op)."
      else
        # A human can close the issue while this run is preparing the
        # notes splice. Re-read state immediately before the write to
        # minimize the close/edit race, then compensate below when
        # GitHub timestamps prove the edit landed after the closure.
        GENERATION_TRANSITION=false
        if [ -n "$GENERATION_MARKER" ] && ! rr_has_exact_marker_line "$CUR_BODY_FILE" "$GENERATION_MARKER"; then
          GENERATION_TRANSITION=true
        fi
        PRE_EDIT_META=$(gh issue view "$CANONICAL" \
          --repo "${GITHUB_REPOSITORY}" \
          --json state,body,updatedAt \
          --jq '[.state, .updatedAt, ((.body // "") | @base64)] | @tsv' 2>/dev/null || true)
        IFS=$'\t' read -r PRE_EDIT_STATE PRE_EDIT_UPDATED_AT PRE_EDIT_BODY_B64 <<< "$PRE_EDIT_META"
        if [ "$PRE_EDIT_STATE" != "OPEN" ]; then
          echo "::warning::Issue #${CANONICAL} is no longer open; skipping refresh to preserve the human closure."
        elif [ "$PRE_EDIT_UPDATED_AT" != "$CUR_UPDATED_AT" ] || [ "$PRE_EDIT_BODY_B64" != "$CUR_BODY_B64" ]; then
          echo "::warning::Issue #${CANONICAL} changed while this refresh was preparing; skipping edit to preserve concurrent Release Captain Notes. The next run will merge the latest body."
        else
          if [ "$GENERATION_TRANSITION" = "true" ]; then
            # Capture the timestamp and body from this exact mutation.
            # A later issue event can advance the aggregate updatedAt,
            # so a post-read timestamp cannot prove edit/close order.
            EDIT_RESULT=$(gh api --method PATCH \
              "repos/${GITHUB_REPOSITORY}/issues/${CANONICAL}" \
              -F title="$ISSUE_TITLE" \
              -F body=@"$BODY_FILE" \
              --jq '[.updated_at, ((.body // "") | @base64)] | @tsv')
            IFS=$'\t' read -r EDIT_UPDATED_AT EDIT_BODY_B64 <<< "$EDIT_RESULT"
            POST_EDIT_META=''
            if ! POST_EDIT_META=$(gh issue view "$CANONICAL" \
             --repo "${GITHUB_REPOSITORY}" \
             --json state,closedAt,updatedAt,body \
             --jq '[.state, (.closedAt // ""), (.updatedAt // ""), ((.body // "") | @base64)] | @tsv' 2>/dev/null); then
             echo "::warning::Could not re-read issue #${CANONICAL} after the generation-transition edit; race compensation could not be evaluated."
            elif [ -z "$POST_EDIT_META" ]; then
             echo "::warning::Issue #${CANONICAL} returned no post-edit metadata; race compensation could not be evaluated."
            else
             IFS=$'\t' read -r POST_EDIT_STATE POST_CLOSED_AT POST_UPDATED_AT POST_BODY_B64 <<< "$POST_EDIT_META"
            if [ "$POST_EDIT_STATE" = "CLOSED" ] &&
              [ -n "$EDIT_BODY_B64" ] &&
              [ -n "$POST_BODY_B64" ] &&
              [ "$POST_BODY_B64" = "$EDIT_BODY_B64" ] &&
              rr_edit_landed_after_close "$POST_CLOSED_AT" "$EDIT_UPDATED_AT"; then
             # The mutation response proves this exact edit happened
             # after the human closure and remains the live body.
             # Start from the live closed body, then recheck its revision
             # immediately before removing only the raced marker.
             RACE_CURRENT_BODY_FILE="$(mktemp)"
              RACE_BODY_FILE="$(mktemp)"
             echo "$POST_BODY_B64" | base64 --decode > "$RACE_CURRENT_BODY_FILE"
             rr_remove_exact_marker_line "$RACE_CURRENT_BODY_FILE" "$RACE_BODY_FILE" "$GENERATION_MARKER"
             PRE_RACE_META=$(gh issue view "$CANONICAL" \
               --repo "${GITHUB_REPOSITORY}" \
               --json state,updatedAt,body \
               --jq '[.state, .updatedAt, ((.body // "") | @base64)] | @tsv' 2>/dev/null || true)
             IFS=$'\t' read -r PRE_RACE_STATE PRE_RACE_UPDATED_AT PRE_RACE_BODY_B64 <<< "$PRE_RACE_META"
             if [ "$PRE_RACE_STATE" != "CLOSED" ] ||
                [ "$PRE_RACE_UPDATED_AT" != "$POST_UPDATED_AT" ] ||
                [ "$PRE_RACE_BODY_B64" != "$POST_BODY_B64" ]; then
               echo "::warning::Issue #${CANONICAL} changed during generation-race compensation; preserving the latest human body and leaving reconciliation to the next run."
             else
               if gh issue edit "$CANONICAL" \
                 --repo "${GITHUB_REPOSITORY}" \
                 --body-file "$RACE_BODY_FILE"; then
                 echo "::warning::Issue #${CANONICAL} closed before the generation-transition edit completed; removed the raced marker from the live closed body so the next run can recreate the new generation."
               else
                 echo "::warning::Could not remove the raced generation marker from closed issue #${CANONICAL}; manual reconciliation may be required."
               fi
             fi
             rm -f "$RACE_CURRENT_BODY_FILE" "$RACE_BODY_FILE"
            fi
            fi
          else
            gh issue edit "$CANONICAL" \
             --repo "${GITHUB_REPOSITORY}" \
             --title "$ISSUE_TITLE" \
             --body-file "$BODY_FILE"
          fi
        fi
      fi
    fi
  fi
else
  echo "Creating new tracker issue for ${TRACKER_KEY}"
  CREATE_ARGS=(
    --repo "${GITHUB_REPOSITORY}"
    --title "$ISSUE_TITLE"
    --body-file "$BODY_FILE"
  )
  # area-infrastructure is the durable ownership boundary used by the
  # closed-generation lookup. Refuse to create an issue that the
  # lifecycle code could not recognize later.
  if gh api "repos/${GITHUB_REPOSITORY}/labels/area-infrastructure" --jq '.name' >/dev/null 2>&1; then
    CREATE_ARGS+=(--label "area-infrastructure")
  else
    echo "::error::Required label 'area-infrastructure' not found; refusing to create an untrackable release-readiness issue."
    exit 1
  fi
  # The remaining organizational labels are best-effort.
  for lbl in "report" "s/triaged"; do
    if gh api "repos/${GITHUB_REPOSITORY}/labels/${lbl//\//%2F}" --jq '.name' >/dev/null 2>&1; then
      CREATE_ARGS+=(--label "$lbl")
    else
      echo "::warning::Label '$lbl' not found; creating issue without it."
    fi
  done
  # Best-effort milestone attach — never fail the job for a missing milestone.
  if [ -n "$MILESTONE_NAME" ]; then
    if gh api "repos/${GITHUB_REPOSITORY}/milestones?state=open&per_page=100" \
        --jq ".[] | select(.title == \"$MILESTONE_NAME\") | .number" \
        | grep -q .; then
      CREATE_ARGS+=(--milestone "$MILESTONE_NAME")
    else
      echo "::warning::Milestone '$MILESTONE_NAME' not found; creating issue without milestone."
    fi
  fi
  gh issue create "${CREATE_ARGS[@]}"
fi
