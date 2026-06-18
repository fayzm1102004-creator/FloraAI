import 'package:uuid/uuid.dart';
import '../models/scan_record.dart';
import 'api_service.dart';
import 'database_helper.dart';
import 'lru_cache.dart';

class DiagnosisRepository {
  static final DiagnosisRepository instance = DiagnosisRepository._init();
  
  // LRU Cache for plant diagnoses (Max 10 items)
  final LRUCache<String, Map<String, dynamic>> _ramCache = LRUCache<String, Map<String, dynamic>>(10);
  final Uuid _uuid = const Uuid();

  DiagnosisRepository._init();

  /// Gets diagnosis from RAM cache first. If cache miss, calls API.
  Future<Map<String, dynamic>> getDiagnosis({
    required String plantType,
    required String conditionName,
    required String detectedCategory,
    String? languageCode,
  }) async {
    // Generate a unique cache key based on the parameters
    final cacheKey = '${plantType}_${conditionName}_${detectedCategory}_${languageCode ?? "en"}';

    // Layer 1: Check RAM Cache
    final cachedResult = _ramCache.get(cacheKey);
    if (cachedResult != null) {
      return cachedResult;
    }

    // Layer 2: Cache Miss -> Call API
    final result = await ApiService.scanPlant(
      plantType: plantType,
      conditionName: conditionName,
      detectedCategory: detectedCategory,
      languageCode: languageCode,
    );

    // If successful, save to RAM Cache
    if (result['success'] == true) {
      _ramCache.put(cacheKey, result);
    }

    return result;
  }

  /// Saves a scan offline when there is no internet connection.
  Future<void> saveScanOffline({
    required String plantType,
    required String conditionName,
    required String detectedCategory,
    String? photoPath,
  }) async {
    final String scanId = _uuid.v4();
    final DateTime scannedAt = DateTime.now();

    final record = ScanRecord(
      scanId: scanId,
      plantType: plantType,
      conditionName: conditionName,
      detectedCategory: detectedCategory,
      photoPath: photoPath,
      scannedAt: scannedAt,
    );

    try {
      await DatabaseHelper.instance.insertScan(record);
      print("💾 Scan saved offline to SQLite: $scanId");
    } catch (e) {
      print("❌ Error saving scan offline: $e");
      // Fallback: Optionally handle error, but requirement says "do NOT throw an error"
    }
  }
  
  /// Clears the RAM cache (useful for testing or memory pressure)
  void clearCache() {
    _ramCache.clear();
  }
}
