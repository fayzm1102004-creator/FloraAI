class ScanRecord {
  final String scanId;
  final String plantType;
  final String conditionName;
  final String detectedCategory;
  final String? photoPath;
  final DateTime scannedAt;
  final String status;

  ScanRecord({
    required this.scanId,
    required this.plantType,
    required this.conditionName,
    required this.detectedCategory,
    this.photoPath,
    required this.scannedAt,
    this.status = 'Pending_Sync',
  });

  Map<String, dynamic> toMap() {
    return {
      'scanId': scanId,
      'plantType': plantType,
      'conditionName': conditionName,
      'detectedCategory': detectedCategory,
      'photoPath': photoPath,
      'scannedAt': scannedAt.toIso8601String(),
      'status': status,
    };
  }

  factory ScanRecord.fromMap(Map<String, dynamic> map) {
    return ScanRecord(
      scanId: map['scanId'],
      plantType: map['plantType'],
      conditionName: map['conditionName'],
      detectedCategory: map['detectedCategory'],
      photoPath: map['photoPath'],
      scannedAt: DateTime.parse(map['scannedAt']),
      status: map['status'] ?? 'Pending_Sync',
    );
  }
}
