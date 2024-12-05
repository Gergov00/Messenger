using Avalonia.Controls;
using Avalonia.Interactivity;
using Messenger.Data;
using System.Linq;

namespace Messenger;

public partial class LogIn : Window
{
    public LogIn()
    {
        InitializeComponent();
    }

    public void LoginButton_Click(object? sender, RoutedEventArgs e)
    {
        var usernameOrEmail = EmailTextBox.Text;
        var password = PasswordTextBox.Text;

        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            ShowMessage("Ошибка", "Заполните все поля!");
            return;
        }

        var hasPassword = HashPassword.HashedPassword(password);

        // Проверка учетных данных
        using (var context = new AppDbContext())
        {
            var user = context.users
                .FirstOrDefault(u => (u.username == usernameOrEmail || u.email == usernameOrEmail) && u.passwordhash == hasPassword);

            if (user != null)
            {
                

                var window = new MainWindow(user.username); // Передаем имя пользователя в главное окно
                window.Show();
                this.Close();
            }
            else
            {
                ShowMessage("Ошибка", "Неверный логин или пароль!");
            }
        }
    }

    public void GoSignUpPage_Click(object? sender, RoutedEventArgs e)
    {
        var window = new SignUp();
        window.Show();
        this.Close();
    }

    private async void ShowMessage(string title, string message)
    {
        var messageBox = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard(title, message);
        await messageBox.ShowAsync();
    }
}
