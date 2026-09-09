using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces;

public interface IPasswordCodec
{
    string Hash(StaffUser user, string password);
    bool Verify(StaffUser user, string password);
}
