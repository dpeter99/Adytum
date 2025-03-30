#!/bin/bash
# install_packages.sh - Install packages defined in the profile

section "Installing packages"
PACKAGES=$(yq -r '.packages.install[]' "$PROFILE_FILE" 2>/dev/null || echo "")

if [ -n "$PACKAGES" ]; then
  info "Installing the following packages:"

  # Show package list with gum
  echo "$PACKAGES" | tr ' ' '\n' | gum choose --no-limit --height 15 --selected-prefix "⦿ " --unselected-prefix "○ " --selected.foreground 46

  spinner "Installing packages" sudo dnf install -y $PACKAGES

  if [ $? -eq 0 ]; then
    task "Successfully installed all packages"
  else
    warning "Some packages may have failed to install"
  fi
else
  info "No packages specified"
fi
