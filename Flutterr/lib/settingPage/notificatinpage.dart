import 'package:flutter/material.dart';

class NotificationsPage extends StatefulWidget {
  const NotificationsPage({super.key});

  @override
  State<NotificationsPage> createState() => _NotificationsPageState();
}

class _NotificationsPageState extends State<NotificationsPage> {
  bool notificationsOn = true;
  bool tipsOn = true;
  bool diseaseAlertsOn = true;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final bgColor =
        isDark ? Colors.grey[900]! : const Color.fromARGB(255, 236, 255, 237);
    final cardColor = isDark ? Colors.grey[850]! : Colors.white;
    final shadowColor =
        isDark ? Colors.black : const Color.fromARGB(255, 149, 234, 179);

    return Scaffold(
      backgroundColor: bgColor,
      appBar: AppBar(
        title: const Text(
          "الإشعارات",
          style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
        ),
        centerTitle: true,
        backgroundColor: const Color.fromARGB(255, 56, 114, 64),
        iconTheme: const IconThemeData(color: Colors.white),
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          const SizedBox(height: 10),

          _sectionTitle("إعدادات الإشعارات", isDark),

          _switchCard(
            icon: Icons.notifications_active,
            title: "تفعيل الإشعارات",
            subtitle: "تشغيل أو إيقاف جميع الإشعارات",
            value: notificationsOn,
            cardColor: cardColor,
            shadowColor: shadowColor,
            isDark: isDark,
            onChanged: (value) {
              setState(() {
                notificationsOn = value;
                if (!value) {
                  tipsOn = false;
                  diseaseAlertsOn = false;
                }
              });
            },
          ),

          const SizedBox(height: 12),
          _sectionTitle("أنواع الإشعارات", isDark),

          _switchCard(
            icon: Icons.coronavirus_outlined,
            title: "تنبيهات الأمراض",
            subtitle: "إشعار عند اكتشاف مرض في نبتة",
            value: diseaseAlertsOn,
            cardColor: cardColor,
            shadowColor: shadowColor,
            isDark: isDark,
            onChanged:
                notificationsOn
                    ? (value) => setState(() => diseaseAlertsOn = value)
                    : null,
          ),

          const SizedBox(height: 10),

          _switchCard(
            icon: Icons.lightbulb_outline,
            title: "النصائح اليومية",
            subtitle: "احصل على نصائح يومية للعناية بنباتاتك",
            value: tipsOn,
            cardColor: cardColor,
            shadowColor: shadowColor,
            isDark: isDark,
            onChanged:
                notificationsOn
                    ? (value) => setState(() => tipsOn = value)
                    : null,
          ),
        ],
      ),
    );
  }

  Widget _sectionTitle(String title, bool isDark) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 10, right: 4),
      child: Text(
        title,
        style: TextStyle(
          fontSize: 15,
          fontWeight: FontWeight.bold,
          color: isDark ? Colors.grey[400] : Colors.grey[600],
        ),
      ),
    );
  }

  Widget _switchCard({
    required IconData icon,
    required String title,
    required String subtitle,
    required bool value,
    required Color cardColor,
    required Color shadowColor,
    required bool isDark,
    required Function(bool)? onChanged,
  }) {
    final iconBg =
        onChanged != null
            ? (isDark
                ? Colors.grey[700]!
                : const Color.fromARGB(255, 219, 243, 221))
            : (isDark ? Colors.grey[800]! : Colors.grey[100]!);

    final titleColor =
        onChanged != null
            ? (isDark ? Colors.white : Colors.black)
            : Colors.grey;

    return Container(
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
      child: SwitchListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
        secondary: Container(
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(
            color: iconBg,
            borderRadius: BorderRadius.circular(10),
          ),
          child: Icon(
            icon,
            color:
                onChanged != null
                    ? const Color.fromARGB(255, 56, 114, 64)
                    : Colors.grey,
            size: 24,
          ),
        ),
        title: Text(
          title,
          style: TextStyle(
            fontWeight: FontWeight.bold,
            fontSize: 15,
            color: titleColor,
          ),
        ),
        subtitle: Text(
          subtitle,
          style: TextStyle(fontSize: 12, color: Colors.grey[500]),
        ),
        value: value,
        onChanged: onChanged,
        activeColor: const Color.fromARGB(255, 56, 114, 64),
      ),
    );
  }
}
