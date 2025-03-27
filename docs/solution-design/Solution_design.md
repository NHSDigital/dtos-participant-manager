# Solution Architecture

## Overview

The DTOS Participant Manager is a modern web application designed to manage participant screening records as part of the NHS Digital Transformation of Screening (DTOS) program. The application handles Data Transfer Objects (DTOs) that represent participant screening data, ensuring secure and efficient data management. The application follows a microservices architecture pattern with clear separation of concerns between the frontend and backend components, leveraging Azure Functions for serverless computing.

## Container Diagram

![Participant Manager Container Diagram](https://github.com/NHSDigital/dtos-solution-architecture/blob/main/images/ParticipantManager.png)

## System Components

### 1. Frontend (Web Application)

- Located in `src/web/`
- Modern web application built with React/TypeScript
- Provides user interface for participant management
- Communicates with backend API through REST endpoints

### 2. Backend (API)

- Located in `src/api/`
- Built with C# and .NET
- Azure Functions-based serverless API
- Handles business logic and data management
- Implements authentication and authorization
- Manages data persistence and retrieval
- Leverages Azure Functions triggers for event-driven operations

### 3. Infrastructure

The application is deployed using infrastructure as code (IaC) with Terraform:

#### Core Infrastructure (`infrastructure/tf-core/`)

- Manages core infrastructure components
- Handles networking, security, and basic service setup
- Implements core Azure resources including Azure Functions
- Manages Azure App Service configurations

#### Audit Infrastructure (`infrastructure/tf-audit/`)

- Manages audit-related infrastructure
- Handles logging and monitoring setup
- Implements audit-specific Azure resources

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

### 3. Containerization

- Application components containerized using Docker
- Container orchestration through Docker Compose
- Consistent deployment across environments
- Azure Functions container support

## Security

### 1. Authentication & Authorization

- Secure user authentication
- Role-based access control
- API security with JWT tokens
- Azure AD integration for authentication

### 2. Data Security

- Encrypted data transmission
- Secure storage of sensitive information
- Regular security audits
- Azure Key Vault integration

## Development & Deployment

### 1. Development Environment

- Local development using Docker Compose
- Hot-reloading for frontend development
- Automated testing setup
- Azure Functions Core Tools for local development
- .NET SDK and C# development tools

### 2. CI/CD Pipeline

- Automated builds and tests
- Continuous integration through GitHub Actions
- Automated deployment to Azure
- Azure Functions deployment automation
- .NET build and test automation

### 3. Monitoring & Logging

- Centralized logging system
- Application monitoring
- Performance metrics collection
- Azure Application Insights integration
- Azure Functions monitoring

## Dependencies

### 1. External Services

- Azure Cloud Services
  - Azure Functions
  - Azure App Service
  - Azure Key Vault
  - Azure Application Insights
- Database Services
- Authentication Services

### 2. Development Tools

- Node.js/TypeScript (Frontend)
- C# and .NET SDK (Backend)
- Azure Functions Core Tools
- Docker
- Terraform
- Azure CLI

## Future Considerations

### 1. Scalability

- Horizontal scaling of services
- Load balancing
- Caching strategies
- Azure Functions scaling optimization

### 2. Performance

- Performance optimization
- Caching implementation
- Database optimization
- Azure Functions performance tuning

### 3. Maintenance

- Regular security updates
- Dependency management
- Monitoring improvements
- Azure Functions runtime updates
