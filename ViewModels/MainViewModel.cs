using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoDesktopApp.Models;
using CryptoDesktopApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CryptoDesktopApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ICryptoService _cryptoService;
    private readonly IDockerService _dockerService;

    public MainViewModel(ICryptoService cryptoService, IDockerService dockerService)
    {
        _cryptoService = cryptoService;
        _dockerService = dockerService;
        
        // Упрощенные команды - убираем условия для тестирования
        StartContainerCommand = new AsyncRelayCommand(StartContainerAsync);
        StopContainerCommand = new AsyncRelayCommand(StopContainerAsync);
        RefreshContainersCommand = new AsyncRelayCommand(RefreshContainersAsync);
        RefreshCertificatesCommand = new AsyncRelayCommand(RefreshCertificatesAsync);
        SignDataCommand = new AsyncRelayCommand(SignDataAsync);
        CheckDockerStatusCommand = new AsyncRelayCommand(CheckDockerStatusAsync);
        
        _ = InitializeAsync();
    }

    [ObservableProperty]
    private string _statusMessage = "Инициализация...";

    [ObservableProperty]
    private ObservableCollection<CryptoContainer> _containers = new();

    [ObservableProperty]
    private ObservableCollection<CertificateInfo> _certificates = new();

    [ObservableProperty]
    private CryptoContainer? _selectedContainer;

    [ObservableProperty]
    private string _dataToSign = string.Empty;

    [ObservableProperty]
    private string _signatureResult = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isDockerRunning;

    [ObservableProperty]
    private bool _isContainerRunning;

    [ObservableProperty]
    private string _operationMode = "Тестовый режим";

    [ObservableProperty]
    private string _debugInfo = "";

    // Команды - без условий
    public IAsyncRelayCommand StartContainerCommand { get; }
    public IAsyncRelayCommand StopContainerCommand { get; }
    public IAsyncRelayCommand RefreshContainersCommand { get; }
    public IAsyncRelayCommand RefreshCertificatesCommand { get; }
    public IAsyncRelayCommand SignDataCommand { get; }
    public IAsyncRelayCommand CheckDockerStatusCommand { get; }

    private async Task InitializeAsync()
    {
        Debug.WriteLine("InitializeAsync started");
        try
        {
            await CheckDockerStatusAsync();
            await RefreshContainersAsync();
            await RefreshCertificatesAsync();
            UpdateDebugInfo();
            Debug.WriteLine("InitializeAsync completed");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"InitializeAsync error: {ex.Message}");
            StatusMessage = $"Ошибка инициализации: {ex.Message}";
        }
    }

    private async Task CheckDockerStatusAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Проверка статуса Docker...";
            
            IsDockerRunning = await _dockerService.IsDockerRunningAsync();
            IsContainerRunning = await _dockerService.IsContainerRunningAsync();
            
            OperationMode = IsContainerRunning ? "Режим реальных CSP" : 
                            IsDockerRunning ? "Docker запущен" : "Тестовый режим";
            
            StatusMessage = IsContainerRunning 
                ? "✅ Контейнер с CSP запущен (режим реальных CSP)" 
                : IsDockerRunning 
                    ? "ℹ️ Docker запущен, но контейнер не активен" 
                    : "📝 Тестовый режим - работа с демо-данными";

            UpdateDebugInfo();
            Debug.WriteLine($"Docker: {IsDockerRunning}, Container: {IsContainerRunning}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CheckDockerStatus error: {ex.Message}");
            StatusMessage = $"❌ Ошибка проверки Docker: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task StartContainerAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Запуск контейнера...";
            
            var success = await _dockerService.StartCryptoContainerAsync();
            
            if (success)
            {
                await Task.Delay(3000);
                await CheckDockerStatusAsync();
                await RefreshContainersAsync();
                StatusMessage = "✅ Контейнер успешно запущен";
            }
            else
            {
                StatusMessage = "❌ Ошибка запуска контейнера";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"StartContainer error: {ex.Message}");
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task StopContainerAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Остановка контейнера...";
            
            var success = await _dockerService.StopCryptoContainerAsync();
            
            if (success)
            {
                await CheckDockerStatusAsync();
                await RefreshContainersAsync();
                await RefreshCertificatesAsync();
                StatusMessage = "✅ Контейнер остановлен";
            }
            else
            {
                StatusMessage = "❌ Ошибка остановки контейнера";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"StopContainer error: {ex.Message}");
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshContainersAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Загрузка контейнеров...";
            
            var containers = await _cryptoService.GetContainersAsync();
            Containers.Clear();
            foreach (var container in containers)
            {
                Containers.Add(container);
            }
            
            StatusMessage = IsContainerRunning 
                ? $"✅ Загружено {Containers.Count} реальных контейнеров" 
                : $"📝 Загружено {Containers.Count} тестовых контейнеров";
                
            UpdateDebugInfo();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"RefreshContainers error: {ex.Message}");
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshCertificatesAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Загрузка сертификатов...";
            
            var certificates = await _cryptoService.GetCertificatesAsync();
            Certificates.Clear();
            foreach (var cert in certificates)
            {
                Certificates.Add(cert);
            }
            
            StatusMessage = IsContainerRunning 
                ? $"✅ Загружено {Certificates.Count} реальных сертификатов" 
                : $"📝 Загружено {Certificates.Count} тестовых сертификатов";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"RefreshCertificates error: {ex.Message}");
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SignDataAsync()
    {
        Debug.WriteLine("SignDataAsync called");
        
        if (SelectedContainer == null)
        {
            StatusMessage = "⚠️ Выберите контейнер для подписи";
            Debug.WriteLine("No container selected");
            return;
        }

        if (string.IsNullOrWhiteSpace(DataToSign))
        {
            StatusMessage = "⚠️ Введите данные для подписи";
            Debug.WriteLine("No data to sign");
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Подписание данных...";
            Debug.WriteLine($"Signing data: '{DataToSign}' with container: {SelectedContainer.Name}");
            
            string result;
            if (IsContainerRunning)
            {
                // Режим реального подписания
                result = await _cryptoService.SignDataAsync(DataToSign, SelectedContainer.Name);
                StatusMessage = "✅ Данные успешно подписаны (режим реальных CSP)";
            }
            else
            {
                // Тестовый режим
                await Task.Delay(1000);
                result = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"DEMO_SIGNED[{SelectedContainer.Name}]:{DataToSign}"));
                StatusMessage = "📝 Данные подписаны в тестовом режиме";
            }
            
            SignatureResult = result;
            Debug.WriteLine($"Signature result: {result}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SignData error: {ex.Message}");
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateDebugInfo()
    {
        DebugInfo = $"Container: {SelectedContainer?.Name ?? "None"}, " +
                   $"Data: {(string.IsNullOrEmpty(DataToSign) ? "Empty" : "Has Data")}, " +
                   $"Busy: {IsBusy}, " +
                   $"Docker: {IsDockerRunning}, " +
                   $"ContainerRunning: {IsContainerRunning}";
    }

    // Обработчики изменений свойств
    partial void OnSelectedContainerChanged(CryptoContainer? value)
    {
        Debug.WriteLine($"SelectedContainer changed: {value?.Name}");
        UpdateDebugInfo();
    }

    partial void OnDataToSignChanged(string value)
    {
        Debug.WriteLine($"DataToSign changed: '{value}'");
        UpdateDebugInfo();
    }

    partial void OnIsBusyChanged(bool value)
    {
        Debug.WriteLine($"IsBusy changed: {value}");
        UpdateDebugInfo();
    }
}
