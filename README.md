# Manage your screening

[![CI/CD Pull Request](https://github.com/NHSDigital/dtos-participant-manager/actions/workflows/cicd-1-pull-request.yaml/badge.svg)](https://github.com/NHSDigital/dtos-participant-manager/actions/workflows/cicd-1-pull-request.yaml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=NHSDigital_dtos-participant-manager&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=NHSDigital_dtos-participant-manager)

Manage your screening allows users to see what screening they are eligible for and when their next appointment is due.

The project consists of a number of Azure functions and a user-interface built in Next.js which uses NHS login for user authentication.

## Table of Contents

- [Manage your screening](#manage-your-screening)
  - [Table of Contents](#table-of-contents)
  - [Background](#background)
  - [Solution Architecture](#solution-architecture)
  - [Decision Records](#decision-records)
  - [Setup](#setup)
    - [Prerequisites](#prerequisites)
  - [Configuration](#configuration)
  - [Usage](#usage)
  - [Testing](#testing)
    - [End to end testing](#end-to-end-testing)
  - [Open API types](#open-api-types)
  - [OpenAPI Specifications](#openapi-specifications)
  - [Contact](#contact)
  - [Licence](#licence)

## Background

The participant manager was born out of the work done by the VSCR (Vaccinations and Screening Record) team, that identified the need for presenting both vaccination and screening record data in the NHS App. This repository covers the web front end used to surface the Screening record information. This will eventually be made available within the NHS App via a web integration pattern.

## Solution Architecture

The detailed solution architecture can be found [here](/docs/solution-design)

## Decision Records

All key decision records are covered in the ADR section [here](/docs/adr/). These are intentionally supposed to be lightweight choices the team have made within the engineering red lines, following the Michael Nygard [format](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions) . Anything that steps outside of that has been covered in the internal confluence [site](https://nhsd-confluence.digital.nhs.uk/display/DTS/Architecture+Decisions+-+January+2024+onwards)

When adding a new ADR it is possible to execute it using a simple tool called adr-tools defined in the pre-requisites section

```shell
  adr new <<Name of your decision record>>
```

This will create an incremental decision within the repository for you to complete.

## Setup

Clone the repository

```shell
git clone https://github.com/NHSDigital/dtos-participant-manager
cd dtos-participant-manager
```

### Prerequisites

The following software packages, or their equivalents, are expected to be installed and configured:

- [Docker](https://www.docker.com/) container runtime or a compatible tool, e.g. [Podman](https://podman.io/),
- [Node.js](https://nodejs.org/en) - LTS
- [.NET](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) - .NET 9.0
- [Entity Framework Core tools reference - .NET Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
- [sqlcmd utility](https://learn.microsoft.com/en-us/sql/tools/sqlcmd/sqlcmd-utility?view=sql-server-ver16&tabs=go%2Cwindows&pivots=cs1-bash)
- [Azure functions core tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=macos%2Cisolated-process%2Cnode-v4%2Cpython-v2%2Chttp-trigger%2Ccontainer-apps&pivots=programming-language-csharp)
- [Azure Data Studio](https://learn.microsoft.com/en-us/azure-data-studio/download-azure-data-studio?tabs=win-install%2Cwin-user-install%2Credhat-install%2Cwindows-uninstall%2Credhat-uninstall)
- [adr-tools](https://github.com/npryce/adr-tools)
- [GNU make](https://www.gnu.org/software/make/) 3.82 or later,

> [!NOTE]<br>
> The version of GNU make available by default on macOS is earlier than 3.82. You will need to upgrade it or certain `make` tasks will fail. On macOS, you will need [Homebrew](https://brew.sh/) installed, then to install `make`, like so:
>
> ```shell
> brew install make
> ```
>
> You will then see instructions to fix your [`$PATH`](https://github.com/nhs-england-tools/dotfiles/blob/main/dot_path.tmpl) variable to make the newly installed version available. If you are using [dotfiles](https://github.com/nhs-england-tools/dotfiles), this is all done for you.

- [Python](https://www.python.org/) required to run Git hooks,
- [`jq`](https://jqlang.github.io/jq/) a lightweight and flexible command-line JSON processor.

## Configuration

Installation and configuration of the toolchain dependencies

```shell
make config
```

Rename the `.env.example` file to `.env` and populate the missing environment variables which are listed at the top of the file.

## Usage

You can run the Azure functions, database and frontend with `make all`

Once you have the database running in Docker with `make db-migrations` you can then run the application with `make application`.

`make stop` will shut down all the processes.

## Testing

The full test suite can be ran with `make test`.

Unit tests can be ran with `make test-unit` and linting can be ran with `make test-lint`

### End to end testing

In order to perform the end to end testing with playwright, there needs to be a stubbed out version of the OIDC provider. This is achieved using a keycloak docker container.

To run this locally though, local certificates need to be generated and installed in the /keycloak-config folder, these also need to be trusted by dotnet

First of all create some certs using the dotnet command

```shell
 dotnet dev-certs https --trust
```

Now export the certificate and private key into a pfx keycloak can use

```shell
dotnet dev-certs https -ep ./keycloak-cert.pfx -p <<your_password>>
```

With that pfx generate the public and private key pem files

```shell
  openssl pkcs12 -in keycloak-cert.pfx -clcerts -nokeys -out keycloak-cert.pem
  openssl pkcs12 -in keycloak-cert.pfx -nocerts -nodes -out keycloak-key.pem
```

Finally copy the pem files into the keycloak-config folder

It should now be possible to run the following and for a keycloak instance to start running locally

```shell
docker-compose up --build keycloak
```

This should result in keycloak being available at <https://localhost:8443/realms/master>

One final change is to make the local .env file to have the following values

```shell
AUTH_NHSLOGIN_ISSUER_URL=https://localhost:8443/realms/master
NODE_TLS_REJECT_UNAUTHORIZED=0
```

## Open API types

Some types are imported from the Open API docs for `ParticipantManager.API` and `ParticipantManager.Experience.API`. You can use the `openapi-typescript` npm package to generate the types with the following command:

```shell
npx openapi-typescript ./src/api/ParticipantManager.API/openapi/openapi.yaml -o ./src/web/app/types/ParticipantManager.API/schema.d.ts
```

```shell
npx openapi-typescript ./src/api/ParticipantManager.Experience.API/openapi/openapi.yaml -o ./src/web/app/types/ParticipantManager.Experience.API/schema.d.ts
```

## OpenAPI Specifications

The following OpenAPI Specifications exist for Participant Manager components:

- Participant Experience API - [Raw](https://raw.githubusercontent.com/NHSDigital/dtos-participant-manager/refs/heads/main/src/api/ParticipantManager.API/openapi/openapi.yaml) / [Swagger Editor](https://editor.swagger.io/?url=https://raw.githubusercontent.com/NHSDigital/dtos-participant-manager/refs/heads/main/src/api/ParticipantManager.API/openapi/openapi.yaml)
- Participant API - [Raw](https://raw.githubusercontent.com/NHSDigital/dtos-participant-manager/refs/heads/main/src/api/ParticipantManager.Experience.API/openapi/openapi.yaml) / [Swagger Editor](https://editor.swagger.io/?url=https://raw.githubusercontent.com/NHSDigital/dtos-participant-manager/refs/heads/main/src/api/ParticipantManager.Experience.API/openapi/openapi.yaml)

## Contact

If you are on the NHS England Slack you can contact the team on #mays-team, otherwise you can open a GitHub issue.

## Licence

Unless stated otherwise, the codebase is released under the MIT License. This covers both the codebase and any sample code in the documentation.

Any HTML or Markdown documentation is [© Crown Copyright](https://www.nationalarchives.gov.uk/information-management/re-using-public-sector-information/uk-government-licensing-framework/crown-copyright/) and available under the terms of the [Open Government Licence v3.0](https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/).
