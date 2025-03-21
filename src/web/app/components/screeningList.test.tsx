import { render, screen } from "@testing-library/react";
import "@testing-library/jest-dom";
import ScreeningList from "@/app/components/screeningList";
import { createUrlSlug } from "@/app/lib/utils";

// Mock dependencies
jest.mock("../lib/utils", () => ({
  createUrlSlug: jest.fn(),
}));

describe("ScreeningList", () => {
  beforeEach(() => {
    (createUrlSlug as jest.Mock).mockReset();
  });

  it("displays message when no screening assignments", () => {
    render(<ScreeningList eligibility={[]} />);

    expect(
      screen.getByText("You have no screening assignments.")
    ).toBeInTheDocument();
  });

  it("displays message when eligibility is null", () => {
    render(<ScreeningList eligibility={null} />);

    expect(
      screen.getByText("You have no screening assignments.")
    ).toBeInTheDocument();
  });

  it("renders cards for each eligible screening", () => {
    const mockEligibility = [
      { screeningName: "Bowel Screening", enrolmentId: "123" },
      { screeningName: "Breast Screening", enrolmentId: "456" },
    ];

    (createUrlSlug as jest.Mock)
      .mockReturnValueOnce("bowel-screening")
      .mockReturnValueOnce("breast-screening");

    render(<ScreeningList eligibility={mockEligibility} />);

    // Check each card is rendered
    mockEligibility.forEach((item) => {
      expect(
        screen.getByRole("heading", {
          name: item.screeningName,
          level: 2,
        })
      ).toBeInTheDocument();
    });
  });

  it("creates correct URLs for screening cards", () => {
    const mockEligibility = [
      {
        screeningName: "Bowel Screening",
        enrolmentId: "123",
      },
    ];

    (createUrlSlug as jest.Mock).mockReturnValue("bowel-screening");

    render(<ScreeningList eligibility={mockEligibility} />);

    const link = screen.getByRole("link", { name: "Bowel Screening" });
    expect(link).toHaveAttribute("href", "bowel-screening/123");
    expect(createUrlSlug).toHaveBeenCalledWith("Bowel Screening");
  });

  it("handles multiple screening types", () => {
    const mockEligibility = [
      { screeningName: "Bowel Screening", enrolmentId: "123" },
      { screeningName: "Breast Screening", enrolmentId: "456" },
      { screeningName: "Cervical Screening", enrolmentId: "789" },
    ];

    (createUrlSlug as jest.Mock).mockImplementation((name) =>
      name.toLowerCase().replace(" ", "-")
    );

    render(<ScreeningList eligibility={mockEligibility} />);

    expect(screen.getAllByRole("link")).toHaveLength(3);
    expect(createUrlSlug).toHaveBeenCalledTimes(3);
  });
});
