using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

﻿namespace CryptoDesktopApp.ViewModels;

public partial class MainWindowViewModel : INotifyPropertyChanged
{
    public string Greeting { get; } = "Welcome to Avalonia!";
}
