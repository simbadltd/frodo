using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using AutoMapper;
using Frodo.Composition;
using Frodo.Core;
using Frodo.Core.Repositories;
using Frodo.Infrastructure.Ioc;
using Frodo.Integration.Toggl;
using NodaTime;
using Topshelf;

namespace Frodo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            HostFactory.Run(
                x =>
                {
                    x.Service<WindowsService>(
                        s =>
                        {
                            s.ConstructUsing(name => new WindowsService());
                            s.WhenStarted(tc => tc.Start());
                            s.WhenStopped(tc => tc.Stop());
                        });

                    x.RunAsLocalSystem();

                    x.SetDescription("Frodo");
                    x.SetDisplayName("Frodo");
                    x.SetServiceName("Frodo");
                });
        }
    }
}