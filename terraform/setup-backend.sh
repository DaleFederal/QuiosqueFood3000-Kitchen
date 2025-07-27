#!/bin/bash

# Script to set up Terraform backend resources (S3 bucket and DynamoDB table)
# Run this script before running terraform init

set -e

AWS_REGION="us-east-1"
BUCKET_NAME="quiosquefood3000-terraform-state"
DYNAMODB_TABLE="terraform-state-lock"

echo "Setting up Terraform backend resources..."

# Create S3 bucket for Terraform state
echo "Creating S3 bucket: $BUCKET_NAME"
aws s3api create-bucket \
    --bucket $BUCKET_NAME \
    --region $AWS_REGION \
    --create-bucket-configuration LocationConstraint=$AWS_REGION 2>/dev/null || echo "Bucket already exists"

# Enable versioning on the bucket
echo "Enabling versioning on S3 bucket"
aws s3api put-bucket-versioning \
    --bucket $BUCKET_NAME \
    --versioning-configuration Status=Enabled

# Enable server-side encryption
echo "Enabling server-side encryption on S3 bucket"
aws s3api put-bucket-encryption \
    --bucket $BUCKET_NAME \
    --server-side-encryption-configuration '{
        "Rules": [
            {
                "ApplyServerSideEncryptionByDefault": {
                    "SSEAlgorithm": "AES256"
                }
            }
        ]
    }'

# Block public access
echo "Blocking public access on S3 bucket"
aws s3api put-public-access-block \
    --bucket $BUCKET_NAME \
    --public-access-block-configuration \
        BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true

# Create DynamoDB table for state locking
echo "Creating DynamoDB table: $DYNAMODB_TABLE"
aws dynamodb create-table \
    --table-name $DYNAMODB_TABLE \
    --attribute-definitions AttributeName=LockID,AttributeType=S \
    --key-schema AttributeName=LockID,KeyType=HASH \
    --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 \
    --region $AWS_REGION 2>/dev/null || echo "Table already exists"

echo "Waiting for DynamoDB table to be active..."
aws dynamodb wait table-exists --table-name $DYNAMODB_TABLE --region $AWS_REGION

echo "Backend resources created successfully!"
echo "You can now run 'terraform init' to initialize your Terraform configuration."
