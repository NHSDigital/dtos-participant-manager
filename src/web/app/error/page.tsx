import type { Metadata } from "next";
import { logger } from "@/app/lib/logger";

export const metadata: Metadata = {
  title: `There’s a problem with the service - ${process.env.SERVICE_NAME} - NHS`,
};

export default async function Page(error: Error) {
  if (error) {
    logger.error("Application error:", {
      message: error.message,
      stack: error.stack,
      name: error.name,
    });
  }
  return (
    <main className="nhsuk-main-wrapper" id="maincontent" role="main">
      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-two-thirds">
          <h1>There’s a problem with the service</h1>
          <p>Try again later.</p>
        </div>
      </div>
    </main>
  );
}
