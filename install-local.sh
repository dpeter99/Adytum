#!/bin/bash
set -euo pipefail

# Adytum Local Installation Script
# Installs Adytum to ~/.local/bin for testing purposes
# Safe user-only installation - no system-wide changes

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
INSTALL_DIR="$HOME/.local/bin"
APP_NAME="adytum"
TEMP_BUILD_DIR="./local-install-build"

# Helper functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Function to check if directory is in PATH
is_in_path() {
    case ":$PATH:" in
        *":$1:"*) return 0 ;;
        *) return 1 ;;
    esac
}

# Function to show usage
show_usage() {
    echo "Usage: $0 [install|uninstall|status]"
    echo ""
    echo "Commands:"
    echo "  install     Build and install Adytum locally (default)"
    echo "  uninstall   Remove locally installed Adytum"
    echo "  status      Show installation status"
    echo ""
    echo "This script installs Adytum to ~/.local/bin for testing purposes."
}

# Function to check installation status
check_status() {
    log_info "Checking Adytum installation status..."
    
    if [[ -f "$INSTALL_DIR/$APP_NAME" ]]; then
        log_success "Adytum is installed at: $INSTALL_DIR/$APP_NAME"
        
        # Check version
        if command_exists "$APP_NAME" || [[ -x "$INSTALL_DIR/$APP_NAME" ]]; then
            echo "Version information:"
            "$INSTALL_DIR/$APP_NAME" version 2>/dev/null || echo "  (Unable to get version)"
        fi
        
        # Check PATH
        if is_in_path "$INSTALL_DIR"; then
            log_success "$INSTALL_DIR is in your PATH"
        else
            log_warning "$INSTALL_DIR is NOT in your PATH"
            echo "  You may need to run: export PATH=\"\$HOME/.local/bin:\$PATH\""
        fi
        
        return 0
    else
        log_info "Adytum is not installed locally"
        return 1
    fi
}

# Function to uninstall
uninstall() {
    log_info "Uninstalling Adytum..."
    
    if [[ -f "$INSTALL_DIR/$APP_NAME" ]]; then
        rm -f "$INSTALL_DIR/$APP_NAME"
        log_success "Adytum has been removed from $INSTALL_DIR"
    else
        log_warning "Adytum was not found at $INSTALL_DIR/$APP_NAME"
    fi
}

# Function to install
install() {
    log_info "Starting Adytum local installation..."
    
    # Check prerequisites
    if ! command_exists dotnet; then
        log_error ".NET SDK not found. Please install .NET 9.0 or later."
        exit 1
    fi
    
    # Check if we're in the right directory (should contain Adytum.sln)
    if [[ ! -f "Adytum.sln" ]]; then
        log_error "Adytum.sln not found. Please run this script from the Adytum project root directory."
        exit 1
    fi
    
    # Create install directory
    log_info "Creating installation directory: $INSTALL_DIR"
    mkdir -p "$INSTALL_DIR"
    
    # Clean previous temp build
    if [[ -d "$TEMP_BUILD_DIR" ]]; then
        log_info "Cleaning previous build directory..."
        rm -rf "$TEMP_BUILD_DIR"
    fi
    
    # Build and publish
    log_info "Building and publishing Adytum..."
    dotnet publish Adytum/Adytum.csproj \
        --configuration Release \
        --output "$TEMP_BUILD_DIR" \
        -p:PublishSingleFile=true \
        -p:PublishReadyToRun=true \
        --verbosity quiet
    
    if [[ $? -ne 0 ]]; then
        log_error "Build failed!"
        # Clean up on failure
        [[ -d "$TEMP_BUILD_DIR" ]] && rm -rf "$TEMP_BUILD_DIR"
        exit 1
    fi
    
    # Check if the executable was created
    if [[ ! -f "$TEMP_BUILD_DIR/Adytum" ]]; then
        log_error "Executable not found after build!"
        [[ -d "$TEMP_BUILD_DIR" ]] && rm -rf "$TEMP_BUILD_DIR"
        exit 1
    fi
    
    # Copy executable to install directory
    log_info "Installing to $INSTALL_DIR..."
    cp "$TEMP_BUILD_DIR/Adytum" "$INSTALL_DIR/$APP_NAME"
    chmod +x "$INSTALL_DIR/$APP_NAME"
    
    # Clean up temp build directory
    rm -rf "$TEMP_BUILD_DIR"
    
    log_success "Adytum has been installed to $INSTALL_DIR/$APP_NAME"
    
    # Check PATH and provide guidance
    if is_in_path "$INSTALL_DIR"; then
        log_success "$INSTALL_DIR is already in your PATH"
        log_info "You can now run: $APP_NAME version"
    else
        log_warning "$INSTALL_DIR is not in your PATH"
        echo ""
        echo "To use Adytum from anywhere, add this to your shell profile (~/.bashrc, ~/.zshrc, etc.):"
        echo "  export PATH=\"\$HOME/.local/bin:\$PATH\""
        echo ""
        echo "Or run Adytum with the full path:"
        echo "  $INSTALL_DIR/$APP_NAME version"
        echo ""
        echo "For this session only, you can run:"
        echo "  export PATH=\"\$HOME/.local/bin:\$PATH\""
    fi
    
    # Test the installation
    log_info "Testing installation..."
    if "$INSTALL_DIR/$APP_NAME" version >/dev/null 2>&1; then
        log_success "Installation test passed!"
    else
        log_error "Installation test failed!"
        exit 1
    fi
}

# Main script logic
COMMAND="${1:-install}"

case "$COMMAND" in
    "install")
        install
        ;;
    "uninstall")
        uninstall
        ;;
    "status")
        check_status
        ;;
    "-h"|"--help"|"help")
        show_usage
        ;;
    *)
        log_error "Unknown command: $COMMAND"
        echo ""
        show_usage
        exit 1
        ;;
esac