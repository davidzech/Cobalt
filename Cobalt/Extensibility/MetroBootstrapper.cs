using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Windows;
using Caliburn.Micro;

namespace Cobalt.Extensibility
{
    public class MetroBootstrapper<TRootViewModel> : BootstrapperBase
    {
        private CompositionContainer Container { get; set; }

        protected MetroBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            Container = new CompositionContainer(new AggregateCatalog(AssemblySource.Instance.Select(x => new AssemblyCatalog(x))));
            ConfigureBootstrapper();

            CompositionBatch batch = new CompositionBatch();
            ConfigureContainer(batch);
            batch.AddExportedValue(Container);

            Container.Compose(batch);
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = Container.GetExportedValues<object>(contract).ToList();

            if (exports.Any())
            {
                return exports.First();
            }

            throw new Exception($"Could not locate any instances of contract {contract}.");
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return Container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override void BuildUp(object instance)
        {
            Container.SatisfyImportsOnce(instance);
        }

        protected virtual void ConfigureBootstrapper()
        {
        }

        protected virtual void ConfigureContainer(CompositionBatch builder)
        {
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<TRootViewModel>();
        }
    }
}
