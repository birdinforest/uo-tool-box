using System;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CUO_API;
using DefaultNamespace;

unsafe
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();


    app.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action=Index}/{id?}");

    app.MapFallbackToFile("index.html");;

    // var PluginPath = "/Users/forrrest/projects/ClassicUO/bin/Debug/Data/Plugins/ClassLibrary1.exe";
    // Assembly asm = Assembly.LoadFile(PluginPath);
    // Type type = asm.GetType("Assistant.Engine");
    // MethodInfo meth = type?.GetMethod("HookWebAPI", BindingFlags.Public | BindingFlags.Static);
    //
    // var dataTransfer = new DataTransfer();
    //
    // PluginHeader header = new PluginHeader
    // {
    //     OnRecv = Marshal.GetFunctionPointerForDelegate<OnPacketSendRecv>(dataTransfer.OnRecv),
    // };
    // void* func = &header;
    // meth?.Invoke(null, new object[] { (IntPtr)func });

    app.Run();
}