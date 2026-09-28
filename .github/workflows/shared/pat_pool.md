---
description: Agentic workflow import to integrate the Copilot PAT Pool

jobs:
  pat_pool:
    environment: ${{ github.aw.import-inputs.environment }}
    needs: [pre_activation]
    runs-on: ubuntu-slim
    outputs:
      pat_number: ${{ steps.select-pat-number.outputs.copilot_pat_number }}
    steps:
      - id: select-pat-number
        name: Select Copilot token from pool
        if: needs.pre_activation.outputs.activated == 'true'
        env:
          COPILOT_PAT_0: ${{ github.aw.import-inputs.COPILOT_PAT_0 }}
          COPILOT_PAT_1: ${{ github.aw.import-inputs.COPILOT_PAT_1 }}
          COPILOT_PAT_2: ${{ github.aw.import-inputs.COPILOT_PAT_2 }}
          COPILOT_PAT_3: ${{ github.aw.import-inputs.COPILOT_PAT_3 }}
          COPILOT_PAT_4: ${{ github.aw.import-inputs.COPILOT_PAT_4 }}
          COPILOT_PAT_5: ${{ github.aw.import-inputs.COPILOT_PAT_5 }}
          COPILOT_PAT_6: ${{ github.aw.import-inputs.COPILOT_PAT_6 }}
          COPILOT_PAT_7: ${{ github.aw.import-inputs.COPILOT_PAT_7 }}
          COPILOT_PAT_8: ${{ github.aw.import-inputs.COPILOT_PAT_8 }}
          COPILOT_PAT_9: ${{ github.aw.import-inputs.COPILOT_PAT_9 }}
          RANDOM_SEED: ${{ github.aw.import-inputs.random_seed }}
        shell: bash
        run: |
          # Only select tokens that can authenticate with the Copilot model API.
          PAT_NUMBERS=()
          POOL_INDICATORS=(➖ ➖ ➖ ➖ ➖ ➖ ➖ ➖ ➖ ➖)

          for i in $(seq 0 9); do
            var="COPILOT_PAT_${i}"
            val="${!var}"
            if [ -n "$val" ]; then
              if ! status=$(curl --silent --show-error --proto '=https' \
                --connect-timeout 5 --max-time 15 --retry 2 --retry-delay 2 \
                --output /dev/null --write-out '%{http_code}' \
                --header "Authorization: Bearer $val" \
                --header 'Accept: application/json' \
                https://api.githubcopilot.com/models); then
                echo "::error::Could not verify Copilot PAT slot ${i}; token selection is unsafe."
                exit 1
              fi
              case "$status" in
                200)
                  PAT_NUMBERS+=("$i")
                  POOL_INDICATORS[i]="🟪"
                  ;;
                401|403)
                  POOL_INDICATORS[i]="⚠️"
                  echo "::warning::Copilot PAT slot ${i} cannot authenticate (HTTP ${status}); skipping it."
                  ;;
                *)
                  echo "::error::Copilot PAT slot ${i} check returned HTTP ${status}; token selection is unsafe."
                  exit 1
                  ;;
              esac
            fi
          done

          # Never pass an empty or unauthenticated token to the agent.
          if [ ${#PAT_NUMBERS[@]} -eq 0 ]; then
            echo "::error::No usable Copilot PATs in slots 0 through 9. Renew or replace a PAT before retrying."
            exit 1
          fi

          # Select a random index using the seed if specified.
          if [ -n "$RANDOM_SEED" ]; then
            RANDOM=$RANDOM_SEED
          fi

          PAT_INDEX=$(( RANDOM % ${#PAT_NUMBERS[@]} ))
          PAT_NUMBER="${PAT_NUMBERS[$PAT_INDEX]}"
          POOL_INDICATORS[PAT_NUMBER]="✅"

          echo "Pool size: ${#PAT_NUMBERS[@]}"
          echo "Selected PAT number ${PAT_NUMBER} (index: ${PAT_INDEX})"

          echo "|0|1|2|3|4|5|6|7|8|9|" >> "$GITHUB_STEP_SUMMARY"
          echo "|-|-|-|-|-|-|-|-|-|-|" >> "$GITHUB_STEP_SUMMARY"
          (IFS='|'; printf '|%s' "${POOL_INDICATORS[@]}"; printf '|\n') >> "$GITHUB_STEP_SUMMARY"

          echo "copilot_pat_number=${PAT_NUMBER}" >> "$GITHUB_OUTPUT"

import-schema:
  environment:
    type: string
    required: true
  COPILOT_PAT_0:
    type: string
    required: false
    default: ${{ secrets.COPILOT_PAT_0 }}
  COPILOT_PAT_1:
    type: string
    required: false
    default: ${{ secrets.COPILOT_PAT_1 }}
  COPILOT_PAT_2:
    type: string
    required: false
    default: ${{ secrets.COPILOT_PAT_2 }}
  COPILOT_PAT_3:
    type: string
    required: false
    default: ${{ secrets.COPILOT_PAT_3 }}
  COPILOT_PAT_4:
    type: string
    required: false
    default: ${{ secrets.COPILOT_PAT_4 }}
  COPILOT_PAT_5:
    type: string
    required: false
    default: ${{ secrets.COPILOT_PAT_5 }}
  COPILOT_PAT_6:
    type: string
    required: false
    default: ${{ secrets.COPILOT_PAT_6 }}
  COPILOT_PAT_7:
    type: string
    required: false
    default: ${{ secrets.COPILOT_PAT_7 }}
  COPILOT_PAT_8:
    type: string
    required: false
    default: ${{ secrets.COPILOT_PAT_8 }}
  COPILOT_PAT_9:
    type: string
    required: false
    default: ${{ secrets.COPILOT_PAT_9 }}
  random_seed:
    type: number
    required: false
    description: >-
      A seed number to use for the random PAT number selection,
      for deterministic selection if needed.
---
