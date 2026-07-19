#!/usr/bin/env bash
set -Eeuo pipefail

root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

api_project="$root_dir/src/HomeBudget.Api/HomeBudget.Api.csproj"
web_project="$root_dir/src/HomeBudget.Web/HomeBudget.Web.csproj"

api_url="${HOMEBUDGET_API_URL:-http://localhost:5095}"
web_url="${HOMEBUDGET_WEB_URL:-http://localhost:5179}"

pids=()

cleanup() {
    local exit_code=$?

    trap - EXIT INT TERM

    if [ "${#pids[@]}" -gt 0 ]; then
        echo
        echo "Stopping HomeBudget dev services..."
        kill "${pids[@]}" 2>/dev/null || true
        wait "${pids[@]}" 2>/dev/null || true
    fi

    exit "$exit_code"
}

start_service() {
    local name="$1"
    local project="$2"
    local url="$3"

    echo "Starting $name on $url"

    (
        cd "$root_dir"
        ASPNETCORE_ENVIRONMENT=Development \
        ASPNETCORE_URLS="$url" \
        dotnet run --no-launch-profile --project "$project"
    ) &

    pids+=("$!")
}

trap cleanup EXIT INT TERM

start_service "HomeBudget.Api" "$api_project" "$api_url"
start_service "HomeBudget.Web" "$web_project" "$web_url"

echo
echo "HomeBudget.Api health: $api_url/health"
echo "HomeBudget.Web UI:      $web_url"
echo "Press Ctrl+C to stop both services."

while true; do
    for pid in "${pids[@]}"; do
        if ! kill -0 "$pid" 2>/dev/null; then
            wait "$pid"
            exit $?
        fi
    done

    sleep 1
done
