# Participant Manager UI

User interface template for Screening services.

Built with [Next.js](https://nextjs.org/).

## Prerequisites

- [npm](https://nodejs.org/en) package manager

## Environment variables

Rename the `.env.example` to `.env` and add the missing values.

```text
# Default
SERVICE_NAME="Manage your screening"

# Auth
NEXTAUTH_URL=https://localhost:3000/api/auth
NEXTAUTH_SECRET={RANDOM_SECRET_STRING}

# NHS login
AUTH_NHSLOGIN_ISSUER_URL=https://auth.sandpit.signin.nhs.uk
AUTH_NHSLOGIN_CLIENT_ID={CLIENT_ID}
AUTH_NHSLOGIN_PRIVATE_KEY={SECRET_KEY}

# API
API_URL=https://localhost:5001

# Required for hosted environments
AUTH_TRUST_HOST=true
```

For `NEXTAUTH_SECRET` you can use `openssl rand -base64 32` or [https://generate-secret.vercel.app/32](https://generate-secret.vercel.app/32) to generate a random value.

## Running locally

Install the required dependencies using

```bash
npm install
```

Then, run the development server:

```bash
npm run dev:secure
```

Open [https://localhost:3000](https://localhost:3000).

## Testing

### Unit tests

Unit tests are written using [Jest](https://jestjs.io/) and [React Testing Library](https://testing-library.com/docs/react-testing-library/intro/).

#### Running unit tests

To run unit tests, use the following command:

```bash
npm run test:unit
```

This will execute all unit tests and provide a summary of the tests results.

#### Running unit tests in watch mode

To run the unit tests in watch mode, use the following command:

```bash
npm run test:unit:watch
```

This will run the tests and re-run them whenever a file changes.

#### Coverage report

To generate a code coverage report, use the following command:

```bash
npm run test:unit:coverage
```

This will generate a coverage report in the `coverage` directory.

### End-to-end tests

End-to-end tests are written using [Playwright](https://playwright.dev/).

Before running end-to-end tests make sure your development server is running locally on `http://localhost:3000`. Using the command `pnpm run dev`.

#### Running end-to-end tests

To run the end-to-end tests, use the following command:

```bash
npm run test:e2e:ui
```

This will open the Playwright test runner, where you can run the tests interactively.

### Running end-to-end tests in headless mode

To run the end-to-end tests in headless mode, use the following command:

```bash
npm run test:e2e
```

This will run the tests in headless mode and output the results to the terminal.

## Licence

Unless stated otherwise, the codebase is released under the MIT License. This covers both the codebase and any sample code in the documentation.

Any HTML or Markdown documentation is [© Crown Copyright](https://www.nationalarchives.gov.uk/information-management/re-using-public-sector-information/uk-government-licensing-framework/crown-copyright/) and available under the terms of the [Open Government Licence v3.0](https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/).
