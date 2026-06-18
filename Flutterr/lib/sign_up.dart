import 'dart:io';
import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';
import 'services/api_service.dart';

String currentUserName = "";
File? userProfileImage;

class SignUpFields extends StatefulWidget {
  final VoidCallback onBack;
  const SignUpFields({super.key, required this.onBack});

  @override
  State<SignUpFields> createState() => _SignUpFieldsState();
}

class _SignUpFieldsState extends State<SignUpFields> {
  final GlobalKey<FormState> formstate = GlobalKey();
  final firstNameController = TextEditingController();
  final lastNameController = TextEditingController();
  final emailController = TextEditingController();
  final passwordController = TextEditingController();
  final confirmPasswordController = TextEditingController();
  bool isLoading = false;

  final emailreg = RegExp(
    r'^[A-Za-z0-9]+@[A-Za-z]+\.com$',
    caseSensitive: false,
  );

  Future<void> handleRegister() async {
    if (!formstate.currentState!.validate()) return;
    setState(() => isLoading = true);
    final result = await ApiService.register(
      firstName: firstNameController.text.trim(),
      lastName: lastNameController.text.trim(),
      email: emailController.text.trim(),
      password: passwordController.text,
      // ← شلنا phone
    );
    setState(() => isLoading = false);
    if (!mounted) return;
    if (result['success']) {
      currentUserName =
          '${firstNameController.text.trim()} ${lastNameController.text.trim()}';
      Navigator.of(context).pushReplacementNamed('/home');
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(result['message'] ?? 'register_failed'.tr()),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  Widget buildTextField(
    String label, {
    bool isPassword = false,
    bool isEmail = false,
    bool isConfirmPassword = false,
    TextInputType? keyboardType,
    TextEditingController? controller,
  }) {
    return SizedBox(
      width: 200,
      child: TextFormField(
        controller: controller,
        validator: (value) {
          if (value!.isEmpty) return 'field_empty'.tr();
          if (isEmail && !emailreg.hasMatch(value.trim()))
            return 'invalid_email_format'.tr();
          if (isConfirmPassword && value != passwordController.text)
            return 'passwords_not_match'.tr();
          return null;
        },
        obscureText: isPassword,
        keyboardType: keyboardType,
        decoration: InputDecoration(
          labelText: label,
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
    );
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      child: Form(
        key: formstate,
        child: Column(
          key: const ValueKey('signup'),
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const SizedBox(height: 22),
            Text(
              'create_account'.tr(),
              style: const TextStyle(
                fontSize: 22,
                color: Colors.white,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 20),
            buildTextField('first_name'.tr(), controller: firstNameController),
            const SizedBox(height: 15),
            buildTextField('last_name'.tr(), controller: lastNameController),
            const SizedBox(height: 15),
            buildTextField(
              'email_field'.tr(),
              keyboardType: TextInputType.emailAddress,
              isEmail: true,
              controller: emailController,
            ),
            const SizedBox(height: 15),
            // ← شلنا phone field خالص
            buildTextField(
              'password_field'.tr(),
              isPassword: true,
              controller: passwordController,
            ),
            const SizedBox(height: 15),
            buildTextField(
              'confirm_password'.tr(),
              isPassword: true,
              controller: confirmPasswordController,
              isConfirmPassword: true,
            ),
            const SizedBox(height: 25),
            ElevatedButton(
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color.fromARGB(255, 18, 112, 65),
                foregroundColor: Colors.white,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(15),
                ),
                padding: const EdgeInsets.symmetric(
                  horizontal: 60,
                  vertical: 10,
                ),
              ),
              onPressed: isLoading ? null : handleRegister,
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
                      : Text('create_account_btn'.tr()),
            ),
            const SizedBox(height: 10),
            TextButton(
              onPressed: widget.onBack,
              child: Text(
                'back'.tr(),
                style: const TextStyle(color: Colors.white70),
              ),
            ),
            const SizedBox(height: 18),
          ],
        ),
      ),
    );
  }
}
