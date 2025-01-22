using System;
using System.Security.Cryptography;
using System.Text;
using BeatTogether.Core.Enums;

namespace BeatTogether.MasterServer.Api.Util
{
    public static class UserIdHash
    {
        public static string Generate(Platform platform, string platformUserId)
        {
            var platformStr = platform switch
            {
                Platform.Test => "Test#",
                Platform.Oculus => "Oculus#",
                Platform.OculusQuest => "OculusQuest#",
                Platform.Pico => "Pico#",
                Platform.Steam => "Steam#",
                Platform.PS4 or Platform.PS5 => "PSN#",
                _ => ""
            };
            return Generate(platformStr, platformUserId);
        }
        
        public static string Generate(string platformStr, string platformUserId)
        {
            return Convert.ToBase64String(
                SHA256.Create().ComputeHash(
                    Encoding.UTF8.GetBytes(platformStr + platformUserId)))[..22];
        }
    }
}