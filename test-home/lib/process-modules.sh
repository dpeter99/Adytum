#!/bin/bash
# process_modules.sh - Process and execute modules defined in the profile

section "Processing modules"
MODULES=$(yq -r '.modules.enabled[]' "$PROFILE_FILE" 2>/dev/null || echo "")

if [ -n "$MODULES" ]; then
  # Create an array to store modules with their prefixes
  declare -a MODULE_LIST=()

  # Collect module paths and their numeric prefixes
  for module in $MODULES; do
    # Extract the numeric prefix if it exists, otherwise use "999" to place at the end
    PREFIX=$(echo "$module" | grep -o '^[0-9]\+' || echo "999")

    # Handle modules listed without their numeric prefix (like "nvm" instead of "110-nvm")
    if [[ "$PREFIX" == "999" ]]; then
      # Search for matching module in modules.d directory
      FOUND_MODULE=$(find "${CONF_DIR}/modules.d/" -name "*-${module}.sh" | sort | head -1)
      if [[ -n "$FOUND_MODULE" ]]; then
        # Extract the actual prefix from the found module
        PREFIX=$(basename "$FOUND_MODULE" | grep -o '^[0-9]\+' || echo "999")
        module=$(basename "$FOUND_MODULE" .sh)
      fi
    fi

    MODULE_PATH="${CONF_DIR}/modules.d/$module.sh"

    # Check if module has a .install.sh extension
    if [[ ! -f "$MODULE_PATH" && "$module" == *".install" ]]; then
      MODULE_PATH="${CONF_DIR}/modules.d/$module.sh"
    fi

    if [ -f "$MODULE_PATH" ]; then
      # Add module to array with prefix for sorting
      MODULE_LIST+=("$PREFIX:$MODULE_PATH")
    else
      error "Module installation script not found: $MODULE_PATH"
    fi
  done

  # Sort the modules array (requires bash 4.0+)
  IFS=$'\n' SORTED_MODULES=($(sort <<<"${MODULE_LIST[*]}"))
  unset IFS

  # Process modules in sorted order
  for module_entry in "${SORTED_MODULES[@]}"; do
    # Split the prefix and path
    prefix="${module_entry%%:*}"
    module_path="${module_entry#*:}"

    module_name=$(basename "$module_path" .sh)
    info "Installing module: $module_name"

    module_header "$module_name"

    if bash "$module_path"; then
      task "Successfully installed module: $module_name"
    else
      warning "Module installation may have issues: $module_name"
    fi

    # Draw a separator line
    separator
  done
else
  info "No modules specified"
fi