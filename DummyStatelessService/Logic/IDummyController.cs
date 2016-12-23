using System.Collections.Generic;
using System.Threading.Tasks;
using DummyStatelessService.Model;
using PipServices.Commons.Data;

namespace DummyStatelessService.Logic
{
    public interface IDummyController
    {
        Task<DummyObject> CreateAsync(string correlationId, DummyObject entity);
        Task<DummyObject> UpdateAsync(string correlationId, DummyObject entity);
        Task<DummyObject> DeleteByIdAsync(string correlationId, string id);
        Task<DummyObject> GetOneByIdAsync(string correlationId, string id);
        Task<List<DummyObject>> GetListByQueryAsync(string correlationId);
    }
}
