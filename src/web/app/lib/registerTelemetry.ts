const { NodeSDK, tracing, logs, api } = require("@opentelemetry/sdk-node");
import { PinoInstrumentation } from "@opentelemetry/instrumentation-pino";

export function registerOpenTelemetry() {
  const sdk = new NodeSDK({
    spanProcessor: new tracing.SimpleSpanProcessor(
      new tracing.ConsoleSpanExporter()
    ),
    logRecordProcessor: new logs.SimpleLogRecordProcessor(
      new logs.ConsoleLogRecordExporter()
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
