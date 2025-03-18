import {
  formatNhsNumber,
  formatDate,
  formatCompactDate,
  createUrlSlug,
} from "@/app/lib/utils";

describe("formatNhsNumber", () => {
  it("should format the NHS number as XXX XXX XXXX", () => {
    const input = "1234567890";
    const expectedOutput = "123 456 7890";
    expect(formatNhsNumber(input)).toBe(expectedOutput);
  });

  it("should return the input if it is not a valid NHS number", () => {
    const input = "12345";
    const expectedOutput = "12345";
    expect(formatNhsNumber(input)).toBe(expectedOutput);
  });
});

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

describe("formatCompactDate", () => {
  it("should format the date as 26 February 1993", () => {
    const input = "19930226";
    const expectedOutput = "26 February 1993";
    expect(formatCompactDate(input)).toBe(expectedOutput);
  });
});

describe("createUrlSlug", () => {
  it("converts string to url friendly slug", () => {
    expect(createUrlSlug("Breast Screening")).toBe("breast-screening");
    expect(createUrlSlug("  Multiple   Spaces  ")).toBe("multiple-spaces");
  });
});
