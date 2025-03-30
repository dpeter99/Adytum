#!/bin/bash
# load_profile.sh - Load and validate the selected profile with simple inheritance support
# Create a temporary directory for profile processing
TEMP_DIR=$(mktemp -d)
EFFECTIVE_PROFILE_FILE="${TEMP_DIR}/effective-profile.yaml"

debug "Temporary directory: $EFFECTIVE_PROFILE_FILE"

# Function to parse a profile with inheritance
parse_profile() {
  local profile_name="$1"
  local profile_file="${CONF_DIR}/profiles.d/${profile_name}.yaml"

  # Check if profile exists
  if [ ! -f "$profile_file" ]; then
    error "Profile not found: $profile_name"
    return 1
  fi

  # Get parent profile (if any)
  local parent=$(yq -r '.inherit // ""' "$profile_file")

  if [ -n "$parent" ]; then
    info "Profile $profile_name inherits from: $parent"

    local parent_file="${CONF_DIR}/profiles.d/${parent}.yaml"

    # Check if parent exists
    if [ ! -f "$parent_file" ]; then
      error "Parent profile not found: $parent"
      return 1
    fi

    # Merge parent and child profiles using yq's ireduce with *+ operator
    # This properly merges arrays by concatenating them
    yq eval-all '. as $item ireduce ({}; . *+ $item)' "${parent_file}" "${profile_file}" > "$EFFECTIVE_PROFILE_FILE"
  else
    # No inheritance, just use the profile as-is
    cat "$profile_file" > "$EFFECTIVE_PROFILE_FILE"
  fi

  return 0
}

# Check if yq is installed
if ! command -v yq &> /dev/null; then
  warning "yq not found. Installing dependencies..."
  spinner "Installing yq" sudo dnf install -y yq
  task "Installed yq"
fi

# Parse the profile with inheritance
section "Loading profile: $PROFILE"
parse_profile "$PROFILE"

if [ $? -ne 0 ]; then
  error "Failed to load profile: $PROFILE"
  exit 1
fi

# Get profile information from the effective profile
PROFILE_NAME=$(yq -r '.name // "'"$PROFILE"'"' "$EFFECTIVE_PROFILE_FILE")
PROFILE_DESC=$(yq -r '.description // "No description provided"' "$EFFECTIVE_PROFILE_FILE")

section "Setting up profile: $PROFILE_NAME"
info "Description: $PROFILE_DESC"

# If we're in debug mode, show the effective profile
if [ -n "$DEBUG" ]; then
  debug "Effective profile content:"
  debug "$(cat "$EFFECTIVE_PROFILE_FILE")"
fi

# Export profile variables for use in other scripts
export PROFILE_NAME
export PROFILE_DESC
export PROFILE_FILE="$EFFECTIVE_PROFILE_FILE"  # Use the effective profile file instead of the original