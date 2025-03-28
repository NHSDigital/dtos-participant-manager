import { exec } from "child_process";
import { promisify } from "util";
import sql from "mssql";
import dotenv from "dotenv";
import path from "path";

const execAsync = promisify(exec);

// Load environment variables from root .env file
dotenv.config({ path: path.resolve(__dirname, "../../../../../.env") });

export class TestUtils {
  private static pool: sql.ConnectionPool;

  static async initializeDatabase() {
    const config: sql.config = {
      user: process.env.DATABASE_USER || "sa",
      password: process.env.DATABASE_PASSWORD,
      server: process.env.DATABASE_HOST || "localhost",
      database: process.env.DATABASE_NAME || "participant_manager",
      port: 1433,
      options: {
        encrypt: false,
        trustServerCertificate: true,
      },
    };

    try {
      this.pool = await sql.connect(config);
      console.log("Connected to SQL Server");
    } catch (error) {
      console.error("Failed to connect to SQL Server:", error);
      throw error;
    }
  }

  static async executeSql(sqlString: string) {
    try {
      const result = await this.pool.request().query(sqlString);
      return result;
    } catch (error) {
      console.error("Failed to execute SQL:", error);
      throw error;
    }
  }

  static async setupTestData() {
    const setupSql = `
      -- Create test user
      INSERT INTO Users (Id, Email, FirstName, LastName, CreatedAt, UpdatedAt)
      VALUES (
        'test-user-id',
        'test.user@example.com',
        'Test',
        'User',
        GETUTCDATE(),
        GETUTCDATE()
      );

      -- Create test enrolment
      INSERT INTO Enrolments (Id, UserId, Status, CreatedAt, UpdatedAt)
      VALUES (
        'test-enrolment-id',
        'test-user-id',
        'Active',
        GETUTCDATE(),
        GETUTCDATE()
      );

      -- Add any additional test data as needed
    `;
    await this.executeSql(setupSql);
  }

  static async cleanupTestData() {
    const cleanupSql = `
      -- Clean up test data in reverse order of creation
      DELETE FROM Enrolments WHERE Id = 'test-enrolment-id';
      DELETE FROM Users WHERE Id = 'test-user-id';
    `;
    await this.executeSql(cleanupSql);
  }

  static async mockOIDCResponse() {
    // TODO: Implement OIDC mock response
    // This could involve setting up a mock server or intercepting requests
  }

  static async closeDatabase() {
    if (this.pool) {
      await this.pool.close();
    }
  }
}
