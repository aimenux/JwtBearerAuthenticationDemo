using Microsoft.AspNetCore.Mvc;

namespace Example05.Presentation.Authentication;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class JwtSecurityServiceFilterAttribute : ServiceFilterAttribute
{
    public JwtSecurityServiceFilterAttribute() : base(typeof(JwtSecurityFilter))
    {
    }
}
