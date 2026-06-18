import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';
import 'MyGarden.dart';

class HealthRecordSection extends StatefulWidget {
  final plant Plant;
  const HealthRecordSection({super.key, required this.Plant});

  @override
  State<HealthRecordSection> createState() => _HealthRecordSectionState();
}

class _HealthRecordSectionState extends State<HealthRecordSection> {
  String _formatDate(DateTime date) => '${date.day}/${date.month}/${date.year}';

  @override
  Widget build(BuildContext context) {
    final records = widget.Plant.healthRecords;

    return records.isEmpty
        ? Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                Icons.health_and_safety_outlined,
                size: 60,
                color: Colors.grey[400],
              ),
              const SizedBox(height: 10),
              Text(
                'no_health_records'.tr(),
                style: const TextStyle(color: Colors.grey, fontSize: 15),
              ),
            ],
          ),
        )
        : ListView.separated(
          padding: const EdgeInsets.all(16),
          itemCount: records.length,
          separatorBuilder: (_, __) => const SizedBox(height: 8),
          itemBuilder: (context, index) {
            final record = records[index];
            return ListTile(
              onTap: () {
                showDialog(
                  context: context,
                  builder:
                      (context) => AlertDialog(
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(16),
                        ),
                        title: Row(
                          children: [
                            const Icon(
                              Icons.coronavirus_outlined,
                              color: Color.fromARGB(255, 56, 114, 64),
                            ),
                            const SizedBox(width: 8),
                            Expanded(
                              child: Text(
                                'disease_details'.tr(),
                                style: const TextStyle(
                                  fontWeight: FontWeight.bold,
                                  color: Color.fromARGB(255, 56, 114, 64),
                                ),
                              ),
                            ),
                          ],
                        ),
                        content: SingleChildScrollView(
                          child: Column(
                            mainAxisSize: MainAxisSize.min,
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                '${'date'.tr()}: ${_formatDate(record.date)}',
                                style: TextStyle(
                                  color: Colors.grey[600],
                                  fontSize: 13,
                                ),
                              ),
                              const SizedBox(height: 12),
                              Text(
                                record.treatment,
                                style: const TextStyle(
                                  fontSize: 15,
                                  height: 1.5,
                                ),
                              ),
                            ],
                          ),
                        ),
                        actions: [
                          ElevatedButton(
                            style: ElevatedButton.styleFrom(
                              backgroundColor: const Color.fromARGB(
                                255,
                                56,
                                114,
                                64,
                              ),
                              foregroundColor: Colors.white,
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(10),
                              ),
                            ),
                            onPressed: () => Navigator.pop(context),
                            child: Text('close'.tr()),
                          ),
                        ],
                      ),
                );
              },
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
              ),
              tileColor: const Color.fromARGB(255, 236, 255, 237),
              leading: const CircleAvatar(
                backgroundColor: Color.fromARGB(255, 56, 114, 64),
                child: Icon(
                  Icons.coronavirus_outlined,
                  color: Colors.white,
                  size: 20,
                ),
              ),
              title: Text(
                record.disease.length > 40
                    ? '${record.disease.substring(0, 40)}...'
                    : record.disease,
                style: const TextStyle(
                  fontWeight: FontWeight.bold,
                  fontSize: 14,
                  color: Color.fromARGB(255, 56, 114, 64),
                ),
              ),
              subtitle: Text(
                '${'date'.tr()} ${_formatDate(record.date)}',
                style: TextStyle(color: Colors.grey[600], fontSize: 12),
              ),
              trailing: const Icon(
                Icons.arrow_forward_ios,
                size: 14,
                color: Colors.grey,
              ),
            );
          },
        );
  }
}
