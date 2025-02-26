import pino, { Logger } from "pino";
import { registerOpenTelemetry } from "@/app/lib/registerTelemetry";

// Initialize OpenTelemetry
registerOpenTelemetry();

export const logger: Logger =
  process.env["NODE_ENV"] === "production"
    ? // JSON in production
      pino({ level: "warn" })
    : // Pretty print in development
      pino({
        transport: {
          targets: [
            {
              target: "pino-pretty",
              level: "debug",
              options: {
                colorize: true,
                translateTime: "SYS:standard",
              },
            },
          ],
        },
        level: "debug",
      });
