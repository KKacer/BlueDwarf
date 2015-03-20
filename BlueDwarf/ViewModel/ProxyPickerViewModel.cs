﻿// This is the blue dwarf
// more information at https://github.com/picrap/BlueDwarf
namespace BlueDwarf.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ArxOne.MrAdvice.Collection;
    using ArxOne.MrAdvice.MVVM.Navigation;
    using ArxOne.MrAdvice.MVVM.Properties;
    using ArxOne.MrAdvice.MVVM.Threading;
    using Configuration;
    using Microsoft.Practices.Unity;
    using Net.Geolocation;
    using Net.Proxy.Client;
    using Net.Proxy.Client.Diagnostic;
    using Net.Proxy.Scanner;

    public class ProxyPickerViewModel : ViewModel
    {
        [Dependency]
        public IProxyPageScanner ProxyPageScanner { get; set; }

        [Dependency]
        public IPersistence Persistence { get; set; }

        [Dependency]
        public INavigator Navigator { get; set; }

        [Dependency]
        public IGeolocation Geolocation { get; set; }

        [Dependency]
        public IProxyClient ProxyClient { get; set; }

        [Dependency]
        public IProxyAnalyzer ProxyAnalyzer { get; set; }

        [NotifyPropertyChanged]
        public IList<ProxyPage> ProxyPages { get; set; }

        private ProxyPage _proxyPage;

        [Persistent("ProxyPage", AutoSave = true)]
        public Uri ProxyPageUri { get; set; }

        /// <summary>
        /// Gets or sets the selected proxy page.
        /// </summary>
        /// <value>
        /// The proxy page.
        /// </value>
        [NotifyPropertyChanged]
        public ProxyPage ProxyPage
        {
            get { return _proxyPage; }
            set
            {
                _proxyPage = value;
                ProxyPageUri = value.PageUri;
                CheckProxyServers();
            }
        }

        /// <summary>
        /// Gets or sets the proxy servers list. Selector is bound to this
        /// </summary>
        /// <value>
        /// The proxy servers.
        /// </value>
        public IList<Proxy> ProxyServers { get; set; }

        [Persistent("ProxyTest", AutoSave = true)]
        public string TestTarget { get; set; }

        /// <summary>
        /// Gets the test target URI.
        /// </summary>
        /// <value>
        /// The test target URI.
        /// </value>
        public Uri TestTargetUri
        {
            get
            {
                try
                {
                    if (TestTarget != null)
                        return new Uri(TestTarget);
                }
                catch (UriFormatException)
                {
                }
                return null;
            }
        }

        [Persistent("Proxy1")]
        public Uri LocalProxy { get; set; }

        private Proxy _proxyServer;

        /// <summary>
        /// Gets or sets the selected proxy server.
        /// If non null, this causes the window to close
        /// </summary>
        /// <value>
        /// The proxy server.
        /// </value>
        [NotifyPropertyChanged]
        public Proxy ProxyServer
        {
            get { return _proxyServer; }
            set
            {
                _proxyServer = value;
                Navigator.Exit(true);
            }
        }

        /// <summary>
        /// Loads data related to this view-model.
        /// </summary>
        public override void Load()
        {
            ProxyServers = new DispatcherObservableCollection<Proxy>();
            ProxyPages = ProxyPage.Default;
            ProxyPage = ProxyPages.SingleOrDefault(p => p.PageUri == ProxyPageUri) ?? ProxyPages[0];
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            Navigator.Exit(false);
        }

        /// <summary>
        /// Scans for proxy servers in given page.
        /// </summary>
        [Async(KillExisting = true)]
        private void CheckProxyServers()
        {
            try
            {
                Loading = true;
                ProxyServers.Clear();
                var testTargetUri = TestTargetUri;
                if (testTargetUri != null)
                {
                    var route = ProxyClient.CreateRoute(LocalProxy);
                    foreach (var hostPort in ProxyPageScanner.ScanPage(ProxyPage.PageUri, ProxyPage.ParseAsText, ProxyPage.HostPortEx, route, testTargetUri))
                    {
                        var location = Geolocation.Locate(hostPort.Address, route);
                        var p = ProxyAnalyzer.MeasurePerformance(route, testTargetUri);
                        if (p == null)
                            continue;
                        var proxy = new Proxy(hostPort, location, (int)p.Ping.TotalMilliseconds, (int)(p.DownloadSpeed / 1024));
                        ProxyServers.Add(proxy);
                    }
                }
            }
            finally
            {
                Loading = false;
            }
        }
    }
}
