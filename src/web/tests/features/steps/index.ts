import { expect } from "@playwright/test";
import { Given, When, Then } from "./fixtures";

Given("I am on the Homepage {string}", async ({ page }, url) => {
  await page.goto(url);
});

When("I arrive on on the Homepage {string}", async ({ page }, url) => {
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
