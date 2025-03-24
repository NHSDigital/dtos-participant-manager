import { render, screen } from "@testing-library/react";
import "@testing-library/jest-dom";
import Card from "@/app/components/card";

describe("Card", () => {
  const defaultProps = {
    title: "Test Card",
    url: "/test-url",
  };

  it("renders card with correct title", () => {
    render(<Card {...defaultProps} />);

    const heading = screen.getByRole("heading", {
      level: 2,
      name: defaultProps.title,
    });
    expect(heading).toBeInTheDocument();
  });

  it("renders link with correct URL", () => {
    render(<Card {...defaultProps} />);

    const link = screen.getByRole("link", { name: defaultProps.title });
    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute("href", defaultProps.url);
  });
});
