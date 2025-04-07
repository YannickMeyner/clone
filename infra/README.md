# Switch Engines

This project is deployed on Switch Engines.

## Getting Started

### Prerequisites

- [Terraform](https://www.terraform.io/downloads.html)

### Configuration

To connect the Terraform OpenStack provider to Switch Engines, you need to create `clouds.yaml` in `~/.config/openstack/` with the following content.
Or append the `woweb` section to the existing `clouds.yaml` file.

```yaml
clouds:
  woweb:
    auth:
      auth_url: https://keystone.cloud.switch.ch:5000/v3
      username: "<your email here>"
      password: "<api credentials here>"
      project_id: fbfd6ca1065d43598332e705b970647a
      project_name: "Woweb_Tetris_Gruppe_3"
      user_domain_name: "Default"
    region_name: ZH
    interface: "public"
    identity_api_version: 3
```

## Provisioning

To provision the required infrastructure, run the following commands:

```bash
cd infra
terraform init
terraform apply     # confirm with "yes"

# to destroy the infrastructure:
terraform destroy   # confirm with "yes"
```