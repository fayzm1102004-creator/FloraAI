import 'dart:io';
import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';
import 'package:image_picker/image_picker.dart';
import 'package:dotted_border/dotted_border.dart';
import 'package:project/Disease_translations.dart';
import 'MyGarden.dart';
import 'services/api_service.dart';
import 'Plant_inference_service.dart';

class Botaipage extends StatefulWidget {
  const Botaipage({super.key});

  @override
  State<Botaipage> createState() => _BotaipageState();
}

class _BotaipageState extends State<Botaipage> {
  final ImagePicker picker = ImagePicker();
  final PlantInferenceService _inferenceService = PlantInferenceService();
  final TextEditingController nameController = TextEditingController();

  File? imagefile;
  bool showNameButton = false;
  bool showTreatmentBox = false;
  bool showSaveButton = false;
  bool isAnalyzing = false;
  String diseaseResult = "";
  String treatmentResult = "";
  Map<String, String> _careData = {};

  @override
  void initState() {
    super.initState();
    _inferenceService.initModels();
  }

  @override
  void dispose() {
    _inferenceService.dispose();
    nameController.dispose();
    super.dispose();
  }

  Future<void> pickImage(ImageSource source) async {
    final XFile? image = await picker.pickImage(
      source: source,
      imageQuality: 70,
    );
    if (image != null) {
      setState(() {
        imagefile = File(image.path);
        showNameButton = false;
        showTreatmentBox = false;
        showSaveButton = false;
        diseaseResult = "";
        treatmentResult = "";
        _careData = {};
      });
    }
  }

  Future<void> analyzeImage() async {
    if (imagefile == null) return;
    final lang = context.locale.languageCode;
    setState(() => isAnalyzing = true);

    final label = await _inferenceService.processImage(imagefile!);

    if (label == "INVALID_IMAGE") {
      setState(() {
        isAnalyzing = false;
        diseaseResult = 'invalid_image'.tr();
        showNameButton = false;
        showTreatmentBox = false;
        showSaveButton = false;
      });
      return;
    }

    if (label.startsWith("ERROR")) {
      setState(() {
        isAnalyzing = false;
        diseaseResult = 'analysis_error'.tr();
        showNameButton = false;
        showTreatmentBox = false;
        showSaveButton = false;
      });
      return;
    }

    final info = diseaseInfo[label] ?? diseaseInfo['healthy']!;

    final serverResult = await ApiService.scanPlant(
      plantType: info['plantType']!,
      conditionName: info['conditionName']!,
      detectedCategory: info['detectedCategory']!,
      languageCode: lang,
    );

    String finalDisease = info['disease']!;
    String finalTreatment = '';

    if (serverResult['success'] && serverResult['data'] != null) {
      final data = serverResult['data'];
      final t = data['treatment'] ?? '';
      if (t is String && t.isNotEmpty) {
        finalTreatment = t;
      }

      if (data['careAdvice'] is Map) {
        final advice = data['careAdvice'] as Map;
        _careData['watering'] = advice['watering']?.toString() ?? '';
        _careData['light'] = advice['light']?.toString() ?? '';
        _careData['fertilizing'] = advice['fertilizing']?.toString() ?? '';
        _careData['soil'] = advice['soil']?.toString() ?? '';
        _careData['humidity'] = advice['humidity']?.toString() ?? '';
      }
    }

    setState(() {
      isAnalyzing = false;
      diseaseResult = finalDisease;
      treatmentResult = finalTreatment;
      showNameButton = true;
      showTreatmentBox = false;
      showSaveButton = false;
    });
  }

