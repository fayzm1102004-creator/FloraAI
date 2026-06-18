import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';
import 'package:project/welcome_screen.dart';
import 'sign_in.dart';
import 'sign_up.dart';

class LoginBox extends StatefulWidget {
  const LoginBox({super.key});

  @override
  State<LoginBox> createState() => _LoginBoxState();
}

class _LoginBoxState extends State<LoginBox> {
  bool showSignIn = false;
  bool showSignUp = false;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color.fromARGB(255, 14, 20, 14),
      body: Stack(
        children: [
          Container(
            decoration: const BoxDecoration(
              image: DecorationImage(
                image: AssetImage('images/plan3.jpg'),
                fit: BoxFit.cover,
              ),
            ),
          ),
          Center(
            child: ClipRRect(
              borderRadius: BorderRadius.circular(15),
              child: BackdropFilter(
                filter: ImageFilter.blur(sigmaX: 7, sigmaY: 7),
                child: Container(
                  width: 300,
                  height: 400,
                  decoration: BoxDecoration(
                    border: Border.all(
                      color: const Color.fromARGB(255, 198, 198, 198),
                      width: 0.2,
                    ),
                    color: const Color.fromARGB(
                      255,
                      63,
                      63,
                      63,
                    ).withOpacity(0.1),
                    borderRadius: BorderRadius.circular(15),
                  ),
                  child: AnimatedSwitcher(
                    duration: const Duration(milliseconds: 350),
                    transitionBuilder: (
                      Widget child,
                      Animation<double> animation,
                    ) {
                      final offsetAnimation = Tween<Offset>(
                        begin:
                            showSignIn
                                ? const Offset(1, 0)
                                : const Offset(-1, 0),
                        end: Offset.zero,
                      ).animate(animation);
                      return SlideTransition(
                        position: offsetAnimation,
                        child: child,
                      );
                    },
                    child:
                        showSignIn
                            ? Fields(
                              onBack: () => setState(() => showSignIn = false),
                            )
                            : showSignUp
                            ? SignUpFields(
                              onBack: () => setState(() => showSignUp = false),
                            )
                            : buildMainButtons(),
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget buildMainButtons() {
    return SingleChildScrollView(
      child: Column(
        key: const ValueKey('main'),
        children: [
          const SizedBox(height: 15),
          Container(
            padding: const EdgeInsets.only(left: 170, top: 40),
            child: Text(
              'hello'.tr(),
              style: const TextStyle(
                fontWeight: FontWeight.w900,
                fontSize: 25,
                color: Color.fromARGB(255, 201, 201, 201),
              ),
            ),
          ),
          Container(
            padding: const EdgeInsets.only(left: 87),
            child: Text(
              'identify_fast'.tr(),
              style: const TextStyle(
                color: Color.fromARGB(255, 214, 214, 214),
                fontSize: 12,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
          const SizedBox(height: 20),
          ElevatedButton(
            style: ElevatedButton.styleFrom(
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(15),
              ),
              fixedSize: const Size(230, 42),
              backgroundColor: const Color.fromARGB(255, 18, 112, 65),
              foregroundColor: Colors.white,
            ),
            onPressed:
                () => setState(() {
                  showSignIn = true;
                  showSignUp = false;
                }),
            child: Text('sign_in'.tr()),
          ),
          const SizedBox(height: 10),
          ElevatedButton(
            style: ElevatedButton.styleFrom(
              side: const BorderSide(color: Colors.white, width: 0.5),
              backgroundColor: Colors.transparent,
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(15),
              ),
              fixedSize: const Size(230, 42),
            ),
            onPressed:
                () => setState(() {
                  showSignUp = true;
                  showSignIn = false;
                }),
            child: Text('sign_up'.tr()),
          ),
          const SizedBox(height: 10),
          TextButton(
            onPressed:
                () => Navigator.push(
                  context,
                  MaterialPageRoute(builder: (context) => WelcomeScreens()),
                ),
            child: Text(
              'back'.tr(),
              style: const TextStyle(color: Colors.white70),
            ),
          ),
        ],
      ),
    );
  }
}
