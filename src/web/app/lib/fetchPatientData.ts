import { EligibilityResponse, PathwayResponse } from "@/app/types";

export async function fetchPatientScreeningEligibility(
  accessToken: string
): Promise<EligibilityResponse> {
  const traceId = crypto.randomUUID();

  try {
    const url = `${process.env.EXPERIENCE_API_URL}/api/eligibility`;
    const response = await fetch(url, {
      method: "GET",
      headers: {
        Authorization: "Bearer " + accessToken,
        traceId: traceId,
      },
    });

    if (!response.ok) {
      throw new Error(
        `Error fetching patient screening eligibility data: ${response.statusText}`
      );
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching patient screening eligibility data:", error);
    throw error;
  }
}

export async function fetchPathwayAssignment(
  accessToken: string,
  assignmentId: string
): Promise<PathwayResponse> {
  const traceId = crypto.randomUUID();

  try {
    const url = `${process.env.EXPERIENCE_API_URL}/api/pathwayassignments/${assignmentId}`;
    const response = await fetch(url, {
      method: "GET",
      headers: {
        Authorization: "Bearer " + accessToken,
        traceId: traceId,
      },
    });

    if (!response.ok) {
      throw new Error(
        `Error fetching pathway assignment data: ${response.statusText}`
      );
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching pathway assignment data:", error);
    throw error;
  }
}
