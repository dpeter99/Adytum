#!/bin/bash
#
# Helper functions for setup scripts using gum
# Save this as /opt/setup/lib/helpers.sh
#

# Ensure gum is installed
function ensure_gum() {
  if ! command -v gum &> /dev/null; then
    echo "Installing gum for nice formatting..."
    # For Fedora/RHEL
    if command -v dnf &> /dev/null; then
      sudo dnf install -y dnf-plugins-core
      sudo dnf copr enable -y atim/gum
      sudo dnf install -y gum
    else
      echo "Unable to install gum - package manager not supported"
      exit 1
    fi
  fi
}

# Make sure gum is available
ensure_gum

# Define styled output functions using gum
function section() {
  echo ""
  gum style --foreground 45 --bold "$1"
  echo ""
}

function task() {
  gum style --foreground 46 "✓ $1"
}

function subtask() {
  gum style --foreground 39 "• $1"
}

function warning() {
  gum style --foreground 214 "! WARNING: $1"
}

function error() {
  gum style --foreground 196 "✗ ERROR: $1"
}

function info() {
  gum style --foreground 248 "i $1"
}

function debug() {
  if [ -n "$DEBUG" ]; then
    gum style --foreground 240 "DEBUG: $1"
  fi
}

function banner() {
  gum style \
    --foreground 212 --border-foreground 212 --border double \
    --align center --width 50 --margin "1 2" --padding "1 2" \
    "$1"
}

function success_banner() {
  gum style \
    --foreground 46 --background 22 --bold \
    --align center --width 50 --margin "1 2" --padding "0 2" \
    "$1"
}

function module_header() {
  gum style \
    --foreground 250 --border normal --border-foreground 250 \
    --width 70 --margin "0 0 1 0" --padding "0 1" --align center \
    "Module: $1"
}

function separator() {
  printf '%*s\n' "${COLUMNS:-$(tput cols)}" '' | tr ' ' -
}

function spinner() {
  local message="$1"
  shift

  gum spin --spinner dot --title "$message" --show-error -- "$@"
  return $?
}

function choose_items() {
  echo "$1" | tr ' ' '\n' | gum choose --no-limit --height 15 --selected-prefix "⦿ " --unselected-prefix "○ " --selected-foreground 46
}

function confirm() {
  gum confirm "$1"
  return $?
}

# Utility functions for common tasks

# Install RPM from URL
function install_rpm_url() {
  local url="$1"
  local rpm_name=$(basename "$url")
  local temp_dir=$(mktemp -d)
  local temp_file="$temp_dir/$rpm_name"

  debug "Downloading $url to $temp_dir as $rpm_name"

  subtask "Downloading $rpm_name"
  spinner "Downloading $rpm_name" curl -o "$temp_file" "$url"

  if [ $? -ne 0 ]; then
    error "Failed to download $rpm_name"
    #rm -rf "$temp_dir"
    return 1
  fi

  subtask "Installing $rpm_name"
  spinner "Installing $rpm_name" sudo dnf install -y "$temp_file"

  if [ $? -ne 0 ]; then
    error "Failed to install $rpm_name"
    rm -rf "$temp_dir"
    return 1
  fi

  task "Successfully installed $rpm_name"
  rm -rf "$temp_dir"
  return 0
}

# Check if a command exists
function check_command() {
  command -v "$1" &> /dev/null
  return $?
}

# Check if a package is installed
function check_package() {
  if command -v rpm &> /dev/null; then
    rpm -q "$1" &> /dev/null
  else
    warning "Unable to check if package $1 is installed"
    return 1
  fi
  return $?
}

# Install packages with DNF and visual feedback
function install_packages() {
  local packages="$1"

  if [ -z "$packages" ]; then
    info "No packages specified"
    return 0
  fi

  info "Installing the following packages:"
  choose_items "$packages"

  spinner "Installing packages" sudo dnf install -y $packages

  if [ $? -eq 0 ]; then
    task "Successfully installed all packages"
    return 0
  else
    warning "Some packages may have failed to install"
    return 1
  fi
}

# Enable COPR repository
function enable_copr() {
  local repo="$1"

  if [ -z "$repo" ]; then
    warning "No COPR repository specified"
    return 1
  fi

  subtask "Enabling COPR repository: $repo"
  spinner "Enabling COPR repository" sudo dnf copr enable -y "$repo"

  if [ $? -eq 0 ]; then
    task "Enabled COPR repository: $repo"
    return 0
  else
    warning "Failed to enable COPR repository: $repo"
    return 1
  fi
}