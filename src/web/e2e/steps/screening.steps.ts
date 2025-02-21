import { Given, When, Then } from "@playwright/bdd";
import { expect } from "@playwright/test";

Given("I am a logged-in user with a {string} level of verification", async ({ page }, level) => {
  await page.goto("/login");
  await page.fill("#username", "testuser");
  await page.fill("#password", "testpassword");
  await page.click("#login-button");

  // Mock user verification level
  await page.evaluate((userLevel) => {
    localStorage.setItem("verification_level", userLevel);
  }, level);

  await page.reload();
});

When("I access the application", async ({ page }) => {
  await page.goto("/dashboard");
});

Then("I should see an informational page explaining my ineligibility", async ({ page }) => {
  await expect(page.locator(".info-message")).toContainText("You are not eligible for screening.");
});

Given("I am eligible for {string}", async ({ page }, screeningType) => {
  await page.evaluate((type) => {
    localStorage.setItem("eligible_screening", type);
  }, screeningType);
  await page.reload();
});

Then("I should see a {string} link", async ({ page }, screeningType) => {
  await expect(page.locator(`a:has-text("${screeningType} Screening")`)).toBeVisible();
});

When("I click on the {string} link", async ({ page }, screeningType) => {
  await page.click(`a:has-text("${screeningType} Screening")`);
});

Then("I should be taken to a page that shows my next test due date", async ({ page }) => {
  await expect(page.locator(".test-due-date")).toBeVisible();
});

When("I click the More Information link", async ({ page }) => {
  await page.click('a:has-text("More Information")');
});

Then("I should be redirected to the NHS UK webpage about {string}", async ({ page }, screeningType) => {
  const expectedUrl = screeningType.toLowerCase().includes("breast")
    ? "https://www.nhs.uk/conditions/breast-screening-mammogram/"
    : "https://www.nhs.uk/conditions/cervical-screening/";

  await expect(page).toHaveURL(expectedUrl);
});

Then("I should see both the Breast Screening link and the Cervical Screening link", async ({ page }) => {
  await expect(page.locator('a:has-text("Breast Screening")')).toBeVisible();
  await expect(page.locator('a:has-text("Cervical Screening")')).toBeVisible();
});

Then("I should be prompted to upgrade to P9 verification", async ({ page }) => {
  await expect(page.locator(".upgrade-message")).toContainText("Upgrade to P9 verification.");
});
