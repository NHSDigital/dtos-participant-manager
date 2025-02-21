application           = "parman"
application_full_name = "participant-manager"
environment           = "NFT"

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
    address_space     = "10.121.0.0/16"
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
        priority              = 801
        action                = "Allow"
        rule_name             = "ParmanToAudit"
        source_addresses      = ["10.115.0.0/16"] # will be populated with the serins manager subnet address space
        destination_addresses = ["10.116.0.0/16"] # will be populated with the audit subnet address space
        protocols             = ["TCP", "UDP"]
        destination_ports     = ["443"]
      },
      {
        name                  = "AllowAuditToParman"
        priority              = 811
        action                = "Allow"
        rule_name             = "AuditToParman"
        source_addresses      = ["10.116.0.0/16"]
        destination_addresses = ["10.115.0.0/16"]
        protocols             = ["TCP", "UDP"]
        destination_ports     = ["443"]
      }
    ]
    route_table_routes_to_audit = [
      {
        name                   = "ParmanToAudit"
        address_prefix         = "10.116.0.0/16"
        next_hop_type          = "VirtualAppliance"
        next_hop_in_ip_address = "" # will be populated with the Firewall Private IP address
      }
    ]
    route_table_routes_from_audit = [
      {
        name                   = "AuditToParman"
        address_prefix         = "10.115.0.0/16"
        next_hop_type          = "VirtualAppliance"
        next_hop_in_ip_address = "" # will be populated with the Firewall Private IP address
      }
    ]
  }
}
