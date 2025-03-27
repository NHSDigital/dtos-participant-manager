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
    // TODO: Add test data setup SQL
    const setupSql = `
      -- Add your test data setup SQL here
    `;
    await this.executeSql(setupSql);
  }

  static async cleanupTestData() {
    // TODO: Add test data cleanup SQL
    const cleanupSql = `
      -- Add your test data cleanup SQL here
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
