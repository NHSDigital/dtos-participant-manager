import { render, screen } from "@testing-library/react";
import "@testing-library/jest-dom";
import Breadcrumb from "@/app/components/breadcrumb";

describe("Breadcrumb", () => {
  const mockItems = [
    { label: "Home", url: "/" },
    { label: "Screening", url: "/screening" },
    { label: "Bowel", url: "/bowel-screening" },
  ];

  it("renders breadcrumb navigation with correct ARIA label", () => {
    render(<Breadcrumb items={[]} />);
    expect(
      screen.getByRole("navigation", { name: "Breadcrumb" })
    ).toBeInTheDocument();
  });

  it("renders all breadcrumb items as links", () => {
    render(<Breadcrumb items={mockItems} />);

    mockItems.forEach((item) => {
      const link = screen.getByRole("link", { name: item.label });
      expect(link).toBeInTheDocument();
      expect(link).toHaveAttribute("href", item.url);
    });
  });

  it("renders items in correct order", () => {
    render(<Breadcrumb items={mockItems} />);

    const links = screen.getAllByRole("link");
    mockItems.forEach((item, index) => {
      expect(links[index]).toHaveTextContent(item.label);
      expect(links[index]).toHaveAttribute("href", item.url);
    });
  });

  it("renders back link with visually hidden text", () => {
    render(<Breadcrumb items={mockItems} />);

    const lastItem = mockItems[mockItems.length - 1];
    const backLink = screen.getByRole("link", {
      name: `Back to ${lastItem.label}`,
    });

    expect(backLink).toBeInTheDocument();
    expect(backLink).toHaveAttribute("href", lastItem.url);
    expect(screen.getByText("Back to")).toHaveClass("nhsuk-u-visually-hidden");
  });

  it("handles empty items array", () => {
    render(<Breadcrumb items={[]} />);

    expect(screen.queryByRole("link")).not.toBeInTheDocument();
    expect(screen.queryByText("Back to")).not.toBeInTheDocument();
  });

  it("handles single item", () => {
    const singleItem = [{ label: "Home", url: "/" }];
    render(<Breadcrumb items={singleItem} />);

    const links = screen.getAllByRole("link");
    expect(links).toHaveLength(2);
    expect(links[0]).toHaveTextContent("Home");
    expect(links[1]).toHaveTextContent("Home");
  });
});
