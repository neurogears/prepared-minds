# Video Preparation

To allow for interactive real-time manipulation and super-imposition of video frames, each of the videos to be used needs to be prepared and calibrated. The short guide below outlines the required steps.

## Prerequisites

All video conversion steps require [FFmpeg](https://ffmpeg.org/). The easiest way to install on Windows is to download the latest essentials binary release build at [Gyan.dev GitHub](https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-github). Copy the `ffmpeg.exe` and `ffprobe.exe` files from the `bin` folder in the zip file into the `Videos` folder. Make sure all the videos you would like to prepare are also in the `Videos` folder.

## Split audio tracks

To ensure correct playback of audio together with the super-imposed video, we need to split the original audio track from the video into a separate WAV file. This can be done with the following command, where `video` should be replaced with the name of the specific video file we want to convert:

```
ffmpeg -i video.mp4 video.wav
```

We also need to gather the playback rate of the video (e.g. 25 fps) and sample rate of the audio (e.g. 48000 Hz). To find them, we can use the following command:

```
ffprobe video.mp4
```
