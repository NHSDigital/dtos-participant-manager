import { formatDate, createUrlSlug } from "@/app/lib/utils";

describe("formatDate", () => {
  it("should format the date as 26 February 1993", () => {
    const input = "1993-02-26T11:53:01.243";
    const expectedOutput = "26 February 1993";
    expect(formatDate(input)).toBe(expectedOutput);
  });
});

describe("createUrlSlug", () => {
  it("converts string to url friendly slug", () => {
    expect(createUrlSlug("Breast Screening")).toBe("breast-screening");
    expect(createUrlSlug("prostate cancer")).toBe("prostate-cancer");
  });
});
