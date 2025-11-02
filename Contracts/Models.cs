namespace Contracts;

public interface IPersonRepository
{
    Task<List<Person>> GetAllAsync();
    Task<Person?> GetByIdAsync(int id);
    Task AddAsync(Person person);
    Task UpdateAsync(Person person);
    Task<bool> DeleteAsync(int id);
}

public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public record PersonCreateRequest(string FirstName, string LastName, string? Email);
public record PersonUpdateRequest(string? FirstName, string? LastName, string? Email);
