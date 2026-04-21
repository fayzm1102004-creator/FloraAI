# 🔗 FloraAI API Route Map v2.0

هذه هي الخريطة الشاملة لجميع نقاط الوصول (Endpoints) المتاحة في النظام، مرتبة حسب الصلاحيات والوظيفة.

---

## 🔐 1. قسم المصادقة (Authentication)
| المسار (Endpoint) | الطريقة (Method) | الوصف | الصلاحيات |
| :--- | :--- | :--- | :--- |
| `/api/auth/register` | `POST` | إنشاء حساب مستخدم جديد | للجميع |
| `/api/auth/login` | `POST` | تسجيل الدخول واستلام Access & Refresh Tokens | للجميع |
| `/api/auth/refresh-token` | `POST` | تجديد الـ Access Token المنتهي | للجميع |

---

## 🤖 2. قسم التشخيص والذكاء الاصطناعي (Diagnosis)
| المسار (Endpoint) | الطريقة (Method) | الوصف | مميزات إضافية |
| :--- | :--- | :--- | :--- |
| `/api/diagnosis/scan` | `POST` | تشخيص حالة النبتة بناءً على صورة أو وصف | **Redis Cached** ⚡ |

---

## 🌿 3. قسم نباتات المستخدم (User Plants Library)
*جميع المسارات تتطلب Authorization Header.*

| المسار (Endpoint) | الطريقة (Method) | الوصف | نظام الرد |
| :--- | :--- | :--- | :--- |
| `/api/userplants/save` | `POST` | حفظ نبتة شخصية في مكتبة المستخدم | `UserPlantDto` |
| `/api/userplants/user/{userId}` | `GET` | عرض نباتات مستخدم معين | **PagedResponse** 📄 |
| `/api/userplants/{plantId}` | `GET` | تفاصيل نبتة واحدة | - |
| `/api/userplants/{plantId}/status`| `PUT` | تحديث حالة النبتة (سليم/مريض) | - |

---

## 🔍 4. قسم البحث والمعلومات (Plant Lookup)
| المسار (Endpoint) | الطريقة (Method) | الوصف | نظام الرد |
| :--- | :--- | :--- | :--- |
| `/api/plantlookup/all` | `GET` | عرض قائمة بكل النباتات المدعومة | **PagedResponse** 📄 |
| `/api/plantlookup/search` | `GET` | البحث عن نبتة بالاسم | **PagedResponse** 📄 |

---

## 👑 5. قسم الإدارة (Admin Dashboard)
*خاص بالمستخدمين الذين يمتلكون Role: Admin فقط.*

| المسار (Endpoint) | الطريقة (Method) | الوصف | الأولوية |
| :--- | :--- | :--- | :--- |
| `/api/admin/stats` | `GET` | جلب إحصائيات النظام (مستخدمين، فحوصات، فئات) | **Redis Cached** ⚡ |
| `/api/admin/refresh-condition`| `POST` | إجبار النظام على إعادة توليد تشخيص معين من AI | High |

---

## 📋 6. قواعد عامة للتعامل مع الـ Pagination
عند استدعاء أي Endpoint يدعم **PagedResponse**، يمكنك إرسال المعايير التالية كـ Query Parameters:
*   `pageNumber`: رقم الصفحة (الافتراضي 1).
*   `pageSize`: عدد العناصر في الصفحة (الافتراضي 10).

**شكل استجابة الـ Pagination:**
```json
{
  "data": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalRecords": 100,
  "totalPages": 10,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```
---
**Technical Reference:** FloraAI Enterprise Backend Engine.
