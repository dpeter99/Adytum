#!/bin/bash

. "${SCRIPT_DIR}"/conf/base.sh

print_step "Adding work packages"

dnf_submit_install

run_module "host-docker-expose"

export SHELL_TO_USE=/bin/fish
setup_shell
