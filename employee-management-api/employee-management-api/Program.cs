using EmployeeManagementAPI.Repositories;
using EmployeeManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);


// REGISTER ARHITECTURE LAYERS (DI)

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS TO ALLOW ANGULAR UI
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Matches Angular dev port
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// TURN CORS ON
app.UseCors("AllowAngularApp");

app.UseAuthorization();

app.MapControllers();

app.Run();