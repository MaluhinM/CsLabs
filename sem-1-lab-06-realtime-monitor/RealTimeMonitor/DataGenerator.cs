namespace DataGenerator;

using System;
using System.Timers;

public class ServerMonitor : IDisposable
{
    private readonly Random _random = new();
    private readonly Lock _lock = new(); // Объект для синхронизации

    private readonly Timer _timer;

    private double _cpu = 35;
    private double _ram = 42;
    private double _temperature = 48;

    public event Action<double, double, double>? DataReceived;

    public ServerMonitor()
    {
        _timer = new Timer(1000);
        _timer.Elapsed += (_, _) => GenerateData();
    }

    public void Start()
    {
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    private void GenerateData()
    {
        lock (_lock) // Блокируем доступ для других потоков на время расчета
        {
            _cpu += _random.NextDouble() * 10 - 5;
            _ram += _random.NextDouble() * 6 - 3;
            _temperature += _random.NextDouble() * 4 - 2;

            _cpu = Math.Clamp(_cpu, 0, 100);
            _ram = Math.Clamp(_ram, 10, 100);
            _temperature = Math.Clamp(_temperature, 30, 95);

            DataReceived?.Invoke(
                Math.Round(_cpu, 1),
                Math.Round(_ram, 1),
                Math.Round(_temperature, 1));
        }
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}