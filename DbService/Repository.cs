using Contracts;
using Microsoft.EntityFrameworkCore;

namespace DbService;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Person> People => Set<Person>();
}

public class PersonRepository : IPersonRepository
{
    private readonly AppDbContext _db;
    public PersonRepository(AppDbContext db) => _db = db;

    public async Task<List<Person>> GetAllAsync() => await _db.People.AsNoTracking().ToListAsync();

    public async Task<Person?> GetByIdAsync(int id) => await _db.People.FindAsync(id);

    public async Task AddAsync(Person person)
    {
        _db.People.Add(person);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Person person)
    {
        _db.People.Update(person);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _db.People.FindAsync(id);
        if (existing is null) return false;
        _db.People.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
}
