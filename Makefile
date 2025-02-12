# Detect whether to use podman or docker
DOCKER := $(shell command -v podman >/dev/null 2>&1 && echo podman || echo docker)
# Load environment variables from .env if it exists
ifneq (,$(wildcard .env))
    include .env
    export
endif


# 🔹 Ensure multi-line secrets are handled properly
ifeq ($(OS),Windows_NT)
    AUTH_NHSLOGIN_PRIVATE_KEY := $(shell type $(AUTH_NHSLOGIN_PRIVATE_KEY_FILE))
else
    AUTH_NHSLOGIN_PRIVATE_KEY := $(shell cat $(AUTH_NHSLOGIN_PRIVATE_KEY_FILE))
endif


# Define directories
WEB_DIR=src/web
API1_DIR=src/api/ParticipantManager.API
API2_DIR=src/api/ExperienceAPI

# Define the database container
SQL_CONTAINER_NAME=participant_database_local
SQL_IMAGE=mcr.microsoft.com/mssql/server:latest
SQL_PORT=1433
SQL_ENV_VARS=-e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=$(DATABASE_PASSWORD)"


# 🔹 Store background process PIDs
PIDS=

# 🔹 Cleanup function to kill processes on exit
define cleanup
	@echo "Stopping all services..."
	@pkill -P $$ || true
	@kill -TERM $(PIDS) 2>/dev/null || true
	@$(DOCKER) stop $(SQL_CONTAINER_NAME) 2>/dev/null || true
	@echo "Cleanup completed."
endef

# Default command (runs everything)
all: db web api1 api2

# Start the Next.js frontend
web:
	@echo "Starting Next.js..."
	cd $(WEB_DIR) && npm run dev:secure &
	@PIDS+="$! "

# Start API1 (Participant Manager)
api1:
	@echo "Starting ParticipantManager API..."
	cd $(API1_DIR) && dotnet watch run &
	@PIDS+="$! "

# Start API2 (Experience API)
api2:
	@echo "Starting Experience API..."
	cd $(API2_DIR) && dotnet watch run &
	@PIDS+="$! "

# Start SQL Server in Podman/Docker
db:
	@echo "Starting SQL Server using $(DOCKER)..."
	$(DOCKER) run --rm --name $(SQL_CONTAINER_NAME) -p $(SQL_PORT):1433 $(SQL_ENV_VARS) -d $(SQL_IMAGE)
	@PIDS+="$! "

# Stop SQL Server
stop-db:
	@echo "Stopping SQL Server..."
	$(DOCKER) stop $(SQL_CONTAINER_NAME)

# Stop all running services
stop:
	@echo "Stopping all services..."
	@pkill -P $$ || true
	@kill -TERM $(PIDS) 2>/dev/null || true
	@$(DOCKER) stop $(SQL_CONTAINER_NAME) 2>/dev/null || true
	@echo "Cleanup completed."

.PHONY: all web api1 api2 db stop-db stop
