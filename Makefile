include scripts/init.mk
# Load environment variables from .env if it exists
ifneq (,$(wildcard .env))
	include .env
	export
endif

ifeq ($(OS),Windows_NT)
	DOCKER := wsl docker
else
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
DOCKER_RUN_ARGS = --rm --name $(SQL_CONTAINER_NAME) -p $(SQL_PORT):1433 $(SQL_ENV_VARS) $(SQL_IMAGE)

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
all: db db-migrations api1 api2 web

# Start the Next.js frontend
web:
	@echo "Starting Next.js..."
ifeq ($(OS), Windows_NT)
	cd "$(WEB_DIR)" && npm install && set PORT=$(WEB_PORT) && start /B npm run dev:secure
else
		cd $(WEB_DIR) && npm install && PORT=$(WEB_PORT) npm run dev:secure
endif

# Start API1 (Participant Manager)
api1:
	@echo "Starting ParticipantManager API..."
ifeq ($(OS), Windows_NT)
	@cd "$(API1_DIR)" && start /B dotnet watch run --port $(API_PORT)
else
		cd $(API1_DIR) && dotnet watch run --port $(API_PORT) &
endif

# Start API2 (Experience API)
api2:
	@echo "Starting Experience API..."
ifeq ($(OS), Windows_NT)
	@cd $(API2_DIR) && start /B dotnet watch run --port $(EXPERIENCE_PORT)"
else
		cd $(API2_DIR) && dotnet watch run --port $(EXPERIENCE_PORT) &
endif

# Start SQL Server in Podman/Docker
db:
	@echo "Starting SQL Server using $(DOCKER)..."
ifeq ($(OS), Windows_NT)
	@cmd /c start /B $(DOCKER) run $(DOCKER_RUN_ARGS)
else
	$(DOCKER) run -d $(DOCKER_RUN_ARGS)
endif

db-migrations: db
	echo "⏳ Waiting for SQL Server to be ready..."
ifeq ($(OS), Windows_NT)
	@timeout /t 10 /nobreak
	powershell -Command "while (-not (Test-NetConnection -ComputerName localhost -Port 1433 -WarningAction SilentlyContinue).TcpTestSucceeded) { Write-Host '⏳ Waiting for database to be reachable...'; Start-Sleep -Seconds 3 }"
	echo "✅ Database is ready!"
	echo "⚙️  Running database migrations..."
	cmd /c "cd $(API1_DIR) && set ParticipantManagerDatabaseConnectionString=$(ParticipantManagerDatabaseConnectionString) && dotnet ef database update"
else
	sleep 10
	until nc -z localhost 1433; do \
		echo "⏳ Waiting for database to be reachable..."; sleep 3; \
	done
	echo "✅ Database is ready!"
	echo "⚙️  Running database migrations..."
	cd $(API1_DIR) && ParticipantManagerDatabaseConnectionString="$(ParticipantManagerDatabaseConnectionString)" dotnet ef database update
endif
	echo "🌱 Seeding initial test data..."
	@sqlcmd -S $(DATABASE_HOST) -d $(DATABASE_NAME) -i $(INITIAL_SEED_SCRIPT) -U $(DATABASE_USER) -P $(DATABASE_PASSWORD)
	echo "✅ Migrations applied!"

# Stop SQL Server
stop-db:
	@echo "Stopping SQL Server..."
ifeq ($(OS), Windows_NT)
	-@ cmd /c $(DOCKER) stop $(SQL_CONTAINER_NAME)
else
	@$(DOCKER) stop $(SQL_CONTAINER_NAME)
endif

# Stop all running services
stop:

ifeq ($(OS), Windows_NT)
	@echo "Stopping dotnet watch windows processes..."
	@echo "Killing processes on ports $(WEB_PORT), $(API_PORT), and $(EXPERIENCE_PORT)..."
	@powershell -Command "if (Get-NetTCPConnection -LocalPort $(WEB_PORT) -ErrorAction SilentlyContinue) { Stop-Process -Id (Get-NetTCPConnection -LocalPort $(WEB_PORT)).OwningProcess -Force -ErrorAction SilentlyContinue }"
	@powershell -Command "if (Get-NetTCPConnection -LocalPort $(API_PORT) -ErrorAction SilentlyContinue) { Stop-Process -Id (Get-NetTCPConnection -LocalPort $(API_PORT)).OwningProcess -Force -ErrorAction SilentlyContinue }"
	@powershell -Command "if (Get-NetTCPConnection -LocalPort $(EXPERIENCE_PORT) -ErrorAction SilentlyContinue) { Stop-Process -Id (Get-NetTCPConnection -LocalPort $(EXPERIENCE_PORT)).OwningProcess -Force -ErrorAction SilentlyContinue }"
	@echo "Processes killed."

else

# Stop dotnet watch processes first
	@echo "Stopping dotnet watch processes..."
	@pkill -f "dotnet watch" || true
	@sleep 1  # Give it a moment to stop

# Stop processes using port numbers on macOS/Linux
	@for port in $(WEB_PORT) $(API_PORT) $(EXPERIENCE_PORT); do \
			PID=$$(lsof -ti :$$port); \
			echo "PID: $$PID"; \
			if [ -n "$$PID" ]; then \
				ACTUAL_CMD=$$(ps -o comm= -p $$PID); \
				if [ "$$ACTUAL_CMD" = "node" ] || [ "$$ACTUAL_CMD" = "func" ]; then \
					kill -9 $$PID 2>/dev/null || true; \
					echo "Stopped $$ACTUAL_CMD on port $$port"; \
				else \
					echo "Skipping $$ACTUAL_CMD (not expected)"; \
				fi \
			fi; \
	done
endif

# Stop the SQL Server container

ifeq ($(OS), Windows_NT)
	@$(DOCKER) stop $(SQL_CONTAINER_NAME) 2>nul || exit 0
else
	@$(DOCKER) stop $(SQL_CONTAINER_NAME) 2>/dev/null || true
endif

	@echo "Cleanup completed."

.PHONY: all web api1 api2 db stop-db stop
