import 'dart:io';
import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';
import 'MyGarden.dart';
import 'Care.dart';
import 'Health_record.dart';

class PlantDetailsPage extends StatefulWidget {
  final plant Plant;
  const PlantDetailsPage({super.key, required this.Plant});

  @override
  State<PlantDetailsPage> createState() => _PlantDetailsPageState();
}

class _PlantDetailsPageState extends State<PlantDetailsPage> {
  @override
  Widget build(BuildContext context) {
    return DefaultTabController(
      length: 2,
      child: Scaffold(
        appBar: AppBar(
          foregroundColor: Colors.white,
          title: Text(
            'plant_file'.tr(),
            style: const TextStyle(fontWeight: FontWeight.bold),
          ),
          centerTitle: true,
          backgroundColor: const Color.fromARGB(255, 56, 114, 64),
        ),
        body: Column(
          children: [
            ClipRRect(
              borderRadius: const BorderRadius.only(
                topLeft: Radius.circular(5),
                topRight: Radius.circular(5),
                bottomLeft: Radius.circular(20),
                bottomRight: Radius.circular(20),
              ),
              child: Image.file(
                File(widget.Plant.image),
                height: 200,
                width: 360,
                fit: BoxFit.cover,
              ),
            ),
            const SizedBox(height: 10),
            Text(
              widget.Plant.name,
              style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 10),
            Container(
              margin: const EdgeInsets.all(13),
              padding: const EdgeInsets.all(3),
              decoration: BoxDecoration(
                color: Colors.grey.shade300,
                borderRadius: BorderRadius.circular(10),
              ),
              child: TabBar(
                dividerColor: Colors.grey.shade300,
                indicatorSize: TabBarIndicatorSize.tab,
                labelStyle: const TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                ),
                unselectedLabelStyle: const TextStyle(fontSize: 12),
                indicator: BoxDecoration(
                  color: const Color.fromARGB(255, 48, 88, 39),
                  borderRadius: BorderRadius.circular(5),
                ),
                labelColor: Colors.white,
                unselectedLabelColor: Colors.black,
                tabs: [Tab(text: 'care'.tr()), Tab(text: 'health_record'.tr())],
              ),
            ),
            Expanded(
              child: TabBarView(
                children: [
                  // ← key بيجبر الـ widget يعمل rebuild لما careInstructions تتغير
                  InfoSection(
                    key: ValueKey(widget.Plant.careInstructions),
                    plantData: widget.Plant,
                  ),
                  HealthRecordSection(Plant: widget.Plant),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
