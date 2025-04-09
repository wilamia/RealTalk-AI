using Firebase.Database;

public static class FirebaseService
{
    private static readonly string FirebaseUrl = "https://realtalk-ai.firebaseio.com/";
    private static readonly string ApiKey = "AIzaSyCdTJg_iSeWoX2Ete3-8emKdqBVnY71AIA";
    private static FirebaseClient firebaseClient;

    static FirebaseService()
    {
        firebaseClient = new FirebaseClient(FirebaseUrl);
    }

    // Method to encode email to make it safe for Firebase paths
    private static string EncodeEmail(string email)
    {
        // Replace "@" and "." with other characters to make email safe for Firebase path
        return email.Replace("@", "_").Replace(".", "_");
    }

    public static async Task SaveChatHistoryAsync(string userId, string email, string nickname, List<(string sender, string message)> chatHistory)
    {
        // Encode the email to make it Firebase-safe
        string encodedEmail = EncodeEmail(email);

        // Create the Firebase path using the encoded email
        var path = $"users/{userId}/{encodedEmail}/{nickname}/chats";

        // Prepare the chat history for saving
        var chatData = chatHistory.Select(m => new { sender = m.sender, message = m.message }).ToList();

        // Save the chat history to Firebase at the specified path
        await firebaseClient
            .Child(path)
            .PostAsync(chatData);
    }
}
