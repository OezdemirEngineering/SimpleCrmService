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

# On Linux GitHub runners, host.docker.internal may not resolve inside containers.
# If the target uses host.docker.internal, rewrite it to a reachable IP.
if echo "$TARGET_URL" | grep -q "host.docker.internal"; then
  GW_IP=""
  # 1) If host.docker.internal resolves, use it
  if command -v getent >/dev/null 2>&1; then
    HOST_IP=$(getent hosts host.docker.internal | awk '{print $1}' | head -n1 || true)
    if [ -n "${HOST_IP:-}" ]; then GW_IP=$HOST_IP; fi
  fi
  # 2) Fallback to default gateway via ip route
  if [ -z "${GW_IP:-}" ] && command -v ip >/dev/null 2>&1; then
    GW_IP=$(ip route | awk '/default/ {print $3; exit}' || true)
  fi
  # 3) Fallback to parsing /proc/net/route (works without iproute2)
  if [ -z "${GW_IP:-}" ] && [ -r /proc/net/route ]; then
    GW_IP=$(python - <<'PY' 2>/dev/null || true
import sys
try:
    with open('/proc/net/route') as f:
        for line in f:
            parts = line.split() 
            if len(parts) > 2 and parts[1] == '00000000':
                gw = parts[2]
                ip = '.'.join(str(int(gw[i:i+2],16)) for i in (6,4,2,0))
                print(ip)
                break
except Exception:
    pass
PY
)
  fi
  if [ -n "${GW_IP:-}" ]; then
    TARGET_URL=$(printf '%s' "$TARGET_URL" | sed "s/host\\.docker\\.internal/$GW_IP/g")
    echo "Rewrote target to: $TARGET_URL"
  else
    echo "Warning: Could not determine gateway IP; proceeding with original target" >&2
  fi
fi

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
