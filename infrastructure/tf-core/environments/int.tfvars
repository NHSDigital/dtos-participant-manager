application           = "parman"
application_full_name = "participant-manager"
environment           = "INT"

features = {
  acr_enabled                          = false
  api_management_enabled               = false
  event_grid_enabled                   = false
  private_endpoints_enabled            = true
  private_service_connection_is_manual = false
  public_network_access_enabled        = false
}

tags = {
  Project = "Participant-Manager"
}

regions = {
  uksouth = {
    is_primary_region = true
    address_space     = "10.123.0.0/16"
    connect_peering   = true
    subnets = {
      apps = {
        cidr_newbits               = 8
        cidr_offset                = 2
        delegation_name            = "Microsoft.Web/serverFarms"
        service_delegation_name    = "Microsoft.Web/serverFarms"
        service_delegation_actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
      }
      pep = {
        cidr_newbits = 8
        cidr_offset  = 1
      }
      sql = {
        cidr_newbits = 8
        cidr_offset  = 3
      }
      webapps = {
        cidr_newbits               = 8
        cidr_offset                = 4
        delegation_name            = "Microsoft.Web/serverFarms"
        service_delegation_name    = "Microsoft.Web/serverFarms"
        service_delegation_actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
      }
      pep-dmz = {
        cidr_newbits = 8
        cidr_offset  = 5
      }
    }
  }
}

routes = {
  uksouth = {
    firewall_policy_priority = 100
    application_rules        = []
    nat_rules                = []
    network_rules = [
      {
        name                  = "AllowParmanToAudit"
        priority              = 831
        action                = "Allow"
        rule_name             = "ParmanToAudit"
        source_addresses      = ["10.117.0.0/16"] # will be populated with the participant manager subnet address space
        destination_addresses = ["10.118.0.0/16"] # will be populated with the audit subnet address space
        protocols             = ["TCP", "UDP"]
        destination_ports     = ["443"]
      },
      {
        name                  = "AllowAuditToParman"
        priority              = 832
        action                = "Allow"
        rule_name             = "AuditToParman"
        source_addresses      = ["10.118.0.0/16"]
        destination_addresses = ["10.117.0.0/16"]
        protocols             = ["TCP", "UDP"]
        destination_ports     = ["443"]
      }
    ]
    route_table_routes_to_audit = [
      {
        name                   = "ParmanToAudit"
        address_prefix         = "10.118.0.0/16"
        next_hop_type          = "VirtualAppliance"
        next_hop_in_ip_address = "" # will be populated with the Firewall Private IP address
      }
    ]
    route_table_routes_from_audit = [
      {
        name                   = "AuditToParman"
        address_prefix         = "10.117.0.0/16"
        next_hop_type          = "VirtualAppliance"
        next_hop_in_ip_address = "" # will be populated with the Firewall Private IP address
      }
    ]
  }
}

app_insights_name    = "appi-int-uks-parman"
app_insights_rg_name = "rg-parman-int-uks-audit"

app_service_plan = {
  os_type                  = "Linux"
  sku_name                 = "P2v3"
  vnet_integration_enabled = true

  autoscale = {
    memory_percentage = {
      metric = "MemoryPercentage"

      capacity_min = "1"
      capacity_max = "5"
      capacity_def = "1"

      time_grain       = "PT1M"
      statistic        = "Average"
      time_window      = "PT10M"
      time_aggregation = "Average"

      inc_operator        = "GreaterThan"
      inc_threshold       = 70
      inc_scale_direction = "Increase"
      inc_scale_type      = "ChangeCount"
      inc_scale_value     = 1
      inc_scale_cooldown  = "PT5M"

      dec_operator        = "LessThan"
      dec_threshold       = 25
      dec_scale_direction = "Decrease"
      dec_scale_type      = "ChangeCount"
      dec_scale_value     = 1
      dec_scale_cooldown  = "PT5M"
    }
  }

  instances = {
    Default = {}
    WebApp  = {}
    # BIAnalyticsDataService       = {}
    # BIAnalyticsService           = {}
    # DemographicsService          = {}
    # EpisodeDataService           = {}
    # EpisodeIntegrationService    = {}
    # EpisodeManagementService     = {}
    # MeshIntegrationService       = {}
    # ParticipantManagementService = {}
    # ReferenceDataService         = {}
  }
}

diagnostic_settings = {
  metric_enabled = true
}

