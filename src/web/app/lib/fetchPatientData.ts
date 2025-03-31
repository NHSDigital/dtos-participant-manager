import { EligibilityResponse, PathwayResponse } from "@/app/types";
import { logger } from "@/app/lib/logger";
import type { Session } from "next-auth";

export async function fetchPatientScreeningEligibility(
  session: Session
): Promise<EligibilityResponse> {
  const correlationId = crypto.randomUUID();

  try {
    const url = `${process.env.EXPERIENCE_API_URL}/api/participants/${session.user?.participantId}/eligibility`;
    logger.info({ url, correlationId }, "Making eligibility API request");
    const response = await fetch(url, {
      method: "GET",
      headers: {
        Authorization: "Bearer " + session.user?.accessToken,
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

export async function fetchPathwayTypeEnrolment(
  session: Session,
  enrolmentId: string
): Promise<PathwayResponse> {
  const correlationId = crypto.randomUUID();

  try {
    const participantId = session.user?.participantId;
    const url = `${process.env.EXPERIENCE_API_URL}/api/participants/${participantId}/pathwaytypeenrolments/${enrolmentId}`;
    logger.info({ url, correlationId }, "Making pathway API request");
    logger.info(`session.user.participantId: ${session.user?.participantId}`);
    const response = await fetch(url, {
      method: "GET",
      headers: {
        Authorization: "Bearer " + session.user?.accessToken,
        "X-Correlation-ID": correlationId,
      },
    });

    if (!response.ok) {
      logger.error(
        { url, correlationId },
        `Failed to get pathway data: ${response.statusText}`
      );
      throw new Error(
        `Error fetching pathway type enrolment data: ${response.statusText}`
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

export async function fetchParticipantId(
  accessToken: string
): Promise<string> {
  const correlationId = crypto.randomUUID();

  try {
    const url = `${process.env.EXPERIENCE_API_URL}/api/participants/me/id`;
    logger.info({ url, correlationId }, "Making get participant id API request");
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
        `Failed to get participant id: ${response.statusText}`
      );
      throw new Error(
        `Error fetching participant id: ${response.statusText}`
      );
    }

    const participantId = await response.text();
    logger.info({ url, correlationId }, `Successfully got participant id from API`);
    return participantId;
  } catch (error) {
    logger.error(
      { correlationId },
      `Failed to get participant id: ${
        error instanceof Error ? error.message : "Unknown error"
      }`
    );
    throw error;
  }
}
