#!/bin/bash

echo "Waiting for SQL Server to be ready..."

until /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "$SA_PASSWORD" \
  -C \
  -Q "SELECT 1" > /dev/null 2>&1
do
  echo "SQL Server is not ready yet..."
  sleep 2
done

echo "SQL Server is ready."

/opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "$SA_PASSWORD" \
  -C \
  -v DB_NAME="$DB_NAME" \
  -i /docker-entrypoint-initdb.d/init.sql

echo "Database initialization completed."