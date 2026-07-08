#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
HOOK="${ROOT}/.git/hooks/pre-commit"

cat > "${HOOK}" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
ROOT="$(git rev-parse --show-toplevel)"
exec "${ROOT}/tools/verify_unity.sh" project
EOF

chmod +x "${HOOK}"
echo "Installed pre-commit hook: ${HOOK}"
echo "It runs tools/verify_unity.sh project (requires Unity on macOS)."