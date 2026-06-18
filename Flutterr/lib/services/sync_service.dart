import 'dart:async';
import 'package:connectivity_plus/connectivity_plus.dart';
import 'database_helper.dart';
import 'api_service.dart';

class SyncService {
  static final SyncService instance = SyncService._init();
  
  late StreamSubscription<List<ConnectivityResult>> _connectivitySubscription;
  bool _isSyncing = false;

  SyncService._init();

  void initialize() {
    _connectivitySubscription = Connectivity().onConnectivityChanged.listen((List<ConnectivityResult> results) {
      if (results.contains(ConnectivityResult.mobile) || results.contains(ConnectivityResult.wifi)) {
        print("🌐 Network reconnected. Triggering background sync...");
        _processPendingScans();
      }
    });
  }

  void dispose() {
    _connectivitySubscription.cancel();
  }

  Future<void> _processPendingScans() async {
    if (_isSyncing) return;
    _isSyncing = true;

    try {
      final pendingScans = await DatabaseHelper.instance.getPendingScans();
      if (pendingScans.isEmpty) {
        _isSyncing = false;
        return;
      }

      print("🔄 Found ${pendingScans.length} pending scans. Starting sync...");

      for (var scan in pendingScans) {
        // We do not have a multipart upload for photos in the API Service yet, 
        // so we send the metadata exactly as requested for the idempotent API.
        final result = await ApiService.scanPlant(
          plantType: scan.plantType,
          conditionName: scan.conditionName,
          detectedCategory: scan.detectedCategory,
          scanId: scan.scanId,
          scannedAt: scan.scannedAt.toIso8601String(),
        );

        if (result['success'] == true) {
          // Success (200/201 from Backend) -> Mark as Synced
          await DatabaseHelper.instance.updateScanStatus(scan.scanId, 'Synced');
          print("✅ Successfully synced scan: ${scan.scanId}");
        } else {
          print("⚠️ Failed to sync scan: ${scan.scanId}. Will retry later.");
          // We break or continue depending on whether we want to stop on first failure.
          // Continuing is fine, maybe next one succeeds.
        }
      }
      
      // Optional: Clean up synced items queue to save space
      await DatabaseHelper.instance.deleteSyncedScans();
      print("🧹 Cleaned up synced items from SQLite.");

    } catch (e) {
      print("❌ Error during background sync: $e");
    } finally {
      _isSyncing = false;
    }
  }
}
