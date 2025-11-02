# syntax=docker/dockerfile:1
FROM ghcr.io/zaproxy/zaproxy:stable

# Default workdir where ZAP writes reports
WORKDIR /zap/wrk

# Entry script to run baseline and copy report to repo path
COPY entrypoint.sh /entrypoint.sh

# Use sh to execute the script without requiring executable bit changes
ENTRYPOINT ["sh", "/entrypoint.sh"]
