# Detect whether to use podman or docker
# DOCKER := $(shell command -v podman >/dev/null 2>&1 && echo podman || echo docker)
# Load environment variables from .env if it exists
ifneq (,$(wildcard .env))
    include .env
    export
endif


# 🔹 Ensure multi-line secrets are handled properly
ifeq ($(OS),Windows_NT)
    AUTH_NHSLOGIN_PRIVATE_KEY := $(shell type $(AUTH_NHSLOGIN_PRIVATE_KEY_FILE))
		DOCKER := wsl docker
else
    AUTH_NHSLOGIN_PRIVATE_KEY := $(shell cat $(AUTH_NHSLOGIN_PRIVATE_KEY_FILE))
		DOCKER := docker
endif

# Define directories
WEB_DIR=src/web
API1_DIR=src/api/ParticipantManager.API
API2_DIR=src/api/ParticipantManager.Experience.API

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

.PHONY: all web api1 api2 db stop-db stop

# Default command (runs everything)
all: db start wait

start:
ifeq ($(OS), Windows_NT)
# Start Next.js (Windows)
	start /B cmd /c "cd $(WEB_DIR) && npm run dev:secure" & echo $$! > web.pid

# Start API 1 (Windows)
	start /B cmd /c "cd $(API1_DIR) && dotnet run" & echo $$! > api1.pid

# Start API 2 (Windows)
	start /B cmd /c "cd $(API2_DIR) && dotnet run" & echo $$! > api2.pid
else
# Start Next.js (macOS/Linux)
	cd $(WEB_DIR) && npm run dev:secure & echo $$! > web.pid

# Start API 1 (macOS/Linux)
	cd $(API1_DIR) && dotnet run & echo $$! > api1.pid

# Start API 2 (macOS/Linux)
	cd $(API2_DIR) && dotnet run & echo $$! > api2.pid
endif



# Start the Next.js frontend
web:
	@echo "Starting Next.js..."
	cd $(WEB_DIR) && npm run dev:secure &
	echo $$! > web.pid

# Start API1 (Participant Manager)
api1:
	@echo "Starting ParticipantManager API..."
	cd $(API1_DIR) && dotnet run &
	echo $$! > api1.pid

# Start API2 (Experience API)
api2:
	@echo "Starting Experience API..."
	cd $(API2_DIR) && dotnet run &
	echo $$! > api2.pid

# Start SQL Server in Podman/Docker
db:
	@echo "Starting SQL Server using $(DOCKER)..."
	$(DOCKER) run -d --rm --name $(SQL_CONTAINER_NAME) -p $(SQL_PORT):1433 $(SQL_ENV_VARS) $(SQL_IMAGE)
# cd $(API1_DIR) && dotnet ef database update  &

# Stop SQL Server
stop-db:
	@echo "Stopping SQL Server..."
	$(DOCKER) stop $(SQL_CONTAINER_NAME)

# Stop all running services
stop:
ifeq ($(OS), Windows_NT)
	# Stop processes using port numbers on Windows
	@for %%P in ($(WEB_PORT) $(API_PORT) $(EXPERIENCE_PORT)) do (
		for /f "tokens=5" %%T in ('netstat -ano ^| findstr :%%P') do (
			taskkill /F /PID %%T 2>nul
		)
	)
else
	# Stop processes using port numbers on macOS/Linux
	@for port in $(WEB_PORT) $(API_PORT) $(EXPERIENCE_PORT); do \
			PID=$$(lsof -ti :$$port); \
			echo "PID: $$PID"; \
			if [ -n "$$PID" ]; then \
					kill -9 $$PID 2>/dev/null || true; \
			fi; \
	done
endif

	# Stop the SQL Server container
	@$(DOCKER) stop $(SQL_CONTAINER_NAME) 2>/dev/null || true

	@echo "Cleanup completed."

# ===========================
# :white_tick: Wait Until Processes are Stopped
# ===========================
wait:
	@echo "Press CTRL+C to stop services..."
	ifeq ($(OS), Windows_NT)
		@timeout /t -1
	else
		@trap 'make stop' INT TERM
		@wait $(shell cat web.pid api1.pid api2.pid) || true
	endif

debug:
	@echo $(API1_DIR)
	@echo $(API2_DIR)
	@echo $(AUTH_NHSLOGIN_PRIVATE_KEY_FILE)
	@echo $(AUTH_NHSLOGIN_PRIVATE_KEY)
	@echo $(API_URL)
	@echo $(NEXTAUTH_URL)
