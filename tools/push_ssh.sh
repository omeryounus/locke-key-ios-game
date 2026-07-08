#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
git push origin main --force
echo "Pushed single commit to main via SSH."