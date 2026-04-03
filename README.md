# Ticket Notification System (.NET 8 Web API)

This is an educational project featuring a ticket management and multi-channel notification system (Email, SMS, Push). The project is built following **SOLID** principles and modern ASP.NET Core architectural patterns.

## 🚀 Key Features

* **Ticket Management**: Create tickets with automatic notification queue generation.
* **Multi-channel Support**: Built-in support for sending notifications via Email, SMS, and Push.
* **Retry Logic**: Automatic retry mechanism in case of delivery failures.
* **Idempotency**: Protection against duplicate sending of already delivered notifications.
* **In-Memory Storage**: Uses a Singleton-based storage for database-less operation.

## 🛠 Tech Stack

* **Framework**: .NET 8.0 (ASP.NET Core Web API)
* **DI Container**: Built-in Microsoft.Extensions.DependencyInjection
* **Testing**: xUnit, Moq
* **Documentation**: Swagger / OpenAPI

## 🏗 Architecture & Patterns

The project demonstrates the practical application of the following concepts:

1. **Open/Closed (OCP)**: Adding a new communication channel (e.g., Telegram) does not require modifying existing service code.
2. **Dependency Inversion (DIP)**: Extensive use of interfaces (`ITicketStorage`, `INotificationSender`) for flexible implementation swapping.
3. **Strategy Pattern**: Dynamic selection of the appropriate sender based on the notification channel type.

## 📋 Getting Started

1. Clone the repository.
2. Open the solution (`.sln`) in Visual Studio 2022.
3. Press `F5` to run the project.
4. Navigate to `https://localhost:XXXX/swagger` to access the API documentation.

## 🧪 Testing

The project includes 6 Unit Tests covering critical business logic:
* Validation of 3-channel notification generation for each ticket.
* Verification of the `MaxRetryAttempts` limit.
* Idempotency testing (duplicate protection).
* Error handling and `LastError` state persistence.

To run the tests, open **Test Explorer** in Visual Studio and click **Run All**.

## 📖 API Endpoints

* `POST /api/tickets` — Create a new ticket.
* `GET /api/tickets/{id}` — Retrieve ticket data and its notification statuses.
* `POST /api/tickets/{id}/notify` — Trigger the notification dispatch process for a specific ticket.
