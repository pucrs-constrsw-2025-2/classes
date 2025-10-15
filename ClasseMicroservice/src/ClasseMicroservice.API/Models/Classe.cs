public class Class
{
    public string Id { get; set; }
    public string ClassNumber { get; set; }
    public int Year { get; set; }
    public int Semester { get; set; }
    public string Schedule { get; set; }
    public List<Exam> Exams { get; set; }
    public List<Student> Students { get; set; }
    public List<Professor> Professors { get; set; }
    public Course Course { get; set; }
}