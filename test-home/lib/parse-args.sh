#!/bin/bash
# parse_args.sh - Handle command line arguments and environment setup

# Parse arguments
while [[ $# -gt 0 ]]; do
  case $1 in
  -d | --debug)
    export DEBUG=yes
    debug "DEBUG is set to $DEBUG"
    shift # past argument
    ;;
  -p | --profile)
    export PROFILE="$2"
    shift # past argument
    shift # past value
    ;;
  *)
    # unknown option
    warning "Unknown option: $1"
    shift # past argument
    ;;
  esac
done

# Set system information
export HOST=$HOSTNAME

# If we're in a container, use container ID as profile
# TODO: Needs to not overwrite the user specified profile
if ! [[ -z $CONTAINER_ID ]]; then
  debug "CONTAINER_ID is set to $CONTAINER_ID"
  PROFILE="${CONTAINER_ID}"
fi

# If no profile specified, prompt user to select one
if [[ -z $PROFILE ]]; then
  # Get available profiles
  AVAILABLE_PROFILES=$(ls "${CONF_DIR}/profiles.d/" | sed 's/\.toml$//')
  
  if [[ -z $AVAILABLE_PROFILES ]]; then
    error "No profiles found in ${CONF_DIR}/profiles.d/"
    exit 1
  fi
  
  echo "Please select a profile:"
  PROFILE=$(echo "$AVAILABLE_PROFILES" | tr ' ' '\n' | gum choose --height 10)
  
  if [[ -z $PROFILE ]]; then
    error "No profile selected"
    exit 1
  fi
fi

export PROFILE
debug "Using profile: $PROFILE"
