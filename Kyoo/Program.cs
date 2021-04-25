using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Unity;
using Unity.Microsoft.DependencyInjection;

namespace Kyoo
{
	/// <summary>
	/// Program entrypoint.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// Main function of the program
		/// </summary>
		/// <param name="args">Command line arguments</param>
		public static async Task Main(string[] args)
		{
			if (!File.Exists("./settings.json"))
				File.Copy(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "settings.json"), "settings.json");
			
			bool? debug = Environment.GetEnvironmentVariable("ENVIRONMENT")?.ToLowerInvariant() switch
			{
				"d" => true,
				"dev" => true,
				"debug" => true,
				"development" => true,
				"p" => false,
				"prod" => false,
				"production" => false,
				_ => null
			};

			if (debug == null && Environment.GetEnvironmentVariable("ENVIRONMENT") != null)
				Console.WriteLine($"Invalid ENVIRONMENT variable. Supported values are \"debug\" and \"prod\". Ignoring...");

			#if DEBUG
				debug ??= true;
			#endif

			Console.WriteLine($"Running as {Environment.UserName}.");
			IWebHostBuilder host = CreateWebHostBuilder(args);
			if (debug != null)
				host = host.UseEnvironment(debug == true ? "Development" : "Production");
			await host.Build().RunAsync();
		}

		/// <summary>
		/// Register settings.json, environment variables and command lines arguments as configuration.
		/// </summary>
		/// <param name="builder">The configuration builder to use</param>
		/// <param name="args">The command line arguments</param>
		/// <returns>The modified configuration builder</returns>
		private static IConfigurationBuilder SetupConfig(IConfigurationBuilder builder, string[] args)
		{
			return builder.AddJsonFile("./settings.json", false, true)
				.AddEnvironmentVariables()
				.AddCommandLine(args);
		}
		
		/// <summary>
		/// Createa a web host
		/// </summary>
		/// <param name="args">Command line parameters that can be handled by kestrel</param>
		/// <returns>A new web host instance</returns>
		private static IWebHostBuilder CreateWebHostBuilder(string[] args)
		{
			UnityContainer container = new();
			container.EnableDebugDiagnostic();
			
			return new WebHostBuilder()
				.UseContentRoot(AppDomain.CurrentDomain.BaseDirectory)
				.UseConfiguration(SetupConfig(new ConfigurationBuilder(), args).Build())
				.ConfigureAppConfiguration(x => SetupConfig(x, args))
				.ConfigureLogging((context, builder) =>
				{
					builder.AddConfiguration(context.Configuration.GetSection("logging"))
						.AddConsole()
						.AddDebug()
						.AddEventSourceLogger();
				})
				.UseDefaultServiceProvider((context, options) =>
				{
					options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
					if (context.HostingEnvironment.IsDevelopment())
						StaticWebAssetsLoader.UseStaticWebAssets(context.HostingEnvironment, context.Configuration);
				})
				.UseUnityServiceProvider(container)
				.ConfigureServices(x => x.AddRouting())
				.UseKestrel(options => { options.AddServerHeader = false; })
				.UseIIS()
				.UseIISIntegration()
				.UseStartup<Startup>();
		}
	}
}
