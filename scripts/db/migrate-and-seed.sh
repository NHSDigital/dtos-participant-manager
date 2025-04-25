#!/bin/bash
set -e

/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$DATABASE_PASSWORD" -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'participant_database') BEGIN CREATE DATABASE participant_database; END"
/opt/mssql-tools/bin/sqlcmd -S db -d participant_database -U sa -P "$DATABASE_PASSWORD" -i /scripts/migration.sql
/opt/mssql-tools/bin/sqlcmd -S db -d participant_database -U sa -P "$DATABASE_PASSWORD" -i /scripts/db/InitialTestData.sql
