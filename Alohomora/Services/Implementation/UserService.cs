using Alohomora.Core.Services.Contact;

namespace Alohomora.Core.Services.Implementation;

public class UserService : IUserService
{

    #region Fields and Ctor

    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    #endregion


    public Guid CurrentUserId => Guid.Empty;
}