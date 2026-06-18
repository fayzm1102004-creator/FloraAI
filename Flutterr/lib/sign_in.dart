import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';
import 'services/api_service.dart';
import 'sign_up.dart';

class Fields extends StatefulWidget {
  final VoidCallback onBack;
  const Fields({super.key, required this.onBack});

  @override
  State<Fields> createState() => _FieldsState();
}

class _FieldsState extends State<Fields> {
  final GlobalKey<FormState> formstate = GlobalKey();
  final TextEditingController emailController = TextEditingController();
  final TextEditingController passwordController = TextEditingController();
  bool isLoading = false;

  final emailReg = RegExp(
    r'^[A-Za-z0-9]+@[A-Za-z]+\.com$',
    caseSensitive: false,
  );

  Future<void> handleLogin() async {
    if (!formstate.currentState!.validate()) return;
    setState(() => isLoading = true);
    final result = await ApiService.login(
      emailController.text.trim(),
      passwordController.text,
    );
    setState(() => isLoading = false);
    if (!mounted) return;
    if (result['success']) {
      currentUserName = result['data']['user']?['fullName'] ?? '';
      Navigator.of(context).pushReplacementNamed('/home');
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(result['message'] ?? 'login_failed'.tr()),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Form(
      key: formstate,
      child: Column(
        key: const ValueKey('signin'),
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Text(
            'sign_in'.tr(),
            style: const TextStyle(
              fontSize: 24,
              color: Colors.white,
              fontWeight: FontWeight.w900,
            ),
          ),
          const SizedBox(height: 20),
          SizedBox(
            width: 220,
            child: TextFormField(
              controller: emailController,
              validator: (value) {
                if (value!.isEmpty) return 'field_empty'.tr();
                if (!emailReg.hasMatch(value.trim()))
                  return 'invalid_email'.tr();
                return null;
              },
              decoration: InputDecoration(
                labelText: 'email_field'.tr(),
                labelStyle: const TextStyle(
                  color: Color.fromARGB(255, 185, 185, 185),
                ),
                enabledBorder: const UnderlineInputBorder(
                  borderSide: BorderSide(color: Colors.white70),
                ),
                focusedBorder: const UnderlineInputBorder(
                  borderSide: BorderSide(color: Colors.white),
                ),
              ),
              style: const TextStyle(color: Colors.white),
            ),
          ),
          const SizedBox(height: 15),
          SizedBox(
            width: 220,
            child: TextFormField(
              controller: passwordController,
              validator: (value) => value!.isEmpty ? 'field_empty'.tr() : null,
              obscureText: true,
              decoration: InputDecoration(
                labelText: 'password_field'.tr(),
                labelStyle: const TextStyle(
                  color: Color.fromARGB(255, 185, 185, 185),
                ),
                enabledBorder: const UnderlineInputBorder(
                  borderSide: BorderSide(color: Colors.white70),
                ),
                focusedBorder: const UnderlineInputBorder(
                  borderSide: BorderSide(color: Colors.white),
                ),
              ),
              style: const TextStyle(color: Colors.white),
            ),
          ),
          const SizedBox(height: 25),
          ElevatedButton(
            style: ElevatedButton.styleFrom(
              backgroundColor: const Color.fromARGB(255, 18, 112, 65),
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(15),
              ),
              padding: const EdgeInsets.symmetric(horizontal: 60, vertical: 10),
            ),
            onPressed: isLoading ? null : handleLogin,
            child:
                isLoading
                    ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(
                        color: Colors.white,
                        strokeWidth: 2,
                      ),
                    )
                    : Text('login_btn'.tr()),
          ),
          const SizedBox(height: 10),
          TextButton(
            onPressed: widget.onBack,
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
