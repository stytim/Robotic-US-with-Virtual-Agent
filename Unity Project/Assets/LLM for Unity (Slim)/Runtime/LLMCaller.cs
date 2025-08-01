/// @file
/// @brief File implementing the basic functionality for LLM callers.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace LLMUnity
{
    [DefaultExecutionOrder(-2)]
    /// @ingroup llm
    /// <summary>
    /// Class implementing calling of LLM functions (local and remote).
    /// </summary>
    public class LLMCaller : MonoBehaviour
    {
        /// <summary> show/hide advanced options in the GameObject </summary>
        [Tooltip("show/hide advanced options in the GameObject")]
        [HideInInspector] public bool advancedOptions = false;
        /// <summary> use remote LLM server </summary>
        [Tooltip("use remote LLM server")]
        [LocalRemote] public bool remote = true;
        /// <summary> API key for the remote server </summary>
        [Tooltip("API key for the remote server")]
        [Remote] public string APIKey;
        /// <summary> host of the remote LLM server </summary>
        [Tooltip("host of the remote LLM server")]
        [Remote] public string host = "localhost";
        /// <summary> port of the remote LLM server </summary>
        [Tooltip("port of the remote LLM server")]
        [Remote] public int port = 13333;
        /// <summary> number of retries to use for the remote LLM server requests (-1 = infinite) </summary>
        [Tooltip("number of retries to use for the remote LLM server requests (-1 = infinite)")]
        [Remote] public int numRetries = 10;

        //protected LLM _prellm;
        protected List<(string, string)> requestHeaders;
        protected List<UnityWebRequest> WIPRequests = new List<UnityWebRequest>();

        /// <summary>
        /// The Unity Awake function that initializes the state before the application starts.
        /// The following actions are executed:
        /// - the corresponding LLM server is defined (if ran locally)
        /// - the grammar is set based on the grammar file
        /// - the prompt and chat history are initialised
        /// - the chat template is constructed
        /// - the number of tokens to keep are based on the system prompt (if setNKeepToPrompt=true)
        /// </summary>
        public virtual void Awake()
        {
            // Start the LLM server in a cross-platform way
            if (!enabled) return;

            requestHeaders = new List<(string, string)> { ("Content-Type", "application/json") };
 
            if (!String.IsNullOrEmpty(APIKey)) requestHeaders.Add(("Authorization", "Bearer " + APIKey));
            
        }

        protected virtual void Reset()
        {
            //AssignLLM();
        }

        protected virtual List<int> TokenizeContent(TokenizeResult result)
        {
            // get the tokens from a tokenize result received from the endpoint
            return result.tokens;
        }

        protected virtual string DetokenizeContent(TokenizeRequest result)
        {
            // get content from a chat result received from the endpoint
            return result.content;
        }

        protected virtual List<float> EmbeddingsContent(EmbeddingsResult result)
        {
            // get content from a chat result received from the endpoint
            return result.embedding;
        }

        protected virtual Ret ConvertContent<Res, Ret>(string response, ContentCallback<Res, Ret> getContent = null)
        {
            // template function to convert the json received and get the content
            if (response == null) return default;
            response = response.Trim();
            if (response.StartsWith("data: "))
            {
                string responseArray = "";
                foreach (string responsePart in response.Replace("\n\n", "").Split("data: "))
                {
                    if (responsePart == "") continue;
                    if (responseArray != "") responseArray += ",\n";
                    responseArray += responsePart;
                }
                response = $"{{\"data\": [{responseArray}]}}";
            }
            return getContent(JsonUtility.FromJson<Res>(response));
        }

        protected virtual void CancelRequestsRemote()
        {
            foreach (UnityWebRequest request in WIPRequests)
            {
                request.Abort();
            }
            WIPRequests.Clear();
        }

        /// <summary>
        /// Cancel the ongoing requests e.g. Chat, Complete.
        /// </summary>
        // <summary>
        public virtual void CancelRequests()
        {
            CancelRequestsRemote();
        }

        public virtual async Task<bool> CheckServerHealth(string endpoint = "tokenize", string payloadJson = null)
        {
            if (!remote) {
                 // Optionally add a health check for local setup if relevant
                 LLMUnitySetup.Log("Health check skipped (not in remote mode). Assuming healthy.");
                 return true;
            }

            string json = payloadJson ?? JsonUtility.ToJson(new TokenizeRequest { content = "" }); // Default payload
            string checkEndpoint = string.IsNullOrEmpty(endpoint) ? "tokenize" : endpoint; // Ensure endpoint is not empty

            LLMUnitySetup.Log($"Performing health check on endpoint: {checkEndpoint}...");

            // Use PostRequest directly, bypassing the public Tokenize/Detokenize/Embeddings methods
            List<int> result = await PostRequest<TokenizeResult, List<int>>(
                json,
                checkEndpoint,
                TokenizeContent, // Use appropriate content converter if endpoint changes
                null            // No callback needed for health check
            );

            bool isHealthy = result != null; // Simple check: if PostRequest succeeded, result won't be null

            if (isHealthy) {
                LLMUnitySetup.Log($"Health check successful: Server responded on {host}:{port}/{checkEndpoint}.");
            } else {
                LLMUnitySetup.LogWarning($"Health check failed: Server did not respond successfully on {host}:{port}/{checkEndpoint}.");
            }

            return isHealthy;
        }

        protected virtual async Task<Ret> PostRequestRemote<Res, Ret>(string json, string endpoint, ContentCallback<Res, Ret> getContent, Callback<Ret> callback = null)
        {
            // send a post request to the server and call the relevant callbacks to convert the received content and handle it
            // this function has streaming functionality i.e. handles the answer while it is being received
            if (endpoint == "slots")
            {
                LLMUnitySetup.LogError("Saving and loading is not currently supported in remote setting");
                return default;
            }

            Ret result = default;
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            UnityWebRequest request = null;
            string error = null;
            int tryNr = numRetries;

            while (tryNr != 0)
            {
                using (request = UnityWebRequest.Put($"{host}{(port != 0 ? $":{port}" : "")}/{endpoint}", jsonToSend))
                {
                    WIPRequests.Add(request);

                    request.method = "POST";
                    if (requestHeaders != null)
                    {
                        for (int i = 0; i < requestHeaders.Count; i++)
                            request.SetRequestHeader(requestHeaders[i].Item1, requestHeaders[i].Item2);
                    }

                    // Start the request asynchronously
                    UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
                    await Task.Yield(); // Wait for the next frame so that asyncOperation is properly registered (especially if not in main thread)

                    float lastProgress = 0f;
                    // Continue updating progress until the request is completed
                    while (!asyncOperation.isDone)
                    {
                        float currentProgress = request.downloadProgress;
                        // Check if progress has changed
                        if (currentProgress != lastProgress && callback != null)
                        {
                            callback?.Invoke(ConvertContent(request.downloadHandler.text, getContent));
                            lastProgress = currentProgress;
                        }
                        // Wait for the next frame
                        await Task.Yield();
                    }
                    WIPRequests.Remove(request);
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        result = ConvertContent(request.downloadHandler.text, getContent);
                        error = null;
                        break;
                    }
                    else
                    {
                        result = default;
                        error = request.error;
                        if (request.responseCode == (int)System.Net.HttpStatusCode.Unauthorized) break;
                    }
                }
                tryNr--;
                if (tryNr > 0) await Task.Delay(200 * (numRetries - tryNr));
            }

            if (error != null) LLMUnitySetup.LogError(error);
            callback?.Invoke(result);
            return result;
        }

        protected virtual async Task<Ret> PostRequest<Res, Ret>(string json, string endpoint, ContentCallback<Res, Ret> getContent, Callback<Ret> callback = null)
        {
            return await PostRequestRemote(json, endpoint, getContent, callback);
        }

        /// <summary>
        /// Tokenises the provided query.
        /// </summary>
        /// <param name="query">query to tokenise</param>
        /// <param name="callback">callback function called with the result tokens</param>
        /// <returns>list of the tokens</returns>
        public virtual async Task<List<int>> Tokenize(string query, Callback<List<int>> callback = null)
        {
            // handle the tokenization of a message by the user
            TokenizeRequest tokenizeRequest = new TokenizeRequest();
            tokenizeRequest.content = query;
            string json = JsonUtility.ToJson(tokenizeRequest);
            return await PostRequest<TokenizeResult, List<int>>(json, "tokenize", TokenizeContent, callback);
        }

        /// <summary>
        /// Detokenises the provided tokens to a string.
        /// </summary>
        /// <param name="tokens">tokens to detokenise</param>
        /// <param name="callback">callback function called with the result string</param>
        /// <returns>the detokenised string</returns>
        public virtual async Task<string> Detokenize(List<int> tokens, Callback<string> callback = null)
        {
            // handle the detokenization of a message by the user
            TokenizeResult tokenizeRequest = new TokenizeResult();
            tokenizeRequest.tokens = tokens;
            string json = JsonUtility.ToJson(tokenizeRequest);
            return await PostRequest<TokenizeRequest, string>(json, "detokenize", DetokenizeContent, callback);
        }

        /// <summary>
        /// Computes the embeddings of the provided input.
        /// </summary>
        /// <param name="tokens">input to compute the embeddings for</param>
        /// <param name="callback">callback function called with the result string</param>
        /// <returns>the computed embeddings</returns>
        public virtual async Task<List<float>> Embeddings(string query, Callback<List<float>> callback = null)
        {
            // handle the tokenization of a message by the user
            TokenizeRequest tokenizeRequest = new TokenizeRequest();
            tokenizeRequest.content = query;
            string json = JsonUtility.ToJson(tokenizeRequest);
            return await PostRequest<EmbeddingsResult, List<float>>(json, "embeddings", EmbeddingsContent, callback);
        }
    }
}