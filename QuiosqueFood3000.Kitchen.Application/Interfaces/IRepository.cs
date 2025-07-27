
using QuiosqueFood3000.Kitchen.Domain.Entities;

namespace QuiosqueFood3000.Kitchen.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T> Add(T entity);
    Task<T> Update(T entity);
    Task<T?> GetById(Guid id);
    Task<IEnumerable<T>> GetAll();
}
