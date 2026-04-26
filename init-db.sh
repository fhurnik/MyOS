#!/bin/bash

echo "Waiting for SQL Server to start..."

sleep 20

echo "Creating database: $DB_NAME"

/opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "$SA_PASSWORD" \
  -C \
  -v DB_NAME="$DB_NAME" \
  -i /docker-entrypoint-initdb.d/init.sql

echo "Database initialization completed."