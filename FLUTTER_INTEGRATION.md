# 🌿 FloraAI Flutter Integration Guide v1.0

أهلاً بك في الدليل التقني لربط تطبيق **Flutter** مع **FloraAI Backend**. هذا الملف موجه لمطوري فلاتر لشرح كيفية استخدام نقاط الوصول (API Endpoints) بشكل احترافي.

---

## 🌐 1. الروابط الأساسية (Base URL)

| البيئة (Environment) | الرابط (URL) | الوصف |
| :--- | :--- | :--- |
| **Emulator** | `http://10.0.2.2:5098` | للتشغيل على محاكي الأندرويد |
| **Real Device** | `http://[IPv4_Address]:5098` | للتشغيل على موبايل حقيقي (نفس الشبكة) |
| **Production** | `https://api.floraai.com` | رابط السيرفر المباشر (عند الرفع) |

> ⚠️ **ملاحظة أمنية:** جميع الروابط (ما عدا التسجيل والدخول) تتطلب إرسال الـ Token في الـ Header كـ:  
> `Authorization: Bearer {Your_Token}`

---

## 🔐 2. قسم المصادقة (Authentication)

### 🔹 إنشاء حساب جديد (Register)
*   **Endpoint:** `/api/auth/register` | **Method:** `POST`
*   **Headers:** `Content-Type: application/json`
*   **Request Body:**
```json
{
  "fullName": "Name",
  "email": "user@example.com",
  "password": "Password123"
}
```
*   **Response (201 Created):** `{"id": 1, "fullName": "Name", "email": "user@example.com"}`

---

### 🔹 تسجيل الدخول (Login)
*   **Endpoint:** `/api/auth/login` | **Method:** `POST`
*   **Request Body:** `{"email": "user@example.com", "password": "Password123"}`
*   **Response (200 OK):** `{"id": 1, "token": "eyJhbG... (Bearer Token)"}`

#### 💻 Dart Example (Login):
```dart
Future<String?> login(String email, String password) async {
  final response = await http.post(
    Uri.parse('$baseUrl/api/auth/login'),
    body: jsonEncode({'email': email, 'password': password}),
    headers: {'Content-Type': 'application/json'},
  );
  if (response.statusCode == 200) {
    return jsonDecode(response.body)['token'];
  }
  return null;
}
```

---

## 🤖 3. قسم التشخيص الهجين (Diagnosis)

### 🔹 فحص النبات (Hybrid Scan)
هذا هو الرابط الأهم؛ حيث يستقبل تصنيف موديل الموبايل ويرد بالروشتة العلاجية.

*   **Endpoint:** `/api/diagnosis/scan` | **Method:** `POST`
*   **Headers:** `Authorization: Bearer {Token}`
*   **Request Body:**
```json
{
  "plantType": "Tomato",
  "conditionName": "Unknown/User description",
  "detectedCategory": "فطريات" 
}
```
*   **Response (200 OK):**
```json
{
  "plantType": "Tomato",
  "conditionName": "البياض الدقيقي",
  "treatment": "استخدم مبيد فطري نحاسي...",
  "careInstructions": "اعزل النبات...",
  "lastUpdated": "2026-04-21T02:00:00Z"
}
```

💡 **Pro-Tip:** إذا فشل الـ Vision Model في الموبايل في تحديد الفئة، أرسل `detectedCategory` كـ `null`؛ وسيقوم النظام بالتحليل العشوائي من الصفر.

---

## 🌿 4. قسم إدارة نباتات المستخدم (User Plants)

### 🔸 حفظ نبتة في المكتبة
*   **Endpoint:** `/api/userplants/save` | **Method:** `POST`
*   **Request Body:** `{"userId": 1, "nickname": "My Rose", "plantType": "Rose", "currentStatus": "Healthy", ...}`

### 🔸 عرض كل نباتات المستخدم
*   **Endpoint:** `/api/userplants/user/{userId}` | **Method:** `GET`
*   **Response:** `List<UserPlantResponseDto>`

---

## 🔄 5. المزامنة والسجل (Sync & History)

### 🔹 رفع الفحوصات المعلقة (Push Offline)
تُستخدم عند استعادة الإنترنت لإرسال ما تم فحصه Offline ليقوم السيرفر بجلبه من الذكاء الاصطناعي.
*   **Endpoint:** `/api/sync/push` | **Method:** `POST`
*   **Request Body:** 
```json
{
  "pendingScans": [
    {
      "plantType": "Tomato",
      "conditionName": "البقع البنية"
    }
  ]
}
```
*   **Response (200 OK):** سيعيد مصفوفة `diagnosisResults` تحتوي على (العلاج + الرعاية) لكل نبتة تم رفعها.

💡 **Pro-Tips (شاشة ملف النبتة):**
1. **التعامل مع الحالة (Healthy vs Sick):**
   - إذا كانت النبتة سليمة، حقل `SavedTreatment` قد يعود فارغاً أو يحتوي على عبارة "النبات سليم". يُنصح مبرمج الفلاتر بإخفاء قسم "الروشتة" برمجياً إذا كان الحقل فارغاً.
   - حقل `SavedCareInstructions` يجب أن يُعرض دائماً لأنه يحتوي على (جدول الري، الإضاءة، التسميد) سواء كانت النبتة سليمة أو مريضة.
2. **تخزين الـ Token:** قم بتخزين الـ JWT محلياً باستخدام `shared_preferences` أو `flutter_secure_storage`.

---

## 👑 6. قسم الإدارة (Admin Operations)

| الوظيفة | المسار (Endpoint) | الطريقة (Method) |
| :--- | :--- | :--- |
| **عرض كل المستخدمين** | `/api/admin/users` | `GET` |
| **حذف مستخدم** | `/api/admin/users/{userId}` | `DELETE` |

---
**إعداد مطور الباك إند: [اسمك] 💻✨**
