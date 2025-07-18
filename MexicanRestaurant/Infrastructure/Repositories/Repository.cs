using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MexicanRestaurant.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected ApplicationDbContext _context { get; set; }
        private DbSet<T> _dbSet { get; set; }
        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null");

            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
               throw new KeyNotFoundException($"Entity with id {id} not found");

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(QueryOptions<T> options)
        {
            var query = ApplyOptions(options);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllByIdAsync<TKey>(TKey id, string propertyName, QueryOptions<T> options)
        {
            var query = ApplyOptions(options);

            //Filter by the specified property Name and Id
            query = query.Where(e => EF.Property<TKey>(e, propertyName).Equals(id));
            return await query.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id, QueryOptions<T> options)
        {
            options.DisablePaging = true;
            var query = ApplyOptions(options);

            var key = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.FirstOrDefault();
            string primaryKeyName = key?.Name;
            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, primaryKeyName) == id);
        }

        public async Task UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null");

            _context.Update(entity);
            await _context.SaveChangesAsync();
        }

        public IQueryable<T> Table => _context.Set<T>();

        private IQueryable<T> ApplyOptions(QueryOptions<T> options)
        {
            IQueryable<T> query = _dbSet;

            if (options.HasWhere)
                query = query.Where(options.Where);

            if (options.HasOrderByWithFunc)
                query = options.OrderByWithFunc(query);

            if (options.HasPaging && !options.DisablePaging)
                query = query.Skip((options.PageNumber - 1) * options.PageSize)
                             .Take(options.PageSize);

            foreach (var include in options.GetIncludes())
                query = query.Include(include);

            return query;
        }
    }
}
