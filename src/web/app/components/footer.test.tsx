import { render, screen } from "@testing-library/react";
import "@testing-library/jest-dom";
import Footer from "@/app/components/footer";

describe("Footer", () => {
  it("renders footer with correct role", () => {
    render(<Footer />);
    expect(screen.getByRole("contentinfo")).toBeInTheDocument();
  });

  it("renders visually hidden support links heading", () => {
    render(<Footer />);
    const heading = screen.getByRole("heading", {
      level: 2,
      name: "Support links",
    });
    expect(heading).toHaveClass("nhsuk-u-visually-hidden");
  });

  it("renders all footer links correctly", () => {
    render(<Footer />);

    const expectedLinks = [
      { text: "Accessibility statement", href: "/accessibility-statement" },
      { text: "Contact us", href: "mailto:england.digitalscreening@nhs.net" },
      { text: "Cookies", href: "/cookies-policy" },
      { text: "Privacy policy", href: "#" },
      { text: "Terms and conditions", href: "#" },
    ];

    expectedLinks.forEach(({ text, href }) => {
      const link = screen.getByRole("link", { name: text });
      expect(link).toBeInTheDocument();
      expect(link).toHaveAttribute("href", href);
      expect(link).toHaveClass("nhsuk-footer__list-item-link");
    });
  });

  it("renders copyright text", () => {
    render(<Footer />);
    expect(screen.getByText(/© NHS England/)).toBeInTheDocument();
  });
});
