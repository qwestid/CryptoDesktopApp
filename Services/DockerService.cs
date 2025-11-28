using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoDesktopApp.Services;

public interface IDockerService
{
    Task<bool> IsDockerRunningAsync();
    Task<bool> StartCryptoContainerAsync();
    Task<bool> StopCryptoContainerAsync();
    Task<bool> IsContainerRunningAsync();
    Task<string> ExecuteCommandAsync(string command);
}

public class DockerService : IDockerService
{
    private readonly DockerClient _dockerClient;
    private const string ContainerName = "crypto-csp-container";
    private const string ComposeFilePath = "/opt/crypto-app/docker-compose.yml";

    public DockerService()
    {
        _dockerClient = new DockerClientConfiguration().CreateClient();
    }

    public async Task<bool> IsDockerRunningAsync()
    {
        try
        {
            await _dockerClient.System.PingAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> StartCryptoContainerAsync()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"compose -f {ComposeFilePath} up -d",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null) return false;

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting container: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> StopCryptoContainerAsync()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"compose -f {ComposeFilePath} down",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null) return false;

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping container: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> IsContainerRunningAsync()
    {
        try
        {
            var containers = await _dockerClient.Containers.ListContainersAsync(
                new ContainersListParameters
                {
                    All = true,
                    Filters = new Dictionary<string, IDictionary<string, bool>>
                    {
                        ["name"] = new Dictionary<string, bool> { [ContainerName] = true }
                    }
                });

            return containers.Any(c => c.State == "running");
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> ExecuteCommandAsync(string command)
    {
        try
        {
            // Обертываем команду в shell для поддержки перенаправления
            string shellCommand = $"sh -c \"{command}\"";
            
            var processInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"exec {ContainerName} {shellCommand}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null) return string.Empty;

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();
            
            return string.IsNullOrEmpty(error) ? output : error;
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
