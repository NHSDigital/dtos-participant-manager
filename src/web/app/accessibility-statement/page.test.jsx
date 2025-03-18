import "@testing-library/jest-dom";
import { render, screen } from "@testing-library/react";
import Page from "@/app/accessibility-statement/page";

describe("Page", () => {
  beforeEach(() => {
    process.env.SERVICE_NAME = "Test Service";
    consoleErrorSpy = jest.spyOn(console, "error").mockImplementation(() => {});
  });
  it("renders the page successfully", async () => {
    render(await Page());

    // Check main landmarks are present
    expect(screen.getByRole("main")).toBeInTheDocument();

    // Check essential content
    expect(
      screen.getByRole("heading", {
        level: 1,
      })
    ).toBeInTheDocument();

    // Check service name is displayed
    expect(screen.getByText(/Test Service/)).toBeInTheDocument();

    // Verify no console errors occurred during render
    expect(console.error).not.toHaveBeenCalled();
  });
});
