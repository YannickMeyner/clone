variable "ssh_user" {
  type        = string
  default     = "woweb"
  description = "Name of the SSH user"
}

variable "private_key_path" {
  type        = string
  default     = "~/.ssh/id_ed25519"
  description = "Path to the private key on local machine"
}
