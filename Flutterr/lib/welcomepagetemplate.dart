import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';

class WelcomePageTemplate extends StatefulWidget {
  final String image, title, desc;
  final IconData icon;
  final Color iconColor;
  final double overlayOpacity;
  final VoidCallback onNext;
  final bool isLastPage;

  const WelcomePageTemplate({
    super.key,
    required this.image,
    required this.title,
    required this.desc,
    required this.icon,
    required this.iconColor,
    required this.overlayOpacity,
    required this.onNext,
    this.isLastPage = false,
  });

  @override
  State<WelcomePageTemplate> createState() => _WelcomePageTemplateState();
}

class _WelcomePageTemplateState extends State<WelcomePageTemplate> {
  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        Container(
          decoration: BoxDecoration(
            image: DecorationImage(
              image: AssetImage(widget.image),
              fit: BoxFit.cover,
            ),
          ),
          child: Container(
            color: Colors.black.withOpacity(widget.overlayOpacity),
          ),
        ),
        SafeArea(
          child: Column(
            children: [
              const Spacer(),
              Icon(widget.icon, color: widget.iconColor, size: 90),
              const SizedBox(height: 25),
              Text(
                widget.title,
                textAlign: TextAlign.center,
                style: const TextStyle(
                  color: Color.fromARGB(255, 243, 255, 225),
                  fontSize: 15,
                ),
              ),
              const SizedBox(height: 16),
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 20),
                child: Text(
                  widget.desc,
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                    color: Color.fromARGB(255, 241, 255, 228),
                    fontSize: 13,
                  ),
                ),
              ),
              const Spacer(),
              Padding(
                padding: const EdgeInsets.symmetric(
                  horizontal: 20,
                  vertical: 20,
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    TextButton(
                      onPressed:
                          widget.isLastPage
                              ? null
                              : () => Navigator.of(
                                context,
                              ).pushReplacementNamed('/login'),
                      child: Text(
                        widget.isLastPage ? '' : 'skip'.tr(),
                        style: const TextStyle(
                          color: Colors.white,
                          fontWeight: FontWeight.bold,
                          fontSize: 14,
                        ),
                      ),
                    ),
                    ElevatedButton(
                      onPressed:
                          widget.isLastPage
                              ? () => Navigator.of(
                                context,
                              ).pushReplacementNamed('/login')
                              : widget.onNext,
                      style: ElevatedButton.styleFrom(
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                        backgroundColor:
                            widget.isLastPage
                                ? const Color.fromARGB(255, 82, 68, 0)
                                : const Color.fromARGB(255, 72, 82, 0),
                        padding: const EdgeInsets.symmetric(
                          horizontal: 30,
                          vertical: 8,
                        ),
                      ),
                      child: Text(
                        widget.isLastPage ? 'start_now'.tr() : 'next'.tr(),
                        style: const TextStyle(
                          fontSize: 13,
                          color: Colors.white,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
