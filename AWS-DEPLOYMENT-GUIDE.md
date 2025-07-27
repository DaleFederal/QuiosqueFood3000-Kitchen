# AWS Deployment Guide for QuiosqueFood3000 Kitchen

## Overview
This guide provides step-by-step instructions to deploy the QuiosqueFood3000 Kitchen API to AWS using GitHub Actions, ECS Fargate, and supporting services.

## Prerequisites
- AWS Account with appropriate permissions
- GitHub repository with the code
- Domain name (optional, for custom SSL)

## AWS Services Used
- **ECS Fargate**: Container orchestration
- **ECR**: Container registry
- **Application Load Balancer**: Load balancing and SSL termination
- **VPC**: Network isolation
- **CloudWatch**: Logging and monitoring
- **Secrets Manager**: Secure configuration storage
- **DynamoDB**: Managed NoSQL database (replaces MongoDB/DocumentDB)

## Step-by-Step Setup

### 1. AWS Account Setup

#### Create IAM User for GitHub Actions
```bash
# Create IAM user
aws iam create-user --user-name github-actions-quiosquefood3000

# Create access key
aws iam create-access-key --user-name github-actions-quiosquefood3000
```

#### Attach Required Policies
```bash
# Attach policies for ECR, ECS, CloudFormation
aws iam attach-user-policy --user-name github-actions-quiosquefood3000 --policy-arn arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryFullAccess
aws iam attach-user-policy --user-name github-actions-quiosquefood3000 --policy-arn arn:aws:iam::aws:policy/AmazonECS_FullAccess
aws iam attach-user-policy --user-name github-actions-quiosquefood3000 --policy-arn arn:aws:iam::aws:policy/CloudFormationFullAccess
aws iam attach-user-policy --user-name github-actions-quiosquefood3000 --policy-arn arn:aws:iam::aws:policy/IAMFullAccess
aws iam attach-user-policy --user-name github-actions-quiosquefood3000 --policy-arn arn:aws:iam::aws:policy/AmazonVPCFullAccess
aws iam attach-user-policy --user-name github-actions-quiosquefood3000 --policy-arn arn:aws:iam::aws:policy/SecretsManagerReadWrite
```

### 2. GitHub Repository Setup

#### Add GitHub Secrets
Go to your GitHub repository → Settings → Secrets and variables → Actions

Add the following secrets:
- `AWS_ACCESS_KEY_ID`: From step 1
- `AWS_SECRET_ACCESS_KEY`: From step 1
- `AWS_ACCOUNT_ID`: Your AWS account ID

### 3. Database Setup: DynamoDB

#### Create DynamoDB Table
```bash
aws dynamodb create-table \
    --table-name OrderSolicitations \
    --attribute-definitions AttributeName=Id,AttributeType=S \
    --key-schema AttributeName=Id,KeyType=HASH \
    --billing-mode PAY_PER_REQUEST \
    --region us-east-1
```

No additional secrets are needed for DynamoDB unless you use IAM roles for fine-grained access control. The application will read the table name and region from environment variables or configuration.
### 4. Deploy Infrastructure

#### Update Configuration Files
1. Replace `ACCOUNT_ID` in `aws/task-definition.json` with your AWS account ID
2. Update DynamoDB table name in `aws/infrastructure.yml` and application environment variables

#### Deploy CloudFormation Stack
```bash
# Deploy infrastructure
aws cloudformation deploy \
    --template-file aws/infrastructure.yml \
    --stack-name quiosquefood3000-kitchen-infrastructure \
    --parameter-overrides \
        ProjectName=QuiosqueFood3000Kitchen \
        Environment=production \
    --capabilities CAPABILITY_IAM \
    --region us-east-1
```

### 5. Configure Application

#### Update DynamoDB Table Name in Environment

