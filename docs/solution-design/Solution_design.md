# Solution Architecture

## Overview

The DTOS Participant Manager is a modern web application designed to manage participant screening records as part of the NHS Digital Transformation of Screening (DTOS) program. The application handles Data Transfer Objects (DTOs) that represent participant screening data, ensuring secure and efficient data management. The application follows a microservices architecture pattern with clear separation of concerns between the frontend and backend components, leveraging Azure Functions for serverless computing.

## Container Diagram

![Participant Manager Container Diagram](https://github.com/NHSDigital/dtos-solution-architecture/blob/main/images/ParticipantManager_Alpha.png)

For the Alpha we have essentially 5 components: -

- Participant Facing Web Application
- Participant Experience API
- Participant API
- Participant Manager Product Event Handler
- Participant Manager Data Store

## System Components

### 1. Participant Facing Web Application

- Provides a logged in user interface for participants to view their screening information
- Located in `src/web/`
- Modern web application built with Nextjs
- The nextjs implementation is Server Side Rendered, which removes the dependency on javascript on the client
- It uses NHS Login as it's Identity provider and follows the standard OIDC protocol for integration
- Communicates only with the experience API to return data for the user to see.

### 2. Participant Experience API

- Acts as an aggregation function to pull data from various sources in order to support a seamless participant experience
- Located in `src/api/ParticipantManager.Experience.API`
- Built with C#, it is a single Azure function, with multiple HTTP endpoints
- It is secured using NHS Login's access token
- It is currently only pulling data using the Participant Manager API, but the expectation is for this to increase as we look to pull data from multiple sources

### 3. Participant API

- Provides a CRUD interface to interact with the underlying Data Entities (Participant/PathwayEnrollment/Episode)
- Located in `src/api/ParticipantManager.API`
- Built with C#, it is a single Azure function, with multiple HTTP endpoints
- It is not internet accessible and can only be called by the Experience API, but this is controlled at a Function level
- It queries data from the database using Entity Framework

### 4. Participant Data Store

- Single datastore with 3 tables within it

````mermaid
erDiagram
    PARTICIPANT ||--o{ PATHWAY_TYPE_ENROLMENT : has
    PATHWAY_TYPE_ENROLMENT ||--o{ EPISODE : has

    PARTICIPANT {
        Guid ParticipantId PK
        string NhsNumber
        date DateOfBirth
    }

    PATHWAY_TYPE_ENROLMENT {
        Guid EnrolmentId PK
        Guid ParticipantId FK
        string PathwayType
    }

    EPISODE {
        Guid EpisodeId PK
        Guid EnrolmentId FK
        string PathwayVersion
        string Status
    }

- Database structure is managed using the Entity Framework Code First approach and therefore management of database schema changes is managed by Entity Framework

### 5. ParticipantManager Product Event Handler

- Invocation of functionality within Participant Manager is provided by invoking events. The Event handler here listens to events on a Product specific Service Bus
- The event handler will talk to the Participant API to affect changes in the database layer

## Sequence Diagrams

### User Logging In

```mermaid
sequenceDiagram
    participant User
    participant NextJS
    participant NHSLogin
    participant ExperienceAPI
    participant ParticipantAPI
    participant Database

    activate User
    User->>NextJS: Click "Continue to NHS login"
    activate NextJS
    NextJS->>NHSLogin: Redirect to NHS Login
    activate NHSLogin
    NHSLogin->>User: Show NHS Login page
    User->>NHSLogin: Enter credentials
    NHSLogin->>NHSLogin: Validate credentials
    NHSLogin->>NextJS: Redirect with auth code
    deactivate NHSLogin
    NextJS->>NHSLogin: Exchange code for tokens
    activate NHSLogin
    NHSLogin->>NextJS: Return access_token & id_token
    deactivate NHSLogin
    NextJS->>NHSLogin: Request userinfo
    activate NHSLogin
    NHSLogin->>NextJS: Return user profile
    deactivate NHSLogin
    NextJS->>ExperienceAPI: Get participant ID
    activate ExperienceAPI
    ExperienceAPI->>ParticipantAPI: Get participant by NHS number
    activate ParticipantAPI
    ParticipantAPI->>Database: Query participant
    activate Database
    Database-->>ParticipantAPI: Return participant
    deactivate Database
    ParticipantAPI-->>ExperienceAPI: Return participant ID
    deactivate ParticipantAPI
    ExperienceAPI-->>NextJS: Return participant ID
    deactivate ExperienceAPI
    NextJS->>User: Show screening page
    deactivate NextJS
````

### User retrieving eligibility data

## Architecture Patterns

### 1. Microservices Architecture

- Clear separation between frontend and backend services
- Independent deployment and scaling of components
- Service-to-service communication via REST APIs
- Serverless architecture using Azure Functions

### 2. Infrastructure as Code

- All infrastructure components defined in Terraform
- Version controlled infrastructure
- Automated deployment and management
- Azure Functions app configuration management

### 3. Event based invocation

- External services are invoked using an Event Bases Architecture
- Participant manager assumes that the sequencing of commands is important and will listen to events on a Service Bus queue

## Non Functional Aspects

### 1. Security

#### Authentication & Authorisation

- Web user interface is secured using NHS Login and follows the OIDC Web Flow
- Only P9 users can access the service as the data being presented is medically sensitive
- The communication from the Nextjs application code to the Participant Experience API is secured using the Access Token provided by NHS Login
- Access from the experience layer to the crud operations is secured using the in-built azure function level permissions

#### Data Security

- Encrypted data transmission
- Secure storage of sensitive information
- Regular security audits
- Azure Key Vault integration

### 2. Monitoring & Logging

- Centralized logging system
- Application monitoring
- Performance metrics collection
- Azure Application Insights integration
- Azure Functions monitoring

### 3.Scalability

- Horizontal scaling of services
- Load balancing
- Caching strategies
- Azure Functions scaling optimization

### 4.Performance

- Performance optimization
- Caching implementation
- Database optimization
- Azure Functions performance tuning

### 5.Maintenance

- Regular security updates
- Dependency management
- Monitoring improvements
- Azure Functions runtime updates

```

```
