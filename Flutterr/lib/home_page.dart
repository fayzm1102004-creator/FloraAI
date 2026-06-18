import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';
import 'profile.dart';
import 'home.dart';
import 'botAi.dart';
import 'MyGarden.dart';

class Home extends StatefulWidget {
  const Home({super.key});

  @override
  State<Home> createState() => _HomeState();
}

class _HomeState extends State<Home> {
  int currentIndex = 3;

  final List<IconData> unselectedIcons = [
    Icons.person_2_outlined,
    Icons.camera_enhance_outlined,
    Icons.eco_outlined,
    Icons.other_houses_outlined,
  ];

  final List<IconData> selectedIcons = [
    Icons.person,
    Icons.camera_enhance,
    Icons.eco,
    Icons.other_houses,
  ];

  final List<Widget> page = [
    Profilepage(),
    Botaipage(),
    Gardenpage(),
    Homepage(),
  ];

  @override
  Widget build(BuildContext context) {
    final List<String> labels = [
      'my_profile'.tr(),
      'identify_plant'.tr(),
      'my_garden'.tr(),
      'home'.tr(),
    ];

    return Scaffold(
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: currentIndex,
        onTap: (value) => setState(() => currentIndex = value),
        type: BottomNavigationBarType.fixed,
        selectedLabelStyle: const TextStyle(
          fontSize: 15,
          fontWeight: FontWeight.bold,
        ),
        iconSize: 25,
        selectedIconTheme: const IconThemeData(size: 32),
        selectedItemColor: const Color.fromARGB(255, 59, 121, 61),
        unselectedItemColor: const Color.fromARGB(255, 67, 78, 73),
        items: List.generate(labels.length, (val) {
          return BottomNavigationBarItem(
            icon: Icon(
              currentIndex == val ? selectedIcons[val] : unselectedIcons[val],
            ),
            label: labels[val],
          );
        }),
      ),
      body: page[currentIndex],
    );
  }
}
