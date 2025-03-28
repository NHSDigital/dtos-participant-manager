module "postgresql_flexible_db" {
  for_each = var.postgresql != {} ? var.regions : {}

  source = "../../../dtos-devops-templates/infrastructure/modules/postgresql-flexible"

  name                = module.regions_config[each.key].names.postgres-sql-server
  resource_group_name = azurerm_resource_group.core[each.key].name
  location            = each.key

  backup_retention_days           = var.postgresql.backup_retention_days
  geo_redundant_backup_enabled    = var.postgresql.geo_redundant_backup_enabled
  postgresql_admin_object_id      = data.azuread_group.postgres_sql_admin_group.object_id
  postgresql_admin_principal_name = var.postgresql.postgres_sql_admin_group
  postgresql_admin_principal_type = "Group"
  public_network_access_enabled   = var.postgresql.public_network_access_enabled

  # Diagnostic Settings
  log_analytics_workspace_id                                = data.terraform_remote_state.audit.outputs.log_analytics_workspace_id[local.primary_region]
  monitor_diagnostic_setting_postgresql_server_enabled_logs = local.monitor_diagnostic_setting_postgresql_server_enabled_logs
  monitor_diagnostic_setting_postgresql_server_metrics      = local.monitor_diagnostic_setting_postgresql_server_metrics

  password_auth_enabled = false
  administrator_login   = "parman_db_user"
  # key_vault_id                    = module.key_vault[each.key].key_vault_id
  # key_vault_admin_pwd_secret_name = "DATABASE-PASSWORD"

  sku_name     = var.postgresql.dbs.commgt.sku_name
  storage_mb   = var.postgresql.dbs.commgt.storage_mb
  storage_tier = var.postgresql.dbs.commgt.storage_tier

  server_version = var.postgresql.server_version
  tenant_id      = data.azurerm_client_config.current.tenant_id
  zone           = var.postgresql.zone

  # postgresql_configurations
  postgresql_configurations = {}

  # Private Endpoint Configuration if enabled
  private_endpoint_properties = var.features.private_endpoints_enabled ? {
    private_dns_zone_ids_sql             = [data.terraform_remote_state.hub.outputs.private_dns_zones["${each.key}-postgres_sql"].id]
    private_endpoint_enabled             = var.features.private_endpoints_enabled
    private_endpoint_subnet_id           = module.subnets["${module.regions_config[each.key].names.subnet}-pep"].id
    private_endpoint_resource_group_name = azurerm_resource_group.rg_private_endpoints[each.key].name
    private_service_connection_is_manual = var.features.private_service_connection_is_manual
  } : null

  databases = {
    db1 = {
      collation   = "en_US.utf8"
      charset     = "UTF8"
      max_size_gb = 10
      name        = "participant_database"
    }
  }

  depends_on = [
    module.peering_spoke_hub,
    module.peering_hub_spoke
  ]

  tags = var.tags
}
