import { formatDate, createUrlSlug } from "@/app/lib/utils";

describe("formatDate", () => {
  it("should format the date as Month YYYY", () => {
    const input = "2026-10-12T11:53:01.243";
    const expectedOutput = "October 2026";
    expect(formatDate(input)).toBe(expectedOutput);
  });

  it("should handle different months", () => {
    expect(formatDate("2024-01-05T10:00:00.000")).toBe("January 2024");
    expect(formatDate("2025-12-25T00:00:00.000")).toBe("December 2025");
  });
});

describe("createUrlSlug", () => {
  it("converts string to url friendly slug", () => {
    expect(createUrlSlug("Breast Screening")).toBe("breast-screening");
    expect(createUrlSlug("  Multiple   Spaces  ")).toBe("multiple-spaces");
  });
});
