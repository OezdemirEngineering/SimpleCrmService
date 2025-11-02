#!/usr/bin/env sh
set -eu

if [ "$1" = "--" ]; then shift; fi
TARGET_URL=${1:?"target url required"}
REPORT_PATH=${2:?"report path required"}
EXTRA_ARGS=${3:-}

# Ensure ZAP writes any temp/automation files to a writable location
# GitHub Actions sets HOME to /github/home which can be read-only for non-root users in container actions
export HOME=/zap/wrk
mkdir -p "$HOME"

# Run baseline scan
zap-baseline.py -t "$TARGET_URL" -r zap_report.html $EXTRA_ARGS

# Ensure destination directory exists and copy report out
mkdir -p "$(dirname "$REPORT_PATH")"
cp -f /zap/wrk/zap_report.html "$REPORT_PATH"

# Print a short summary to logs
if [ -f "$REPORT_PATH" ]; then
  echo "ZAP report written to $REPORT_PATH"
else
  echo "ZAP report not found" >&2
  exit 2
fi
