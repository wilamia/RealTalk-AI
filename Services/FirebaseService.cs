using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public static class FirebaseService
{
    private static readonly FirebaseClient _databaseClient = new FirebaseClient("https://realtalk-ai-default-rtdb.firebaseio.com/");

    public static async Task SaveChatHistoryAsync(string userEmail, string nickname,
     List<(string sender, string message)> chatHistory, string chatTopic, string chatId = null)
    {
        var formattedEmail = userEmail.Replace("@", "_").Replace(".", "_");
        var dbRef = _databaseClient
            .Child("users").Child(formattedEmail).Child("chats");

        Debug.WriteLine($"Saving chat with ChatId: {chatId}");

        var chatData = new
        {
            Username = nickname,
            Topic = chatTopic,
            Messages = chatHistory.Select(m => new { sender = m.sender, message = m.message }).ToList(),
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() // <- Сохраняем текущее время в секундах
        };

        if (!string.IsNullOrEmpty(chatId))
        {
            await dbRef.Child(chatId).PutAsync(chatData);
        }
        else
        {
            await dbRef.PostAsync(chatData);
        }
    }

}
