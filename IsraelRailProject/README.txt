# IsraelRailProject

מערכת ניהול טפסי עבודה לרכבת ישראל, המבוססת על **ASP.NET MVC 5** ו-**Web API 2** (על גבי ‎**.NET Framework 4.8**), עם **SQL Server LocalDB** כבסיס נתונים.  
המערכת תומכת בשני סוגי משתמשים: **מנהל** ו-**עובד**. מנהלים פותחים טפסי עבודה, משייכים עובדים וסיכונים, פותחים/סוגרים לחתימות ורואים היסטוריה. עובדים רואים טפסים שהוקצו להם וחותמים.

---

## 🎯 מהות ומטרות הפרויקט
- **מטרה עסקית:** סטנדרטיזציה של פתיחת עבודות שטח, והבטחת עמידה בפרוצדורות בטיחות (שיוך עובדים, סימון סיכונים, חתימות).
- **מהות:** ניהול מחזור חיים של טופס עבודה:
  1) יצירת טופס (פרטי אתר/זמן/סוג עבודה)  
  2) שיוך עובדים וסיכונים  
  3) פתיחה לחתימות, מעקב סטטוס חתימות  
  4) סגירה (רק לאחר שכל החתימות הנדרשות התקבלו)  
  5) היסטוריית טפסים + צפייה מפורטת

---

## 🧰 טכנולוגיות עיקריות
- **Backend:** ASP.NET MVC 5, Web API 2, ‎.NET Framework 4.8  
- **Database:** SQL Server (LocalDB) עם MDF (`App_Data/IsraelRailDb.mdf`)  
- **DAL:** ADO.NET (SqlConnection / SqlCommand)  
- **Frontend:** Razor Views, Bootstrap 5 (CDN), DataTables (CDN), TinyMCE לעריכות טקסט עשיר (במידת הצורך)  
- **אימות בסיסי:** Session-Based (לצרכי POC)

---

## 🗂️ מבנה הפרויקט (תקיות וקבצים מרכזיים)

IsraelRailProject/
├─ App_Data/
│ └─ IsraelRailDb.mdf # מסד הנתונים המקומי (LocalDB)
├─ app/v1/
│ ├─ controllers/
│ │ └─ WorkFormController.cs # API לטפסי עבודה (Create/Status/Close/Update)
│ ├─ DAL/
│ │ ├─ Db.cs # יצירת SqlConnection
│ │ ├─ UserDAL.cs # אימות/CRUD משתמשים + FirstSetup
│ │ ├─ WorkFormDAL.cs # WorkForms: יצירה/סטטוס/גרסאות/עדכון בסיסי/SetRiskItems
│ │ ├─ WorkFormEmployeeDAL.cs # שיוך עובדים לטופס (Add/Remove/Get)
│ │ ├─ WorkFormRiskItemDAL.cs # שיוך סיכונים לטופס (Add/RemoveAll/Get/SetForForm)
│ │ ├─ WorkFormDetailsDAL.cs # שליפת טופס מפורט + עובדים + סיכונים
│ │ └─ WorkFormSignDAL.cs # טפסים ממתינים לחתימה + ביצוע חתימה (Signatures)
│ ├─ models/
│ │ └─ WorkForm.cs
│ └─ models/dtos/
│ ├─ WorkFormDtos.cs # EmployeeDto / RiskItemDto / WorkFormHistoryDto / WorkFormForSignDto
│ └─ WorkFormDetailsDto.cs # WorkFormDetailsDto + Employees + Risks
├─ Controllers/
│ ├─ AuthController.cs # התחברות/יציאה + FirstSetup (כניסה ראשונה)
│ ├─ UiController.cs # דפי מנהל: WorkForm / Risks / History
│ ├─ EmployeesController.cs # MVC: ניהול עובדים (מנהל בלבד)
│ ├─ EmployeeController.cs # MVC: איזור עובד (טפסים לחתימה) + Sign
│ └─ HistoryController.cs # MVC: רשימת היסטוריה + צפייה בטופס
├─ Views/
│ ├─ Shared/_Layout.cshtml # תפריט צד, RTL, Bootstrap
│ ├─ Auth/Login.cshtml # התחברות (מס' עובד + סיסמה)
│ ├─ Auth/FirstSetup.cshtml # השלמת פרטים בכניסה ראשונה
│ ├─ WorkForm/Index.cshtml # פתיחת טופס (בחירת עובדים/סיכונים מרובים + שליחה)
│ ├─ Risks/Index.cshtml # ניהול סיכונים (CRUD + DataTables)
│ ├─ Employees/Index.cshtml # ניהול עובדים (טופס הוספה + מודאל עריכה)
│ ├─ Employee/Home.cshtml # איזור עובד – טפסים שממתינים לחתימה (צפייה/חתימה)
│ └─ History/
│ ├─ Index.cshtml # היסטוריית טפסים + כפתור "צפייה"
│ └─ View.cshtml # צפייה מפורטת בטופס (פרטים, עובדים/חתימות, סיכונים)
├─ App_Start/RouteConfig.cs # ראוטים MVC (WorkForm/Default)
└─ Web.config



--------------------------------------------------------------------------------------


