using System;

using Rift.Data.Models;

using Discord;
using Newtonsoft.Json;

namespace Rift.Services.Message
{
    public class IonicMessage
    {
        public string Text { get; }
        public Embed Embed { get; }
        public string ImageUrl { get; }

        public IonicMessage(string text, Embed embed, string imageUrl)
        {
            Text = text;
            Embed = embed;
            ImageUrl = imageUrl;
        }

        public IonicMessage(RiftMessage msg)
        {
            Text = msg.Text;
            ImageUrl = msg.ImageUrl;

            try
            {
                Embed = JsonConvert.DeserializeObject<RiftEmbed>(msg.Embed).ToEmbed();
            }
            catch (Exception ex)
            {
                RiftBot.Log.Error($"{ex.Message}\n{ex.InnerException}");
                Embed = null;
            }
        }
    }
}
