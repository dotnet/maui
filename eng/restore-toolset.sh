#!/usr/bin/env bash

if [[ "$OSTYPE" == darwin* ]]; then
  # Apple's PrepareAssemblies redirects Console globally, so Arcade's optional
  # timeline logger triggers MT7178. Keep normal MSBuild console/binlog output
  # and failure reporting while avoiding that logger's Console.WriteLine calls.
  pipelines_log=false
fi
