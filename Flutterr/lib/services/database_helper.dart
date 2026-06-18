import 'package:sqflite/sqflite.dart';
import 'package:path/path.dart';
import '../models/scan_record.dart';

class DatabaseHelper {
  static final DatabaseHelper instance = DatabaseHelper._init();
  static Database? _database;

  DatabaseHelper._init();

  Future<Database> get database async {
    if (_database != null) return _database!;
    _database = await _initDB('flora_ai.db');
    return _database!;
  }

  Future<Database> _initDB(String filePath) async {
    final dbPath = await getDatabasesPath();
    final path = join(dbPath, filePath);

    return await openDatabase(
      path,
      version: 1,
      onCreate: _createDB,
    );
  }

  Future _createDB(Database db, int version) async {
    await db.execute('''
      CREATE TABLE OfflineScansQueue (
        scanId TEXT PRIMARY KEY,
        plantType TEXT NOT NULL,
        conditionName TEXT NOT NULL,
        detectedCategory TEXT NOT NULL,
        photoPath TEXT,
        scannedAt TEXT NOT NULL,
        status TEXT NOT NULL
      )
    ''');
  }

  Future<void> insertScan(ScanRecord record) async {
    final db = await instance.database;
    await db.insert(
      'OfflineScansQueue',
      record.toMap(),
      conflictAlgorithm: ConflictAlgorithm.replace,
    );
  }

  Future<List<ScanRecord>> getPendingScans() async {
    final db = await instance.database;
    final maps = await db.query(
      'OfflineScansQueue',
      where: 'status = ?',
      whereArgs: ['Pending_Sync'],
      orderBy: 'scannedAt ASC', // Process oldest first
    );

    if (maps.isNotEmpty) {
      return maps.map((map) => ScanRecord.fromMap(map)).toList();
    } else {
      return [];
    }
  }

  Future<void> updateScanStatus(String scanId, String status) async {
    final db = await instance.database;
    await db.update(
      'OfflineScansQueue',
      {'status': status},
      where: 'scanId = ?',
      whereArgs: [scanId],
    );
  }

  Future<void> deleteSyncedScans() async {
    final db = await instance.database;
    await db.delete(
      'OfflineScansQueue',
      where: 'status = ?',
      whereArgs: ['Synced'],
    );
  }
}
