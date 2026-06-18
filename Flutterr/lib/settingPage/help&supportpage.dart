import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';

class HelpPage extends StatelessWidget {
  const HelpPage({super.key});

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
          'help_support'.tr(),
          style: const TextStyle(
            color: Colors.white,
            fontWeight: FontWeight.bold,
          ),
        ),
        centerTitle: true,
        backgroundColor: greenColor,
        iconTheme: const IconThemeData(color: Colors.white),
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          const SizedBox(height: 10),
          _section(
            cardColor,
            shadowColor,
            greenColor,
            textColor,
            title: 'faq'.tr(),
            icon: Icons.help_outline,
            items: [
              'كيف أقوم بفحص النبتة؟',
              'لماذا فشل تحليل النبتة؟',
              'كيف يعمل الذكاء الاصطناعي في التطبيق؟',
            ],
          ),
          const SizedBox(height: 12),
          _section(
            cardColor,
            shadowColor,
            greenColor,
            textColor,
            title: 'contact_us'.tr(),
            icon: Icons.contact_support_outlined,
            items: [
              'البريد الإلكتروني: aslmkaled@gmail.com',
              'الهاتف: 01014056925',
            ],
          ),
        ],
      ),
    );
  }

  Widget _section(
    Color cardColor,
    Color shadowColor,
    Color greenColor,
    Color textColor, {
    required String title,
    required IconData icon,
    required List<String> items,
  }) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: cardColor,
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            blurRadius: 8,
            color: shadowColor,
            offset: const Offset(2, 3),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(icon, color: greenColor, size: 22),
              const SizedBox(width: 8),
              Text(
                title,
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                  color: greenColor,
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          ...items.map(
            (item) => Padding(
              padding: const EdgeInsets.only(bottom: 8),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    "• ",
                    style: TextStyle(color: Color.fromARGB(255, 56, 114, 64)),
                  ),
                  Expanded(
                    child: Text(
                      item,
                      style: TextStyle(fontSize: 14, color: textColor),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}
