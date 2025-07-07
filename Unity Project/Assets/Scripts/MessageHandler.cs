using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class MessageHandler : MonoBehaviour
{
    public static MessageHandler Instance { get; private set; }
    public enum Mode { Server, Client }
    public Mode mode = Mode.Client;

    public string serverIP = "127.0.0.1";
    public int port = 5555;

    public LLMHandler llmHandler;
    public TTSHandler ttsHandler;
    // public enum AppRole { Patient, Doctor } // Keep if you consolidated roles
    // public AppRole currentRole = AppRole.Patient; // Keep if you consolidated roles


    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;
    private Thread networkThread;
    private bool isRunning = false; // Consider 'volatile' if issues persist, but start simple
    private bool isConnected = false;
    private DateTime lastHeartbeatTime;

    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
    private ConcurrentQueue<string> actionQueue = new ConcurrentQueue<string>();

    private const int heartbeatInterval = 5000; // Heartbeat check every 5 seconds
    private const int heartbeatTimeout = 10000; // If no response in 10 seconds, assume disconnected
    private const int MaxMessageSizeOriginal = 2048; // Max message size for safety with framing
    private const int NetworkThreadJoinTimeout = 2000;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        isRunning = true;
        lastHeartbeatTime = DateTime.Now;

        if (mode == Mode.Server)
            StartServer();
        else
            StartClient();
    }

    void Update()
    {
        while (messageQueue.TryDequeue(out string message))
        {
            // Assuming llmHandler.DoctorQuestion was from your pasted original
            // Adjust if you have the AppRole logic integrated
            llmHandler.DoctorQuestion(message);
        }

        while (actionQueue.TryDequeue(out string message))
        {
            ttsHandler.PlayDelayedAudio();
        }
    }

    void OnApplicationQuit()
    {
        StopNetworking();
    }

    public void StartServer()
    {
        networkThread = new Thread(() =>
        {
            try
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                Debug.Log("Server started, waiting for clients...");

                while (isRunning)
                {
                    try
                    {
                        if (client == null || !client.Connected)
                        {
                            Debug.Log("Waiting for client...");
                            client = server.AcceptTcpClient();
                            isConnected = true;
                            lastHeartbeatTime = DateTime.Now; // From original
                            Debug.Log("Client connected!");
                            HandleConnection(client); // Original call
                        }
                        else if ((DateTime.Now - lastHeartbeatTime).TotalMilliseconds > heartbeatTimeout)
                        {
                            Debug.LogWarning("Client lost connection (timeout). Re-listening...");
                            CleanupConnection();
                        }
                    }
                    catch (SocketException ex)
                    {
                        // Catch specific exception if server.AcceptTcpClient() is interrupted by server.Stop()
                        if (!isRunning && ex.SocketErrorCode == SocketError.Interrupted)
                        {
                            Debug.Log("Server stopping, AcceptTcpClient interrupted as expected.");
                        }
                        else
                        {
                            Debug.LogError("Server operational error: " + ex.Message);
                        }
                        // Optional: A brief pause if it was an unexpected error before retrying accept or cleaning up
                        // if (isRunning) Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Server error in main loop: " + ex.Message);
                        CleanupConnection(); // Ensure cleanup
                    }
                    Thread.Sleep(100); // Reduced sleep from original 500ms when actively connected or looping
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Server Start Error: " + ex.Message);
            }
            finally
            {
                if (server != null) server.Stop();
                Debug.Log("Server thread finished.");
            }
        });

        networkThread.IsBackground = true; // From original
        networkThread.Start();
    }

    public void StartClient()
    {
        networkThread = new Thread(() =>
        {
            while (isRunning)
            {
                if (!isConnected)
                {
                    try
                    {
                        client = new TcpClient();
                        Debug.Log("Attempting to connect to server...");
                        client.Connect(serverIP, port);
                        isConnected = true;
                        lastHeartbeatTime = DateTime.Now; // From original
                        Debug.Log("Connected to server!");
                        HandleConnection(client); // Original call
                    }
                    catch (Exception ex) // Original catch was generic
                    {
                        Debug.LogWarning($"Server not available or connection failed ({ex.GetType().Name}), retrying in 2 seconds...");
                        CleanupConnection(); // Ensure cleanup before retry
                    }
                    if (isRunning && !isConnected) Thread.Sleep(2000); // From original
                }
                else if ((DateTime.Now - lastHeartbeatTime).TotalMilliseconds > heartbeatTimeout)
                {
                    Debug.LogWarning("Lost connection to server (timeout). Reconnecting...");
                    CleanupConnection();
                }
                // Add a small sleep if connected and not timed out, to prevent tight loop on this outer check
                else if (isConnected)
                {
                    Thread.Sleep(100);
                }
            }
            Debug.Log("Client thread finished.");
        });
        networkThread.IsBackground = true; // From original
        networkThread.Start();
    }

    // Helper to read exactly N bytes, crucial for framing
    // Kept simple, relies on stream.Read blocking nature or throwing exceptions on error/closure
    private byte[] ReadExactlyNBytes(NetworkStream currentStream, int byteCount)
    {
        byte[] buffer = new byte[byteCount];
        int totalBytesRead = 0;
        while (totalBytesRead < byteCount)
        {
            // Check if we should still be running before attempting a blocking read
            if (!isRunning || (client != null && !client.Connected))
            {
                throw new OperationCanceledException("Network operation cancelled or client disconnected.");
            }

            int bytesRead = currentStream.Read(buffer, totalBytesRead, byteCount - totalBytesRead);
            if (bytesRead == 0)
            {
                // Graceful shutdown by peer
                throw new System.IO.EndOfStreamException("Remote connection closed gracefully.");
            }
            totalBytesRead += bytesRead;
        }
        return buffer;
    }

    private void HandleConnection(TcpClient tcpClient)
    {
        NetworkStream localStream = null; // Use a local variable for the stream in this specific connection handler
        try
        {
            localStream = tcpClient.GetStream();
            this.stream = localStream; // Assign to class member if SendMessageToNetwork needs it directly (original did)

            // Removed fixed 1024 buffer, will read based on length prefix

            while (isRunning && tcpClient.Connected && isConnected) // Added isConnected here too
            {
                // --- MODIFIED PART: Read with Length Prefixing ---
                byte[] lengthPrefixBuffer;
                int messageLength;
                try
                {
                    // 1. Read 4-byte length prefix
                    // Debug.Log("Attempting to read message length..."); // Optional: for verbose debugging
                    lengthPrefixBuffer = ReadExactlyNBytes(localStream, 4);
                    messageLength = BitConverter.ToInt32(lengthPrefixBuffer, 0);
                    // Debug.Log($"Message length received: {messageLength}"); // Optional
                }
                catch (System.IO.EndOfStreamException)
                {
                    Debug.Log("Remote side closed connection (EOS while reading length).");
                    break; // Exit while loop
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("Read length operation cancelled (client disconnected or shutting down).");
                    break;
                }
                catch (Exception ex)
                { // Catch other IO/Socket exceptions during length read
                    Debug.LogError($"Error reading message length: {ex.GetType().Name} - {ex.Message}. Closing connection.");
                    break;
                }


                if (messageLength <= 0 || messageLength > MaxMessageSizeOriginal)
                {
                    Debug.LogError($"Invalid message length: {messageLength}. Max: {MaxMessageSizeOriginal}. Closing connection.");
                    break; // Exit while loop
                }

                byte[] messageBuffer;
                string message;
                try
                {
                    // 2. Read the actual message
                    // Debug.Log($"Attempting to read message body of {messageLength} bytes..."); // Optional
                    messageBuffer = ReadExactlyNBytes(localStream, messageLength);
                    message = Encoding.UTF8.GetString(messageBuffer);
                    // Debug.Log("Message body read successfully."); // Optional
                }
                catch (System.IO.EndOfStreamException)
                {
                    Debug.Log("Remote side closed connection (EOS while reading message body).");
                    break; // Exit while loop
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("Read message body operation cancelled (client disconnected or shutting down).");
                    break;
                }
                catch (Exception ex)
                { // Catch other IO/Socket exceptions
                    Debug.LogError($"Error reading message body: {ex.GetType().Name} - {ex.Message}. Closing connection.");
                    break;
                }
                // --- END OF MODIFIED READ PART ---

                // Original message processing logic
                lastHeartbeatTime = DateTime.Now; // Update on any successful message read

                if (message == "hb")
                {
                    // Still update lastHeartbeatTime, already done above.
                    // Consider if PONG is needed or if this simple "hb" exchange is sufficient for now.
                }
                else if (message == "ttsd") // Or your other TTS command
                {
                    Debug.Log("Received TTS command: " + message);
                    actionQueue.Enqueue("tts");
                }
                else
                {
                    Debug.Log("Received App Message: " + message);
                    messageQueue.Enqueue(message);
                }

                // Original heartbeat sending logic (can be kept or refined later)
                // This logic might be too simple if messages are frequent, as it resets on *any* received message.
                // And it updates lastHeartbeatTime on *sending* "hb", which can mask true unresponsiveness of the other side.
                if ((DateTime.Now - lastHeartbeatTime).TotalMilliseconds > heartbeatInterval)
                {
                    SendMessageToNetwork("hb"); // Original heartbeat message
                    lastHeartbeatTime = DateTime.Now; // Original: updates on send
                }
                Thread.Sleep(10); // A small delay to prevent tight loop if no data comes quickly
            }
        }
        catch (Exception ex) // Catch exceptions from GetStream or other setup before loop
        {
            // If error is "Cannot access a disposed object", it means tcpClient was closed elsewhere.
            Debug.LogError($"Error in HandleConnection setup or unhandled loop error: {ex.GetType().Name} - {ex.Message}");
        }
        finally // Ensures cleanup happens if HandleConnection exits for any reason
        {
            Debug.Log("HandleConnection finishing. Cleaning up this connection.");
            // We don't call CleanupConnection() directly here anymore in this version of HandleConnection's structure.
            // The outer loops (StartServer/StartClient) will call CleanupConnection when HandleConnection returns.
            // Set isConnected to false to signal the outer loops.
            isConnected = false; // Signal that this specific connection handling is done.
            this.stream = null; // Clear the class member stream if it was set
        }
    }

    public void SendMessageToNetwork(string message)
    {
        // Use the class member 'stream' as per original design, ensure it's valid.
        NetworkStream currentStream = this.stream; // Use the stream associated with the active connection
        if (currentStream != null && currentStream.CanWrite && client != null && client.Connected && isConnected)
        {
            try
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                if (messageBytes.Length == 0 || messageBytes.Length > MaxMessageSizeOriginal)
                {
                    Debug.LogError($"Attempt to send message with invalid length: {messageBytes.Length}");
                    return;
                }
                byte[] lengthPrefix = BitConverter.GetBytes(messageBytes.Length);

                // Send length prefix, then message
                currentStream.Write(lengthPrefix, 0, lengthPrefix.Length);
                currentStream.Write(messageBytes, 0, messageBytes.Length);

                if (message != "hb") // Don't log every heartbeat send if too verbose
                {
                    Debug.Log("Sent: " + message);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error sending message: {ex.Message}. Might disconnect.");
                // Consider triggering a disconnect by setting isConnected = false here.
                // For now, let read failures or heartbeat timeouts detect it.
            }
        }
        else
        {
            Debug.LogWarning("Cannot send message. No active, writable, or connected stream.");
        }
    }

    public void StopNetworking()
    {
        Debug.Log("StopNetworking called.");
        bool mainThreadIsRunning = isRunning; // Store initial state
        isRunning = false; // Signal threads to stop

        // Order: Cleanup connection resources first, which should help threads unblock
        CleanupConnection(); // This closes client and stream

        if (server != null)
        {
            Debug.Log("Stopping server listener...");
            server.Stop(); // This should interrupt AcceptTcpClient()
            server = null;
        }

        if (networkThread != null && networkThread.IsAlive)
        {
            Debug.Log("Attempting to join network thread...");
            // Original used Abort. Join is preferred for graceful shutdown.
            // However, if ReadExactlyNBytes is blocking indefinitely without a timeout on the stream,
            // Join might hang. The OperationCanceledException in ReadExactlyNBytes helps if isRunning is false.
            if (mainThreadIsRunning)
            { // Only try to join if it was running to avoid issues
                bool joined = networkThread.Join(NetworkThreadJoinTimeout); // 2 seconds timeout
                if (!joined)
                {
                    Debug.LogWarning("Network thread did not join in time. Original script used Abort.");
                    // networkThread.Abort(); // Re-enable if Join consistently fails and Abort was acceptable
                }
                else
                {
                    Debug.Log("Network thread joined successfully.");
                }
            }
            networkThread = null;
        }
        Debug.Log("Networking stopped.");
    }

    private void CleanupConnection()
    {
        Debug.Log("CleanupConnection called.");
        isConnected = false; // Crucial to stop loops and allow reconnection attempts

        if (stream != null)
        {
            try { stream.Close(); } catch { /* ignore */ }
            stream = null;
        }
        if (client != null)
        {
            try { client.Close(); } catch { /* ignore */ }
            client = null;
        }
    }
}