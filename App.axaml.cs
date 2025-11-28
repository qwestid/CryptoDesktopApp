using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CryptoDesktopApp.ViewModels;
using CryptoDesktopApp.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CryptoDesktopApp.Services;

namespace CryptoDesktopApp;

public partial class App : Application
{
    private IHost? _host;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Создаем и конфигурируем хост с зависимостями
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // Регистрируем сервисы
                services.AddSingleton<IDockerService, DockerService>();
                services.AddSingleton<ICryptoService, CryptoService>();
                services.AddSingleton<MainViewModel>();
            })
            .Build();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Получаем MainViewModel из контейнера зависимостей
            var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}