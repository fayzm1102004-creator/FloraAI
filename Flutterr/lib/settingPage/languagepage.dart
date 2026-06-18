import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';

ValueNotifier<String> appLanguageNotifier = ValueNotifier("ar");

class LanguagePage extends StatefulWidget {
  const LanguagePage({super.key});

  @override
  State<LanguagePage> createState() => _LanguagePageState();
}

class _LanguagePageState extends State<LanguagePage> {
  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final bgColor =
        isDark ? Colors.grey[900]! : const Color.fromARGB(255, 236, 255, 237);
    final cardColor = isDark ? Colors.grey[850]! : Colors.white;
    final shadowColor =
        isDark ? Colors.black : const Color.fromARGB(255, 149, 234, 179);

    final currentLang = context.locale.languageCode;

    return Scaffold(
      backgroundColor: bgColor,
      appBar: AppBar(
        title: Text(
          'language'.tr(),
          style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
        ),
        centerTitle: true,
        backgroundColor: const Color.fromARGB(255, 56, 114, 64),
        iconTheme: IconThemeData(color: Colors.white),
      ),
      body: ListView(
        padding: EdgeInsets.all(16),
        children: [
          SizedBox(height: 10),
          Text(
            'choose_language'.tr(),
            style: TextStyle(
              fontSize: 15,
              fontWeight: FontWeight.bold,
              color: isDark ? Colors.grey[400] : Colors.grey[600],
            ),
          ),
          SizedBox(height: 12),
          _langCard(
            flag: '🇸🇦',
            title: 'arabic'.tr(),
            subtitle: 'العربية',
            locale: Locale('ar'),
            isSelected: currentLang == 'ar',
            isDark: isDark,
            cardColor: cardColor,
            shadowColor: shadowColor,
          ),
          SizedBox(height: 10),
          _langCard(
            flag: '🇬🇧',
            title: 'english'.tr(),
            subtitle: 'English',
            locale: Locale('en'),
            isSelected: currentLang == 'en',
            isDark: isDark,
            cardColor: cardColor,
            shadowColor: shadowColor,
          ),
        ],
      ),
    );
  }

  Widget _langCard({
    required String flag,
    required String title,
    required String subtitle,
    required Locale locale,
    required bool isSelected,
    required bool isDark,
    required Color cardColor,
    required Color shadowColor,
  }) {
    return GestureDetector(
      onTap: () async {
        await context.setLocale(locale);
        appLanguageNotifier.value = locale.languageCode;
        print("🌍 Language changed to: ${appLanguageNotifier.value}");
        setState(() {});
        if (context.mounted) {
          Navigator.of(context).popUntil((route) => route.isFirst);
        }
      },
      child: Container(
        padding: EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        decoration: BoxDecoration(
          color: isSelected ? Color.fromARGB(255, 219, 243, 221) : cardColor,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color:
                isSelected
                    ? Color.fromARGB(255, 56, 114, 64)
                    : (isDark ? Colors.white : Colors.black),
            width: 2,
          ),
          boxShadow: [
            BoxShadow(blurRadius: 8, color: shadowColor, offset: Offset(2, 3)),
          ],
        ),
        child: Row(
          children: [
            Text(flag, style: TextStyle(fontSize: 30)),
            SizedBox(width: 16),
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                    color:
                        isSelected
                            ? Color.fromARGB(255, 56, 114, 64)
                            : Theme.of(context).colorScheme.onSurface,
                  ),
                ),
                Text(
                  subtitle,
                  style: TextStyle(fontSize: 13, color: Colors.grey[500]),
                ),
              ],
            ),
            Spacer(),
            if (isSelected)
              Icon(
                Icons.check_circle,
                color: Color.fromARGB(255, 56, 114, 64),
                size: 26,
              ),
          ],
        ),
      ),
    );
  }
}
