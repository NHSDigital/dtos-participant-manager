import { test, expect } from "@playwright/test";
import { TestUtils } from "./utils/test-utils";

test.describe("Participant Management", () => {
  test.beforeEach(async () => {
    // Setup test data before each test
    await TestUtils.setupTestData();
  });

  test.afterEach(async () => {
    // Cleanup test data after each test
    await TestUtils.cleanupTestData();
  });

  test("should display participant list", async ({ page }) => {
    // Navigate to the participant list page
    await page.goto("/participants");

    // Verify the page loads correctly
    await expect(page).toHaveTitle(/Participant Management/);

    // Add more specific test assertions here
  });

  test("should allow adding a new participant", async ({ page }) => {
    // Navigate to the add participant page
    await page.goto("/participants/new");

    // Fill in the participant form
    await page.fill('[name="firstName"]', "John");
    await page.fill('[name="lastName"]', "Doe");
    // Add more form fields as needed

    // Submit the form
    await page.click('button[type="submit"]');

    // Verify the participant was added
    await expect(page.locator(".success-message")).toBeVisible();
  });
});
