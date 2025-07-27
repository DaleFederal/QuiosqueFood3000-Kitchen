# ECS Task Definition
resource "aws_ecs_task_definition" "app" {
  family                   = "${var.project_name}-${var.environment}-task"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = var.container_cpu
  memory                   = var.container_memory
  execution_role_arn       = data.aws_iam_role.lab_role.arn
  task_role_arn            = data.aws_iam_role.lab_role.arn

  container_definitions = jsonencode([
    {
      name      = "kitchen-api"
      image     = "${aws_ecr_repository.app.repository_url}:latest"
      essential = true

      portMappings = [
        {
          containerPort = var.container_port
          protocol      = "tcp"
        }
      ]

      environment = [
        {
          name  = "ASPNETCORE_ENVIRONMENT"
          value = "Production"
        },
        {
          name  = "ASPNETCORE_URLS"
          value = "http://+:${var.container_port}"
        }
      ]

      # No secrets needed for DynamoDB (assuming IAM roles are used)


      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.app.name
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "ecs"
        }
      }

      healthCheck = {
        command = [
          "CMD-SHELL",
          "curl -f http://localhost:${var.container_port}/health || exit 1"
        ]
        interval    = 30
        timeout     = 5
        retries     = 3
        startPeriod = 60
      }
    }
  ])

  tags = {
    Name = "${var.project_name}-${var.environment}-task-definition"
  }
}

# ECS Service
resource "aws_ecs_service" "main" {
  name            = "${var.project_name}-${var.environment}-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.app.arn
  desired_count   = var.app_count
  launch_type     = "FARGATE"

  network_configuration {
    security_groups  = [aws_security_group.ecs_tasks.id]
    subnets          = aws_subnet.private[*].id
    assign_public_ip = false
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.app.arn
    container_name   = "kitchen-api"
    container_port   = var.container_port
  }

  depends_on = [aws_lb_listener.app]

  tags = {
    Name = "${var.project_name}-${var.environment}-service"
  }

  lifecycle {
    ignore_changes = [task_definition]
  }
}

# Auto Scaling
resource "aws_appautoscaling_target" "ecs_target" {
  max_capacity       = 10
  min_capacity       = 1
  resource_id        = "service/${aws_ecs_cluster.main.name}/${aws_ecs_service.main.name}"
  scalable_dimension = "ecs:service:DesiredCount"
  service_namespace  = "ecs"
}

resource "aws_appautoscaling_policy" "ecs_policy_up" {
  name               = "${var.project_name}-${var.environment}-scale-up"
  policy_type        = "StepScaling"
  resource_id        = aws_appautoscaling_target.ecs_target.resource_id
  scalable_dimension = aws_appautoscaling_target.ecs_target.scalable_dimension
  service_namespace  = aws_appautoscaling_target.ecs_target.service_namespace

  step_scaling_policy_configuration {
    adjustment_type         = "ChangeInCapacity"
    cooldown                = 60
    metric_aggregation_type = "Maximum"

    step_adjustment {
      metric_interval_lower_bound = 0
      scaling_adjustment          = 1
    }
  }
}

resource "aws_appautoscaling_policy" "ecs_policy_down" {
  name               = "${var.project_name}-${var.environment}-scale-down"
  policy_type        = "StepScaling"
  resource_id        = aws_appautoscaling_target.ecs_target.resource_id
  scalable_dimension = aws_appautoscaling_target.ecs_target.scalable_dimension
  service_namespace  = aws_appautoscaling_target.ecs_target.service_namespace

  step_scaling_policy_configuration {
    adjustment_type         = "ChangeInCapacity"
    cooldown                = 60
    metric_aggregation_type = "Maximum"

    step_adjustment {
      metric_interval_upper_bound = 0
      scaling_adjustment          = -1
    }
  }
}

# CloudWatch Alarms
resource "aws_cloudwatch_metric_alarm" "cpu_high" {
  alarm_name          = "${var.project_name}-${var.environment}-cpu-utilization-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "2"
  metric_name         = "CPUUtilization"
  namespace           = "AWS/ECS"
  period              = "60"
  statistic           = "Average"
  threshold           = "80"
  alarm_description   = "This metric monitors ecs cpu utilization"
  alarm_actions       = [aws_appautoscaling_policy.ecs_policy_up.arn]

  dimensions = {
    ServiceName = aws_ecs_service.main.name
    ClusterName = aws_ecs_cluster.main.name
  }

  tags = {
    Name = "${var.project_name}-${var.environment}-cpu-high-alarm"
  }
}

resource "aws_cloudwatch_metric_alarm" "cpu_low" {
  alarm_name          = "${var.project_name}-${var.environment}-cpu-utilization-low"
  comparison_operator = "LessThanThreshold"
  evaluation_periods  = "2"
  metric_name         = "CPUUtilization"
  namespace           = "AWS/ECS"
  period              = "60"
  statistic           = "Average"
  threshold           = "10"
  alarm_description   = "This metric monitors ecs cpu utilization"
  alarm_actions       = [aws_appautoscaling_policy.ecs_policy_down.arn]

  dimensions = {
    ServiceName = aws_ecs_service.main.name
    ClusterName = aws_ecs_cluster.main.name
  }

  tags = {
    Name = "${var.project_name}-${var.environment}-cpu-low-alarm"
  }
}
