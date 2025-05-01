using Proton.Lexer.Interfaces;
using Proton.Lexer.Services;
using Proton.Lexer;
using Proton.Parser;
using Proton.Parser.Service;
using Proton.Parser.Interfaces;
using ProtonComplierUI.Hubs;
using Proton.Semantic.Interfaces;
using Proton.Semantic;
using Proton.Semantic.Services;
using Proton.CodeGenerator;
using System.CodeDom.Compiler;
using Proton.CodeGenerator.Interfaces;
using Proton.CodeGenerator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Register lexical services
builder.Services.AddScoped<ITokenizer, Tokenizer>();
builder.Services.AddScoped<ILexicalAnalyzer, LexicalAnalyzer>();
builder.Services.AddScoped<LexicalService>();

// Register parser services
builder.Services.AddScoped<ISyntaxAnalyzer, SyntaxAnalyzer>();
builder.Services.AddScoped<ParserService>();

// Register semantic services
builder.Services.AddScoped<ISemanticAnalyzer, SemanticAnalyzer>();
builder.Services.AddScoped<SemanticService>();

// Register codegenerator services
builder.Services.AddScoped<IGenerateCode, GenerateCode>();
builder.Services.AddScoped<ICodeExecutor, CodeExecutor>();
builder.Services.AddScoped<CodeGeneratorService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapHub<CompilerHub>("/compilerhub"); // SignalR endpoint

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