Set the following environment variables for the ECS Task or in your deployment configuration:
- `DynamoDB__OrderSolicitationTableName=OrderSolicitations`
- `AWS_REGION=us-east-1`
```

#### Add Health Check Endpoint
Add this to your .NET API (if not already present):

```csharp
// In Program.cs or Startup.cs
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));
```

### 6. Deploy Application

#### Push to Main Branch
```bash
git add .
git commit -m "Add AWS deployment configuration"
git push origin main
```

The GitHub Actions workflow will automatically:
1. Run tests
2. Build Docker image
3. Push to ECR
4. Update ECS service
5. Deploy new version

### 7. Verify Deployment

#### Check ECS Service
```bash
# Check service status
aws ecs describe-services \
    --cluster quiosquefood3000-cluster \
    --services quiosquefood3000-kitchen-service
```

#### Test API Endpoint
```bash
# Get load balancer URL from CloudFormation outputs
aws cloudformation describe-stacks \
    --stack-name quiosquefood3000-kitchen-infrastructure \
    --query 'Stacks[0].Outputs[?OutputKey==`LoadBalancerURL`].OutputValue' \
    --output text

# Test health endpoint
curl http://your-load-balancer-url/health
```

### 8. Domain and SSL Setup (Optional)

#### Register Domain in Route 53
```bash
# Create hosted zone
aws route53 create-hosted-zone \
    --name yourdomain.com \
    --caller-reference $(date +%s)
```

#### Request SSL Certificate
```bash
# Request certificate
aws acm request-certificate \
    --domain-name api.yourdomain.com \
    --validation-method DNS \
    --region us-east-1
```

#### Update Load Balancer
Add HTTPS listener to the load balancer with the SSL certificate.

### 9. Monitoring and Logging

#### CloudWatch Dashboard
```bash
# Create custom dashboard for monitoring
aws cloudwatch put-dashboard \
    --dashboard-name QuiosqueFood3000Kitchen \
    --dashboard-body file://aws/cloudwatch-dashboard.json
```

#### Set Up Alarms
```bash
# CPU utilization alarm
aws cloudwatch put-metric-alarm \
    --alarm-name "QuiosqueFood3000-HighCPU" \
    --alarm-description "High CPU utilization" \
    --metric-name CPUUtilization \
    --namespace AWS/ECS \
    --statistic Average \
    --period 300 \
    --threshold 80 \
    --comparison-operator GreaterThanThreshold \
    --evaluation-periods 2
```

## Cost Optimization

### Use Fargate Spot
Update task definition to use Fargate Spot for cost savings:
```json
{
  "capacityProviderStrategy": [
    {
      "capacityProvider": "FARGATE_SPOT",
      "weight": 1
    }
  ]
}
```

### Auto Scaling
Configure ECS service auto scaling based on CPU/memory metrics.

## Security Best Practices

1. **Use least privilege IAM policies**
2. **Enable VPC Flow Logs**
3. **Use AWS WAF for the load balancer**
4. **Enable GuardDuty for threat detection**
5. **Regular security updates for container images**

## Troubleshooting

### Common Issues

#### ECS Task Fails to Start
```bash
# Check task logs
aws logs get-log-events \
    --log-group-name /ecs/quiosquefood3000-kitchen \
    --log-stream-name ecs/kitchen-api/TASK_ID
```

#### Database Connection Issues
- Verify security groups allow traffic
- Check connection string in Secrets Manager
- Ensure database is accessible from ECS subnets

#### Load Balancer Health Checks Failing
- Verify health endpoint returns 200 status
- Check security group rules
- Ensure application listens on correct port

## Maintenance

### Regular Tasks
1. **Monitor CloudWatch metrics**
2. **Update container images regularly**
3. **Review and rotate secrets**
4. **Monitor costs and optimize**
5. **Backup database regularly**

### Scaling
- Adjust ECS service desired count
- Update task definition CPU/memory
- Consider using Application Auto Scaling

## Support
For issues with this deployment:
1. Check CloudWatch logs
2. Review ECS service events
3. Verify GitHub Actions workflow logs
4. Check AWS CloudFormation stack events