  Future<void> _fetchPlantCare(String plantName) async {
    final lang = context.locale.languageCode;
    final result = await ApiService.getPlantCare(plantName, languageCode: lang);
    print("🌱 Care result: $result");

    if (result['success'] && result['data'] != null) {
      final data = result['data'];
      dynamic care;

      if (data is List && data.isNotEmpty) {
        care = data[0];
      } else if (data is Map) {
        care = data;
      }

      if (care is Map<String, dynamic>) {
        final treatment = care['treatment']?.toString() ?? '';
        final careInstructions = care['careInstructions']?.toString() ?? '';

        setState(() {
          _careData['treatment'] = treatment;
          _careData['careInstructions'] = careInstructions;

          if (care['careAdvice'] is Map) {
            final advice = care['careAdvice'] as Map;
            if (advice['watering'] != null)
              _careData['watering'] = advice['watering'].toString();
            if (advice['light'] != null)
              _careData['light'] = advice['light'].toString();
            if (advice['fertilizing'] != null)
              _careData['fertilizing'] = advice['fertilizing'].toString();
            if (advice['soil'] != null)
              _careData['soil'] = advice['soil'].toString();
            if (advice['humidity'] != null)
              _careData['humidity'] = advice['humidity'].toString();
          }
        });
        print("✅ Care data: $_careData");
      }
    }
  }

