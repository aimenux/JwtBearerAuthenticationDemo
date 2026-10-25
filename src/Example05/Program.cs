using Example05;
using Example05.Infrastructure;
using Example05.Presentation;

var app = WebApplication.CreateBuilder(args, enforceHttps: true)
    .AddPresentation()
    .AddInfrastructure()
    .Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHsts();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();