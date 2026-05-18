# Giai thich `fields` va `expand`

Tai lieu nay giai thich 2 chuc nang trong LAB1:

- `fields`: cho client chon nhung field muon nhan trong list API.
- `expand`: cho client yeu cau API tra kem du lieu lien quan.

## 1. `fields` dung de lam gi?

`fields` dung de giam du lieu tra ve trong response. Client co the chi dinh danh sach field can lay thay vi lay toan bo object.

Vi du:

```http
GET /api/students?fields=studentId,fullName,email
```

Response se chi lay cac field duoc yeu cau trong moi item:

```json
{
  "success": true,
  "message": "Get student list successfully",
  "data": {
    "items": [
      {
        "studentId": 1,
        "fullName": "Nguyen Van A",
        "email": "a@example.com"
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 10,
      "totalItems": 50,
      "totalPages": 5
    }
  },
  "errors": null
}
```

## 2. `FieldSelectionHelper` dung de lam gi?

`FieldSelectionHelper` nam trong:

```text
PRN232.LMS.API/Helpers/FieldSelectionHelper.cs
```

Helper nay nhan vao:

- Danh sach response model, vi du `List<StudentResponse>`.
- Chuoi `fields` tu query string, vi du `"studentId,fullName,email"`.

Sau do helper dung reflection de doc cac property public cua response model va tao ra object moi chi gom cac field duoc yeu cau.

Neu client khong truyen `fields`, helper tra ve day du object ban dau.

## 3. Luong xu ly cua `fields`

Vi du request:

```http
GET /api/enrollments?fields=enrollmentId,status,enrollDate
```

Luong xu ly:

1. Client gui request len `EnrollmentController`.
2. Controller nhan query parameter:

```csharp
[FromQuery] string? fields
```

3. Controller goi service de lay du lieu enrollment nhu binh thuong.
4. Service tra ve `EnrollmentModel`.
5. Controller map `EnrollmentModel` sang `EnrollmentResponse`.
6. Controller goi:

```csharp
FieldSelectionHelper.SelectFields(enrollmentResponses, fields)
```

7. Helper tach chuoi `fields` thanh danh sach field:

```text
enrollmentId
status
enrollDate
```

8. Helper chi giu lai cac property tuong ung trong response.
9. Controller tra ve `BaseResponse<PagedResult<object>>`.

Ket qua: client chi nhan cac field can dung.

## 4. `expand` dung de lam gi?

`expand` dung de yeu cau API tra kem du lieu lien quan cua resource.

Neu khong co `expand`, list API chi tra ve du lieu chinh.

Vi du khong expand:

```http
GET /api/enrollments
```

Moi enrollment chi co cac field co ban:

```json
{
  "enrollmentId": 1,
  "studentId": 1,
  "courseId": 1,
  "enrollDate": "2026-05-18T00:00:00",
  "status": "Active"
}
```

Neu co expand:

```http
GET /api/enrollments?expand=student,course
```

Moi enrollment co them object `student` va `course`:

```json
{
  "enrollmentId": 1,
  "studentId": 1,
  "courseId": 1,
  "enrollDate": "2026-05-18T00:00:00",
  "status": "Active",
  "student": {
    "studentId": 1,
    "fullName": "Nguyen Van A",
    "email": "a@example.com",
    "dateOfBirth": "2004-01-01T00:00:00",
    "enrollments": []
  },
  "course": {
    "courseId": 1,
    "courseName": "PRN232",
    "semesterId": 1,
    "semester": {
      "semesterId": 1,
      "semesterName": "Spring 2026",
      "startDate": "2026-01-01T00:00:00",
      "endDate": "2026-05-31T00:00:00"
    }
  }
}
```

## 5. Luong xu ly cua `expand`

Vi du request:

```http
GET /api/enrollments?expand=student,course
```

Luong xu ly:

1. Client gui request len `EnrollmentController`.
2. Controller nhan query parameter:

```csharp
[FromQuery] string? expand
```

3. Controller truyen `expand` xuong service:

```csharp
_enrollmentService.GetAllAsync(search, sort, expand, page, pageSize)
```

4. Trong `EnrollmentService`, chuoi `expand` duoc tach thanh danh sach:

```text
student
course
```

5. Service kiem tra client co yeu cau expand gi:

```csharp
var includeStudent = includes.Contains("student");
var includeCourse = includes.Contains("course");
```

6. Neu co `student`, service them:

```csharp
query = query.Include(e => e.Student);
```

7. Neu co `course`, service them:

```csharp
query = query
    .Include(e => e.Course)
    .ThenInclude(c => c.Semester);
```

8. Entity Framework Core se lay them du lieu lien quan tu database.
9. Service map entity sang business model:

```text
Enrollment -> EnrollmentModel
Student -> StudentModel
Course -> CourseModel
Semester -> SemesterModel
```

10. Controller map business model sang response model:

```text
EnrollmentModel -> EnrollmentResponse
StudentModel -> StudentResponse
CourseModel -> CourseResponse
SemesterModel -> SemesterResponse
```

11. API tra ve response co du lieu lien quan.

## 6. `expand` trong cac API hien tai

### Students

```http
GET /api/students?expand=enrollments
```

Tra them danh sach enrollment cua moi student.

### Courses

```http
GET /api/courses?expand=semester
```

Tra them thong tin semester cua moi course.

### Enrollments

```http
GET /api/enrollments?expand=student,course
```

Tra them thong tin student va course cua moi enrollment.

## 7. Ket hop `fields` va `expand`

Co the dung ca hai cung luc.

Vi du:

```http
GET /api/enrollments?expand=student,course&fields=enrollmentId,status,student,course
```

Y nghia:

- `expand=student,course`: yeu cau load them du lieu student va course.
- `fields=enrollmentId,status,student,course`: response chi tra ve cac field nay.

Neu khong them `student,course` vao `fields`, du lieu expand co the da duoc load o service nhung se khong xuat hien trong response cuoi cung.

## 8. Vi sao khong xu ly `fields` trong Repository?

Repository chi nen lam nhiem vu data access. Theo yeu cau LAB1:

- Repository khong chua business logic.
- Repository khong dung Request/Response Model.

`fields` lien quan truc tiep den API output, nen duoc xu ly o API Layer bang `FieldSelectionHelper`.

## 9. Vi sao `expand` duoc xu ly trong Service?

`expand` anh huong den viec load du lieu lien quan tu database bang Entity Framework Core `Include`.

Controller chi nhan query va truyen xuong.
Service quyet dinh can lay them related data nao.
Repository chi cung cap `Query()` de service co the build query phu hop.

Luong nay giu dung 3-layer architecture:

```text
Controller -> Service -> Repository -> Database
```

