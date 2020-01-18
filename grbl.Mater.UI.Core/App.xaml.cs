using System.Windows;

namespace grbl.Mater.UI.Core
{
    using System;

    using grbl.Master.BL.Implementation;
    using grbl.Master.BL.Interface;
    using grbl.Master.Service.Implementation;
    using grbl.Master.Service.Interface;
    using grbl.Master.UI.ViewModels;
    using grbl.Mater.UI.Core.Services;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost host;

        public App()
        {
            host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                    {
                        // Add other configuration files...
                        builder.AddJsonFile("appsettings.json", optional: true);
                    }).ConfigureServices((context, services) =>
                    {
                        ConfigureServices(context.Configuration, services);
                    })
                .ConfigureLogging(logging =>
                    {
                        // Add other loggers...
                    })
                .Build();
        }
        private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));

            services.AddScoped<ISampleService, SampleService>();
            
            services.AddSingleton(typeof(IGrblDispatcher),  typeof(GrblDispatcher));
            services.AddSingleton(typeof(IComService),  typeof(COMService));
            services.AddSingleton(typeof(IGrblResponseTypeFinder),  typeof(GrblResponseTypeFinder));
            services.AddSingleton(typeof(IGrblCommandPreProcessor),  typeof(GrblCommandPreProcessor));
            services.AddSingleton(typeof(ICommandSender),  typeof(CommandSender));
            services.AddSingleton(typeof(IGrblPrompt),  typeof(GrblPrompt));
            services.AddSingleton(typeof(IGrblStatus),  typeof(GrblStatus));

            // ...
            services.AddSingleton<MainWindow>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await host.StartAsync();

            var mainWindow = host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (host)
            {
                await host.StopAsync(TimeSpan.FromSeconds(5));
            }

            base.OnExit(e);
        }
    }
}
