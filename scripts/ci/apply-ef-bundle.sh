#!/usr/bin/env bash

set -euo pipefail

usage() {
  cat <<'EOF'
Usage: apply-ef-bundle.sh --env-file <path> --bundle-path <path> [--podman-network <name>] [--container-image <image>]
EOF
}

env_file=""
bundle_path=""
podman_network=""
container_image="mcr.microsoft.com/dotnet/runtime-deps:10.0-azurelinux3.0"

while [ "$#" -gt 0 ]; do
  case "$1" in
    --env-file)
      env_file="${2:-}"
      shift 2
      ;;
    --bundle-path)
      bundle_path="${2:-}"
      shift 2
      ;;
    --podman-network)
      podman_network="${2:-}"
      shift 2
      ;;
    --container-image)
      container_image="${2:-}"
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage >&2
      exit 1
      ;;
  esac
done

if [ -z "$env_file" ] || [ -z "$bundle_path" ]; then
  usage >&2
  exit 1
fi

if [ ! -f "$env_file" ]; then
  echo "Database env file not found: $env_file" >&2
  exit 1
fi

if [ ! -f "$bundle_path" ]; then
  echo "EF migration bundle not found: $bundle_path" >&2
  exit 1
fi

connection_string="$({
python3 - "$env_file" <<'PY'
import pathlib
import sys

env_file = pathlib.Path(sys.argv[1])

for raw_line in env_file.read_text().splitlines():
    line = raw_line.strip()
    if not line or line.startswith('#'):
        continue

    if line.startswith('export '):
        line = line[len('export '):].strip()

    if '=' not in line:
        continue

    key, value = line.split('=', 1)

    if key.strip() != 'ConnectionStrings__rocketreps':
        continue

    value = value.strip()
    if len(value) >= 2 and value[0] == value[-1] and value[0] in {'"', "'"}:
        value = value[1:-1]

    print(value)
    break
else:
    raise SystemExit('ConnectionStrings__rocketreps was not found in the database env file.')
PY
} )"

chmod +x "$bundle_path"

connection_host="$({
python3 - "$connection_string" <<'PY'
import sys

connection_string = sys.argv[1]

for segment in connection_string.split(';'):
    if '=' not in segment:
        continue

    key, value = segment.split('=', 1)
    if key.strip().lower() not in {'host', 'server'}:
        continue

    print(value.strip())
    break
PY
} )"

if [ -n "$podman_network" ]; then
  if ! command -v podman >/dev/null 2>&1; then
    echo "Podman is required when --podman-network is provided." >&2
    exit 1
  fi

  if ! podman network exists "$podman_network"; then
    echo "Podman network not found: $podman_network" >&2
    exit 1
  fi

  bundle_path="$({
  python3 - "$bundle_path" <<'PY'
import pathlib
import sys

print(pathlib.Path(sys.argv[1]).resolve())
PY
  } )"
  bundle_dir="$(dirname "$bundle_path")"
  bundle_name="$(basename "$bundle_path")"

  podman run --rm --pull=missing \
    --network "$podman_network" \
    --volume "$bundle_dir:/bundle:ro" \
    --entrypoint "/bundle/$bundle_name" \
    "$container_image" \
    --connection "$connection_string"

  exit 0
fi

connection_host_lc="$(printf '%s' "$connection_host" | tr '[:upper:]' '[:lower:]')"

if [ -n "$connection_host_lc" ] && [ "$connection_host_lc" != "localhost" ] && [ "$connection_host_lc" != "127.0.0.1" ] && [ "$connection_host_lc" != "::1" ]; then
  echo "ConnectionStrings__rocketreps uses host '$connection_host', but no Podman network was provided." >&2
  echo "Pass --podman-network <name> so the EF bundle can run inside the same container network as the database." >&2
  exit 1
fi

"$bundle_path" --connection "$connection_string"
