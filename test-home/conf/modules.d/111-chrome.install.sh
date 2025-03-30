#!/bin/bash

# Source helper functions
if [ -f "${LIB_DIR}/utils.sh" ]; then
  source "${LIB_DIR}/utils.sh"
else
  echo "Helper functions not found! Please install them first."
  exit 1
fi

# Check if Chrome is already installed
if check_package "google-chrome-stable"; then
  info "Google Chrome is already installed"

  # Check for updates
  subtask "Checking for updates"
  spinner "Checking for Chrome updates" "dnf check-update google-chrome-stable &>/dev/null"

  if [ $? -eq 0 ]; then
    info "Google Chrome is up to date"
  else
    subtask "Updating Google Chrome"
    spinner "Updating Google Chrome" "dnf update -y google-chrome-stable"
    task "Google Chrome updated successfully"
  fi
else
  # Install Chrome
  section "Installing Google Chrome"
  install_rpm_url "https://dl.google.com/linux/direct/google-chrome-stable_current_x86_64.rpm"
fi

# Verify installation
if check_package "google-chrome-stable"; then
  success_banner "Google Chrome setup complete"
else
  error "Google Chrome installation failed"
  exit 1
fi