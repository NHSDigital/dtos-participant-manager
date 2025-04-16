import { expect } from "@playwright/test";
import { Given, When, Then } from "./fixtures";
import AxeBuilder from "@axe-core/playwright";

Given("I am on the page {string}", async ({ page }, url) => {
  await page.goto(url);
});

When("I arrive on on the page {string}", async ({ page }, url) => {
  await expect(page).toHaveURL(url);
});

Then("I see the heading {string}", async ({ page }, heading) => {
  await expect(page.locator("h1")).toHaveText(heading);
});

Then("I see the button {string}", async ({ page }, label) => {
  await expect(page.locator('[data-qa="nhs-login-button"]')).toContainText(
    label
  );
});

Then(
  "I should expect {string} accessibility issues",
  async ({ page }, accessibilityIssues: string) => {
    const accessibilityScanResults = await new AxeBuilder({
      page,
    }).analyze();

    const expectedViolations =
      Number(accessibilityIssues) === 0
        ? []
        : accessibilityScanResults.violations.slice(
            0,
            Number(accessibilityIssues)
          );
    expect(accessibilityScanResults.violations).toEqual(expectedViolations);
  }
);
