import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';

class PlantTip {
  final String id, title, description;
  final IconData icon;
  final Color iconColor;
  PlantTip({
    required this.id,
    required this.title,
    required this.description,
    this.icon = Icons.lightbulb_outline,
    this.iconColor = const Color.fromARGB(255, 203, 159, 27),
  });
}

List<PlantTip> savedTips = [];

class SavedTipsPage extends StatefulWidget {
  const SavedTipsPage({super.key});

  @override
  State<SavedTipsPage> createState() => _SavedTipsPageState();
}

class _SavedTipsPageState extends State<SavedTipsPage> {
  void removeTip(String id) =>
      setState(() => savedTips.removeWhere((t) => t.id == id));

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final bgColor =
        isDark ? Colors.grey[900]! : const Color.fromARGB(255, 236, 255, 237);
    final cardColor = isDark ? Colors.grey[850]! : Colors.white;
    final shadowColor =
        isDark ? Colors.black : const Color.fromARGB(255, 149, 234, 179);
    final textColor = isDark ? Colors.white : Colors.black;
    const greenColor = Color.fromARGB(255, 56, 114, 64);

    return Scaffold(
      backgroundColor: bgColor,
      appBar: AppBar(
        title: Text(
          'saved_tips_title'.tr(),
          style: const TextStyle(
            color: Colors.white,
            fontWeight: FontWeight.bold,
          ),
        ),
        centerTitle: true,
        backgroundColor: greenColor,
        iconTheme: const IconThemeData(color: Colors.white),
        actions: [
          if (savedTips.isNotEmpty)
            IconButton(
              icon: const Icon(
                Icons.delete_sweep_outlined,
                color: Colors.white,
              ),
              tooltip: 'clear_all'.tr(),
              onPressed: () {
                showDialog(
                  context: context,
                  builder:
                      (context) => AlertDialog(
                        backgroundColor: cardColor,
                        title: Text(
                          'clear_all_tips'.tr(),
                          style: TextStyle(color: textColor),
                        ),
                        content: Text(
                          'clear_all_tips_confirm'.tr(),
                          style: TextStyle(color: Colors.grey[500]),
                        ),
                        actions: [
                          TextButton(
                            onPressed: () => Navigator.pop(context),
                            child: Text('cancel'.tr()),
                          ),
                          ElevatedButton(
                            style: ElevatedButton.styleFrom(
                              backgroundColor: Colors.red,
                              foregroundColor: Colors.white,
                            ),
                            onPressed: () {
                              setState(() => savedTips.clear());
                              Navigator.pop(context);
                            },
                            child: Text('delete'.tr()),
                          ),
                        ],
                      ),
                );
              },
            ),
        ],
      ),
      body:
          savedTips.isEmpty
              ? Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.star_border, size: 70, color: Colors.grey[400]),
                    const SizedBox(height: 14),
                    Text(
                      'no_saved_tips'.tr(),
                      style: TextStyle(
                        fontSize: 16,
                        color: Colors.grey[500],
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 6),
                    Text(
                      'no_saved_tips_hint'.tr(),
                      style: TextStyle(fontSize: 13, color: Colors.grey[400]),
                    ),
                  ],
                ),
              )
              : ListView.separated(
                padding: const EdgeInsets.all(16),
                itemCount: savedTips.length,
                separatorBuilder: (_, __) => const SizedBox(height: 10),
                itemBuilder: (context, index) {
                  final tip = savedTips[index];
                  return Dismissible(
                    key: Key(tip.id),
                    direction: DismissDirection.endToStart,
                    background: Container(
                      alignment: Alignment.centerLeft,
                      padding: const EdgeInsets.only(left: 20),
                      decoration: BoxDecoration(
                        color: Colors.red[400],
                        borderRadius: BorderRadius.circular(16),
                      ),
                      child: const Icon(
                        Icons.delete_outline,
                        color: Colors.white,
                        size: 28,
                      ),
                    ),
                    onDismissed: (_) => removeTip(tip.id),
                    child: Container(
                      decoration: BoxDecoration(
                        color: cardColor,
                        borderRadius: BorderRadius.circular(16),
                        boxShadow: [
                          BoxShadow(
                            blurRadius: 8,
                            color: shadowColor,
                            offset: const Offset(2, 3),
                          ),
                        ],
                      ),
                      child: Padding(
                        padding: const EdgeInsets.all(14),
                        child: Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Container(
                              padding: const EdgeInsets.all(10),
                              decoration: BoxDecoration(
                                color: tip.iconColor.withOpacity(0.15),
                                borderRadius: BorderRadius.circular(12),
                              ),
                              child: Icon(
                                tip.icon,
                                color: tip.iconColor,
                                size: 26,
                              ),
                            ),
                            const SizedBox(width: 12),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    tip.title,
                                    style: TextStyle(
                                      fontWeight: FontWeight.bold,
                                      fontSize: 15,
                                      color: textColor,
                                    ),
                                  ),
                                  const SizedBox(height: 5),
                                  Text(
                                    tip.description,
                                    maxLines: 2,
                                    overflow: TextOverflow.ellipsis,
                                    style: TextStyle(
                                      fontSize: 13,
                                      color: Colors.grey[500],
                                      height: 1.5,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                            IconButton(
                              icon: const Icon(
                                Icons.bookmark_remove_outlined,
                                color: Colors.red,
                                size: 22,
                              ),
                              tooltip: 'remove_from_saved'.tr(),
                              onPressed: () => removeTip(tip.id),
                            ),
                          ],
                        ),
                      ),
                    ),
                  );
                },
              ),
    );
  }
}
