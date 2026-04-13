#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PORT="${1:-5500}"

cd "$ROOT_DIR"
exec python3 -m http.server "$PORT" --directory frontend