function_apps = {
  acr_mi_name = "dtos-participant-manager-acr-push"
  acr_name    = "acrukshubdevparman"
  acr_rg_name = "rg-hub-dev-uks-parman"

  app_service_logs_disk_quota_mb         = 35
  app_service_logs_retention_period_days = 7

  always_on = true

  cont_registry_use_mi = true

  docker_CI_enable  = "true"
  docker_env_tag    = "integration"
  docker_img_prefix = "participant-manager"

  enable_appsrv_storage         = "false"
  ftps_state                    = "Disabled"
  https_only                    = true
  remote_debugging_enabled      = false
  storage_uses_managed_identity = null
  worker_32bit                  = false
  ip_restriction_default_action = "Deny"

  function_app_config = {

    ParticipantManager = {
      name_suffix            = "backend-api"
      function_endpoint_name = "ParticipantManager"
      app_service_plan_key   = "Default"
      db_connection_string   = "ParticipantManagerDatabaseConnectionString"
    }

    ParticipantManagerExperience = {
      name_suffix            = "experience-api"
      function_endpoint_name = "ParticipantManagerExperience"
      app_service_plan_key   = "Default"
      local_urls = {
        CRUD_API_URL = "https://%s-backend-api.azurewebsites.net"
      }
      env_vars_static = {
        AUTH_NHSLOGIN_ISSUER_URL = "https://auth.sandpit.signin.nhs.uk"
        AUTH_NHSLOGIN_CLIENT_ID  = "screening participant manager"
      }
    }
  }
}

function_app_slots = []

key_vault = {
  disk_encryption   = true
  soft_del_ret_days = 7
  purge_prot        = true
  sku_name          = "standard"
}

linux_web_app = {
  acr_mi_name = "dtos-participant-manager-acr-push"
  acr_name    = "acrukshubdevparman"
  acr_rg_name = "rg-hub-dev-uks-parman"

  app_insights_name    = "appi-int-uks-parman"
  app_insights_rg_name = "rg-parman-int-uks-audit"

  always_on = true

  cont_registry_use_mi = true

  docker_CI_enable  = "true"
  docker_env_tag    = "integration"
  docker_img_prefix = "participant-manager"

  enable_appsrv_storage    = "false"
  ftps_state               = "Disabled"
  https_only               = true
  remote_debugging_enabled = false
  worker_32bit             = false
  # storage_name             = "webappstor"
  # storage_type             = "AzureBlob"
  # share_name               = "webapp"

  linux_web_app_config = {

    FrontEndUi = {
      name_suffix          = "nextjs-frontend"
      app_service_plan_key = "WebApp"
      env_vars_static = {
        AUTH_NHSLOGIN_CLIENT_ID  = "screening participant manager"
        AUTH_NHSLOGIN_ISSUER_URL = "https://auth.sandpit.signin.nhs.uk"
        AUTH_TRUST_HOST          = true
        NEXTAUTH_URL             = "https://www-int.non-live.nationalscreening.nhs.uk/api/auth"
        SERVICE_NAME             = "Manage your screening"
      }
      local_urls = {
        EXPERIENCE_API_URL = "https://%s-experience-api.azurewebsites.net"
      }
      env_vars_from_key_vault = [
        {
          env_var_name          = "AUTH_NHSLOGIN_CLIENT_SECRET"
          key_vault_secret_name = "auth-nhslogin-client-secret"
        },
        {
          env_var_name          = "NEXTAUTH_SECRET"
          key_vault_secret_name = "nextauth-secret"
        }
      ]
    }
  }
}

linux_web_app_slots = []

sqlserver = {
  sql_uai_name                         = "dtos-participant-manager-sql-adm"
  sql_admin_group_name                 = "sqlsvr_parman_int_uks_admin"
  ad_auth_only                         = true
  auditing_policy_retention_in_days    = 30
  security_alert_policy_retention_days = 30

  server = {
    sqlversion                    = "12.0"
    tlsversion                    = 1.2
    azure_services_access_enabled = true
  }

  # parman database
  dbs = {
    parman = {
      db_name_suffix = "participant_database"
      collation      = "SQL_Latin1_General_CP1_CI_AS"
      licence_type   = "LicenseIncluded"
      max_gb         = 5
      read_scale     = false
      sku            = "S0"
    }
  }

  fw_rules = {}
}

storage_accounts = {
  fnapp = {
    name_suffix                   = "fnappstor"
    account_tier                  = "Standard"
    replication_type              = "LRS"
    public_network_access_enabled = false
    containers                    = {}
  }
}
