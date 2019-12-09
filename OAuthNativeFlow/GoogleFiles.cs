using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OAuthNativeFlow
{
    public partial class GoogleFiles
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("etag")]
        public string Etag { get; set; }

        [JsonProperty("selfLink")]
        public string SelfLink { get; set; }

        [JsonProperty("nextPageToken")]
        public string NextPageToken { get; set; }

        [JsonProperty("nextLink")]
        public string NextLink { get; set; }

        [JsonProperty("incompleteSearch")]
        public bool IncompleteSearch { get; set; }

        public List<GoogleDriveFile> Items { get; set; }
    }

    public class GoogleDriveFile
    {
        public string Id { get; set; }
        public string WebContentLink { get; set; }
        public string Title { get; set; }
    }
}