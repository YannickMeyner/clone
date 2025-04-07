output "webserver_ip" {
  description = "IP address of the web server"
  value       = openstack_networking_floatingip_v2.public_ip.address
  sensitive   = false
}
