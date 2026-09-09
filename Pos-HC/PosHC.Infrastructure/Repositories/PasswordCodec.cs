using Microsoft.AspNetCore.Identity;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHC.Infrastructure.Repositories;

public class PasswordCodec : IPasswordCodec
{
    private readonly PasswordHasher<StaffUser> hasher = new();
    public string Hash(StaffUser user, string password) => hasher.HashPassword(user, password);
    public bool Verify(StaffUser user, string password) => hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
}
