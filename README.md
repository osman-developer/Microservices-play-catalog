# Game Purchase Microservices

This repository contains two interdependent microservices designed to handle in-game purchases within a game. These services allow users to buy items from an inventory. Below is a detailed overview of the services, technologies, patterns, and shared libraries used in the implementation.

## Overview

The system consists of two main microservices:

1. **Product Management Service**:
   - Manages the inventory and products available for purchase.
   - Ensures product availability and updates the product list.

2. **Purchase Service**:
   - Handles user purchases of products from the inventory.
   - Updates inventory and processes transactions.

### Communication Between Services

The two services communicate using two primary methods:

- **Synchronous Communication**:
  - **HttpClient** is used for real-time interactions. This ensures that immediate feedback is provided to the user regarding product availability and successful transactions.

- **Asynchronous Communication**:
  - **RabbitMQ** is used for event-driven communication, allowing tasks like inventory updates and purchase completions to be processed without requiring immediate responses.

## Technologies and Design Patterns

- **.NET**: The framework used to build both microservices, ensuring a scalable and performant backend system.
- **MongoDB**: A NoSQL database used for efficient storage of product and transaction data. It handles large volumes of inventory and purchase records.
- **MassTransit**: A message bus simplifying integration with **RabbitMQ** for event-driven communication, ensuring scalability and reliability in handling asynchronous messages.
- **HttpClient**: Facilitates synchronous communication between services, enabling real-time interactions during the purchasing process.
- **Polly & Circuit Breaker**: These libraries are used to ensure resiliency, managing transient faults and preventing cascading failures during communication with external services.
- **RabbitMQ**: A message broker that ensures reliable and decoupled communication between services, processing events such as inventory updates and purchase completions.
- **Docker**: The services are containerized using Docker, ensuring easy deployment, scaling, and isolation of the microservices in various environments.

## Shared Libraries

To promote reusability and consistency across the services, **NuGet packages** have been created for commonly used functionality. These shared packages encapsulate common logic such as validation, error handling, and messaging patterns, ensuring that both services share the same implementations, reducing code duplication and maintaining consistency.

## Architecture

- **Microservices**: The services are loosely coupled, each with its own independent responsibility (product management and purchase handling).
- **Event-Driven**: Asynchronous communication through RabbitMQ allows for decoupled processing of events, such as inventory updates or purchase processing, ensuring smooth operations even under high load.
- **Resiliency**: The combination of **Polly**, **Circuit Breaker**, and asynchronous messaging ensures the system remains stable and available even during intermittent failures or high demand.

## Getting Started

### Prerequisites

- Docker
- .NET Core SDK
- MongoDB (local or a cloud service)
- RabbitMQ (local or a cloud service)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/game-purchase-microservices.git
   cd game-purchase-microservices
