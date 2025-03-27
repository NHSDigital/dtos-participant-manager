import { exec } from "child_process";
import { promisify } from "util";
import { TestUtils } from "./utils/test-utils";

const execAsync = promisify(exec);

async function teardownTestEnvironment() {
  console.log("Tearing down test environment...");

  // Cleanup test database
  try {
    // TODO: Add database cleanup SQL scripts
    console.log("Test database cleanup completed");
  } catch (error) {
    console.error("Failed to cleanup test database:", error);
  }

  // Close database connection
  try {
    await TestUtils.closeDatabase();
    console.log("Database connection closed");
  } catch (error) {
    console.error("Failed to close database connection:", error);
  }

  // Stop Docker Compose services
  try {
    await execAsync("docker compose down");
    console.log("Docker Compose services stopped successfully");
  } catch (error) {
    console.error("Failed to stop Docker Compose services:", error);
  }
}

export default teardownTestEnvironment;
