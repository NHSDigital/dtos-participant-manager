import { logger } from "@/app/lib/logger";

export async function register() {
  if (process.env.NEXT_RUNTIME !== "nodejs") return;

  const { useAzureMonitor } = await import("@azure/monitor-opentelemetry");
  const { NodeSDK } = await import("@opentelemetry/sdk-node");
  const { PinoInstrumentation } = await import(
    "@opentelemetry/instrumentation-pino"
  );
  const { HttpInstrumentation } = await import(
    "@opentelemetry/instrumentation-http"
  );
  const { FetchInstrumentation } = await import(
    "@opentelemetry/instrumentation-fetch"
  );

  const sdk = new NodeSDK({
    instrumentations: [
      new HttpInstrumentation(),
      new FetchInstrumentation(),
      new PinoInstrumentation({
        logKeys: {
          traceId: "traceId",
          spanId: "spanId",
          traceFlags: "traceFlags",
        },
      }),
    ],
  });

  process.on("SIGTERM", () => {
    sdk
      .shutdown()
      .then(() => console.log("SDK shut down successfully"))
      .catch((error) => console.log("Error shutting down SDK", error))
      .finally(() => process.exit(0));
  });

  const options = {
    azureMonitorExporterOptions: {
      connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING,
    },
  };

  if (!options.azureMonitorExporterOptions.connectionString) {
    logger.warn(
      "APPLICATIONINSIGHTS_CONNECTION_STRING environment variable is not set. Skipping Azure Monitor initialization."
    );
    return;
  }

  useAzureMonitor(options);
}
