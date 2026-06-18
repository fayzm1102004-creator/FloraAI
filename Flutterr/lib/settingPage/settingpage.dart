import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';
import 'aboutpage.dart';
import 'clear_scan_history.dart';
import 'edit_profile.dart';
import 'help&supportpage.dart';
import 'languagepage.dart';
import 'notificatinpage.dart';
import 'clear_all_history.dart';
import 'themepage.dart';

class SettingPage extends StatefulWidget {
  const SettingPage({super.key});

  @override
  State<SettingPage> createState() => _SettingPageState();
}

class _SettingPageState extends State<SettingPage> {
  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final bgColor =
        isDark ? Colors.grey[900]! : const Color.fromARGB(255, 236, 255, 237);
    final cardColor = isDark ? Colors.grey[850]! : Colors.white;
    final shadowColor =
        isDark ? Colors.black : const Color.fromARGB(255, 149, 234, 179);
    final iconBg =
        isDark ? Colors.grey[800]! : const Color.fromARGB(255, 219, 243, 221);
    final textColor = isDark ? Colors.white : Colors.black87;
    const greenColor = Color.fromARGB(255, 56, 114, 64);

    return Scaffold(
      backgroundColor: bgColor,
      appBar: AppBar(
        title: Text(
          'settings'.tr(),
          style: const TextStyle(
            color: Colors.white,
            fontWeight: FontWeight.bold,
          ),
        ),
        backgroundColor: greenColor,
        centerTitle: true,
        iconTheme: const IconThemeData(color: Colors.white),
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          const SizedBox(height: 10),
          _sectionTitle('account'.tr()),
          _settingCard(
            Icons.person,
            'edit_profile'.tr(),
            cardColor,
            iconBg,
            shadowColor,
            textColor,
            () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const EditProfilePage()),
              ).then((_) => setState(() {}));
            },
          ),
          const SizedBox(height: 12),
          _sectionTitle('app'.tr()),
          _settingCard(
            Icons.notifications,
            'notifications'.tr(),
            cardColor,
            iconBg,
            shadowColor,
            textColor,
            () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => NotificationsPage()),
              );
            },
          ),
          _settingCard(
            Icons.language,
            'language'.tr(),
            cardColor,
            iconBg,
            shadowColor,
            textColor,
            () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => LanguagePage()),
              );
            },
          ),
          _settingCard(
            Icons.color_lens,
            'appearance'.tr(),
            cardColor,
            iconBg,
            shadowColor,
            textColor,
            () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const ThemePage()),
              ).then((_) => setState(() {}));
            },
          ),
          const SizedBox(height: 12),
          _sectionTitle('privacy'.tr()),
          _settingCard(
            Icons.delete_outline,
            'clear_scan_history'.tr(),
            cardColor,
            iconBg,
            shadowColor,
            textColor,
            () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => ClearHistoryPage()),
              );
            },
          ),
          _settingCard(
            Icons.delete_forever_outlined,
            'clear_all_data'.tr(),
            cardColor,
            iconBg,
            shadowColor,
            textColor,
            () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const ClearAllHistory()),
              ).then((_) => setState(() {}));
            },
          ),
          const SizedBox(height: 12),
          _sectionTitle('help'.tr()),
          _settingCard(
            Icons.help_outline,
            'help_support'.tr(),
            cardColor,
            iconBg,
            shadowColor,
            textColor,
            () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => HelpPage()),
              );
            },
          ),
          _settingCard(
            Icons.info_outline,
            'about_app'.tr(),
            cardColor,
            iconBg,
            shadowColor,
            textColor,
            () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => AboutPage()),
              );
            },
          ),
        ],
      ),
    );
  }

  Widget _sectionTitle(String title) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Padding(
      padding: const EdgeInsets.only(bottom: 10, right: 4),
      child: Text(
        title,
        style: TextStyle(
          fontSize: 15,
          fontWeight: FontWeight.bold,
          color: isDark ? Colors.white : Colors.black87,
        ),
      ),
    );
  }

  Widget _settingCard(
    IconData icon,
    String title,
    Color cardColor,
    Color iconBg,
    Color shadowColor,
    Color textColor,
    VoidCallback onTap,
  ) {
    return Container(
      margin: const EdgeInsets.only(bottom: 10),
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
      child: ListTile(
        onTap: onTap,
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
        leading: Container(
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(
            color: iconBg,
            borderRadius: BorderRadius.circular(10),
          ),
          child: Icon(
            icon,
            color: const Color.fromARGB(255, 56, 114, 64),
            size: 22,
          ),
        ),
        title: Text(
          title,
          style: TextStyle(
            fontWeight: FontWeight.bold,
            fontSize: 15,
            color: textColor,
          ),
        ),
        trailing: const Icon(
          Icons.arrow_forward_ios,
          size: 16,
          color: Color.fromARGB(255, 56, 114, 64),
        ),
      ),
    );
  }
}
