import { EligibilityResponse, PathwayResponse } from "@/app/types";
import { logger } from "@/app/lib/logger";
import type { Session } from "next-auth";

export async function fetchPatientScreeningEligibility(
  accessToken: string
): Promise<EligibilityResponse> {
  const correlationId = crypto.randomUUID();

  try {
    const url = `${process.env.EXPERIENCE_API_URL}/api/eligibility`;
    logger.info({ url, correlationId }, "Making eligibility API request");
    const response = await fetch(url, {
      method: "GET",
      headers: {
        Authorization: "Bearer " + accessToken,
        "X-Correlation-ID": correlationId,
      },
    });

    if (!response.ok) {
      logger.error(
        { url, correlationId },
        `Failed to get eligibility data: ${response.statusText}`
      );
      throw new Error(
        `Error fetching patient screening eligibility data: ${response.statusText}`
      );
    }
    const data = await response.json();
    logger.info(
      { url, correlationId },
      `Successfully got eligibility API data`
    );
    return data;
  } catch (error) {
    logger.error(
      { correlationId },
      `Failed to get eligibility data: ${
        error instanceof Error ? error.message : "Unknown error"
      }`
    );
    throw error;
  }
}

export async function fetchPathwayEnrolment(
  session: Session,
  enrolmentId: string
): Promise<PathwayResponse> {
  const correlationId = crypto.randomUUID();

  try {
    const url = `${process.env.EXPERIENCE_API_URL}/api/pathwayenrolments/${enrolmentId}`;
    logger.info({ url, correlationId }, "Making pathway API request");
    logger.info(`WARREN userId: ${session.user?.id}`);
    const response = await fetch(url, {
      method: "GET",
      headers: {
        Authorization: "Bearer " + session,
        "X-Correlation-ID": correlationId,
      },
    });

    if (!response.ok) {
      logger.error(
        { url, correlationId },
        `Failed to get pathway data: ${response.statusText}`
      );
      throw new Error(
        `Error fetching pathway enrolment data: ${response.statusText}`
      );
    }

    const data = await response.json();
    logger.info({ url, correlationId }, `Successfully got pathway API data`);
    return data;
  } catch (error) {
    logger.error(
      { correlationId },
      `Failed to get pathway data: ${
        error instanceof Error ? error.message : "Unknown error"
      }`
    );
    throw error;
  }
}
