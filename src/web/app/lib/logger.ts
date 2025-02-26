import pino, { Logger } from "pino";
//import { TelemetryClient } from 'applicationinsights';

import type { SpanExporter } from '@opentelemetry/sdk-trace-base';
import { registerOTel } from '@vercel/otel';

export async function register() {
  let traceExporter: SpanExporter | undefined;

  if (process.env.NEXT_RUNTIME === 'nodejs') {
    const { AzureMonitorTraceExporter } = await import('@azure/monitor-opentelemetry-exporter');
    traceExporter = new AzureMonitorTraceExporter({
      connectionString: process.env.APP_INSIGHTS_INSTRUMENTATION_KEY,
      // you can read from ENV if you prefer to
      // connectionString: process.env.APP_INSIGHTS_CONNECTION_STRING,
    });
  }

  registerOTel({ serviceName: 'your-project-name', traceExporter });
}

//const appInsights = new TelemetryClient(process.env.TESTVARIABLE);

export const logger: Logger =
  process.env["NODE_ENV"] === "production"
    ? // JSON in production
      pino({ level: "warn" })
    : // Pretty print in development
      pino({
        transport: {
          targets: [
            {
              target: 'pino-pretty',
              level: 'debug',
              options: {
                colorize: true,
                translateTime: 'SYS:standard'
              }
            }
          ]
        },
        level: "debug",
      });
