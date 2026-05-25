#!/usr/bin/env bash
set -euo pipefail

ENV_FILE="${1:-/opt/maintenance-management/deploy/backup.env}"

if [[ -f "$ENV_FILE" ]]; then
  set -a
  # shellcheck disable=SC1090
  source "$ENV_FILE"
  set +a
fi

: "${MYSQL_HOST:?MYSQL_HOST is required}"
: "${MYSQL_PORT:=3306}"
: "${MYSQL_DATABASE:?MYSQL_DATABASE is required}"
: "${MYSQL_USER:?MYSQL_USER is required}"
: "${MYSQL_PASSWORD:?MYSQL_PASSWORD is required}"

BACKUP_DIR="${BACKUP_DIR:-/opt/backups/maintenance-mysql}"
RETENTION_DAYS="${RETENTION_DAYS:-14}"
TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
OUTPUT_FILE="${BACKUP_DIR}/${MYSQL_DATABASE}-${TIMESTAMP}.sql.gz"

mkdir -p "$BACKUP_DIR"

MYSQL_PWD="$MYSQL_PASSWORD" mysqldump \
  --host="$MYSQL_HOST" \
  --port="$MYSQL_PORT" \
  --user="$MYSQL_USER" \
  --single-transaction \
  --quick \
  --routines \
  --events \
  --triggers \
  "$MYSQL_DATABASE" | gzip -c > "$OUTPUT_FILE"

find "$BACKUP_DIR" -type f -name "${MYSQL_DATABASE}-*.sql.gz" -mtime +"$RETENTION_DAYS" -delete

echo "Backup written: $OUTPUT_FILE"
