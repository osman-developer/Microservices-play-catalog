Two Microservices, the purpose of these services is like a purchase action in a game, so the user can buy items from inventory, so one service is used to manage products, and another one is used to manage the purchase of product from the inventory by a user. 
The two services communicate in two ways, sync(httpClient) and async(RabbitMQ). 
tech and patterns used : .NET , MongoDB ,MassTransit,HttpClient, Polly,Circuit Breaker, RabbitMQ, Docker 

Created nuget packages to be commonly used between services.
