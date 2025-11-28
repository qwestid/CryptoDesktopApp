using Avalonia.Controls;
using Avalonia.Interactivity;
using CryptoDesktopApp.ViewModels;
using System;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Linq;

namespace CryptoDesktopApp.Views;

public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        SetupEvents();
        
        this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        _viewModel = DataContext as MainViewModel;
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdateUI();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateUI();
    }

    private void SetupEvents()
    {
        StartButton.Click += OnStartButtonClick;
        StopButton.Click += OnStopButtonClick;
        StatusButton.Click += OnStatusButtonClick;
        RefreshButton.Click += OnRefreshButtonClick;
        SignButton.Click += OnSignButtonClick;
        DataInput.TextChanged += OnDataInputChanged;
        
    }

    private async void OnStartButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
            await _viewModel.StartContainerCommand.ExecuteAsync(null);
    }

    private async void OnStopButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
            await _viewModel.StopContainerCommand.ExecuteAsync(null);
    }

    private async void OnStatusButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
            await _viewModel.CheckDockerStatusCommand.ExecuteAsync(null);
    }

    private async void OnRefreshButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
            await _viewModel.RefreshContainersCommand.ExecuteAsync(null);
    }

    private async void OnSignButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            // Автоматически выбираем первый контейнер если не выбран
            if (_viewModel.SelectedContainer == null && _viewModel.Containers.Count > 0)
            {
                _viewModel.SelectedContainer = _viewModel.Containers[0];
            }
            await _viewModel.SignDataCommand.ExecuteAsync(null);
        }
    }

    private void OnDataInputChanged(object? sender, TextChangedEventArgs e)
    {
        if (_viewModel != null)
            _viewModel.DataToSign = DataInput.Text ?? "";
    }

    private void UpdateUI()
    {
        if (_viewModel == null) return;

        try
        {
            StatusText.Text = _viewModel.StatusMessage;
            ResultOutput.Text = _viewModel.SignatureResult;
            ProgressBar.IsIndeterminate = _viewModel.IsBusy;

            // Показываем контейнеры как текст
            if (_viewModel.Containers.Count > 0)
            {
                var containerNames = string.Join(", ", _viewModel.Containers.Select(c => c.Name));
                ContainersText.Text = $"Containers: {containerNames}";
                
                // Автовыбор первого контейнера
                if (_viewModel.SelectedContainer == null)
                {
                    _viewModel.SelectedContainer = _viewModel.Containers[0];
                }
            }
            else
            {
                ContainersText.Text = "Containers: None";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UI Update error: {ex.Message}");
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
        base.OnClosed(e);
    }
}
