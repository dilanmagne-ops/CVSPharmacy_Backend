using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IUsuarioRepository : IBaseRepository<Usuario>
{
    Task<Usuario?> GetPorEmailConRolAsync(string email);
    Task<Usuario?> GetPorIdConRolAsync(int id);
}
