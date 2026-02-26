using AppSimple.MvvmApp.Services;
using AppSimple.MvvmApp.Session;
using AppSimple.MvvmApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AppSimple.MvvmApp.Extensions;

/// <summary>Registers all MvvmApp-specific services with the DI container.</summary>
public static class MvvmAppServiceExtensions
{
    /// <summary>Adds ViewModels, session, and the main window to <paramref name="services"/>.</summary>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddMvvmAppServices(this IServiceCollection services)
    {
        services.AddSingleton<UserSession>();

        services.AddSingleton<ThemeManager>();

        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<ProfileViewModel>();
        services.AddSingleton<UsersViewModel>();
        services.AddSingleton<NotesViewModel>();
        services.AddSingleton<ContactsViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        services.AddTransient<MainWindow>();

        return services;
    }
}
