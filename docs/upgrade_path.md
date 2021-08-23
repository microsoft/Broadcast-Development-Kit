# Upgrade path

Here you can find a quick summary of the configuration changes required to upgrade from a previous [pre-release version](https://github.com/microsoft/Broadcast-Development-Kit/releases) to the next one.

## From 0.4.0-dev to 0.5.0-dev

While all the configuration settings are the same between versions **0.4.0-dev** and **0.5.0-dev**, the later includes support for extracting feeds using RTMP/S in pull mode. This requires to apply some changes in the list of ports available in your virtual machine, as well as the configuration of the NGINX server that is used to listen to RTMP/S connections.

- Please review the [inbound rules](how-to-run-the-solution-in-azure/virtual_machine.md#network-security-group-inbound-rules) listed in the documentation and update your VM to match those rules.

- Check the latest [settings for the NGINX server](common/install_and_configure_nginx_with_rtmp_module_on_windows.md#installation) and update your NGINX configuration file in the VM to match these settings.

The version **0.5.0-dev** also included an slate image used by the bot when no injection is active. Optionally, you can [change the slate image to one of your choosing](common/customize_bdk_slate_image.md).
