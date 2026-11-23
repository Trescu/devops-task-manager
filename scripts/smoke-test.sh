#!/usr/bin/env bash
set -euo pipefail
BASE_URL="${1:-http://localhost:8080}"
echo "Checking $BASE_URL"
curl --fail --silent --show-error "$BASE_URL/health" | grep -q '"status":"healthy"'
curl --fail --silent --show-error "$BASE_URL/" | grep -q 'DevOps Task Manager'
response=$(curl --fail --silent --show-error -X POST "$BASE_URL/api/tasks" -H 'Content-Type: application/json' -d '{"title":"Smoke task","description":"HTTP verification","status":"todo","priority":"high"}')
id=$(printf '%s' "$response" | python3 -c 'import json,sys; print(json.load(sys.stdin)["id"])')
test -n "$id"
curl --fail --silent --show-error "$BASE_URL/api/tasks/$id" | grep -q 'Smoke task'
curl --fail --silent --show-error -X PUT "$BASE_URL/api/tasks/$id" -H 'Content-Type: application/json' -d '{"title":"Smoke updated","description":"","status":"done","priority":"medium"}' | grep -q '"status":"done"'
curl --fail --silent --show-error "$BASE_URL/api/tasks/$id" | grep -q 'Smoke updated'
status=$(curl --silent --output /dev/null --write-out '%{http_code}' -X DELETE "$BASE_URL/api/tasks/$id")
test "$status" = 204
status=$(curl --silent --output /dev/null --write-out '%{http_code}' "$BASE_URL/api/tasks/$id")
test "$status" = 404
echo "PASS: health, frontend, create, read, update, delete"
