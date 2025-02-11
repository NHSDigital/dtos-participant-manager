# This file is for you! Edit it to implement your own hooks (make targets) into
# the project as automated steps to be executed on locally and in the CD pipeline.

include scripts/init.mk

# ==============================================================================

# Example CI/CD targets are: dependencies, build, publish, deploy, clean, etc.

dependencies: # Install dependencies needed to build and test the project @Pipeline
	# TODO: Implement installation of your project dependencies

build: # Build the project artefact @Pipeline
	# TODO: Implement the artefact build step

publish: # Publish the project artefact @Pipeline
	# TODO: Implement the artefact publishing step

deploy: # Deploy the project artefact to the target environment @Pipeline
	# TODO: Implement the artefact deployment step

clean:: # Clean-up project resources (main) @Operations
	# TODO: Implement project resources clean-up step

config:: # Configure development environment (main) @Configuration
	# TODO: Use only 'make' targets that are specific to this project, e.g. you may not need to install Node.js
	make _install-dependencies

# ==============================================================================

${VERBOSE}.SILENT: \
	build \
	clean \
	config \
	dependencies \
	deploy \

NPM="C:/Program Files/nodejs/npm"
# Define directories
WEB_DIR=src/web
API1_DIR=src/api/ParticipantManager.API
API2_DIR=src/api/ParticipantManager.Experience.API
# Define the SQL Server container name
SQL_CONTAINER_NAME=participant_database
SQL_IMAGE=mcr.microsoft.com/mssql/server:latest
SQL_PORT=1433
SQL_ENV_VARS=-e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourSecurePassword!123"
# Default command (runs everything)
all: db web api1 api2
# Start the Next.js frontend
web:
	@echo "Starting Next.js..."
	cd $(WEB_DIR) && $(NPM) install && $(NPM) run dev:secure &
# Start API1 (Participant Manager)
api1:
	@echo "Starting ParticipantManager API..."
	cd $(API1_DIR) && dotnet watch run &
# Start API2 (Experience API)
api2:
	@echo "Starting Experience API..."
	cd $(API2_DIR) && dotnet watch run &
# Start SQL Server in Docker
db:
	@echo "Starting SQL Server..."
	podman run --rm --name $(SQL_CONTAINER_NAME) -p $(SQL_PORT):1433 $(SQL_ENV_VARS) -d $(SQL_IMAGE)
# Stop SQL Server
stop-db:
	@echo "Stopping SQL Server..."
	podman stop $(SQL_CONTAINER_NAME)
# Clean up background processes
stop:
	@echo "Stopping all services..."
	@pkill -f "npm run dev:secure" || true
	@pkill -f "dotnet watch run" || true
	@make stop-db
.PHONY: all web api1 api2 db stop-db stop
