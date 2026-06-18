import 'dart:io';
import 'package:tflite_flutter/tflite_flutter.dart';
import 'package:image/image.dart' as img;

class PlantInferenceService {
  Interpreter? _validatorInterpreter;
  Interpreter? _diseaseInterpreter;

  // ← لو أي مرض فوق 25% نقول مريضة حتى لو healthy أعلى
  static const double _diseaseThreshold = 0.25;

  Future<void> initModels() async {
    try {
      _validatorInterpreter = await Interpreter.fromAsset(
        'assets/leaf_validator.tflite',
      );
      _diseaseInterpreter = await Interpreter.fromAsset(
        'assets/disease_classifier.tflite',
      );
      print("✅ Models loaded successfully");
    } catch (e) {
      print("❌ Failed to load models: $e");
    }
  }

  Future<String> processImage(File imageFile) async {
    if (_validatorInterpreter == null || _diseaseInterpreter == null) {
      return "ERROR: Models not initialized";
    }

    final bytes = await imageFile.readAsBytes();
    img.Image? originalImage = img.decodeImage(bytes);
    if (originalImage == null) return "ERROR: Could not decode image";

    var inputTensor = _preprocessImage(originalImage);

    // Stage 1: التأكد إنها ورقة نبات
    var validatorOutput = List.generate(1, (_) => List.filled(2, 0.0));
    _validatorInterpreter!.run(inputTensor, validatorOutput);

    double leafConfidence = validatorOutput[0][0];
    print("🌿 Leaf Confidence: ${(leafConfidence * 100).toStringAsFixed(2)}%");

    if (leafConfidence < 0.80) return "INVALID_IMAGE";

    // Stage 2: تحديد المرض
    var diseaseOutput = List.generate(1, (_) => List.filled(5, 0.0));
    _diseaseInterpreter!.run(inputTensor, diseaseOutput);

    return _smartDetection(diseaseOutput[0]);
  }

  List<List<List<List<double>>>> _preprocessImage(img.Image image) {
    img.Image resized = img.copyResize(image, width: 224, height: 224);
    return List.generate(
      1,
      (_) => List.generate(
        224,
        (y) => List.generate(224, (x) {
          final pixel = resized.getPixel(x, y);
          return [
            (pixel.r / 127.5) - 1.0,
            (pixel.g / 127.5) - 1.0,
            (pixel.b / 127.5) - 1.0,
          ];
        }),
      ),
    );
  }

  String _smartDetection(List<dynamic> output) {
    double bacteria = (output[0] as num).toDouble();
    double fungi = (output[1] as num).toDouble();
    double healthy = (output[2] as num).toDouble();
    double pests = (output[3] as num).toDouble();
    double virus = (output[4] as num).toDouble();

    print("🔍 Scores:");
    print("  bacteria: ${(bacteria * 100).toStringAsFixed(2)}%");
    print("  fungi:    ${(fungi * 100).toStringAsFixed(2)}%");
    print("  healthy:  ${(healthy * 100).toStringAsFixed(2)}%");
    print("  pests:    ${(pests * 100).toStringAsFixed(2)}%");
    print("  virus:    ${(virus * 100).toStringAsFixed(2)}%");

    // ابحث عن أعلى مرض
    Map<String, double> diseases = {
      'bacteria': bacteria,
      'fungi': fungi,
      'pests': pests,
      'virus': virus,
    };

    String topDisease = 'healthy';
    double topScore = 0.0;
    diseases.forEach((label, score) {
      if (score > topScore) {
        topScore = score;
        topDisease = label;
      }
    });

    print(
      "🦠 Top disease: $topDisease (${(topScore * 100).toStringAsFixed(2)}%)",
    );

    // لو أعلى مرض فوق الـ threshold → مريضة
    if (topScore >= _diseaseThreshold) {
      print("⚠️ RESULT: DISEASED → $topDisease");
      return topDisease;
    }

    print("✅ RESULT: HEALTHY");
    return 'healthy';
  }

  void dispose() {
    _validatorInterpreter?.close();
    _diseaseInterpreter?.close();
  }
}
