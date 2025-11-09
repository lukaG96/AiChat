using System;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace StudentsMcpServer.Models;

public class StudentService {
  private readonly HttpClient _httpClient = new();
  private readonly ILogger<StudentService> _logger;
  private List<Student>? _studentsCache = null;
  private DateTime _cacheTime;
  private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10); // adjust as needed

  public StudentService(ILogger<StudentService> logger)
  {
    _logger = logger;
  }

  private async Task<List<Student>> FetchStudentsFromApi() {
    try {
      _logger.LogInformation("Fetching students from API: https://apipool.azurewebsites.net/api/students");
      var response = await _httpClient.GetAsync("https://apipool.azurewebsites.net/api/students");
      if (response.IsSuccessStatusCode) {
        var studentsFromApi = await response.Content.ReadFromJsonAsync<List<Student>>(StudentContext.Default.ListStudent);
        var count = studentsFromApi?.Count ?? 0;
        _logger.LogInformation("Successfully fetched {Count} students from API", count);
        return studentsFromApi ?? [];
        }
      else {
        _logger.LogWarning("Failed to fetch students from API. Status code: {StatusCode}", response.StatusCode);
      }
    } catch (Exception ex) {
      _logger.LogError(ex, "Error fetching students from API: {Message}", ex.Message);
    }
    return [];
  }

  public async Task<List<Student>> GetStudents() {
    if (_studentsCache == null || DateTime.UtcNow - _cacheTime > _cacheDuration) {
      _logger.LogInformation("Cache expired or empty. Fetching fresh data from API.");
      _studentsCache = await FetchStudentsFromApi();
      _cacheTime = DateTime.UtcNow;
      _logger.LogInformation("Cache updated with {Count} students. Cache will expire in {Minutes} minutes.", 
        _studentsCache.Count, _cacheDuration.TotalMinutes);
    }
    else {
      var cacheAge = DateTime.UtcNow - _cacheTime;
      _logger.LogDebug("Using cached data. Cache age: {Age} minutes. {Count} students available.", 
        cacheAge.TotalMinutes, _studentsCache.Count);
    }
    return _studentsCache ?? [];
  }

  public async Task<Student?> GetStudentByFullName(string name) {
    _logger.LogInformation("Searching for student by full name: {Name}", name);
    var students = await GetStudents();

    var nameParts = name.Split(' ', 2);
    if (nameParts.Length != 2) {
      _logger.LogWarning("Invalid name format. Expected 'FirstName LastName', got: {Name}", name);
      return null;
    }

    var firstName = nameParts[0].Trim();
    var lastName = nameParts[1].Trim();

    foreach (var s in students.Where(s => s.FirstName?.Contains(firstName, StringComparison.OrdinalIgnoreCase) == true)) {
      _logger.LogDebug("Found partial first name match: '{FirstName}' '{LastName}'", s.FirstName, s.LastName);
    }

    var student = students.FirstOrDefault(m => {
      var firstNameMatch = string.Equals(m.FirstName, firstName, StringComparison.OrdinalIgnoreCase);
      var lastNameMatch = string.Equals(m.LastName, lastName, StringComparison.OrdinalIgnoreCase);
      return firstNameMatch && lastNameMatch;
    });

    if (student != null) {
      _logger.LogInformation("Found student: {Student}", student);
    }
    else {
      _logger.LogWarning("Student not found with name: {Name}", name);
    }

    return student;
  }

  public async Task<Student?> GetStudentById(int id) {
    _logger.LogInformation("Searching for student by ID: {Id}", id);
    var students = await GetStudents();
    var student = students.FirstOrDefault(s => s.StudentId == id);

    if (student != null) {
      _logger.LogInformation("Found student: {Student}", student);
    }
    else {
      _logger.LogWarning("No student found with ID: {Id}", id);
    }
    return student;
  }

  public async Task<List<Student>> GetStudentsBySchoolJson(string school) {
    _logger.LogInformation("Searching for students by school: {School}", school);
    var students = await GetStudents();
    var filteredStudents = students.Where(s => s.School?.Equals(school, StringComparison.OrdinalIgnoreCase) == true).ToList();

    if (filteredStudents.Count == 0) {
      _logger.LogWarning("No students found for school: {School}", school);
    }
    else {
      _logger.LogInformation("Found {Count} students for school: {School}", filteredStudents.Count, school);
    }

    return filteredStudents;
  }

  public async Task<List<Student>> GetStudentsByLastName(string lastName) {
    _logger.LogInformation("Searching for students by last name: {LastName}", lastName);
    var students = await GetStudents();
    var filteredStudents = students.Where(s => s.LastName?.Equals(lastName, StringComparison.OrdinalIgnoreCase) == true).ToList();

    if (filteredStudents.Count == 0) {
      _logger.LogWarning("No students found with last name: {LastName}", lastName);
    }
    else {
      _logger.LogInformation("Found {Count} students with last name: {LastName}", filteredStudents.Count, lastName);
    }

    return filteredStudents;
  }

  public async Task<List<Student>> GetStudentsByFirstName(string firstName) {
    _logger.LogInformation("Searching for students by first name: {FirstName}", firstName);
    var students = await GetStudents();
    var filteredStudents = students.Where(s => s.FirstName?.Equals(firstName, StringComparison.OrdinalIgnoreCase) == true).ToList();

    if (filteredStudents.Count == 0) {
      _logger.LogWarning("No students found with first name: {FirstName}", firstName);
    }
    else {
      _logger.LogInformation("Found {Count} students with first name: {FirstName}", filteredStudents.Count, firstName);
    }

    return filteredStudents;
  }

  public async Task<string> GetStudentsJson() {
    _logger.LogInformation("Getting all students as JSON");
    var students = await GetStudents();
    _logger.LogDebug("Serializing {Count} students to JSON", students.Count);
    return System.Text.Json.JsonSerializer.Serialize(students);
  }
}
