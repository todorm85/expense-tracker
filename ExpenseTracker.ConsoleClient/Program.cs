using ExpenseTracker.Core;
using ExpenseTracker.Data;
using ExpenseTracker.RestClient;
using ExpenseTracker.UI;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ExpenseTracker.ConsoleClient
{
    internal class Program
    {
        private static Settings settings;

        public static void Main(string[] args)
        {
            LoadSettings();
            var container = RegisterIoCTypes();
            container.Resolve<ExtendedMenuBuilder>().Build().Run();
        }

        private static IUnityContainer RegisterIoCTypes()
        {
            var container = new UnityContainer();
            container.RegisterInstance<IOutputRenderer>(new Renderer());
            container.RegisterType<IMenuFactory, MenuFactory>();

            RegisterServices(container);
            container.RegisterInstance<ISettings>(settings);

            return container;
        }

        private static void LoadSettings()
        {
            settings = new Settings(new Dictionary<string, string>());

            var settingsPath = ConfigurationManager.AppSettings["settingsPath"];
            if (File.Exists(settingsPath))
            {
                var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>), new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true
                });

                var json = File.OpenText(settingsPath).ReadToEnd();
                var jsonBytes = Encoding.UTF8.GetBytes(json);
                var settingsCollection = serializer.ReadObject(new MemoryStream(jsonBytes)) as Dictionary<string, string>;
                settings = new Settings(settingsCollection);
            }

        }

        private static void RegisterServices(UnityContainer container)
        {
            if (settings.ClientMode == Settings.WebClientModeValue)
            {
                var authHeader = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(settings.WebClientAuth));
                container.RegisterType<IHttpClient, RestHttpClient>(new InjectionConstructor(settings.WebServiceBase, authHeader));
                container.RegisterType<IExpensesService, ExpensesRestClient>();
                container.RegisterType<IBudgetService, BudgetRestClient>();
                container.RegisterType<IBaseDataItemService<Category>, DataItemRestClient<Category>>(new InjectionConstructor(typeof(IHttpClient), "api/categories"));
            }
            else
            {
                container.RegisterType<IUnitOfWork, UnitOfWork>(new InjectionConstructor(settings.DbPath));
                container.RegisterType<IExpensesService, ExpensesService>();
                container.RegisterType<IBudgetService, BudgetService>();
                container.RegisterType<IBaseDataItemService<Category>, CategoriesService>();
            }
        }
    }
}