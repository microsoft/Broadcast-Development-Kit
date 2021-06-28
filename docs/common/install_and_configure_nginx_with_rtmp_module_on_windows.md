# How to Install and configure NGINX with RTMP module on Windows

## Getting Started

The objective of this guide is to explain how to correctly install and configure NGINX with the RTMP module in Windows, how to inject a live broadcast locally, and how to run NGINX as a Windows service.

>  **NOTE**: We are going to use NGINX version 1.14.1. It has not been tested with recent versions.

## Installation

Download as a zip NGINX with RTMP module from the following [GitHub repository](https://github.com/illuspas/nginx-rtmp-win32).

![Nginix GitHub repository](../images/running_solution_in_azure/nginx_repository.png)

After that, unzip the file in a location of preference (e.g: C:\), open `nginx.config` file with your text editor of preferences and replace its content with the following code snippet:

```nginx
worker_processes  1;
error_log  logs/error.log;
error_log  logs/error.log  notice;
error_log  logs/error.log  info;
pid        logs/nginx.pid;
#pcre_jit on;
events {
    worker_connections  8192;
    # max value 32768, nginx recycling connections+registry optimization =
    #   this.value * 20 = max concurrent connections currently tested with one worker
    #   C1000K should be possible depending there is enough ram/cpu power
    # multi_accept on;
}

stream {
    upstream publish {
        server 127.0.0.1:29361;
    }
    server {
        listen 2936 ssl;        # additional port for publishing
        proxy_pass publish;
        ssl_certificate c:\\certs\\fullchain.pem;
        ssl_certificate_key c:\\certs\\privkey.pem;

       # allow 190.55.159.5;        # allow publish from this IP
        #allow 192.0.2.0/24;     # -- also supports CIDR notation!
        allow all;               # deny publish from the rest
    }

    upstream live {
        server 127.0.0.1:29351;
    }
    server {
        listen 2935 ssl;        # standard RTMP(S) port
        proxy_pass live;
        ssl_certificate c:\\certs\\fullchain.pem;
        ssl_certificate_key c:\\certs\\privkey.pem;

        allow all;              # this is public (this is also the default)
    }
}

rtmp {
    server {
        listen 127.0.0.1:29361;
        chunk_size 4096;

        application secure-ingest{
            live on;
            record off;

	    on_publish http://localhost/api/bot/validate-stream-key;

            allow publish 127.0.0.1;  # publishing through rtmps://rtmp.example.com:1936
            allow play 127.0.0.1;     # for the pull from rtmp://localhost:19351/live
        }
    }

    server {
        listen 127.0.0.1:29351;
        chunk_size 4096;

        application live {
            live on;
            record off;
            deny publish all;         # no need to publish on /live -- IMPORTANT!!!
            allow play 127.0.0.1;     # playing through rtmps://rtmp.example.com:1935/live

            pull rtmp://127.0.0.1:29361/secure-ingest;
        }
    }

    server {
        listen 1936;
        chunk_size 4096;
        application ingest {
            live on;
            record off;
	    
	    on_publish http://localhost/api/bot/validate-stream-key;
        }
    }
}

http {
    server {
        listen      8080;
		
        location / {
            root html;
        }
		
        location /stat {
            rtmp_stat all;
            rtmp_stat_stylesheet stat.xsl;
        }

        location /stat.xsl {
            root html;
        }
    }
}
```

## Test NGINX server configuration

To start testing the NGINX server, we must open the command line, navigate to the NGINX root folder and execute `nginx.exe`.

![Open Nginx](../images/running_solution_in_azure/open_nginx.png)

Once the server is running, we are going to locally test the RTMP endpoint by injecting content with GStreamer.

Open a new command line window, and execute the following command:

```bash
gst-device-monitor-1.0
```

### List of found devices

![Nginx gst device monitor](../images/running_solution_in_azure/nginx_device_monitor.png)

The command will prompt you with a list of input and output multimedia devices. We must identify the audio input device we will be using. We must search for a device with **Audio/Source** class where its **device.api** property equals to **wasapi**, and copy the value of **device.strid**.

![wasapi device](../images/running_solution_in_azure/nginx_wasapi_device.png)

Once we identified the device and copied its id value, we need to run the following command (GStreamer CLI pipeline) to start capturing the video from your webcam and the audio of the selected device, and process it so we can inject it as an RTMP stream into the RTMP server.

```bash
gst-launch-1.0 wasapisrc device="your-audio-device-input" ! audioconvert ! avenc_aac ! aacparse ! queue ! mux. autovideosrc ! video/x-raw, format=YUY2, width=320, height=180, framerate=30/1 ! videoconvert ! x264enc tune=zerolatency key-int-max=60 bitrate=2500 ! queue ! mux. flvmux name=mux streamable=true latency=500000000 ! rtmpsink location=rtmp://localhost:1935/injection/1
```

To validate the server and GStreamer are correctly working, we must start consuming the live endpoint with a player.

Copy the value of **device.strid** and replace it in the command given above. This command will start the injection of audio and video to the RTMP server, it only remains to start consuming it with a player.

**Using GStreamer**

```bash
gst-launch-1.0 rtmpsrc location=rtmp://{your-local-ip}:1935/injection/1 ! decodebin name=decoder ! queue ! videoconvert ! autovideosink decoder. ! queue ! audioconvert ! audioresample ! autoaudiosink
```
![Using GStreamer](../images/running_solution_in_azure/nginx_using_gstreamer.png)

**Ffplay**
```bash
ffplay.exe rtmp://{your-local-ip}:1935/injection/1
```
![Ffplay](../images/running_solution_in_azure/nginx_ffplay.png)

**VLC Player**

> **NOTE**: Make sure you have a sufficiently recent version of VLC. RTMP streaming is supported in VLC versions 1.1 and later; you can download the most recent version by pointing your browser to [videolan.org](https://www.videolan.org/) and clicking **Download VLC**. Follow the on-screen instructions to download and install the program.

Once VLC is installed, follow these steps:
1. Open VLC's ***Media*** menu and click **Open Network Stream**, or simply hold down **CTRL** and press **N**.

1. Paste the URL of the stream you want to watch, with the following format: `rtmp://{your-local-ip}:1935/injection/1` in the **Please enter a network URL** box.  
![](../images/running_solution_in_azure/nginx_vlc_player.png)

1. Click the **Play** button.

## Run NGINX as a Windows Service
In order to run NGINX as Windows Service is necessary to wrap the executable file into a Windows Service using an external program.

To do so, we can use the [nssm](https://nssm.cc/) tool and execute it in the PC or VM where we want to install the service.  Download the nssm tool  ([download link](https://nssm.cc/download)), unzip the package, and execute the tool from the command line.

Once the tool is open, set the path for the NGINX executable file and click on the **Install Service** button. After receiving the confirmation message the service is ready to start.

![Install nginx](../images/running_solution_in_azure/install_nginx.png)

The first time, the service does not run automatically. We must initialize the service from the Windows services manager or, restart the machine.

![](../images/running_solution_in_azure/nginx_as_windows_service.png)
