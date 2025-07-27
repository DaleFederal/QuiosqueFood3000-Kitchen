# Terraform Infrastructure for QuiosqueFood3000 Kitchen

This directory contains Terraform configuration files to provision AWS infrastructure for the QuiosqueFood3000 Kitchen API.

## Architecture Overview

The Terraform configuration creates the following AWS resources:

### Networking
- **VPC** with public and private subnets across 2 availability zones
- **Internet Gateway** for public subnet internet access
- **NAT Gateways** for private subnet outbound connectivity
- **Route Tables** and associations

### Compute & Container Orchestration
- **ECS Fargate Cluster** for container orchestration
- **ECS Service** with auto-scaling capabilities
- **ECS Task Definition** with health checks
- **Application Load Balancer** with target groups

### Storage & Security
- **ECR Repository** for container images
- **Secrets Manager** for MongoDB connection string
- **CloudWatch Log Groups** for application logging
- **IAM Roles** with least privilege access

### Monitoring & Auto-scaling
- **CloudWatch Alarms** for CPU utilization
- **Application Auto Scaling** policies
- **Container Insights** enabled

## Prerequisites

1. **AWS CLI** configured with appropriate credentials
2. **Terraform** >= 1.0 installed
3. **MongoDB connection string** (Atlas or DocumentDB)

## Setup Instructions

### 1. Backend Setup

First, create the S3 bucket and DynamoDB table for Terraform state management:

```bash
# Make the script executable and run it
chmod +x setup-backend.sh
./setup-backend.sh
```

### 2. Configure Variables

Copy the example variables file and customize it:

```bash
cp terraform.tfvars.example terraform.tfvars
```

Edit `terraform.tfvars` with your specific values:

```hcl
aws_region    = "us-east-1"
project_name  = "QuiosqueFood3000Kitchen"
environment   = "production"
vpc_cidr      = "10.0.0.0/16"

# Update with your MongoDB connection string
mongodb_connection_string = "mongodb+srv://username:password@cluster.mongodb.net/QuiosqueFood3000Kitchen"

# Container configuration
container_cpu    = 512
container_memory = 1024
app_count        = 2
```

### 3. Initialize and Deploy

```bash
# Initialize Terraform
terraform init

# Validate configuration
terraform validate

# Plan the deployment
terraform plan

# Apply the configuration
terraform apply
```

## GitHub Actions Integration

The infrastructure is managed through GitHub Actions workflows:

### Terraform Workflow (`.github/workflows/terraform.yml`)

**Triggers:**
- Push to `main` branch (auto-apply)
- Pull requests (plan only)
- Manual workflow dispatch

**Features:**
- Terraform plan/apply/destroy
- Security scanning with Checkov
- Cost estimation with Infracost
- Pull request comments with plan details

**Required Secrets:**
- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`
- `MONGODB_CONNECTION_STRING`
- `INFRACOST_API_KEY` (optional, for cost estimation)

### Workflow Actions

1. **Automatic on PR**: Runs `terraform plan` and posts results as PR comment
2. **Automatic on main**: Runs `terraform apply` when changes are merged
3. **Manual**: Trigger plan/apply/destroy through GitHub Actions UI

## File Structure

```
terraform/
├── main.tf              # Main infrastructure configuration
├── variables.tf         # Input variables
├── outputs.tf          # Output values
├── ecs.tf              # ECS-specific resources
├── terraform.tfvars.example  # Example variables file
├── setup-backend.sh    # Backend setup script
└── README.md          # This file
```

## Resource Outputs

After successful deployment, Terraform outputs:

- **ALB URL**: Load balancer endpoint for the API
- **ECR Repository URL**: Container registry URL
- **ECS Cluster Name**: Cluster name for deployments
- **VPC/Subnet IDs**: Network resource identifiers

## Security Features

- **VPC isolation** with private subnets for ECS tasks
- **Security groups** with minimal required access
- **IAM roles** with least privilege principles
- **Secrets Manager** for sensitive configuration
- **Encrypted S3 backend** for state storage
- **Security scanning** in CI/CD pipeline

## Cost Optimization

- **Fargate Spot** capacity provider for cost savings
- **Auto-scaling** based on CPU utilization
- **ECR lifecycle policies** to manage image storage
- **CloudWatch log retention** policies

## Monitoring

- **CloudWatch Container Insights** for cluster monitoring
- **Application Load Balancer** health checks
- **ECS service** health monitoring
- **Auto-scaling alarms** for performance management

## Troubleshooting

### Common Issues

1. **Backend bucket doesn't exist**
   ```bash
   ./setup-backend.sh
   ```

2. **Permission errors**
   - Verify AWS credentials have required permissions
   - Check IAM policies for Terraform operations

3. **State lock errors**
   ```bash
   terraform force-unlock <LOCK_ID>
   ```

4. **Resource conflicts**
   - Check for existing resources with same names
   - Verify region and account settings

### Useful Commands

```bash
# Show current state
terraform show

# List all resources
terraform state list

# Import existing resource
terraform import aws_instance.example i-1234567890abcdef0

# Refresh state
terraform refresh

# Destroy specific resource
terraform destroy -target=aws_instance.example
```

## Maintenance

### Regular Tasks

1. **Update Terraform version** in workflows
2. **Review and rotate secrets** in Secrets Manager
3. **Monitor costs** through AWS Cost Explorer
4. **Update container images** through CI/CD
5. **Review security scan results**

### Scaling

To scale the application:

1. **Horizontal scaling**: Increase `app_count` variable
2. **Vertical scaling**: Increase `container_cpu` and `container_memory`
3. **Auto-scaling**: Adjust CloudWatch alarm thresholds

## Support

For infrastructure issues:
1. Check Terraform plan output
2. Review CloudWatch logs
3. Verify AWS resource status
4. Check GitHub Actions workflow logs
