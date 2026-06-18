import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';
import 'MyGarden.dart';

class InfoSection extends StatelessWidget {
  final plant plantData;
  const InfoSection({super.key, required this.plantData});

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final subColor = isDark ? Colors.grey[400]! : Colors.grey[600]!;
    final cardColor = isDark ? Colors.grey[850]! : Colors.white;
    final textColor = isDark ? Colors.white : Colors.black;
    const greenColor = Color.fromARGB(255, 56, 114, 64);

    // ══════════════════════════════════════════
    //  التصنيفات مع أيقوناتها وألوانها
    // ══════════════════════════════════════════
    final List<Map<String, dynamic>> careInfo =
        [
          {
            'name': 'watering'.tr(),
            'icon': Icons.water_drop_outlined,
            'value': plantData.watering,
            'bg': const Color.fromARGB(255, 210, 235, 255),
            'iconColor': const Color.fromARGB(255, 50, 120, 200),
          },
          {
            'name': 'light'.tr(),
            'icon': Icons.wb_sunny_outlined,
            'value': plantData.light,
            'bg': const Color.fromARGB(255, 255, 243, 210),
            'iconColor': const Color.fromARGB(255, 203, 159, 27),
          },
          {
            'name': 'fertilizing'.tr(),
            'icon': Icons.grass,
            'value': plantData.fertilizing,
            'bg': const Color.fromARGB(255, 220, 242, 220),
            'iconColor': greenColor,
          },
          {
            'name': 'soil'.tr(),
            'icon': Icons.terrain_outlined,
            'value': plantData.soil,
            'bg': const Color.fromARGB(255, 240, 228, 210),
            'iconColor': const Color.fromARGB(255, 139, 90, 43),
          },
          {
            'name': 'humidity'.tr(),
            'icon': Icons.cloud_queue_sharp,
            'value': plantData.humidity,
            'bg': const Color.fromARGB(255, 220, 240, 255),
            'iconColor': const Color.fromARGB(255, 80, 160, 220),
          },
        ].where((item) {
          final v = item['value'];
          return v != null && v.toString().isNotEmpty;
        }).toList();

    final hasCareInstructions = plantData.careInstructions != null &&
        plantData.careInstructions!.isNotEmpty;

    // ══════════════════════════════════════════
    //  مفيش بيانات خالص → empty state
    // ══════════════════════════════════════════
    if (!hasCareInstructions && careInfo.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.eco_outlined, size: 64, color: Colors.grey[400]),
            const SizedBox(height: 14),
            Text(
              'مفيش معلومات رعاية متاحة لهذه النبتة',
              textAlign: TextAlign.center,
              style: TextStyle(
                color: Colors.grey[500],
                fontSize: 15,
                height: 1.5,
              ),
            ),
          ],
        ),
      );
    }

    // ══════════════════════════════════════════
    //  عرض البيانات
    // ══════════════════════════════════════════
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        // ────── careInstructions من السيرفر (card واحد) ──────
        if (hasCareInstructions)
          Container(
            margin: const EdgeInsets.only(bottom: 16),
            padding: const EdgeInsets.all(18),
            decoration: BoxDecoration(
              color: cardColor,
              borderRadius: BorderRadius.circular(16),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.start,
                  children: [
                    Container(
                      padding: const EdgeInsets.all(10),
                      decoration: BoxDecoration(
                        color: const Color.fromARGB(255, 220, 242, 220),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: const Icon(
                        Icons.eco,
                        size: 24,
                        color: greenColor,
                      ),
                    ),
                    const SizedBox(width: 10),
                    Text(
                      'care_instructions'.tr(),
                      style: const TextStyle(
                        fontSize: 17,
                        fontWeight: FontWeight.bold,
                        color: greenColor,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 14),
                const Divider(height: 1),
                const SizedBox(height: 14),
                Text(
                  plantData.careInstructions!,
                  style: TextStyle(
                    fontSize: 14,
                    color: subColor,
                    height: 1.8,
                  ),
                  textAlign: TextAlign.start,
                ),
              ],
            ),
          ),

        // ────── الـ fields المنظمة (ري، ضوء، إلخ) ──────
        for (final item in careInfo)
          Container(
            margin: const EdgeInsets.only(bottom: 12),
            padding: const EdgeInsets.symmetric(
              horizontal: 16,
              vertical: 14,
            ),
            decoration: BoxDecoration(
              color: cardColor,
              borderRadius: BorderRadius.circular(16),
            ),
            child: Row(
              children: [
                Container(
                  padding: const EdgeInsets.all(10),
                  decoration: BoxDecoration(
                    color: item['bg'] as Color,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Icon(
                    item['icon'] as IconData,
                    size: 28,
                    color: item['iconColor'] as Color,
                  ),
                ),
                const SizedBox(width: 14),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        item['name'] as String,
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                          color: textColor,
                        ),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        item['value'].toString(),
                        style: TextStyle(fontSize: 13, color: subColor),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
      ],
    );
  }
}