  void showNameSheet() {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    nameController.clear();
    showModalBottomSheet(
      isScrollControlled: true,
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) {
        return Container(
          color: isDark ? Colors.grey[850] : Colors.white,
          child: Padding(
            padding: EdgeInsets.only(
              top: 20,
              left: 20,
              right: 20,
              bottom: MediaQuery.of(context).viewInsets.bottom + 20,
            ),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(
                  'plant_name'.tr(),
                  style: TextStyle(
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                    color: isDark ? Colors.white : Colors.black,
                  ),
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: nameController,
                  autofocus: true,
                  textAlign: TextAlign.right,
                  style: TextStyle(color: isDark ? Colors.white : Colors.black),
                  decoration: InputDecoration(
                    hintText: 'plant_name_hint'.tr(),
                    hintStyle: TextStyle(color: Colors.grey[500]),
                    filled: true,
                    fillColor: isDark ? Colors.grey[800] : Colors.grey[100],
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: BorderSide.none,
                    ),
                    focusedBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: const BorderSide(
                        color: Color.fromARGB(255, 56, 114, 64),
                        width: 2,
                      ),
                    ),
                  ),
                ),
                const SizedBox(height: 16),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    style: ElevatedButton.styleFrom(
                      backgroundColor: const Color.fromARGB(255, 56, 114, 64),
                      foregroundColor: Colors.white,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                    onPressed: () async {
                      if (nameController.text.trim().isNotEmpty) {
                        Navigator.pop(context);
                        await _fetchPlantCare(nameController.text.trim());
                        setState(() {
                          final name = nameController.text.trim();
                          String replacePlant(String text) {
                            return text
                                .replaceAll(RegExp(r'\(\s*[Pp]lant\s*\)'), name)
                                .replaceAll(RegExp(r'\b[Pp]lant\b'), name);
                          }

                          if (_careData['treatment'] != null) {
                            _careData['treatment'] = replacePlant(
                              _careData['treatment']!,
                            );
                          }
                          if (_careData['careInstructions'] != null) {
                            _careData['careInstructions'] = replacePlant(
                              _careData['careInstructions']!,
                            );
                          }
                          treatmentResult = replacePlant(treatmentResult);

                          final fromTreatment = _careData['treatment'] ?? '';
                          final fromCareInstr =
                              _careData['careInstructions'] ?? '';
                          if (fromTreatment.isNotEmpty) {
                            treatmentResult = fromTreatment;
                          } else if (fromCareInstr.isNotEmpty) {
                            treatmentResult = fromCareInstr;
                          }
                          showTreatmentBox = true;
                          showSaveButton = true;
                          showNameButton = false;
                        });
                      }
                    },
                    child: Text('confirm'.tr()),
                  ),
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  void showimagepickeroptions() {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) {
        final isDark = Theme.of(context).brightness == Brightness.dark;
        return Container(
          color: isDark ? Colors.grey[850] : Colors.white,
          child: Padding(
            padding: const EdgeInsets.all(20),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.end,
                  children: [
                    IconButton(
                      iconSize: 20,
                      icon: const Icon(Icons.close),
                      onPressed: () => Navigator.pop(context),
                    ),
                  ],
                ),
                ListTile(
                  leading: Icon(
                    Icons.camera_alt,
                    color:
                        isDark
                            ? Colors.white70
                            : const Color.fromARGB(255, 56, 56, 56),
                  ),
                  title: Text(
                    'open_camera'.tr(),
                    style: TextStyle(
                      color:
                          isDark
                              ? Colors.white
                              : const Color.fromARGB(255, 89, 89, 89),
                    ),
                  ),
                  onTap: () {
                    Navigator.pop(context);
                    pickImage(ImageSource.camera);
                  },
                ),
                ListTile(
                  leading: Icon(
                    Icons.photo,
                    color:
                        isDark
                            ? Colors.white70
                            : const Color.fromARGB(255, 60, 60, 60),
                  ),
                  title: Text(
                    'choose_from_gallery'.tr(),
                    style: TextStyle(
                      color:
                          isDark
                              ? Colors.white
                              : const Color.fromARGB(255, 80, 80, 80),
                    ),
                  ),
                  onTap: () {
                    Navigator.pop(context);
                    pickImage(ImageSource.gallery);
                  },
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  Future<void> showAddPlantWithDiagnosisSheet(BuildContext context) async {
    final XFile? newImage = await picker.pickImage(
      source: ImageSource.gallery,
      imageQuality: 70,
    );
    if (newImage == null) return;

    final File newImageFile = File(newImage.path);
    final TextEditingController newNameController = TextEditingController(
      text: nameController.text,
    );

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) {
        final isDark = Theme.of(context).brightness == Brightness.dark;
        return Padding(
          padding: EdgeInsets.only(
            top: 20,
            left: 20,
            right: 20,
            bottom: MediaQuery.of(context).viewInsets.bottom + 20,
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Container(
                width: 40,
                height: 4,
                margin: const EdgeInsets.only(bottom: 16),
                decoration: BoxDecoration(
                  color: Colors.grey[300],
                  borderRadius: BorderRadius.circular(10),
                ),
              ),
              Text(
                'add_new_plant'.tr(),
                style: const TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                  color: Color.fromARGB(255, 56, 114, 64),
                ),
              ),
              const SizedBox(height: 16),
              ClipRRect(
                borderRadius: BorderRadius.circular(12),
                child: Image.file(
                  newImageFile,
                  height: 150,
                  width: double.infinity,
                  fit: BoxFit.cover,
                ),
              ),
              const SizedBox(height: 16),
              TextFormField(
                controller: newNameController,
                textAlign: TextAlign.start,
                style: TextStyle(color: isDark ? Colors.white : Colors.black),
                decoration: InputDecoration(
                  labelText: 'plant_name'.tr(),
                  filled: true,
                  fillColor: isDark ? Colors.grey[800] : Colors.white,
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                  focusedBorder: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: const BorderSide(
                      color: Color.fromARGB(255, 56, 114, 64),
                      width: 2,
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 20),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceAround,
                children: [
                  ElevatedButton(
                    style: ElevatedButton.styleFrom(
                      backgroundColor: const Color.fromARGB(255, 56, 114, 64),
                      foregroundColor: Colors.white,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                    onPressed: () async {
                      if (newNameController.text.trim().isNotEmpty) {
                        final newName = newNameController.text.trim();
                        if (diseaseResult.isNotEmpty) {
                          await ApiService.savePlant(
                            name: newName,
                            disease: diseaseResult,
                            treatment: treatmentResult,
                          );
                        }

                        setState(() {
                          final newPlant = plant(
                            image: newImageFile.path,
                            name: newName,
                          );

                          if (diseaseResult.isNotEmpty) {
                            final ci = _careData['careInstructions'] ?? '';
                            final tr = _careData['treatment'] ?? '';
                            newPlant.careInstructions = ci.isNotEmpty ? ci : tr;
                            newPlant.watering = _careData['watering'];
                            newPlant.light = _careData['light'];
                            newPlant.fertilizing = _careData['fertilizing'];
                            newPlant.soil = _careData['soil'];
                            newPlant.humidity = _careData['humidity'];

                            newPlant.healthRecords.add(
                              HealthRecord(
                                disease: diseaseResult,
                                treatment: treatmentResult,
                                date: DateTime.now(),
                                hasDisease:
                                    diseaseResult !=
                                    diseaseInfo['healthy']!['disease'],
                              ),
                            );
                          }

                          myplants.add(newPlant);

                          imagefile = null;
                          showNameButton = false;
                          showTreatmentBox = false;
                          showSaveButton = false;
                          diseaseResult = "";
                          treatmentResult = "";
                          _careData = {};
                        });

                        Navigator.pop(context);
                        Navigator.pop(context);
                      }
                    },
                    child: Text('add'.tr()),
                  ),
                  ElevatedButton(
                    style: ElevatedButton.styleFrom(
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                    onPressed: () => Navigator.pop(context),
                    child: Text('cancel'.tr()),
                  ),
                ],
              ),
            ],
          ),
        );
      },
    );
  }

  void showGardenSheet(
    BuildContext context,
    double screenWidth,
    double screenHeight,
  ) {
    showModalBottomSheet(
      isScrollControlled: true,
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) {
        final isDark = Theme.of(context).brightness == Brightness.dark;
        return StatefulBuilder(
          builder: (context, setSheetState) {
            return DraggableScrollableSheet(
              expand: false,
              initialChildSize: 0.75,
              minChildSize: 0.4,
              maxChildSize: 0.95,
              builder: (context, scrollController) {
                return Container(
                  color: isDark ? Colors.grey[850]! : Colors.white,
                  child: Padding(
                    padding: EdgeInsets.only(
                      top: 16,
                      left: 16,
                      right: 16,
                      bottom: MediaQuery.of(context).viewInsets.bottom + 16,
                    ),
                    child: Column(
                      children: [
                        Container(
                          width: 40,
                          height: 4,
                          margin: const EdgeInsets.only(bottom: 16),
                          decoration: BoxDecoration(
                            color: Colors.grey[400],
                            borderRadius: BorderRadius.circular(10),
                          ),
                        ),
                        Text(
                          'my_garden_emoji'.tr(),
                          style: const TextStyle(
                            fontSize: 20,
                            fontWeight: FontWeight.bold,
                            color: Color.fromARGB(255, 56, 114, 64),
                          ),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          'garden_select_hint'.tr(),
                          style: TextStyle(
                            color: Colors.grey[500],
                            fontSize: 13,
                          ),
                        ),
                        const SizedBox(height: 16),
                        Expanded(
                          child: GridView.builder(
                            controller: scrollController,
                            itemCount: myplants.length + 1,
                            gridDelegate:
                                const SliverGridDelegateWithFixedCrossAxisCount(
                                  crossAxisCount: 2,
                                  crossAxisSpacing: 12,
                                  mainAxisSpacing: 12,
                                  childAspectRatio: 0.85,
                                ),
                            itemBuilder: (context, index) {
                              if (index == myplants.length) {
                                return GestureDetector(
                                  onTap:
                                      () => showAddPlantWithDiagnosisSheet(
                                        context,
                                      ),
                                  child: Container(
                                    decoration: BoxDecoration(
                                      color:
                                          isDark
                                              ? Colors.grey[850]!
                                              : Colors.white,
                                      borderRadius: BorderRadius.circular(15),
                                      boxShadow: [
                                        BoxShadow(
                                          blurRadius: 8,
                                          color:
                                              isDark
                                                  ? Colors.black
                                                  : const Color.fromARGB(
                                                    255,
                                                    149,
                                                    234,
                                                    179,
                                                  ),
                                          offset: const Offset(2, 3),
                                        ),
                                      ],
                                    ),
                                    child: CustomPaint(
                                      painter: DottedBorderPainter(),
                                      child: const Center(
                                        child: Icon(
                                          Icons.add,
                                          size: 35,
                                          color: Color.fromARGB(
                                            255,
                                            56,
                                            114,
                                            64,
                                          ),
                                        ),
                                      ),
                                    ),
                                  ),
                                );
                              }

                              return GestureDetector(
                                onTap: () async {
                                  if (diseaseResult.isNotEmpty) {
                                    await ApiService.savePlant(
                                      name: myplants[index].name,
                                      disease: diseaseResult,
                                      treatment: treatmentResult,
                                    );
                                    setState(() {
                                      // ← حفظ careInstructions في النبتة
                                      final ci =
                                          _careData['careInstructions'] ?? '';
                                      final tr = _careData['treatment'] ?? '';
                                      myplants[index].careInstructions =
                                          ci.isNotEmpty ? ci : tr;
                                      myplants[index].watering =
                                          _careData['watering'];
                                      myplants[index].light =
                                          _careData['light'];
                                      myplants[index].fertilizing =
                                          _careData['fertilizing'];
                                      myplants[index].soil = _careData['soil'];
                                      myplants[index].humidity =
                                          _careData['humidity'];

                                      myplants[index].healthRecords.add(
                                        HealthRecord(
                                          disease: diseaseResult,
                                          treatment: treatmentResult,
                                          date: DateTime.now(),
                                          hasDisease:
                                              diseaseResult !=
                                              diseaseInfo['healthy']!['disease'],
                                        ),
                                      );
                                      imagefile = null;
                                      showNameButton = false;
                                      showTreatmentBox = false;
                                      showSaveButton = false;
                                      diseaseResult = "";
                                      treatmentResult = "";
                                      _careData = {};
                                    });
                                  }
                                  Navigator.pop(context);
                                },
                                child: ClipRRect(
                                  borderRadius: BorderRadius.circular(15),
                                  child: Stack(
                                    children: [
                                      Image.file(
                                        File(myplants[index].image),
                                        width: double.infinity,
                                        height: double.infinity,
                                        fit: BoxFit.cover,
                                      ),
                                      Positioned(
                                        bottom: 0,
                                        left: 0,
                                        right: 0,
                                        child: Container(
                                          padding: const EdgeInsets.all(8),
                                          decoration: BoxDecoration(
                                            color: Colors.black.withOpacity(
                                              0.5,
                                            ),
                                            borderRadius:
                                                const BorderRadius.only(
                                                  bottomLeft: Radius.circular(
                                                    15,
                                                  ),
                                                  bottomRight: Radius.circular(
                                                    15,
                                                  ),
                                                ),
                                          ),
                                          child: Center(
                                            child: Text(
                                              myplants[index].name,
                                              style: const TextStyle(
                                                fontSize: 16,
                                                color: Colors.white,
                                                fontWeight: FontWeight.bold,
                                              ),
                                              maxLines: 1,
                                              overflow: TextOverflow.ellipsis,
                                            ),
                                          ),
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                              );
                            },
                          ),
                        ),
                        const SizedBox(height: 10),
                        SizedBox(
                          width: double.infinity,
                          child: ElevatedButton(
                            style: ElevatedButton.styleFrom(
                              backgroundColor: const Color.fromARGB(
                                255,
                                56,
                                114,
                                64,
                              ),
                              foregroundColor: Colors.white,
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(12),
                              ),
                            ),
                            onPressed: () => Navigator.pop(context),
                            child: Text('close'.tr()),
                          ),
                        ),
                      ],
                    ),
                  ),
                );
              },
            );
          },
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final bgColor =
        isDark ? Colors.grey[900]! : const Color.fromARGB(255, 236, 255, 237);
    final cardColor = isDark ? Colors.grey[850]! : Colors.white;
    final shadowColor =
        isDark ? Colors.black : const Color.fromARGB(255, 149, 234, 179);
    final textColor = isDark ? Colors.white : Colors.black87;
    final hintColor =
        isDark ? Colors.grey[400]! : const Color.fromARGB(255, 78, 139, 88);
    const greenColor = Color.fromARGB(255, 56, 114, 64);
    final screenWidth = MediaQuery.of(context).size.width;
    final screenHeight = MediaQuery.of(context).size.height;

    return Scaffold(
      appBar: AppBar(
        automaticallyImplyLeading: false,
        centerTitle: true,
        backgroundColor: greenColor,
        iconTheme: const IconThemeData(color: Colors.white),
        title: Text(
          'identify_plant'.tr(),
          style: const TextStyle(
            color: Colors.white,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      backgroundColor: bgColor,
      body: ListView(
        padding: const EdgeInsets.all(12),
        children: [
          if (imagefile == null) ...[
            Padding(
              padding: const EdgeInsets.only(right: 8, top: 8, bottom: 4),
              child: Text(
                'shooting_instructions'.tr(),
                style: TextStyle(
                  fontSize: 18,
                  color: textColor,
                  fontWeight: FontWeight.bold,
                ),
                textAlign: TextAlign.start,
              ),
            ),
            Padding(
              padding: const EdgeInsets.only(right: 8, bottom: 8),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  _tipRow('tip_lighting'.tr(), textColor),
                  _tipRow('tip_steady'.tr(), textColor),
                  _tipRow('tip_close'.tr(), textColor),
                  _tipRow('tip_focus'.tr(), textColor),
                ],
              ),
            ),
          ],
          Container(
            decoration: BoxDecoration(
              color: cardColor,
              borderRadius: BorderRadius.circular(20),
              boxShadow: [
                BoxShadow(
                  blurRadius: 30,
                  color: shadowColor,
                  offset: const Offset(7, 5),
                ),
              ],
            ),
            padding: EdgeInsets.only(
              left: screenWidth * 0.08,
              right: screenWidth * 0.08,
              top: screenHeight * 0.04,
              bottom: screenHeight * 0.03,
            ),
            margin: EdgeInsets.symmetric(vertical: screenHeight * 0.02),
            child: Column(
              children: [
                DottedBorder(
                  color:
                      isDark
                          ? Colors.grey[600]!
                          : const Color.fromARGB(255, 201, 222, 203),
                  strokeWidth: 2,
                  borderType: BorderType.RRect,
                  dashPattern: const [8, 6],
                  radius: const Radius.circular(20),
                  child: Container(
                    width: double.infinity,
                    height: screenHeight * 0.25,
                    alignment: Alignment.center,
                    child:
                        imagefile == null
                            ? Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                Icon(
                                  Icons.upload_file_rounded,
                                  size: screenWidth * 0.12,
                                  color: hintColor,
                                ),
                                const SizedBox(height: 8),
                                Text(
                                  'choose_image'.tr(),
                                  style: TextStyle(
                                    fontSize: screenWidth * 0.042,
                                    fontWeight: FontWeight.bold,
                                    color: greenColor,
                                  ),
                                ),
                                const SizedBox(height: 4),
                                Text(
                                  'image_clear_hint'.tr(),
                                  style: TextStyle(
                                    fontSize: 11,
                                    color: Colors.grey[500],
                                  ),
                                ),
                              ],
                            )
                            : ClipRRect(
                              borderRadius: BorderRadius.circular(20),
                              child: Image.file(
                                imagefile!,
                                width: double.infinity,
                                height: double.infinity,
                                fit: BoxFit.cover,
                              ),
                            ),
                  ),
                ),
                SizedBox(height: screenHeight * 0.02),
                imagefile == null
                    ? ElevatedButton.icon(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: greenColor,
                        foregroundColor: Colors.white,
                      ),
                      onPressed: showimagepickeroptions,
                      icon: const Icon(Icons.add_photo_alternate),
                      label: Text('choose_photo'.tr()),
                    )
                    : Wrap(
                      alignment: WrapAlignment.center,
                      crossAxisAlignment: WrapCrossAlignment.center,
                      spacing: 10,
                      runSpacing: 10,
                      children: [
                        ElevatedButton.icon(
                          style: ElevatedButton.styleFrom(
                            backgroundColor: greenColor,
                            foregroundColor: Colors.white,
                          ),
                          onPressed: isAnalyzing ? null : analyzeImage,
                          icon:
                              isAnalyzing
                                  ? const SizedBox(
                                    width: 16,
                                    height: 16,
                                    child: CircularProgressIndicator(
                                      color: Colors.white,
                                      strokeWidth: 2,
                                    ),
                                  )
                                  : const Icon(Icons.search),
                          label: Text(
                            isAnalyzing ? 'analyzing'.tr() : 'analyze'.tr(),
                          ),
                        ),
                        TextButton.icon(
                          onPressed:
                              () => setState(() {
                                imagefile = null;
                                showNameButton = false;
                                showTreatmentBox = false;
                                showSaveButton = false;
                                diseaseResult = "";
                                treatmentResult = "";
                                _careData = {};
                              }),
                          icon: const Icon(
                            Icons.delete_outline,
                            color: Colors.red,
                          ),
                          label: Text(
                            'delete_image'.tr(),
                            style: const TextStyle(color: Colors.red),
                          ),
                        ),
                      ],
                    ),
              ],
            ),
          ),
          if (diseaseResult.isNotEmpty)
            Container(
              padding: EdgeInsets.all(screenWidth * 0.05),
              margin: const EdgeInsets.only(bottom: 12),
              decoration: BoxDecoration(
                color: cardColor,
                borderRadius: BorderRadius.circular(20),
                boxShadow: [
                  BoxShadow(
                    blurRadius: 20,
                    color: shadowColor,
                    offset: const Offset(5, 4),
                  ),
                ],
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.center,
                children: [
                  Text(
                    'analysis_result'.tr(),
                    style: TextStyle(
                      fontSize: screenWidth * 0.045,
                      fontWeight: FontWeight.bold,
                      color: greenColor,
                    ),
                  ),
                  const SizedBox(height: 10),
                  Text(
                    diseaseResult,
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      fontSize: screenWidth * 0.04,
                      color: textColor,
                      height: 1.5,
                    ),
                  ),
                ],
              ),
            ),
          if (showNameButton)
            Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: SizedBox(
                width: double.infinity,
                height: 50,
                child: ElevatedButton.icon(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: greenColor,
                    foregroundColor: Colors.white,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(14),
                    ),
                    textStyle: const TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  onPressed: showNameSheet,
                  icon: const Icon(Icons.edit),
                  label: Text('enter_plant_name'.tr()),
                ),
              ),
            ),
          if (showTreatmentBox)
            Container(
              padding: EdgeInsets.all(screenWidth * 0.05),
              margin: const EdgeInsets.only(bottom: 12),
              decoration: BoxDecoration(
                color: cardColor,
                borderRadius: BorderRadius.circular(20),
                boxShadow: [
                  BoxShadow(
                    blurRadius: 20,
                    color: shadowColor,
                    offset: const Offset(5, 4),
                  ),
                ],
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Center(
                    child: Text(
                      'treatment_plan_title'.tr(),
                      style: TextStyle(
                        fontSize: screenWidth * 0.045,
                        fontWeight: FontWeight.bold,
                        color: greenColor,
                      ),
                    ),
                  ),
                  const SizedBox(height: 10),
                  treatmentResult.isNotEmpty
                      ? Text(
                        treatmentResult,
                        textAlign: TextAlign.start,
                        style: TextStyle(
                          fontSize: screenWidth * 0.038,
                          color: textColor,
                          height: 1.8,
                        ),
                      )
                      : Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(
                            Icons.info_outline,
                            color: Colors.grey[500],
                            size: 18,
                          ),
                          const SizedBox(width: 6),
                          Text(
                            'no_treatment_available'.tr(),
                            style: TextStyle(
                              fontSize: screenWidth * 0.037,
                              color: Colors.grey[500],
                              fontStyle: FontStyle.italic,
                            ),
                          ),
                        ],
                      ),
                ],
              ),
            ),
          if (showSaveButton)
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 8),
              child: SizedBox(
                width: double.infinity,
                height: 50,
                child: ElevatedButton.icon(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: greenColor,
                    foregroundColor: Colors.white,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(14),
                    ),
                    textStyle: const TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  onPressed:
                      () => showGardenSheet(context, screenWidth, screenHeight),
                  icon: const Icon(Icons.save_alt),
                  label: Text('save_to_garden'.tr()),
                ),
              ),
            ),
          const SizedBox(height: 20),
        ],
      ),
    );
  }

  Widget _tipRow(String text, Color textColor) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 4),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Padding(
            padding: EdgeInsets.only(top: 6),
            child: Icon(
              Icons.circle,
              size: 6,
              color: Color.fromARGB(255, 56, 114, 64),
            ),
          ),
          const SizedBox(width: 8),
          Expanded(
            child: Text(text, style: TextStyle(fontSize: 13, color: textColor)),
          ),
        ],
      ),
    );
  }
}
