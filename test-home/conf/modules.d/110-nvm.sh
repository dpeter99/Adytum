#!/bin/bash

# Source helper functions
if [ -f "${LIB_DIR}/utils.sh" ]; then
  source "${LIB_DIR}/utils.sh"
else
  echo "Helper functions not found! Please install them first."
  exit 1
fi

# Check if NVM is already installed
if [ -d "$HOME/.nvm" ]; then
  info "NVM is already installed"
else
  # Install NVM
  subtask "Installing NVM"
  # Create a temporary file with mktemp
  temp_file=$(mktemp)
  debug "Using temporary file: $temp_file"

  spinner "Downloading NVM installer" curl -o $temp_file https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.2/install.sh

  if [ $? -ne 0 ]; then
    error "Failed to download NVM installer"
    exit 1
  fi

  spinner "Installing NVM" bash /tmp/nvm-install.sh

  if [ $? -ne 0 ]; then
    error "Failed to install NVM"
    exit 1
  fi

  task "NVM installed successfully"
fi

# Verify installation
if [ -f "$HOME/.config/nvm/nvm.sh" ]; then
  success_banner "NVM setup complete"
else
  error "NVM installation verification failed"
  exit 1
fi