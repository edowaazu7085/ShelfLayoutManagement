﻿# Shelf Layout Management Solution
This solution has been created using gRPC and MongoDB. gRPC is typically faster sending binary and using HTTP/2 which uses network resources more efficiently than HTTP/1.1 used in many REST APIs. 
I also chose gRPC for it's stream capabilities incase of low latency communication. MongoDB has been selected for the backend due mainly due to horizontal scaling being a bit of an issue for traditional relational databases. The trade off is the lack of procedures has meant I have had to code functions like finding the next cabinet number instead of letting the database do this automatically.

If I had more time I would implement a better suite of tests, and implement Redis to cache results of the gRPC services. Redis was not chosen as the main database due to it being an in-memory database and the requirement was the data should be persisted.

## Authentication
Although an optional task is to create an account management process, I would recommend leaving that to an external federated identity provider such as AzureAD, and connect via OIDC or ADFS using the Microsoft identity platform.
Then providing access and any additional policies can be handler via the IdP. I have included the configuration for connecting using OIDC with the Microsoft identity platform.

I have removed the authentication attributes due to a lack of a test IdP, however below is where they would be placed.

* ShelvesService.cs

	[Authorize]
	public class ShelvesService : Shelves.ShelvesBase

* StockKeepingUnitService.cs

	[Authorize]
    public class StockKeepingUnitService : Products.ProductsBase
	 
* Program.cs
 
	builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
        {
            builder.Configuration.Bind("AzureAd", options);
        },
        options => builder.Configuration.Bind("AzureAd", options));

    builder.Services.AddAuthorization();

    var app = builder.Build();

    app.UseAuthentication();
    app.UseAuthorization();



## Getting Started
Before running tests, ensure the ShelfLayoutManagement.Server executable is up and running to provide the necessary gRPC servers for shelves and SKUs. This server facilitates communication between components, enabling seamless interaction with the shelves and their respective SKUs. By ensuring the server is operational, you can run both the provided tests and create custom tests to validate the system's functionality.

Having the ShelfLayoutManagement.Server executable active is essential as it acts as the backbone, enabling the execution of various test cases. Ensure a stable connection to this server to facilitate accurate testing and validation of your application's features.


## Deploy
In the solution items, you'll find several YAML files designed to set up your application and a MongoDB instance within Kubernetes:

* app-deployment.yaml
This file configures the deployment of your application. The current replica count is set to 1, but you can scale it up as needed. This YAML file pulls in the Docker image generated by ShelfLayoutManagement.Server.

* mongodb-deployment.yaml
The database configuration is implemented as a StatefulSet, ensuring stable and persistent storage across deployments.

* pvc.yaml
This file defines the Persistent Volume Claim for your application.

To deploy your application in Kubernetes, follow these steps:

* Apply the application deployment configuration:
kubectl apply -f app-deployment.yaml

* Apply the MongoDB deployment configuration:
kubectl apply -f mongodb-deployment.yaml

* Apply the Persistent Volume Claim configuration:
kubectl apply -f pvc.yaml

These instructions set up your application and its associated database, enabling seamless operation within the Kubernetes environment. You can further adjust the deployment settings and scale the application according to your requirements.

## Technologies
* [ASP.NET Core gRPC](https://learn.microsoft.com/en-us/aspnet/core/grpc/?view=aspnetcore-6.0)
* [MongoDB](https://www.mongodb.com/)
* [NUnit](https://nunit.org/)
* [Moq](https://github.com/moq)

## Log
Here is an estimate of how long it took for this solution. 

ShelfLayoutManagement.Server - 6 hours
ShelfLayoutManagement.Common - 2 hours (mostly due to refactoring the proto files)
ShelfLayoutManagement.Data - 3 hours
ShelfLayoutManagement.Tests - 2 hours