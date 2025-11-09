using System;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace StudentsMcpServer.Models;

[McpServerToolType]
public static class StudentTools {
  private static StudentService? _studentService;
  private static ILogger? _logger;

  public static void Initialize(StudentService studentService, ILogger logger)
  {
    _studentService = studentService;
    _logger = logger;
    _logger.LogInformation("StudentTools initialized");
  }

  [McpServerTool, Description("Get a list of students and return as JSON array")]
  public static string GetStudentsJson() {
    _logger?.LogInformation("GetStudentsJson tool called");
    if (_studentService == null) {
      _logger?.LogError("StudentService is not initialized");
      return "{\"error\": \"Service not initialized\"}";
    }
    var task = _studentService.GetStudentsJson();
    return task.GetAwaiter().GetResult();
  }

  [McpServerTool, Description("Get a student by name and return as JSON")]
  public static string GetStudentJson([Description("The name of the student to get details for")] string name) {
    _logger?.LogInformation("GetStudentJson tool called with name: {Name}", name);
    if (_studentService == null) {
      _logger?.LogError("StudentService is not initialized");
      return "{\"error\": \"Service not initialized\"}";
    }
    var task = _studentService.GetStudentByFullName(name);
    var student = task.GetAwaiter().GetResult();
    if (student == null) {
      _logger?.LogWarning("Student not found: {Name}", name);
      return "Student not found";
    }

    return System.Text.Json.JsonSerializer.Serialize(student, StudentContext.Default.Student);
  }

  [McpServerTool, Description("Get a student by ID and return as JSON")]
  public static string GetStudentByIdJson([Description("The ID of the student to get details for")] int id) {
    _logger?.LogInformation("GetStudentByIdJson tool called with ID: {Id}", id);
    if (_studentService == null) {
      _logger?.LogError("StudentService is not initialized");
      return "{\"error\": \"Service not initialized\"}";
    }
    var task = _studentService.GetStudentById(id);
    var student = task.GetAwaiter().GetResult();
    if (student == null) {
      _logger?.LogWarning("Student not found with ID: {Id}", id);
      return "Student not found";
    }

    return System.Text.Json.JsonSerializer.Serialize(student, StudentContext.Default.Student);
  }

  [McpServerTool, Description("Get students by school and return as JSON")]
  public static string GetStudentsBySchoolJson([Description("The name of the school to filter students by")] string school) {
    _logger?.LogInformation("GetStudentsBySchoolJson tool called with school: {School}", school);
    if (_studentService == null) {
      _logger?.LogError("StudentService is not initialized");
      return "{\"error\": \"Service not initialized\"}";
    }
    var task = _studentService.GetStudentsBySchoolJson(school);
    var students = task.GetAwaiter().GetResult();
    _logger?.LogDebug("Returning {Count} students for school: {School}", students.Count, school);
    return System.Text.Json.JsonSerializer.Serialize(students, StudentContext.Default.ListStudent);
  }

  [McpServerTool, Description("Get students by last name and return as JSON")]
  public static string GetStudentsByLastNameJson([Description("The last name of the student to filter by")] string lastName) {
    _logger?.LogInformation("GetStudentsByLastNameJson tool called with last name: {LastName}", lastName);
    if (_studentService == null) {
      _logger?.LogError("StudentService is not initialized");
      return "{\"error\": \"Service not initialized\"}";
    }
    var task = _studentService.GetStudentsByLastName(lastName);
    var students = task.GetAwaiter().GetResult();
    _logger?.LogDebug("Returning {Count} students with last name: {LastName}", students.Count, lastName);
    return System.Text.Json.JsonSerializer.Serialize(students, StudentContext.Default.ListStudent);
  }

  [McpServerTool, Description("Get students by first name and return as JSON")]
  public static string GetStudentsByFirstNameJson([Description("The first name of the student to filter by")] string firstName) {
    _logger?.LogInformation("GetStudentsByFirstNameJson tool called with first name: {FirstName}", firstName);
    if (_studentService == null) {
      _logger?.LogError("StudentService is not initialized");
      return "{\"error\": \"Service not initialized\"}";
    }
    var task = _studentService.GetStudentsByFirstName(firstName);
    var students = task.GetAwaiter().GetResult();
    _logger?.LogDebug("Returning {Count} students with first name: {FirstName}", students.Count, firstName);
    return System.Text.Json.JsonSerializer.Serialize(students, StudentContext.Default.ListStudent);
  }

  [McpServerTool, Description("Get count of total students")]
  public static int GetStudentCount() {
    _logger?.LogInformation("GetStudentCount tool called");
    if (_studentService == null) {
      _logger?.LogError("StudentService is not initialized");
      return 0;
    }
    var task = _studentService.GetStudents();
    var students = task.GetAwaiter().GetResult();
    _logger?.LogInformation("Total student count: {Count}", students.Count);
    return students.Count;
  }
}