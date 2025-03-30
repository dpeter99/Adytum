#!/bin/bash
# install_copr.sh - Enable and install COPR repositories

section "Enabling COPR repositories"
COPR_REPOS=$(yq -r '.packages.copr[]' "$PROFILE_FILE" 2>/dev/null || echo "")

if [ -n "$COPR_REPOS" ]; then
  for repo in $COPR_REPOS; do
    subtask "Enabling COPR repository: $repo"
    spinner "Enabling COPR repository" sudo dnf copr enable -y "$repo" &> /dev/null

    if [ $? -eq 0 ]; then
      task "Enabled COPR repository: $repo"
    else
      warning "Failed to enable COPR repository: $repo"
    fi
  done
else
  info "No COPR repositories specified"
fi
