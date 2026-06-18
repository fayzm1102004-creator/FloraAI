# FloraAI

FloraAI is a comprehensive full-stack application designed to diagnose plant pathogens and diseases using Deep learning. By capturing an image of a plant leaf, users can receive instant, AI-driven diagnostics and care recommendations.

## Project Architecture

This repository is divided into three main components:

* **Frontend (`Flutterr/`)**: A cross-platform mobile application built with Flutter.
* **Backend (`back/`)**: A RESTful API built with .NET Core and PostgreSQL, handling user authentication, data storage, and integration with the Gemini AI service.
* **AI Models (`AI/`)**: Deep learning models trained to classify various plant diseases.

## Getting Started

To get a local copy up and running, please follow the instructions in the respective directories:

1. **Backend**: Navigate to the `back/` directory to configure your database and run the .NET API.
2. **AI**: Navigate to the `AI/` directory for details on the `.tflite` and `.keras` models.
3. **Frontend**: Navigate to the `Flutterr/` directory to build and run the mobile application.

## Prerequisites

* Flutter SDK
* .NET Core SDK
* PostgreSQL
* Python 3.x (for running Jupyter notebooks or retraining models)
