variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

variable "project_name" {
  description = "name of the project"
  type        = string
  default     = "QuiosqueFood3000Kitchen"
}

variable "environment" {
  description = "Environment (dev, staging, prod)"
  type        = string
  default     = "production"
}

variable "vpc_cidr" {
  description = "CIDR block for VPC"
  type        = string
  default     = "10.0.0.0/16"
}

variable "dynamodb_table_name" {
  # Example variables file for Terraform
  # Trigger workflow test
  # Another workflow trigger
  description = "DynamoDB table name for OrderSolicitation"
  type        = string
  default     = "OrderSolicitations"
}

variable "container_cpu" {
  description = "Fargate instance CPU units to provision (1 vCPU = 1024 CPU units)"
  type        = number
  default     = 512
}

variable "container_memory" {
  description = "Fargate instance memory to provision (in MiB)"
  type        = number
  default     = 1024
}

variable "app_count" {
  description = "Number of docker containers to run"
  type        = number
  default     = 2
}

variable "health_check_grace_period_seconds" {
  description = "Seconds to ignore failing load balancer health checks on newly instantiated tasks"
  type        = number
  default     = 30
}

variable "container_port" {
  description = "Port exposed by the docker image to redirect traffic to"
  type        = number
  default     = 8080
}
