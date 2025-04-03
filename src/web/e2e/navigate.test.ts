import { test, expect } from "@playwright/test";

// As a user
// When I navigate to the home page
// Then I should see the home page
// And I should see the NHS login button
test("navigate to the home page", async ({ page }) => {
  await page.goto("/");
  await expect(page.locator("h1")).toHaveText("Manage your screening");
  await expect(page.locator('[data-qa="nhs-login-button"]')).toContainText(
    "Continue to NHS login"
  );
});

// As an unauthenticated user
// When I navigate to /screening
// Then I should be redirected to the home page
test("unauthenticated user should be redirected to the home page", async ({
  page,
}) => {
  await page.goto("/screening");
  await expect(page).toHaveURL("/");
  await expect(page.locator("h1")).toHaveText("Manage your screening");
});

// As a user
// When I navigate to the cookies policy page
// Then I should see the cookies policy page
test("navigate to the cookies policy page", async ({ page }) => {
  await page.goto("/cookies-policy");
  await expect(page).toHaveURL("/cookies-policy");
  await expect(page.locator("h1")).toHaveText("Cookie policy");
});

// As a user
// When I navigate to the accessibility statement page
// Then I should see the accessibility statement page
test("navigate to the accessibility statement page", async ({ page }) => {
  await page.goto("/accessibility-statement");
  await expect(page).toHaveURL("/accessibility-statement");
  await expect(page.locator("h1")).toHaveText("Accessibility statement");
});
