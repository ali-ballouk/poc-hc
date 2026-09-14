namespace PosHC.Application.Interfaces
{
    public interface ICurrentStaff
    {
        Guid Id { get; }
        string Name { get; }
        string Role { get; }
    }
}
