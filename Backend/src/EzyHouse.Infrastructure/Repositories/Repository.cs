using EzyHouse.Domain.Interfaces;
using EzyHouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Infrastructure.Repositories;

public sealed class Repository<T>(EzyHouseDbContext db) : IRepository<T> where T : class
{
    private readonly DbSet<T> set = db.Set<T>();

    public IQueryable<T> Query() => set;

    public async Task<IEnumerable<T>> GetAllAsync() => await set.ToListAsync();

    public async Task<T?> GetByIdAsync(int id) => await set.FindAsync(id);

    public async Task AddAsync(T entity) => await set.AddAsync(entity);

    public void Update(T entity) => set.Update(entity);

    public void Delete(T entity) => set.Remove(entity);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
