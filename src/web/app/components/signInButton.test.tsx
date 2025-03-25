import { render, screen } from "@testing-library/react";
import "@testing-library/jest-dom";
import SignInButton from "@/app/components/signInButton";
import { getAuthConfig } from "@/app/lib/auth";

// Mock dependencies
jest.mock("../lib/auth", () => ({
  getAuthConfig: jest.fn(),
}));

// Mock the server action
jest.mock("next/navigation", () => ({
  useFormStatus: () => ({ pending: false }),
}));

describe("SignInButton", () => {
  let consoleError: jest.SpyInstance;
  let mockSignIn: jest.Mock;

  beforeEach(() => {
    mockSignIn = jest.fn();
    (getAuthConfig as jest.Mock).mockResolvedValue({
      signIn: mockSignIn,
    });
    // Suppress form action warning
    consoleError = jest.spyOn(console, "error").mockImplementation(() => {});
  });

  afterEach(() => {
    jest.clearAllMocks();
    consoleError.mockRestore();
  });

  it("renders the sign in button with correct text", () => {
    render(<SignInButton />);

    const button = screen.getByRole("button", {
      name: "Continue to NHS login",
    });
    expect(button).toBeInTheDocument();
  });

  it("has correct button attributes", () => {
    render(<SignInButton />);

    const button = screen.getByRole("button");
    expect(button).toHaveAttribute("type", "submit");
    expect(button).toHaveAttribute("data-module", "nhsuk-button");
  });
});
