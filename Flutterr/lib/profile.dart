import 'dart:io';
import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';
import 'package:project/sign_up.dart';
import 'settingPage/settingpage.dart';
import 'MyGarden.dart';
import 'login.dart';
import 'scan_history_page.dart';
import 'saved_tips_page.dart';

class Profilepage extends StatefulWidget {
  const Profilepage({super.key});

  @override
  State<Profilepage> createState() => _ProfilepageState();
}

class _ProfilepageState extends State<Profilepage> {
  int getTotalScans() =>
      myplants.fold(0, (sum, p) => sum + p.healthRecords.length);
  int getTotalDiseases() => myplants.fold(
    0,
    (sum, p) => sum + p.healthRecords.where((r) => r.hasDisease).length,
  );

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final bgColor =
        isDark ? Colors.grey[900]! : const Color.fromARGB(255, 236, 255, 237);
    final cardColor = isDark ? Colors.grey[850]! : Colors.white;
    final textColor = isDark ? Colors.white : Colors.black;
    final shadowColor =
        isDark ? Colors.black : const Color.fromARGB(255, 149, 234, 179);
    final iconBg =
        isDark ? Colors.grey[800]! : const Color.fromARGB(255, 219, 243, 221);
    const greenColor = Color.fromARGB(255, 56, 114, 64);

    final totalScans = getTotalScans();
    final totalDiseases = getTotalDiseases();

    return Scaffold(
      backgroundColor: bgColor,
      appBar: AppBar(
        automaticallyImplyLeading: false,
        elevation: 0,
        backgroundColor: greenColor,
        centerTitle: true,
        iconTheme: const IconThemeData(color: Colors.white),
        title: Text(
          'my_profile'.tr(),
          style: const TextStyle(
            color: Colors.white,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          const SizedBox(height: 10),
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: cardColor,
              borderRadius: BorderRadius.circular(20),
              boxShadow: [
                BoxShadow(
                  blurRadius: 12,
                  color: shadowColor,
                  offset: const Offset(2, 3),
                ),
              ],
            ),
            child: Column(
              children: [
                Row(
                  children: [
                    CircleAvatar(
                      radius: 40,
                      backgroundColor: greenColor,
                      backgroundImage:
                          userProfileImage != null
                              ? FileImage(userProfileImage!)
                              : null,
                      child:
                          userProfileImage == null
                              ? const Icon(
                                Icons.person,
                                size: 40,
                                color: Colors.white,
                              )
                              : null,
                    ),
                    const SizedBox(width: 16),
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          currentUserName.isEmpty
                              ? 'المستخدم'
                              : currentUserName,
                          style: const TextStyle(
                            fontSize: 20,
                            fontWeight: FontWeight.bold,
                            color: greenColor,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          'your_stats'.tr(),
                          style: TextStyle(
                            color: isDark ? Colors.grey[400] : Colors.grey[600],
                            fontSize: 13,
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
                const SizedBox(height: 20),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceAround,
                  children: [
                    _statCard(
                      Icons.eco_outlined,
                      'my_plants'.tr(),
                      myplants.length.toString(),
                    ),
                    _divider(isDark),
                    _statCard(
                      Icons.coronavirus_outlined,
                      'diseases'.tr(),
                      totalDiseases.toString(),
                    ),
                    _divider(isDark),
                    _statCard(
                      Icons.qr_code_scanner,
                      'scans'.tr(),
                      totalScans.toString(),
                    ),
                  ],
                ),
              ],
            ),
          ),
          const SizedBox(height: 20),
          _button(
            'scan_history'.tr(),
            Icons.history,
            cardColor,
            iconBg,
            textColor,
            shadowColor,
            () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const ScanHistoryPage()),
              );
            },
          ),
          _button(
            'saved_tips'.tr(),
            Icons.star_border,
            cardColor,
            iconBg,
            textColor,
            shadowColor,
            () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const SavedTipsPage()),
              );
            },
          ),
          _button(
            'settings'.tr(),
            Icons.settings,
            cardColor,
            iconBg,
            textColor,
            shadowColor,
            () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (context) => const SettingPage()),
              ).then((_) => setState(() {}));
            },
          ),
          _button(
            'logout'.tr(),
            Icons.logout,
            cardColor,
            iconBg,
            textColor,
            shadowColor,
            () {
              showDialog(
                context: context,
                builder:
                    (context) => AlertDialog(
                      title: Text('logout'.tr()),
                      content: Text('logout_confirm'.tr()),
                      actions: [
                        TextButton(
                          child: Text('no'.tr()),
                          onPressed: () => Navigator.pop(context),
                        ),
                        TextButton(
                          child: Text('yes'.tr()),
                          onPressed: () {
                            Navigator.pushAndRemoveUntil(
                              context,
                              MaterialPageRoute(
                                builder: (context) => const LoginBox(),
                              ),
                              (route) => false,
                            );
                          },
                        ),
                      ],
                    ),
              );
            },
          ),
        ],
      ),
    );
  }

  Widget _statCard(IconData icon, String label, String value) {
    return Column(
      children: [
        Icon(icon, color: const Color.fromARGB(255, 56, 114, 64), size: 26),
        const SizedBox(height: 6),
        Text(
          value,
          style: const TextStyle(
            fontSize: 22,
            fontWeight: FontWeight.bold,
            color: Color.fromARGB(255, 56, 114, 64),
          ),
        ),
        const SizedBox(height: 2),
        Text(label, style: TextStyle(fontSize: 12, color: Colors.grey[600])),
      ],
    );
  }

  Widget _divider(bool isDark) => Container(
    height: 50,
    width: 1,
    color: isDark ? Colors.grey[700] : const Color.fromARGB(255, 201, 222, 203),
  );

  Widget _button(
    String text,
    IconData icon,
    Color cardColor,
    Color iconBg,
    Color textColor,
    Color shadowColor,
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
          child: Icon(icon, color: const Color.fromARGB(255, 56, 114, 64)),
        ),
        title: Text(
          text,
          style: TextStyle(
            fontSize: 15,
            fontWeight: FontWeight.bold,
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
