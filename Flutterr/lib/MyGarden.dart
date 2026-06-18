import 'dart:io';
import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';
import 'package:image_picker/image_picker.dart';
import 'PlantDetailsPage.dart';

class Gardenpage extends StatefulWidget {
  const Gardenpage({super.key});

  @override
  State<Gardenpage> createState() => _GardenpageState();
}

List<plant> myplants = [];

class _GardenpageState extends State<Gardenpage> {
  final ImagePicker picker = ImagePicker();

  Future<void> showAddPlantSheet(BuildContext context) async {
    final XFile? image = await picker.pickImage(
      source: ImageSource.gallery,
      imageQuality: 70,
    );
    if (image == null) return;
    final File imageFile = File(image.path);
    final TextEditingController nameController = TextEditingController();

    showModalBottomSheet(
      // ignore: use_build_context_synchronously
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
                  imageFile,
                  height: 150,
                  width: double.infinity,
                  fit: BoxFit.cover,
                ),
              ),
              const SizedBox(height: 16),
              TextFormField(
                controller: nameController,
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
                    onPressed: () {
                      if (nameController.text.isNotEmpty) {
                        setState(
                          () => myplants.add(
                            plant(
                              image: imageFile.path,
                              name: nameController.text,
                            ),
                          ),
                        );
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

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final bgColor =
        isDark ? Colors.grey[900]! : const Color.fromARGB(255, 236, 255, 237);
    final cardColor = isDark ? Colors.grey[850]! : Colors.white;
    final shadowColor =
        isDark ? Colors.black : const Color.fromARGB(255, 149, 234, 179);
    const greenColor = Color.fromARGB(255, 56, 114, 64);

    return Scaffold(
      backgroundColor: bgColor,
      appBar: AppBar(
        automaticallyImplyLeading: false,
        backgroundColor: greenColor,
        centerTitle: true,
        iconTheme: const IconThemeData(color: Colors.white),
        title: Text(
          'my_garden'.tr(),
          style: const TextStyle(
            color: Colors.white,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: GridView.builder(
          itemCount: myplants.length + 1,
          gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
            crossAxisCount: 2,
            crossAxisSpacing: 14,
            mainAxisSpacing: 14,
            childAspectRatio: 0.85,
          ),
          itemBuilder: (context, index) {
            if (index == myplants.length) {
              return GestureDetector(
                onTap: () => showAddPlantSheet(context),
                child: Container(
                  decoration: BoxDecoration(
                    color: cardColor,
                    borderRadius: BorderRadius.circular(18),
                    boxShadow: [
                      BoxShadow(
                        blurRadius: 8,
                        color: shadowColor,
                        offset: const Offset(2, 3),
                      ),
                    ],
                  ),
                  child: CustomPaint(
                    painter: DottedBorderPainter(),
                    child: const Center(
                      child: Icon(Icons.add, size: 35, color: greenColor),
                    ),
                  ),
                ),
              );
            }
            return GestureDetector(
              onTap:
                  () => Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (_) => PlantDetailsPage(Plant: myplants[index]),
                    ),
                  ),
              onLongPress: () {
                showDialog(
                  context: context,
                  builder:
                      (context) => AlertDialog(
                        title: Text('delete_plant'.tr()),
                        content: Text(
                          'delete_plant_confirm'.tr(
                            namedArgs: {'name': myplants[index].name},
                          ),
                        ),
                        actions: [
                          TextButton(
                            child: Text('no'.tr()),
                            onPressed: () => Navigator.pop(context),
                          ),
                          TextButton(
                            child: Text(
                              'yes'.tr(),
                              style: const TextStyle(color: Colors.red),
                            ),
                            onPressed: () {
                              setState(() => myplants.removeAt(index));
                              Navigator.pop(context);
                            },
                          ),
                        ],
                      ),
                );
              },
              child: Container(
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(18),
                  boxShadow: [
                    BoxShadow(
                      blurRadius: 8,
                      color: shadowColor,
                      offset: const Offset(2, 3),
                    ),
                  ],
                ),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(18),
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
                          padding: const EdgeInsets.all(10),
                          color: Colors.black.withOpacity(0.45),
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
              ),
            );
          },
        ),
      ),
    );
  }
}

class DottedBorderPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final paint =
        Paint()
          ..color = const Color.fromARGB(255, 56, 114, 64)
          ..strokeWidth = 2
          ..style = PaintingStyle.stroke;
    const double dashWidth = 8, dashSpace = 6, radius = 15;
    final path =
        Path()..addRRect(
          RRect.fromRectAndRadius(
            Rect.fromLTWH(1, 1, size.width - 2, size.height - 2),
            const Radius.circular(radius),
          ),
        );
    for (final metric in path.computeMetrics()) {
      double distance = 0;
      while (distance < metric.length) {
        canvas.drawPath(
          metric.extractPath(distance, distance + dashWidth),
          paint,
        );
        distance += dashWidth + dashSpace;
      }
    }
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}

class plant {
  final String image;
  final String name;
  String? notes,
      wateringSchedule,
      location,
      careInstructions,
      watering,
      light,
      fertilizing,
      soil,
      humidity;
  List<HealthRecord> healthRecords;

  plant({
    required this.image,
    required this.name,
    this.notes,
    this.wateringSchedule,
    this.location,
    this.watering,
    this.light,
    this.fertilizing,
    this.soil,
    this.humidity,
    List<HealthRecord>? healthRecords,
  }) : healthRecords = healthRecords ?? [];
}

class HealthRecord {
  final String disease, treatment;
  final DateTime date;
  final bool hasDisease;
  bool isRecovered;
  DateTime? doneTodayAt;
  HealthRecord({
    required this.disease,
    required this.treatment,
    required this.date,
    required this.hasDisease,
    this.isRecovered = false,
    this.doneTodayAt,
  });

  // ← خطوة النهارده بس
  String get todayStep {
    if (treatment.isEmpty) return treatment;
    
    // قسّم على . 
    final steps = treatment
        .split('.')
        .map((s) => s.trim())
        .where((s) => s.isNotEmpty)
        .toList();
    
    if (steps.isEmpty) return treatment;
    if (steps.length == 1) return treatment;
    
    // ← كل يوم خطوة مختلفة (تتغير عند منتصف الليل 12:00 AM)
    final todayMidnight = DateTime(DateTime.now().year, DateTime.now().month, DateTime.now().day);
    final dateMidnight = DateTime(date.year, date.month, date.day);
    final daysSince = todayMidnight.difference(dateMidnight).inDays;
    
    final stepIndex = daysSince % steps.length;
    return steps[stepIndex];
  }
}
