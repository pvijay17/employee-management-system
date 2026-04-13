#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTNET_BIN="${DOTNET_BIN:-/opt/homebrew/opt/dotnet@8/libexec/dotnet}"

if [[ ! -x "$DOTNET_BIN" ]]; then
  DOTNET_BIN="$(command -v dotnet)"
fi

export DOTNET_ROOT="$(cd "$(dirname "$DOTNET_BIN")/.." && pwd)"
export DOTNET_CLI_HOME="${DOTNET_CLI_HOME:-/tmp/dotnet8home}"
export HOME="${HOME:-$DOTNET_CLI_HOME}"
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

cd "$ROOT_DIR"
exec "$DOTNET_BIN" run --project EMS.API --launch-profile http "$@"
