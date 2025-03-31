import {
  fetchPatientScreeningEligibility,
  fetchPathwayTypeEnrolment,
  fetchParticipantId,
} from "@/app/lib/fetchPatientData";
import { logger } from "@/app/lib/logger";

// Mock dependencies
jest.mock("./logger");
global.fetch = jest.fn();

// Mock the Web Crypto API
const mockCrypto = {
  randomUUID: jest.fn(() => "test-correlation-id"),
  subtle: {} as SubtleCrypto,
  getRandomValues: () => new Uint8Array(),
} as Crypto;

Object.defineProperty(global, "crypto", {
  value: mockCrypto,
  writable: true,
});

describe("fetchPatientData", () => {
  const mockSession = {
    user: {
      accessToken: "test-token",
      participantId: "test-participant-id",
    },
    expires: "2024-01-01T00:00:00.000Z",
  };

  const correlationId = "test-correlation-id";

  beforeEach(() => {
    jest.clearAllMocks();
    (global.fetch as jest.Mock).mockClear();
    process.env.EXPERIENCE_API_URL = "https://api.test";
    (mockCrypto.randomUUID as jest.Mock).mockReturnValue(correlationId);
  });

  describe("fetchPatientScreeningEligibility", () => {
    const mockEligibilityData = {
      bowelScreening: { eligible: true },
      breastScreening: { eligible: false },
    };

    it("returns eligibility data when fetch is successful", async () => {
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockEligibilityData),
      });

      const result = await fetchPatientScreeningEligibility(mockSession);

      expect(result).toEqual(mockEligibilityData);
      expect(global.fetch).toHaveBeenCalledWith(
        "https://api.test/api/participants/test-participant-id/eligibility",
        {
          method: "GET",
          headers: {
            Authorization: "Bearer test-token",
            "X-Correlation-ID": correlationId,
          },
        }
      );
      expect(logger.info).toHaveBeenCalledWith(
        expect.objectContaining({ correlationId }),
        "Successfully got eligibility API data"
      );
    });

    it("throws error when API response is not ok", async () => {
      const errorMessage = "Not Found";
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: false,
        statusText: errorMessage,
      });

      await expect(
        fetchPatientScreeningEligibility(mockSession)
      ).rejects.toThrow(
        `Error fetching patient screening eligibility data: ${errorMessage}`
      );
      expect(logger.error).toHaveBeenCalledWith(
        expect.objectContaining({ correlationId }),
        `Failed to get eligibility data: ${errorMessage}`
      );
    });

    it("handles network errors", async () => {
      const networkError = new Error("Network error");
      (global.fetch as jest.Mock).mockRejectedValueOnce(networkError);

      await expect(
        fetchPatientScreeningEligibility(mockSession)
      ).rejects.toThrow(networkError);
      expect(logger.error).toHaveBeenCalledWith(
        expect.objectContaining({ correlationId }),
        "Failed to get eligibility data: Network error"
      );
    });
  });

  describe("fetchPathwayTypeEnrolment", () => {
    const mockEnrolmentId = "test-enrolment";
    const mockPathwayData = {
      id: mockEnrolmentId,
      status: "active",
    };

    it("returns pathway data when fetch is successful", async () => {
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockPathwayData),
      });

      const result = await fetchPathwayTypeEnrolment(mockSession, mockEnrolmentId);

      expect(result).toEqual(mockPathwayData);
      expect(global.fetch).toHaveBeenCalledWith(
        `https://api.test/api/participants/test-participant-id/pathwaytypeenrolments/${mockEnrolmentId}`,
        {
          method: "GET",
          headers: {
            Authorization: "Bearer test-token",
            "X-Correlation-ID": correlationId,
          },
        }
      );
      expect(logger.info).toHaveBeenCalledWith(
        expect.objectContaining({ correlationId }),
        "Successfully got pathway API data"
      );
    });

    it("throws error when API response is not ok", async () => {
      const errorMessage = "Not Found";
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: false,
        statusText: errorMessage,
      });

      await expect(
        fetchPathwayTypeEnrolment(mockSession, mockEnrolmentId)
      ).rejects.toThrow(
        `Error fetching pathway type enrolment data: ${errorMessage}`
      );
    });
  });

  describe("fetchParticipantId", () => {
    const mockParticipantId = "test-participant-id";

    it("returns participant ID when fetch is successful", async () => {
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        text: () => Promise.resolve(mockParticipantId),
      });

      const result = await fetchParticipantId("test-token");

      expect(result).toBe(mockParticipantId);
      expect(global.fetch).toHaveBeenCalledWith(
        "https://api.test/api/participants/me/id",
        {
          method: "GET",
          headers: {
            Authorization: "Bearer test-token",
            "X-Correlation-ID": correlationId,
          },
        }
      );
      expect(logger.info).toHaveBeenCalledWith(
        expect.objectContaining({ correlationId }),
        "Successfully got participant id from API"
      );
    });

    it("throws error when API response is not ok", async () => {
      const errorMessage = "Unauthorized";
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: false,
        statusText: errorMessage,
      });

      await expect(fetchParticipantId("test-token")).rejects.toThrow(
        `Error fetching participant id: ${errorMessage}`
      );
    });
  });
});
