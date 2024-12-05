using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using System.Linq;
using Messenger.Data;
using MsBox.Avalonia;
using System.Collections.Generic;

namespace Messenger
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
        private User SelectedUser { get; set; }
        private int CurrentUserId { get; set; }
        public MainWindow(string username)
        {
            InitializeComponent();

            // Установка текущего пользователя
            using (var context = new AppDbContext())
            {
                var currentUser = context.users.FirstOrDefault(u => u.username == username);
                if (currentUser == null)
                {
                    throw new System.Exception("Пользователь не найден.");
                }
                CurrentUserId = currentUser.id;

                // Загрузка списка других пользователей
                var users = context.users.Where(u => u.id != CurrentUserId).ToList();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
            }

            // Устанавливаем ItemsSource для UserList
            DataContext = this; // Прямое связывание коллекции с ListBox

            

            // Инициализация компонентов UI
            UserList.SelectionChanged += UserSelected;
            MessagesPanel = MessagesPanel ?? new StackPanel();
            SendButton.Click += SendMessage;
        }




        private void UserSelected(object? sender, SelectionChangedEventArgs e)
        {
            SelectedUser = (User)((ListBox)sender).SelectedItem;
            if (SelectedUser != null)
            {
                LoadMessages();
            }
        }

        private void LoadMessages()
        {
            MessagesPanel.Children.Clear();
            using (var context = new AppDbContext())
            {
                var messages = context.messages
                    .Where(m => (m.senderid == CurrentUserId && m.receiverid == SelectedUser.id) ||
                (m.senderid == SelectedUser.id && m.receiverid == CurrentUserId))
                .AsEnumerable()  // Принудительно выполняем запрос на клиенте
                .OrderBy(m => m.timestamp)  // Сортировка уже на клиенте
                .ToList();
                    

                foreach (var message in messages)
                {
                    AddMessageToChat(message.content, message.senderid == CurrentUserId);
                }
            }
        }

        private void SendMessage(object? sender, RoutedEventArgs e)
        {
            if (SelectedUser == null || string.IsNullOrWhiteSpace(MessageInput.Text))
                return;

            string messageContent = MessageInput.Text;  // Получаем текст сообщения
            AddMessageToChat(messageContent, true);
            using (var context = new AppDbContext())
            {
                var message = new Message
                {
                    senderid = CurrentUserId,
                    receiverid = SelectedUser.id,
                    content = messageContent  // Убедитесь, что content не пустое
                };

                // Проверяем, что content действительно не пустое
                if (string.IsNullOrWhiteSpace(message.content))
                {
                    ShowMessage("Ошибка", "Сообщение не может быть пустым.");
                    return;
                }

                // Здесь может возникать ошибка, если content = NULL
                
                context.messages.Add(message);

                context.SaveChanges();
              
            }

            
            MessageInput.Text = string.Empty;
        }


        private void AddMessageToChat(string message, bool isCurrentUser)
        {
            var textBlock = new TextBlock
            {
                Text = message,
                Margin = new Avalonia.Thickness(5),
                Background = new Avalonia.Media.SolidColorBrush(
                    isCurrentUser ? Avalonia.Media.Colors.LightBlue : Avalonia.Media.Colors.LightGray
                ),
                HorizontalAlignment = isCurrentUser ? Avalonia.Layout.HorizontalAlignment.Right : Avalonia.Layout.HorizontalAlignment.Left,
                MaxWidth = 250,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };

            MessagesPanel.Children.Add(textBlock);
        }



        private async void ShowMessage(string title, string message)
        {
            var messageBox = MessageBoxManager
            .GetMessageBoxStandard(title, message);
            var result = await messageBox.ShowAsync();
        }
    }
}
