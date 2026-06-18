import 'dart:io';
import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';
import 'package:project/sign_up.dart';
import 'MyGarden.dart';
import 'PlantDetailsPage.dart';

class Homepage extends StatefulWidget {
  const Homepage({super.key});

  @override
  State<Homepage> createState() => _HomepageState();
}

class _HomepageState extends State<Homepage> {
  List<Map<String, dynamic>> getTodayTasks() {
    List<Map<String, dynamic>> tasks = [];
    final today = DateTime.now();
    for (final p in myplants) {
      for (final record in p.healthRecords) {
        if (!record.hasDisease) continue;
        if (record.isRecovered) continue;

        final doneToday = record.doneTodayAt;
        if (doneToday != null &&
            doneToday.year == today.year &&
            doneToday.month == today.month &&
            doneToday.day == today.day)
          continue;

        tasks.add({
          'plantName': p.name,
          'plantImage': p.image,
          'disease': record.disease,
          'treatment': record.todayStep, // ← بدل record.treatment
          'plant': p,
          'record': record,
        });
      }
    }
    return tasks;
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final bgColor =
        isDark ? Colors.grey[900]! : const Color.fromARGB(255, 236, 255, 237);
    final cardColor = isDark ? Colors.grey[850]! : Colors.white;
    final textColor = isDark ? Colors.white : Colors.black;
    final shadowColor =
        isDark ? Colors.black : const Color.fromARGB(255, 149, 234, 179);
    const greenColor = Color.fromARGB(255, 56, 114, 64);

    final recentPlants =
        myplants.length <= 5 ? myplants : myplants.sublist(myplants.length - 5);
    final tasks = getTodayTasks();

    return Scaffold(
      backgroundColor: bgColor,
      appBar: AppBar(
        automaticallyImplyLeading: false,
        backgroundColor: greenColor,
        centerTitle: true,
        iconTheme: const IconThemeData(color: Colors.white),
        title: Text(
          'home'.tr(),
          style: const TextStyle(
            color: Colors.white,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      body: SingleChildScrollView(
        child: Column(
          children: [
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: cardColor,
                borderRadius: const BorderRadius.only(
                  bottomLeft: Radius.circular(20),
                  bottomRight: Radius.circular(20),
                ),
                boxShadow: [
                  BoxShadow(
                    blurRadius: 8,
                    color: shadowColor,
                    offset: const Offset(2, 3),
                  ),
                ],
              ),
              child: ListTile(
                title: Text(
                  '${'welcome'.tr()} $currentUserName',
                  style: const TextStyle(
                    fontSize: 26,
                    color: greenColor,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                subtitle: Text(
                  tasks.isEmpty
                      ? 'no_tasks_today'.tr()
                      : 'tasks_today'.tr(
                        namedArgs: {'count': tasks.length.toString()},
                      ),
                  style: TextStyle(
                    color: isDark ? Colors.grey[400] : Colors.grey[700],
                  ),
                ),
              ),
            ),
            const SizedBox(height: 20),
            _sectionTitle('today_tasks'.tr(), textColor),
            tasks.isEmpty
                ? Padding(
                  padding: const EdgeInsets.symmetric(vertical: 20),
                  child: Text(
                    'no_tasks_plants_ok'.tr(),
                    style: TextStyle(color: Colors.grey[500], fontSize: 14),
                  ),
                )
                : ListView.builder(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  itemCount: tasks.length,
                  itemBuilder: (context, index) {
                    final task = tasks[index];
                    final record = task['record'] as HealthRecord;

                    return Container(
                      margin: const EdgeInsets.symmetric(
                        horizontal: 12,
                        vertical: 6,
                      ),
                      decoration: BoxDecoration(
                        color: cardColor,
                        borderRadius: BorderRadius.circular(14),
                        boxShadow: [
                          BoxShadow(
                            blurRadius: 8,
                            color: shadowColor,
                            offset: const Offset(2, 3),
                          ),
                        ],
                      ),
                      child: Padding(
                        padding: const EdgeInsets.all(12),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            // ── اسم النبتة والصورة ──
                            Row(
                              children: [
                                ClipRRect(
                                  borderRadius: BorderRadius.circular(10),
                                  child: Image.file(
                                    File(task['plantImage']),
                                    width: 55,
                                    height: 55,
                                    fit: BoxFit.cover,
                                  ),
                                ),
                                const SizedBox(width: 12),
                                Expanded(
                                  child: Column(
                                    crossAxisAlignment:
                                        CrossAxisAlignment.start,
                                    children: [
                                      Text(
                                        task['plantName'],
                                        style: TextStyle(
                                          fontWeight: FontWeight.bold,
                                          fontSize: 15,
                                          color: textColor,
                                        ),
                                      ),
                                      const SizedBox(height: 4),
                                      Row(
                                        children: [
                                          const Icon(
                                            Icons.coronavirus_outlined,
                                            size: 13,
                                            color: Colors.red,
                                          ),
                                          const SizedBox(width: 4),
                                          Expanded(
                                            child: Text(
                                              task['disease'],
                                              maxLines: 1,
                                              overflow: TextOverflow.ellipsis,
                                              style: const TextStyle(
                                                fontSize: 12,
                                                color: Colors.red,
                                              ),
                                            ),
                                          ),
                                        ],
                                      ),
                                    ],
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 10),
                            // ── العلاج كامل ──
                            Container(
                              width: double.infinity,
                              padding: const EdgeInsets.all(12),
                              decoration: BoxDecoration(
                                color:
                                    isDark
                                        ? Colors.grey[800]
                                        : const Color.fromARGB(
                                          255,
                                          236,
                                          255,
                                          237,
                                        ),
                                borderRadius: BorderRadius.circular(10),
                              ),
                              child: Row(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  const Icon(
                                    Icons.healing,
                                    size: 16,
                                    color: greenColor,
                                  ),
                                  const SizedBox(width: 8),
                                  Expanded(
                                    child: Text(
                                      task['treatment'].toString(),
                                      style: TextStyle(
                                        fontSize: 13,
                                        color:
                                            isDark
                                                ? Colors.white
                                                : Colors.black87,
                                        height: 1.6,
                                      ),
                                      textAlign: TextAlign.right,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                            const SizedBox(height: 10),
                            // ── الأزرار ──
                            Row(
                              mainAxisAlignment: MainAxisAlignment.end,
                              children: [
                                // ← زر تعافت نهائياً
                                OutlinedButton.icon(
                                  style: OutlinedButton.styleFrom(
                                    foregroundColor: Colors.green,
                                    side: const BorderSide(color: Colors.green),
                                    shape: RoundedRectangleBorder(
                                      borderRadius: BorderRadius.circular(10),
                                    ),
                                    padding: const EdgeInsets.symmetric(
                                      horizontal: 10,
                                      vertical: 6,
                                    ),
                                  ),
                                  onPressed: () {
                                    setState(() {
                                      record.isRecovered = true;
                                    });
                                  },
                                  icon: const Icon(
                                    Icons.check_circle,
                                    size: 16,
                                  ),
                                  label: Text(
                                    'RECOVER'.tr(),
                                    style: const TextStyle(fontSize: 12),
                                  ),
                                ),
                                const SizedBox(width: 8),
                                // ← زر تم النهارده
                                ElevatedButton.icon(
                                  style: ElevatedButton.styleFrom(
                                    backgroundColor: greenColor,
                                    foregroundColor: Colors.white,
                                    shape: RoundedRectangleBorder(
                                      borderRadius: BorderRadius.circular(10),
                                    ),
                                    padding: const EdgeInsets.symmetric(
                                      horizontal: 10,
                                      vertical: 6,
                                    ),
                                  ),
                                  onPressed: () {
                                    setState(() {
                                      record.doneTodayAt = DateTime.now();
                                    });
                                  },
                                  icon: const Icon(Icons.done, size: 16),
                                  label: Text(
                                    'done'.tr(),
                                    style: const TextStyle(fontSize: 12),
                                  ),
                                ),
                              ],
                            ),
                          ],
                        ),
                      ),
                    );
                  },
                ),
            const SizedBox(height: 15),
            _sectionTitle('my_garden'.tr(), textColor),
            myplants.isEmpty
                ? Padding(
                  padding: const EdgeInsets.symmetric(vertical: 20),
                  child: Text(
                    'garden_empty'.tr(),
                    style: TextStyle(color: Colors.grey[500], fontSize: 14),
                  ),
                )
                : SingleChildScrollView(
                  padding: const EdgeInsets.only(
                    right: 15,
                    left: 15,
                    bottom: 20,
                  ),
                  scrollDirection: Axis.horizontal,
                  child: Row(
                    children:
                        recentPlants.map((p) {
                          return Padding(
                            padding: const EdgeInsets.only(right: 15),
                            child: GestureDetector(
                              onTap: () async {
                                await Navigator.push(
                                  context,
                                  MaterialPageRoute(
                                    builder: (_) => PlantDetailsPage(Plant: p),
                                  ),
                                );
                                setState(() {});
                              },
                              child: Column(
                                children: [
                                  Container(
                                    decoration: BoxDecoration(
                                      shape: BoxShape.circle,
                                      border: Border.all(
                                        color: greenColor,
                                        width: 3,
                                      ),
                                    ),
                                    child: CircleAvatar(
                                      radius: 50,
                                      backgroundImage: FileImage(File(p.image)),
                                    ),
                                  ),
                                  const SizedBox(height: 6),
                                  Text(
                                    p.name,
                                    style: TextStyle(
                                      fontWeight: FontWeight.bold,
                                      fontSize: 13,
                                      color: textColor,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          );
                        }).toList(),
                  ),
                ),
            const SizedBox(height: 20),
          ],
        ),
      ),
    );
  }

  Widget _sectionTitle(String title, Color textColor) {
    return Align(
      alignment: Alignment.centerRight,
      child: Padding(
        padding: const EdgeInsets.only(right: 18, bottom: 8),
        child: Text(
          title,
          style: TextStyle(
            fontWeight: FontWeight.bold,
            fontSize: 20,
            color: textColor,
          ),
        ),
      ),
    );
  }
}
