terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 2.26"
    }
    random = {
      source = "hashicorp/random"
      version = "3.1.0"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "prirg" {
  name     = "jobfit-carlot-rg"
  location = var.location
}

resource "random_integer" "ri" {
  min = 10000
  max = 99999
}

resource "azurerm_storage_account" "funcstrg" {
  name                     = "jfcarlotstrg01"
  resource_group_name      = azurerm_resource_group.prirg.name
  location                 = azurerm_resource_group.prirg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_cosmosdb_account" "cosmos" {
  name                      = "jobfit-carlot-${random_integer.ri.result}-db"
  resource_group_name       = azurerm_resource_group.prirg.name
  location                  = azurerm_resource_group.prirg.location
  offer_type                = "Standard"
  kind                      = "MongoDB"
  enable_automatic_failover = true
  enable_free_tier = true

  capabilities {
    name = "EnableAggregationPipeline"
  }

  capabilities {
    name = "mongoEnableDocLevelTTL"
  }

  capabilities {
    name = "MongoDBv3.4"
  }

  consistency_policy {
    consistency_level       = "BoundedStaleness"
    max_interval_in_seconds = 10
    max_staleness_prefix    = 200
  }

  geo_location {
    location          = azurerm_resource_group.prirg.location
    failover_priority = 0
  }
}

resource "azurerm_cosmosdb_mongo_database" "mongodb" {
  name                = "jobfit-carlot-db"
  resource_group_name = azurerm_cosmosdb_account.cosmos.resource_group_name
  account_name        = azurerm_cosmosdb_account.cosmos.name
}

resource "azurerm_cosmosdb_mongo_collection" "mongocollection" {
  name                = "jobfit-carlot-db"
  resource_group_name = azurerm_cosmosdb_account.cosmos.resource_group_name
  account_name        = azurerm_cosmosdb_account.cosmos.name
  database_name       = azurerm_cosmosdb_mongo_database.mongodb.name

  default_ttl_seconds = "777"
  throughput          = 400
}

resource "azurerm_app_service_plan" "funcasp" {
  name                = "jobfit-carlot-funapp-01"
  location            = azurerm_resource_group.prirg.location
  resource_group_name = azurerm_resource_group.prirg.name

  sku {
    tier = "Free"
    size = "F1"
  }
}

resource "azurerm_app_service_source_control_token" "token" {
  type  = "GitHub"
  token = ""
}

resource "azurerm_function_app" "funcapp" {
  name                       = "jobfit-carlot-funcapp-01"
  location                   = azurerm_resource_group.prirg.location
  resource_group_name        = azurerm_resource_group.prirg.name
  app_service_plan_id        = azurerm_app_service_plan.funcasp.id
  storage_account_name       = azurerm_storage_account.funcstrg.name
  storage_account_access_key = azurerm_storage_account.funcstrg.primary_access_key
  version = "~3"
  source_control {
    repo_url = "https://github.com/kforsberg/AzureApiTest.git"
    branch = "master"
  }
  app_settings = {
    "mongoConnectionString" = azurerm_cosmosdb_account.cosmos.connection_strings[0]
  }
}
