using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DummyStatelessService.Model;
using Microsoft.ServiceFabric.Services.Remoting;

namespace DummyStatelessService
{
    public interface IDummyMicroservice : IService
    {
        Task<DummyObject> CreateAsync(string correlationId, DummyObject entity);
        Task<DummyObject> UpdateAsync(string correlationId, DummyObject entity);
        Task<DummyObject> DeleteByIdAsync(string correlationId, string id);
        Task<DummyObject> GetOneByIdAsync(string correlationId, string id);
        Task<List<DummyObject>> GetListByQueryAsync(string correlationId);
    }
}
