import { test, expect } from "@playwright/test";

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
