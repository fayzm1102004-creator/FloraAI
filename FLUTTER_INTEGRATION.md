# 🌿 FloraAI Flutter Integration Guide v2.0 (Production-Ready)

أهلاً بك في الدليل التقني المحدث لربط تطبيق **Flutter** مع **FloraAI Backend**. تم تحديث هذا الدليل ليعكس ميزة الأمان المتقدمة ونظام التحميل الجزئي (Pagination).

---

## 🔐 1. نظام الأمان المتطور (Authentication & Refresh Token)

بناءً على معايير الإنتاج، تم فصل الـ `Access Token` عن الـ `Refresh Token` لضمان أمان فائق.

### 🔹 تسجيل الدخول (Login)
*   **Endpoint:** `/api/auth/login` | **Method:** `POST`
*   **Response (200 OK):**
```json
{
  "id": 1,
  "fullName": "Name",
  "email": "user@example.com",
  "role": "User",
  "token": "ACCESS_TOKEN (Expires in 60m)",
  "refreshToken": "REFRESH_TOKEN (Expires in 7 days)"
}
```

### 🔹 آلية تجديد التوكن (Refresh Token Flow)
عندما ينتهي الـ `Access Token` ويرد السيرفر بـ **401 Unauthorized**، لا تطلب من المستخدم تسجيل الدخول مرة أخرى. بدلاً من ذلك، استدعِ هذا الرابط بصمت في الخلفية:

*   **Endpoint:** `/api/auth/refresh-token` | **Method:** `POST`
*   **Request Body:**
```json
{
  "accessToken": "THE_EXPIRED_TOKEN",
  "refreshToken": "THE_SAVED_REFRESH_TOKEN"
}
```
*   **Response (200 OK):** ستحصل على `token` و `refreshToken` جديدين. قم بتحديثهما في التخزين المحلي فوراً.

---

## 📄 2. نظام التحميل الجزئي (Pagination & Infinite Scroll)

لتوفير باقة الإنترنت وسرعة التطبيق، أصبحت الروابط التي تعيد قوائم كبيرة (Lists) تدعم نظام الصفحات `PagedResponse<T>`.

### 🔹 عرض نباتات المستخدم
*   **Endpoint:** `/api/userplants/user/{userId}?pageNumber=1&pageSize=10`
*   **Method:** `GET`
*   **Response Structure:**
```json
{
  "data": [...],          // القائمة الفعلية للنباتات
  "pageNumber": 1,
  "pageSize": 10,
  "totalRecords": 15,     // إجمالي العناصر في الداتا بيز
  "totalPages": 2,        // إجمالي الصفحات
  "hasNextPage": true,    // هل توجد صفحة تالية؟ استخدمها في الـ Infinite Scroll
  "hasPreviousPage": false
}
```

#### 💻 Dart Example (Infinite Scroll Logic):
```dart
Future<void> fetchNextPage() async {
  if (pagedResponse?.hasNextPage ?? true) {
    final nextPage = (pagedResponse?.pageNumber ?? 0) + 1;
    final response = await http.get(
      Uri.parse('$baseUrl/api/userplants/user/$userId?pageNumber=$nextPage&pageSize=10'),
      headers: {'Authorization': 'Bearer $token'}
    );
    // Parse JSON into PagedResponse and append data to your list
  }
}
```

---

## 🤖 3. التشخيص والبحث (Diagnosis & Lookup)

### 🔹 البحث عن النباتات (Plant Lookup)
تمت إضافة Pagination لعملية البحث لتقليل حجم البيانات.
*   **Endpoint:** `/api/plantlookup/search?query=Rose&pageNumber=1&pageSize=5` | **Method:** `GET`

### 🔹 فحص النبات (Diagnosis Scan)
*   **Endpoint:** `/api/diagnosis/scan` | **Method:** `POST`
*   **Tip:** تم تسريع هذا الرابط باستخدام **Redis Caching**. الردود المتكررة لنفس المرض ستكون فورية.

---

## 👑 4. لوحة تحكم الأدمن (Admin Analytics)

نهاية طرفية جديدة مخصصة للأدمن فقط لجلب إحصائيات التطبيق.
*   **Endpoint:** `/api/admin/stats` | **Method:** `GET`
*   **Authorization:** يتطلب مستخدم بـ `role: Admin`.
*   **Response:**
```json
{
  "totalScans": 250,
  "totalUsers": 120,
  "topPlants": [
    { "plantName": "Tomato", "count": 45 },
    { "plantName": "Rose", "count": 30 }
  ],
  "categoryDistribution": [
    { "category": "Fungi", "count": 80 },
    { "category": "Healthy", "count": 60 }
  ]
}
```

---
**Technical Note:** الـ API يدعم الـ Redis Caching في الخلفية لضمان سرعة استجابة فائقة (High Performance).  
**إعداد مطور الباك إند: [Antigravity AI] 💻✨**
