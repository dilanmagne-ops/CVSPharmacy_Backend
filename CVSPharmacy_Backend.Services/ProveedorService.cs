using CVSPharmacy_Backend.Core.Dtos;
using CVSPharmacy_Backend.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CVSPharmacy_Backend.Services
{
    public class ProveedorService : IProveedorService
    {
        public Task<ProveedorDto?> ChangeStatusAsync(int id, CambiarEstadoProveedorDto estadoDto)
        {
            throw new NotImplementedException();
        }

        public Task<ProveedorDto> CreateAsync(GuardarProveedorDto proveedorDto)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ProveedorDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProveedorDto?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ProveedorDto?> UpdateAsync(int id, GuardarProveedorDto proveedorDto)
        {
            throw new NotImplementedException();
        }
    }
}
