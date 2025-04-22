import { expect } from "@playwright/test";
import { Given, When, Then } from "./fixtures";
import AxeBuilder from "@axe-core/playwright";

When("I go to the page {string}", async ({ page }, url) => {
  await page.goto(url, { timeout: 60000 });
});

Given(
  "I am signed in as {string} with password {string}",
  async ({ page }, username: string, password: string) => {
    await page.goto("/");
    await page.getByRole("button", { name: /Continue to NHS login/i }).click();
    await page.waitForSelector("form#kc-form-login", { timeout: 60000 });
    await page.locator("#username").fill(username);
    await page.locator("#password").fill(password);
    await page.locator("#kc-login").click();
    await page.waitForURL("/screening", { timeout: 60000 });
    await expect(page).toHaveURL(/\/screening$/);
  }
);

When("I click the button {string}", async ({ page }, buttonText) => {
  await page.getByRole("button", { name: buttonText }).click();
});

When("I click the card with title {string}", async ({ page }, title: string) => {
  const cardLink = page.locator(".nhsuk-card__link", { hasText: title });
  await cardLink.click();
});

When(
  "I fill in the login form with username {string} and password {string}",
  async ({ page }, username: string, password: string) => {
    await page.waitForSelector("form#kc-form-login", { timeout: 60000 });
    await page.locator('#username').fill(username);
    await page.locator('#password').fill(password);
    await page.locator('#kc-login').click();
  }
);

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



Then("I should be redirected to the page {string}", async ({ page }, expectedUrl) => {
  const urlRegex = new RegExp(expectedUrl);
  await page.waitForURL(urlRegex, { timeout: 60000 });
  await expect(page).toHaveURL(urlRegex);
});

Then("I should see the heading {string}", async ({ page }, headingText) => {
  await expect(page.locator("h1")).toHaveText(headingText);
});

Then("I should see {int} clickable cards", async ({ page }, count: number) => {
  const cards = await page.locator(".nhsuk-card--clickable");
  await expect(cards).toHaveCount(count);
});

Then("I should see a card with title {string}", async ({ page }, title: string) => {
  const card = page.locator(".nhsuk-card__link", { hasText: title });
  await expect(card).toHaveCount(1);
});

Then("I should see the user name {string} in the header", async ({ page }, name: string) => {
    await expect(
      page
        .locator(".header-module-scss-module__APzleW__nhsuk-header__account-list")
        .getByText(name, { exact: false })
    ).toBeVisible();
});

Then("I should not have the session cookie", async ({ page }) => {
  await expect.poll(async () => {
    const cookies = await page.context().cookies();
    return cookies.find((cookie) => cookie.name === "__Secure-authjs.session-token");
  }, {
    timeout: 60000,
    message: "Expected session cookie to be removed",
  }).toBeUndefined();
});
