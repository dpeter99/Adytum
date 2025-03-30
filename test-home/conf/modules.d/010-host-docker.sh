#!/bin/bash

if [ ! -f /usr/local/bin/podman ]; then
    sudo ln -s /usr/bin/distrobox-host-exec /usr/local/bin/podman
fi

printf "#!/bin/sh \n exec /usr/local/bin/podman "\$\@" " | sudo tee /usr/local/bin/docker
sudo chmod +x /usr/local/bin/docker

podman completion -f ~/.config/fish/completions/podman.fish fish