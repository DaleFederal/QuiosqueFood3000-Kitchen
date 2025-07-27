using Amazon.DynamoDBv2;
using FluentValidation;
using FluentValidation.AspNetCore;
using QuiosqueFood3000.Kitchen.Application.Validators;
using Microsoft.Extensions.DependencyInjection;
using QuiosqueFood3000.Kitchen.Application.Interfaces;
using QuiosqueFood3000.Kitchen.Application.Services;
using QuiosqueFood3000.Kitchen.Infrastructure.Repositories;

namespace QuiosqueFood3000.Kitchen.Api.IoC;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure AWS DynamoDB
        var serviceUrl = configuration.GetValue<string>("DynamoDB:ServiceURL");
        if (!string.IsNullOrEmpty(serviceUrl))
        {
            // Local DynamoDB configuration
            services.AddSingleton<IAmazonDynamoDB>(provider =>
            {
                var config = new AmazonDynamoDBConfig
                {
                    ServiceURL = serviceUrl,
                    UseHttp = true
                };
                return new AmazonDynamoDBClient("local", "local", config);
            });
        }
        else
        {
            // AWS DynamoDB configuration
            services.AddAWSService<IAmazonDynamoDB>();
        }
        
        // Configure DynamoDB settings
        services.Configure<DynamoDBOptions>(configuration.GetSection("DynamoDB"));

        services.AddScoped<IOrderSolicitationRepository, OrderSolicitationRepository>();
        services.AddScoped<IOrderSolicitationService, OrderSolicitationService>();
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<OrderSolicitationValidator>();
    }
}

public class DynamoDBOptions
{
    public string OrderSolicitationTableName { get; set; } = "OrderSolicitations";
    public string Region { get; set; } = "us-east-1";
}