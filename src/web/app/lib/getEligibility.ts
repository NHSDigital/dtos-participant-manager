import type { Session } from "next-auth";
import type { EligibilityResponse } from "@/app/types";
import { fetchPatientScreeningEligibility } from "@/app/lib/fetchPatientData";
import { logger } from "@/app/lib/logger";

export async function getEligibility(
  session: Session | null
): Promise<EligibilityResponse | null> {
  if (!session?.user?.accessToken) {
    logger.warn("No access token found for eligibility check");
    return null;
  }

  try {
    return await fetchPatientScreeningEligibility(session.user.accessToken);
  } catch (error) {
    logger.error("Failed to get eligibility data:", error);
    return null;
  }
}
