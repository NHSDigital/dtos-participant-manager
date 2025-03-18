import { getEligibility } from "@/app/lib/getEligibility";
import { fetchPatientScreeningEligibility } from "@/app/lib/fetchPatientData";
import { logger } from "@/app/lib/logger";

// Mock dependencies
jest.mock("./fetchPatientData");
jest.mock("./logger");

describe("getEligibility", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("returns null when no session is provided", async () => {
    const result = await getEligibility(null);

    expect(result).toBeNull();
    expect(logger.warn).toHaveBeenCalledWith(
      "No access token found for eligibility check"
    );
    expect(fetchPatientScreeningEligibility).not.toHaveBeenCalled();
  });

  it("returns null when session has no access token", async () => {
    const session = { user: {} };

    const result = await getEligibility(session);

    expect(result).toBeNull();
    expect(logger.warn).toHaveBeenCalledWith(
      "No access token found for eligibility check"
    );
    expect(fetchPatientScreeningEligibility).not.toHaveBeenCalled();
  });

  it("returns eligibility data when fetch is successful", async () => {
    const mockEligibilityData = {
      bowelScreening: { eligible: true },
      breastScreening: { eligible: false },
      cervicalScreening: { eligible: true },
    };

    const session = {
      user: {
        accessToken: "test-token",
      },
    };

    (fetchPatientScreeningEligibility as jest.Mock).mockResolvedValue(
      mockEligibilityData
    );

    const result = await getEligibility(session);

    expect(result).toEqual(mockEligibilityData);
    expect(fetchPatientScreeningEligibility).toHaveBeenCalledWith("test-token");
    expect(logger.warn).not.toHaveBeenCalled();
    expect(logger.error).not.toHaveBeenCalled();
  });

  it("returns null and logs error when fetch fails", async () => {
    const error = new Error("Fetch failed");
    const session = {
      user: {
        accessToken: "test-token",
      },
    };

    (fetchPatientScreeningEligibility as jest.Mock).mockRejectedValue(error);

    const result = await getEligibility(session);

    expect(result).toBeNull();
    expect(fetchPatientScreeningEligibility).toHaveBeenCalledWith("test-token");
    expect(logger.error).toHaveBeenCalledWith(
      "Failed to get eligibility data:",
      error
    );
  });
});
