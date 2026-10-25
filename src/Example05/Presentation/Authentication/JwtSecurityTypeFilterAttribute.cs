using Microsoft.AspNetCore.Mvc;

namespace Example05.Presentation.Authentication;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class JwtSecurityTypeFilterAttribute : TypeFilterAttribute
{
    public JwtSecurityTypeFilterAttribute() : base(typeof(JwtSecurityFilter))
    {
    }
}
