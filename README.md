# QuiosqueFood3000-Kitchen

QuiosqueFood3000-Kitchen is a backend service for managing kitchen order processing in a food kiosk scenario. It is built using .NET 8, follows clean architecture principles, and integrates with AWS DynamoDB for persistence. The project includes robust API endpoints, business logic, validation, and comprehensive automated testing. Code quality and coverage are continuously analyzed using SonarCloud.

## Features
- RESTful API for order creation, status updates, and production queue retrieval
- Health check endpoint for service monitoring
- Business logic encapsulated in service and repository layers
- Validation using FluentValidation
- Persistence using AWS DynamoDB (local and cloud support)
- Dependency injection for all services and infrastructure
- Comprehensive unit and integration tests using xUnit, Moq, AutoFixture, and FluentAssertions
- Code quality and coverage reporting via SonarCloud and GitHub Actions

## Project Structure
```
QuiosqueFood3000-Kitchen/
├── QuiosqueFood3000.Kitchen.Api/           # ASP.NET Core Web API (controllers, IoC)
├── QuiosqueFood3000.Kitchen.Application/   # Application services, interfaces, validators
├── QuiosqueFood3000.Kitchen.Domain/        # Domain entities and enums
├── QuiosqueFood3000.Kitchen.Infrastructure/# DynamoDB repository implementation
├── QuiosqueFood3000.Kitchen.Tests/         # Automated tests (xUnit)
├── .github/workflows/                      # CI/CD and SonarCloud workflows
├── .runsettings                            # Code coverage settings
├── Dockerfile, docker-compose.yml           # Containerization support
└── README.md                               # Project documentation
```

## Getting Started
### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [AWS DynamoDB Local](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html) (for local development)
- Docker (optional, for containerized runs)

### Setup
1. **Clone the repository:**
   ```sh
   git clone <repo-url>
   cd QuiosqueFood3000-Kitchen
   ```
2. **Restore dependencies:**
   ```sh
   dotnet restore
   ```
3. **Configure DynamoDB:**
   - For local: Set `DynamoDB:ServiceURL` in `appsettings.Development.json` to your local DynamoDB endpoint (e.g., `http://localhost:8000`).
   - For AWS: Configure AWS credentials and region as needed.

4. **Run the application:**
   ```sh
   dotnet run --project QuiosqueFood3000.Kitchen.Api
   ```

### Running Tests
To execute all tests and generate a coverage report:
```sh
# Run tests with coverage (OpenCover format)
dotnet test --settings .runsettings /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```
Coverage reports will be generated under the `TestResults` directory.

### Code Quality & Coverage
- Code quality and coverage are checked automatically by GitHub Actions using SonarCloud.
- The workflow is defined in `.github/workflows/sonarqube.yml`.
- Coverage is reported using the OpenCover format and excludes simple POCO domain entities.

## API Endpoints
- `POST /api/ordersolicitation` - Create a new order
- `PUT /api/ordersolicitation/{id}/status` - Update order status
- `GET /api/ordersolicitation/production-queue` - Get production queue
- `GET /health` - Health check

## Testing
- Unit and integration tests cover controllers, services, repositories, validators, and dependency injection.
- Mocks and stubs are used for DynamoDB and other dependencies.
- Tests are located in the `QuiosqueFood3000.Kitchen.Tests` project.

## Contributing
Contributions are welcome! Please fork the repository and submit a pull request.

## License
This project is intended for educational and demonstration purposes only.

---

For further details, see inline code comments and the AWS deployment guide (`AWS-DEPLOYMENT-GUIDE.md`).
