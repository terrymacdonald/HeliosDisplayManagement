﻿using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Windows.Forms;

namespace HeliosPlus.InterProcess
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal class IPCService : IService
    {
        private static ServiceHost _serviceHost;

        private IPCService()
        {
            Status = InstanceStatus.Busy;
        }

        public int HoldProcessId { get; set; } = 0;

        public InstanceStatus Status { get; set; }

        public void StopHold()
        {
            Application.Exit();
        }

        public static IPCService GetInstance()
        {
            if (_serviceHost != null || StartService())
            {
                return _serviceHost?.SingletonInstance as IPCService;
            }

            return null;
        }

        public static bool StartService()
        {
            if (_serviceHost == null)
            {
                try
                {
                    var process = Process.GetCurrentProcess();
                    var service = new IPCService();
                    _serviceHost = new ServiceHost(
                        service,
                        new Uri($"net.pipe://localhost/HeliosPlus_IPC{process.Id}"));

                    _serviceHost.AddServiceEndpoint(typeof(IService), new NetNamedPipeBinding(), "Service");
                    _serviceHost.Open();

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"IPCService/StartService exception: {ex.Message}: {ex.InnerException}");
                    try
                    {
                        _serviceHost?.Close();
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine($"IPCService/StartService exception 2: {ex2.Message}: {ex2.InnerException}");
                        // ignored
                    }

                    _serviceHost = null;
                }
            }

            return false;
        }
    }
}