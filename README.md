# prepared-minds
A pipeline for analysis and comparison of dance videos

## How to install

This project uses [DeepLabCut](https://github.com/DeepLabCut/DeepLabCut/) for labelling and training pose estimator models and [Bonsai](https://bonsai-rx.org/) for pre-processing and live inference.

### Prerequisites

Make sure to install the following before proceeding:

  * [CUDA Toolkit 11.2](https://developer.nvidia.com/cuda-11.2.0-download-archive) (only CUDA is needed)
  * cuDNN 8.1.1 from the [cuDNN Archive](https://developer.nvidia.com/rdp/cudnn-archive)

### DeepLabCut

The `python` folder contains self-bootstrapping scripts for installing and configuring a Python environment for training models using DeepLabCut.

If you already have a Python distribution, use `pip install -r requirements.txt` to install the required packages.

If you don't have any Python distribution, or would like to use a self-contained environment, run the `setup.cmd` from the command-line or `setup.ps1` from Powershell to download a portable installation of Python and bootstrap the environment.