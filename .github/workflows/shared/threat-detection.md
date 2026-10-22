---
description: Preserve a single authoritative, structured threat-detection verdict.
safe-outputs:
  threat-detection:
    prompt: |
      Complete the full security analysis yourself, without delegating to subagents.
      Do not emit intermediate, example, or alternative detector verdicts.
      The final verdict must have exactly these fields: prompt_injection,
      secret_leak, malicious_patch (JSON booleans), and reasons (an array of strings).
      Write that final JSON object to
      /tmp/gh-aw/threat-detection/detection_result.json using the file-editing tool.
      Then emit the identical object once, on one line prefixed
      THREAT_DETECTION_RESULT:. Do not repeat it in another format.
      Never default a threat field to false because evidence or analysis is missing.
---
