==================================================
FloraAI - Complete Installation Instructions
==================================================

This guide provides step-by-step instructions to run the FloraAI project. 
The project consists of a .NET 10 Backend API and a Flutter Mobile App.

--------------------------------------------------
Step 0: Clone the Repository
--------------------------------------------------
First, clone the project to your local machine and navigate into the folder.
Open your terminal and run:

```bash
git clone https://github.com/fayzm1102004-creator/FloraAI.git
cd FloraAI
```

--------------------------------------------------
Step 1: Setup and Run the Backend (.NET 10)
--------------------------------------------------
Ensure you have the .NET 10 SDK and PostgreSQL installed before proceeding.

1. Navigate to the backend directory:
```bash
cd back
```

2. Restore the required .NET packages:
```bash
dotnet restore
```

3. Navigate to the API project folder:
```bash
cd FloraAI.API
```

4. Apply the database migrations:
```bash
dotnet ef database update
```

5. Run the API server:
```bash
dotnet run
```
The backend server is now running! Keep this terminal window open.

--------------------------------------------------
Step 2: Setup and Run the Frontend (Flutter)
--------------------------------------------------
Ensure you have the Flutter SDK installed and an Android/iOS Emulator running (or a physical device connected). 
Open a NEW terminal window for these steps.

1. Navigate to the Flutter directory (from the root FloraAI folder):
```bash
cd Flutterr
```

2. Download all the required Flutter packages:
```bash
flutter pub get
```

3. Run the Flutter application:
```bash
flutter run
```

Done! The mobile app is now running and connected to the backend.
