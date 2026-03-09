using CollaborativeWhiteboard.ClientApp.Views;

namespace CollaborativeWhiteboard;

public partial class AppShell : Shell
{
	public AppShell(MainPage mainPage)
	{
		InitializeComponent();
		Items.Add(new ShellContent { Content = mainPage, Route = "MainPage" });
	}
}
