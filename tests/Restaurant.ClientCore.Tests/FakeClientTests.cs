using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.Client.Core;
using Restaurant.Contracts;
using Xunit;

namespace Restaurant.ClientCore.Tests;

public sealed class FakeClientTests
{
    private readonly FakeRestaurantApi api = new(TimeProvider.System);

    [Fact]
    public async Task Fake_floor_contains_all_five_display_statuses()
    {
        var floor = await api.GetFloorAsync(TestContext.Current.CancellationToken);
        floor.Areas.Should().HaveCount(3);
        floor.Tables.Should().HaveCount(20);
        floor.Tables.Select(table => table.DisplayStatus).Distinct().Should().BeEquivalentTo(Enum.GetValues<TableDisplayStatus>());
    }

    [Fact]
    public async Task Fake_checkout_demonstrates_negative_net_amount()
    {
        var viewModel = new CheckoutViewModel(api);
        await viewModel.LoadPreviewAsync();
        viewModel.Preview!.NetAmount.Should().Be(-70000);
        viewModel.IsRefundDue.Should().BeTrue();
    }

    [Fact]
    public async Task Login_and_floor_view_models_work_with_fake_api()
    {
        var session = new SessionStore();
        var login = new LoginViewModel(api, session) { Username = "manager" };
        await login.LoginCommand.ExecuteAsync(null);
        session.CurrentUser!.Role.Should().Be(Role.MANAGER);

        var floor = new FloorPlanViewModel(api);
        await floor.RefreshAsync();
        floor.Tiles.Should().HaveCount(20);
    }

    [Fact]
    public void Dependency_injection_resolves_every_documented_view_model()
    {
        var services = new ServiceCollection();
        services.AddClientCore(fake: true);
        using var provider = services.BuildServiceProvider();
        Type[] viewModels =
        [
            typeof(LoginViewModel), typeof(ShellViewModel), typeof(FloorPlanViewModel), typeof(ReservationsViewModel),
            typeof(PosViewModel), typeof(CheckoutViewModel), typeof(KitchenViewModel), typeof(EscalationQueueViewModel),
            typeof(AiActivityViewModel), typeof(WaitlistViewModel), typeof(InventoryViewModel), typeof(RecipeEditorViewModel),
            typeof(MenuAdminViewModel), typeof(UsersAdminViewModel), typeof(SettingsViewModel), typeof(ReportsViewModel),
            typeof(PrintSettingsViewModel)
        ];

        viewModels.Select(provider.GetRequiredService).Should().NotContainNulls();
    }
}
