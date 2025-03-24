import { register } from "./instrumentation";

// Mock dependencies
jest.mock("@azure/monitor-opentelemetry", () => ({
  useAzureMonitor: jest.fn(),
}));

jest.mock("@opentelemetry/sdk-node", () => ({
  NodeSDK: jest.fn().mockImplementation(() => ({
    shutdown: jest.fn().mockResolvedValue(undefined),
  })),
}));

// Add proper mock constructors
jest.mock("@opentelemetry/instrumentation-fetch", () => ({
  FetchInstrumentation: jest.fn().mockImplementation(() => ({
    setConfig: jest.fn(),
    enable: jest.fn(),
    disable: jest.fn(),
  })),
}));

jest.mock("@opentelemetry/instrumentation-http", () => ({
  HttpInstrumentation: jest.fn().mockImplementation(() => ({
    setConfig: jest.fn(),
    enable: jest.fn(),
    disable: jest.fn(),
  })),
}));

jest.mock("@opentelemetry/instrumentation-pino", () => ({
  PinoInstrumentation: jest.fn().mockImplementation(() => ({
    setConfig: jest.fn(),
    enable: jest.fn(),
    disable: jest.fn(),
  })),
}));

describe("Instrumentation", () => {
  const originalEnv = process.env;
  let consoleLogSpy: jest.SpyInstance;

  beforeEach(() => {
    process.env = { ...originalEnv };
    process.env.NEXT_RUNTIME = "nodejs";
    consoleLogSpy = jest.spyOn(console, "log").mockImplementation();
  });

  afterEach(() => {
    process.env = originalEnv;
    jest.clearAllMocks();
    consoleLogSpy.mockRestore();
  });

  it("initializes Azure Monitor when connection string is set", async () => {
    process.env.APPLICATIONINSIGHTS_CONNECTION_STRING =
      "test-connection-string";

    await register();

    const { useAzureMonitor } = require("@azure/monitor-opentelemetry");
    expect(useAzureMonitor).toHaveBeenCalledWith({
      azureMonitorExporterOptions: {
        connectionString: "test-connection-string",
      },
    });
  });

  it("skips initialization when not in Node.js runtime", async () => {
    process.env.NEXT_RUNTIME = "edge";
    process.env.APPLICATIONINSIGHTS_CONNECTION_STRING =
      "test-connection-string";

    await register();

    const { useAzureMonitor } = require("@azure/monitor-opentelemetry");
    expect(useAzureMonitor).not.toHaveBeenCalled();
  });
});
