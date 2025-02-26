const { NodeSDK, tracing, logs, api } = require ("@opentelemetry/sdk-node");
const { useAzureMonitor, AzureMonitorOpenTelemetryOptions } = require("@azure/monitor-opentelemetry");
import { PinoInstrumentation } from "@opentelemetry/instrumentation-pino";

export function registerOpenTelemetry() {

  const options: AzureMonitorOpenTelemetryOptions = {
    azureMonitorExporterOptions: {
      connectionString: "<your connection string>"
    }
  };

  useAzureMonitor(options);

  const sdk = new NodeSDK({
    logRecordProcessor: new logs.SimpleLogRecordProcessor(
      new logs.ConsoleLogRecordExporter(),
    ),
    instrumentations: [
      new PinoInstrumentation({
        logKeys: {
          traceId: "traceId",
          spanId: "spanId",
          traceFlags: "traceFlags",
        },
        logHook: (_span, record) => {
          record["resource.service.name"] = "frontend-app";
        },
      }),
    ],
  });

  sdk.start();

  process.on("SIGTERM", () => {
    sdk
      .shutdown()
      .then(() => console.log("SDK shut down successfully"))
      .catch((error: any) => console.log("Error shutting down SDK", error))
      .finally(() => process.exit(0));
  });
}
