import { render, screen } from "@testing-library/react";
import "@testing-library/jest-dom";
import InsetText from "@/app/components/insetText";
import { formatDate } from "@/app/lib/utils";

// Mock the utils module
jest.mock("@/app/lib/utils", () => ({
  formatDate: jest.fn(),
}));

describe("InsetText", () => {
  beforeEach(() => {
    (formatDate as jest.Mock).mockReset();
  });

  it("renders text content", () => {
    const text = "Test message";
    render(<InsetText text={text} />);

    expect(screen.getByText(/Test message/)).toBeInTheDocument();
  });

  it("includes visually hidden 'Information' text", () => {
    render(<InsetText text="Test" />);

    const hiddenText = screen.getByText("Information:");
    expect(hiddenText).toHaveClass("nhsuk-u-visually-hidden");
  });

  it("formats and displays date when provided", () => {
    const text = "Your next appointment is on";
    const date = "2024-03-15";
    const formattedDate = "March 2024";

    (formatDate as jest.Mock).mockReturnValue(formattedDate);

    render(<InsetText text={text} date={date} />);

    expect(screen.getByText(formattedDate)).toBeInTheDocument();
    expect(formatDate).toHaveBeenCalledWith(date);
  });

  it("renders paragraph with text and optional date", () => {
    const text = "Next available appointment:";
    const date = "2024-03-15";
    const formattedDate = "March 2024";

    (formatDate as jest.Mock).mockReturnValue(formattedDate);

    render(<InsetText text={text} date={date} />);

    const paragraph = screen.getByText(/Next available appointment:/);
    expect(paragraph.tagName).toBe("P");
    expect(paragraph).toContainElement(screen.getByText(formattedDate));
  });
});
