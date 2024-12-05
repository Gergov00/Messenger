using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using System.Security.Cryptography;
using System.Text;
using System;
using Messenger.Data;
using System.Linq;

namespace Messenger;

public partial class SignUp : Window
{

    string[] emails = ["@gmail.com", "@yahoo.com", "@hotmail.com", "@aol.com", "@hotmail.co.uk", "@hotmail.fr", "@msn.com", "@yahoo.fr", "@wanadoo.fr", "@orange.fr", "@comcast.net", "@yahoo.co.uk", "@yahoo.com.br", "@yahoo.co.in", "@live.com", "@rediffmail.com", "@free.fr", "@gmx.de", "@web.de", "@yandex.ru", "@ymail.com", "@libero.it", "@outlook.com", "@uol.com.br", "@bol.com.br", "@mail.ru", "@cox.net", "@hotmail.it", "@sbcglobal.net", "@sfr.fr", "@live.fr", "@verizon.net", "@live.co.uk", "@googlemail.com", "@yahoo.es", "@ig.com.br", "@live.nl", "@bigpond.com", "@terra.com.br", "@yahoo.it", "@neuf.fr", "@yahoo.de", "@alice.it", "@rocketmail.com", "@att.net", "@laposte.net", "@facebook.com", "@bellsouth.net", "@yahoo.in", "@hotmail.es", "@charter.net", "@yahoo.ca", "@yahoo.com.au", "@rambler.ru", "@hotmail.de", "@tiscali.it", "@shaw.ca", "@yahoo.co.jp", "@sky.com", "@earthlink.net", "@optonline.net", "@freenet.de", "@t-online.de", "@aliceadsl.fr", "@virgilio.it", "@home.nl", "@qq.com", "@telenet.be", "@me.com", "@yahoo.com.ar", "@tiscali.co.uk", "@yahoo.com.mx", "@voila.fr", "@gmx.net", "@mail.com", "@planet.nl", "@tin.it", "@live.it", "@ntlworld.com", "@arcor.de", "@yahoo.co.id", "@frontiernet.net", "@hetnet.nl", "@live.com.au", "@yahoo.com.sg", "@zonnet.nl", "@club-internet.fr", "@juno.com", "@optusnet.com.au", "@blueyonder.co.uk", "@bluewin.ch", "@skynet.be", "@sympatico.ca", "@windstream.net", "@mac.com", "@centurytel.net", "@chello.nl", "@live.ca", "@aim.com", "@bigpond.net.au"];

    public SignUp()
    {
        InitializeComponent();
        
        
    }

    public void RegisterButton_Click(object? sender, RoutedEventArgs e)
    {
        var username = UsernameTextBox.Text;
        var email = EmailTextBox.Text;
        var password = PasswordTextBox.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ShowMessage("Ошибка", "Заполните все поля!");
            return;
        }

        if (!ParseEmal(email))
        {
            ShowMessage("Ошибка", "Введен неверный email");
            return;
        }

        using (var context = new AppDbContext())
        {
            // Проверяем наличие пользователя с таким email или username
            var existingUser = context.users
                .FirstOrDefault(u => u.email == email || u.username == username);

            if (existingUser != null)
            {
                ShowMessage("Ошибка", "Пользователь с таким email или username уже существует!");
                return;
            }

            // Хэшируем пароль
            var hashedPassword = HashPassword.HashedPassword(password);

            // Добавляем нового пользователя
            var user = new User
            {
                username = username,
                email = email,
                passwordhash = hashedPassword
            };

            context.users.Add(user);
            context.SaveChanges();
        }

        

        var window = new MainWindow(username);
        window.Show();
        this.Close();
    }

    public void GoLogInPage_Click(object? sender, RoutedEventArgs e)
    {
        var window = new LogIn();
        window.Show();
        this.Close();
    }

    private async void ShowMessage(string title, string message)
    {
        var messageBox = MessageBoxManager
        .GetMessageBoxStandard(title, message);
        var result = await messageBox.ShowAsync();
    }

    private bool ParseEmal(string emal)
    {
        foreach (var email in emails)
        {
            if (emal.Contains(email))
            {
                return true;
            }
        }
        return false;
    }


    
}