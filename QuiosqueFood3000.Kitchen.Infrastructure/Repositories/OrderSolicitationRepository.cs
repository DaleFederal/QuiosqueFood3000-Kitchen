
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using QuiosqueFood3000.Kitchen.Application.Interfaces;
using QuiosqueFood3000.Kitchen.Domain.Entities;
using System.Text.Json;

namespace QuiosqueFood3000.Kitchen.Infrastructure.Repositories;

public class OrderSolicitationRepository : IOrderSolicitationRepository
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly string _tableName;

    public OrderSolicitationRepository(IAmazonDynamoDB dynamoDbClient, IConfiguration configuration)
    {
        _dynamoDbClient = dynamoDbClient;
        _tableName = configuration.GetConnectionString("DynamoDB:OrderSolicitationTableName") ?? "OrderSolicitations";
        
        // Initialize table if it doesn't exist (for local development)
        InitializeTableAsync().GetAwaiter().GetResult();
    }

    public async Task<OrderSolicitation> Add(OrderSolicitation entity)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { S = entity.Id.ToString() },
            ["Status"] = new AttributeValue { N = ((int)entity.Status).ToString() },
            ["GenerateDate"] = new AttributeValue { S = entity.GenerateDate.ToString("O") },
            ["CustomerId"] = entity.CustomerId.HasValue ? new AttributeValue { S = entity.CustomerId.Value.ToString() } : new AttributeValue { NULL = true },
            ["AnonymousIdentification"] = !string.IsNullOrEmpty(entity.AnonymousIdentification) ? new AttributeValue { S = entity.AnonymousIdentification } : new AttributeValue { NULL = true },
            ["Products"] = new AttributeValue { S = JsonSerializer.Serialize(entity.Products) }
        };

        if (entity.DeliveredDate.HasValue)
        {
            item["DeliveredDate"] = new AttributeValue { S = entity.DeliveredDate.Value.ToString("O") };
        }

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDbClient.PutItemAsync(request);
        return entity;
    }

    public async Task<OrderSolicitation> Update(OrderSolicitation entity)
    {
        return await Add(entity); // DynamoDB PutItem acts as upsert
    }

    public async Task<OrderSolicitation?> GetById(Guid id)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id.ToString() }
            }
        };

        var response = await _dynamoDbClient.GetItemAsync(request);
        
        if (!response.IsItemSet)
            return null;

        return MapFromDynamoDbItem(response.Item);
    }

    public async Task<IEnumerable<OrderSolicitation>> GetAll()
    {
        var request = new ScanRequest
        {
            TableName = _tableName
        };

        var response = await _dynamoDbClient.ScanAsync(request);
        
        return response.Items.Select(MapFromDynamoDbItem).ToList();
    }

    private static OrderSolicitation MapFromDynamoDbItem(Dictionary<string, AttributeValue> item)
    {
        var orderSolicitation = new OrderSolicitation
        {
            Id = Guid.Parse(item["Id"].S),
            Status = (Domain.Enums.OrderStatus)int.Parse(item["Status"].N),
            GenerateDate = DateTime.Parse(item["GenerateDate"].S),
            Products = JsonSerializer.Deserialize<List<Product>>(item["Products"].S) ?? new List<Product>()
        };

        if (item.ContainsKey("DeliveredDate") && !item["DeliveredDate"].NULL)
        {
            orderSolicitation.DeliveredDate = DateTime.Parse(item["DeliveredDate"].S);
        }

        if (item.ContainsKey("CustomerId") && !item["CustomerId"].NULL)
        {
            orderSolicitation.CustomerId = Guid.Parse(item["CustomerId"].S);
        }

        if (item.ContainsKey("AnonymousIdentification") && !item["AnonymousIdentification"].NULL)
        {
            orderSolicitation.AnonymousIdentification = item["AnonymousIdentification"].S;
        }

        return orderSolicitation;
    }

    private async Task InitializeTableAsync()
    {
        try
        {
            await _dynamoDbClient.DescribeTableAsync(_tableName);
        }
        catch (ResourceNotFoundException)
        {
            // Table doesn't exist, create it
            var createTableRequest = new CreateTableRequest
            {
                TableName = _tableName,
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "Id",
                        KeyType = KeyType.HASH
                    }
                },
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = "Id",
                        AttributeType = ScalarAttributeType.S
                    }
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            await _dynamoDbClient.CreateTableAsync(createTableRequest);
            
            // Wait for table to be active
            await WaitForTableToBeActiveAsync();
        }
    }

    private async Task WaitForTableToBeActiveAsync()
    {
        var maxAttempts = 30;
        var attempt = 0;
        
        while (attempt < maxAttempts)
        {
            try
            {
                var response = await _dynamoDbClient.DescribeTableAsync(_tableName);
                if (response.Table.TableStatus == TableStatus.ACTIVE)
                {
                    return;
                }
            }
            catch (ResourceNotFoundException)
            {
                // Table still being created
            }
            
            await Task.Delay(1000); // Wait 1 second
            attempt++;
        }
        
        throw new TimeoutException($"Table {_tableName} did not become active within the expected time.");
    }
}
