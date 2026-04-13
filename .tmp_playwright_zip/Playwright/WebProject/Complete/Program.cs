var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddXmlSerializerFormatters()
    .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.WriteIndented = true;
        });
builder.Services.AddDbContext<RestoraniContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("RestoraniCS"));
});

builder.Services.AddCors(cors => cors
    .AddPolicy("CORS", options =>
    {
        options.AllowAnyHeader()
               .AllowAnyMethod()
               .AllowAnyOrigin();
    }
));
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CORS");

app.MapControllers();

app.UseHttpsRedirection();
app.Run();
