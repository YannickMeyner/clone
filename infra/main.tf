data "openstack_images_image_v2" "image" {
  name = "Debian Bookworm 12 (SWITCHengines)"
}

data "openstack_networking_network_v2" "private" {
  name = "private"
}

data "openstack_networking_secgroup_v2" "default" {
  name = "default"
}

resource "openstack_networking_secgroup_v2" "ssh" {
  name        = "ssh"
  description = "Allow SSH"
}

resource "openstack_networking_secgroup_rule_v2" "ssh" {
  direction         = "ingress"
  ethertype         = "IPv4"
  protocol          = "tcp"
  port_range_min    = 22
  port_range_max    = 22
  security_group_id = openstack_networking_secgroup_v2.ssh.id
}

resource "openstack_networking_secgroup_v2" "web" {
  name        = "web"
  description = "Allow HTTP and HTTPS"
}

resource "openstack_networking_secgroup_rule_v2" "http" {
  direction         = "ingress"
  ethertype         = "IPv4"
  protocol          = "tcp"
  port_range_min    = 80
  port_range_max    = 80
  security_group_id = openstack_networking_secgroup_v2.web.id
}

resource "openstack_networking_secgroup_rule_v2" "https" {
  direction         = "ingress"
  ethertype         = "IPv4"
  protocol          = "tcp"
  port_range_min    = 443
  port_range_max    = 443
  security_group_id = openstack_networking_secgroup_v2.web.id
}

resource "openstack_networking_floatingip_v2" "public_ip" {
  pool = "public"
}

resource "openstack_networking_port_v2" "public" {
  network_id = data.openstack_networking_network_v2.private.id
  security_group_ids = [
    data.openstack_networking_secgroup_v2.default.id,
    openstack_networking_secgroup_v2.ssh.id,
    openstack_networking_secgroup_v2.web.id,
  ]
  admin_state_up = true
}

resource "openstack_networking_floatingip_associate_v2" "public_ip" {
  floating_ip = openstack_networking_floatingip_v2.public_ip.address
  port_id     = openstack_networking_port_v2.public.id
}

resource "openstack_compute_instance_v2" "webserver" {
  name        = "webserver"
  image_id    = data.openstack_images_image_v2.image.id
  flavor_name = "m1.large"
  user_data   = templatefile("${path.module}/cloud-config.tftpl", { ssh_user = var.ssh_user })
  block_device {
    uuid                  = data.openstack_images_image_v2.image.id
    source_type           = "image"
    destination_type      = "volume"
    delete_on_termination = true
    volume_size           = 20
    volume_type           = "ceph-ssd"
  }
  network {
    port = openstack_networking_port_v2.public.id
  }
  provisioner "remote-exec" {
    inline = ["echo 'Hello World'"]
    connection {
      type        = "ssh"
      user        = var.ssh_user
      agent       = true
      host        = openstack_networking_floatingip_v2.public_ip.address
    }
  }
  provisioner "local-exec" {
    command = "ansible-playbook -i '${openstack_networking_floatingip_v2.public_ip.address},' --user ${var.ssh_user} --private-key ${var.private_key_path} playbook.yml"
  }
}
