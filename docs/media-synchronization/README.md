# Media syncrhonization

## Getting Started

The GStreamer pipelines (where BDK relies on to extract the participants' camera feed) share the same internal clock and base time in order to keep synchronized. This base time is set with the current system time of the first extraction and then is shared with the next extractions.

In this document we will explain how we tested the synchronization between different streams, some expected results and findings.

## Setup
To play the streams and compare their running times, we used _gst-play_ (one instance per stream) and OBS Studio as main tools.

To run those tools, we used a PC with the following characteristics:

* i7 8700 non-k (6 cores - 12 threads)
* 16 Gb of RAM
* RTX 2080 8 GB VRAM (?? Hardware decoding ??)

To run BDK and extract 4 participants at the same time, we used an Azure virtual machine with the following the *Standard F16s_v2* sku (16 vCpus, 32 Gb of RAM)

## Tests

We've invited the BDK bot into a Microsoft Teams meeting and started the extraction of:
* Dominant Speaker
* Participant
* Together Mode (the user must enable inside the meeting)
* Screen Share (the user must share his screen)

### Gst-play

{{PENDING}}
### OBS Studio

Media soruces configuration (no-buffering, etc)


### Findings

Issues with the tools over time, synchornization differences in ms, screen share drop framwes
{{PENDING}}