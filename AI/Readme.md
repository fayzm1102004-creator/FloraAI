# FloraAI - Machine Learning Models

This directory contains the machine learning models and training notebooks responsible for diagnosing plant diseases from leaf images. 

## Supported Pathogens

The models are trained to classify images into the following categories:
* Bacteria
* Fungi
* Healthy
* Pests
* Virus

## Model Architectures

We experimented with and generated models using three different architectures to balance accuracy and performance:
* **EfficientNetB0**
* **MobileNetV2**
* **ResNet50**

## Directory Structure

* `EfficientNetB0/`: Contains the `.ipynb` training notebook, `.keras` model, and the quantized `.tflite` model for on-device inference.
* `MobileNetV2/`: Contains the `.ipynb` training notebook, `.keras` model, and the quantized `.tflite` model.
* `ResNet50/`: Contains the Jupyter notebook used for training the ResNet50 variant.

## Usage

The mobile application utilizes the `.tflite` files for lightweight, on-device predictions. If you wish to retrain the models:

1. Install the required Python packages (e.g., TensorFlow, Keras, Pandas, NumPy).
2. Ensure you have the plant disease dataset downloaded locally
dataset link: https://www.kaggle.com/datasets/kanishk3813/pathogen-dataset
3. Run the respective `.ipynb` notebooks in Jupyter or Google Colab.
