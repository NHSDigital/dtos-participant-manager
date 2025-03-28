import { render, screen } from "@testing-library/react";
import "@testing-library/jest-dom";
import Header from "@/app/components/header";
import { auth } from "@/app/lib/auth";

// Mock Next.js navigation and form action
jest.mock("next/navigation", () => ({
  useRouter: () => ({
    push: jest.fn(),
  }),
}));

// Mock the auth module
jest.mock("@/app/lib/auth", () => ({
  auth: jest.fn(),
}));

describe("Header", () => {
  let consoleError: jest.SpyInstance;
  const mockServiceName = "Test Service";

  beforeEach(() => {
    process.env.SERVICE_NAME = mockServiceName;
    process.env.AUTH_NHS_LOGIN_SETTINGS_URL = "https://settings.test";
    // Silence console.error for form validation
    consoleError = jest.spyOn(console, "error").mockImplementation(() => {});
  });

  afterEach(() => {
    consoleError.mockRestore();
  });

  it("renders header with service name", async () => {
    (auth as jest.Mock).mockResolvedValue(null);

    render(await Header({}));

    expect(screen.getByRole("banner")).toBeInTheDocument();
    expect(screen.getByText(mockServiceName)).toBeInTheDocument();
  });

  describe("when user is authenticated", () => {
    const mockUser = {
      firstName: "John",
      lastName: "Doe",
    };

    beforeEach(() => {
      (auth as jest.Mock).mockResolvedValue({ user: mockUser });
    });

    it("displays user name", async () => {
      render(await Header({}));
      expect(
        screen.getByText(`${mockUser.firstName} ${mockUser.lastName}`)
      ).toBeInTheDocument();
    });

    it("renders account navigation", async () => {
      render(await Header({}));

      const nav = screen.getByRole("navigation", { name: "Account" });
      expect(nav).toBeInTheDocument();

      expect(
        screen.getByRole("link", { name: "Account and settings" })
      ).toHaveAttribute("href", "https://settings.test");

      expect(
        screen.getByRole("button", { name: "Log out" })
      ).toBeInTheDocument();
    });

    it("displays log out button", async () => {
      render(await Header({}));
      expect(
        screen.getByRole("button", { name: "Log out" })
      ).toBeInTheDocument();
    });
  });

  describe("when user is not authenticated", () => {
    beforeEach(() => {
      (auth as jest.Mock).mockResolvedValue(null);
    });

    it("does not display account navigation", async () => {
      render(await Header({}));

      expect(
        screen.queryByRole("navigation", { name: "Account" })
      ).not.toBeInTheDocument();

      expect(screen.queryByText("Log out")).not.toBeInTheDocument();
    });
  });

  it("uses custom service name when provided", async () => {
    (auth as jest.Mock).mockResolvedValue(null);

    const customServiceName = "Custom Service";
    render(await Header({ serviceName: customServiceName }));

    expect(screen.getByText(customServiceName)).toBeInTheDocument();
  });
});
