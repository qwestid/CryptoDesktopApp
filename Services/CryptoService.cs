using CryptoDesktopApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoDesktopApp.Services;

public interface ICryptoService
{
    Task<List<CryptoContainer>> GetContainersAsync();
    Task<List<CertificateInfo>> GetCertificatesAsync();
    Task<string> SignDataAsync(string data, string containerName);
}

public class CryptoService : ICryptoService
{
    private readonly IDockerService _dockerService;

    public CryptoService(IDockerService dockerService)
    {
        _dockerService = dockerService;
    }

    public async Task<List<CryptoContainer>> GetContainersAsync()
    {
        var isContainerRunning = await _dockerService.IsContainerRunningAsync();
        
        if (isContainerRunning)
        {
            // Попытка получить реальные контейнеры из КриптоПро
            try
            {
                var result = await _dockerService.ExecuteCommandAsync(
                    "/opt/cprocsp/bin/amd64/csptest -keyset -enum_cont -verifyc -fqcn"
                );
                
                return ParseCryptoProContainers(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting real containers: {ex.Message}");
                // Возвращаем демо-контейнеры в случае ошибки
                return GetDemoContainers();
            }
        }
        else
        {
            // Тестовый режим
            return GetDemoContainers();
        }
    }

    public async Task<List<CertificateInfo>> GetCertificatesAsync()
    {
        var isContainerRunning = await _dockerService.IsContainerRunningAsync();
        
        if (isContainerRunning)
        {
            // Попытка получить реальные сертификаты
            try
            {
                var result = await _dockerService.ExecuteCommandAsync(
                    "/opt/cprocsp/bin/amd64/certmgr -list"
                );
                
                return ParseCryptoProCertificates(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting real certificates: {ex.Message}");
                return GetDemoCertificates();
            }
        }
        else
        {
            return GetDemoCertificates();
        }
    }

    public async Task<string> SignDataAsync(string data, string containerName)
    {
        var isContainerRunning = await _dockerService.IsContainerRunningAsync();
        
        if (isContainerRunning)
        {
            // РЕАЛЬНОЕ подписание через КриптоПро
            try
            {
            	//await _dockerService.ExecuteCommandAsync(
            	//    "echo 1111 > /tmp/111.txt");
                // Сохраняем данные во временный файл
                var base64Data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));
                await _dockerService.ExecuteCommandAsync(
                    $"echo '{base64Data}' | base64 -d > /tmp/data.txt"
                );

                // Подписываем данные
                var signResult = await _dockerService.ExecuteCommandAsync(
                    //$"/opt/cprocsp/bin/amd64/cryptcp -sign -der -cont '{containerName}' /tmp/data.txt /tmp/signature.sig -pin 123456"
                    $"/opt/cprocsp/bin/amd64/cryptcp -sign -cont '{containerName}' /tmp/data.txt /tmp/signature.sig -pin 123456"
                );

                // Читаем подпись
                var signatureBase64 = await _dockerService.ExecuteCommandAsync(
                    "cat /tmp/signature.sig | base64"
                    //"cat /tmp/signature.sig | base64 -w 0"
                    //"base64 -w 0 /tmp/signature.sig"
                );

                // Очищаем временные файлы
                //await _dockerService.ExecuteCommandAsync("rm -f /tmp/data.txt /tmp/signature.sig");

                return signatureBase64.Trim();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка подписания через КриптоПро: {ex.Message}");
            }
        }
        else
        {
            // Тестовый режим
            await Task.Delay(500);
            return Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"DEMO_SIGNED[{containerName}]:{data}")
            );
        }
    }

    private List<CryptoContainer> GetDemoContainers()
    {
        return new List<CryptoContainer>
        {
            new CryptoContainer 
            { 
                Name = "Demo_CryptoPro_Container", 
                Provider = CryptoProvider.CryptoPro,
                IsAvailable = true,
                Description = "Демо-контейнер КриптоПро"
            }
        };
    }

    private List<CertificateInfo> GetDemoCertificates()
    {
        return new List<CertificateInfo>
        {
            new CertificateInfo 
            { 
                Subject = "CN=Тестовый Пользователь, O=Тестовая Организация",
                Issuer = "CN=Тестовый УЦ",
                ValidFrom = DateTime.Now.AddYears(-1),
                ValidTo = DateTime.Now.AddYears(1),
                Provider = CryptoProvider.CryptoPro
            }
        };
    }

    private List<CryptoContainer> ParseCryptoProContainers(string output)
    {
        var containers = new List<CryptoContainer>();
        
        var lines = output.Split('\n');
        foreach (var line in lines)
        {
            if (line.Contains("\\\\.\\") || line.StartsWith("\\\\.\\"))
            {
                containers.Add(new CryptoContainer
                {
                    Name = line.Trim(),
                    Provider = CryptoProvider.CryptoPro,
                    IsAvailable = true,
                    Description = "Реальный контейнер КриптоПро"
                });
            }
        }

        return containers.Count > 0 ? containers : GetDemoContainers();
    }

    private List<CertificateInfo> ParseCryptoProCertificates(string output)
    {
        // Упрощенный парсинг вывода certmgr
        var certificates = new List<CertificateInfo>();
        
        // Здесь будет сложная логика парсинга вывода КриптоПро
        // Пока возвращаем демо-сертификаты
        
        return certificates.Count > 0 ? certificates : GetDemoCertificates();
    }
}
