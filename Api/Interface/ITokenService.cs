using DA.Model;

namespace Api.Interface
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}
