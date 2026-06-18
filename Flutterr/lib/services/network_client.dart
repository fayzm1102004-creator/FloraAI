import 'dart:convert';
import 'package:dio/dio.dart';

class NetworkClient {
  static const String baseUrl =
      'https://easygoing-quietude-production-9004.up.railway.app'; // ← اللينك الجديد

  static Dio? _dio;

  static Dio get instance {
    _dio ??= _createDio();
    return _dio!;
  }

  static Dio _createDio() {
    final dio = Dio(
      BaseOptions(
        baseUrl: baseUrl,
        connectTimeout: const Duration(seconds: 30),
        receiveTimeout: const Duration(seconds: 30),
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
          // ← شلنا الـ Basic Auth لأن السيرفر الجديد مش محتاجه
        },
      ),
    );

    dio.interceptors.add(_AuthInterceptor());
    dio.interceptors.add(_LogInterceptor());

    return dio;
  }
}

// ── Interceptor بيضيف الـ Bearer Token لو موجود ──
class _AuthInterceptor extends Interceptor {
  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    handler.next(options);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    if (err.response?.data is String &&
        (err.response!.data as String).contains('<html')) {
      err = err.copyWith(message: 'السيرفر رجع HTML — تأكد من الـ endpoint');
    }
    handler.next(err);
  }
}

// ── Interceptor للـ Logging ──
class _LogInterceptor extends Interceptor {
  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    print('📤 REQUEST: ${options.method} ${options.path}');
    handler.next(options);
  }

  @override
  void onResponse(Response response, ResponseInterceptorHandler handler) {
    print('✅ RESPONSE: ${response.statusCode} ${response.requestOptions.path}');
    handler.next(response);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    print('❌ ERROR: ${err.message} | ${err.response?.statusCode}');
    handler.next(err);
  }
}