## 🗄️ מסד נתונים – טבלאות ולוגיקה
- **Users**: `Id, FullName, Email, Pass, Role('Manager'/'Employee')`
- **WorkForms**: `Id, ManagerId(FK Users), Site, WorkDateTime(UTC), WorkType, Status('Draft'/'Open'/'Closed'), Version, OriginalFormId, CreatedAt`
- **WorkFormEmployees**: שיוך N:N בין טופס לעובדים (`WorkFormId, EmployeeId`, PK משולב)
- **RiskItems**: מאסטר סיכונים (`Id, Name`)
- **WorkFormRiskItems**: שיוך N:N בין טופס לסיכונים
- **Signatures**: חתימות עובדים על טופס (`WorkFormId, EmployeeId, SignedAt`)

**זרימה עיקרית:**  
מנהל יוצר **WorkForm**, משייך עובדים (WorkFormEmployees) וסיכונים (WorkFormRiskItems), פותח ל-חתימות (**Status='Open'**). עובד שנשויך רואה את הטופס באיזור האישי ויכול **לחתום** (רשומה בטבלת Signatures). רק כשהחתימות הושלמו – ניתן לסגור (**Status='Closed'**).

--------------------------------------------------------------------------------------


## 🚀 הפעלה – מהר ל-רוץ
1. **שכפול הפרויקט:**
   ```bash
   git clone https://github.com/snir1988/IsraelRailProject.git


   פתח/י את הפתרון ב-Visual Studio (הסגול).

NuGet Restore:
Build → Restore NuGet Packages.

LocalDB:
ודא/י ש־App_Data/IsraelRailDb.mdf קיים ונגיש. אם המסד חדש/ריק – צור טבלאות לפי הסכימה למעלה (או קובץ סקריפט משלך).

הרצה (F5):
הגע/י אל ~/Auth/Login.


-----------------------------------------------------------------------------------


🔐 התחברות (Login) ו-First Setup

התחברות מתבצעת עם:

מספר עובד = Users.Id

סיסמה = Users.Pass
אם העובד הוזן ע"י מנהל ללא פרטים מלאים (למשל ללא Email/Pass), בלוגין ראשון יופנה ל־FirstSetup להשלים: שם מלא/אימייל/סיסמה.

לאחר התחברות:

מנהל יופנה ל-/Ui/WorkForm (פתיחת טופס, ניהול עובדים/סיכונים, היסטוריה).

עובד יופנה ל-/Employee/Home (טפסים שממתינים לחתימתו, וצפייה בטופס).

טיפ: כדי להתחבר כמנהל – ודא/י שיש שורה ב־Users עם Role='Manager'. את ה-Id ניתן לראות ב-Query פשוט (או ב-SSMS) ולהשתמש בו במסך ההתחברות

לשים לב- המזהה id של מנהל הוא 1 מה שאומר שתהחברות ראשונית חייבת להיות של מנהל על מנת לההכניס למערכת את העובדים ולקבל  עבורם מספר מזהה



-----------------------------------------------------------------------------------

👨‍💼 אזור מנהל – יכולות עיקריות

טופס חדש (/Ui/WorkForm)
בחירת עובדים מרובים + סיכונים מרובים → POST /api/v1/workforms שומר:

רשומת WorkForm

שיוכי עובדים (WorkFormEmployees)

שיוכי סיכונים (WorkFormRiskItems)
לאחר יצירה ניתן לפתוח לחתימות, לראות סטטוס, ולסגור.

ניהול עובדים (/Employees/Index)
הוספה/עריכה/מחיקה. עובדים חדשים מקבלים כברירת מחדל Role='Employee'.

ניהול סיכונים (/Ui/Risks)
CRUD לטבלת RiskItems.

היסטוריה (/History/Index)
רשימת טפסים עם פרטים ו-Counters של חתימות. כפתור צפייה → /History/View/{id} להצגה מלאה, כולל רשימת עובדים וחתימות + סיכונים שסומנו.

----------------------------------------------------------------------------------

👷‍♀️ אזור עובד – יכולות עיקריות

הטפסים שלי (/Employee/Home)
רואה רק טפסים שהוא שובץ אליהם ושעדיין לא חתם עליהם.
מכל שורה:

צפייה בטופס המלא (/History/View/{id})

חתום – יוצר/מעדכן רשומה ב־Signatures.



-----------------------------------------------------------------------------------
🔗 API עיקריים (תמצית)

POST /api/v1/workforms – יצירה:

{
  "ManagerId": 1,
  "Site": "אשדוד מזרח",
  "WorkDateTime": "2025-08-14T06:30:00Z",
  "WorkType": "תחזוקת מסילה",
  "EmployeeIds": [2,3],
  "RiskItemIds": [1,3]
}


POST /api/v1/workforms/{id}/send – פתיחה לחתימות (Status='Open')

GET /api/v1/workforms/{id}/status – סטטוס חתימות (חתומים/ממתינים)

POST /api/v1/workforms/{id}/close – סגירה (רק אם אין ממתינים)

PUT /api/v1/workforms/{id} – גרסה חדשה (כלל 12 שעות), החלפת עובדים/סיכונים



-----------------------------------------------------------------------------------

🛡️ הערות אבטחה ותפעול

סיסמאות נשמרות Plain-Text לצורכי POC → לפרודקשן חובה Hash+Salt (PBKDF2/bcrypt/Argon2).

אימות באמצעות Session בסיסי → מומלץ לשדרג ל-OWIN Cookie Auth / ASP.NET Identity.

תלות ב-CDN (Bootstrap/DataTables/TinyMCE) – בסביבה ללא אינטרנט יש להחליף לקבצים מקומיים.

Newtonsoft.Json 6.0.4 – מומלץ לעדכן לגרסה חדשה יותר.


-----------------------------------------------------------------------------------






