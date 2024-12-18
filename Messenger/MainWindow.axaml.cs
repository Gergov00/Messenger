using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using System.Linq;
using Messenger.Data;
using MsBox.Avalonia;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace Messenger
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<User> Friends { get; set; } = new ObservableCollection<User>();
        public ObservableCollection<User> SearchResults { get; set; } = new ObservableCollection<User>();
        public ObservableCollection<User> FriendRequests { get; set; } = new ObservableCollection<User>();

        public ICommand AddFriendCommand { get; }

        private User SelectedUser { get; set; }
        private int CurrentUserId { get; set; }
        private DispatcherTimer _messageRefreshTimer;


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

                
            }

            DataContext = this;
            // Обновляем список заявок
            LoadFriendRequests();

            // Настроим таймер для обновления сообщений каждую секунду
           
            LoadFriendRequests();
            // Загрузка друзей
            LoadFriends();

            _messageRefreshTimer = new DispatcherTimer();
            _messageRefreshTimer.Interval = TimeSpan.FromSeconds(0.7);  // Интервал обновления
            _messageRefreshTimer.Tick += async (s, e) => await LoadMessages();  // Вызов LoadMessages каждую секунду
            _messageRefreshTimer.Start();
        }





        private void LoadFriendRequests()
        {
            using (var context = new AppDbContext())
            {
                // Получаем все заявки, которые находятся в состоянии "pending"
                var friendRequestIds = context.friendships
                    .Where(f => f.friend_id == CurrentUserId && f.status == "pending")  // Получаем все заявки на добавление в друзья
                    .Select(f => f.user_id)
                    .ToList();

                // Загружаем пользователей, которые отправили запрос
                var friendRequests = context.users
                    .Where(u => friendRequestIds.Contains(u.id))
                    .ToList();

                FriendRequests.Clear();
                foreach (var user in friendRequests)
                {
                    FriendRequests.Add(user);  // Добавляем в коллекцию для отображения в UI
                }
            }
        }

        private void OnAcceptRequestClick(object sender, RoutedEventArgs e)
        {
            var userToAccept = (User)((Button)sender).CommandParameter;

            if (userToAccept == null)
            {
                ShowMessage("Ошибка", "Пользователь не выбран.");
                return;
            }

            using (var context = new AppDbContext())
            {
                // Получаем заявку с состоянием 'pending'
                var friendshipRequest = context.friendships
                    .FirstOrDefault(f => f.user_id == userToAccept.id && f.friend_id == CurrentUserId && f.status == "pending");

                if (friendshipRequest != null)
                {
                    // Изменяем статус на 'accepted'
                    friendshipRequest.status = "accepted";
                    context.SaveChanges();

                    ShowMessage("Успех", "Заявка на добавление в друзья принята.");
                }
                else
                {
                    ShowMessage("Ошибка", "Заявка не найдена.");
                }
            }

            // Обновляем список заявок
            LoadFriendRequests();
            LoadFriends();
        }

        private void OnRejectRequestClick(object sender, RoutedEventArgs e)
        {
            var userToReject = (User)((Button)sender).CommandParameter;

            if (userToReject == null)
            {
                ShowMessage("Ошибка", "Пользователь не выбран.");
                return;
            }

            using (var context = new AppDbContext())
            {
                // Получаем заявку с состоянием 'pending'
                var friendshipRequest = context.friendships
                    .FirstOrDefault(f => f.user_id == userToReject.id && f.friend_id == CurrentUserId && f.status == "pending");

                if (friendshipRequest != null)
                {
                    // Удаляем заявку
                    context.friendships.Remove(friendshipRequest);
                    context.SaveChanges();

                    ShowMessage("Успех", "Заявка отклонена.");
                }
                else
                {
                    ShowMessage("Ошибка", "Заявка не найдена.");
                }
            }

            
        }


        private void UserSelected(object? sender, SelectionChangedEventArgs e)
        {
            SelectedUser = (User)((ListBox)sender).SelectedItem;


            

            if (SelectedUser != null)
            {
                LoadMessages();
            }
        }

        private async Task LoadMessages()
        {
            if (SelectedUser == null)
                return;

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

        private void LoadFriends()
        {
            using (var context = new AppDbContext())
            {
                var friendIds = context.friendships
                    .Where(f => f.user_id == CurrentUserId && f.status == "accepted") // Вы можете фильтровать по статусу дружбы
                    .Select(f => f.friend_id)
                    .ToList();

                // Загрузка пользователей, которые являются друзьями
                var friends = context.users
                    .Where(u => friendIds.Contains(u.id))
                    .ToList();

                Friends.Clear();
                foreach (var friend in friends)
                {
                    Friends.Add(friend);  // Добавляем в ObservableCollection для отображения в UI
                }
            }
        }



        private void SearchUsers(object? sender, RoutedEventArgs e)
        {
            SearchResults.Clear();
            var searchText = SearchBox.Text?.Trim().ToLower() ?? string.Empty;

            if (!string.IsNullOrEmpty(searchText))
            {
                using (var context = new AppDbContext())
                {
                    var users = context.users
                        .Where(u => u.username.ToLower().Contains(searchText) && u.id != CurrentUserId)
                        .ToList();

                    foreach (var user in users)
                    {
                        SearchResults.Add(user);
                    }
                }
            }
        }


        private void OnAddFriendClick(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного пользователя из SearchResults
            var userToAdd = (User)((Button)sender).DataContext;

            // Убедимся, что выбран пользователь
            if (userToAdd == null)
            {
                ShowMessage("Ошибка", "Выберите пользователя для добавления в друзья.");
                return;
            }

            using (var context = new AppDbContext())
            {
                // Проверяем, существует ли уже связь дружбы
                var existingFriendship = context.friendships
                    .FirstOrDefault(f => f.user_id == CurrentUserId && f.friend_id == userToAdd.id);

                if (existingFriendship != null)
                {
                    ShowMessage("Ошибка", "Этот пользователь уже в вашем списке друзей.");
                    return;
                }

                // Добавляем нового друга
                context.friendships.Add(new Friendship
                {
                    user_id = CurrentUserId,
                    friend_id = userToAdd.id,
                    status = "pending"  // Статус может быть 'pending', 'accepted' или 'rejected'
                });
                context.SaveChanges();

                ShowMessage("Успех", "Запрос на добавление в друзья отправлен.");
            }

            // Обновляем список друзей
            LoadFriends();
        }



        private void RemoveFriend(User user)
        {
            if (user == null)
            {
                ShowMessage("Ошибка", "Пользователь не выбран.");
                return;
            }

            using (var context = new AppDbContext())
            {
                // Ищем дружбу между текущим пользователем и другом
                var friendship = context.friendships
                    .FirstOrDefault(f => f.user_id == CurrentUserId && f.friend_id == user.id ||
                                         f.user_id == user.id && f.friend_id == CurrentUserId);

                if (friendship == null)
                {
                    ShowMessage("Ошибка", "Дружба не найдена.");
                    return;
                }

                // Удаляем запись о дружбе
                context.friendships.Remove(friendship);
                context.SaveChanges();

                ShowMessage("Успех", "Пользователь удален из списка друзей.");

                // Обновляем список друзей
                LoadFriends();
            }
        }


        private void OnRemoveFriendClick(object sender, RoutedEventArgs e)
        {
            var userToRemove = (User)((Button)sender).CommandParameter;
            RemoveFriend(userToRemove);
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
