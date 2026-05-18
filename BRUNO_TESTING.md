# Bruno API testing

This project includes a Bruno collection for testing the LMS API.

Collection path:

```text
bruno/PRN232_LMS_API
```

## Open in Bruno

1. Open Bruno.
2. Choose `Open Collection`.
3. Select this folder:

```text
bruno/PRN232_LMS_API
```

4. Select the `Local` environment.
5. Make sure the API is running at:

```text
http://localhost:8080
```

## Run API with Docker

```powershell
docker compose up -d --build api
docker compose --profile seed run --rm db-seed
```

Swagger:

```text
http://localhost:8080/swagger
```

## Environment variables

The Bruno environment is stored at:

```text
bruno/PRN232_LMS_API/environments/Local.bru
```

Current variables:

```text
baseUrl=http://localhost:8080
studentId=1
courseId=1
enrollmentId=1
subjectId=1
semesterId=1
```

Change these IDs if your seeded data uses different records.

## Notes

- Most requests are safe `GET` requests.
- `Create Student`, `Update Student`, `Delete Student`, and `Create Enrollment` modify data.
- Be careful with `Delete Student` because deleting a student may also affect related enrollments depending on database constraints.

## Optional CLI run

If you install Bruno CLI, you can run the collection from terminal:

```powershell
bru run bruno/PRN232_LMS_API --env Local
```

If the CLI cannot find the environment by name, pass the environment file directly:

```powershell
bru run bruno/PRN232_LMS_API --env-file bruno/PRN232_LMS_API/environments/Local.bru
```
